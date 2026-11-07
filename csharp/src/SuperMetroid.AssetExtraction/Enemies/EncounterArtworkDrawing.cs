using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Frames an encounter draws in some of its looks (#1513, #1538): the enemy frames, projectile frames and the frames of the
/// room sprite objects listed, drawn in the looks listed. A sheet holds a frame's finished picture of each look it is drawn
/// in; a frame drawn in one look, or alike in every look it is drawn in, has one picture.
/// </summary>
/// <param name="Encounter">The encounter.</param>
/// <param name="Looks">The names of the encounter's looks the frames are drawn in, its room's load among them when they are drawn before it shows another.</param>
internal sealed record EncounterArtworkDrawing(ComposedEncounter Encounter, IReadOnlyList<string> Looks)
{
    /// <summary>The enemy frames drawn, wherever a sheet holds them.</summary>
    internal IReadOnlyList<EnemyFrameId> Frames { get; init; } = [];

    /// <summary>The projectile frames drawn.</summary>
    internal IReadOnlyList<EnemyProjectileFrameId> Projectiles { get; init; } = [];

    /// <summary>The room sprite objects whose frames are drawn.</summary>
    internal IReadOnlyList<RoomSpriteObjectKind> SpriteObjects { get; init; } = [];

    /// <summary>
    /// Whether the listed frames' artwork is what their one look copies (#1513): such a frame is whole once that look has
    /// copied it, never a look still to come nor another look's artwork written over its characters afterwards, so its
    /// sheet holds it whole; a frame the look's copies never write shows nothing.
    /// </summary>
    internal bool ArtworkOfLook { get; init; }
}
