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

using Zongsoft.Plugins;
using Zongsoft.Plugins.Builders;

namespace Zongsoft.Services.Plugins.Builders
{
	[BuilderBehaviour(typeof(PluginServiceProvider))]
	public class ServiceProviderBuilder : Zongsoft.Plugins.Builders.BuilderBase
	{
		#region 重写方法
		public override object Build(BuilderContext context)
		{
			//阻止对子节点的构建
			context.Cancel = true;

			string providerPath = context.Builtin.Properties.GetRawValue("path");

			if(string.IsNullOrWhiteSpace(providerPath))
				providerPath = "/Workspace/Services/" + context.Builtin.Name;
			else if(providerPath == ".")
				providerPath = context.Builtin.FullPath;

			return new PluginServiceProvider(context.PluginContext, providerPath);
		}

		protected override void OnBuilt(BuilderContext context)
		{
			var serviceFactory = context.Owner as ServiceProviderFactory;

			if(serviceFactory != null)
				serviceFactory.Register(context.Builtin.Name, (IServiceProvider)context.Result);
			else
				base.OnBuilt(context);
		}
		#endregion
	}
}
