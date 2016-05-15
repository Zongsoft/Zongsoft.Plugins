﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2010-2014 Zongsoft Corporation <http://www.zongsoft.com>
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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
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

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Options.Plugins
{
	internal static class PluginSettingsProviderFactory
	{
		#region 私有变量
		private static readonly Zongsoft.Collections.ObjectCache<PluginSettingsProvider> _cache;
		#endregion

		#region 静态构造
		static PluginSettingsProviderFactory()
		{
			_cache = new Collections.ObjectCache<PluginSettingsProvider>(0);
		}
		#endregion

		#region 公共方法
		public static PluginSettingsProvider GetProvider(Zongsoft.Plugins.Plugin plugin)
		{
			if(plugin == null)
				throw new ArgumentNullException("plugin");

			var configuration = OptionUtility.GetConfiguration(plugin);

			if(configuration == null)
				return null;

			return _cache.Get(plugin.FilePath, key =>
			{
				return new PluginSettingsProvider(plugin, configuration);
			});
		}
		#endregion
	}
}
