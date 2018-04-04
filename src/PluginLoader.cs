﻿/*
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
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Zongsoft.ComponentModel;

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 关于插件加载的功能。
	/// </summary>
	/// <remarks>
	///		<para>插件加载器根据一系列策略进行插件加载，可以通过<seealso cref="Zongsoft.Plugins.PluginLoader.Plugins"/>或<seealso cref="Zongsoft.Plugins.PluginTree.Plugins"/>属性获取加载成功的所有根插件集。</para>
	///		<para>关于插件加载中的相关定义如下：</para>
	///		<list type="table">
	///			<item>
	///				<term>插件根目录</term>
	///				<description>由<seealso cref="Zongsoft.Plugins.PluginSetupBase.PluginsDirectory"/>指定的完全限定路径，默认为当前应用程序目录下的plugins文件夹。</description>
	///			</item>
	///			<item>
	///				<term>插件目录</term>
	///				<description>
	///					<para>位于插件根目录下的子目录或者插件根目录均称为插件目录。不是所有插件根目录下的子目录都是插件子目录，必须包含插件定义文件(*.plugin)的子目录才是插件子目录。</para>
	///				</description>
	///			</item>
	///			<item>
	///				<term>父子插件</term>
	///				<description>插件的父子关系只依赖于插件目录的层次关系，处于上级插件目录中的插件为其下级插件目录中各插件的父插件，一个插件可以有零或多个子插件但是只能有零或一个父插件。有关父子插件的加载策略与关系确定条件请参考后面的说明。</description>
	///			</item>
	///			<item>
	///				<term>插件依赖项</term>
	///				<description>插件依赖项在插件定义文件(*.plugin)中通过&lt;dependencies&gt;节点进行声明。它表示在同级插件中的加载次序，父插件总是在子插件之前加载完成，如果父插件加载失败，系统不会去尝试加载它的子插件。</description>
	///			</item>
	///			<item>
	///				<term>主插件</term>
	///				<description>主插件是相对于插件目录而言的。在插件目录中如果只有一个插件定义文件(*.plugin)，那么这个插件定义文件对应的插件即为该插件目录的主插件；如果目录中有多个插件定义文件，则其中没有任何依赖项的即为主插件，因此目录中主插件可能有多个。</description>
	///			</item>
	///		</list>
	///		
	///		<para>插件的父子关系确定涉及下列步骤：</para>
	///		<list type="number">
	///			<item>
	///				<description>在当前插件目录下如果有子文件夹，则启动子插件的搜索。</description>
	///			</item>
	///			<item>
	///				<description>如果插件目录下有子插件，则这些子插件的父插件为上级插件目录的主插件；当上级插件目录有多个主插件，则他们之间没有从属关系，即为平级关系，并以此类推。</description>
	///			</item>
	///		</list>
	///		
	///		<para>在插件树的加载过程中，插件解析或加载过程出现的异常不会导致整个加载过程的中断，可以通过挂接<seealso cref="Failure"/>事件来处理其失败通知，失败的插件不会出现在跟插件集合或对应的父插件子集中。加载器的加载步骤如下：</para>
	///		<list type="number">
	///			<item>
	///				<description>从插件根目录中以插件文件名排序依次预加载插件，预加载成功的根插件进入跟插件集合中。</description>
	///			</item>
	///			<item>
	///				<description>如果预加载插件成功的插件是主插件，则完整的加载它。</description>
	///			</item>
	///			<item>
	///				<description>依次递归预加载子插件目录中的各主插件文件。</description>
	///			</item>
	///			<item>
	///				<description>在系统中所有主插件加载完毕后，则从上向下按级加载从插件。</description>
	///			</item>
	///			<item>
	///				<description>如果从插件集中不能有加载的依赖项或者有循环引用的情况，则这些从插件的状态被置为失败，并从上级插件树列表移除，然后激发<see cref="Failrue"/>事件。</description>
	///			</item>
	///			<item>
	///				<description>依次递归预加载子插件目录中的各从插件。</description>
	///			</item>
	///		</list>
	/// </remarks>
	public class PluginLoader : MarshalByRefObject
	{
		#region 事件定义
		/// <summary>表示所有插件加载完成。</summary>
		/// <remarks>该事件由Load方法激发。只要Load被执行，该事件总会被激发，无论加载过程是否异常。</remarks>
		public event EventHandler<PluginLoadEventArgs> Loaded;

		/// <summary>表示开始进行整体插件加载。</summary>
		/// <remarks>该事件由Load方法激发，只要Load被执行，该事件总是第一个被激发。</remarks>
		public event EventHandler<PluginLoadEventArgs> Loading;

		/// <summary>表示单个插件加载完成。</summary>
		/// <remarks>该事件由Load方法激发，在对应的<seealso cref="Zongsoft.Plugins.PluginLoader.PluginLoading"/>事件被激发后，该事件不一定总会得到激发，因为加载过程可能出现异常。在<seealso cref="Zongsoft.Plugins.PluginLoadedEventArgs"/>事件参数中通过Plugin属性获取到成功加载的插件对象。</remarks>
		public event EventHandler<PluginLoadedEventArgs> PluginLoaded;

		/// <summary>表示单个插件开始加载。</summary>
		/// <remarks>该事件由Load方法激发，在每次加载到相应的插件会得到激发。</remarks>
		public event EventHandler<PluginLoadingEventArgs> PluginLoading;

		/// <summary>表示单个插件卸载完成。</summary>
		/// <remarks>该事件由Unload方法激发，在对应的<seealso cref="Zongsoft.Plugins.PluginLoader.PluginUnloading"/>事件被激发后，该事件不一定总会得到激发，因为卸载过程可能出现异常。在<seealso cref="Zongsoft.Plugins.PluginUnloadedEventArgs"/>事件参数中通过Plugin属性获取到成功卸载的插件对象。</remarks>
		public event EventHandler<PluginUnloadedEventArgs> PluginUnloaded;

		/// <summary>表示单个插件开始卸载。</summary>
		/// <remarks>该事件由Unload方法激发，在每次卸载指定的插件会得到激发。</remarks>
		public event EventHandler<PluginUnloadingEventArgs> PluginUnloading;
		#endregion

		#region 成员变量
		private PluginCollection _plugins;
		private PluginResolver _resolver;
		private PluginLoaderSetup _settings;
		#endregion

		#region 构造函数
		internal PluginLoader(PluginResolver resolver) : this(resolver, null)
		{
		}

		internal PluginLoader(PluginResolver resolver, PluginLoaderSetup setup)
		{
			if(resolver == null)
				throw new ArgumentNullException("resolver");

			_resolver = resolver;
			_settings = setup;
			_plugins = new PluginCollection();
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取加载的根插件对象集。
		/// </summary>
		/// <remarks>如果加载器尚未加载过，则返回一个空集。</remarks>
		public PluginCollection Plugins
		{
			get
			{
				return _plugins;
			}
		}

		/// <summary>
		/// 获取加载器的当前加载配置对象。该属性值可通过带参的<seealso cref="Zongsoft.Plugins.PluginLoader.Load(PluginLoaderSetup)"/>方法注入。
		/// </summary>
		public PluginLoaderSetup Settings
		{
			get
			{
				return _settings;
			}
		}
		#endregion

		#region 公共方法
		/// <summary>
		/// 使用加载器的当前配置进行插件加载。
		/// </summary>
		/// <remarks>
		///		<para>在调用该方法前，必须确保当前<seealso cref="Zongsoft.Plugins.PluginLoader.Settings"/>属性已被初始化，否则请使用带<seealso cref="Zongsoft.Plugins.PluginLoaderSetup"/>类型参数的Load方法重载版本。</para>
		///		<para>通过<seealso cref="Zongsoft.Plugins.PluginTree.Loader"/>属性获取的加载器对象默认已经通过<seealso cref="Zongsoft.Plugins.PluginContext"/>上下文对象的Settings属性初始化过加载器的<see cref="PluginLoaderSetup"/>配置对象。</para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">当前Settings属性没有设置，即其值为空(null)。</exception>
		public void Load()
		{
			if(_settings == null)
				throw new InvalidOperationException("The Settings property is not set.");

			this.Load(_settings);
		}

		/// <summary>
		/// 应用指定的加载配置进行插件加载。
		/// </summary>
		/// <param name="settings">指定的加载配置对象。</param>
		/// <remarks>
		///		<para>使用不同的<see cref="Zongsoft.Plugins.PluginLoaderSetup"/>设置项多次加载，会导致最后一次加载覆盖上次加载的插件结构，这有可能会影响您的插件应用对构件或服务的获取路径，从而导致不可预知的结果。</para>
		///		<para>如果要重用上次加载的配置，请调用无参的Load方法。</para>
		///	</remarks>
		/// <exception cref="System.ArgumentNullException">参数<paramref name="settings"/>为空(null)。</exception>
		public void Load(PluginLoaderSetup settings)
		{
			if(settings == null)
				throw new ArgumentNullException("settings");

			//激发“Loading”事件
			this.OnLoading(new PluginLoadEventArgs(settings));

			//如果指定的目录路径不存在则激发“Failure”事件，并退出
			if(!Directory.Exists(_settings.PluginsDirectory))
				throw new DirectoryNotFoundException(string.Format("The '{0}' plugins directory is not exists.", _settings.PluginsDirectory));

			//清空插件列表
			_plugins.Clear();

			//预加载插件目录下的所有插件文件
			this.PreloadPluginFiles(_settings.PluginsDirectory, null, settings);

			//正式加载所有插件
			this.LoadPlugins(_plugins, settings);

			//保存加载配置对象
			_settings = settings;

			//激发“Loaded”事件
			this.OnLoaded(new PluginLoadEventArgs(settings));
		}

		/// <summary>
		/// 卸载所有插件。
		/// </summary>
		public void Unload()
		{
			if(_plugins == null || _plugins.Count < 1)
				return;

			var plugins = new Plugin[_plugins.Count];
			_plugins.CopyTo(plugins, 0);

			foreach(var plugin in plugins)
			{
				this.Unload(plugin);
			}
		}

		/// <summary>
		/// 卸载指定的插件。
		/// </summary>
		/// <param name="plugin">指定要卸载的插件。</param>
		/// <remarks>
		///		<para>如果指定的插件状态不是已经加载的（即插件对象的Status属性值不等于<seealso cref="Zongsoft.Plugins.PluginStatus.Loaded"/>），则不能对其进行卸载。</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">当<paramref name="plugin"/>参数为空(null)。</exception>
		public void Unload(Plugin plugin)
		{
			if(plugin == null || plugin.Status != PluginStatus.Loaded)
				return;

			//激发“PluginUnloading”事件，如果事件处理程序取消后续执行则返回
			if(this.OnPluginUnloading(plugin))
				return;

			//设置插件状态
			plugin.Status = PluginStatus.Unloading;

			//递推卸载子插件
			foreach(Plugin child in plugin.Children)
			{
				this.Unload(child);
				plugin.Children.Remove(child.Name);
			}

			//获取指定插件的直隶从属插件集
			IEnumerable<Plugin> slaves = plugin.GetSlaves(false);

			//递归卸载附属插件
			foreach(Plugin slave in slaves)
			{
				this.Unload(slave);
				slave.Manifest.Dependencies.Remove(plugin);
			}

			//将指定插件中的所有构件依次从插件树中卸载掉，因为UnmountBuiltin方法会改变PluginTree对象的内部构件列表，所以不能使用foreach而必须使用while进行遍历
			//注意：对构件的卸载必须在卸载构建器之前，因为卸载构件需要使用到对应构建器的Destroy方法。
			while(plugin.InnerBuiltins.Count > 0)
				_resolver.PluginTree.Unmount(plugin.InnerBuiltins[0]);

			//卸载当前插件下的所有固定元素(构建器、解析器、模块)
			this.UnloadFixedElements(plugin);

			//将指定卸载的插件从当前根插件列表中删除
			if(_plugins != null && plugin.Parent == null)
				_plugins.Remove(plugin.Name);

			//设置插件状态
			plugin.Status = PluginStatus.Unloaded;

			//激发“PluginUnloaded”事件
			this.OnPluginUnloaded(new PluginUnloadedEventArgs(plugin));
		}
		#endregion

		#region 私有方法
		private void UnloadFixedElements(Plugin plugin)
		{
			if(plugin == null)
				return;

			foreach(BuilderElement builderElement in plugin.Builders)
			{
				this.UnloadBuilder(builderElement);
			}

			foreach(FixedElement<IParser> element in plugin.Parsers)
			{
				if(element.HasValue)
				{
					IDisposable disposable = element.Value as IDisposable;
					if(disposable != null)
						disposable.Dispose();
				}
			}

			foreach(FixedElement<IApplicationModule> element in plugin.Modules)
			{
				if(element.HasValue)
				{
					IDisposable disposable = element.Value as IDisposable;
					if(disposable != null)
						disposable.Dispose();
				}
			}

			plugin.Builders.Clear();
			plugin.Parsers.Clear();
			plugin.Modules.Clear();
		}

		private void UnloadBuilder(BuilderElement builderElement)
		{
			if(builderElement == null)
				return;

			if(builderElement.HasValue)
				builderElement.Value.Dispose();
		}

		private void PreloadPluginFiles(string directoryPath, Plugin parent, PluginLoaderSetup settings)
		{
			//获取指定目录下的插件文件
			var filePaths = Directory.GetFiles(directoryPath, "*.plugin", SearchOption.TopDirectoryOnly);

			//如果当前目录下没有插件文件则查找子目录
			if(filePaths == null || filePaths.Length < 1)
			{
				this.PreloadPluginChildrenFiles(directoryPath, parent, settings);
				return;
			}

			//已经成功加载的主插件列表
			var masters = new List<Plugin>();

			//注意：取消了对返回的文件名数组进行排序(包含文件扩展名的排序是不准的)
			//Array.Sort<string>(filePaths, StringComparer.Ordinal);

			//依次加载所有插件文件
			foreach(string filePath in filePaths)
			{
				//首先加载插件文件的清单信息(根据清单信息中的依赖插件来决定是否需要完整加载)
				Plugin plugin = this.LoadPluginManifest(filePath, parent, settings);

				//如果预加载失败，则跳过以进行下一个插件文件的处理
				if(plugin == null || plugin.Status == PluginStatus.Failed)
					continue;

				//判断当前预加载的插件是否为主插件
				if(plugin.IsMaster)
				{
					//将当前已完整加载的插件加入主插件列表中
					masters.Add(plugin);
				}
			}

			//定义子插件的父插件，默认为当前插件目录的父插件
			Plugin ownerPlugin = parent;

			//如果当前插件目录下有主插件则所有子插件的父为第一个主插件
			if(masters.Count > 0)
				ownerPlugin = masters[0];

			//预加载子插件
			this.PreloadPluginChildrenFiles(directoryPath, ownerPlugin, settings);
		}

		private void PreloadPluginChildrenFiles(string directoryPath, Plugin parent, PluginLoaderSetup settings)
		{
			//获取当前插件目录的下级子目录
			string[] childDirectoriePaths = Directory.GetDirectories(directoryPath);

			//依次加载子目录下的所有插件
			foreach(string childDirectoryPath in childDirectoriePaths)
			{
				this.PreloadPluginFiles(childDirectoryPath, parent, settings);
			}
		}

		private Plugin LoadPluginManifest(string filePath, Plugin parent, PluginLoaderSetup settings)
		{
			//激发“PluginLoading”事件
			this.OnPluginLoading(new PluginLoadingEventArgs(settings, filePath));

			//解析插件清单
			Plugin plugin = _resolver.ResolvePluginOnlyManifest(filePath, parent);

			if(plugin == null)
				return null;

			//设置插件状态
			plugin.Status = PluginStatus.Loading;

			if(parent == null)
			{
				if(_plugins.Any(p => string.Equals(p.Name, plugin.Name, StringComparison.OrdinalIgnoreCase)))
				{
					plugin.Status = PluginStatus.Failed;
					plugin.StatusDescription = string.Format("The name is '{0}' of plugin was exists. it's path is: '{1}'", plugin.Name, plugin.FilePath);

					//抛出插件文件异常(原因为插件名称重复)
					throw new PluginFileException(plugin.FilePath, $"The name is '{plugin.Name}' of plugin was exists. it's path is: '{plugin.FilePath}'");
				}

				//将预加载的插件对象加入到根插件的集合中
				_plugins.Add(plugin);
			}
			else
			{
				//将预加载的插件对象加入到父插件的子集中，如果返回假则表示加载失败
				if(!parent.Children.Add(plugin, false))
				{
					plugin.Status = PluginStatus.Failed;
					plugin.StatusDescription = string.Format("The name is '{0}' of plugin was exists. it's path is: '{1}'", plugin.Name, plugin.FilePath);

					//抛出插件文件异常(原因为插件名称重复)
					throw new PluginFileException(plugin.FilePath, $"The name is '{plugin.Name}' of plugin was exists. it's path is: '{plugin.FilePath}'");
				}
			}

			return plugin;
		}

		private void LoadPluginContent(Plugin plugin, PluginLoaderSetup settings)
		{
			if(plugin == null)
				throw new ArgumentNullException("plugin");

			if(plugin.Status != PluginStatus.Loading)
				return;

			try
			{
				//解析插件对象
				_resolver.ResolvePluginWithoutManifest(plugin);

				//设置已加载插件对象所属应用域的私有路径（测试用代码）
				PluginContainer.SetPrivatePath(plugin.FilePath, AppDomain.CurrentDomain);

				//设置插件状态
				plugin.Status = PluginStatus.Loaded;

				//激发“PluginLoaded”事件
				this.OnPluginLoaded(new PluginLoadedEventArgs(settings, plugin));
			}
			catch(Exception ex)
			{
				plugin.Status = PluginStatus.Failed;
				plugin.StatusDescription = ex.Message;

				if(plugin.Parent == null)
					_plugins.Remove(plugin.Name);
				else
					plugin.Parent.Children.Remove(plugin.Name);

				throw new PluginFileException(plugin.FilePath, $"The '{plugin.FilePath}' plugin file resolve failed.", ex);
			}
		}

		private void LoadPlugins(PluginCollection plugins, PluginLoaderSetup settings)
		{
			if(plugins == null || plugins.Count < 1)
				return;

			var stack = new Stack<Plugin>();

			//注意：①. 先加载同级插件
			foreach(var plugin in plugins)
			{
				//确保同级插件栈内的所有插件一定都是未加载的插件
				if(plugin.Status == PluginStatus.Loading)
				{
					this.TryPushToStack(plugin, stack);
					this.LoadPlugin(stack, pluginName => plugins[pluginName], settings);
				}
			}

			//注意：②. 再依次加载各个子插件
			foreach(var plugin in plugins)
			{
				if(plugin.Status == PluginStatus.Loaded)
					this.LoadPlugins(plugin.Children, settings);
			}
		}

		private void LoadPlugin(Stack<Plugin> stack, Func<string, Plugin> dependencyThunk, PluginLoaderSetup settings)
		{
			if(stack == null || stack.Count < 1)
				return;

			var plugin = stack.Peek();

			if(this.IsRequiredLoad(plugin, dependencyThunk))
			{
				this.LoadPluginContent(stack.Pop(), settings);
			}
			else
			{
				foreach(var dependency in plugin.Manifest.Dependencies)
				{
					if(this.IsRequiredLoad(dependency.Plugin, dependencyThunk))
						this.LoadPluginContent(dependency.Plugin, settings);
					else
						this.TryPushToStack(dependency.Plugin, stack);
				}
			}

			if(stack.Count > 0)
				this.LoadPlugin(stack, dependencyThunk, settings);
		}

		private bool IsRequiredLoad(Plugin plugin, Func<string, Plugin> dependencyThunk)
		{
			//如果当前插件状态不是未加载状态则返回假，即表示该插件现在还不能立即加载
			if(plugin == null || plugin.Status != PluginStatus.Loading)
				return false;

			if(plugin.Manifest.HasDependencies)
			{
				foreach(var dependency in plugin.Manifest.Dependencies)
				{
					if(dependency.Plugin == null)
						dependency.Plugin = dependencyThunk(dependency.Name);

					if(dependency.Plugin == null)
					{
						plugin.Status = PluginStatus.Failed;
						plugin.StatusDescription = string.Format("The '{0}' plugin load failed. it's '{1}' dependent plugin is not exists.", plugin.Name, dependency.Name);

						throw new PluginException(plugin.StatusDescription);
					}

					//只要有一个依赖插件未加载完成则表示该插件不能立即加载(即返回假)
					if(dependency.Plugin.Status != PluginStatus.Loaded)
						return false;
				}
			}

			//表示当前插件的所有依赖插件都已加载完成则返回真(表示可以立即加载了)
			return true;
		}

		private bool TryPushToStack(Plugin plugin, Stack<Plugin> stack)
		{
			if(plugin == null || plugin.Status != PluginStatus.Loading || stack.Contains(plugin))
				return false;

			stack.Push(plugin);

			return true;
		}
		#endregion

		#region 事件方法
		private void OnLoaded(PluginLoadEventArgs args)
		{
			if(Loaded != null)
				this.Loaded(this, args);
		}

		private void OnLoading(PluginLoadEventArgs args)
		{
			if(Loading != null)
				this.Loading(this, args);
		}

		private void OnPluginLoaded(PluginLoadedEventArgs args)
		{
			if(PluginLoaded != null)
				this.PluginLoaded(this, args);
		}

		private void OnPluginLoading(PluginLoadingEventArgs args)
		{
			if(PluginLoading != null)
				this.PluginLoading(this, args);
		}

		private void OnPluginUnloaded(PluginUnloadedEventArgs args)
		{
			if(PluginUnloaded != null)
				this.PluginUnloaded(this, args);
		}

		private void OnPluginUnloading(PluginUnloadingEventArgs args)
		{
			if(PluginUnloading != null)
				this.PluginUnloading(this, args);
		}

		private bool OnPluginUnloading(Plugin plugin)
		{
			PluginUnloadingEventArgs args = new PluginUnloadingEventArgs(plugin);
			this.OnPluginUnloading(args);
			return args.Cancel;
		}
		#endregion
	}
}
