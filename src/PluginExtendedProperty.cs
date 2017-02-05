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
using System.Threading;

namespace Zongsoft.Plugins
{
	public class PluginExtendedProperty
	{
		#region 成员变量
		private string _name;
		private object _value;
		private PluginTreeNode _valueNode;
		private string _rawValue;
		private Plugin _plugin;
		private PluginElement _owner;
		#endregion

		#region 私有变量
		private readonly object _syncRoot;
		private int _valueEvaluated;
		#endregion

		#region 构造函数
		internal PluginExtendedProperty(PluginElement owner, string name, string rawValue, Plugin plugin)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(plugin == null)
				throw new ArgumentNullException("plugin");

			_owner = owner;
			_name = name.Trim();
			_rawValue = rawValue;
			_plugin = plugin;
			_syncRoot = new object();
		}

		internal PluginExtendedProperty(PluginElement owner, string name, PluginTreeNode valueNode, Plugin plugin)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(valueNode == null)
				throw new ArgumentNullException("valueNode");

			if(plugin == null)
				throw new ArgumentNullException("plugin");

			_owner = owner;
			_name = name.Trim();
			_valueNode = valueNode;
			_rawValue = valueNode.FullPath;
			_plugin = plugin;
			_syncRoot = new object();
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前扩展属性的名称。
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// 获取或设置当前扩展属性的原生文本值。
		/// </summary>
		/// <remarks>
		///		<para>如果该属性发生改变，在下次获取<see cref="Value"/>属性时将自动引发重新计算。</para>
		/// </remarks>
		public string RawValue
		{
			get
			{
				return _rawValue;
			}
			set
			{
				if(string.Equals(_rawValue, value, StringComparison.Ordinal))
					return;

				_rawValue = value;
				_valueEvaluated = 0;
			}
		}

		/// <summary>
		/// 获取当前扩展属性的定义插件。
		/// </summary>
		/// <remarks>
		///		<para>注意：该属性值表示本扩展属性是由哪个插件扩展的。因此它未必等同于<see cref="Owner"/>属性对应的<seealso cref="PluginElement"/>类型中的Plugin属性值。</para>
		/// </remarks>
		public Plugin Plugin
		{
			get
			{
				return _plugin;
			}
		}

		/// <summary>
		/// 获取当前扩展属性的所有者。
		/// </summary>
		public PluginElement Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// 获取当前扩展属性的值。
		/// </summary>
		/// <remarks>
		///		<para>注意：当该属性值被计算过后就不在重复计算。</para>
		/// </remarks>
		public object Value
		{
			get
			{
				var valueEvaluated = Interlocked.CompareExchange(ref _valueEvaluated, 1, 0);

				if(valueEvaluated == 0)
					_value = this.GetValue(null, null);

				return _value;
			}
		}
		#endregion

		#region 公共方法
		public object GetValue(Type valueType)
		{
			object defaultValue = valueType == null ? null : Zongsoft.Common.TypeExtension.GetDefaultValue(valueType);
			return this.GetValue(valueType, defaultValue);
		}

		public object GetValue(Type valueType, object defaultValue)
		{
			if(_valueNode == null)
				return PluginUtility.ResolveValue(_owner, _rawValue, _name, valueType, defaultValue);

			var result = _valueNode.UnwrapValue(ObtainMode.Auto, _owner);

			if(valueType != null)
				result = Zongsoft.Common.Convert.ConvertValue(result, valueType, defaultValue);

			return result;
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			return string.Format("{0}=\"{1}\"", this.Name, this.RawValue);
		}
		#endregion
	}
}
