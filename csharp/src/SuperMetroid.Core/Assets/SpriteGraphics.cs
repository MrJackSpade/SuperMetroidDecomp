namespace SuperMetroid.Core.Assets;

/// <summary>
/// Where on a shared sprite sheet a sprite takes its characters, and the sprite palette row it draws in: what an enemy,
/// or a projectile or sprite object it spawns, carries so its compositions draw from the right characters and colours.
/// </summary>
/// <param name="TileBase">The sheet character its compositions' parts count from, 0 through <see cref="SpritePart.MaximumCharacter"/>.</param>
/// <param name="PaletteRow">The sprite palette row it draws in, 0 through <see cref="SpritePart.MaximumPalette"/>.</param>
public readonly record struct SpriteGraphics(int TileBase, int PaletteRow)
{
    /// <summary>Character zero in palette row <paramref name="row"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the eight sprite palette rows.</exception>
    public static SpriteGraphics InPalette(int row) => Of(0, row);

    /// <summary>A validated tile base and palette row.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The tile base is outside the sheet or the row outside the eight rows.</exception>
    public static SpriteGraphics Of(int tileBase, int paletteRow) =>
        new(Ensure.BetweenInclusive(0, SpritePart.MaximumCharacter, tileBase),
            Ensure.BetweenInclusive(0, SpritePart.MaximumPalette, paletteRow));

    /// <summary>The same characters drawn in palette row <paramref name="row"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the eight sprite palette rows.</exception>
    public SpriteGraphics InRow(int row) => Of(TileBase, row);
}
