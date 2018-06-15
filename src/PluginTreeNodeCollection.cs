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
using System.Collections.Generic;

namespace Zongsoft.Plugins
{
	public class PluginTreeNodeCollection : PluginElementCollection<PluginTreeNode>
	{
		private PluginTreeNode _owner;

		#region 构造函数
		public PluginTreeNodeCollection(PluginTreeNode owner)
		{
			_owner = owner;
		}
		#endregion

		#region 公共属性
		public PluginTreeNode this[int index]
		{
			get
			{
				return this.Get(index);
			}
		}

		public PluginTreeNode this[string name]
		{
			get
			{
				return this.Get(name);
			}
		}
		#endregion

		#region 重写方法
		//protected override void SetElementOwner(PluginTreeNode element, PluginTreeNode owner)
		//{
		//	if(element != null)
		//		element.Parent = owner;
		//}

		protected override void OnSetComplete(PluginTreeNode oldValue, PluginTreeNode newValue)
		{
			oldValue.Parent = null;
			newValue.Parent = null;

			base.OnSetComplete(oldValue, newValue);
		}

		protected override void OnInsertComplete(PluginTreeNode value, int index)
		{
			value.Parent = _owner;
			base.OnInsertComplete(value, index);
		}

		protected override void OnRemoveComplete(PluginTreeNode value)
		{
			value.Parent = null;
			base.OnRemoveComplete(value);
		}

		protected override bool ValidateElement(PluginTreeNode element)
		{
			if(element == null)
				throw new ArgumentNullException("element");

			if(element.Name.Contains("/"))
				return false;

			if(element.Parent == null)
				return true;

			return element.Parent == _owner;
		}
		#endregion

		#region 内部方法
		internal void Add(PluginTreeNode node)
		{
			if(node == null)
				throw new ArgumentNullException("node");

			if(node.NodeType == PluginTreeNodeType.Builtin)
				this.Insert(node, ((Builtin)node.Value).Position);
			else
				this.Insert(node, -1);
		}

		internal void Clear()
		{
			this.BaseClear();
		}

		internal void Remove(PluginTreeNode node)
		{
			this.BaseRemove(node);
		}

		internal void Remove(string name)
		{
			this.BaseRemoveKey(name);
		}
		#endregion
	}
}
