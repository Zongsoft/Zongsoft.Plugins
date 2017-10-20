/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2010-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Zongsoft.Plugins.Builders
{
	public class ObjectBuilder : BuilderBase, IAppender
	{
		#region 构造函数
		public ObjectBuilder() : base(new string[] { "value" })
		{
		}

		public ObjectBuilder(IEnumerable<string> ignoredProperties) : base(ignoredProperties)
		{
		}
		#endregion

		#region 重写方法
		public override Type GetValueType(Builtin builtin)
		{
			if(builtin == null)
				return base.GetValueType(builtin);

			var property = builtin.Properties["value"];

			if(property != null && Parsers.Parser.CanParse(property.RawValue))
				return Parsers.Parser.GetValueType(property.RawValue, builtin);

			return base.GetValueType(builtin);
		}

		public override object Build(BuilderContext context)
		{
			object result = null;

			if(context.Builtin.Properties.TryGetValue("value", out result))
				PluginUtility.UpdateProperties(result, context.Builtin, this.IgnoredProperties);
			else
				result = base.Build(context);

			return result;
		}
		#endregion

		#region 显式实现
		bool IAppender.Append(AppenderContext context)
		{
			if(context.Container == null || context.Value == null)
				return false;

			return this.Append(context.Container, context.Value, context.Node.Name);
		}
		#endregion

		#region 私有方法
		private bool Append(object container, object child, string key)
		{
			if(container == null || child == null)
				return false;

			Type containerType = container.GetType();

			if(typeof(IDictionary).IsAssignableFrom(containerType))
			{
				((IDictionary)container).Add(key, child);
				return true;
			}
			else if(typeof(IList).IsAssignableFrom(containerType))
			{
				var list = (IList)container;

				if(child.GetType() != typeof(string) && child is IEnumerable)
				{
					var count = 0;

					foreach(var entry in (IEnumerable)child)
					{
						if(list.Add(entry) >= 0)
							count++;
					}

					return count > 0;
				}
				else
				{
					return list.Add(child) >= 0;
				}
			}

			var attribute = (System.ComponentModel.DefaultPropertyAttribute)Attribute.GetCustomAttribute(containerType, typeof(System.ComponentModel.DefaultPropertyAttribute), true);

			if(attribute != null)
			{
				var property = containerType.GetProperty(attribute.Name);

				if(property != null && this.Append(property.GetValue(container), child, key))
					return true;
			}

			var methods = containerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
						  .Where(method => method.Name == "Add" || method.Name == "Register")
						  .OrderByDescending(method => method.GetParameters().Length);

			foreach(var method in methods)
			{
				var parameters = method.GetParameters();

				if(parameters.Length == 2)
				{
					if(parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType.IsAssignableFrom(child.GetType()))
					{
						method.Invoke(container, new object[] { key, child });
						return true;
					}
				}
				else if(parameters.Length == 1)
				{
					if(parameters[0].ParameterType.IsAssignableFrom(child.GetType()))
					{
						method.Invoke(container, new object[] { child });
						return true;
					}
				}
			}

			return false;
		}
		#endregion
	}
}
