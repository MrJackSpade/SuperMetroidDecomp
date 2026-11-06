using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Frames an encounter's artwork sequence is drawn under (#1513): the enemy frames, projectile frames and the frames of
/// the room sprite objects listed, drawn at the steps listed, and before the sequence's first step when it says so. A
/// sheet holds a frame's picture of a step only when the frame is drawn at that step.
/// </summary>
/// <param name="Sequence">The encounter's artwork sequence.</param>
/// <param name="DrawnBeforeFirstStep">Whether the frames are drawn before the sequence reaches its first step, showing the room's load.</param>
/// <param name="Steps">The steps at which the frames are drawn.</param>
internal sealed record EncounterArtworkDrawing(EncounterArtworkSequence Sequence, bool DrawnBeforeFirstStep,
    IReadOnlyList<EncounterArtworkStepRange> Steps)
{
    /// <summary>The enemy frames drawn, wherever a sheet holds them.</summary>
    internal IReadOnlyList<EnemyFrameId> Frames { get; init; } = [];

    /// <summary>The projectile frames drawn.</summary>
    internal IReadOnlyList<EnemyProjectileFrameId> Projectiles { get; init; } = [];

    /// <summary>The room sprite objects whose frames are drawn.</summary>
    internal IReadOnlyList<RoomSpriteObjectKind> SpriteObjects { get; init; } = [];

    /// <summary>
    /// The source whose first run of pages completes the listed frames' artwork, or none when each step's look is theirs
    /// (#1513). Such a frame is whole once that run has loaded, or as the room's load leaves it: it shows that one picture
    /// at every step it is drawn at and before the first, never a page still loading nor another source's artwork
    /// written over its characters afterwards, so its sheet holds it whole.
    /// </summary>
    internal EncounterArtworkSource? CompletedBy { get; init; }
}
