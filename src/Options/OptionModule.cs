/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2014-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Linq;

using Zongsoft.Plugins;

namespace Zongsoft.Options.Plugins
{
	public class OptionModule : Zongsoft.ComponentModel.IApplicationModule
	{
		#region 公共属性
		public string Name
		{
			get
			{
				return "OptionConfigurationModule";
			}
		}
		#endregion

		#region 初始化器
		public void Initialize(PluginApplicationContext context)
		{
			if(context == null)
				return;

			//将当前应用的主配置文件加入到选项管理器中
			if(context.Configuration != null)
				context.OptionManager.Providers.Add(context.Configuration);

			context.PluginContext.PluginTree.Loader.PluginLoaded += Loader_PluginLoaded;
			context.PluginContext.PluginTree.Loader.PluginUnloaded += Loader_PluginUnloaded;
		}

		void Zongsoft.ComponentModel.IApplicationModule.Initialize(Zongsoft.ComponentModel.ApplicationContextBase context)
		{
			this.Initialize(context as PluginApplicationContext);
		}
		#endregion

		#region 事件处理
		private void Loader_PluginLoaded(object sender, PluginLoadedEventArgs e)
		{
			if(OptionUtility.HasConfigurationFile(e.Plugin))
			{
				var proxy = new ConfigurationProxy(() => OptionUtility.GetConfiguration(e.Plugin));
				e.Plugin.Context.ApplicationContext.OptionManager.Providers.Add(proxy);
			}
		}

		private void Loader_PluginUnloaded(object sender, PluginUnloadedEventArgs e)
		{
			var providers = e.Plugin.Context.ApplicationContext.OptionManager.Providers;

			var found = providers.FirstOrDefault(provider =>
			{
				var proxy = provider as ConfigurationProxy;

				return (proxy != null && proxy.IsValueCreated &&
				        string.Equals(proxy.Value.FilePath, OptionUtility.GetConfigurationFilePath(e.Plugin)));
			});

			if(found != null)
				providers.Remove(found);
		}
		#endregion

		#region 嵌套子类
		private class ConfigurationProxyLoader : Configuration.OptionConfigurationLoader
		{
			#region 构造函数
			public ConfigurationProxyLoader(OptionNode root) : base(root)
			{
			}
			#endregion

			public override void Load(IOptionProvider provider)
			{
				var proxy = provider as ConfigurationProxy;

				if(proxy != null)
					base.LoadConfiguration(proxy.Value);
				else
					base.Load(provider);
			}

			public override void Unload(IOptionProvider provider)
			{
				var proxy = provider as ConfigurationProxy;

				if(proxy != null)
					base.UnloadConfiguration(proxy.Value);
				else
					base.Unload(provider);
			}
		}

		[OptionLoader(LoaderType = typeof(ConfigurationProxyLoader))]
		private class ConfigurationProxy : IOptionProvider
		{
			#region 成员字段
			private readonly Lazy<Configuration.OptionConfiguration> _proxy;
			#endregion

			#region 构造函数
			public ConfigurationProxy(Func<Configuration.OptionConfiguration> valueFactory)
			{
				if(valueFactory == null)
					throw new ArgumentNullException("valueFactory");

				_proxy = new Lazy<Configuration.OptionConfiguration>(valueFactory, true);
			}
			#endregion

			#region 公共属性
			public Configuration.OptionConfiguration Value
			{
				get
				{
					return _proxy.Value;
				}
			}

			public bool IsValueCreated
			{
				get
				{
					return _proxy.IsValueCreated;
				}
			}
			#endregion

			#region 公共方法
			public object GetOptionValue(string text)
			{
				var configuration = _proxy.Value;

				if(configuration != null)
					return configuration.GetOptionValue(text);

				return null;
			}

			public void SetOptionValue(string text, object value)
			{
				var configuration = _proxy.Value;

				if(configuration != null)
					configuration.SetOptionValue(text, value);
			}
			#endregion
		}
		#endregion
	}
}
