using System;

namespace TypeScripter
{
	public class LowerCamelCaseFormatter : TsFormatter
	{
		public LowerCamelCaseFormatter(bool generateTypeWithNamespace) : base(generateTypeWithNamespace)
		{
		}

		public override string Format(TsProperty property)
		{
			var result = base.Format(property);
			return char.ToLower(result[0]) + (result.Length == 1 ? string.Empty : result.Substring(1));
		}

	}
}