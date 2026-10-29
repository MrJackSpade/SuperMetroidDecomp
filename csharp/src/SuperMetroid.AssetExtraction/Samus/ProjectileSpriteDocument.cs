using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Mutable JSON authoring data for projectile OAM appearance, separate from timed instruction records, damage, hitboxes, and motion.</summary>
public sealed record ProjectileSpriteDocument
{
    /// <summary>Gets the schema revision, which must equal <see cref="ProjectileSpriteDefinitions.Version"/> when loaded.</summary>
    public required int Version { get; init; }
    /// <summary>Gets caller-owned ordered parts keyed by <see cref="ProjectileSpriteId.Name"/> for every required composition; zero-part compositions remain valid.</summary>
    /// <remarks>Each composition permits at most 128 parts with explicit palette selectors 0..7, pixel offsets, 8- or 16-pixel sizes, and tile coordinates in a 16-column by 32-row OBJ grid. Large parts may cross tile-grid boundaries using hardware wrapping. Loading compiles and copies values rather than retaining these mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
