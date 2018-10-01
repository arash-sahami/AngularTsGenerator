using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TypeScripter
{
    /// <summary>
    /// The base class for all TypeScript objects
    /// </summary>
    public abstract class TsObject
    {
        #region Properties
        /// <summary>
        /// The name of the type
        /// </summary>
        public TsName Name
        {
            get;
            private set;
        }

		/// <summary>
		/// Gets whether the type is defined in another module.
		/// </summary>
	    public bool IsExternallyDefined { get; }

	    #endregion

		#region Creation

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The name of the type</param>
		/// <param name="isExternallyDefined">This indicates that the generated type must always contain the namespace prefix.</param>
		protected TsObject(TsName name, bool isExternallyDefined = false)
		{
			this.Name = name;
			IsExternallyDefined = isExternallyDefined;
		}
        #endregion

        #region Methods
        /// <summary>
        /// The ToString implementation
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            var name = this.Name;
            if (name != null)
                return name.ToString();
            return base.ToString();
        }
        #endregion
    }
}
