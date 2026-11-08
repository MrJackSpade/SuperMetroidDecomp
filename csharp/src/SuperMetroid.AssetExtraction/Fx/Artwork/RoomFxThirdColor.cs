using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>Independent third-color red and calculated weather green/blue, preserving edits.</summary>
internal sealed class RoomFxThirdColor
{
    /// <summary>Selector used to calculate the weather ink's shared channels.</summary>
    internal readonly RoomFxPaletteBlend selection;
    /// <summary>Independent red channel of the third ink.</summary>
    internal readonly int red;
    /// <summary>Edited green channel when the weather rule does not match.</summary>
    internal readonly int? greenOverride;
    /// <summary>Edited blue channel when the weather rule does not match.</summary>
    internal readonly int? blueOverride;

    /// <summary>Retains independent red and only the green/blue components that override weather rules.</summary>
    public RoomFxThirdColor(RoomFxPaletteBlend selection, RgbColor color)
    {
        this.selection = selection;
        FadeLevelColour levels = color.FadeLevels;
        red = levels.Red;
        int green = levels.Green;
        greenOverride = RoomFxPaletteBlendRules.CalculatedThirdGreen(selection) == green ? null : green;
        int blue = levels.Blue;
        blueOverride = RoomFxPaletteBlendRules.CalculatedThirdBlue(selection, levels.Red) == blue ? null : blue;
    }

    /// <summary>Combines the retained red channel with edited or calculated weather channels.</summary>
    internal RgbColor CreateColor() => new FadeLevelColour(red,
        greenOverride ?? RoomFxPaletteBlendRules.CalculatedThirdGreen(selection) ??
            throw new InvalidOperationException("A blend color without a green edit has a calculated green."),
        blueOverride ?? RoomFxPaletteBlendRules.CalculatedThirdBlue(selection, red) ??
            throw new InvalidOperationException("A blend color without a blue edit has a calculated blue.")).ToColour();
}
