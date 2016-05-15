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
using System.Collections.Generic;

using Zongsoft.Services;
using Zongsoft.Plugins.Parsers;

namespace Zongsoft.Services.Plugins.Parsers
{
	public class CommandParser : Parser
	{
		#region 解析方法
		public override object Parse(ParserContext context)
		{
			return new DelegateCommand(context.Text);
		}
		#endregion

		#region 嵌套子类
		private class DelegateCommand : CommandBase
		{
			#region 私有变量
			private string _commandText;
			#endregion

			#region 构造函数
			public DelegateCommand(string commandText)
			{
				_commandText = commandText;
			}
			#endregion

			#region 执行方法
			protected override object OnExecute(object parameter)
			{
				var commandExecutor = CommandExecutor.Default;

				if(commandExecutor == null)
					throw new InvalidOperationException("Can not get the CommandExecutor from 'Zongsoft.Services.CommandExecutor.Default' static member.");

				return commandExecutor.Execute(_commandText, parameter);
			}
			#endregion
		}
		#endregion
	}
}
