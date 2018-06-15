﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2010-2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.ObjectModel;

namespace Zongsoft.Plugins
{
	public class BuiltinBehaviorCollection : KeyedCollection<string, BuiltinBehavior>
	{
		#region 成员字段
		private Builtin _builtin;
		#endregion

		#region 构造函数
		public BuiltinBehaviorCollection(Builtin builtin) : base(StringComparer.OrdinalIgnoreCase)
		{
			if(builtin == null)
				throw new ArgumentNullException(nameof(builtin));

			_builtin = builtin;
		}
		#endregion

		#region 公共方法
		public BuiltinBehavior Add(string name, string text = null)
		{
			var result = new BuiltinBehavior(_builtin, name, text);
			this.Add(result);
			return result;
		}

		public bool TryGet(string name, out BuiltinBehavior value)
		{
			value = null;

			var dictionary = this.Dictionary;

			if(dictionary != null)
				return dictionary.TryGetValue(name, out value);

			foreach(var item in Items)
			{
				if(this.Comparer.Equals(GetKeyForItem(item), name))
				{
					value = item;
					return true;
				}
			}

			return false;
		}

		public T GetBehaviorValue<T>(string name, T defaultValue = default(T))
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			var index = name.IndexOf('.');

			if(index < 0 || index >= name.Length - 1)
				throw new ArgumentException();

			BuiltinBehavior behavior = null;

			if(this.TryGet(name.Substring(0, index), out behavior))
				return behavior.GetPropertyValue<T>(name.Substring(index + 1), defaultValue);

			return defaultValue;
		}
		#endregion

		#region 重写方法
		protected override string GetKeyForItem(BuiltinBehavior item)
		{
			return item.Name;
		}

		protected override void InsertItem(int index, BuiltinBehavior item)
		{
			if(item == null)
				throw new ArgumentNullException(nameof(item));

			//设置属性的所有者
			item.Builtin = _builtin;

			//调用基类同名方法
			base.InsertItem(index, item);
		}
		#endregion
	}
}
