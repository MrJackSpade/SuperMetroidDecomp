using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal TitleScreenAmbientPaletteFxColorDefinition()
    {
    }

    /// <summary>Semantic title-palette owner selecting the tube or the displays definition; not a runtime palette-object slot index.</summary>
    public required TitleScreenAmbientPaletteFxProgramOwner Owner { get; init; }
    /// <summary>The colours the program animates.</summary>
    public required ColourEffectColours Colours { get; init; }
    /// <summary>Timed palette-image records per loop: eight tube shades or two display states, distinct from the number of palette updates spent in the loop.</summary>
    public required int FrameCount { get; init; }
    /// <summary>Editable packed RGB5 words per timed record: four for the tube light or two for the displays, excluding duration and wait control words.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => ColorFrames.Color(frame, color);
}
