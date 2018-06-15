/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2010-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Text.RegularExpressions;

namespace Zongsoft.Plugins.Parsers
{
	public abstract class Parser : MarshalByRefObject, IParser
	{
		#region 静态成员
		private static readonly Regex _regex = new Regex(@"(?<prefix>[^\{]*)?{\s*(?<scheme>\w+)\s*:\s*(?<value>[^\}]+)\s*}(?<suffix>.*)?", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
		#endregion

		#region 构造函数
		protected Parser()
		{
		}
		#endregion

		#region 静态方法
		public static bool CanParse(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return false;

			return _regex.IsMatch(text);
		}

		public static object Parse(string text, PluginTreeNode node, string memberName, Type memberType)
		{
			if(node == null)
				throw new ArgumentNullException("node");

			return Parse(text, node, (scheme, expression, element) => new ParserContext(scheme, expression, node, memberName, memberType));
		}

		public static object Parse(string text, Builtin builtin, string memberName, Type memberType)
		{
			if(builtin == null)
				throw new ArgumentNullException("builtin");

			return Parse(text, builtin, (scheam, expression, element) => new ParserContext(scheam, expression, builtin, memberName, memberType));
		}

		public static Type GetValueType(string text, Builtin builtin)
		{
			if(string.IsNullOrWhiteSpace(text) || builtin == null)
				return null;

			string scheme, value, prefix, suffix;

			//解析输入的文本
			ResolveText(text, out scheme, out value, out prefix, out suffix);

			if(string.IsNullOrWhiteSpace(scheme))
				return null;

			Plugin plugin = builtin.Plugin;

			if(plugin == null)
				return null;

			//通过插件向上查找指定的解析器
			IParser parser = plugin.GetParser(scheme);

			return parser.GetValueType(new ParserContext(scheme, value, builtin, null, null));
		}

		public static IParser GetParser(string text, PluginElement element)
		{
			if(string.IsNullOrWhiteSpace(text))
				return null;

			Match match = _regex.Match(text);

			if(!match.Success)
				return null;

			var scheme = match.Groups["scheme"].Value;

			if(string.IsNullOrWhiteSpace(scheme))
				return null;

			Plugin plugin = element.Plugin;

			if(plugin == null)
				return null;

			//通过插件向上查找指定的解析器
			return plugin.GetParser(scheme);
		}
		#endregion

		#region 私有方法
		private static object Parse(string text, PluginElement element, Func<string, string, PluginElement, ParserContext> createContext)
		{
			if(string.IsNullOrWhiteSpace(text))
				return text;

			string scheme, value, prefix, suffix;

			//解析输入的文本
			ResolveText(text, out scheme, out value, out prefix, out suffix);

			if(string.IsNullOrWhiteSpace(scheme))
				return value;

			Plugin plugin = element.Plugin;

			if(plugin == null)
				return null;

			//通过插件向上查找指定的解析器
			IParser parser = plugin.GetParser(scheme);

			if(parser == null)
				throw new PluginException(string.Format("This '{0}' parser no found, and use in this '{1}' plugin.", scheme, plugin.Name));

			//创建解析器上下文对象
			var context = createContext(scheme, value, element);
			//调用解析器的解析方法，获取解析结果
			var result = parser.Parse(context);

			//如果表达式文本中无前缀和后缀则直接返回解析结果
			if(string.IsNullOrWhiteSpace(prefix) && string.IsNullOrWhiteSpace(suffix))
				return result;

			//注意：否则将对解析结果与前缀和后缀做文本连接并返回该文本
			return string.Format("{1}{0}{2}", result, prefix, suffix);
		}

		internal static void ResolveText(string text, out string scheme, out string value, out string prefix, out string suffix)
		{
			//设置输出参数的默认值
			scheme = null;
			value = text;
			prefix = null;
			suffix = null;

			if(string.IsNullOrWhiteSpace(text))
				return;

			Match match = _regex.Match(text);

			if(!match.Success)
				throw new PluginException(string.Format("Invalid format of parser, this expression is '{0}'.", text));

			//设置解析器模式为匹配成功的模式值
			scheme = match.Groups["scheme"].Value;
			//返回解析器原始文本中匹配成功的文本值
			value = match.Groups["value"].Value;

			prefix = match.Groups["prefix"].Value;
			suffix = match.Groups["suffix"].Value;
		}
		#endregion

		#region 获取类型
		public virtual Type GetValueType(ParserContext context)
		{
			return this.Parse(context)?.GetType();
		}
		#endregion

		#region 抽象方法
		/// <summary>
		/// 解析目标对象。
		/// </summary>
		/// <returns>返回解析后的对象。</returns>
		public abstract object Parse(ParserContext context);
		#endregion
	}
}
