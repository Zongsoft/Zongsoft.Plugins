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

using Zongsoft.Plugins;
using Zongsoft.Services;

namespace Zongsoft.Terminals.Plugins
{
	public class Workbench : Zongsoft.Plugins.WorkbenchBase
	{
		#region 成员字段
		private TerminalCommandExecutor _executor;
		#endregion

		#region 构造函数
		public Workbench(PluginApplicationContext applicationContext) : base(applicationContext)
		{
		}
		#endregion

		#region 公共属性
		public TerminalCommandExecutor Executor
		{
			get
			{
				return _executor ?? CommandExecutor.Default as TerminalCommandExecutor;
			}
			set
			{
				_executor = value;
			}
		}
		#endregion

		#region 打开方法
		protected override void OnStart(string[] args)
		{
			var executor = this.Executor;

			if(executor == null)
				throw new InvalidOperationException("The command executor is null.");

			//调用基类同名方法
			base.OnStart(args);

			//激发“Opened”事件
			this.OnOpened(EventArgs.Empty);

			//启动命令运行器
			executor.Run();

			//关闭命令执行器
			this.Close();
		}
		#endregion
	}
}
