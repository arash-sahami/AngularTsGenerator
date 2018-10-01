using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using TypeScripter;

namespace ClassLibrary1
{
	public class Analyser
	{

		public static void Open()
		{
			Compilation compilation;
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

				//CreateMetaDataTypescript();
				var typeByMetadataName = compilation.GetTypeByMetadataName(typeof(Guid).FullName);

				Scripter scripter = new Scripter(compilation);
				scripter.TypeFilter = t => t.Name != "IExtensibleDataObject" && t.Name != "ClientBase";
				scripter.WithTypeMapping(new TsExternalReference(nameof(DayOfWeek), "TypeScriptModels.ts"),
					typeof(DayOfWeek));
				GetAllSymbolsVisitor visitor = new GetAllSymbolsVisitor(scripter);
				visitor.Visit(compilation.Assembly.GlobalNamespace);
				scripter.SaveToDirectory(@"c:\temp\TypeScriptOutput");
			}
			catch (Exception)
			{

			}
		}


		public static void CreateMetaDataTypescript()
		{
			//TODO Arash: How to add this file to output.
			var fileToCompile = @"C:\Users\u045992\source\repos\AngularTsGenerator\Generator\TypeScripter\TypeScript\TypeScriptObject\ValidatorModelBase.cs";
			var source = File.ReadAllText(fileToCompile);
			var parsedSyntaxTree = Parse(source, "", CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp5));
			var compilation = CSharpCompilation.Create("Test.dll", new SyntaxTree[] { parsedSyntaxTree }, DefaultReferences, DefaultCompilationOptions);
			Scripter scripter = new Scripter(compilation);
			GetAllSymbolsVisitor visitor = new GetAllSymbolsVisitor(scripter);
			visitor.Visit(compilation.Assembly.GlobalNamespace);
			scripter.SaveToDirectory(@"c:\temp\TypeScriptOutput");

		}

		public static SyntaxTree Parse(string text, string filename = "", CSharpParseOptions options = null)
		{
			var stringText = SourceText.From(text, Encoding.UTF8);
			return SyntaxFactory.ParseSyntaxTree(stringText, options, filename);
		}

		private static string runtimePath = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1\{0}.dll";

		private static readonly CSharpCompilationOptions DefaultCompilationOptions =
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
				.WithOverflowChecks(true).WithOptimizationLevel(OptimizationLevel.Release)
				.WithUsings(DefaultNamespaces);

		private static readonly IEnumerable<MetadataReference> DefaultReferences =
			new[]
			{
				MetadataReference.CreateFromFile(string.Format(runtimePath, "mscorlib")),
				MetadataReference.CreateFromFile(string.Format(runtimePath, "System")),
				MetadataReference.CreateFromFile(string.Format(runtimePath, "System.Core"))
			};

		private static readonly IEnumerable<string> DefaultNamespaces =
			new[]
			{
				"System",
				"System.IO",
				"System.Net",
				"System.Linq",
				"System.Text",
				"System.Text.RegularExpressions",
				"System.Collections.Generic"
			};

	}
}
