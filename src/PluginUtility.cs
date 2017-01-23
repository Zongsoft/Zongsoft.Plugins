/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zongsoft.Plugins
{
	public static class PluginUtility
	{
		#region 私有变量
		private static int _anonymousId;
		#endregion

		#region 获取类型
		/// <summary>
		/// 根据指定的类型限定名动态加载并返回对应的<seealso cref="System.Type"/>，如果查找失败亦不会抛出异常。
		/// </summary>
		/// <param name="typeFullName">要获取的类型限定名称。</param>
		/// <returns>返回加载成功的类型对象，如果加载失败则返回空(null)。</returns>
		public static Type GetType(string typeFullName)
		{
			if(string.IsNullOrWhiteSpace(typeFullName))
				return null;

			Type type = GetTypeFromAlias(typeFullName);

			if(type != null)
				return type;

			type = Type.GetType(typeFullName, assemblyName =>
			{
				Assembly assembly = ResolveAssembly(assemblyName);

				if(assembly == null)
					assembly = LoadAssembly(assemblyName);

				return assembly;
			}, (assembly, typeName, ignoreCase) =>
			{
				if(assembly == null)
					return Type.GetType(typeName, false, ignoreCase);
				else
					return assembly.GetType(typeName, false, ignoreCase);
			}, false);

			if(type == null)
				throw new PluginException(string.Format("The '{0}' type resolve failed.", typeFullName));

			return type;
		}

		public static Type GetType(Builtin builtin)
		{
			if(builtin == null)
				return null;

			if(builtin.BuiltinType != null)
				return builtin.BuiltinType.Type;
			else
				return GetType(builtin.Properties.GetValue<string>("type"));
		}

		private static Type GetTypeFromAlias(string typeName)
		{
			if(string.IsNullOrEmpty(typeName))
				return null;

			switch(typeName.Replace(" ", "").ToLowerInvariant())
			{
				case "string":
					return typeof(string);
				case "string[]":
					return typeof(string[]);

				case "int":
					return typeof(int);
				case "int?":
					return typeof(int?);
				case "int[]":
					return typeof(int[]);

				case "long":
					return typeof(long);
				case "long?":
					return typeof(long?);
				case "long[]":
					return typeof(long[]);

				case "short":
					return typeof(short);
				case "short?":
					return typeof(short?);
				case "short[]":
					return typeof(short[]);

				case "byte":
					return typeof(byte);
				case "byte?":
					return typeof(byte?);
				case "byte[]":
					return typeof(byte[]);

				case "bool":
				case "boolean":
					return typeof(bool);
				case "bool?":
				case "boolean?":
					return typeof(bool?);
				case "bool[]":
				case "boolean[]":
					return typeof(bool[]);

				case "money":
				case "number":
				case "numeric":
				case "decimal":
					return typeof(decimal);
				case "money?":
				case "number?":
				case "numeric?":
				case "decimal?":
					return typeof(decimal?);
				case "money[]":
				case "number[]":
				case "numeric[]":
				case "decimal[]":
					return typeof(decimal[]);

				case "float":
				case "single":
					return typeof(float);
				case "float?":
				case "single?":
					return typeof(float?);
				case "float[]":
				case "single[]":
					return typeof(float[]);

				case "double":
					return typeof(double);
				case "double?":
					return typeof(double?);
				case "double[]":
					return typeof(double[]);

				case "uint":
					return typeof(uint);
				case "uint?":
					return typeof(uint?);
				case "uint[]":
					return typeof(uint[]);

				case "ulong":
					return typeof(ulong);
				case "ulong?":
					return typeof(ulong?);
				case "ulong[]":
					return typeof(ulong[]);

				case "ushort":
					return typeof(ushort);
				case "ushort?":
					return typeof(ushort?);
				case "ushort[]":
					return typeof(ushort[]);

				case "sbyte":
					return typeof(sbyte);
				case "sbyte?":
					return typeof(sbyte?);
				case "sbyte[]":
					return typeof(sbyte[]);

				case "char":
					return typeof(char);
				case "char?":
					return typeof(char?);
				case "char[]":
					return typeof(char[]);

				case "date":
				case "time":
				case "datetime":
					return typeof(DateTime);
				case "date?":
				case "time?":
				case "datetime?":
					return typeof(DateTime?);
				case "date[]":
				case "time[]":
				case "datetime[]":
					return typeof(DateTime[]);

				case "timespan":
					return typeof(TimeSpan);
				case "timespan?":
					return typeof(TimeSpan?);
				case "timespan[]":
					return typeof(TimeSpan[]);

				case "guid":
					return typeof(Guid);
				case "guid?":
					return typeof(Guid?);
				case "guid[]":
					return typeof(Guid[]);
			}

			return null;
		}
		#endregion

		#region 构建构件
		public static object BuildBuiltin(Builtin builtin, IEnumerable<string> ignoredProperties = null)
		{
			if(builtin == null)
				throw new ArgumentNullException("builtin");

			var result = PluginUtility.BuildType(builtin);

			//设置更新目标对象的属性集
			if(result != null)
				UpdateProperties(result, builtin, ignoredProperties);

			return result;
		}

		internal static void UpdateProperties(object target, Builtin builtin, IEnumerable<string> ignoredProperties)
		{
			if(target == null || builtin == null)
				return;

			foreach(string propertyName in builtin.Properties.AllKeys)
			{
				//如果当前属性名为忽略属性则忽略设置
				if(ignoredProperties != null && ignoredProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
					continue;

				try
				{
					var propertyType = Zongsoft.Common.Convert.GetMemberType(target, propertyName);
					var propertyValue = builtin.Properties.GetValue(propertyName, propertyType, null);

					Zongsoft.Common.Convert.SetValue(target, propertyName, propertyValue);
				}
				catch(Exception ex)
				{
					var message = new StringBuilder();

					message.AppendFormat("{0}[{1}]", ex.Message, ex.Source);
					message.AppendLine();

					if(ex.InnerException != null)
					{
						message.AppendFormat("\t{0}: {1}[{2}]", ex.GetType().FullName, ex.Message, ex.Source);
						message.AppendLine();
					}

					message.AppendFormat("\tOccurred an error on set '{1}' property of '{0}' builtin, it's raw value is \"{2}\", The target type of builtin is '{3}'.",
											builtin.ToString(),
											propertyName,
											builtin.Properties[propertyName].RawValue,
											target.GetType().AssemblyQualifiedName);

					throw new PluginException(message.ToString(), ex);
				}
			}
		}

		internal static object BuildType(Builtin builtin)
		{
			if(builtin == null)
				throw new ArgumentNullException("builtin");

			if(builtin.BuiltinType != null)
				return BuildType(builtin.BuiltinType);

			string typeName = builtin.Properties.GetValue<string>("type");

			if(string.IsNullOrWhiteSpace(typeName))
				return null;
			else
				return BuildType(typeName, builtin);
		}

		internal static object BuildType(BuiltinType builtinType)
		{
			if(builtinType == null)
				throw new ArgumentNullException("builtinType");

			if(string.IsNullOrEmpty(builtinType.TypeName))
				return null;

			object target = null;

			if(builtinType.Constructor == null || builtinType.Constructor.Count < 1)
			{
				target = BuildType(builtinType.TypeName, builtinType.Builtin);
			}
			else
			{
				object[] values = new object[builtinType.Constructor.Count];

				for(int i = 0; i < values.Length; i++)
				{
					values[i] = builtinType.Constructor.Parameters[i].GetValue(null);
				}

				try
				{
					target = Activator.CreateInstance(builtinType.Type, values);

					//注入依赖属性
					InjectProperties(target, builtinType.Builtin);
				}
				catch(Exception ex)
				{
					throw new PluginException(string.Format("Create object of '{0}' type faild, The parameters count of constructor is {1}.", builtinType.TypeName, values.Length), ex);
				}
			}

			return target;
		}

		internal static object BuildType(string typeName, Builtin builtin)
		{
			Type type = PluginUtility.GetType(typeName);

			if(type == null)
				throw new PluginException(string.Format("Can not get type from '{0}' text for '{1}' builtin.", typeName, builtin));

			try
			{
				object result = BuildType(type, (Type parameterType, string parameterName, out object parameterValue) =>
				{
					if(parameterType == typeof(Builtin))
					{
						parameterValue = builtin;
						return true;
					}

					if(parameterType == typeof(PluginTreeNode))
					{
						parameterValue = builtin.Node;
						return true;
					}

					if(parameterType == typeof(string) && string.Equals(parameterName, "name", StringComparison.OrdinalIgnoreCase))
					{
						parameterValue = builtin.Name;
						return true;
					}

					if(typeof(Zongsoft.Services.IServiceProvider).IsAssignableFrom(parameterType))
					{
						parameterValue = FindServiceProvider(builtin);
						return true;
					}

					return ObtainParameter(builtin.Plugin, parameterType, parameterName, out parameterValue);
				});

				if(result == null)
					throw new PluginException(string.Format("Can not build instance of '{0}' type, Maybe that's cause type-generator not found matched constructor with parameters. in '{1}' builtin.", type.FullName, builtin));

				//注入依赖属性
				InjectProperties(result, builtin);

				return result;
			}
			catch(Exception ex)
			{
				throw new PluginException(string.Format("Occurred an exception on create a builtin instance of '{0}' type, at '{1}' builtin.", type.FullName, builtin), ex);
			}
		}

		internal static object BuildType(Type type, ObtainParameterCallback obtainParameter)
		{
			if(type == null)
				throw new ArgumentNullException("type");

			if(obtainParameter == null)
				throw new ArgumentNullException("obtainParameter");

			ConstructorInfo[] constructors = type.GetConstructors();

			foreach(ConstructorInfo constructor in constructors.OrderByDescending(ctor => ctor.GetParameters().Length))
			{
				ParameterInfo[] parameters = constructor.GetParameters();

				if(parameters.Length == 0)
					return Activator.CreateInstance(type);

				bool matched = false;
				object[] values = new object[parameters.Length];

				for(int i = 0; i < parameters.Length; i++)
				{
					//依次获取当前构造函数的参数值
					matched = obtainParameter(parameters[i].ParameterType, parameters[i].Name, out values[i]);

					//如果获取参数值失败，则当前构造函数匹配失败
					if(!matched)
						break;
				}

				if(matched)
					return Activator.CreateInstance(type, values);
			}

			return null;
		}

		#region 委托定义
		internal delegate bool ObtainParameterCallback(Type parameterType, string parameterName, out object parameterValue);
		#endregion

		internal static bool ObtainParameter(Plugin plugin, Type parameterType, string parameterName, out object parameterValue)
		{
			if(parameterType == typeof(Plugin))
			{
				parameterValue = plugin;
				return true;
			}

			if(parameterType == typeof(PluginContext))
			{
				parameterValue = plugin.Context;
				return true;
			}

			if(typeof(Zongsoft.ComponentModel.ApplicationContextBase).IsAssignableFrom(parameterType))
			{
				parameterValue = plugin.Context.ApplicationContext;
				return true;
			}

			if(typeof(Zongsoft.Services.IServiceProviderFactory).IsAssignableFrom(parameterType))
			{
				parameterValue = plugin.Context.ServiceFactory;
				return true;
			}

			if(typeof(Zongsoft.Services.IServiceProvider).IsAssignableFrom(parameterType))
			{
				parameterValue = plugin.Context.ServiceFactory.Default;
				return true;
			}

			if(typeof(Zongsoft.Options.Profiles.Profile).IsAssignableFrom(parameterType))
			{
				parameterValue = Zongsoft.Options.Plugins.OptionUtility.GetProfile(plugin);
				return true;
			}

			if(typeof(Zongsoft.Options.Configuration.OptionConfiguration).IsAssignableFrom(parameterType))
			{
				parameterValue = Zongsoft.Options.Plugins.OptionUtility.GetConfiguration(plugin);
				return true;
			}

			if(typeof(Zongsoft.Options.IOptionProvider).IsAssignableFrom(parameterType))
			{
				parameterValue = plugin.Context.ApplicationContext.OptionManager;
				return true;
			}

			if(typeof(Zongsoft.Options.ISettingsProvider).IsAssignableFrom(parameterType))
			{
				parameterValue = Zongsoft.Options.Plugins.PluginSettingsProviderFactory.GetProvider(plugin);
				return true;
			}

			parameterValue = null;
			return false;
		}
		#endregion

		internal static Zongsoft.Services.IServiceProvider FindServiceProvider(Builtin builtin)
		{
			if(builtin == null)
				return null;

			if(builtin.Node != null && builtin.Node.Parent != null)
				return builtin.Context.ServiceFactory.GetProvider(builtin.Node.Parent.Name) ?? builtin.Context.ServiceFactory.Default;

			return builtin.Context.ServiceFactory.Default;
		}

		internal static int GetAnonymousId(string assortment)
		{
			return System.Threading.Interlocked.Increment(ref _anonymousId);
		}

		internal static object ResolveValue(PluginElement element, string text, string memberName, Type memberType, object defaultValue)
		{
			if(element == null)
				throw new ArgumentNullException("element");

			if(string.IsNullOrWhiteSpace(text))
				return Zongsoft.Common.Convert.ConvertValue(text, memberType, defaultValue);

			object result = text;

			//进行解析器处理，如果解析器无法处理将会返回传入的原始值
			if(Parsers.Parser.CanParse(text))
			{
				if(element is Builtin)
					result = Parsers.Parser.Parse(text, (Builtin)element, memberName, memberType);
				else if(element is PluginTreeNode)
					result = Parsers.Parser.Parse(text, (PluginTreeNode)element, memberName, memberType);
				else
					throw new ArgumentException(string.Format("Can not support the '{0}' element type.", element.GetType()));
			}

			//对最后的结果进行类型转换，如果指定的类型为空，该转换操作不会执行任何动作
			if(memberType == null)
				return result;
			else
				return Zongsoft.Common.Convert.ConvertValue(result, memberType, defaultValue);
		}

		internal static Assembly LoadAssembly(AssemblyName assemblyName)
		{
			if(assemblyName == null)
				return null;

			return AppDomain.CurrentDomain.Load(assemblyName);
		}

		internal static Assembly ResolveAssembly(AssemblyName assemblyName)
		{
			if(assemblyName == null)
				return null;

			byte[] token = assemblyName.GetPublicKeyToken();
			IList<Assembly> assemblies = new List<Assembly>();

			foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				bool matched = string.Equals(assembly.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase);

				if(token != null && token.Length > 0)
					matched &= CompareBytes(token, assembly.GetName().GetPublicKeyToken());

				if(matched)
					assemblies.Add(assembly);
			}

			if(assemblies.Count < 1)
			{
				if(assemblyName.Name.StartsWith("System."))
					return AppDomain.CurrentDomain.Load(assemblyName.Name);

				return null;
			}

			if(assemblies.Count == 1)
				return assemblies[0];

			Assembly maxAssembly = assemblies[0];

			foreach(Assembly assembly in assemblies)
			{
				if(assembly.GetName().Version == null)
					continue;

				if(assembly.GetName().Version.CompareTo(maxAssembly.GetName().Version) > 0)
					maxAssembly = assembly;
			}

			return maxAssembly;
		}

		internal static MemberInfo GetStaticMember(string qualifiedName)
		{
			if(string.IsNullOrWhiteSpace(qualifiedName))
				return null;

			var parts = qualifiedName.Split(',');

			if(parts.Length != 2)
				throw new ArgumentException(string.Format("Invalid qualified name '{0}'.", qualifiedName));

			var assemblyName = parts[1].Trim();

			if(string.IsNullOrWhiteSpace(assemblyName))
				throw new ArgumentException(string.Format("Missing assembly name in the qualified name '{0}'.", qualifiedName));

			//根据指定程序集名称获取对应的程序集
			var assembly = ResolveAssembly(new AssemblyName(assemblyName));

			if(assembly == null)
				throw new InvalidOperationException(string.Format("Not found '{0}' assembly in the runtimes, for '{1}' qualified type name.", assemblyName, qualifiedName));

			//分解类型成员的完整路径
			parts = parts[0].Split('.');

			//不能小于三个部分，因为「Namespace.Type.Member」至少包含三个部分
			if(parts.Length < 3)
				return null;

			var typeFullName = string.Join(".", parts, 0, parts.Length - 1);
			var type = assembly.GetType(typeFullName, false);

			if(type == null)
				throw new ArgumentException(string.Format("Cann't obtain the type by '{0}' type-name in the '{1}' assembly.", typeFullName, assembly.FullName));

			//获取指定的成员信息
			return type.GetMember(parts[parts.Length - 1], (MemberTypes.Field | MemberTypes.Property), BindingFlags.Public | BindingFlags.Static).FirstOrDefault();
		}

		private static void InjectProperties(object target, Builtin builtin)
		{
			if(target == null || builtin == null)
				return;

			//获取当前构件所属的服务容器
			var serviceProvider = FindServiceProvider(builtin);

			if(serviceProvider == null)
				return;

			//查找指定目标对象需要注入的属性和字段集(支持对非公共成员的注入)
			var members = target.GetType().FindMembers(MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (m, t) => m.GetCustomAttribute((Type)t, true) != null, typeof(Zongsoft.Services.ServiceDependencyAttribute));

			if(members == null || members.Length < 1)
				return;

			object memberValue;

			foreach(var member in members)
			{
				//如果当前成员已经在构件属性集中显式存在则跳过
				if(builtin.HasProperties && builtin.Properties.Contains(member.Name))
					continue;

				//获取需注入成员的注入标记
				var attribute = (Zongsoft.Services.ServiceDependencyAttribute)member.GetCustomAttribute(typeof(Zongsoft.Services.ServiceDependencyAttribute), true);

				switch(member.MemberType)
				{
					case MemberTypes.Field:
						if(!string.IsNullOrWhiteSpace(attribute.Name))
							memberValue = serviceProvider.Resolve(attribute.Name);
						else
							memberValue = serviceProvider.Resolve(attribute.Contract ?? ((FieldInfo)member).FieldType);

						((FieldInfo)member).SetValue(target, memberValue);
						break;
					case MemberTypes.Property:
						if(((PropertyInfo)member).CanWrite)
						{
							if(!string.IsNullOrWhiteSpace(attribute.Name))
								memberValue = serviceProvider.Resolve(attribute.Name);
							else
								memberValue = serviceProvider.Resolve(attribute.Contract ?? ((PropertyInfo)member).PropertyType);

							((PropertyInfo)member).SetValue(target, memberValue);
						}

						break;
				}
			}
		}

		private static bool CompareBytes(byte[] a, byte[] b)
		{
			if(a == null && b == null)
				return true;
			if(a == null || b == null)
				return false;
			if(a.Length != b.Length)
				return false;

			for(int i = 0; i < a.Length; i++)
			{
				if(a[i] != b[i])
					return false;
			}

			return true;
		}
	}
}
