/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2010-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zongsoft.Plugins.Builders
{
	public class BuilderManager
	{
		#region 事件定义
		public event EventHandler<BuilderEventArgs> Built;
		#endregion

		#region 单例变量
		private static BuilderManager _current;
		#endregion

		#region 私有变量
		private readonly object _syncRoot;
		private readonly Stack<BuildToken> _stack;
		#endregion

		#region 私有构造
		private BuilderManager()
		{
			_syncRoot = new object();
			_stack = new Stack<BuildToken>();
		}
		#endregion

		#region 单例属性
		public static BuilderManager Current
		{
			get
			{
				if(_current == null)
					System.Threading.Interlocked.CompareExchange(ref _current, new BuilderManager(), null);

				return _current;
			}
		}
		#endregion

		#region 公共方法
		public object Build(Builtin builtin, object parameter, Action<BuilderContext> build)
		{
			//获取当前构件的构建器对象
			IBuilder builder = this.GetBuilder(builtin);

			lock(_syncRoot)
			{
				return this.BuildCore(builder, BuilderContext.CreateContext(builder, builtin, parameter), build);
			}
		}
		#endregion

		#region 保护方法
		protected virtual void OnBuilt(BuilderEventArgs args)
		{
			if(this.Built != null)
				this.Built(this, args);
		}
		#endregion

		#region 私有方法
		private object Build(Builtin builtin, object parameter, object owner, PluginTreeNode ownerNode)
		{
			//获取当前构件的构建器对象
			IBuilder builder = this.GetBuilder(builtin);

			return this.BuildCore(builder, BuilderContext.CreateContext(builder, builtin, parameter, owner, ownerNode), null);
		}

		private object BuildCore(IBuilder builder, BuilderContext context, Action<BuilderContext> build)
		{
			object value;

			//判断当前待创建的构件是否正在当前构建序列中，如果是则立即返回其目标值
			if(this.TryGetBuilding(context.Builtin, out value))
				return value;

			if(build != null)
				build(context);
			else
			{
				var target = builder.Build(context);

				//如果创建器以两种方式传递过来的目标对象均不为空并且引用不相等，则抛出异常，以确保实现者返回的目标对象是明确唯一的。
				if(target != null && context.Result != null && object.ReferenceEquals(target, context.Result) == false)
					throw new PluginException(FailureCodes.BuiltinBuildFailed, string.Format("The builder of '{0}' builtin was faild.", context.Builtin.ToString()));

				context.Result = context.Result ?? target;
			}

			//如果要保存构建的目标对象，则将其保存在当前构件对象的Value属性内
			context.Builtin.Value = context.Value ?? context.Result;

			if(context.Result != null)
			{
				try
				{
					//将创建成功的目标对象保存到当前调用栈中，以便在子构件的构建过程中进行循环创建调用作检测之用
					//注意：将当前创建的目标对象保存到调用栈中的操作必须在构建子构件集合之前完成！
					_stack.Push(new BuildToken(context.Builtin, context.Result));

					//如果构建器不取消后续构建操作的话，则继续构建子集
					if(!context.Cancel)
						BuildChildren(context.Node, context.Parameter, context.Result, context.Node);

					//设置当前构件为构建已完成标志
					context.Builtin.IsBuilded = (context.Result != null);
				}
				finally
				{
					if(_stack.Count > 0)
						_stack.Pop();
				}
			}

			//通知构建器本次构建工作已完成
			builder.OnBuilt(context);

			//激发构建完成事件
			this.OnBuilt(new BuilderEventArgs(builder, context));

			//返回生成的目标对象
			return context.Result;
		}

		private void BuildChildren(PluginTreeNode node, object parameter, object owner, PluginTreeNode ownerNode)
		{
			if(node == null)
				return;

			foreach(var child in node.Children)
			{
				switch(child.NodeType)
				{
					case PluginTreeNodeType.Builtin:
						this.Build((Builtin)child.Value, parameter, owner, ownerNode);
						break;
					case PluginTreeNodeType.Empty:
						this.BuildChildren(child, parameter, owner, ownerNode);
						break;
					case PluginTreeNodeType.Custom:
						this.BuildChildren(child, parameter, child.Value, child);
						break;
				}
			}
		}

		private IBuilder GetBuilder(Builtin builtin)
		{
			if(builtin == null)
				throw new ArgumentNullException("builtin");

			//获取当前构件的构建器对象
			IBuilder builder = builtin.Plugin.GetBuilder(builtin.BuilderName);

			if(builder == null)
				throw new PluginException(string.Format(@"The name is '{0}' of Builder not found in '{1}' plugin." + Environment.NewLine + "{2}", builtin.BuilderName, builtin.Plugin.Name, builtin.Plugin.FilePath));

			return builder;
		}

		private bool TryGetBuilding(Builtin builtin, out object value)
		{
			value = null;

			foreach(var item in _stack)
			{
				if(object.ReferenceEquals(item.Builtin, builtin))
				{
					value = item.Target;
					return true;
				}
			}

			return false;
		}
		#endregion

		#region 嵌套子类
		private class BuildToken
		{
			internal Builtin Builtin;
			internal object Target;

			internal BuildToken(Builtin builtin, object target)
			{
				this.Builtin = builtin;
				this.Target = target;
			}
		}
		#endregion
	}
}
