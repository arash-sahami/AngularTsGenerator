using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TypeScripter
{
	public class TypeSymbolEqualityComparer : IEqualityComparer<ITypeSymbol>
	{
		public TypeSymbolEqualityComparer()
		{
		}

		public bool Equals(ITypeSymbol x, ITypeSymbol y)
		{
			return x.ContainingSymbol?.Name == y.ContainingSymbol?.Name && x.Name.Equals(y.Name);
		}

		public int GetHashCode(ITypeSymbol obj)
		{
			unchecked
			{
				var hashCode = 13;
				var nameHashCode = !string.IsNullOrEmpty(obj.MetadataName) ? obj.MetadataName.GetHashCode().GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ nameHashCode;
				var namespaceHashCode = !string.IsNullOrEmpty(obj.ContainingSymbol?.Name) ? obj.ContainingSymbol?.Name.GetHashCode() : 0;

				hashCode = (hashCode * 397) ^ namespaceHashCode.GetHashCode();
				return hashCode;
			}
		}
	}
}