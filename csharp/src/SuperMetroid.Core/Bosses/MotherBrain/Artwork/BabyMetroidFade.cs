using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The cutscene Baby's fade to black. The Baby's fade displays the separately identified final-health paints at
/// five-sevenths down to zero intensity. Supplied edits remain independent; no latent RGB8 endpoint reconstruction or
/// stored stock sample is needed.
/// </summary>
internal sealed class BabyMetroidFade
{
    /// <summary>Stores the edited frames only when they differ from the final-health fade formula.</summary>
    internal readonly RgbColor[][]? supplied;

    /// <summary>Keeps any independently authored frames; stock frames use the composition.</summary>
    /// <param name="supplied">The supplied frames when one differs from the final-health fade formula, otherwise null.</param>
    internal BabyMetroidFade(RgbColor[][]? supplied) => this.supplied = supplied;

    /// <summary>Hashes the initial image, then the six resolved fade frames.</summary>
    internal static string ContentIdentity(BabyMetroidCutsceneColorCatalog colors) =>
        SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
        {
            colors.selection.Initial.AppendIdentity(content);
            colors.selection.Fade.AppendIdentity(content);
        });

    /// <summary>
    /// One displayed fade color: <paramref name="paletteIndex"/> one through six and <paramref name="color"/> zero
    /// through thirteen; the exception names <paramref name="paletteIndex"/> for both invalid cases.
    /// </summary>
    internal RgbColor Displayed(int paletteIndex, int color) =>
        paletteIndex is >= 1 and <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount &&
        (uint)color < BabyMetroidCutsceneColorDefinitions.FadeColorCount
            ? Resolve(paletteIndex - 1, color)
            : throw new ArgumentOutOfRangeException(nameof(paletteIndex),
                $"Cutscene Baby fade index {paletteIndex}, color {color} is outside the authored images.");

    /// <summary>Retrieves one displayed color from the edited frames or the composition.</summary>
    internal RgbColor Resolve(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

    /// <summary>Looks up the matching final-health paint before applying fade intensity.</summary>
    internal static RgbColor Endpoint(int color) => BabyMetroidFinalHealthPaint.Color(color);

    /// <summary>Scales each RGB5 channel by the remaining-sevenths intensity.</summary>
    internal static RgbColor Calculate(int frame, int color)
    {
        int remaining = BabyMetroidCutsceneColorDefinitions.FadeFrameCount - 1 - frame;
        int intervals = BabyMetroidCutsceneColorDefinitions.FadeFrameCount + 1;
        return Endpoint(color).Map((_, channel) => channel * remaining / intervals);
    }

    /// <summary>Hashes the six resolved frames so and equivalent supplied artwork share an identity.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        content.Append("fade", BabyMetroidCutsceneColorDefinitions.FadeFrameCount);
        Span<RgbColor> frameColors = stackalloc RgbColor[BabyMetroidCutsceneColorDefinitions.FadeColorCount];
        for (int frame = 0; frame < BabyMetroidCutsceneColorDefinitions.FadeFrameCount; frame++)
        {
            for (int color = 0; color < frameColors.Length; color++) frameColors[color] = Resolve(frame, color);
            content.AppendColors("row", frameColors);
        }
    }
}
