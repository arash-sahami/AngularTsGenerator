using System.Collections.Generic;

namespace TypeScripter.TypeScript
{
    /// <summary>
    /// A class representing a TypeScript enumeration
    /// </summary>
    public sealed class TsEnum : TsType
    {
        #region Properties
        /// <summary>
        /// The enumeration values
        /// </summary>
        public IDictionary<string, long?> Values
        {
            get;
            set;
        }
		#endregion

		#region Creation

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The enum name</param>
		/// <param name="values">The enum values</param>
		/// <param name="isExternallyDefined">This indicates that the generated type must always contain the namespace prefix.</param>
		public TsEnum(TsName name, IDictionary<string, long?> values, bool isExternallyDefined = false)
            : base(name, isExternallyDefined)
        {
            this.Values = values;
        }
        #endregion
    }
}
