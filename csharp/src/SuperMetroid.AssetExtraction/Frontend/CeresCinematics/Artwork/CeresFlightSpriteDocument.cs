using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Editable JSON schema for Ceres approach/destruction OAM compositions; scene scripts retain actor placement, motion, and frame-selection timing.</summary>
public sealed record CeresFlightSpriteDocument
{
    /// <summary>Schema revision, which must equal <see cref="CeresFlightSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly six ordered-part compositions named <c>stars</c>, <c>large-asteroid</c>, <c>station-under-attack</c>, <c>small-asteroid</c>, <c>vortex-even</c>, and <c>vortex-odd</c>, each bounded by the 128-part OAM limit.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
