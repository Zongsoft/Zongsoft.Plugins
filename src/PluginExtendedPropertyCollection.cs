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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Zongsoft.Plugins
{
	public class PluginExtendedPropertyCollection : NameObjectCollectionBase
	{
		#region 成员变量
		private PluginElement _owner;
		#endregion

		#region 构造函数
		public PluginExtendedPropertyCollection(PluginElement owner) : base(StringComparer.OrdinalIgnoreCase)
		{
			if(owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
		}
		#endregion

		#region 公共属性
		public PluginElement Owner
		{
			get
			{
				return _owner;
			}
		}

		public string[] AllKeys
		{
			get
			{
				return base.BaseGetAllKeys();
			}
		}

		/// <summary>
		/// 获取指定名称的扩展属性对象。
		/// </summary>
		/// <param name="name">指定的扩展属性名。</param>
		/// <returns>返回的<seealso cref="PluginExtendedProperty"/>扩展属性对象，如果指定名称的扩展属性不存在则返回空。</returns>
		public PluginExtendedProperty this[string name]
		{
			get
			{
				return (PluginExtendedProperty)base.BaseGet(name);
			}
		}

		public PluginExtendedProperty this[int index]
		{
			get
			{
				return (PluginExtendedProperty)base.BaseGet(index);
			}
		}
		#endregion

		#region 公共方法
		public bool Contains(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
				return false;

			return base.BaseGet(name) != null;
		}

		public object GetValue(string name, Type type, object defaultValue)
		{
			var item = (PluginExtendedProperty)base.BaseGet(name) ?? this.GetPropertyOfBuiltin(name);
			return item == null ? defaultValue : item.GetValue(type, defaultValue);
		}

		public T GetValue<T>(string name)
		{
			return this.GetValue<T>(name, default(T));
		}

		public T GetValue<T>(string name, T defaultValue)
		{
			var item = (PluginExtendedProperty)base.BaseGet(name) ?? this.GetPropertyOfBuiltin(name);
			return item == null ? defaultValue : (T)item.GetValue(typeof(T), defaultValue);
		}

		public string GetRawValue(string name)
		{
			var item = (PluginExtendedProperty)base.BaseGet(name) ?? this.GetPropertyOfBuiltin(name);
			return item == null ? null : item.RawValue;
		}

		public bool TryGetValue(string name, out object value)
		{
			value = null;
			var item = (PluginExtendedProperty)base.BaseGet(name) ?? this.GetPropertyOfBuiltin(name);

			if(item == null)
				return false;

			value = item.Value;
			return true;
		}

		public bool TryGetValue<T>(string name, out T value)
		{
			value = default(T);
			var item = (PluginExtendedProperty)base.BaseGet(name) ?? this.GetPropertyOfBuiltin(name);

			if(item == null)
				return false;

			value = (T)item.GetValue(typeof(T));
			return true;
		}
		#endregion

		#region 内部方法
		internal PluginExtendedProperty Set(string name, object value, Plugin plugin = null)
		{
			PluginExtendedProperty property = null;

			if(value is Builtin)
				property = new PluginExtendedProperty(_owner, name, ((Builtin)value).Node, plugin ?? ((Builtin)value).Plugin);
			else if(value is PluginTreeNode)
				property = new PluginExtendedProperty(_owner, name, (PluginTreeNode)value, plugin ?? ((PluginTreeNode)value).Plugin);
			else if(value is string)
				property = new PluginExtendedProperty(_owner, name, (string)value, plugin ?? _owner.Plugin);
			else
				throw new ArgumentException("Invalid value argument.");

			this.BaseSet(name, property);
			return property;
		}
		#endregion

		#region 私有方法
		private PluginExtendedProperty GetPropertyOfBuiltin(string name)
		{
			var node = _owner as PluginTreeNode;

			if(node != null && node.NodeType == PluginTreeNodeType.Builtin)
				return ((Builtin)node.Value).Properties[name];

			return null;
		}
		#endregion

		#region 遍历方法
		public override IEnumerator GetEnumerator()
		{
			var values = base.BaseGetAllValues();

			for(int i = 0; i < values.Length; i++)
				yield return values[i];
		}
		#endregion
	}
}
