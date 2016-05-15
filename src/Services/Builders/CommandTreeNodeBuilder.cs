﻿/*
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

using Zongsoft.Plugins;
using Zongsoft.Plugins.Builders;

namespace Zongsoft.Services.Plugins.Builders
{
	public class CommandTreeNodeBuilder : BuilderBase, IAppender
	{
		public override object Build(BuilderContext context)
		{
			if(context.Builtin.BuiltinType == null || context.Builtin.BuiltinType.Type == null)
				return new CommandTreeNode(context.Builtin.Name, null);

			var command = base.Build(context) as ICommand;

			if(command != null)
				return new CommandTreeNode(command, null);

			return null;
		}

		bool IAppender.Append(AppenderContext context)
		{
			var commandNode = context.Container as CommandTreeNode;

			if(commandNode != null)
			{
				if(context.Value is ICommand)
					commandNode.Children.Add((ICommand)context.Value);
				else if(context.Value is CommandTreeNode)
					commandNode.Children.Add((CommandTreeNode)context.Value);
				else
					return false;

				return true;
			}

			return false;
		}
	}
}
