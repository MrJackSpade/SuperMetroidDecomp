using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete persistent Zebes-explosion ambient palette loop.</summary>
public sealed class ZebesExplosionAmbientPaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal ZebesExplosionAmbientPaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive ambient owner.</summary>
    public required ZebesExplosionAmbientPaletteFxProgramOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks>One object installs the afterglow and the other the lava.</remarks>
    public required ColourEffectId Object { get; init; }


    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public required int ColorsPerFrame { get; init; }

    /// <summary>The complete loop duration in frames.</summary>
    public required int CycleFrames { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
