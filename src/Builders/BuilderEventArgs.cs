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

namespace Zongsoft.Plugins.Builders
{
	public class BuilderEventArgs : EventArgs
	{
		#region 成员字段
		private BuilderContext _context;
		#endregion

		#region 构造函数
		public BuilderEventArgs(BuilderContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前的构建上下文对象。
		/// </summary>
		public BuilderContext Context
		{
			get
			{
				return _context;
			}
		}
		#endregion
	}
}
