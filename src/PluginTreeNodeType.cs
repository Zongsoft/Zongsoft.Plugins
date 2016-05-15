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
using System.ComponentModel;

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 表示插件树节点的类型。
	/// </summary>
	public enum PluginTreeNodeType
	{
		/// <summary>空节点(路径节点)，即该节点的<see cref="Zongsoft.Plugins.PluginTreeNode.Value"/>属性为空。</summary>
		[Description("空节点")]
		Empty,

		/// <summary>构件节点，即该节点的<see cref="Zongsoft.Plugins.PluginTreeNode.Value"/>属性值的类型为<seealso cref="Zongsoft.Plugins.Builtin"/>。</summary>
		[Description("构件节点")]
		Builtin,

		/// <summary>自定义节点，即该节点对应的值为内部挂载的自定义对象。</summary>
		[Description("对象节点")]
		Custom,
	}
}
