using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>Loaded artwork inks with calculated components separated from arbitrary edits.</summary>
/// <remarks>
/// The blend's three colours are the effect overlay's primary, secondary and background inks. Its independent channels
/// specify each theme's chosen hue and brightness, not samples of time or distance; the uniform and shared channels are
/// calculated by <see cref="RoomFxPaletteBlendRules"/>.
/// </remarks>
internal sealed class RoomFxBlendColors
{
    /// <summary>First effect overlay ink, with shared tint rules separated from supplied colours.</summary>
    internal readonly RoomFxPairColor primary;
    /// <summary>Second effect overlay ink, with shared tint rules separated from supplied colours.</summary>
    internal readonly RoomFxPairColor secondary;
    /// <summary>Third ink only when it differs from the selector's calculated color.</summary>
    internal readonly RoomFxThirdColor? thirdOverride;

    /// <summary>Separates selected colors into calculated components and explicit edits.</summary>
    public RoomFxBlendColors(RoomFxPaletteBlend selection, RgbColor primary, RgbColor secondary, RgbColor third)
    {
        this.primary = new(selection, primary, RoomFxPairMember.Primary);
        this.secondary = new(selection, secondary, RoomFxPairMember.Secondary);
        thirdOverride = RoomFxPaletteBlendRules.CalculatedThirdColor(selection) == third ? null : new(selection, third);
    }

    /// <summary>The three resulting blend colours, in order.</summary>
    internal IReadOnlyList<RgbColor> Colours() =>
        [primary.CreateColor(), secondary.CreateColor(), thirdOverride?.CreateColor() ?? RgbColor.Black];
}
