using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete cinematic glow control program.</summary>
public sealed class CinematicGlowPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal CinematicGlowPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive glow owner.</summary>
    public required CinematicGlowPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks>
    /// One object owns the old Mother Brain lights and the other the gunship.
    /// </remarks>
    public required ColourEffectId Object { get; init; }


    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The authored duration of each timed record.</summary>
    public required int FrameDuration { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
