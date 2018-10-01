using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScripter
{
    /// <summary>
    /// The base class for all TypeScript types
    /// </summary>
    public abstract class TsType : TsObject
    {
	    #region Creation

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The name of the type</param>
		/// <param name="isExternallyDefined">This indicates that the generated type must always contain the namespace prefix.</param>
		protected TsType(TsName name, bool isExternallyDefined = false)
            : base(name, isExternallyDefined)
		{
		}
        #endregion

        #region Method
        /// <summary>
        /// The ToString implementation
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return this.Name.FullName;
        }
        #endregion
    }
}
