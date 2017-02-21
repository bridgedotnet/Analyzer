using BridgeBrowserCompatibilityAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace UnitTests
{
	public sealed class SelectedOptionsAnalyserTests : DiagnosticVerifier
	{
		[Test]
		public void SelectedOptionsDirectlyAccessedOnAssignedInstance()
		{
			var testContent = @"
				using Bridge;

				namespace TestCase
				{
					public class Example
					{
						public void Go()
						{
							var x = new HTMLSelectElement();
							var o = x.SelectedOptions;
						}
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = Constants.AnalyzerDiagnosticId,
				Message = SelectedOptionsAccess.DoNotAccessSelectedOptionsPropertyRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 11, 18)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		[Test]
		public void SelectedOptionsDirectlyAccessedOnNewInstance()
		{
			var testContent = @"
				using Bridge;

				namespace TestCase
				{
					public class Example
					{
						public void Go()
						{
							var o = new HTMLSelectElement().SelectedOptions;
						}
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = Constants.AnalyzerDiagnosticId,
				Message = SelectedOptionsAccess.DoNotAccessSelectedOptionsPropertyRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 10, 40)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		[Test]
		public void SelectedOptionsAccessedAsNestedProperty()
		{
			var testContent = @"
				using Bridge;

				namespace TestCase
				{
					public class Example
					{
						public void Go()
						{
							var x = new MySelect();
							var o = x.Value.SelectedOptions;
						}
					}
					public class MySelect
					{
						public HTMLSelectElement Value { get; }
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = Constants.AnalyzerDiagnosticId,
				Message = SelectedOptionsAccess.DoNotAccessSelectedOptionsPropertyRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 11, 24)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		[Test]
		public void PropertyOnSelectedOptionsReferenceAccessed()
		{
			var testContent = @"
				using Bridge;

				namespace TestCase
				{
					public class Example
					{
						public void Go()
						{
							var x = new MySelect();
							var o = x.Value.SelectedOptions.Length;
						}
					}
					public class MySelect
					{
						public HTMLSelectElement Value { get; }
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = Constants.AnalyzerDiagnosticId,
				Message = SelectedOptionsAccess.DoNotAccessSelectedOptionsPropertyRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 11, 24)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		/// <summary>
		/// The analyser should only warn if the SelectedOptions property is accessed on the Bridge HTMLSelectElement class - if code declares its own HTMLSelectElement class then it's
		/// not up to us to judge it
		/// </summary>
		[Test]
		public void AccessingSelectedOptionsOnNonBridgeHTMLSelectElementClassIsAcceptable()
		{
			var testContent = @"
				namespace TestCase
				{
					public class Example
					{
						public void Go()
						{
							var x = new HTMLSelectElement();
							var o = x.SelectedOptions;
						}
					}

					public class HTMLSelectElement
					{
						public SelectedOptions Value { get; }
					}
				}";

			VerifyCSharpDiagnostic(testContent);
		}

		/// <summary>
		/// I think that the SelectedOptions property should be read only (which would mean that it couldn't be accessed during MySelect initialisation) but that's not what we've
		/// got at this point in time (Feb 2017)
		/// </summary>
		[Test]
		public void ObjectInitializerSpecifiesSelectedOptionsProperty()
		{
			var testContent = @"
				using Bridge;

				namespace TestCase
				{
					public class Example
					{
						public void Go()
						{
							var x = new HTMLSelectElement
							{
								SelectedOptions = null
							};
						}
					}
				}";

			var expected = new DiagnosticResult
			{
				Id = Constants.AnalyzerDiagnosticId,
				Message = SelectedOptionsAccess.DoNotAccessSelectedOptionsPropertyRule.MessageFormat.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", 12, 9)
				}
			};

			VerifyCSharpDiagnostic(testContent, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new UpToIE11CompatibilityAnalyser();
		}
	}
}
