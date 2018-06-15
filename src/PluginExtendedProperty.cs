/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
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

namespace Zongsoft.Plugins
{
	public class PluginExtendedProperty : PluginElementProperty
	{
		#region 成员字段
		private Plugin _plugin;
		#endregion

		#region 构造函数
		internal PluginExtendedProperty(PluginElement owner, string name, string rawValue, Plugin plugin) : base(owner, name, rawValue)
		{
			if(plugin == null)
				throw new ArgumentNullException(nameof(plugin));

			_plugin = plugin;
		}

		internal PluginExtendedProperty(PluginElement owner, string name, PluginTreeNode valueNode, Plugin plugin) : base(owner, name, valueNode)
		{
			if(plugin == null)
				throw new ArgumentNullException(nameof(plugin));

			_plugin = plugin;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前扩展属性的定义插件。
		/// </summary>
		/// <remarks>
		///		<para>注意：该属性值表示本扩展属性是由哪个插件扩展的。因此它未必等同于<see cref="Owner"/>属性对应的<seealso cref="PluginElement"/>类型中的Plugin属性值。</para>
		/// </remarks>
		public Plugin Plugin
		{
			get
			{
				return _plugin;
			}
		}
		#endregion
	}
}
