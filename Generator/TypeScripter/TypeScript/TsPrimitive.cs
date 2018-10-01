using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScripter
{
	public enum PrimitiveType
	{
		Undefined = 0,
		Void,
		Boolean = 0,
		Number,
		String,
		Date,
		Object,
		Array,
	}

	/// <summary>
	/// A class representing a TypeScript primitive
	/// </summary>
	public class TsPrimitive : TsType
    {
	    public PrimitiveType PrimitiveType { get; }

	    #region Primitive Definitions
        /// <summary>
        /// TypeScript any 
        /// </summary>
        public static readonly TsPrimitive Any = new TsPrimitive(new TsName("any"), PrimitiveType.Object);
        /// <summary>
        /// TypeScript void
        /// </summary>
        public static readonly TsPrimitive Void = new TsPrimitive(new TsName("void"), PrimitiveType.Void);
        /// <summary>
        /// TypeScript boolean
        /// </summary>
        public static readonly TsPrimitive Boolean = new TsPrimitive(new TsName("boolean"), PrimitiveType.Boolean);
        /// <summary>
        /// TypeScript number
        /// </summary>
        public static readonly TsPrimitive Number = new TsPrimitive(new TsName("number"), PrimitiveType.Number);
        /// <summary>
        /// TypeScript string
        /// </summary>
        public static readonly TsPrimitive String = new TsPrimitive(new TsName("string"), PrimitiveType.String);
	    /// <summary>
	    /// TypeScript Date
	    /// </summary>
		public static readonly TsPrimitive Date = new TsPrimitive(new TsName("Date"), PrimitiveType.Date);
		/// <summary>
		/// TypeScript undefined
		/// </summary>
		public static readonly TsPrimitive Undefined = new TsPrimitive(new TsName("undefined"), PrimitiveType.Undefined);
		
        #endregion

        #region Creation

	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="name">The type name</param>
	    /// <param name="primitiveType">The type of primitive</param>
	    protected TsPrimitive(TsName name, PrimitiveType primitiveType)
            : base(name)
        {
	        PrimitiveType = primitiveType;
        }
        #endregion
    }
}
