using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A named location.</summary>
/// <param name="Name">The name.</param>
/// <param name="Location">The location.</param>
public readonly record struct NamedLocation(string Name, Location Location);
