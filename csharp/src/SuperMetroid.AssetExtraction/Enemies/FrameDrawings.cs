using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Which looks of each encounter one frame is drawn in (#1513, #1538), as <see cref="EncounterArtworkDrawingDefinitions"/>
/// lists them: a sheet holds the frame's finished pictures of only those looks.
/// </summary>
internal sealed class FrameDrawings
{
    /// <summary>Every catalogue entry with the frames it lists resolved, its sprite objects' frames among them.</summary>
    private static readonly ResolvedDrawing[] Entries = [.. EncounterArtworkDrawingDefinitions.All.Select(drawing => new ResolvedDrawing(drawing,
        new HashSet<EnemyFrameId>(drawing.Frames.Concat(drawing.SpriteObjects.SelectMany(kind =>
            RoomSpriteObjectAnimations.Segments.FramesOf(RoomSpriteObjectAnimations.StartingWith(RoomSpriteObjectDefinitions.Animations[kind]))).Select(RoomSpriteObjectVisualDefinitions.ShownFrames.Of))),
        new HashSet<EnemyProjectileFrameId>(drawing.Projectiles)))];

    /// <summary>The entries listing the frame.</summary>
    private readonly EncounterArtworkDrawing[] listed;

    /// <summary>A frame the entries <paramref name="listed"/> list.</summary>
    private FrameDrawings(EncounterArtworkDrawing[] listed) => this.listed = listed;

    /// <summary>A frame no encounter draws in a look of its own, which shows only its room's load.</summary>
    internal static FrameDrawings None { get; } = new([]);

    /// <summary>When enemy frame <paramref name="frame"/> is drawn, on whichever sheet holds it.</summary>
    internal static FrameDrawings Of(EnemyFrameId frame) =>
        new([.. Entries.Where(entry => entry.Frames.Contains(frame)).Select(entry => entry.Drawing)]);

    /// <summary>When projectile frame <paramref name="frame"/> is drawn.</summary>
    internal static FrameDrawings Of(EnemyProjectileFrameId frame) =>
        new([.. Entries.Where(entry => entry.Projectiles.Contains(frame)).Select(entry => entry.Drawing)]);

    /// <summary>The looks of <paramref name="encounter"/> the frame is drawn in, or none when no entry lists it for the encounter.</summary>
    /// <exception cref="InvalidDataException">The entries disagree on whether the frame's artwork is its look's own, or name a look the encounter lacks.</exception>
    internal FrameDrawnLooks? For(ComposedEncounter encounter)
    {
        EncounterArtworkDrawing[] drawn = [.. listed.Where(entry => entry.Encounter == encounter)];
        if (drawn.Length == 0)
            return null;
        bool[] own = [.. drawn.Select(entry => entry.ArtworkOfLook).Distinct()];
        if (own.Length != 1)
            throw new InvalidDataException($"Entries listing a frame for {encounter} disagree on whether its artwork is its look's own.");
        return new FrameDrawnLooks([.. drawn.SelectMany(entry => entry.Looks).Select(look => EncounterLookUploads.IndexOf(encounter, look)).Distinct().Order()],
            own[0]);
    }

    /// <summary>A catalogue entry with the frames it lists resolved.</summary>
    /// <param name="Drawing">The entry.</param>
    /// <param name="Frames">The enemy frames it lists, its sprite objects' frames among them.</param>
    /// <param name="Projectiles">The projectile frames it lists.</param>
    private sealed record ResolvedDrawing(EncounterArtworkDrawing Drawing, IReadOnlySet<EnemyFrameId> Frames, IReadOnlySet<EnemyProjectileFrameId> Projectiles);
}
