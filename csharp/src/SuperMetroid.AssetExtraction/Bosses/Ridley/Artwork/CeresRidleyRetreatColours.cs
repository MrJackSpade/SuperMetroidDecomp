using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>The installed colours Ceres Ridley's retreat shows: its background's and the colours it shares.</summary>
internal sealed record CeresRidleyRetreatColours
{
    /// <summary>The retreat background colours.</summary>
    public required RgbColor[] Background { get; init; }

    /// <summary>The retreat shared colours.</summary>
    public required RgbColor[] Shared { get; init; }
}
