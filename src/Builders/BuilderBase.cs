/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2010-2016 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Plugins.
 *
 * Zongsoft.Plugins is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Plugins is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Plugins; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zongsoft.Plugins.Builders
{
	public abstract class BuilderBase : MarshalByRefObject, IBuilder
	{
		#region 成员变量
		private IEnumerable<string> _ignoredProperties;
		#endregion

		#region 构造函数
		protected BuilderBase()
		{
		}

		protected BuilderBase(IEnumerable<string> ignoredProperties)
		{
			_ignoredProperties = ignoredProperties;
		}
		#endregion

		#region 保护属性
		/// <summary>
		/// 获取在创建目标对象时要忽略设置的扩展属性名。
		/// </summary>
		/// <remarks>
		///		<para>对重写<see cref="Build"/>方法的实现者的说明：在构建目标对象后应排除本属性所指定的在Builtin.Properties中的属性项。</para>
		/// </remarks>
		protected virtual IEnumerable<string> IgnoredProperties
		{
			get
			{
				return _ignoredProperties;
			}
		}
		#endregion

		#region 获取类型
		public virtual Type GetValueType(Builtin builtin)
		{
			if(builtin != null && builtin.BuiltinType != null)
				return builtin.BuiltinType.Type;

			var attribute = (Builders.BuilderBehaviourAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(Builders.BuilderBehaviourAttribute), true);

			if(attribute != null)
				return attribute.ValueType;

			return null;
		}
		#endregion

		#region 公共方法
		/// <summary>
		/// 创建指定构件对应的目标对象。
		/// </summary>
		/// <param name="context">调用本方法进行构建的上下文对象，可通过该参数获取构建过程的相关设置或状态。</param>
		/// <returns>创建成功后的目标对象。</returns>
		public virtual object Build(BuilderContext context)
		{
			return PluginUtility.BuildBuiltin(context.Builtin, this.IgnoredProperties);
		}

		public virtual void Destroy(BuilderContext context)
		{
			if(context == null || context.Builtin == null)
				return;

			Builtin builtin = context.Builtin;

			if(builtin.HasValue)
			{
				IDisposable value = builtin.Value as IDisposable;

				if(value != null)
				{
					value.Dispose();
				}
				else
				{
					System.Collections.IEnumerable collection = builtin.Value as System.Collections.IEnumerable;

					if(collection != null)
					{
						foreach(object item in collection)
						{
							IDisposable disposable = item as IDisposable;

							if(disposable != null)
								disposable.Dispose();
						}
					}
				}

				builtin.Value = null;
			}
		}

		/// <summary>
		/// 当构建器所属的插件被卸载，该方法被调用。
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region 虚拟方法
		protected virtual void ApplyBehaviors(BuilderContext context)
		{
			var behavior = context.Builtin.Behaviors["property"];

			if(behavior != null)
			{
				var propertyBehavior = behavior.Populate<PropertyBehavior>();

				if(propertyBehavior == null)
					throw new PluginException();

				propertyBehavior.Apply();
			}
		}

		protected virtual void OnBuilt(BuilderContext context)
		{
			if(context.OwnerNode == null || context.OwnerNode.NodeType != PluginTreeNodeType.Builtin)
				return;

			var appender = context.OwnerNode.Plugin.GetBuilder(((Builtin)context.OwnerNode.Value).BuilderName) as IAppender;

			if(appender != null)
				appender.Append(new AppenderContext(context.PluginContext, context.Result, context.Node, context.Owner, context.OwnerNode, AppenderBehaviour.Append));
		}

		protected virtual void Dispose(bool disposing)
		{
		}
		#endregion

		#region 显式实现
		void IBuilder.OnBuilt(BuilderContext context)
		{
			this.ApplyBehaviors(context);
			this.OnBuilt(context);
		}
		#endregion

		#region 嵌套子类
		private class PropertyBehavior
		{
			public string Type
			{
				get;
				set;
			}

			public object Target
			{
				get;
				set;
			}

			public string Name
			{
				get;
				set;
			}

			public object Value
			{
				get;
				set;
			}

			public void Apply()
			{
				if(string.IsNullOrWhiteSpace(this.Name))
					throw new PluginException();

				var type = this.Target == null ? PluginUtility.GetType(this.Type) : this.Target.GetType();
				var member = type.GetMember(this.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.SetField | BindingFlags.SetProperty).FirstOrDefault();

				if(member == null)
					throw new PluginException();

				switch(member.MemberType)
				{
					case MemberTypes.Field:
						((FieldInfo)member).SetValue(this.Target, this.Value);
						break;
					case MemberTypes.Property:
						((PropertyInfo)member).SetValue(this.Target, this.Value);
						break;
				}
			}
		}
		#endregion
	}
}
