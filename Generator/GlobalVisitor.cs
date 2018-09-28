using Microsoft.CodeAnalysis;
using TypeScripter;

namespace ClassLibrary1
{
	public class GetAllSymbolsVisitor : SymbolVisitor
	{
		private Scripter _scripter;

		public GetAllSymbolsVisitor(Scripter scripter)
		{
			_scripter = scripter;
		}

		public override void VisitNamespace(INamespaceSymbol symbol)
		{
			foreach (var typeSymbol in symbol.GetMembers())
			{
				typeSymbol.Accept(this);
			}
		}

		public override void VisitNamedType(INamedTypeSymbol symbol)
		{
			_scripter.AddType(symbol);
		}

	}
}