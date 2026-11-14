using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One complete exploding-Zebes layer-fade control program.</summary>
public sealed class ZebesExplosionLayerFadePaletteFxColorDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal ZebesExplosionLayerFadePaletteFxColorDefinition()
    {
    }

    /// <summary>The mutually exclusive faded layer.</summary>
    public required ZebesExplosionLayerFadePaletteFxProgramOwner Owner { get; init; }

    /// <summary>The palette-FX object that installs this program.</summary>
    /// <remarks>One object fades the crust and the other the grey clouds.</remarks>
    public required ColourEffectId Object { get; init; }


    /// <summary>The authored duration of each timed record.</summary>
    public required int FrameDuration { get; init; }

    /// <summary>The program's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
