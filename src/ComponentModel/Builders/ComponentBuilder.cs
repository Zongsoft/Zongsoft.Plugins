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
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zongsoft.Plugins;
using Zongsoft.Plugins.Builders;

namespace Zongsoft.ComponentModel.Plugins.Builders
{
	public class ComponentBuilder : Zongsoft.Plugins.Builders.BuilderBase
	{
		#region 重写方法
		protected override void OnBuildComplete(BuilderContext context)
		{
			IContainer container = null;
			IComponent component = context.Result as IComponent;

			if(component == null)
				return;

			container = context.Owner as IContainer;

			if(container == null)
			{
				var workbench = context.Owner as IWorkbench;

				if(workbench == null)
				{
					container = this.GetContainer(context.Owner);
				}
				else
				{
					container = workbench.Window as IContainer;

					if(container == null)
						container = this.GetContainer(workbench.Window);
				}
			}

			if(container != null)
				container.Add(component, context.Builtin.Name);
		}
		#endregion

		#region 私有方法
		private IContainer GetContainer(object target)
		{
			if(target == null)
				return null;

			var memberInfo = target.GetType().FindMembers((MemberTypes.Field | MemberTypes.Property),
												(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty),
												(member, criteria) =>
												{
													if(member.MemberType == MemberTypes.Field)
														return typeof(IContainer).IsAssignableFrom(((FieldInfo)member).FieldType);

													if(member.MemberType == MemberTypes.Property)
														return typeof(IContainer).IsAssignableFrom(((PropertyInfo)member).PropertyType);

													return false;
												}, null).FirstOrDefault();

			if(memberInfo.MemberType == MemberTypes.Field)
				return ((FieldInfo)memberInfo).GetValue(target) as IContainer;

			if(memberInfo.MemberType == MemberTypes.Property)
				return ((PropertyInfo)memberInfo).GetValue(target, null) as IContainer;

			return null;
		}
		#endregion
	}
}
