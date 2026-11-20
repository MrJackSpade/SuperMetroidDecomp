using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>One suit's heat-pulse records and the colours each writes.</summary>
/// <remarks>Each suit shows one record for each heat shimmer phase.</remarks>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Only the catalog creates definitions.</summary>
    internal PaletteFxHeatProgramDefinition()
    {
    }

    /// <summary>Fifteen colours follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>The mutually exclusive suit palette represented by this program.</summary>
    public required ColourEffectHeatSuit Suit { get; init; }

    /// <summary>The sixteen phase records and the colours each writes.</summary>
    public required PaletteFxColorFrames ColorFrames { get; init; }
}
