namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One named visual frame selected by an otherwise compiled enemy program.</summary>
internal readonly record struct EnemySpritemapDefinition(byte Bank, ushort Pointer, string Name);
