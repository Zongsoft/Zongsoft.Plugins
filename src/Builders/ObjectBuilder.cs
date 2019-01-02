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
		public ObjectBuilder()
		{
		}

		public ObjectBuilder(IEnumerable<string> ignoredProperties) : base(ignoredProperties)
		{
		}
		#endregion

		#region 重写方法
		public override Type GetValueType(Builtin builtin)
		{
			//调用基类同名方法
			var type = base.GetValueType(builtin);

			if(type == null)
			{
				//尝试获取value属性值的类型
				if(builtin.Properties.TryGet("value", out var property) && Parsers.Parser.CanParse(property.RawValue))
					return Parsers.Parser.GetValueType(property.RawValue, builtin);

				//返回所有者元素类型（如果所有者如果是一个泛型集合的话，否则返回空）
				return PluginUtility.GetOwnerElementType(builtin.Node);
			}

			return type;
		}

		public override object Build(BuilderContext context)
		{
			//如果定义了类型（或简单的type或完整的constructor）
			if(context.Builtin.BuiltinType != null)
				return base.Build(context);

			//如果定义了value属性，则采用该属性值作为构建结果
			if(context.Builtin.Properties.TryGetValue("value", out var result) && result != null)
			{
				//必须将value自身作为忽略属性项
				var ignoredProperties = this.IgnoredProperties == null ?
					new HashSet<string>(new[] { "value" }, StringComparer.OrdinalIgnoreCase) :
					new HashSet<string>(this.IgnoredProperties.Concat(new[] { "value" }), StringComparer.OrdinalIgnoreCase);

				//更新构件属性到目标对象的属性中
				PluginUtility.UpdateProperties(result, context.Builtin, ignoredProperties);

				//你会value属性值
				return result;
			}

			//调用基类同名方法
			return base.Build(context);
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

			//第一步(a)：确认容器对象实现的各种泛型字典接口
			var add = GetDictionaryAddMethod(container, child.GetType(), out var valueType);

			if(add != null && Common.Convert.TryConvertValue(child, valueType, out var value))
			{
				add.DynamicInvoke(key, value);
				return true;
			}

			//第一步(b)：确认容器对象实现的各种泛型集合接口
			add = GetCollectionAddMethod(container, child.GetType(), out valueType);

			if(add != null)
			{
				//获取子元素的元素类型（如果子元素不是一个可遍历对象，则返回的元素类型为空）
				var childElementType = Common.TypeExtension.GetElementType(child.GetType());

				if(childElementType != null && valueType.IsAssignableFrom(childElementType))
				{
					int count = 0;

					foreach(var entry in (IEnumerable)child)
					{
						if(Common.Convert.TryConvertValue(entry, valueType, out var item))
						{
							add.DynamicInvoke(item);
							count++;
						}
					}

					if(count > 0)
						return true;
					else
						return false;
				}
				else
				{
					if(Common.Convert.TryConvertValue(child, valueType, out var item))
					{
						add.DynamicInvoke(item);
						return true;
					}
				}
			}

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

		private Delegate GetDictionaryAddMethod(object container, Type childType, out Type valueType)
		{
			//设置输出参数默认值
			valueType = null;

			//获取容器类型实现的所有接口
			var interfaces = container.GetType().GetInterfaces();

			//确保泛型字典接口在非泛型字典接口之前
			var contracts = interfaces.Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IDictionary<,>))
				.Concat(interfaces.Where(p => p == typeof(IDictionary)));

			foreach(var contract in contracts)
			{
				MethodInfo method = null;
				var mapping = container.GetType().GetInterfaceMap(contract);

				for(int i = 0; i < mapping.InterfaceMethods.Length; i++)
				{
					if(mapping.InterfaceMethods[i].Name == "Add")
					{
						var parameters = mapping.InterfaceMethods[i].GetParameters();

						if(parameters.Length == 2 && (parameters[0].ParameterType == typeof(string) || parameters[0].ParameterType == typeof(object)) &&
						   parameters[1].ParameterType.IsAssignableFrom(childType))
							method = mapping.TargetMethods[i];

						break;
					}
				}

				if(method != null)
				{
					valueType = method.GetParameters()[1].ParameterType;
					return method.CreateDelegate(typeof(Action<,>).MakeGenericType(method.GetParameters()[0].ParameterType, valueType), container);
				}
			}

			return null;
		}

		private Delegate GetCollectionAddMethod(object container, Type childType, out Type valueType)
		{
			//设置输出参数默认值
			valueType = null;

			//获取容器类型实现的所有接口
			var interfaces = container.GetType().GetInterfaces();

			//确保泛型集合接口在非泛型集合接口之前
			var contracts = interfaces.Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(ICollection<>))
				.Concat(interfaces.Where(p => p == typeof(ICollection)));

			foreach(var contract in contracts)
			{
				MethodInfo method = null;
				var mapping = container.GetType().GetInterfaceMap(contract);

				for(int i = 0; i < mapping.InterfaceMethods.Length; i++)
				{
					if(mapping.InterfaceMethods[i].Name == "Add")
					{
						var parameters = mapping.InterfaceMethods[i].GetParameters();

						if(parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(childType))
							method = mapping.TargetMethods[i];

						break;
					}
				}

				if(method != null)
				{
					valueType = method.GetParameters()[0].ParameterType;
					return method.CreateDelegate(typeof(Action<>).MakeGenericType(valueType), container);
				}
			}

			return null;
		}
		#endregion
	}
}
