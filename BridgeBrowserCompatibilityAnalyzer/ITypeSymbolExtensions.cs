using System;
using Microsoft.CodeAnalysis;

namespace BridgeBrowserCompatibilityAnalyzer
{
	public static class ITypeSymbolExtensions
	{
		/// <summary>
		/// There is technically a chance that someone could be using this analyser with a custom Bridge reference and we wouldn't know the difference because we're only checking the name of the
		/// assembly here but that's an edge case and it feels like someone deliberately getting up to monkey business if they're combining this analyser with their own Bridge assembly and so
		/// let's not worry about them (actually, this is what is done within the unit tests since they use a mock Bridge assembly)
		/// </summary>
		public static bool IsBridgeClass(this ITypeSymbol type, string className)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrWhiteSpace(className))
				throw new ArgumentException($"Null/blank {nameof(className)} specified");

			return
				(type.ContainingAssembly.Identity.Name == "Bridge") &&
				(type.Name == className);
		}
	}
}
