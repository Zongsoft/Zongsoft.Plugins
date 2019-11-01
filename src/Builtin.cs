﻿/*
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
using System.ComponentModel;
using System.Collections.Generic;

namespace Zongsoft.Plugins
{
	/// <summary>
	/// 关于构件功能的类，构件是组成插件的基本组成单位。
	/// </summary>
	/// <remarks>
	///		<para>构件是组成插件的基本组成单位。</para>
	///		<para>构件一旦被创建就属于某个插件，其位于所属插件的<seealso cref="Zongsoft.Plugins.Plugin.Builtins"/>集合内，但该构件是否处于插件树中，则取决于其所属的插件是否已经被成功加载，即插件的<seealso cref="Zongsoft.Plugins.Plugin.Status"/>属性应为<seealso cref="Zongsoft.Plugins.PluginStatus.Loaded"/>。</para>
	/// </remarks>
	public sealed class Builtin : PluginElement
	{
		#region 事件定义
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		public event EventHandler<ValueChangingEventArgs> ValueChanging;
		#endregion

		#region 同步变量
		private readonly object _syncRoot = new object();
		#endregion

		#region 成员变量
		private string _builderName;
		private string _position;
		private BuiltinType _builtinType;
		private bool _isBuilded;
		private object _value;
		private PluginTreeNode _node;
		private PluginExtendedPropertyCollection _properties;
		private BuiltinBehaviorCollection _behaviors;
		#endregion

		#region 构造函数
		internal Builtin(string builderName, string name, Plugin plugin) : base(name, plugin)
		{
			if(string.IsNullOrWhiteSpace(builderName))
				throw new ArgumentNullException("builderName");

			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(plugin == null)
				throw new ArgumentNullException("plugin");

			_builtinType = null;
			_value = null;
			_isBuilded = false;
			_position = string.Empty;
			_builderName = builderName.Trim();

			//将当前构件加入到所属插件的构件集中
			plugin.RegisterBuiltin(this);
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前的插件上下文对象。
		/// </summary>
		public PluginContext Context
		{
			get
			{
				if(this.Plugin == null)
					return null;

				return this.Plugin.Context;
			}
		}

		/// <summary>
		/// 获取当前构件所在的插件树节点，如果当前构件所在插件已被卸载则返回空。
		/// </summary>
		public PluginTreeNode Node
		{
			get
			{
				return _node;
			}
			internal set
			{
				_node = value;
			}
		}

		/// <summary>
		/// 获取当前构件所位于插件树节点的路径，其等同于<see cref="Node"/>属性指定的<seealso cref="PluginTreeNode.Path"/>值。
		/// </summary>
		public string Path
		{
			get
			{
				var node = _node;
				return node == null ? string.Empty : node.Path;
			}
		}

		/// <summary>
		/// 获取当前构件所位于插件树节点的完整路径，其等同于<see cref="Node"/>属性指定的<seealso cref="PluginTreeNode.FullPath"/>值。
		/// </summary>
		public string FullPath
		{
			get
			{
				var node = _node;
				return node == null ? string.Empty : node.FullPath;
			}
		}

		/// <summary>
		/// 获取构件的构建器名称，即构件在插件文件中的元素名。
		/// </summary>
		public string BuilderName
		{
			get
			{
				return _builderName;
			}
		}

		/// <summary>
		/// 获取构件是否已经构建过，只要构件被构建过该值则返回真，否则返回假。
		/// </summary>
		public bool IsBuilded
		{
			get
			{
				return _isBuilded;
			}
			internal set
			{
				if(_isBuilded == value)
					return;

				_isBuilded = value;

				//激发“PropertyChanged”事件
				this.OnPropertyChanged("IsBuilded");
			}
		}

		/// <summary>
		/// 获取构件的位置。
		/// </summary>
		/// <remarks>
		///		<para>
		///		在同级的构件中，通过指定该属性值来调整构件的排列顺序。
		///		</para>
		/// </remarks>
		public string Position
		{
			get
			{
				return _position;
			}
			internal set
			{
				if(string.IsNullOrEmpty(value))
					_position = string.Empty;
				else
					_position = value.Trim();
			}
		}

		/// <summary>
		/// 获取当前Value是否可用。如果Value不为空(null)则返回真(True)，否则返回假(False)。
		/// </summary>
		public bool HasValue
		{
			get
			{
				return _value != null;
			}
		}

		/// <summary>
		/// 获取构件的缓存值，获取该属性值始终不会引发构建动作。
		/// </summary>
		public object Value
		{
			get
			{
				return _value;
			}
			internal set
			{
				if(_value == value)
					return;

				//激发“ValueChanging”事件
				this.OnValueChanging(_value, value);

				//保存当前新值
				_value = value;

				//激发“ValueChanged”事件
				this.OnValueChanged(_value);
			}
		}

		/// <summary>
		/// 获取构件的类型定义。
		/// </summary>
		public BuiltinType BuiltinType
		{
			get
			{
				return _builtinType;
			}
			internal set
			{
				if(_builtinType != null)
					throw new InvalidOperationException();

				_builtinType = value;
			}
		}

		/// <summary>
		/// 获取当前构件是否具有特性，即 <see cref="Behaviors"/> 属性不为空并且集合元素数量大于零。
		/// </summary>
		public bool HasBehaviors
		{
			get
			{
				var behaviors = _behaviors;
				return behaviors != null && behaviors.Count > 0;
			}
		}

		/// <summary>
		/// 获取构件的行为特性集。
		/// </summary>
		public BuiltinBehaviorCollection Behaviors
		{
			get
			{
				if(_behaviors == null)
					System.Threading.Interlocked.CompareExchange(ref _behaviors, new BuiltinBehaviorCollection(this), null);

				return _behaviors;
			}
		}

		/// <summary>
		/// 获取当前构件是否具有扩展属性。
		/// </summary>
		public bool HasProperties
		{
			get
			{
				var properties = _properties;
				return properties != null && properties.Count > 0;
			}
		}

		/// <summary>
		/// 获取构件的扩展属性集。
		/// </summary>
		public PluginExtendedPropertyCollection Properties
		{
			get
			{
				if(_properties == null)
					System.Threading.Interlocked.CompareExchange(ref _properties, new PluginExtendedPropertyCollection(this), null);

				return _properties;
			}
		}
		#endregion

		#region 获取方法
		/// <summary>
		/// 在不构建值的情况下，获取构件值的类型。
		/// </summary>
		/// <returns>如果<see cref="HasValue"/>为真(true)则返回<see cref="Value"/>属性值的类型，否则根据当前构件的插件类型声明进行类型解析。</returns>
		/// <remarks>
		///		<para>对于自定义构建器的目标类型，将由构建器标注的<seealso cref="Builders.BuilderBehaviorAttribute"/>特性提供，详情请参考它的描述信息。</para>
		/// </remarks>
		public Type GetValueType()
		{
			if(_value != null)
				return _value.GetType();

			if(_builtinType != null)
				return _builtinType.Type;

			var builder = this.Plugin.GetBuilder(_builderName);

			if(builder == null)
				throw new PluginException($"Not found the builder for the '{_builderName}'({this.FullPath}) builtin.");

			return builder.GetValueType(this);
		}

		public object GetValue(ObtainMode obtainMode, Builders.BuilderSettings settings = null)
		{
			switch(obtainMode)
			{
				case ObtainMode.Alway:
					//注意：当获取方式为始终创建新的实例时，必须忽略后续的追加操作，
					//以避免将重复新建的实例追加到所有者集合中可能导致的集合项键冲突的错误。

					if(settings == null)
						settings = Builders.BuilderSettings.Create(Builders.BuilderSettingsFlags.IgnoreAppending);
					else
						settings.SetFlags(Builders.BuilderSettingsFlags.IgnoreAppending);

					return this.Build(settings);
				case ObtainMode.Auto:
					if(_value == null)
					{
						lock(_syncRoot)
						{
							if(_value == null)
								return this.Build(settings);
						}
					}
					break;
			}

			return _value;
		}
		#endregion

		#region 构建方法
		public object Build(Builders.BuilderSettings settings = null)
		{
			return Builders.BuilderManager.Current.Build(this, settings);
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			return string.Format("[{0}]{1}@{2}", this.BuilderName, this.FullPath, this.Plugin.Name);
		}

		public override bool Equals(object obj)
		{
			if(obj == null)
				return false;

			Builtin target = obj as Builtin;

			if(target == null)
				return false;

			return string.Equals(_builderName, target.BuilderName, StringComparison.OrdinalIgnoreCase) &&
				   string.Equals(this.FullPath, target.FullPath, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return string.Format("{0}@{1}", this.FullPath.ToLowerInvariant(), this.BuilderName.ToLowerInvariant()).GetHashCode();
		}
		#endregion

		#region 私有方法
		private void OnValueChanged(object value)
		{
			if(this.ValueChanged != null)
				this.ValueChanged(this, new ValueChangedEventArgs(value));
		}

		private void OnValueChanging(object oldValue, object newValue)
		{
			if(this.ValueChanging != null)
				this.ValueChanging(this, new ValueChangingEventArgs(oldValue, newValue));
		}
		#endregion
	}
}
