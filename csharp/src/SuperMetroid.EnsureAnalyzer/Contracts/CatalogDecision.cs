using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>One decision or procedure a catalog may not hold: where it is and how a diagnostic names it.</summary>
/// <param name="Location">Where the decision is reported.</param>
/// <param name="What">How the diagnostic names the decision, such as "an 'if'".</param>
internal readonly record struct CatalogDecision(Location Location, string What);
