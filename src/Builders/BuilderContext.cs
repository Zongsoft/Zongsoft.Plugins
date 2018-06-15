/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2010-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
	public class BuilderContext : MarshalByRefObject
	{
		#region 同步字段
		private readonly object _syncRoot;
		#endregion

		#region 成员字段
		private IBuilder _builder;
		private IAppender _appender;
		private bool _cancel;
		private Builtin _builtin;
		private object _parameter;
		private object _result;
		private object _value;
		private object _owner;
		private PluginTreeNode _ownerNode;
		#endregion

		#region 构造函数
		private BuilderContext(IBuilder builder, Builtin builtin, object parameter, object owner, PluginTreeNode ownerNode)
		{
			_builder = builder ?? throw new ArgumentNullException(nameof(builder));
			_builtin = builtin ?? throw new ArgumentNullException(nameof(builtin));
			_appender = builder as IAppender;

			_parameter = parameter;
			_owner = owner;
			_ownerNode = ownerNode;

			if(builtin.HasBehaviors)
				_cancel = builtin.Behaviors.GetBehaviorValue<bool>(builtin.BuilderName + ".break");

			_syncRoot = new object();
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前的插件上下文对象。
		/// </summary>
		public PluginContext PluginContext
		{
			get
			{
				return _builtin.Context;
			}
		}

		/// <summary>
		/// 获取当前插件上下文中的插件树。
		/// </summary>
		/// <remarks>
		///		该属性返回值完全等同于<see cref="PluginContext"/>属性返回的<seealso cref="Zongsoft.Plugins.PluginTree"/>对象。
		/// </remarks>
		public PluginTree PluginTree
		{
			get
			{
				var pluginContext = this.PluginContext;
				return pluginContext == null ? null : pluginContext.PluginTree;
			}
		}

		/// <summary>
		/// 获取当前的构建器对象。
		/// </summary>
		public IBuilder Builder
		{
			get
			{
				return _builder;
			}
		}

		/// <summary>
		/// 获取或设置构建过程的追加器。
		/// </summary>
		/// <remarks>
		///		<para>注意：该属性可能会被构建过程设置为空(null)，以阻止后续的追加动作。</para>
		/// </remarks>
		public IAppender Appender
		{
			get
			{
				return _appender;
			}
			set
			{
				_appender = value;
			}
		}

		/// <summary>
		/// 获取或设置参数对象。
		/// </summary>
		/// <remarks>
		///		<para>系统对属性不做约定，该属性值取决于特定调用者也可能在中途被其他构建器修改，因此建议使用者尽量不要过于依赖该值。</para>
		///		<para>对构建器实现者的建议：默认的构建动作内部会依次从上至下调用所有子级构件对应的构建动作，在这些层级调用过程中获取该属性值可能只对特定级别的构建器有意义，因此构建器的实现者需要与调用者进行约定，而在某些场合这些约定是难以确保的，因此请谨慎对待该属性值！</para>
		/// </remarks>
		public object Parameter
		{
			get
			{
				return _parameter;
			}
			set
			{
				_parameter = value;
			}
		}

		/// <summary>
		/// 获取或设置是否取消后续构建。
		/// </summary>
		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		/// <summary>
		/// 获取当前构建器要操作的构件。
		/// </summary>
		public Builtin Builtin
		{
			get
			{
				return _builtin;
			}
		}

		/// <summary>
		/// 获取当前构建器需要操作的插件节点，即为<see cref="Builtin"/>属性所指定的构件所属的<see cref="PluginTreeNode"/>插件树节点。
		/// </summary>
		public PluginTreeNode Node
		{
			get
			{
				return _builtin.Node;
			}
		}

		/// <summary>
		/// 获取当前节点的所有者对象，即所有者节点对应的目标对象。
		/// </summary>
		/// <remarks>
		///		<para>获取该属性值不会激发对所有者节点的创建动作，以避免在构建过程中发生无限递归调用。</para>
		/// </remarks>
		public object Owner
		{
			get
			{
				if(_owner == null)
				{
					var ownerNode = this.OwnerNode;

					//注意：解析所有者节点的目标对象。该操作绝不能激发创建动作，不然将可能导致无限递归调用。
					if(ownerNode != null)
						_owner = ownerNode.UnwrapValue(ObtainMode.Never, _parameter, null);
				}

				return _owner;
			}
		}

		/// <summary>
		/// 获取当前节点的所有者节点。
		/// </summary>
		public PluginTreeNode OwnerNode
		{
			get
			{
				if(_ownerNode == null)
				{
					lock(_syncRoot)
					{
						if(_ownerNode == null)
						{
							var node = this.Node;
							var tree = this.PluginTree;

							if(tree == null)
								return null;

							if(node == null)
								_ownerNode = tree.GetOwnerNode(_builtin.FullPath);
							else
								_ownerNode = tree.GetOwnerNode(node);
						}
					}
				}

				return _ownerNode;
			}
		}

		/// <summary>
		/// 获取或设置由构建器创建的目标对象。
		/// </summary>
		/// <remarks>
		///		<para>该属性返回值会被添加到<see cref="Owner"/>对象的子集中。</para>
		/// </remarks>
		public object Result
		{
			get
			{
				return _result;
			}
			set
			{
				_result = value;
			}
		}

		/// <summary>
		/// 获取或设置由构建器返回的需要保存到对应<see cref="Builtin"/>构件中对象中的<seealso cref="Zongsoft.Plugins.Builtin.Value"/>属性。
		/// </summary>
		/// <remarks>
		///		<para>该属性返回值会被作为对应<see cref="Builtin"/>的子构件对应目标对象的所有者(即上级对象)。</para>
		///		<para>如果构建器不需要将创建的目标对象保存到<see cref="Builtin"/>构件对象，则可以将该属性值设置为空(null)。</para>
		/// </remarks>
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}
		#endregion

		#region 创建方法
		internal static BuilderContext CreateContext(IBuilder builder, Builtin builtin, object parameter, object owner = null, PluginTreeNode ownerNode = null)
		{
			return new BuilderContext(builder, builtin, parameter, owner, ownerNode);
		}
		#endregion
	}
}
