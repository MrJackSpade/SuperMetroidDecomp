using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>The installed initial and six fade RGB5 images for the Mother Brain cutscene Baby, kept as installed.</summary>
public sealed class BabyMetroidCutsceneColorCatalog
{
    /// <summary>The installed initial image.</summary>
    private readonly InstalledColourRows initial;
    /// <summary>The installed fade images, the first displayed fade image first.</summary>
    private readonly InstalledColourRows fade;

    /// <summary>Keeps the complete installed initial image and fade images.</summary>
    /// <param name="initial">The initial image's colours.</param>
    /// <param name="fade">Each displayed fade image's colours, in display order.</param>
    /// <exception cref="InvalidDataException">An image or a colour is missing.</exception>
    internal BabyMetroidCutsceneColorCatalog(RgbColor[] initial, RgbColor[][] fade)
    {
        this.initial = new InstalledColourRows(initial, BabyMetroidCutsceneColorFormat.InitialColorCount, "Cutscene Baby initial");
        this.fade = new InstalledColourRows(fade, BabyMetroidCutsceneColorDefinitions.FadeFrameCount,
            BabyMetroidCutsceneColorFormat.FadeColorCount, "Cutscene Baby fade");
    }

    /// <summary>Resolves one installed initial Baby Metroid sprite color, corresponding to the fifteen-color image; no colour write or cutscene state change occurs.</summary>
    /// <param name="color">Zero-based image index from zero through 14, corresponding to colors 1..15 of sprite palette seven; the transparent slot is excluded.</param>
    /// <returns>The image color.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is outside the fifteen-color initial image.</exception>
    public RgbColor InitialColor(int color) => initial.Colour(color);

    /// <summary>Resolves one installed displayed death fade-to-black color, without advancing the fade timer.</summary>
    /// <param name="paletteIndex">One-based displayed image selector from one through six; the unused image zero is not installed.</param>
    /// <param name="color">Zero-based image index from zero through 13, corresponding to OBJ palette seven colors 1..14; transparent color zero and black color 15 are excluded.</param>
    /// <returns>The installed image color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either selector is outside the installed fade images; the exception names <paramref name="paletteIndex"/> for both invalid cases.</exception>
    public RgbColor FadeColor(int paletteIndex, int color) =>
        paletteIndex is >= 1 and <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount &&
        (uint)color < BabyMetroidCutsceneColorFormat.FadeColorCount
            ? fade.Colour(paletteIndex - 1, color)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the installed images.");
}
