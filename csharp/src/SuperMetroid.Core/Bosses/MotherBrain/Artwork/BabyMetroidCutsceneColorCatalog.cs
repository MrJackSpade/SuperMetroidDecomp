using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable initial and six fade RGB5 images for the Mother Brain cutscene Baby.</summary>
public sealed class BabyMetroidCutsceneColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
        {
            initial.AppendIdentity(content);
            fade.AppendIdentity(content);
        });

    /// <summary>Compiled fifteen-color image shown before the death fade.</summary>
    internal readonly BabyMetroidInitialPalette initial;
    /// <summary>Six displayed fade rows, with native formula or supplied edited colors.</summary>
    internal readonly ColorFade fade;

    /// <summary>Copies compiled images into immutable catalog-owned palette views.</summary>
    internal BabyMetroidCutsceneColorCatalog(Bgr555[] initial, Bgr555[][] fade)
    {
        this.initial = new(initial);
        this.fade = new(fade);
    }

    /// <summary>Resolves one initial Baby Metroid sprite color, corresponding to the fifteen-word image at <c>$A9:94D4</c>; no CGRAM write or cutscene state change occurs.</summary>
    /// <param name="color">Zero-based image index from zero through 14, corresponding to colors 1..15 of OBJ palette seven; the transparent slot is excluded.</param>
    /// <returns>The image color.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="color"/> is outside the fifteen-color initial image.</exception>
    public Bgr555 InitialColor(int color) => initial.Resolve(color);

    /// <summary>Resolves one displayed death fade-to-black color from native <c>BabyMetroidFadingToBlackPalettes_1</c> through <c>_6</c> at <c>$AD:E90C-$E9B3</c>, without advancing the fade timer.</summary>
    /// <param name="paletteIndex">One-based displayed image selector from one through six; the unused native image zero is not installed.</param>
    /// <param name="color">Zero-based image index from zero through 13, corresponding to OBJ palette seven colors 1..14; transparent color zero and black color 15 are excluded.</param>
    /// <returns>The selected image color, retaining independently supplied artwork edits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either selector is outside the installed fade images; the exception names <paramref name="paletteIndex"/> for both invalid cases.</exception>
    public Bgr555 FadeColor(int paletteIndex, int color) =>
        paletteIndex is >= 1 and <= BabyMetroidCutsceneColorRomData.FadeFrameCount &&
        (uint)color < BabyMetroidCutsceneColorRomData.FadeColorCount
            ? fade.Resolve(paletteIndex - 1, color)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the authored images.");

    /// <summary>$AD:E90C-E9B3 displays the separately identified final-health paints
    /// at five-sevenths down to zero intensity. Supplied edits remain independent;
    /// no latent RGB8 endpoint reconstruction or stored stock sample is needed.</summary>
    internal sealed class ColorFade
    {
        /// <summary>Stores edited rows only when they differ from the native final-health fade formula.</summary>
        internal readonly Bgr555[][]? supplied;

        /// <summary>Uses the native composition for stock rows and retains any independently authored edits.</summary>
        internal ColorFade(Bgr555[][] frames)
        {
            for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < frames[frame].Length; color++)
                if (Calculate(frame, color) != frames[frame][color])
                {
                    supplied = frames.Select(row => row.ToArray()).ToArray();
                    return;
                }
        }

        /// <summary>Retrieves one displayed color from edited rows or the native composition.</summary>
        internal Bgr555 Resolve(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Looks up the matching final-health paint before applying fade intensity.</summary>
        internal static Bgr555 Endpoint(int color) => BabyMetroidFinalHealthPaintDefinitions.Color(color);

        /// <summary>Scales each RGB5 channel by the native remaining-sevenths intensity.</summary>
        private static Bgr555 Calculate(int frame, int color)
        {
            int remaining = BabyMetroidCutsceneColorRomData.FadeFrameCount - 1 - frame;
            int intervals = BabyMetroidCutsceneColorRomData.FadeFrameCount + 1;
            return Endpoint(color).Map((_, channel) => channel * remaining / intervals);
        }

        /// <summary>Hashes the six resolved rows so native and equivalent supplied artwork share an identity.</summary>
        internal void AppendIdentity(SelectedPresentationHash content)
        {
            content.Append("fade", BabyMetroidCutsceneColorRomData.FadeFrameCount);
            Span<Bgr555> row = stackalloc Bgr555[BabyMetroidCutsceneColorRomData.FadeColorCount];
            for (int frame = 0; frame < BabyMetroidCutsceneColorRomData.FadeFrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendColors("row", row);
            }
        }
    }
}
