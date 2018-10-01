using System;

namespace MetaDataModels
{
	public enum ValidatorTypeEnum
	{
		None = 0,
		Required,
		StringLength,
		Range,
		Date
	}

	public abstract class ValidatorModelBase
	{
		public ValidatorTypeEnum ValidatorType { get; }

		protected ValidatorModelBase(ValidatorTypeEnum type)
		{
			ValidatorType = type;
		}
	}

	public class DateTimeValidatorModel : ValidatorModelBase
	{
		public DateTime? Min { get; set; }
		public DateTime? Max { get; set; }


		public DateTimeValidatorModel() : base(ValidatorTypeEnum.Date)
		{
		}

		public static ValidatorModelBase ParseFromTypeWriterString(string value)
		{
			var dvm = new DateTimeValidatorModel();

			if (value == null) return dvm;

			var attrValues = value.Split(',');

			foreach (var item in attrValues)
			{
				Console.WriteLine(item);
				if (DateTime.TryParse(item.Split('=')[1].Replace("\"", string.Empty).Trim(), out DateTime result))
				{
					dvm.Min = result;
				}
			}

			return dvm;
		}
	}

	public class RangeValidatorModel : ValidatorModelBase
	{
		public int Min { get; private set; }
		public int Max { get; private set; }
		public RangeValidatorModel() : base(ValidatorTypeEnum.Range)
		{
		}

		/// <summary>
		/// Parsing the string representation of <see cref="RangeValidatorModel"/> into an instance.
		/// The format is following [min], [max]
		/// </summary>
		/// <returns>Instance of a validator.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RangeValidatorModel", Justification = "The error message is not going to be localized.")]
		public static ValidatorModelBase ParseFromTypeWriterString(string value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value), "The input string cannot be null.");

			var attrValues = value.Split(',');
			if (attrValues.Length != 2)
			{
				throw new InvalidOperationException("Failed to convert the input string into an instance of " + nameof(RangeValidatorModel) + ".");
			}

			var rvm = new RangeValidatorModel
			{
				Min = Int32.Parse(attrValues[0]),
				Max = Int32.Parse(attrValues[1])
			};

			return rvm;
		}
	}

	public class RequiredValidatorModel : ValidatorModelBase
	{
		public RequiredValidatorModel()
			: base(ValidatorTypeEnum.Required)
		{
		}

		public static ValidatorModelBase ParseFromTypeWriterString(string value)
		{
			throw new System.NotImplementedException();
		}
	}

	public class StringLengthValidatorModel : ValidatorModelBase
	{
		public int MaxLength { get; private set; }
		public int? MinLength { get; private set; }
		public StringLengthValidatorModel() : base(ValidatorTypeEnum.StringLength)
		{
		}

		/// <summary>
		/// Parsing the string representation of <see cref="StringLengthValidatorModel"/> into an instance.
		/// The format is following [maxlength], MinimumLength = [minlength]
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StringLengthValidatorModel", Justification = "The error string is not localized in this case.")]
		public static ValidatorModelBase ParseFromTypeWriterString(string value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value), "The input string cannot be null.");

			var attrValues = value.Split(',');
			if (attrValues.Length == 0)
			{
				throw new InvalidOperationException($"Failed to convert the input string to instance of {nameof(StringLengthValidatorModel)}.");
			}

			var svm = new StringLengthValidatorModel { MaxLength = Int32.Parse(attrValues[0]) };
			for (int i = 1; i < attrValues.Length; i++)
			{
				if (int.TryParse(attrValues[1].Split('=')[1], out int result))
				{
					svm.MinLength = result;
				}
			}

			return svm;
		}
	}
}