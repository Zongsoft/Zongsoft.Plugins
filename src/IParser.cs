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

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 表示解析器的接口。
	/// </summary>
	public interface IParser
	{
		/// <summary>
		/// 获取解析器目标对象的类型。
		/// </summary>
		/// <param name="context">解析器上下文对象。</param>
		/// <returns>返回的目标类型。</returns>
		/// <remarks>
		///		<para>该方法尽量以不构建目标类型的方式去获取目标类型。</para>
		/// </remarks>
		Type GetValueType(Parsers.ParserContext context);

		/// <summary>
		/// 解析表达式，返回目标对象。
		/// </summary>
		/// <param name="context">解析器上下文对象。</param>
		/// <returns>返回解析后的目标对象。</returns>
		object Parse(Parsers.ParserContext context);
	}
}
