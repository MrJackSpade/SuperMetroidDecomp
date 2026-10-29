using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Editable OAM composition schema for the Ceres explosion and Zebes reveal, separate from cinematic actors, motion, timing, and the shared approach artwork.</summary>
public sealed record CeresDestructionSpriteDocument
{
    /// <summary>Composition schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 23 named ordered part arrays: seven asteroid/planet/title/star backdrops, six small blasts, four large blasts, and six station blasts; null part palettes inherit the actor's palette.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
