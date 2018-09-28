using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using TypeScripter;
using TypeScripter.TypeScript;

namespace ClassLibrary1
{
    public class Analyser
    {
		public static Compilation compilation { get; private set; }
	    public static async Task Open()
	    {
		    var msWorkspace = MSBuildWorkspace.Create();

		    msWorkspace.WorkspaceFailed += (object sender, WorkspaceDiagnosticEventArgs e) =>
		    {
			    Debug.WriteLine(e.Diagnostic.ToString());
		    };
			string solutionPath = @"C:\Users\u045992\source\Projects\TestedLibrary\TestedLibrary.sln";

		    try
			{
				var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;
				

				var proj = solution.Projects.Single(p => p.Name == "TargetLibrary");

				compilation = proj.GetCompilationAsync().Result;

				var typeByMetadataName = compilation.GetTypeByMetadataName(typeof(Guid).FullName);

				Scripter scripter = new Scripter(Analyser.compilation);
				scripter.TypeFilter = t => t.Name != "IExtensibleDataObject" && t.Name != "ClientBase";
				scripter.WithTypeMapping(new TsExternalReference(nameof(DayOfWeek), "TypeScriptModels.ts"), typeof(DayOfWeek));
				GetAllSymbolsVisitor visitor = new GetAllSymbolsVisitor(scripter);
				visitor.Visit(compilation.Assembly.GlobalNamespace);
				scripter.SaveToDirectory(@"c:\temp\TypeScriptOutput");
			}
			catch (Exception ex)
			{

			}
		}


	}
}
