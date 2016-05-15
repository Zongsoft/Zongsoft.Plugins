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
using System.Text;
using System.Text.RegularExpressions;

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 表示插件路径的类型。
	/// </summary>
	public enum PluginPathType
	{
		/// <summary>基于根节点的绝对路径。</summary>
		Rooted,
		/// <summary>相对于父节点的路径。</summary>
		Parent,
		/// <summary>相对于当前节点的路径。</summary>
		Current,
	}

	/// <summary>
	/// 提供插件路径文本的解析功能。
	/// </summary>
	/// <remarks>
	///		<para>插件路径文本支持以下几种格式：</para>
	///		<list type="number">
	///			<item>
	///				<term>绝对路径：/root/node1/node2/node3.property1.property2</term>
	///				<term>相对路径：../siblingNode/node1/node2.property1.property2 或者 ./childNode/node1/node2.property1.property2</term>
	///				<term>属性路径：../@property1.property2 或者 ./@property1.property2（对于本节点的属性也可以简写成：@property1.property2）</term>
	///			</item>
	///		</list>
	/// </remarks>
	public static class PluginPath
	{
		#region 私有变量
		/*
^\s*
(?<prefix>\.{1,2})?
(?<path>(/[\w-]+)*)?
(?(path)|(?(prefix)/)@(?<member>[\w]+|\[[^\]]+\]))
(\.(?<member>[\w]+(\[[^\]]+\])?))*
\s*$
		 */
		private static readonly Regex _regex = new Regex(@"^\s*(?<prefix>\.{1,2})?(?<path>(/[\w-]+)*)?(?(path)|(?(prefix)/)@(?<member>[\w]+|\[[^\]]+\]))(\.(?<member>[\w]+(\[[^\]]+\])?))*\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
		#endregion

		public static bool IsPath(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return false;

			return _regex.IsMatch(text);
		}

		public static bool TryResolvePath(string text, out PluginPathType type, out string path, out string[] memberNames)
		{
			type = PluginPathType.Rooted;
			path = string.Empty;
			memberNames = null;

			if(string.IsNullOrWhiteSpace(text))
				return false;

			var match = _regex.Match(text);

			if(match.Success)
			{
				path = match.Groups["path"].Value ?? string.Empty;
				memberNames = new string[match.Groups["member"].Captures.Count];

				switch(match.Groups["prefix"].Value)
				{
					case ".":
						type = PluginPathType.Current;
						path = path.Trim('/');
						break;
					case "..":
						type = PluginPathType.Parent;
						path = path.Trim('/');
						break;
					default:
						if(string.IsNullOrEmpty(path))
							type = PluginPathType.Current;
						break;
				}

				for(int i = 0; i < memberNames.Length; i++)
				{
					memberNames[i] = match.Groups["member"].Captures[i].Value;
				}
			}

			return match.Success;
		}

		#region 公共方法
		public static string Combine(params string[] parts)
		{
			if(parts == null || parts.Length < 1)
				return string.Empty;

			StringBuilder text = new StringBuilder();
			string temp;

			foreach(string part in parts)
			{
				if(string.IsNullOrWhiteSpace(part))
					continue;

				temp = part.Trim('/', ' ', '\t');

				if(temp.Length > 0)
					text.Append('/' + temp);
			}

			return text.ToString();
		}
		#endregion
	}
}
