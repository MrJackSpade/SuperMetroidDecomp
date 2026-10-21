namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>A composition named by where the cartridge stores it, as verification oracles derive it.</summary>
internal readonly record struct NativeEnemySpritemapDefinition(byte Bank, ushort Pointer, string Name);
