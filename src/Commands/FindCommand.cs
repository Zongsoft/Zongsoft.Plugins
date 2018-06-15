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
using System.Collections.Generic;

using Zongsoft.Services;
using Zongsoft.Resources;

namespace Zongsoft.Plugins.Commands
{
	[DisplayName("${Text.FindCommand}")]
	[Description("${Text.FindCommand.Description}")]
	[CommandOption("obtain", Type = typeof(ObtainMode), DefaultValue = ObtainMode.Never, Description = "${Text.FindCommand.Options.ObtainMode}")]
	[CommandOption("maxDepth", Type = typeof(int), DefaultValue = 3, Description = "${Text.FindCommand.Options.MaxDepth}")]
	public class FindCommand : CommandBase<CommandContext>
	{
		#region 成员字段
		private PluginContext _pluginContext;
		#endregion

		#region 构造函数
		public FindCommand(PluginContext pluginContext) : this("Find", pluginContext)
		{
		}

		public FindCommand(string name, PluginContext pluginContext) : base(name)
		{
			if(pluginContext == null)
				throw new ArgumentNullException("pluginContext");

			_pluginContext = pluginContext;
		}
		#endregion

		#region 重写方法
		protected override object OnExecute(CommandContext context)
		{
			if(context.Expression.Arguments.Length == 0)
				throw new CommandException(ResourceUtility.GetString("Text.Message.MissingCommandArguments"));

			var result = new PluginTreeNode[context.Expression.Arguments.Length];

			for(int i = 0; i < context.Expression.Arguments.Length; i++)
			{
				result[i] = _pluginContext.PluginTree.Find(context.Expression.Arguments[i]);

				if(result[i] == null)
					context.Output.WriteLine(CommandOutletColor.DarkRed, ResourceUtility.GetString("Text.Message.PluginNodeNotFound", context.Expression.Arguments[i]));

				Utility.PrintPluginNode(context.Output, result[i],
							context.Expression.Options.GetValue<ObtainMode>("obtain"),
							context.Expression.Options.GetValue<int>("maxDepth"));
			}

			if(result.Length == 1)
				return result[0];

			return result;
		}
		#endregion
	}
}
