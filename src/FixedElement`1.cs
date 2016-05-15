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
using System.Reflection;
using System.Collections.Generic;

namespace Zongsoft.Plugins
{
	[Serializable]
	public class FixedElement<T> : FixedElement
	{
		#region 构造函数
		public FixedElement(Type type, string name, Plugin plugin, FixedElementType elementType) : base(type, name, plugin, elementType)
		{
		}

		public FixedElement(string typeName, string name, Plugin plugin, FixedElementType elementType) : base(typeName, name, plugin, elementType)
		{
		}
		#endregion

		#region 公共属性
		public T Value
		{
			get
			{
				return (T)base.GetValue();
			}
		}
		#endregion

		#region 重写方法
		protected override bool ValidateType(Type type)
		{
			if(type == null)
				return false;

			return typeof(T).IsAssignableFrom(type);
		}
		#endregion
	}
}
