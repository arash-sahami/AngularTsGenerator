namespace TypeScripter.TypeScript
{
	public class TsExternalReference : TsType
	{
		public string FileName { get; }

		public TsExternalReference(string name, string fileName) : base(new TsName(name), true)
		{
			FileName = fileName;
		}
	}
}