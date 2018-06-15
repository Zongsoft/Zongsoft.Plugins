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
using System.Collections.Generic;

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 有关插件运行环境的设置信息。
	/// </summary>
	[Serializable]
	public class PluginSetup : PluginSetupBase, ICloneable
	{
		#region 成员变量
		private IsolationLevel _isolationLevel;
		private string _workbenchPath;
		private string _applicationContextPath;
		#endregion

		#region 构造函数
		public PluginSetup() : this(null, null)
		{
		}

		public PluginSetup(string applicationDirectory) : this(applicationDirectory, null)
		{
		}

		public PluginSetup(string applicationDirectory, string pluginsDirectoryName) : this(applicationDirectory, pluginsDirectoryName, IsolationLevel.None)
		{
		}

		public PluginSetup(string applicationDirectory, string pluginsDirectoryName, IsolationLevel isolationLevel) : base(applicationDirectory, pluginsDirectoryName)
		{
			_isolationLevel = isolationLevel;

			_workbenchPath = "/Workbench";
			_applicationContextPath = "/Workspace/Environment/ApplicationContext";
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置插件的隔离级别。
		/// </summary>
		public IsolationLevel IsolationLevel
		{
			get
			{
				return _isolationLevel;
			}
			set
			{
				if(_isolationLevel == value)
					return;

				_isolationLevel = value;
				this.OnPropertyChanged("IsolationLevel");
			}
		}

		/// <summary>
		/// 获取或设置应用程序上下文位于插件树的路径。默认值为：/Workspace/Environment/ApplicationContext
		/// </summary>
		public string ApplicationContextPath
		{
			get
			{
				return _applicationContextPath;
			}
			set
			{
				if(string.IsNullOrEmpty(value))
					throw new ArgumentNullException();

				if(!string.Equals(_applicationContextPath, value, StringComparison.OrdinalIgnoreCase))
				{
					_applicationContextPath = value.Trim().TrimEnd('/');
					this.OnPropertyChanged("ApplicationContextPath");
				}
			}
		}

		/// <summary>
		/// 获取或设置<seealso cref="Zongsoft.Plugins.IWorkbenchBase"/>工作台位于插件树的路径。默认值为：/Workbench
		/// </summary>
		public string WorkbenchPath
		{
			get
			{
				return _workbenchPath;
			}
			set
			{
				if(string.IsNullOrEmpty(value))
					throw new ArgumentNullException();

				if(!string.Equals(_workbenchPath, value, StringComparison.OrdinalIgnoreCase))
				{
					_workbenchPath = value.Trim().TrimEnd('/');
					this.OnPropertyChanged("WorkbenchPath");
				}
			}
		}
		#endregion

		#region 虚拟方法
		public virtual object Clone()
		{
			return new PluginSetup(this.ApplicationDirectory, this.PluginsDirectoryName)
			{
				IsolationLevel = _isolationLevel,
				ApplicationContextPath = _applicationContextPath,
				WorkbenchPath = _workbenchPath,
			};
		}
		#endregion
	}
}
