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
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;

using Zongsoft.ComponentModel;

namespace Zongsoft.Plugins
{
	public static class Application
	{
		#region 事件声明
		public static event EventHandler<ApplicationEventArgs> Starting;
		public static event EventHandler<ApplicationEventArgs> Started;
		public static event CancelEventHandler Exiting;
		#endregion

		#region 成员变量
		private static int _isStarted;
		private static PluginApplicationContext _context;
		#endregion

		#region 公共属性
		public static bool IsStarted
		{
			get
			{
				return _isStarted != 0;
			}
		}

		public static PluginApplicationContext Context
		{
			get
			{
				return _context;
			}
		}
		#endregion

		#region 启动应用
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
		public static void Start(PluginApplicationContext context, params string[] args)
		{
			if(context == null)
				throw new ArgumentNullException("context");

			if(_isStarted != 0)
				return;

			//激发“Starting”事件
			OnStarting(context);

			//激发应用上下文对象的“Starting”事件
			context.RaiseStarting(args);

			try
			{
				context.PluginContext.PluginTree.Loader.Loaded += delegate
				{
					//插件树加载完成即保存当前应用上下文
					_context = context;

					//将应用上下文对象挂载到插件结构中
					_context.PluginContext.PluginTree.Mount(_context.PluginContext.Settings.ApplicationContextPath, _context);

					//将应用上下文对象注册到默认服务容器中
					if(_context.ServiceFactory != null && _context.ServiceFactory.Default != null)
						_context.ServiceFactory.Default.Register("ApplicationContext", _context);
				};

				//初始化全局模块
				InitializeGlobalModules(context);

				//加载插件树
				context.PluginContext.PluginTree.Load();

				//初始化插件模块
				InitializePluginModules(context, context.PluginContext.PluginTree.Plugins);

				//激发应用上下文对象的“Initializing”事件
				context.RaiseInitializing(args);

				//如果工作台对象不为空则运行工作台
				if(context.GetWorkbench(args) != null)
				{
					//激发应用上下文对象的“Initialized”事件
					context.RaiseInitialized(args);

					//注意：因为工作台很可能会阻塞当前主线程，所以需要利用其Opened事件进行注册
					context.Workbench.Opened += delegate
					{
						//激发应用启动完成事件
						RaiseStarted(args);
					};

					context.Workbench.Closed += delegate
					{
						Exit();
					};

					//启动工作台
					context.Workbench.Open(args);
				}

				//激发应用启动完成事件
				RaiseStarted(args);
			}
			catch(Exception ex)
			{
				//应用无法启动，写入日志
				Zongsoft.Diagnostics.Logger.Fatal(ex);

				//重抛异常
				throw;
			}
		}
		#endregion

		#region 关闭应用
		/// <summary>
		/// 关闭当前应用程序。
		/// </summary>
		public static void Exit()
		{
			var context = System.Threading.Interlocked.Exchange(ref _context, null);

			//如果上下文对象为空，则表示尚未启动
			if(context == null)
				return;

			//重置启动标记
			_isStarted = 0;

			//创建取消事件参数
			CancelEventArgs args = new CancelEventArgs();

			//激发“Exiting”事件
			OnExiting(args);

			//激发应用上下文对象的“Exiting”事件
			context.RaiseExiting(args);

			//判断是否取消退出，如果是则退出
			if(args.Cancel)
				return;

			//关闭工作台
			if(context.Workbench != null)
				context.Workbench.Close();

			//卸载插件模块
			DisposePluginModules(context);

			//卸载全局模块
			DisposeGlobalModules(context);

			//将当前应用上下文对象从列表中删除
			_context = null;
		}
		#endregion

		#region 激发事件
		private static void RaiseStarted(string[] args)
		{
			var context = _context;

			if(context == null)
				return;

			if(Interlocked.CompareExchange(ref _isStarted, 1, 0) == 0)
			{
				//激发“Started”事件
				OnStarted(context);

				//激发应用上下文对象的“Started”事件
				context.RaiseStarted(args);
			}
		}

		private static void OnExiting(CancelEventArgs args)
		{
			var handler = Exiting;

			if(handler != null)
				handler(null, args);
		}

		private static void OnStarting(PluginApplicationContext context)
		{
			var handler = Starting;

			if(handler != null)
				handler(null, new ApplicationEventArgs(context));
		}

		private static void OnStarted(PluginApplicationContext context)
		{
			var handler = Started;

			if(handler != null)
				handler(null, new ApplicationEventArgs(context));
		}
		#endregion

		#region 调用模块
		private static void InitializeGlobalModules(PluginApplicationContext context)
		{
			if(context == null)
				return;

			var configuration = context.Configuration;

			if(configuration != null)
			{
				var modules = configuration.GetOptionValue("/modules") as Zongsoft.Options.Configuration.ModuleElementCollection;

				if(modules != null && modules.Count > 0)
				{
					foreach(Zongsoft.Options.Configuration.ModuleElement module in modules)
					{
						context.Modules.Add(module.CreateModule());
					}
				}
			}

			foreach(var module in context.Modules)
			{
				if(module != null)
					module.Initialize(context);
			}
		}

		private static void DisposeGlobalModules(PluginApplicationContext context)
		{
			if(context == null)
				return;

			foreach(var module in context.Modules)
			{
				if(module != null && module is IDisposable)
					((IDisposable)module).Dispose();
			}
		}

		private static void InitializePluginModules(PluginApplicationContext context, IEnumerable<Plugin> plugins)
		{
			if(context == null || plugins == null)
				return;

			foreach(Plugin plugin in plugins)
			{
				if(plugin.Status != PluginStatus.Loaded)
					continue;

				foreach(FixedElement<IApplicationModule> module in plugin.Modules)
				{
					if(module.Value != null)
						module.Value.Initialize(context);
				}

				if(plugin.Children.Count > 0)
					InitializePluginModules(context, plugin.Children);
			}
		}

		private static void DisposePluginModules(PluginApplicationContext context)
		{
			if(context == null)
				return;

			foreach(Plugin plugin in context.PluginContext.PluginTree.Plugins)
			{
				if(plugin.Status != PluginStatus.Loaded)
					continue;

				foreach(FixedElement<IApplicationModule> module in plugin.Modules)
				{
					if(module.HasValue)
					{
						var disposable = module.Value as IDisposable;

						if(disposable != null)
							disposable.Dispose();
					}
				}
			}
		}
		#endregion
	}
}
