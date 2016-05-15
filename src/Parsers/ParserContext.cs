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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zongsoft.Plugins.Parsers
{
	public class ParserContext : MarshalByRefObject
	{
		#region 成员变量
		private string _text;
		private string _scheme;
		private Builtin _builtin;
		private PluginTreeNode _node;
		private Plugin _plugin;
		private string _memberName;
		private Type _memberType;
		#endregion

		#region 构造函数
		internal ParserContext(string scheme, string text, Plugin plugin, string memberName, Type memberType)
		{
			if(plugin == null)
				throw new ArgumentNullException("plugin");

			this.Initialize(scheme, text, null, null, plugin, memberName, memberType);
		}

		internal ParserContext(string scheme, string text, PluginTreeNode node, string memberName, Type memberType)
		{
			if(node == null)
				throw new ArgumentNullException("plugin");

			this.Initialize(scheme, text, null, node, node.Plugin, memberName, memberType);
		}

		internal ParserContext(string scheme, string text, Builtin builtin, string memberName, Type memberType)
		{
			if(builtin == null)
				throw new ArgumentNullException("builtin");

			this.Initialize(scheme, text, builtin, null, builtin.Plugin, memberName, memberType);
		}
		#endregion

		#region 初始化器
		private void Initialize(string scheme, string text, Builtin builtin, PluginTreeNode node, Plugin plugin, string memberName, Type memberType)
		{
			if(string.IsNullOrWhiteSpace(scheme))
				throw new ArgumentNullException("scheme");

			if(builtin == null && node == null && plugin == null)
				throw new ArgumentException();

			_scheme = scheme;
			_text = text ?? string.Empty;
			_memberName = memberName;
			_memberType = memberType;
			_builtin = builtin;
			_node = node ?? (builtin == null ? null : builtin.Node);
			_plugin = plugin;

			if(plugin == null)
			{
				if(builtin == null)
					_plugin = node.Plugin;
				else
					_plugin = builtin.Plugin;
			}
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取解析文本的方案(即解析器名称)。
		/// </summary>
		public string Scheme
		{
			get
			{
				return _scheme;
			}
		}

		/// <summary>
		/// 获取待解析的不包含解析器名的文本。
		/// </summary>
		public string Text
		{
			get
			{
				return _text;
			}
		}

		/// <summary>
		/// 获取待解析文本所在目标对象的成员名称。
		/// </summary>
		public string MemberName
		{
			get
			{
				return _memberName;
			}
		}

		/// <summary>
		/// 获取待解析文本所在目标对象的成员类型。
		/// </summary>
		public Type MemberType
		{
			get
			{
				return _memberType;
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
