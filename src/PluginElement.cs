/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
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
using System.ComponentModel;

namespace Zongsoft.Plugins
{
	[Serializable]
	public abstract class PluginElement : MarshalByRefObject, INotifyPropertyChanged
	{
		#region 事件定义
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region 成员字段
		private string _name;
		private Plugin _plugin;
		#endregion

		#region 构造函数
		protected PluginElement(string name) : this(name, null)
		{
		}

		protected PluginElement(string name, Plugin plugin)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			this.Name = name;
			_plugin = plugin;
		}

		internal PluginElement(string name, bool ignoreNameValidation)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(ignoreNameValidation)
				_name = name;
			else
				this.Name = name;
		}
		#endregion

		#region 公共属性
		public string Name
		{
			get
			{
				return _name;
			}
			private set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				if(Zongsoft.Common.StringExtension.ContainsCharacters(value, @"\/.,:;'""`@%^&*?!()[]{}|"))
					throw new ArgumentException(string.Format("The '{0}' name of plugin-element contains invalid characters in this argument.", value));

				if(string.Equals(_name, value.Trim(), StringComparison.OrdinalIgnoreCase))
					return;

				_name = value.Trim();

				//激发“PropertyChanged”事件
				this.OnPropertyChanged("Name");
			}
		}

		public Plugin Plugin
		{
			get
			{
				return _plugin;
			}
			protected set
			{
				if(object.ReferenceEquals(_plugin, value))
					return;

				_plugin = value;

				//激发“PropertyChanged”事件
				this.OnPropertyChanged("Plugin");
			}
		}
		#endregion

		#region 保护方法
		protected void OnPropertyChanged(string propertyName)
		{
			this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if(this.PropertyChanged != null)
				this.PropertyChanged(this, e);
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			var plugin = _plugin;

			if(plugin == null)
				return string.Format("{0}[{1}]", _name, this.GetType().Name);
			else
				return string.Format("{0}[{1}]@{2}", _name, this.GetType().Name, _plugin.Name);
		}
		#endregion
	}
}
