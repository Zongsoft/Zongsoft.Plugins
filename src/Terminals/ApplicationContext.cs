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
using System.IO;
using System.Reflection;

using Zongsoft.Plugins;

namespace Zongsoft.Terminals.Plugins
{
	public class ApplicationContext : Zongsoft.Plugins.PluginApplicationContext
	{
		#region 静态变量
		public new readonly static ApplicationContext Current = new ApplicationContext();
		#endregion

		#region 成员字段
		private Zongsoft.Options.Configuration.OptionConfiguration _configuration;
		#endregion

		#region 私有构造
		private ApplicationContext() : base("Zongsoft.Terminals.Plugins")
		{
			Zongsoft.ComponentModel.ApplicationContextBase.Current = this;
		}
		#endregion

		#region 重写方法
		public override Zongsoft.Options.Configuration.OptionConfiguration Configuration
		{
			get
			{
				if(_configuration == null)
				{
					string filePaht = Path.Combine(this.ApplicationDirectory, Assembly.GetEntryAssembly().GetName().Name) + ".option";

					if(File.Exists(filePaht))
						_configuration = Options.Configuration.OptionConfiguration.Load(filePaht);
					else
						_configuration = new Options.Configuration.OptionConfiguration(filePaht);
				}

				return _configuration;
			}
		}

		protected override IWorkbenchBase CreateWorkbench(string[] args)
		{
			PluginTreeNode node = this.PluginContext.PluginTree.Find(this.PluginContext.Settings.WorkbenchPath);

			if(node != null && node.NodeType == PluginTreeNodeType.Builtin)
				return base.CreateWorkbench(args);

			return new Workbench(this);
		}
		#endregion
	}
}
