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

namespace Zongsoft.Plugins
{
	[Serializable]
	public class PluginDependency : MarshalByRefObject
	{
		#region 构造函数
		public PluginDependency(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			this.Name = name.Trim();
			this.Plugin = null;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取依赖的插件名。注：此名称不是插件的文件名。
		/// </summary>
		public string Name
		{
			get;
			internal set;
		}

		/// <summary>
		/// 获取依赖的插件对象。
		/// </summary>
		/// <remarks>如果插件未加载完成，该属性返回空(null)。</remarks>
		public Plugin Plugin
		{
			get;
			internal set;
		}
		#endregion
	}
}
