/*
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
using System.Text.RegularExpressions;

using Zongsoft.Plugins;
using Zongsoft.Plugins.Parsers;

namespace Zongsoft.Services.Plugins.Parsers
{
	public class PredicateParser : Zongsoft.Plugins.Parsers.Parser
	{
		private readonly Regex _regex = new Regex(@"[^\s]+", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

		public override object Parse(ParserContext context)
		{
			if(string.IsNullOrWhiteSpace(context.Text))
				throw new PluginException("Can not parse for the predication because the parser text is empty.");

			var matches = _regex.Matches(context.Text);

			if(matches.Count < 1)
				throw new PluginException("Can not parse for the predication.");

			var parts = matches[0].Value.Split('.');

			if(parts.Length != 2)
				throw new PluginException("Can not parse for the predication because of a syntax error.");

			IPredication predication = null;

			if(parts.Length == 1)
				predication = context.PluginContext.ApplicationContext.ServiceFactory.Default.Resolve(parts[0]) as IPredication;
			else
			{
				var serviceProvider = context.PluginContext.ApplicationContext.ServiceFactory.GetProvider(parts[0]);

				if(serviceProvider == null)
					throw new PluginException(string.Format("The '{0}' ServiceProvider is not exists on the predication parsing.", parts[0]));

				predication = serviceProvider.Resolve(parts[1]) as IPredication;
			}

			if(predication != null)
			{
				string text = matches.Count <= 1 ? null : context.Text.Substring(matches[1].Index);
				object parameter = text;

				if(Zongsoft.Common.TypeExtension.IsAssignableFrom(typeof(IPredication<PluginPredicationContext>), predication.GetType()))
					parameter = new PluginPredicationContext(text, context.Builtin, context.Node, context.Plugin);

				return predication.Predicate(parameter);
			}

			return false;
		}
	}
}
