using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BridgeBrowserCompatibilityAnalyzer
{
	public static class SelectedOptionsAccess
	{
		private const string _typeName = "HTMLSelectElement";
		private const string _propertyName = "SelectedOptions";

		public static DiagnosticDescriptor DoNotAccessSelectedOptionsPropertyRule = new DiagnosticDescriptor(
			Constants.AnalyzerDiagnosticId,
			GetLocalizableString(nameof(Resources.SelectedOptionsAccessedTitle)),
			GetLocalizableString(nameof(Resources.SelectedOptionsAccessedDetails)),
			Constants.AnalyzerCategory,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		public static ImmutableArray<DiagnosticDescriptor> Diagnostics
		{
			get { return ImmutableArray.Create(DoNotAccessSelectedOptionsPropertyRule); }
		}

		public static void Register(AnalysisContext context)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			context.RegisterSyntaxNodeAction(LookForSelectedOptionsPropertyAccess, SyntaxKind.SimpleMemberAccessExpression);
			context.RegisterSyntaxNodeAction(LookForSelectedOptionsAccessInObjectInitialisation, SyntaxKind.ObjectInitializerExpression);
		}

		private static void LookForSelectedOptionsPropertyAccess(SyntaxNodeAnalysisContext context)
		{
			var memberAccess = context.Node as MemberAccessExpressionSyntax;
			if (memberAccess == null)
				return;

			// First, look for a member access that specifies "SelectedOptions" as its target - we can do this entirely with the syntax tree, which means that the work is very
			// cheap. If we find a "SelectedOptions" access then we'll have to request the semantic model, which is more expensive.
			if (memberAccess.Name.Identifier.Text != _propertyName)
				return;

			var selectedOptionsSymbol = context.SemanticModel.GetSymbolInfo(memberAccess.Name).Symbol;
			if (selectedOptionsSymbol == null)
				return; // If couldn't resolve without ambiguity then give the benefit of the doubt

			// Check whether the type that owns the property is HTMLSelectElement (we don't have to worry about class that are derived from HTMLSelectElement because
			// HTMLSelectElement is sealed)
			var selectedOptionsContainingType = selectedOptionsSymbol.ContainingType;
			if ((selectedOptionsContainingType == null) || !selectedOptionsContainingType.IsBridgeClass(_typeName))
				return; // The containing type is not HTMLSelectElement within Bridge so it's not up to us to record any warnings

			context.ReportDiagnostic(Diagnostic.Create(
				DoNotAccessSelectedOptionsPropertyRule,
				memberAccess.Name.GetLocation()
			));
		}

		private static void LookForSelectedOptionsAccessInObjectInitialisation(SyntaxNodeAnalysisContext context)
		{
			var initializer = context.Node as InitializerExpressionSyntax;
			if (initializer == null)
				return;

			var selectedOptionsPropertyInitializer = initializer.ChildNodes()
				.OfType<AssignmentExpressionSyntax>()
				.FirstOrDefault(propertyInitializer =>
				{
					var propertyName = propertyInitializer.Left as IdentifierNameSyntax;
					return (propertyName != null) && (propertyName.Identifier.ValueText == _propertyName);
				});
			if (selectedOptionsPropertyInitializer == null)
				return;

			var parentObjectCreation = initializer.Parent as ObjectCreationExpressionSyntax;
			if (parentObjectCreation == null)
				return;

			// Check whether the type that owns the property is HTMLSelectElement (we don't have to worry about class that are derived from HTMLSelectElement because
			// HTMLSelectElement is sealed)
			var typeToCreate = context.SemanticModel.GetTypeInfo(parentObjectCreation);
			if ((typeToCreate.Type == null) && !typeToCreate.Type.IsBridgeClass(_typeName))
				return;

			context.ReportDiagnostic(Diagnostic.Create(
				DoNotAccessSelectedOptionsPropertyRule,
				selectedOptionsPropertyInitializer.GetLocation()
			));
		}

		private static LocalizableString GetLocalizableString(string nameOfLocalizableResource)
		{
			return new LocalizableResourceString(nameOfLocalizableResource, Resources.ResourceManager, typeof(Resources));
		}
	}
}
