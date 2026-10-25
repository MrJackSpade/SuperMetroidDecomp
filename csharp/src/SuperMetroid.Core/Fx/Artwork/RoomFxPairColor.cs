using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>One of the first two blend colors, separating shared tint rules from edits.</summary>
internal sealed class RoomFxPairColor
{
    /// <summary>Native blend selector used by the shared component calculations.</summary>
    private readonly RoomFxPaletteBlend selection;
    /// <summary>Distinguishes the first ink from the second ink's tint rules.</summary>
    private readonly bool isPrimary;
    /// <summary>User red component when it differs from the calculated value.</summary>
    internal readonly int? redOverride;
    /// <summary>User green component when it differs from the calculated value.</summary>
    internal readonly int? greenOverride;
    /// <summary>User blue component when it differs from the calculated value.</summary>
    internal readonly int? blueOverride;

    /// <summary>Stores only RGB components that depart from a selector's shared tint rules.</summary>
    public RoomFxPairColor(RoomFxPaletteBlend selection, RgbColor color, RoomFxPairMember member)
    {
        this.selection = selection;
        this.isPrimary = member == RoomFxPairMember.Primary;
        int red = color.Red, green = color.Green, blue = color.Blue;
        redOverride = RoomFxPaletteBlendDefinitions.CalculatedPairRed(selection, member == RoomFxPairMember.Primary) == red ? null : red;
        greenOverride = RoomFxPaletteBlendDefinitions.CalculatedPairGreen(selection, red, member == RoomFxPairMember.Primary) == green ? null : green;
        blueOverride = RoomFxPaletteBlendDefinitions.CalculatedPairBlue(selection, red, green, member == RoomFxPairMember.Primary) == blue ? null : blue;
    }

    /// <summary>Rebuilds the ink from retained edits and any still-applicable shared tint rules.</summary>
    public RgbColor CreateColor()
    {
        int red = redOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairRed(selection, isPrimary) ??
            throw new InvalidOperationException("A blend color without a red edit has a calculated red.");
        int green = greenOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairGreen(selection, red, isPrimary) ??
            throw new InvalidOperationException("A blend color without a green edit has a calculated green.");
        int blue = blueOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairBlue(selection, red, green, isPrimary) ??
            throw new InvalidOperationException("A blend color without a blue edit has a calculated blue.");
        return new RgbColor(red, green, blue);
    }
}
