using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// When one frame is drawn under each encounter artwork sequence (#1513), as <see cref="EncounterArtworkDrawingDefinitions"/>
/// lists it: a sheet holds the frame's pictures of only those steps.
/// </summary>
internal sealed class FrameDrawings
{
    /// <summary>Every catalogue entry with the frames it lists resolved, its sprite objects' frames among them.</summary>
    private static readonly ResolvedDrawing[] Entries = [.. EncounterArtworkDrawingDefinitions.All.Select(drawing => new ResolvedDrawing(drawing,
        new HashSet<EnemyFrameId>(drawing.Frames.Concat(drawing.SpriteObjects.SelectMany(kind =>
            RoomSpriteObjectAnimations.Programs.FramesFrom(RoomSpriteObjectDefinitions.Programs[kind])).Select(RoomSpriteObjectVisualDefinitions.ShownFrames.Of))),
        new HashSet<EnemyProjectileFrameId>(drawing.Projectiles)))];

    /// <summary>The entries listing the frame.</summary>
    private readonly EncounterArtworkDrawing[] listed;

    /// <summary>A frame the entries <paramref name="listed"/> list.</summary>
    private FrameDrawings(EncounterArtworkDrawing[] listed) => this.listed = listed;

    /// <summary>A frame drawn under no sequence, which shows only its loaded picture.</summary>
    internal static FrameDrawings None { get; } = new([]);

    /// <summary>When enemy frame <paramref name="frame"/> is drawn, on whichever sheet holds it.</summary>
    internal static FrameDrawings Of(EnemyFrameId frame) =>
        new([.. Entries.Where(entry => entry.Frames.Contains(frame)).Select(entry => entry.Drawing)]);

    /// <summary>When projectile frame <paramref name="frame"/> is drawn.</summary>
    internal static FrameDrawings Of(EnemyProjectileFrameId frame) =>
        new([.. Entries.Where(entry => entry.Projectiles.Contains(frame)).Select(entry => entry.Drawing)]);

    /// <summary>When the frame is drawn under <paramref name="sequence"/>, or none when no entry lists it for the sequence.</summary>
    internal FrameDrawnSteps? For(EncounterArtworkSequence sequence)
    {
        EncounterArtworkDrawing[] drawn = [.. listed.Where(entry => entry.Sequence == sequence)];
        return drawn.Length == 0 ? null
            : new FrameDrawnSteps(drawn.Any(entry => entry.DrawnBeforeFirstStep), [.. drawn.SelectMany(entry => entry.Steps)]);
    }

    /// <summary>A catalogue entry with the frames it lists resolved.</summary>
    /// <param name="Drawing">The entry.</param>
    /// <param name="Frames">The enemy frames it lists, its sprite objects' frames among them.</param>
    /// <param name="Projectiles">The projectile frames it lists.</param>
    private sealed record ResolvedDrawing(EncounterArtworkDrawing Drawing, IReadOnlySet<EnemyFrameId> Frames, IReadOnlySet<EnemyProjectileFrameId> Projectiles);
}
