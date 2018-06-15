﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2010-2015 Zongsoft Corporation <http://www.zongsoft.com>
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

using Zongsoft.Plugins;

namespace Zongsoft.Services.Plugins
{
	public class PluginPredicationContext
	{
		#region 成员变量
		private string _parameter;
		private Builtin _builtin;
		private PluginTreeNode _node;
		private Plugin _plugin;
		#endregion

		#region 构造函数
		public PluginPredicationContext(string parameter, Builtin builtin)
		{
			_parameter = parameter;
			_builtin = builtin;
			_node = builtin.Node;
			_plugin = builtin.Plugin;
		}

		public PluginPredicationContext(string parameter, PluginTreeNode node, Plugin plugin)
		{
			_parameter = parameter;
			_node = node;
			_plugin = plugin ?? node.Plugin;
		}

		public PluginPredicationContext(string parameter, Builtin builtin, PluginTreeNode node, Plugin plugin)
		{
			_parameter = parameter;

			_builtin = builtin;

			if(builtin != null)
			{
				_node = builtin.Node;
				_plugin = builtin.Plugin;
			}

			if(node != null)
			{
				_node = node;
				_plugin = plugin ?? node.Plugin;
			}

			if(plugin != null)
				_plugin = plugin;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获传入的参数文本。
		/// </summary>
		public string Parameter
		{
			get
			{
				return _parameter;
			}
		}

		/// <summary>
		/// 获取待解析文本所在的构件(<see cref="Builtin"/>)，注意：该属性可能返回空值(null)。
		/// </summary>
		public Builtin Builtin
		{
			get
			{
				return _builtin;
			}
		}

		/// <summary>
		/// 获取待解析文本所在的插件树节点(<see cref="PluginTreeNode"/>)，注意：该属性可能返回空值(null)。
		/// </summary>
		public PluginTreeNode Node
		{
			get
			{
				return _node;
			}
		}

		/// <summary>
		/// 获取待解析文本所在构件或插件树节点所隶属的插件对象，注意：该属性可能返回空值(null)。
		/// </summary>
		public Plugin Plugin
		{
			get
			{
				return _plugin;
			}
		}

		/// <summary>
		/// 获取插件应用上下文对象，注意：该属性值可能会为空值(null)。
		/// </summary>
		/// <remarks>
		///		<para>如果当前解析器上下文关联到一个空节点或者自定义节点，则该属性返回空。</para>
		/// </remarks>
		public PluginContext PluginContext
		{
			get
			{
				return _plugin == null ? null : _plugin.Context;
			}
		}
		#endregion
	}
}
