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

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 表示工作台的接口，包含对工作台的基本行为特性的定义。
	/// </summary>
	public interface IWorkbenchBase
	{
		/// <summary>当工作台被打开后。</summary>
		event EventHandler Opened;
		/// <summary>当工作台被打开前。</summary>
		event EventHandler Opening;
		/// <summary>当工作台被关闭后。</summary>
		event EventHandler Closed;
		/// <summary>当工作台被关闭前。</summary>
		event CancelEventHandler Closing;

		/// <summary>
		/// 获取工作台的当前状态。
		/// </summary>
		WorkbenchStatus Status
		{
			get;
		}

		/// <summary>
		/// 获取或设置工作台标题。
		/// </summary>
		string Title
		{
			get;
			set;
		}

		/// <summary>
		/// 关闭工作台。
		/// </summary>
		/// <returns>如果关闭成功返回真(true)，否则返回假(false)。如果取消关闭操作，亦返回假(false)。</returns>
		bool Close();

		/// <summary>
		/// 启动工作台。
		/// </summary>
		/// <param name="args">传入的启动参数。</param>
		void Open(string[] args);
	}
}
