using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TargetLibraryMetaData;

namespace MetaDataModels.PropertyDecorators
{
	public static class DefaultPropertyValueAttribute
	{
		public static bool IsDefaultValueAttribute(Compilation compilation, AttributeData attributeData, out object value)
		{
			value = null;
			var defaultValueAttribute = compilation.GetTypeByMetadataName(typeof(System.ComponentModel.DefaultValueAttribute).FullName);
			if (defaultValueAttribute == null)
			{
				throw new InvalidOperationException($"Could not get type {nameof(System.ComponentModel.DefaultValueAttribute)} from the compiler.");
			}
			if (attributeData.AttributeClass.Equals(defaultValueAttribute))
			{
				if (attributeData.ConstructorArguments.Length == 1)
				{
					value = attributeData.ConstructorArguments[0].Value;
				}

				return true;
			}

			return false;
		}
	}


	public static class DateTimePropertyValueAttribute
	{
		public static bool IsDefaultValueAttribute(Compilation compilation, AttributeData attributeData, out DateTimeValidatorModel value)
		{
			value = null;
			var dateTimeAttribute = compilation.GetTypeByMetadataName(typeof(DateTimeAttribute).FullName);
			if (dateTimeAttribute == null)
			{
				throw new InvalidOperationException($"Could not get type {nameof(DateTimeAttribute)} from the compiler.");
			}
			if (attributeData.AttributeClass.Equals(dateTimeAttribute))
			{
				value = new DateTimeValidatorModel();
				if (DateTime.TryParse((string) attributeData.ConstructorArguments[0].Value, out DateTime minTime))
				{
					value.Min = minTime;
				}

				if (attributeData.ConstructorArguments.Length > 1)
				{
					if (DateTime.TryParse((string) attributeData.ConstructorArguments[1].Value, out DateTime maxTime))
					{
						value.Max = maxTime;
					}
				}

				var keyValuePairs = attributeData.NamedArguments.Where(arg => arg.Key == nameof(DateTimeAttribute.Max)).ToArray();
				if (keyValuePairs.Length > 0)
				{
					if (DateTime.TryParse((string)keyValuePairs[0].Value.Value, out DateTime maxTime))
					{
						value.Max = maxTime;
					}
				}

				return true;
			}

			return false;
		}
	}
}