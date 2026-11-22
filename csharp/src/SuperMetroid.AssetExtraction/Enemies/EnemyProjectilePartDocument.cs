using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One ordered part of an enemy-projectile composition (#2563): the character picture it draws, named by its place among
/// the projectile characters its spawner's sheet is composed from, where it lies from the composition's origin, its size,
/// colour row, depth and reflections. Null palette inherits its drawing owner's current palette.
/// </summary>
public sealed partial record EnemyProjectilePartDocument
{
    /// <summary>Signed horizontal pixel displacement from the composition origin.</summary>
    public required int OffsetX { get; init; }

    /// <summary>Signed vertical pixel displacement from the composition origin.</summary>
    public required int OffsetY { get; init; }

    /// <summary>
    /// The character picture the part draws: its place, from zero, among the projectile characters its spawner's sheet is
    /// composed from.
    /// </summary>
    public required int Character { get; init; }

    /// <summary>Square part size in pixels, either 8 or 16.</summary>
    public required int Size { get; init; }

    /// <summary>Where the part draws among the room's layers.</summary>
    [JsonConverter(typeof(ActorDepthJsonConverter))]
    public required ActorDepth Depth { get; init; }

    /// <summary>The part's own colour row, or <see langword="null"/> to inherit the drawing owner's.</summary>
    public required int? Palette { get; init; }

    /// <summary>Horizontal reflection applied to the character picture.</summary>
    public required bool FlipX { get; init; }

    /// <summary>Vertical reflection applied to the character picture.</summary>
    public required bool FlipY { get; init; }
}
