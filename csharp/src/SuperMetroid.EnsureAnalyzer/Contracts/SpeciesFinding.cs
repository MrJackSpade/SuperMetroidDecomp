using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A species-identity finding and what it found.</summary>
/// <param name="Location">The location.</param>
/// <param name="What">The what.</param>
internal readonly record struct SpeciesFinding(Location Location, string What);
