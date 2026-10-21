namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>Two names that pair as components, and whether they are lower-case.</summary>
/// <param name="From">The from.</param>
/// <param name="To">The to.</param>
/// <param name="Lower">The lower.</param>
internal readonly record struct PartnerNames(string From, string To, bool Lower);
