/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 *
 * Copyright (C) 2010-2017 Zongsoft Corporation <http://www.zongsoft.com>
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

using Zongsoft.IO;

using Xunit;

namespace Zongsoft.Plugins.Tests
{
	public class PluginPathTest
	{
		[Fact]
		public void ParseTest()
		{
			PluginPath path;

			path = PluginPath.Parse("");
			Assert.Null(path);
			path = PluginPath.Parse("  ");
			Assert.Null(path);

			path = PluginPath.Parse(".");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Current, path.Anchor);
			Assert.Equal(".", path.Path);
			Assert.Equal(0, path.Members.Length);

			path = PluginPath.Parse("..");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Parent, path.Anchor);
			Assert.Equal("..", path.Path);
			Assert.Equal(0, path.Members.Length);

			path = PluginPath.Parse("/");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Root, path.Anchor);
			Assert.Equal("/", path.Path);
			Assert.Equal(0, path.Members.Length);

			path = PluginPath.Parse("workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.None, path.Anchor);
			Assert.Equal("workbench", path.Path);
			Assert.Equal(1, path.Members.Length);
			Assert.Equal("title", path.Members[0].Name);

			path = PluginPath.Parse("/workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Root, path.Anchor);
			Assert.Equal("/workbench", path.Path);
			Assert.Equal(1, path.Members.Length);
			Assert.Equal("title", path.Members[0].Name);

			path = PluginPath.Parse("./workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Current, path.Anchor);
			Assert.Equal("./workbench", path.Path);
			Assert.Equal(1, path.Members.Length);
			Assert.Equal("title", path.Members[0].Name);

			path = PluginPath.Parse("../workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Parent, path.Anchor);
			Assert.Equal("../workbench", path.Path);
			Assert.Equal(1, path.Members.Length);
			Assert.Equal("title", path.Members[0].Name);

			path = PluginPath.Parse("@workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.None, path.Anchor);
			Assert.Equal("", path.Path);
			Assert.Equal(2, path.Members.Length);
			Assert.Equal("workbench", path.Members[0].Name);
			Assert.Equal("title", path.Members[1].Name);

			path = PluginPath.Parse(".@workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Current, path.Anchor);
			Assert.Equal(".", path.Path);
			Assert.Equal(2, path.Members.Length);
			Assert.Equal("workbench", path.Members[0].Name);
			Assert.Equal("title", path.Members[1].Name);

			path = PluginPath.Parse("./@workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Current, path.Anchor);
			Assert.Equal(".", path.Path);
			Assert.Equal(2, path.Members.Length);
			Assert.Equal("workbench", path.Members[0].Name);
			Assert.Equal("title", path.Members[1].Name);

			path = PluginPath.Parse("../@Property");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Parent, path.Anchor);
			Assert.Equal("..", path.Path);
			Assert.Equal(1, path.Members.Length);
			Assert.Equal("Property", path.Members[0].Name);

			path = PluginPath.Parse("../@workbench.title");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Parent, path.Anchor);
			Assert.Equal("..", path.Path);
			Assert.Equal(2, path.Members.Length);
			Assert.Equal("workbench", path.Members[0].Name);
			Assert.Equal("title", path.Members[1].Name);

			path = PluginPath.Parse("../@workbench.title[0]");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Parent, path.Anchor);
			Assert.Equal("..", path.Path);
			Assert.Equal(3, path.Members.Length);
			Assert.Equal("workbench", path.Members[0].Name);
			Assert.Equal("title", path.Members[1].Name);
			Assert.True(path.Members[2].IsIndexer);
			Assert.True(string.IsNullOrEmpty(path.Members[2].Name));
			Assert.Equal(1, path.Members[2].Parameters.Length);
			Assert.Equal(0, path.Members[2].Parameters[0]);
		}

		[Fact]
		public void ParseTestAdvance()
		{
			PluginPath path;

			path = PluginPath.Parse("@[key].workbench.title[0].Value");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.None, path.Anchor);
			Assert.Equal("", path.Path);
			Assert.Equal(5, path.Members.Length);
			Assert.True(path.Members[0].IsIndexer);
			Assert.Equal(1, path.Members[0].Parameters.Length);
			Assert.Equal("key", path.Members[0].Parameters[0]);
			Assert.Equal("workbench", path.Members[1].Name);
			Assert.Equal("title", path.Members[2].Name);
			Assert.True(path.Members[3].IsIndexer);
			Assert.Equal(1, path.Members[3].Parameters.Length);
			Assert.Equal(0, path.Members[3].Parameters[0]);
			Assert.Equal("Value", path.Members[4].Name);

			path = PluginPath.Parse("../@[key].workbench.title[0].Value");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Parent, path.Anchor);
			Assert.Equal("..", path.Path);
			Assert.Equal(5, path.Members.Length);
			Assert.True(path.Members[0].IsIndexer);
			Assert.Equal(1, path.Members[0].Parameters.Length);
			Assert.Equal("key", path.Members[0].Parameters[0]);
			Assert.Equal("workbench", path.Members[1].Name);
			Assert.Equal("title", path.Members[2].Name);
			Assert.True(path.Members[3].IsIndexer);
			Assert.Equal(1, path.Members[3].Parameters.Length);
			Assert.Equal(0, path.Members[3].Parameters[0]);
			Assert.Equal("Value", path.Members[4].Name);

			path = PluginPath.Parse(@".. / @ [ 'k\' ey' ] . workbench . title[ 0  ].Value");
			Assert.NotNull(path);
			Assert.Equal(PathAnchor.Parent, path.Anchor);
			Assert.Equal("..", path.Path);
			Assert.Equal(5, path.Members.Length);
			Assert.True(path.Members[0].IsIndexer);
			Assert.Equal(1, path.Members[0].Parameters.Length);
			Assert.Equal("k' ey", path.Members[0].Parameters[0]);
			Assert.Equal("workbench", path.Members[1].Name);
			Assert.Equal("title", path.Members[2].Name);
			Assert.True(path.Members[3].IsIndexer);
			Assert.Equal(1, path.Members[3].Parameters.Length);
			Assert.Equal(0, path.Members[3].Parameters[0]);
			Assert.Equal("Value", path.Members[4].Name);
		}
	}
}
