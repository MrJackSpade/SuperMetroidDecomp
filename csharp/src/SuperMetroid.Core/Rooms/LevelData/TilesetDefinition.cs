namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>One nine-byte bank-$8F tileset definition selected by graphics-set index.</summary>
/// <param name="BlockDefinitionsAddress">Full 24-bit native source identity for the graphics set's four-word metatile definitions; used as an installed-artwork key.</param>
/// <param name="CharacterAddress">Full 24-bit native source identity for area-specific planar BG characters, uploaded after the shared CRE character region.</param>
/// <param name="PaletteAddress">Full 24-bit native source identity for the graphics set's BG palette; its first 128 colors are loaded by room graphics setup.</param>
public readonly record struct TilesetDefinition(
    int BlockDefinitionsAddress,
    int CharacterAddress,
    int PaletteAddress);
