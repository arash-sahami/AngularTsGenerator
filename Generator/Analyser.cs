using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using TypeScripter;
using TypeScripter.TypeScript;

namespace ClassLibrary1
{
    public class Analyser
    {
		public static Compilation Compilation { get; private set; }
	    public static void Open()
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

				var proj = solution.Projects.Single();
				Compilation = proj.GetCompilationAsync().Result;

				Scripter scripter = new Scripter(Analyser.Compilation);
				scripter.TypeFilter = t => t.Name != "IExtensibleDataObject" && t.Name != "ClientBase";
				scripter.WithTypeMapping(new TsExternalReference(nameof(DayOfWeek), "TypeScriptModels.ts"), typeof(DayOfWeek));
				GetAllSymbolsVisitor visitor = new GetAllSymbolsVisitor(scripter);
				visitor.Visit(Compilation.Assembly.GlobalNamespace);
				scripter.SaveToDirectory(@"c:\temp\TypeScriptOutput");
			}
			catch (Exception ex)
			{

			}
		}

    }
}
