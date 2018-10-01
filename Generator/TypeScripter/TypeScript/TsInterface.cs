using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaDataModels;
using Microsoft.CodeAnalysis;

namespace TypeScripter
{
    /// <summary>
    /// A class representing a function
    /// </summary>
    public sealed class TsInterface : TsType
    {
        #region Properties
        /// <summary>
        /// The interface properties
        /// </summary>
        public IList<TsProperty> Properties
        {
            get;
            private set;
        }

        /// <summary>
        /// The interface indexer properties
        /// </summary>
        public IList<TsIndexerProperty> IndexerProperties
        {
            get;
            private set;
        }

        /// <summary>
        /// The interface functions
        /// </summary>
        public IList<TsFunction> Functions
        {
            get;
            private set;
        }

        /// <summary>
        /// The base interfaces
        /// </summary>
        public IList<TsType> BaseInterfaces
        {
            get;
            private set;
        }

        /// <summary>
        /// The type parameters
        /// </summary>
        public IList<TsTypeParameter> TypeParameters
        {
            get;
            private set;
        }

        /// <summary>
        /// A flag which indicates whether the interface is used as a object type literal
        /// </summary>
        public bool IsLiteral
        {
            get
            {
                return this.Name == TsName.None;
            }
        }
		#endregion

		#region


	    #endregion

		#region Creation
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The interface name</param>
		public TsInterface(TsName name)
            : base(name)
        {
            TypeParameters = new List<TsTypeParameter>();
            BaseInterfaces = new List<TsType>();
            Properties = new List<TsProperty>();
            IndexerProperties = new List<TsIndexerProperty>();
            Functions = new List<TsFunction>();
		}

        /// <summary>
        /// Constructs an interface to be used as a object type literal
        /// </summary>
        public TsInterface()
            : this(TsName.None)
        {
        }
		#endregion

	    public MetadataInfoModel GetMetaDataInfo(Compilation compilation)
	    {
			return MetadataInfoModel.GetMetaDataInfoModel(this, compilation);
	    }
	}
}
