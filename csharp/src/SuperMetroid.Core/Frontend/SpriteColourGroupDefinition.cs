using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>A colour group among a screen's sprite colours.</summary>
/// <param name="Row">The sprite row the group starts in.</param>
/// <param name="FirstColour">The group's first colour within that row.</param>
/// <param name="ColourCount">The number of colours in the group.</param>
internal sealed record SpriteColourGroupDefinition(int Row, int FirstColour, int ColourCount)
    : ScreenColourGroupDefinition(Row, FirstColour, ColourCount)
{
    /// <summary>A whole sprite row, the colours a sprite draws in.</summary>
    public static SpriteColourGroupDefinition WholeRow(int row) => new(row, 0, ScreenPalette.ColoursPerRow);
}
