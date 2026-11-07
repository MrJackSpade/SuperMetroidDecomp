using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>One of the first two blend colors, separating shared tint rules from edits.</summary>
internal sealed class RoomFxPairColor
{
    /// <summary>Blend selector used by the shared component calculations.</summary>
    internal readonly RoomFxPaletteBlend selection;
    /// <summary>Distinguishes the first ink from the second ink's tint rules.</summary>
    internal readonly bool isPrimary;
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
        redOverride = RoomFxPaletteBlendRules.CalculatedPairRed(selection, member == RoomFxPairMember.Primary) == red ? null : red;
        greenOverride = RoomFxPaletteBlendRules.CalculatedPairGreen(selection, red, member == RoomFxPairMember.Primary) == green ? null : green;
        blueOverride = RoomFxPaletteBlendRules.CalculatedPairBlue(selection, red, green, member == RoomFxPairMember.Primary) == blue ? null : blue;
    }
}
