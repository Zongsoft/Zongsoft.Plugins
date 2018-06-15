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

using Zongsoft.Plugins;
using Zongsoft.Plugins.Builders;

namespace Zongsoft.Options.Plugins.Builders
{
	public class OptionBuilder : Zongsoft.Plugins.Builders.BuilderBase
	{
		#region 重写方法
		public override object Build(BuilderContext context)
		{
			Builtin builtin = context.Builtin;
			IOptionProvider provider = null;
			string providerValue = builtin.Properties.GetRawValue("provider");

			var node = new OptionNode(builtin.Name,
									  builtin.Properties.GetValue<string>("title"),
									  builtin.Properties.GetValue<string>("description"));

			if(string.IsNullOrWhiteSpace(providerValue))
				return node;

			switch(providerValue.Trim().ToLower())
			{
				case ".":
				case "plugin":
					provider = OptionUtility.GetConfiguration(builtin.Plugin);
					break;
				case "/":
				case "application":
					provider = context.PluginContext.ApplicationContext.Configuration;
					break;
				default:
					provider = builtin.Properties.GetValue<IOptionProvider>("provider");
					break;
			}

			if(provider == null)
				throw new PluginException(string.Format("Cann't obtain OptionProvider with '{0}'.", providerValue));

			node.Option = new Option(node, provider)
			{
				View = context.Builtin.Properties.GetValue<IOptionView>("view"),
				ViewBuilder = context.Builtin.Properties.GetValue<IOptionViewBuilder>("viewBuilder"),
			};

			return node;
		}

		protected override void OnBuildComplete(BuilderContext context)
		{
			var childNode = context.Result as OptionNode;

			if(childNode == null)
				return;

			var ownerNode = context.Owner as OptionNode;

			if(ownerNode == null)
				ownerNode = context.PluginContext.ApplicationContext.OptionManager.RootNode;

			if(ownerNode.Children.Contains(childNode.Name))
				ownerNode.Children[childNode.Name] = childNode;
			else
				ownerNode.Children.Add(childNode);
		}
		#endregion
	}
}
