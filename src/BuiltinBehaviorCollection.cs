/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
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
using System.Collections.Generic;

namespace Zongsoft.Plugins
{
	public class BuiltinBehaviorCollection : Zongsoft.Collections.NamedCollectionBase<BuiltinBehavior>
	{
		#region 成员字段
		private Builtin _builtin;
		#endregion

		#region 构造函数
		public BuiltinBehaviorCollection(Builtin builtin)
		{
			if(builtin == null)
				throw new ArgumentNullException("builtin");

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

		public T GetBehaviorValue<T>(string name, T defaultValue = default(T))
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			var parts = name.Split('.');

			if(parts.Length < 2)
				throw new ArgumentException("");

			var behavior = this[parts[0].Trim()];

			if(behavior == null)
				return defaultValue;

			return behavior.GetPropertyValue<T>(parts[1], defaultValue);
		}

		public object GetBehaviorValue(string name, Type valueType, object defaultValue = null)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			var parts = name.Split('.');

			if(parts.Length < 2)
				throw new ArgumentException("");

			var behavior = this[parts[0].Trim()];

			if(behavior == null)
				return defaultValue;

			return behavior.GetPropertyValue(parts[1], valueType, defaultValue);
		}
		#endregion

		#region 重写方法
		protected override void InsertItems(int index, IEnumerable<BuiltinBehavior> items)
		{
			foreach(var item in items)
			{
				if(item.Builtin != null && item.Builtin != _builtin)
					throw new InvalidOperationException();
			}

			//调用基类同名方法
			base.InsertItems(index, items);
		}

		protected override string GetKeyForItem(BuiltinBehavior item)
		{
			return item.Name;
		}
		#endregion
	}
}
