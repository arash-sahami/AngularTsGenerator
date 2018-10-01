using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MetaDataModels
{
	public enum PropertyTypeEnum
	{
		Boolean = 0,
		Number,
		String,
		Date,
		Object,
		Enum
	}

	//[JsonConverter(typeof(PropertyInfoModelConverter))]
	public class PropertyInfoModel
	{
		private readonly List<ValidatorModelBase> _validators = new List<ValidatorModelBase>();
		public string Name { get; set; }
		public IEnumerable<ValidatorModelBase> Validators => _validators.AsEnumerable();
		public PropertyTypeEnum Type { get; set; }
		public object DefaultValue { get; set; }
		public bool IsArray { get; set; }
		public bool IsOptional { get; set; }

		public void Add(ValidatorModelBase model)
		{
			_validators.Add(model);
		}
	}

	public class PropertyInfoModelConverter : JsonConverter
	{
		public override bool CanRead => false;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is PropertyInfoModel propertyInfoModel)
			{
				writer.WriteStartObject();

				var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

				writer.WritePropertyName(nameof(PropertyInfoModel.Name));
				writer.WriteValue(propertyInfoModel.Name);

				writer.WritePropertyName(nameof(PropertyInfoModel.Type));
				writer.WriteValue(propertyInfoModel.Type);

				writer.WritePropertyName(nameof(PropertyInfoModel.DefaultValue));
				writer.WriteValue(propertyInfoModel.DefaultValue);

				writer.WritePropertyName(nameof(PropertyInfoModel.IsArray));
				writer.WriteValue(propertyInfoModel.IsArray);

				writer.WritePropertyName(nameof(PropertyInfoModel.IsOptional));
				writer.WriteValue(propertyInfoModel.IsOptional);


				writer.WritePropertyName(nameof(PropertyInfoModel.Validators));
				foreach (var validator in propertyInfoModel.Validators)
				{
					writer.WriteRawValue(JsonConvert.SerializeObject(validator, jsonSerializerSettings));
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
			throw new NotImplementedException();
		}
	}
}