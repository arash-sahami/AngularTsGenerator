using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TypeScripter;

namespace MetaDataModels
{
	[JsonConverter(typeof(MetadataInfoModelConverter))]
	public class MetadataInfoModel
	{
		readonly List<PropertyInfoModel> _propertyInfoModels = new List<PropertyInfoModel>();
		public string Name { get; private set; }

		public IEnumerable<PropertyInfoModel> Properties => _propertyInfoModels.AsEnumerable();

		private MetadataInfoModel()
		{
		}

		public static MetadataInfoModel GetMetaDataInfoModel(TsInterface type, Compilation compilation)
		{
			MetadataInfoModel model = new MetadataInfoModel {Name = $"{type.Name.Name}Metadata"};

			foreach (var property in type.Properties)
			{
				model._propertyInfoModels.Add(property.CreatePropertyInfoModel(compilation));
			}

			return model;
		}
	}

	public class MetadataInfoModelConverter : JsonConverter
	{
		public override bool CanRead => false;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is MetadataInfoModel metadataInfoModel)
			{
				var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

				writer.WriteStartObject();
				writer.WritePropertyName(ToCamelCase(nameof(MetadataInfoModel.Name)));
				writer.WriteValue(metadataInfoModel.Name);

				foreach (var propertyInfoModel in metadataInfoModel.Properties)
				{
					writer.WritePropertyName(propertyInfoModel.Name);

					writer.WriteRawValue(JsonConvert.SerializeObject(propertyInfoModel, jsonSerializerSettings));
				}

				writer.WriteEndObject();
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(MetadataInfoModel);
		}

		private string ToCamelCase(string str)
		{
			if (string.IsNullOrEmpty(str) == false)
			{
				return Char.ToLowerInvariant(str[0]) + str.Substring(1);
			}

			return string.Empty;
		}
	}
}