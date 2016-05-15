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
using System.Collections;
using System.Collections.Generic;

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 表示工作台的接口，包含对工作台的基本行为特性的定义。
	/// </summary>
	public interface IWorkbench : IWorkbenchBase
	{
		/// <summary>当视图被激活。</summary>
		event EventHandler<ViewEventArgs> ViewActivate;
		/// <summary>当视图失去焦点，当视图被关闭时也会触发该事件。</summary>
		event EventHandler<ViewEventArgs> ViewDeactivate;

		/// <summary>
		/// 获取当前活动的视图对象。
		/// </summary>
		object ActiveView
		{
			get;
		}

		/// <summary>
		/// 获取当前工作台的所有打开的视图对象。
		/// </summary>
		object[] Views
		{
			get;
		}

		/// <summary>
		/// 获取当前工作台的窗口对象。
		/// </summary>
		object Window
		{
			get;
		}

		/// <summary>
		/// 激活指定名称的视图对象。
		/// </summary>
		/// <param name="name">视图名称。</param>
		/// <returns>被激活的视图对象。</returns>
		void ActivateView(string name);
	}
}
