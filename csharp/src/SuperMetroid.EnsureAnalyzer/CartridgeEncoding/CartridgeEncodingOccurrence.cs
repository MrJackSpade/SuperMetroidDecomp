using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>One occurrence: its rule, proposed class, the basis for that class, and where it is.</summary>
internal readonly record struct CartridgeEncodingOccurrence(
    CartridgeEncodingRule Rule,
    CartridgeEncodingClass Class,
    string Basis,
    Location Location);
