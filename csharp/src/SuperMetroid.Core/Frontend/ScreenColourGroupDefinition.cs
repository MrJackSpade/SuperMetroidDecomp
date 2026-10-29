using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where a frontend screen holds one of its colour groups today (#1464): a run of <paramref name="ColourCount"/> colours
/// from colour <paramref name="FirstColour"/> of row <paramref name="Row"/> of the <paramref name="Kind"/> colours. A run may
/// continue into the following rows. Only <see cref="ScreenColourGroups"/> reads it.
/// </summary>
/// <param name="Kind">Whether the group lies among the background or the sprite colours.</param>
/// <param name="Row">The row the group starts in.</param>
/// <param name="FirstColour">The group's first colour within that row.</param>
/// <param name="ColourCount">The number of colours in the group.</param>
internal readonly record struct ScreenColourGroupDefinition(ScreenColourKind Kind, int Row, int FirstColour, int ColourCount)
{
    /// <summary>A whole background row.</summary>
    public static ScreenColourGroupDefinition BackgroundRow(int row) =>
        new(ScreenColourKind.Background, row, 0, ScreenPalette.ColoursPerRow);

    /// <summary>A whole sprite row.</summary>
    public static ScreenColourGroupDefinition SpriteRow(int row) =>
        new(ScreenColourKind.Sprite, row, 0, ScreenPalette.ColoursPerRow);

    /// <summary>Whether the group is exactly one whole sprite row, the colours a sprite draws in.</summary>
    public bool IsWholeSpriteRow => Kind == ScreenColourKind.Sprite && FirstColour == 0 && ColourCount == ScreenPalette.ColoursPerRow;
}
