using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>The symbols an assignment writes and the value it writes.</summary>
/// <param name="Targets">The targets.</param>
/// <param name="Value">The value.</param>
internal readonly record struct IdentityAssignment(IEnumerable<ISymbol> Targets, IOperation? Value);
