namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>What each way of colouring a sheet's pixels draws its characters in (<see cref="SpriteImageColours"/>).</summary>
internal static class SpriteImageColourDefinitions
{
    /// <summary>The colours one set of a two-bit sheet holds.</summary>
    private const int TwoBitSetColours = SuperMetroid.AssetExtraction.Rooms.RoomTileCanvas.TwoBppRowColours;

    /// <summary>The colours one set of a four-bit or full-palette sheet holds.</summary>
    private const int FourBitSetColours = SuperMetroid.AssetExtraction.Rooms.RoomTileCanvas.RowColours;

    /// <summary>
    /// Each colouring: the HUD layer's sets draw two-bit characters, four colours a set; the sprite, background and
    /// full-palette sheets draw characters of sixteen colours a set.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpriteImageColours, SpriteImageColourDefinition> Colourings =
        new Dictionary<SpriteImageColours, SpriteImageColourDefinition>
        {
            [SpriteImageColours.Sprite] = new(TwoBitCharacters: false, FourBitSetColours),
            [SpriteImageColours.Background] = new(TwoBitCharacters: false, FourBitSetColours),
            [SpriteImageColours.TwoBppBackground] = new(TwoBitCharacters: true, TwoBitSetColours),
            [SpriteImageColours.FullPalette] = new(TwoBitCharacters: false, FourBitSetColours),
        };
}
