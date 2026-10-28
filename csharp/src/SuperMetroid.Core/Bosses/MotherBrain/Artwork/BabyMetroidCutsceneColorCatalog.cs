using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable initial and six fade RGB5 images for the Mother Brain cutscene Baby.</summary>
public sealed class BabyMetroidCutsceneColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => BabyMetroidFade.ContentIdentity(this);

    /// <summary>The selected initial image and death fade.</summary>
    internal readonly BabyMetroidCutsceneColorSelection selection;

    /// <summary>Copies compiled images into immutable catalog-owned palette views.</summary>
    internal BabyMetroidCutsceneColorCatalog(RgbColor[] initial, RgbColor[][] fade) =>
        selection = new(new BabyMetroidInitialPalette(initial), new BabyMetroidFade(fade));

    /// <summary>Resolves one initial Baby Metroid sprite color, corresponding to the fifteen-word image at <c>$A9:94D4</c>; no CGRAM write or cutscene state change occurs.</summary>
    /// <param name="color">Zero-based image index from zero through 14, corresponding to colors 1..15 of OBJ palette seven; the transparent slot is excluded.</param>
    /// <returns>The image color.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is outside the fifteen-color initial image.</exception>
    public RgbColor InitialColor(int color) => selection.Initial.Resolve(color);

    /// <summary>Resolves one displayed death fade-to-black color from native <c>BabyMetroidFadingToBlackPalettes_1</c> through <c>_6</c> at <c>$AD:E90C-$E9B3</c>, without advancing the fade timer.</summary>
    /// <param name="paletteIndex">One-based displayed image selector from one through six; the unused native image zero is not installed.</param>
    /// <param name="color">Zero-based image index from zero through 13, corresponding to OBJ palette seven colors 1..14; transparent color zero and black color 15 are excluded.</param>
    /// <returns>The selected image color, retaining independently supplied artwork edits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either selector is outside the installed fade images; the exception names <paramref name="paletteIndex"/> for both invalid cases.</exception>
    public RgbColor FadeColor(int paletteIndex, int color) => selection.Fade.Displayed(paletteIndex, color);
}
