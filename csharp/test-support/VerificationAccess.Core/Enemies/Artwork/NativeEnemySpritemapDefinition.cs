namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// A composition named by where the cartridge stores it, as verification oracles derive it: the name composition documents
/// gave it before frames were named for their meaning (#1465).
/// </summary>
internal readonly record struct NativeEnemySpritemapDefinition(byte Bank, ushort Pointer, string Name);
