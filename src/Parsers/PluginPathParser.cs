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

namespace Zongsoft.Plugins.Parsers
{
	public class PluginPathParser : Parser
	{
		public override Type GetValueType(ParserContext context)
		{
			if(string.IsNullOrWhiteSpace(context.Text))
				return null;

			//处理特殊路径表达式，即获取插件文件路径或目录
			if(context.Text.StartsWith("~"))
				return typeof(string);

			var path = this.ResolveText(context.Text);
			var node = context.PluginContext.PluginTree.Find(path);

			return node?.ValueType;
		}

		public override object Parse(ParserContext context)
		{
			if(string.IsNullOrWhiteSpace(context.Text))
				return null;

			//处理特殊路径表达式，即获取插件文件路径或目录
			if(context.Text == "~")
				return context.Plugin.FilePath;
			else if(context.Text == "~/")
				return System.IO.Path.GetDirectoryName(context.Plugin.FilePath);

			var mode = ObtainMode.Auto;
			var path = this.ResolveText(context.Text, out mode);

			return context.PluginContext.ResolvePath(path, context.Node, mode);
		}

		internal string ResolveText(string text)
		{
			ObtainMode mode;
			return this.ResolveText(text, out mode);
		}

		internal string ResolveText(string text, out ObtainMode mode)
		{
			mode = ObtainMode.Auto;

			if(string.IsNullOrWhiteSpace(text))
				return text;

			var parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			if(parts.Length == 2)
				Enum.TryParse<ObtainMode>(parts[1], true, out mode);

			return parts[0];
		}
	}
}
