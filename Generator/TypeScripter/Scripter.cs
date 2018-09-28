using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeScripter.TypeScript;

namespace TypeScripter
{
	/// <summary>
	/// A class which generates TypeScript definitions for .NET types
	/// </summary>
	public class Scripter
	{
		private readonly Compilation _compilation;
		#region Properties
		private Dictionary<ITypeSymbol, TsType> TypeLookup
		{
			get;
		}

		private HashSet<TsType> Types
		{
			get;
		}

		public Func<ITypeSymbol, bool> TypeFilter
		{
			get;
			set;
		}

		private TsFormatter Formatter
		{
			get;
			set;
		}
		#endregion

		#region Creation

		/// <summary>
		/// Constructor
		/// </summary>
		public Scripter(Compilation compilation)
		{
			Types = new HashSet<TsType>();
			_compilation = compilation;

			// Add mappings for primitives 
			TypeLookup = new Dictionary<ITypeSymbol, TsType>()
			{
				{ _compilation.GetSpecialType(SpecialType.System_Void), TsPrimitive.Void },
				{ _compilation.GetSpecialType(SpecialType.System_Object), TsPrimitive.Any },
				{ _compilation.GetSpecialType(SpecialType.System_String), TsPrimitive.String },
				{ _compilation.GetSpecialType(SpecialType.System_Boolean), TsPrimitive.Boolean },
				{ _compilation.GetSpecialType(SpecialType.System_Byte), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_Single), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_Int16), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_Int32), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_Int64), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_UInt16), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_UInt32), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_UInt64), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_Decimal), TsPrimitive.Number },
				{ _compilation.GetSpecialType(SpecialType.System_Double), TsPrimitive.Number },
				{ _compilation.GetTypeByMetadataName(typeof(Guid).FullName), TsPrimitive.String },
				{ _compilation.GetTypeByMetadataName(typeof(Uri).FullName), TsPrimitive.String },
				{ _compilation.GetTypeByMetadataName(typeof(DateTime).FullName), TsPrimitive.Date },
				{ _compilation.GetTypeByMetadataName(typeof(Task).FullName), TsPrimitive.Void },
			};

			// initialize the scripter with default implementations
			Formatter = new TsFormatter(false);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets the current scripter output
		/// </summary>
		/// <returns>The scripter output</returns>
		public override string ToString()
		{
			var str = new StringBuilder();
			foreach (var module in Modules().OrderBy(x => x.Name))
				str.Append(Formatter.Format(module));
			return str.ToString();
		}
		#endregion

		#region Operations

		/// <summary>
		/// Adds a type to be scripted
		/// </summary>
		/// <param name="tsType">The type</param>
		/// <returns>The <see cref="Scripter"/></returns>
		public Scripter AddType(TsType tsType)
		{
			Types.Add(tsType);
			return this;
		}

		/// <summary>
		/// Adds a type to be scripted
		/// </summary>
		/// <param name="tsType">The type</param>
		/// <param name="type">The .NET type</param>
		/// <returns>The <see cref="Scripter"/></returns>
		private Scripter AddType(TsType tsType, ITypeSymbol type)
		{
			AddType(tsType);
			RegisterTypeMapping(tsType, type);
			return this;
		}

		/// <summary>
		/// Registers a type mapping
		/// </summary>
		/// <param name="tsType">The TypeScript type</param>
		/// <param name="type">The native type</param>
		private void RegisterTypeMapping(TsType tsType, ITypeSymbol type)
		{
			if (type.SpecialType == SpecialType.None)
			{
				TypeLookup[type] = tsType;
			}
		}

		/// <summary>
		/// Registers custom type mapping
		/// </summary>
		/// <param name="tsType">The TypeScript type</param>
		/// <param name="type">The native type</param>
		/// <returns></returns>
		public Scripter WithTypeMapping(TsType tsType, Type type)
		{
			var typeSymbol = _compilation.GetTypeByMetadataName(type.FullName);
			if (typeSymbol == null)
			{
				throw new InvalidOperationException($"Cannot get type {type.FullName} from compiler.");
			}

			if (TypeLookup.ContainsKey(typeSymbol))
			{
				throw new ArgumentException("Mapping for " + type.Name + " is already defined.", nameof(type));
			}


			TypeLookup[typeSymbol] = tsType;
			return this;
		}

		/// <summary>
		/// Adds a particular type to be scripted
		/// </summary>
		/// <param name="type">The type</param>
		/// <returns>The <see cref="Scripter"/></returns>
		public Scripter AddType(ITypeSymbol type)
		{
			Console.WriteLine(type.Name);
			Resolve(type);
			return this;
		}

		/// <summary>
		/// Adds a set of types to be scripted
		/// </summary>
		/// <param name="types">The types to add</param>
		/// <returns>The <see cref="Scripter"/></returns>
		public Scripter AddTypes(IEnumerable<INamedTypeSymbol> types)
		{
			foreach (var type in types)
				AddType(type);
			return this;
		}

		#endregion

		#region Type Generation

		private bool IsGuid(ITypeSymbol type)
		{
			return $"{type.ContainingNamespace}.{type.MetadataName}" == typeof(System.Guid).FullName &&
			       type.TypeKind == TypeKind.Struct;
		}
		/// <summary>
		/// Generates a TypeScript interface for a particular CLR type
		/// </summary>
		/// <param name="type">The type</param>
		/// <returns>The resulting TypeScript interface</returns>
		private TsInterface GenerateInterface(ITypeSymbol type)
		{
			if (type.DeclaringSyntaxReferences.Length == 0)
			{
				//We do not generate interface from other assemblies.
				return null;
			}
			var tsInterface = new TsInterface(GetName(type));
			AddType(tsInterface, type);

			// resolve non-inherited interfaces implemented by the type
			foreach (var interfaceType in type.AllInterfaces)
				AddType(interfaceType);

			if (type is INamedTypeSymbol symbol && symbol.IsGenericType)
			{
				//Skipping generic types for now
				return null;
			}

			// resolve the base class if present
			if (type.BaseType != null && type.BaseType.SpecialType != SpecialType.System_Object)
			{
				var baseType = Resolve(type.BaseType);
				if (baseType != null && baseType != TsPrimitive.Any)
					tsInterface.BaseInterfaces.Add(baseType);
			}

			// process properties
			foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
			{
				var tsProperty = Resolve(property);
				if (tsProperty != null)
					tsInterface.Properties.Add(tsProperty);
			}

			// process fields
			foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
			{
				var tsProperty = Resolve(field);
				if (tsProperty != null)
					tsInterface.Properties.Add(tsProperty);
			}

			// process methods
			foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
			{
				var tsFunction = Resolve(method);
				if (tsFunction != null)
					tsInterface.Functions.Add(tsFunction);
			}

			return tsInterface;
		}

		/// <summary>
		/// Generates a TypeScript enum for a particular CLR enum type
		/// </summary>
		/// <param name="type">The enum type</param>
		/// <returns>The resulting TypeScript enum</returns>
		private TsEnum GenerateEnum(ITypeSymbol type)
		{

			var fields = type.GetMembers().OfType<IFieldSymbol>();
			var entries = new Dictionary<string, long?>();

			long lastValue = 0;

			foreach (var field in fields)
			{
				var syntaxReference = field.DeclaringSyntaxReferences.FirstOrDefault();
				if (syntaxReference == null)
				{
					//If syntaxReference is null then the enum comes from another projects which are not in the current 
					//Project.
					
					return new TsEnum(GetName(type), entries, true);
				}
				var parent = _compilation.GetSemanticModel(syntaxReference.SyntaxTree);
				var syn = syntaxReference.GetSyntax();
				var enumValue = syn.DescendantNodes().OfType<LiteralExpressionSyntax>().SingleOrDefault();
				if (enumValue != null)
				{
					Optional<object> constantValue = parent.GetConstantValue(enumValue);
					if (constantValue.HasValue)
					{
						lastValue = Convert.ToInt64(constantValue.Value);
						entries.Add(field.Name, lastValue);
					}
					else
					{
						entries.Add(field.Name, ++lastValue);
					}
				}
				else
				{
					entries.Add(field.Name, ++lastValue);
				}
			}

			//The Enum can be defined within a class. In that case the class is a 
			//namespace and the enum will be generated in another module (Since we cannot
			//define an enum within a typescript interface.
			bool isExternallyDefined = !(type.ContainingSymbol is INamespaceSymbol);

			var tsEnum = new TsEnum(GetName(type), entries, isExternallyDefined);
			AddType(tsEnum, type);
			return tsEnum;
		}

		/// <summary>
		/// Gets the TypeScript type name for a particular type
		/// </summary>
		/// <param name="type">The type</param>
		/// <returns>The TypeScript type name</returns>
		protected virtual TsName GetName(ITypeSymbol type)
		{
			const char genericNameSymbol = '`';
			var typeName = type.Name;
			if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
			{
				if (typeName.Contains(genericNameSymbol))
					typeName = typeName.Substring(0, typeName.IndexOf(genericNameSymbol));
			}

			return new TsName(typeName, type.ContainingSymbol.Name);
		}

		/// <summary>
		/// Gets the TypeScript name for a CLR method
		/// </summary>
		/// <param name="member">The member</param>
		/// <returns>The TypeScript name</returns>
		protected virtual TsName GetName(ISymbol member)
		{
			return new TsName(member.Name, member.ContainingSymbol.Name);
		}

		#endregion

		#region Type Resolution
		/// <summary>
		/// Resolves a type
		/// </summary>
		/// <param name="type">The type to resolve</param>
		/// <returns>The TypeScript type definition</returns>
		protected TsType Resolve(INamedTypeSymbol type)
		{
			// see if we have already processed the type
			if (!TypeLookup.TryGetValue(type, out var tsType))
				tsType = OnResolve(type);

			if (tsType == null) return null;

			AddType(tsType, type);
			return tsType;

		}

		protected TsType Resolve(ITypeSymbol type)
		{
			// see if we have already processed the type
			if (!TypeLookup.TryGetValue(type, out var tsType))
				tsType = OnResolve(type);

			if (tsType == null) return null;

			AddType(tsType, type);
			return tsType;

		}
		/// <summary>
		/// Resolves a type
		/// </summary>
		/// <param name="type">The type to resolve</param>
		/// <returns>The TypeScript type definition</returns>
		protected virtual TsType OnResolve(ITypeSymbol type)
		{
			if (TypeLookup.TryGetValue(type, out var tsType))
				return tsType;

			if (IsGuid(type))
				return TsPrimitive.String;

			else if (TypeFilter != null && !TypeFilter(type)) // should this assembly be considered?
				tsType = TsPrimitive.Any;
			else if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
			{
				if ($"{namedType.ContainingNamespace}.{namedType.MetadataName}" == typeof(Task<>).FullName)
				{
					//Ignore the type "Task" and use the type argument instead.
					tsType = Resolve(namedType.TypeArguments[0]);
				}
				else
				{
					var tsGenericType = new TsGenericType(new TsName(type.Name));
					foreach (var argument in namedType.TypeArguments)
					{
						var tsArgType = Resolve(argument);
						tsGenericType.TypeArguments.Add(tsArgType);
					}
					tsType = tsGenericType;
				}
			}
			else if (type.Kind == SymbolKind.ArrayType && type is IArrayTypeSymbol arrayType)
			{
				var elementType = Resolve(arrayType.ElementType);
				tsType = new TsArray(elementType, arrayType.Rank);
			}
			else if (type.TypeKind == TypeKind.Enum)
				tsType = GenerateEnum(type);
			else if (type.TypeKind == TypeKind.Class)
				tsType = GenerateInterface(type);
			else if (type.TypeKind == TypeKind.Struct)
				tsType = GenerateInterface(type);
			else if (type.TypeKind == TypeKind.Interface)
				tsType = GenerateInterface(type);
			else
				tsType = TsPrimitive.Any;

			return tsType;
		}


		/// <summary>
		/// Resolves a field
		/// </summary>
		/// <param name="field">The field to resolve</param>
		/// <returns></returns>
		protected virtual TsProperty Resolve(IFieldSymbol field)
		{
			if (field.DeclaredAccessibility != Accessibility.Public)
			{
				return null;
			}

			TsType fieldType;
			bool optional = false;

			if (field.Type is INamedTypeSymbol namedPropertyType && 
			    namedPropertyType.IsGenericType && 
			    namedPropertyType.MetadataName == "Nullable`1")
			{
				var genericArguments = namedPropertyType.TypeArguments;
				fieldType = Resolve((INamedTypeSymbol)genericArguments[0]);
				optional = true;
			}
			else
			{
				fieldType = Resolve(field.Type);
				optional = field.Type.SpecialType == SpecialType.System_Nullable_T;
			}

			if (fieldType != null)
			{
				return new TsProperty(GetName(field), fieldType, optional); ;
			}
			else
			{
				//Unable to resolve field type. Skip it.
				return null;
			}
		}

		/// <summary>
		/// Resolves a property
		/// </summary>
		/// <param name="property">The property to resolve</param>
		/// <returns></returns>
		protected virtual TsProperty Resolve(IPropertySymbol property)
		{
			if (property.DeclaredAccessibility != Accessibility.Public)
			{
				return null;
			}

			TsType propertyType;
			bool optional = false;
			if (property.Type is INamedTypeSymbol namedPropertyType &&
			    namedPropertyType.IsGenericType && 
			    namedPropertyType.MetadataName == "Nullable`1")
			{
				var genericArguments = namedPropertyType.TypeArguments;
				propertyType = Resolve((INamedTypeSymbol)genericArguments[0]);
				optional = true;
			}
			else
			{
				propertyType = Resolve(property.Type);
				optional = property.Type.SpecialType == SpecialType.System_Nullable_T;
			}

			if (propertyType != null)
			{
				return new TsProperty(GetName(property), propertyType, optional);
			}
			else
			{
				//Unable to resolve property type. Skip it!
				return null;
			}
		}

		/// <summary>
		/// Resolves a method
		/// </summary>
		/// <param name="method">The method to resolve</param>
		/// <returns>The TypeScript function definition</returns>
		protected virtual TsFunction Resolve(IMethodSymbol method)
		{
			//Methods are not currently supported
			return null;
			//if (method.MethodKind != MethodKind.Ordinary || method.DeclaredAccessibility != Accessibility.Public)
			//	return null;

			//Console.WriteLine(method.Name);
			//var returnType = Resolve(method.ReturnType);
			//var parameters = method.Parameters;
			//var tsFunction = new TsFunction(GetName(method));
			//tsFunction.ReturnType = returnType;
			//if (method.IsGenericMethod)
			//{
			//	foreach (var genericArgument in method.TypeArguments)
			//	{
			//		var tsTypeParameter = new TsTypeParameter(new TsName(genericArgument.Name));
			//		tsFunction.TypeParameters.Add(tsTypeParameter);
			//	}
			//}

			//foreach (var param in parameters.Select(x => new TsParameter(GetName(x), Resolve(x.Type))))
			//	tsFunction.Parameters.Add(param);

			//return tsFunction;
		}

		#endregion

		#region Modules
		/// <summary>
		/// Returns the list of modules associated with the current set of resolved types
		/// </summary>
		/// <returns>The list of modules</returns>
		public IEnumerable<TsModule> Modules()
		{
			return Types
				.GroupBy(x => x.Name.Namespace)
				.Where(x => !string.IsNullOrEmpty(x.Key))
				.Select(x => new TsModule(new TsName(x.Key), x));
		}
		#endregion

		#region Output
		/// <summary>
		/// Configures the scripter to use a particular formatter
		/// </summary>
		/// <param name="formatter">The formatter</param>
		/// <returns></returns>
		public Scripter UsingFormatter(TsFormatter formatter)
		{
			Formatter = formatter;
			return this;
		}

		/// <summary>
		/// Saves the scripter output to a file
		/// </summary>
		/// <param name="file">The file path</param>
		/// <returns></returns>
		public Scripter SaveToFile(string file)
		{
			System.IO.File.WriteAllText(file, ToString());
			return this;
		}

		/// <summary>
		/// Saves the scripter output to a directory
		/// </summary>
		/// <param name="directory">The directory path</param>
		/// <returns></returns>
		public Scripter SaveToDirectory(string directory)
		{
			
			var includeRef = $"import * as External from './include';{Environment.NewLine}";
			List<string> includeFiles = new List<string>();
			foreach (var module in Modules())
			{
				var fileName = module.Name.FullName + ".d.ts";
				var path = Path.Combine(directory, fileName);
				var output = Formatter.Format(module);
				File.WriteAllText(path, output);
				File.WriteAllText(path, includeRef + Environment.NewLine + output);
				includeFiles.Add(fileName);
			}

			CreateIncludeTsFile(includeFiles.ToImmutableList(), directory);

			// write the include file
			return this;
		}
		#endregion

		private void CreateIncludeTsFile(ImmutableList<string> includeFiles, string directory)
		{
			var includeContent = new StringBuilder();
			foreach (var includeFile in includeFiles)
			{
				includeContent.AppendFormat("export * from './{0}';", Path.GetFileNameWithoutExtension(includeFile));
				includeContent.AppendLine();
			}
			foreach (var externalType in Types.OfType<TsExternalReference>())
			{
				if (!includeFiles.Contains(externalType.FileName))
				{
					//Only add external filename if not already added.
					includeContent.AppendFormat("export * from './{0}';", Path.GetFileNameWithoutExtension(externalType.FileName));
					includeContent.AppendLine();
				}
			}

			File.WriteAllText(System.IO.Path.Combine(directory, "include.ts"), includeContent.ToString());
		}
	}
}
