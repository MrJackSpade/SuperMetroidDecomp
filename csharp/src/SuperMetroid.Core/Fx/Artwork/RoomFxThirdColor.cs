using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

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
        red = color.Red;
        int green = color.Green;
        greenOverride = RoomFxPaletteBlendRules.CalculatedThirdGreen(selection) == green ? null : green;
        int blue = color.Blue;
        blueOverride = RoomFxPaletteBlendRules.CalculatedThirdBlue(selection, color.Red) == blue ? null : blue;
    }
}
