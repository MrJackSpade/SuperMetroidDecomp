using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Loaded artwork inks with calculated components separated from arbitrary edits.</summary>
/// <remarks>
/// Native89:AA02+selector supplies primary/secondary/background inks copied by89:AB5E
/// to the effect overlay's palette 25..27. The remaining24 independent channels specify each theme's
/// chosen hue/brightness, not samples of time or distance. Index interpolation or
/// fitted color transforms would merely recite those choices as code. Their specific
/// artistic-content/nonsense dispositions are in roomFxIndependentInkReview in the
/// issue1165 inventory;48 uniform/shared channels are calculated independently.
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
    public IReadOnlyList<RgbColor> Colours() =>
        [primary.CreateColor(), secondary.CreateColor(), thirdOverride?.CreateColor() ?? RgbColor.Black];
}
