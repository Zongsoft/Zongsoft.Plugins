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
	public class BuiltinBehavior
	{
		#region 成员字段
		private Builtin _builtin;
		private string _name;
		private string _text;
		private IDictionary<string, string> _properties;
		#endregion

		#region 构造函数
		public BuiltinBehavior(Builtin builtin, string name, string text = null)
		{
			if(builtin == null)
				throw new ArgumentNullException("builtin");

			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			_builtin = builtin;
			_name = name.Trim();
			_text = text;
			_properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region 公共属性
		public Builtin Builtin
		{
			get
			{
				return _builtin;
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

		public IDictionary<string, string> Properties
		{
			get
			{
				return _properties;
			}
		}
		#endregion

		#region 公共方法
		public T Populate<T>(Func<T> creator = null)
		{
			var dictionary = Zongsoft.Collections.DictionaryExtension.ToDictionary<string, object>((System.Collections.IDictionary)_properties);

			return Zongsoft.Runtime.Serialization.DictionarySerializer.Default.Deserialize<T>((System.Collections.IDictionary)dictionary, creator, ctx =>
			{
				if(ctx.Direction == Common.Convert.ObjectResolvingDirection.Get)
				{
					ctx.Handled = false;
					return;
				}

				var text = ctx.Value as string;

				if(text != null)
					ctx.Value = PluginUtility.ResolveValue(_builtin, text, ctx.MemberName, ctx.MemberType, Zongsoft.Common.TypeExtension.GetDefaultValue(ctx.MemberType));
			});
		}

		public T GetPropertyValue<T>(string propertyName, T defaultValue = default(T))
		{
			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			string rawValue;

			if(_properties.TryGetValue(propertyName, out rawValue))
				return Zongsoft.Common.Convert.ConvertValue<T>(PluginUtility.ResolveValue(_builtin, rawValue, propertyName, typeof(T), defaultValue));

			return defaultValue;
		}

		public object GetPropertyValue(string propertyName, Type valueType, object defaultValue = null)
		{
			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			string rawValue;

			if(_properties.TryGetValue(propertyName, out rawValue))
				return PluginUtility.ResolveValue(_builtin, rawValue, propertyName, valueType, defaultValue);

			return defaultValue;
		}
		#endregion
	}
}
