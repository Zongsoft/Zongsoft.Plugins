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

namespace Zongsoft.Plugins.Parsers
{
	public class ParserExpression
	{
		#region 成员字段
		private string _scheme;
		private string _content;
		private ParserExpression _next;
		#endregion

		#region 构造函数
		internal ParserExpression(string scheme, string content, ParserExpression next = null)
		{
			if(string.IsNullOrWhiteSpace(scheme))
				throw new ArgumentNullException(nameof(scheme));

			_scheme = scheme.ToLowerInvariant().Trim();
			_content = content?.Trim();
			_next = next;
		}
		#endregion

		#region 公共属性
		public string Scheme
		{
			get
			{
				return _scheme;
			}
		}

		public string Content
		{
			get
			{
				return _content;
			}
			set
			{
				_content = value;
			}
		}

		public ParserExpression Next
		{
			get
			{
				return _next;
			}
		}
		#endregion

		#region 静态方法
		public static ParserExpression Parse(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return null;

		}
		#endregion
	}
}
