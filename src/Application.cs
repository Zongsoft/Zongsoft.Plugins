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
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;

namespace Zongsoft.Plugins
{
	public static class Application
	{
		#region 事件声明
		public static event EventHandler Exiting;
		public static event EventHandler Starting;
		public static event EventHandler Started;
		#endregion

		#region 成员变量
		private static int _flags;
		private static PluginApplicationContext _context;
		#endregion

		#region 公共属性
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
			//保存当前上下文对象
			_context = context ?? throw new ArgumentNullException(nameof(context));

			//激发“Starting”事件
			OnStarting(args);

			#if !DEBUG
			try
			#endif
			{
				context.PluginContext.PluginTree.Loader.Loaded += delegate
				{
					//插件树加载完成即保存当前应用上下文
					_context = context;

					//将应用上下文对象挂载到插件结构中
					_context.PluginContext.PluginTree.Mount(_context.PluginContext.Settings.ApplicationContextPath, _context);

					//将应用上下文对象注册到默认服务容器中
					if(_context.Services != null)
						_context.Services.Register("ApplicationContext", _context);
				};

				//初始化全局模块
				InitializeGlobals(context);

				//加载插件树
				context.PluginContext.PluginTree.Load();

				//如果工作台对象不为空则运行工作台
				if(context.GetWorkbench(args) != null)
				{
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
			#if !DEBUG
			catch(Exception ex)
			{
				//应用无法启动，写入日志
				Zongsoft.Diagnostics.Logger.Fatal(ex);

				//重抛异常
				throw;
			}
			#endif
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

			//激发“Exiting”事件
			OnExiting(context);

			//关闭工作台
			if(context.Workbench != null)
				context.Workbench.Close();

			//卸载全局模块
			DisposeGlobals(context);

			//重置标记
			_flags = 0;

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

			if(Interlocked.CompareExchange(ref _flags, 1, 0) == 0)
			{
				//激发“Started”事件
				OnStarted(args);

				//激发应用上下文对象的“Started”事件
				context.RaiseStarted(args);
			}
		}

		private static void OnExiting(PluginApplicationContext context)
		{
			if(context == null)
				return;

			Exiting?.Invoke(context, EventArgs.Empty);

			//激发当前上下文的“Exiting”事件
			context.RaiseExiting();
		}

		private static void OnStarting(string[] args)
		{
			Starting?.Invoke(null, EventArgs.Empty);

			//激发当前上下文的“Starting”事件
			_context.RaiseStarting(args);
		}

		private static void OnStarted(string[] args)
		{
			Started?.Invoke(null, EventArgs.Empty);

			//激发当前上下文的“Started”事件
			_context.RaiseStarted(args);
		}
		#endregion

		#region 调用模块
		private static void InitializeGlobals(PluginApplicationContext context)
		{
			if(context == null)
				return;

			var configuration = context.Configuration;

			if(configuration != null)
			{
				var filters = configuration.GetOptionValue("/filters") as Zongsoft.Options.Configuration.FilterElementCollection;

				if(filters != null && filters.Count > 0)
				{
					foreach(var filter in filters)
					{
						context.Filters.Add(CreateInitializer((Options.Configuration.FilterElement)filter));
					}
				}
			}

			foreach(var filter in context.Filters)
			{
				if(filter != null)
					filter.Initialize(context);
			}
		}

		private static void DisposeGlobals(PluginApplicationContext context)
		{
			if(context == null)
				return;

			foreach(var module in context.Modules)
			{
				if(module != null && module is IDisposable)
					((IDisposable)module).Dispose();
			}
		}

		private static Zongsoft.Services.IApplicationFilter CreateInitializer(Options.Configuration.FilterElement element)
		{
			if(string.IsNullOrWhiteSpace(element.Type))
				throw new Options.Configuration.OptionConfigurationException("The application-filter type is empty or unspecified.");

			var type = System.Type.GetType(element.Type, false);

			if(type == null)
				throw new Options.Configuration.OptionConfigurationException($"Invalid '{element.Type}' type of application-filter, becase cann't load it.");

			if(!typeof(Zongsoft.Services.IApplicationFilter).IsAssignableFrom(type))
				throw new Options.Configuration.OptionConfigurationException($"Invalid '{element.Type}' type of application-filter, it doesn't implemented {nameof(Services.IApplicationFilter)} interface.");

			return Activator.CreateInstance(type) as Zongsoft.Services.IApplicationFilter;
		}
		#endregion
	}
}
