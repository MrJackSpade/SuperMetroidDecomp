namespace SuperMetroid.Core.Hardware;

/// <summary>A catalog owner's full name and the name of one of its cells.</summary>
/// <param name="FullName">The full name.</param>
/// <param name="Name">The name.</param>
internal readonly record struct OwnedCellName(string? FullName, string Name);
