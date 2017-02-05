/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 *
 * Copyright (C) 2010-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
	/// 表示插件路径表达式的类。
	/// </summary>
	/// <remarks>
	///		<para>插件路径表达式由“路径”和“访问成员”两部分组成，其文本格式如下：</para>
	///		<list type="number">
	///			<item>
	///				<term>绝对路径：/root/node1/node2/node3.property1.property2</term>
	///				<term>相对路径：../siblingNode/node1/node2.property1.property2 或者 ./childNode/node1/node2.property1.property2</term>
	///				<term>属性路径：../@property1.property2 或者 ./@collectionProperty[index]（对于本节点的属性也可以简写成：@property1.property2）</term>
	///			</item>
	///		</list>
	/// </remarks>
	public class PluginPath
	{
		#region 成员字段
		private string _path;
		private string[] _segments;
		private string[] _members;
		private Zongsoft.IO.PathAnchor _anchor;
		#endregion

		#region 构造函数
		private PluginPath(string[] segments, string[] members)
		{
			_segments = segments ?? new string[0];
			_members = members ?? new string[0];
			_anchor = IO.PathAnchor.None;
			_path = string.Empty;

			if(segments.Length > 0)
			{
				switch(segments[0])
				{
					case "":
						_anchor = IO.PathAnchor.Root;
						break;
					case ".":
						_anchor = IO.PathAnchor.Current;
						break;
					case "..":
						_anchor = IO.PathAnchor.Parent;
						break;
				}

				if(segments.Length == 1 && segments[0].Length == 0)
					_path = "/";
				else
					_path = string.Join("/", segments);
			}
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取插件路径的锚定点。
		/// </summary>
		public IO.PathAnchor Anchor
		{
			get
			{
				return _anchor;
			}
		}

		/// <summary>
		/// 获取插件路径表达式的路径文本。
		/// </summary>
		public string Path
		{
			get
			{
				return _path;
			}
		}

		/// <summary>
		/// 获取包含构成<see cref="Path"/>路径段的数组。
		/// </summary>
		public string[] Segments
		{
			get
			{
				return _segments;
			}
		}

		/// <summary>
		/// 获取插件路径表达式中的成员名数组。
		/// </summary>
		public string[] Members
		{
			get
			{
				return _members;
			}
		}
		#endregion

		#region 静态方法
		[Obsolete("Please use Zongsoft.IO.Path.Combine(...) method.")]
		public static string Combine(params string[] parts)
		{
			return Zongsoft.IO.Path.Combine(parts);

			//if(parts == null || parts.Length < 1)
			//	return string.Empty;

			//StringBuilder text = new StringBuilder();
			//string temp;

			//foreach(string part in parts)
			//{
			//	if(string.IsNullOrWhiteSpace(part))
			//		continue;

			//	temp = part.Trim('/', ' ', '\t');

			//	if(temp.Length > 0)
			//		text.Append('/' + temp);
			//}

			//return text.ToString();
		}

		public static PluginPath Parse(string text)
		{
			return ParseCore(text, message =>
			{
				throw new PluginException(message);
			});
		}

		public static bool TryParse(string text, out PluginPath result)
		{
			result = null;

			if(string.IsNullOrEmpty(text))
				return false;

			result = ParseCore(text, null);
			return result != null;
		}
		#endregion

		#region 私有方法
		private static PluginPath ParseCore(string text, Action<string> onError)
		{
			if(string.IsNullOrEmpty(text))
				return null;

			var part = string.Empty;
			var parts = new List<string>();
			var spaces = 0;

			for(int i = 0; i < text.Length; i++)
			{
				var chr = text[i];

				switch(chr)
				{
					case '.':
						if(parts.Count == 0)
						{
							switch(part)
							{
								case "":
								case ".":
									part += chr;
									break;
								case "..":
									onError?.Invoke($"Invalid path anchor in the \"{text}\" path expression.");
									return null;
								default:
									parts.Add(part);
									part = string.Empty;

									return new PluginPath(parts.ToArray(), ParseMembers(text, i + 1, message => onError?.Invoke(message)));
							}
						}
						else
						{
							if(part.Length == 0)
							{
								onError?.Invoke($"Invalid path expression:'{text}'.");
								return null;
							}

							parts.Add(part);
							part = string.Empty;

							return new PluginPath(parts.ToArray(), ParseMembers(text, i + 1, message => onError?.Invoke(message)));
						}

						spaces = 0;

						break;
					case '/':
					case '\\':
						if(part.Length > 0)
						{
							parts.Add(part);
							part = string.Empty;
						}
						else
						{
							if(parts.Count > 0)
							{
								onError?.Invoke($"Contains multiple path separators in the \"{text}\" path expression.");
								return null;
							}

							parts.Add(string.Empty);
						}

						spaces = 0;

						break;
					case '[':
					case '@':
						if(part.Length > 0)
						{
							parts.Add(part);
							part = string.Empty;
						}

						return new PluginPath(parts.ToArray(), ParseMembers(text, i + (chr == '[' ? 0 : 1), message => onError?.Invoke(message)));
					case ' ':
						if(part.Length > 0)
							spaces++;

						break;
					default:
						if(IsIllegalPathChars(chr))
						{
							onError?.Invoke($"Contains '{chr}' illegal character in this \"{text}\" path expression.");
							return null;
						}

						part += (spaces > 0 ? new string(' ', spaces) : "") + chr;
						break;
				}
			}

			//将最后的项加入到列表中
			if(part.Length > 0)
				parts.Add(part);

			if(parts.Count == 0)
				return null;

			return new PluginPath(parts.ToArray(), null);
		}

		private static string[] ParseMembers(string text, int offset, Action<string> onError)
		{
			if(offset < 0 || offset >= text.Length)
				return null;

			var part = string.Empty;
			var parts = new List<string>();
			var quote = '\0';
			var escaping = false;
			var spaces = 0;

			for(int i = offset; i < text.Length; i++)
			{
				var chr = text[i];

				if(quote != '\0')
				{
					if(escaping)
					{
						char escapedChar;

						if(EscapeChar(chr, out escapedChar))
							part += escapedChar;
						else
							part += '\\' + chr;
					}
					else
					{
						if(chr == quote)
							quote = '\0';

						if(chr != '\\')
							part += chr;
					}

					//转义状态只有在引号里面才可能发生
					escaping = chr == '\\' && !escaping;
				}
				else
				{
					switch(chr)
					{
						case '.':
							if(part.Length > 0 && part[0] == '[')
							{
								onError?.Invoke($"Missing matched ']' symbol in the \"{text}\" path expression.");
								return null;
							}

							spaces = 0;

							if(part.Length > 0)
							{
								parts.Add(part);
								part = string.Empty;
							}

							break;
						case '"':
						case '\'':
							if(part.Length != 1 || part[0] != '[')
							{
								onError?.Invoke($"The quotation mark must be in indexer symbol.");
								return null;
							}

							spaces = 0;
							quote = chr;
							part += chr;

							break;
						case '\\':
							onError?.Invoke($"The escaping character(\\) must be in quotes.");
							return null;
						case '[':
							if(part.Length > 0)
								parts.Add(part);

							spaces = 0;
							part = "[";

							break;
						case ']':
							if(part.Length == 0 || part[0] != '[')
							{
								onError?.Invoke($"Missing matched '[' symbol in the \"{text}\" path expresion.");
								return null;
							}

							spaces = 0;
							parts.Add(part + chr);
							part = string.Empty;

							break;
						default:
							if(char.IsWhiteSpace(chr))
							{
								//忽略成员前面的空白字符，或者字符[后面的空格
								if(part.Length == 0 || (part.Length == 1 && part[0] == '['))
									break;

								spaces++;
							}
							else if(char.IsLetter(chr) || chr == '_')
							{
								if(spaces > 0)
								{
									onError?.Invoke($"The member part contains one or many whitespaces.");
									return null;
								}

								part += chr;
							}
							else if(char.IsDigit(chr))
							{
								if(part.Length == 0)
								{
									onError?.Invoke("The first character of member part must be a letter or underscore(_).");
									return null;
								}

								if(spaces > 0)
								{
									onError?.Invoke($"The member part contains whitespaces.");
									return null;
								}

								part += chr;
							}
							else
							{
								onError?.Invoke($"Contains a '{chr}' illegal character in the \"{text}\" path expression.");
								return null;
							}
							break;
					}
				}
			}

			//如果还处在引号里面，则激发错误
			if(quote != '\0')
			{
				onError?.Invoke($"Missing closing quotation mark in the \"{text}\" path expression.");
				return null;
			}

			if(part.Length > 0)
			{
				if(part[0] == '[' && part[part.Length - 1] != ']')
				{
					onError?.Invoke($"Missing matched ']' symbol in the \"{text}\" path expression.");
					return null;
				}

				parts.Add(part);
			}

			return parts.ToArray();
		}

		private static bool EscapeChar(char chr, out char escapedChar)
		{
			switch(chr)
			{
				case '"':
				case '\'':
				case '\\':
				case '[':
				case ']':
					escapedChar = chr;
					return true;
				case 's':
					escapedChar = ' ';
					return true;
				case 't':
					escapedChar = '\t';
					return true;
				case 'n':
					escapedChar = '\n';
					return true;
				case 'r':
					escapedChar = '\r';
					return true;
			}

			escapedChar = chr;
			return false;
		}

		private static bool IsIllegalPathChars(char chr)
		{
			foreach(var c in System.IO.Path.GetInvalidPathChars())
			{
				if(chr == c)
					return true;
			}

			return false;
		}
		#endregion

		#region 内部方法
		internal static string PreparePathText(string text)
		{
			ObtainMode mode;
			return PreparePathText(text, out mode);
		}

		internal static string PreparePathText(string text, out ObtainMode mode)
		{
			mode = ObtainMode.Auto;

			if(string.IsNullOrEmpty(text))
				return string.Empty;

			var index = text.LastIndexOf(',');

			if(index < 0)
				return text;

			if(index < text.Length - 1)
				Enum.TryParse<ObtainMode>(text.Substring(index + 1), true, out mode);

			return text.Substring(0, index);
		}
		#endregion
	}
}
