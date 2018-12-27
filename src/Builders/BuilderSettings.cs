/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2010-2018 Zongsoft Corporation <http://www.zongsoft.com>
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

namespace Zongsoft.Plugins.Builders
{
	/// <summary>
	/// 表示构建设置的类。
	/// </summary>
	public class BuilderSettings
	{
		#region 成员字段
		private BuilderSettingsFlags _flags;
		#endregion

		#region 构造函数
		public BuilderSettings(object parameter, Action<BuilderContext> builded = null)
		{
			this.Parameter = parameter;
			this.Builded = builded;
		}

		public BuilderSettings(Action<BuilderContext> builded = null)
		{
			this.Builded = builded;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置构建的自定义参数。
		/// </summary>
		public object Parameter
		{
			get; set;
		}

		/// <summary>
		/// 获取或设置构建行为的标记。
		/// </summary>
		public BuilderSettingsFlags Flags
		{
			get => _flags;
			set => _flags = value;
		}

		/// <summary>
		/// 获取或设置构建完成的回调方法。
		/// </summary>
		public Action<BuilderContext> Builded
		{
			get; set;
		}
		#endregion

		#region 公共方法
		public void SetFlags(BuilderSettingsFlags flags)
		{
			_flags |= flags;
		}

		public bool HasFlags(BuilderSettingsFlags flags)
		{
			return (_flags & flags) == flags;
		}
		#endregion

		#region 静态方法
		public static BuilderSettings Ignores(BuilderSettingsFlags flags, object parameter = null)
		{
			return new BuilderSettings(parameter) { Flags = flags };
		}
		#endregion
	}
}
