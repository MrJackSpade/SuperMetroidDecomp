using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions: the records <paramref name="effect"/> shows.</summary>
    /// <param name="effect">The colour effect whose records the program holds.</param>
    /// <param name="frameCount">Timed palette-image records per loop.</param>
    /// <param name="colorsPerFrame">Editable packed RGB5 words per timed record.</param>
    internal TitleScreenAmbientPaletteFxColorDefinition(ColourEffectId effect, int frameCount, int colorsPerFrame)
    {
        Effect = effect;
        FrameCount = frameCount;
        ColorsPerFrame = colorsPerFrame;
        ColorFrames = PaletteFxColorFrames.Of(effect, colorsPerFrame);
    }

    /// <summary>The colour effect whose records the program holds, which identifies the definition: the tube light or the displays.</summary>
    public ColourEffectId Effect { get; }
    /// <summary>Timed palette-image records per loop: eight tube shades or two display states, distinct from the number of palette updates spent in the loop.</summary>
    public int FrameCount { get; }
    /// <summary>Editable packed RGB5 words per timed record: four for the tube light or two for the displays, excluding duration and wait control words.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public PaletteFxColorFrames ColorFrames { get; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => ColorFrames.Color(frame, color);
}
