/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

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

			if(builtin.Properties.TryGet("value", out var property) && Parsers.Parser.CanParse(property.RawValue))
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
			var isAdded = false;

			//第一步(a)：确认容器对象实现的各种泛型字典接口
			isAdded = Common.TypeExtension.IsAssignableFrom(typeof(IDictionary<,>), containerType, genericType =>
			{
				var containerElementTypes = genericType.GetGenericArguments();

				if(containerElementTypes[0] == typeof(string) && Common.Convert.TryConvertValue(child, containerElementTypes[1], out var item))
				{
					//创建容器集合Add方法委托，因为该方法可能是显式实现也可能不是，但优先采用显式实现版本
					var add = Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(containerElementTypes), container, Common.TypeExtension.GetExplicitImplementationName(genericType, "Add"), false, false) ??
					          Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(containerElementTypes), container, "Add", false, false);

					add.DynamicInvoke(key, item);
					return true;
				}

				return null;
			});

			if(isAdded)
				return true;

			//第一步(b)：确认容器对象实现的各种泛型集合接口
			isAdded = Common.TypeExtension.IsAssignableFrom(typeof(ICollection<>), containerType, genericType =>
			{
				var containerElementTypes = genericType.GetGenericArguments();

				//创建容器集合Add方法委托，因为该方法可能是显式实现也可能不是，但优先采用显式实现版本
				var add = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(containerElementTypes), container, Common.TypeExtension.GetExplicitImplementationName(genericType, "Add"), false, false) ??
				          Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(containerElementTypes), container, "Add", false, false);

				//获取子元素的元素类型（如果子元素不是一个可遍历对象，则返回的元素类型为空）
				var childElementType = Common.TypeExtension.GetElementType(child.GetType());

				if(childElementType != null && containerElementTypes[0].IsAssignableFrom(childElementType))
				{
					int count = 0;

					foreach(var entry in (IEnumerable)child)
					{
						if(Common.Convert.TryConvertValue(entry, containerElementTypes[0], out var item))
						{
							add.DynamicInvoke(item);
							count++;
						}
					}

					if(count > 0)
						return true;
					else
						return null;
				}
				else
				{
					if(Common.Convert.TryConvertValue(child, containerElementTypes[0], out var item))
					{
						add.DynamicInvoke(item);
						return true;
					}
				}

				return null;
			});

			if(isAdded)
				return true;

			//第二步(a)：非泛型字典容器处理
			if(typeof(IDictionary).IsAssignableFrom(containerType))
			{
				((IDictionary)container).Add(key, child);
				return true;
			}//第二步(b)：非泛型集合容器处理
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

			//第三步：尝试获取容器对象的默认属性标签
			var attribute = (System.ComponentModel.DefaultPropertyAttribute)Attribute.GetCustomAttribute(containerType, typeof(System.ComponentModel.DefaultPropertyAttribute), true);

			if(attribute != null)
			{
				var property = containerType.GetProperty(attribute.Name);

				if(property != null && this.Append(property.GetValue(container), child, key))
					return true;
			}

			//第四步：进行特定方法绑定
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

			//如果上述所有步骤均未完成则返回失败
			return false;
		}
		#endregion
	}
}
