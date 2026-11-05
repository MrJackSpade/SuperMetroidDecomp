using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>The supplied Ceres Ridley colour families a catalog is built from.</summary>
internal sealed record CeresRidleySuppliedColors
{
    /// <summary>The startup colours.</summary>
    public required RgbColor[] Start { get; init; }

    /// <summary>The eye fade's colours, one set per fade shade.</summary>
    public required RgbColor[][] EyeFade { get; init; }

    /// <summary>The body fade's colours, one set per fade shade.</summary>
    public required RgbColor[][] BodyFade { get; init; }

    /// <summary>The health colours, one set per health band.</summary>
    public required RgbColor[][] Health { get; init; }

    /// <summary>The alarm colours, one set per alarm step.</summary>
    public required RgbColor[][] Alarm { get; init; }

    /// <summary>The retreat background colours.</summary>
    public required RgbColor[] RetreatBg { get; init; }

    /// <summary>The retreat colours shared with the startup door paints.</summary>
    public required RgbColor[] RetreatShared { get; init; }

    /// <summary>The Baby Metroid's colours by shade.</summary>
    public required CeresBabyShadeColours Baby { get; init; }
}
