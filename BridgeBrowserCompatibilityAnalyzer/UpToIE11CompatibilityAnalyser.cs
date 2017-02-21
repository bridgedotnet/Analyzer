using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BridgeBrowserCompatibilityAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class UpToIE11CompatibilityAnalyser : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return SelectedOptionsAccess.Diagnostics;
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			SelectedOptionsAccess.Register(context);
		}
	}
}
