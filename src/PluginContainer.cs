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
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Zongsoft.Plugins
{
	internal class PluginContainer
	{
		private PluginContext _context;
		private readonly object _syncRoot;
		private readonly IDictionary<Plugin, AppDomain> _appDomains;

		internal PluginContainer(PluginContext context)
		{
			if(context == null)
				throw new ArgumentNullException("context");

			_context = context;
			_syncRoot = new object();
			_appDomains = new Dictionary<Plugin, AppDomain>();
		}

		public AppDomain GetDomain(Plugin plugin)
		{
			if(_context.IsolationLevel == IsolationLevel.None)
				return Thread.GetDomain();

			if(plugin == null)
				throw new ArgumentNullException("plugin");

			if(!_appDomains.ContainsKey(plugin))
			{
				lock(_syncRoot)
				{
					if(!_appDomains.ContainsKey(plugin))
					{
						AppDomainSetup setup = new AppDomainSetup();

						setup.ApplicationName = plugin.FilePath;
						setup.ApplicationBase = _context.Settings.ApplicationDirectory;
						setup.LoaderOptimization = LoaderOptimization.SingleDomain;
						setup.PrivateBinPath = GetPrivatePath(plugin.FilePath, setup.ApplicationBase);
						setup.ConfigurationFile = System.IO.Path.Combine(Path.GetDirectoryName(plugin.FilePath), "Plugin.config");

						_appDomains[plugin] = AppDomain.CreateDomain(plugin.FilePath, null, setup);
						_appDomains[plugin].SetData("Plugin", plugin);

						_appDomains[plugin].DomainUnload += delegate
						{
							_appDomains[plugin] = null;
							_appDomains.Remove(plugin);
						};
					}
				}
			}

			return _appDomains[plugin];
		}

		public void Run(Plugin plugin, Action callback)
		{
			if(plugin == null)
				throw new ArgumentNullException("plugin");

			if(callback == null)
				throw new ArgumentNullException("callback");

			GetDomain(plugin).DoCallBack(() => callback());
		}

		internal static void SetPrivatePath(string pluginFile, AppDomain appDomain)
		{
			string[] parts;
			appDomain = appDomain ?? AppDomain.CurrentDomain;

			if(string.IsNullOrEmpty(appDomain.SetupInformation.PrivateBinPath))
				parts = new string[0];
			else
				parts = appDomain.SetupInformation.PrivateBinPath.Split(';');

			string privatePath = GetPrivatePath(pluginFile, appDomain.SetupInformation.ApplicationBase, path =>
			{
				return !Array.Exists(parts, item => string.Equals(path, item, StringComparison.OrdinalIgnoreCase));
			});

			appDomain.AppendPrivatePath(privatePath);
		}

		internal static string GetPrivatePath(string pluginFile, string applicationDirectory)
		{
			return GetPrivatePath(pluginFile, applicationDirectory, null);
		}

		internal static string GetPrivatePath(string pluginFile, string applicationDirectory, Predicate<string> validation)
		{
			if(string.IsNullOrEmpty(pluginFile))
				return string.Empty;

			string path = Path.GetDirectoryName(pluginFile);

			if(path.StartsWith(applicationDirectory, StringComparison.OrdinalIgnoreCase))
				path = path.Substring(applicationDirectory.Length).Trim(Path.DirectorySeparatorChar);
			else
				return string.Empty;

			string[] parts = path.Split(Path.DirectorySeparatorChar);
			StringBuilder result = new StringBuilder();

			for(int i = parts.Length; i > 0; i--)
			{
				bool validated = true;
				path = string.Join(Path.DirectorySeparatorChar.ToString(), parts, 0, i);

				if(validation != null)
					validated = validation(path);

				if(validated)
					result.Append(path + ";");
			}

			return result.ToString().TrimEnd(';');
		}
	}
}
