namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>Where text holding an enum member's name ends up (SME6460, SME6462).</summary>
/// <param name="Name">The installed key's member or parameter name, or the collection whose keys the text becomes.</param>
/// <param name="IsCollection">Whether the text becomes a collection's key (SME6462) rather than an installed key (SME6460).</param>
internal readonly record struct KeyDestination(string Name, bool IsCollection);
