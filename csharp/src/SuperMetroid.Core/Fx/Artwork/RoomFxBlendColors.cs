namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Loaded artwork inks with calculated components separated from arbitrary edits.</summary>
/// <remarks>
/// Native89:AA02+selector supplies primary/secondary/background inks copied by89:AB5E
/// to BG3 palette25..27. The remaining24 independent channels specify each theme's
/// chosen hue/brightness, not samples of time or distance. Index interpolation or
/// fitted color transforms would merely recite those choices as code. Their specific
/// artistic-content/nonsense dispositions are in roomFxIndependentInkReview in the
/// issue1165 inventory;48 uniform/shared channels are calculated independently.
/// </remarks>
internal sealed class RoomFxBlendColors
{
    /// <summary>First BG3 ink, with shared tint rules separated from user overrides.</summary>
    internal readonly RoomFxPairColor primary;
    /// <summary>Second BG3 ink, with shared tint rules separated from user overrides.</summary>
    internal readonly RoomFxPairColor secondary;
    /// <summary>Third ink only when it differs from the selector's calculated color.</summary>
    internal readonly RoomFxThirdColor? thirdOverride;

    /// <summary>Separates selected colors into calculated components and explicit edits.</summary>
    public RoomFxBlendColors(RoomFxPaletteBlend selection, Bgr555 primary, Bgr555 secondary, Bgr555 third)
    {
        this.primary = new(selection, primary, true);
        this.secondary = new(selection, secondary, false);
        thirdOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdColor(selection) == third ? null : new(selection, third);
    }

    /// <summary>Writes the three resulting blend inks to BG3 palette entries 25 through 27.</summary>
    public void Apply(SnesCgram cgram)
    {
        new RoomFxPalettes(cgram).Layer3Blend.Write(0, primary.CreateColor());
        new RoomFxPalettes(cgram).Layer3Blend.Write(1, secondary.CreateColor());
        new RoomFxPalettes(cgram).Layer3Blend.Write(2, thirdOverride?.CreateColor() ?? Bgr555.Black);
    }
}
