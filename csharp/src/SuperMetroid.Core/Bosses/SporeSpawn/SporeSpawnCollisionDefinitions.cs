using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The twelve Spore Spawn frames and their fixed gameplay hitboxes (#2241): each head list named for the pose it fits, and
/// each frame listed against its head and its inner point.
/// </summary>
internal static class SporeSpawnCollisionDefinitions
{
    /// <summary>Spore Spawn's contact reaction, on the active shell and vulnerable point.</summary>
    internal static readonly EnemyReaction Touch = SporeSpawnReactions.Touch;
    /// <summary>Spore Spawn's shot reaction, on the exposed vulnerable point.</summary>
    internal static readonly EnemyReaction Shot = SporeSpawnReactions.Shot;
    /// <summary>The inactive head's touch reaction before Spore Spawn opens.</summary>
    internal static readonly EnemyReaction Inert = EnemyReaction.Ignore;
    /// <summary>The dud shot reaction of shell rectangles and the inactive trailing point.</summary>
    internal static readonly EnemyReaction Dud = EnemyReaction.DudShot;

    /// <summary>Builds the trailing vulnerable rectangle with active or dud shot response.</summary>
    private static EnemyCollisionHitbox InnerPoint(EnemyReaction shot) => new(-15, -24, 14, 23, Touch, shot);

    /// <summary>Head rectangles shared by the husk's and the shut head's lists, which differ only in touch AI.</summary>
    private static EnemyCollisionHitbox[] Head(EnemyReaction touch) =>
        [new(-41, -30, 41, 30, touch, Dud), new(-16, -45, 15, -30, touch, Dud)];

    /// <summary>The hardened husk's two shell rectangles, which Samus's touch passes through.</summary>
    internal static readonly EnemyCollisionHitbox[] HuskHead = Head(Inert);
    /// <summary>The living shut head's two shell rectangles, which hurt on touch.</summary>
    internal static readonly EnemyCollisionHitbox[] ShutHead = Head(Touch);
    /// <summary>The head parted 8 pixels: wider shell rectangles, still closed over the inner point.</summary>
    internal static readonly EnemyCollisionHitbox[] Parted8Head =
        [new(-44, -35, 43, 33, Touch, Dud), new(-16, -49, 15, -35, Touch, Dud)];
    /// <summary>The head parted 16 pixels: its upper and lower shell bands, inner point and crown.</summary>
    internal static readonly EnemyCollisionHitbox[] Parted16Head =
        [new(-45, -38, 44, -9, Touch, Dud), new(-45, 8, 44, 35, Touch, Dud), InnerPoint(Shot), new(-16, -54, 16, -22, Touch, Dud)];
    /// <summary>The head parted 24 pixels, its bands farther apart and its crown higher.</summary>
    internal static readonly EnemyCollisionHitbox[] Parted24Head =
        [new(-43, -44, 42, -13, Touch, Dud), new(-44, 12, 42, 42, Touch, Dud), InnerPoint(Shot), new(-16, -58, 16, -42, Touch, Dud)];
    /// <summary>The head parted 32 pixels.</summary>
    internal static readonly EnemyCollisionHitbox[] Parted32Head =
        [new(-45, -47, 44, -17, Touch, Dud), new(-44, 16, 43, 46, Touch, Dud), InnerPoint(Shot), new(-16, -62, 16, -45, Touch, Dud)];
    /// <summary>The head parted 40 pixels.</summary>
    internal static readonly EnemyCollisionHitbox[] Parted40Head =
        [new(-44, -50, 45, -21, Touch, Dud), new(-43, 20, 43, 50, Touch, Dud), InnerPoint(Shot), new(-16, -64, 16, -48, Touch, Dud)];
    /// <summary>The head parted 44 pixels.</summary>
    internal static readonly EnemyCollisionHitbox[] Parted44Head =
        [new(-44, -53, 44, -23, Touch, Dud), new(-44, 22, 43, 52, Touch, Dud), InnerPoint(Shot), new(-16, -68, 16, -48, Touch, Dud)];
    /// <summary>
    /// The head parted 48 pixels, fully open: its inner point extends one pixel farther at both vertical ends, and every fully
    /// open frame holds it.
    /// </summary>
    internal static readonly EnemyCollisionHitbox[] Parted48Head =
        [new(-44, -55, 43, -25, Touch, Dud), new(-45, 24, 43, 55, Touch, Dud), new(-15, -25, 14, 24, Touch, Shot), new(-16, -69, 16, -48, Touch, Dud)];
    /// <summary>The single trailing point that accepts the Spore Spawn shot reaction.</summary>
    internal static readonly EnemyCollisionHitbox[] ShotPointList = [InnerPoint(Shot)];
    /// <summary>The single trailing point that rejects shots through the dud reaction.</summary>
    internal static readonly EnemyCollisionHitbox[] DudPointList = [InnerPoint(Dud)];

    /// <summary>A zero-offset head followed by its zero-offset inner point.</summary>
    private static EnemyCollisionComponent[] HeadAndPoint(EnemyCollisionHitbox[] head, EnemyCollisionHitbox[] point) =>
        [new(Offset16<RoomPixels>.Zero, head), new(Offset16<RoomPixels>.Zero, point)];

    /// <summary>
    /// Each Spore Spawn frame's components: the husk and the shut head hold one head; each opening frame holds the head its
    /// parting fits and its inner point, which rejects shots on the 24-pixel, the 48-pixel and the last fully open frame and
    /// takes them on the rest; the fully open frames hold the 48-pixel head. All component offsets are zero.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, EnemyCollisionComponent[]> FrameComponents = new()
    {
        [EnemyFrameId.SporeSpawnShutTwin] = [new(Offset16<RoomPixels>.Zero, HuskHead)],
        [EnemyFrameId.SporeSpawnShut] = [new(Offset16<RoomPixels>.Zero, ShutHead)],
        [EnemyFrameId.SporeSpawnParted8] = HeadAndPoint(Parted8Head, ShotPointList),
        [EnemyFrameId.SporeSpawnParted16] = HeadAndPoint(Parted16Head, ShotPointList),
        [EnemyFrameId.SporeSpawnParted24] = HeadAndPoint(Parted24Head, DudPointList),
        [EnemyFrameId.SporeSpawnParted32] = HeadAndPoint(Parted32Head, ShotPointList),
        [EnemyFrameId.SporeSpawnParted40] = HeadAndPoint(Parted40Head, ShotPointList),
        [EnemyFrameId.SporeSpawnParted44] = HeadAndPoint(Parted44Head, ShotPointList),
        [EnemyFrameId.SporeSpawnParted48] = HeadAndPoint(Parted48Head, DudPointList),
        [EnemyFrameId.SporeSpawnParted48CoreGlowing] = HeadAndPoint(Parted48Head, ShotPointList),
        [EnemyFrameId.SporeSpawnParted48CoreBright] = HeadAndPoint(Parted48Head, ShotPointList),
        [EnemyFrameId.SporeSpawnParted48Twin] = HeadAndPoint(Parted48Head, DudPointList),
    };
}
