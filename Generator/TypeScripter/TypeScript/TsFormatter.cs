using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaDataModels;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TypeScripter
{
    /// <summary>
    /// A class which is responsible for rendering the TypeScript output
    /// </summary>
    public class TsFormatter
    {
	    private readonly bool _generateTypeWithNamespace;

	    #region Internal Constructs
        private class StringBuilderContext : IDisposable
        {
            StringBuilderContext PriorContext
            {
                get;
                set;
            }

            public StringBuilder StringBuilder
            {
                get;
                set;
            }

            public int IndentLevel
            {
                get;
                set;
            }

            TsFormatter Writer
            {
                get;
                set;
            }

            public StringBuilderContext(TsFormatter writer)
            {
                Writer = writer;
                PriorContext = writer.Context;
                IndentLevel = PriorContext != null ? PriorContext.IndentLevel : 0;
                StringBuilder = new StringBuilder();
                Writer.Context = this;
            }

            public override string ToString()
            {
                return StringBuilder.ToString();
            }

            void IDisposable.Dispose()
            {
                if (PriorContext != null)
                {
                    Writer.Context = PriorContext;
                    PriorContext = null;
                }
            }
        }

        private class IndentContext : IDisposable
        {
            private TsFormatter mFormatter;
            public IndentContext(TsFormatter formatter)
            {
                mFormatter = formatter;
                mFormatter.Context.IndentLevel++;
            }

            void IDisposable.Dispose()
            {
                if (mFormatter != null)
                {
                    mFormatter.Context.IndentLevel--;
                    mFormatter = null;
                }
            }
        }
        #endregion

        #region Properties
        private StringBuilderContext Context
        {
            get;
            set;
        }

        /// <summary>
        /// The mapping of reserved words
        /// </summary>
        public IDictionary<string, string> ReservedWordsMapping
        {
            get;
            private set;
        }

        /// <summary>
        /// Enums are represented as strings not as numbers
        /// </summary>
        public bool EnumsAsString
        {
            get; set;
        }
        #endregion

        #region Creation
        /// <summary>
        /// Constructor
        /// </summary>
        public TsFormatter(bool generateTypeWithNamespace)
        {
	        _generateTypeWithNamespace = generateTypeWithNamespace;
	        Context = new StringBuilderContext(this);
            ReservedWordsMapping = new Dictionary<string, string>()
            {
                {"function","_function"}
            };
        }
        #endregion

        #region Writer
        /// <summary>
        /// Formats a module
        /// </summary>
        /// <param name="module">The module</param>
        /// <returns>The string representation of the module</returns>
        public virtual string Format(TsModule module, Compilation compilation)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                WriteNewline();
                //using (Indent())
                {
	                foreach (var type in module.Types.OfType<TsEnum>().OrderBy(x => x.Name))
	                {
		                Write(Format(type));
	                }
	                foreach (var type in module.Types.OfType<TsInterface>().OrderBy(x => x.Name))
	                {
		                Write(Format(type));
		                WriteNewline();
						Write(Format(type.GetMetaDataInfo(compilation)));
		                WriteNewline();
					}
                }
                WriteNewline();
                return sbc.ToString();
            }
        }

        /// <summary>
        /// Formats an interface
        /// </summary>
        /// <param name="tsInterface">The interface</param>
        /// <returns>The string representation of the interface</returns>
        public virtual string Format(TsInterface tsInterface)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                if (tsInterface.IsLiteral)
                {
                    Write("{");
                    foreach (var property in tsInterface.Properties.OrderBy(x => x.Name))
                        Write(Format(property));

                    foreach (var property in tsInterface.IndexerProperties.OrderBy(x => x.Name))
                        Write(Format(property));

                    foreach (var function in tsInterface.Functions.OrderBy(x => x.Name))
                        Write(Format(function));

                    Write("}");
                    return sbc.ToString();
                }
                else
                {
                    WriteIndent();
                    Write("export interface {0}{1} {2} {{",
                        Format(tsInterface.Name),
                        Format(tsInterface.TypeParameters),
                        tsInterface.BaseInterfaces.Count > 0 ? string.Format("extends {0}", string.Join(", ", tsInterface.BaseInterfaces.OrderBy(x => x.Name).Select(Format))) : string.Empty);
                    WriteNewline();
                    using (Indent())
                    {
                        foreach (var property in tsInterface.Properties.OrderBy(x => x.Name))
                        {
                            WriteIndent();
                            Write(Format(property));
                            WriteNewline();
                        }

                        foreach (var property in tsInterface.IndexerProperties.OrderBy(x => x.Name))
                        {
                            WriteIndent();
                            Write(Format(property));
                            WriteNewline();
                        }

                        foreach (var function in tsInterface.Functions.OrderBy(x => x.Name))
                        {
                            WriteIndent();
                            Write(Format(function));
                            WriteNewline();
                        }
                    }
                    WriteIndent();
                    Write("}");
                    WriteNewline();
                    WriteNewline();
                    return sbc.ToString();
                }
            }
        }

		/// <summary>
		/// Formats the metadata info for.
		/// </summary>
		/// <param name="infoModel"></param>
		/// <returns></returns>
		public virtual string Format(MetadataInfoModel infoModel)
		{
			var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			return JsonConvert.SerializeObject(infoModel, jsonSerializerSettings);
		}

		/// <summary>
		/// Formats a property
		/// </summary>
		/// <param name="property">The property</param>
		/// <returns>The string representation of the property</returns>
		public virtual string Format(TsProperty property)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                Write("{0}{1}: {2};", Format(property.Name), property.Optional?"?":"", Format(property.Type));
                return sbc.ToString();
            }
        }
        
        /// <summary>
        /// Formats an indexer property
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns></returns>
        public virtual string Format(TsIndexerProperty property)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                Write("[{0}: {1}]: {2};", Format(property.Name), Format(property.IndexerType), Format(property.ReturnType));
                return sbc.ToString();
            }
        }

        /// <summary>
        /// Formats a function
        /// </summary>
        /// <param name="function">The function</param>
        /// <returns>The string representation of the function</returns>
        public virtual string Format(TsFunction function)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                Write("{0}{1}({2}){3};",
                    Format(function.Name),
                    Format(function.TypeParameters),
                    Format(function.Parameters),
                    function.ReturnType == TsPrimitive.Any ? string.Empty : string.Format(": {0}", FormatReturnType(function.ReturnType))
                );
                return sbc.ToString();
            }
        }

        /// <summary>
        /// Formats a return type
        /// </summary>
        /// <param name="tsReturnType">The return type</param>
        /// <returns>The string representation of the return type</returns>
        public virtual string FormatReturnType(TsType tsReturnType)
        {
            return Format(tsReturnType);
        }

        /// <summary>
        /// Formats a type
        /// </summary>
        /// <param name="tsType">The type</param>
        /// <returns>The string representation of the type</returns>
        public virtual string Format(TsType tsType)
        {
	        if (tsType == null)
	        {
		        return string.Empty;
	        }

            if (tsType is TsGenericType)
                return Format((TsGenericType)tsType);
            var tsInterface = tsType as TsInterface;
            if (tsInterface != null && tsInterface.IsLiteral)
                return Format(tsInterface);

	        return ResolveTypeName(tsType.Name, tsType.IsExternallyDefined);
        }

        /// <summary>
        /// Formats an enumeration
        /// </summary>
        /// <param name="tsEnum">The enumeration</param>
        /// <returns>The string representation of the enumeration</returns>
        public virtual string Format(TsEnum tsEnum)
        {
            if (EnumsAsString)
            {
                return FormatEnumAsStrings(tsEnum);
            }
            else
            {
                return FormatEnumAsIntegers(tsEnum);
            }
        }

        /// <summary>
        /// Formats an enumeration as string
        /// </summary>
        /// <param name="tsEnum">The enumeration</param>
        /// <returns>The string representation of the enumeration</returns>
        protected string FormatEnumAsStrings(TsEnum tsEnum)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                WriteIndent();
                Write("type {0} = ", Format(tsEnum.Name));
                var values = tsEnum.Values.OrderBy(x => x.Key).ToArray();
                for (int i = 0; i < values.Length; i++)
                {
                    var postFix = i < values.Length - 1 ? " | " : string.Empty;
                    var entry = values[i];
                    Write("\'{0}\'{1}", entry.Key, postFix);
                }
                Write(";");
                WriteNewline();
                return sbc.ToString();
            }
        }


        /// <summary>
        /// Formats an enumaration as integers
        /// </summary>
        /// <param name="tsEnum">The enumeration</param>
        /// <returns>The string representation of the enumeration</returns>
        protected string FormatEnumAsIntegers(TsEnum tsEnum)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                WriteIndent();
                Write("export const enum {0} {{", Format(tsEnum.Name));
                WriteNewline();
                using (Indent())
                {
                    var values = tsEnum.Values.OrderBy(x => x.Key).ToArray();
                    for (int i = 0; i < values.Length; i++)
                    {
                        var postFix = i < values.Length - 1 ? "," : string.Empty;
                        var entry = values[i];
                        WriteIndent();
                        if (entry.Value.HasValue)
                            Write("{0} = {1}{2}", entry.Key, entry.Value, postFix);
                        else
                            Write("{0}{1}", entry.Key, postFix);
                        WriteNewline();
                    }
                }
                WriteIndent();
                Write("}");
                WriteNewline();
                return sbc.ToString();
            }
        }

        /// <summary>
        /// Formats a parameter
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <returns>The string representation of a parameter</returns>
        public virtual string Format(TsParameter parameter)
        {
            using (var sbc = new StringBuilderContext(this))
            {
                Write("{0}{1}: {2}", Format(parameter.Name), parameter.Optional ? "?" : string.Empty, Format(parameter.Type));
                return sbc.ToString();
            }
        }

        /// <summary>
        /// Formats a set of parameters
        /// </summary>
        /// <param name="parameters">The parameters</param>
        /// <returns>The string representation of the parameters</returns>
        public virtual string Format(IEnumerable<TsParameter> parameters)
        {
            return string.Join(", ", parameters.Select(Format));
        }

        /// <summary>
        /// Formats a type parameter
        /// </summary>
        /// <param name="typeParameter">The type parameter</param>
        /// <returns>The string representation of the type parameter</returns>
        public virtual string Format(TsTypeParameter typeParameter)
        {
            return $"{ResolveTypeName(typeParameter.Name, typeParameter.IsExternallyDefined)}{(typeParameter.Extends == null ? string.Empty : $" extends {ResolveTypeName(typeParameter.Extends, typeParameter.IsExternallyDefined)}")}";
        }

	    private string ResolveTypeName(TsName tsName, bool isExternallyDefined)
	    {
		    StringBuilder sb = new StringBuilder();
		    if (_generateTypeWithNamespace)
		    {
			    sb.Append(tsName.FullName);
		    }
		    else if (isExternallyDefined)
		    {
			    sb.Append(tsName.ExternalName);
		    }
		    else
		    {
			    sb.Append(tsName.Name);
		    }

		    return sb.ToString();
	    }

		/// <summary>
		/// Formats a set of type parameters
		/// </summary>
		/// <param name="typeParameters">The type parameters</param>
		/// <returns>The string representation fo the type parameters</returns>
		public virtual string Format(IEnumerable<TsTypeParameter> typeParameters)
        {
            if (!typeParameters.Any())
                return string.Empty;
            return $"<{string.Join(", ", typeParameters.Select(Format))}>";
        }

        /// <summary>
        /// Formats a generic type
        /// </summary>
        /// <param name="tsGenericType">The generic type</param>
        /// <returns>The string representation of the generic type</returns>
        public virtual string Format(TsGenericType tsGenericType)
        {
            return
	            $"{ResolveTypeName(tsGenericType.Name, tsGenericType.IsExternallyDefined)}{(tsGenericType.TypeArguments.Count > 0 ? $"<{string.Join(", ", tsGenericType.TypeArguments.Select(Format))}>" : string.Empty)}";
        }

        /// <summary>
        /// Formats a name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The string representation of the name</returns>
        public virtual string Format(TsName name)
        {
            if (name == null || name.Name == null)
                return string.Empty;
            string result = null;
            if (!ReservedWordsMapping.TryGetValue(name.Name, out result))
                result = name.Name;
            return result;
        }
        #endregion

        #region Methods
        private void Write(string output)
        {
            Context.StringBuilder.Append(output);
        }

        private void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        private void WriteIndent()
        {
            var indent = string.Empty;
            for (int i = 0; i < Context.IndentLevel; i++)
                indent += "\t";
            Write(indent);
        }

        private void WriteNewline()
        {
            Write(Environment.NewLine);
        }

        private IndentContext Indent()
        {
            return new IndentContext(this);
        }
        #endregion
    }
}
