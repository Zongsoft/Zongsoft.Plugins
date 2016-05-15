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

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 表示插件的状态。
	/// </summary>
	public enum PluginStatus
	{
		/// <summary>尚未加载，表示插件刚创建。</summary>
		None = 0,

		/// <summary>表示插件正在加载。</summary>
		Loading,

		/// <summary>表示插件已经成功加载。</summary>
		Loaded,

		/// <summary>表示插件正在卸载。</summary>
		Unloading,

		/// <summary>表示插件已经被卸载。</summary>
		Unloaded,

		/// <summary>表示插件在解析或加过程载中出现错误，该状态的插件的构件不会被挂载到系统中。</summary>
		Failed = 0x80,
	}
}
