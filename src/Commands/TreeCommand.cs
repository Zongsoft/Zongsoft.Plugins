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
	[DisplayName("${Text.TreeCommand}")]
	[Description("${Text.TreeCommand.Description}")]
	[CommandOption("maxDepth", Type = typeof(int), DefaultValue = 3, Description = "${Text.TreeCommand.Options.MaxDepth}")]
	[CommandOption("fullPath", Description = "${Text.TreeCommand.Options.FullPath}")]
	public class TreeCommand : CommandBase<CommandContext>
	{
		#region 成员字段
		private PluginContext _pluginContext;
		#endregion

		#region 构造函数
		public TreeCommand(PluginContext pluginContext) : this("Tree", pluginContext)
		{
		}

		public TreeCommand(string name, PluginContext pluginContext) : base(name)
		{
			if(pluginContext == null)
				throw new ArgumentNullException("pluginContext");

			_pluginContext = pluginContext;
		}
		#endregion

		#region 重写方法
		protected override object OnExecute(CommandContext context)
		{
			var node = context.Parameter as PluginTreeNode;

			if(node == null)
			{
				if(context.Parameter != null)
					throw new CommandException(ResourceUtility.GetString("Text.Message.InvalidCommandParameter", context.CommandNode.FullPath));

				if(context.Expression.Arguments.Length == 0)
					throw new CommandException(ResourceUtility.GetString("Text.Message.MissingCommandArguments"));

				if(context.Expression.Arguments.Length > 1)
					throw new CommandException(ResourceUtility.GetString("Text.Message.CommandArgumentsTooMany"));

				node = _pluginContext.PluginTree.Find(context.Expression.Arguments[0]);

				if(node == null)
				{
					context.Output.WriteLine(CommandOutletColor.DarkRed, ResourceUtility.GetString("Text.Message.PluginNodeNotFound", context.Expression.Arguments[0]));
					return null;
				}
			}

			this.WritePluginTree(context.Output, node, context.Expression.Options.GetValue<int>("maxDepth"), 0, 0, context.Expression.Options.Contains("fullPath"));
			return node;
		}
		#endregion

		public void WritePluginTree(ICommandOutlet output, PluginTreeNode node, int maxDepth, int depth, int index, bool isFullPath)
		{
			if(node == null)
				return;

			var indent = depth > 0 ? new string('\t', depth) : string.Empty;

			if(depth > 0)
			{
				output.Write(CommandOutletColor.DarkMagenta, indent + "[{0}.{1}] ", depth, index);
			}

			output.Write(isFullPath ? node.FullPath : node.Name);
			output.Write(CommandOutletColor.DarkGray, " [{0}]", node.NodeType);

			if(node.Plugin == null)
				output.WriteLine();
			else
			{
				output.Write(CommandOutletColor.DarkGreen, "@");
				output.WriteLine(CommandOutletColor.DarkGray, node.Plugin.Name);
			}

			var target = node.UnwrapValue(ObtainMode.Never, null);
			if(target != null)
				output.WriteLine(CommandOutletColor.DarkYellow, "{0}{1}", indent, target.GetType().FullName);

			if(maxDepth > 0 && depth >= maxDepth)
				return;

			for(int i = 0; i < node.Children.Count; i++)
			{
				WritePluginTree(output, node.Children[i], maxDepth, depth + 1, i + 1, isFullPath);
			}
		}
	}
}
