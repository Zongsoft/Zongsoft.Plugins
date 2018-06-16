/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2016 Zongsoft Corporation <http://www.zongsoft.com>
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

namespace Zongsoft.Plugins.Builders
{
	/// <summary>
	/// 构件链接创建器。
	/// </summary>
	/// <remarks>
	///		<para>该构建器区别于<seealso cref="ObjectBuilder"/>的主要特征在于它不始终不会激发对子节点的构建。</para>
	/// </remarks>
	[Obsolete]
	public class LinkBuilder : ObjectBuilder
	{
		public override Type GetValueType(Builtin builtin)
		{
			if(!builtin.Properties.TryGet("ref", out var property))
				throw new PluginException(string.Format("Missing 'ref' property in '{0}' builtin.", builtin));

			var refNode = builtin.Node.Find(property.RawValue);
			return refNode == null ? null : refNode.ValueType;
		}

		public override object Build(BuilderContext context)
		{
			if(!context.Builtin.Properties.TryGet("ref", out var property))
				throw new PluginException(string.Format("Missing 'ref' property in '{0}' builtin.", context.Builtin));

			//阻止构建下级节点
			context.Cancel = true;

			return context.PluginContext.ResolvePath(property.RawValue, context.Node, ObtainMode.Auto);
		}
	}
}
