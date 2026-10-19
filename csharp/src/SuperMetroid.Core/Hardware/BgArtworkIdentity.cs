namespace SuperMetroid.Core.Hardware;

/// <summary>Names the owned artwork a background binding shows: a catalog entry, an installed asset or a source (#1350).</summary>
/// <param name="Name">The artwork's identity.</param>
public readonly record struct BgArtworkIdentity(string Name);
