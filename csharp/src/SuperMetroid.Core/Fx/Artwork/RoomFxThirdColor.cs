namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Independent third-color red and calculated weather green/blue, preserving edits.</summary>
internal sealed class RoomFxThirdColor
{
    /// <summary>Native selector used to calculate the weather ink's shared channels.</summary>
    private readonly RoomFxPaletteBlend selection;
    /// <summary>Independent red channel of the third ink.</summary>
    private readonly int red;
    /// <summary>Edited green channel when the weather rule does not match.</summary>
    internal readonly int? greenOverride;
    /// <summary>Edited blue channel when the weather rule does not match.</summary>
    internal readonly int? blueOverride;

    /// <summary>Retains independent red and only the green/blue components that override weather rules.</summary>
    public RoomFxThirdColor(RoomFxPaletteBlend selection, Bgr555 color)
    {
        this.selection = selection;
        red = color.Red;
        int green = color.Green;
        greenOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdGreen(selection) == green ? null : green;
        int blue = color.Blue;
        blueOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdBlue(selection, color.Red) == blue ? null : blue;
    }

    /// <summary>Combines the retained red channel with edited or calculated weather channels.</summary>
    public Bgr555 CreateColor() => new(red,
        greenOverride ?? RoomFxPaletteBlendDefinitions.CalculatedThirdGreen(selection) ??
            throw new InvalidOperationException("A blend color without a green edit has a calculated green."),
        blueOverride ?? RoomFxPaletteBlendDefinitions.CalculatedThirdBlue(selection, red) ??
            throw new InvalidOperationException("A blend color without a blue edit has a calculated blue."));
}
