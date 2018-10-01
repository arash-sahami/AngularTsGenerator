using MetaDataModels;
using MetaDataModels.PropertyDecorators;
using Microsoft.CodeAnalysis;

namespace TypeScripter
{
    /// <summary>
    /// A class representing a TypeScript property
    /// </summary>
    public class TsProperty : TsObject
    {
		#region Properties
		/// <summary>
		/// A flag which indicates the property is optional
		/// </summary>
		public bool Optional
        {
            get;
            set;
        }

        /// <summary>
        /// The property type
        /// </summary>
        public TsType Type
        {
            get;
            set;
        }

	    public ISymbol Symbol { get; }

	    #endregion

		#region Creation

	    public TsProperty(TsName name, TsType type, ISymbol symbol, bool optional = false)
		    : base(name)
	    {
		    Type = type;
		    Optional = optional;
		    Symbol = symbol;

		}

		#endregion

		public PropertyInfoModel CreatePropertyInfoModel(Compilation compilation)
	    {
		    PropertyInfoModel propertyInfoModel = new PropertyInfoModel
		    {
				Name = Name.Name,
			    IsOptional = Optional,
			    IsArray = Type is TsArray,
				Type = GetPropertyTypeEnum(this)
				
		    };

		    //Optional

		    //Validators
		    foreach (AttributeData attributeData in Symbol.GetAttributes())
		    {
			    if (DefaultPropertyValueAttribute.IsDefaultValueAttribute(compilation, attributeData, out object defaultValue))
			    {
				    propertyInfoModel.DefaultValue = defaultValue;
			    }
				else if (DateTimePropertyValueAttribute.IsDefaultValueAttribute(compilation, attributeData,
				    out DateTimeValidatorModel validatorModel))
			    {
				    propertyInfoModel.Add(validatorModel);

				}
		    }

			return propertyInfoModel;
	    }

	    private static PropertyTypeEnum GetPropertyTypeEnum(TsProperty property)
	    {
		    if (property.Type is TsPrimitive primitiveType)
		    {
			    switch (primitiveType.PrimitiveType)
			    {
					case PrimitiveType.Number:
						return PropertyTypeEnum.Number;
					case PrimitiveType.String:
						return PropertyTypeEnum.String;
					case PrimitiveType.Date:
						return PropertyTypeEnum.Date;
					case PrimitiveType.Boolean:
						return PropertyTypeEnum.Boolean;
				}
		    }
		    else if (property.Type is TsEnum)
		    {
			    return PropertyTypeEnum.Enum;
		    }

		    return PropertyTypeEnum.Object;
	    }
	}
}
