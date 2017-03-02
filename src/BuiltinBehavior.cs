﻿/*
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
	public class BuiltinBehavior
	{
		#region 成员字段
		private Builtin _builtin;
		private string _name;
		private string _text;
		private PluginElementPropertyCollection _properties;
		#endregion

		#region 构造函数
		public BuiltinBehavior(Builtin builtin, string name, string text = null)
		{
			if(builtin == null)
				throw new ArgumentNullException(nameof(builtin));

			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			_builtin = builtin;
			_name = name.Trim();
			_text = text;
			_properties = new PluginElementPropertyCollection(builtin);
		}
		#endregion

		#region 公共属性
		public Builtin Builtin
		{
			get
			{
				return _builtin;
			}
			internal set
			{
				_builtin = value;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
			}
		}

		public PluginElementPropertyCollection Properties
		{
			get
			{
				return _properties;
			}
		}
		#endregion

		#region 公共方法
		public T GetPropertyValue<T>(string propertyName, T defaultValue = default(T))
		{
			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			PluginElementProperty property;

			if(_properties.TryGet(propertyName, out property))
				return (T)property.GetValue(typeof(T), defaultValue);

			return defaultValue;
		}

		public object GetPropertyValue(string propertyName, Type valueType, object defaultValue = null)
		{
			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			PluginElementProperty property;

			if(_properties.TryGet(propertyName, out property))
				return property.GetValue(valueType, defaultValue);

			return defaultValue;
		}
		#endregion
	}
}
