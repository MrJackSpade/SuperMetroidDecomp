using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The twelve Spore Spawn frames and
/// their fixed gameplay hitboxes. Version 6 mislabeled their visual keys as
/// Draygon, but collision always follows the actual Spore Spawn animation.
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

    /// <summary>Head rectangles shared by the closed and open lists, which differ only in touch AI.</summary>
    private static EnemyCollisionHitbox[] Head(EnemyReaction touch) =>
        [new(-41, -30, 41, 30, touch, Dud), new(-16, -45, 15, -30, touch, Dud)];

    /// <summary>Two closed-head shell rectangles without an active touch response.</summary>
    internal static readonly EnemyCollisionHitbox[] ClosedHeadList = Head(Inert);
    /// <summary>Two open-head shell rectangles with active touch response.</summary>
    internal static readonly EnemyCollisionHitbox[] OpenHeadList = Head(Touch);
    /// <summary>Wider head rectangles used by the first opening pose.</summary>
    internal static readonly EnemyCollisionHitbox[] ExtendedHeadList =
        [new(-44, -35, 43, 33, Touch, Dud), new(-16, -49, 15, -35, Touch, Dud)];
    /// <summary>
    /// The six moving heads' upper and lower shell bands, inner point and crown, indexed by opening step. Each step widens
    /// the vertical shell spacing and raises the crown; the last head's inner point extends one pixel farther at both
    /// vertical ends, and the fully open frames hold it.
    /// </summary>
    internal static readonly EnemyCollisionHitbox[][] MovingHeadLists =
    [
        [new(-45, -38, 44, -9, Touch, Dud), new(-45, 8, 44, 35, Touch, Dud), InnerPoint(Shot), new(-16, -54, 16, -22, Touch, Dud)],
        [new(-43, -44, 42, -13, Touch, Dud), new(-44, 12, 42, 42, Touch, Dud), InnerPoint(Shot), new(-16, -58, 16, -42, Touch, Dud)],
        [new(-45, -47, 44, -17, Touch, Dud), new(-44, 16, 43, 46, Touch, Dud), InnerPoint(Shot), new(-16, -62, 16, -45, Touch, Dud)],
        [new(-44, -50, 45, -21, Touch, Dud), new(-43, 20, 43, 50, Touch, Dud), InnerPoint(Shot), new(-16, -64, 16, -48, Touch, Dud)],
        [new(-44, -53, 44, -23, Touch, Dud), new(-44, 22, 43, 52, Touch, Dud), InnerPoint(Shot), new(-16, -68, 16, -48, Touch, Dud)],
        [new(-44, -55, 43, -25, Touch, Dud), new(-45, 24, 43, 55, Touch, Dud), new(-15, -25, 14, 24, Touch, Shot), new(-16, -69, 16, -48, Touch, Dud)],
    ];
    /// <summary>Single trailing point that accepts the Spore Spawn shot reaction.</summary>
    internal static readonly EnemyCollisionHitbox[] ShotPointList = [InnerPoint(Shot)];
    /// <summary>Single trailing point that rejects shots through the dud reaction.</summary>
    internal static readonly EnemyCollisionHitbox[] DudPointList = [InnerPoint(Dud)];

    /// <summary>The trailing-point hitboxes by oscillation phase B, C, D, C: only phase D's point rejects shots; the mirrored point shares the active list.</summary>
    private static readonly EnemyCollisionHitbox[][] InnerPointsByPhase = [ShotPointList, ShotPointList, DudPointList, ShotPointList];

    /// <summary>The opening poses' head lists, in pose order; the moving heads follow the wider first pose.</summary>
    internal static readonly EnemyCollisionHitbox[][] OpeningHeadLists = [ExtendedHeadList, .. MovingHeadLists];

    /// <summary>The dead root's one inactive head component.</summary>
    internal static readonly EnemyCollisionComponent[] DeadComponents = [new(Offset16<RoomPixels>.Zero, ClosedHeadList)];
    /// <summary>The closed root's one head component.</summary>
    internal static readonly EnemyCollisionComponent[] ClosedComponents = [new(Offset16<RoomPixels>.Zero, OpenHeadList)];
    /// <summary>Each opening root's head and inner point, indexed by opening step; the inner point oscillates B, C, D, C.</summary>
    internal static readonly EnemyCollisionComponent[][] OpeningComponents =
        [.. Enumerable.Range(0, OpeningHeadLists.Length).Select(opening => HeadAndPoint(OpeningHeadLists[opening], opening % 4))];
    /// <summary>Each fully open root's final head and inner point, indexed by oscillation phase B, C, D.</summary>
    internal static readonly EnemyCollisionComponent[][] FullyOpenComponents =
        [.. Enumerable.Range(0, 3).Select(phase => HeadAndPoint(MovingHeadLists[^1], phase))];

    /// <summary>A zero-offset head followed by its zero-offset inner point at <paramref name="phase"/>.</summary>
    private static EnemyCollisionComponent[] HeadAndPoint(EnemyCollisionHitbox[] head, int phase) =>
        [new(Offset16<RoomPixels>.Zero, head), new(Offset16<RoomPixels>.Zero, InnerPointsByPhase[phase])];

    /// <summary>
    /// Each Spore Spawn frame's components: the dead and closed frames contain one head; opening frames advance the head
    /// geometry while the inner point oscillates B, C, D, C; fully open frames hold the final head and select B, C, D.
    /// All component offsets are zero.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, EnemyCollisionComponent[]> FrameComponents = new()
    {
        [EnemyFrameId.SporeSpawnInitialDead] = DeadComponents,
        [EnemyFrameId.SporeSpawnInitialAlive] = ClosedComponents,
        [EnemyFrameId.SporeSpawnOpeningFirst] = OpeningComponents[0],
        [EnemyFrameId.SporeSpawnOpeningSecond] = OpeningComponents[1],
        [EnemyFrameId.SporeSpawnOpeningThird] = OpeningComponents[2],
        [EnemyFrameId.SporeSpawnOpeningFourth] = OpeningComponents[3],
        [EnemyFrameId.SporeSpawnOpeningFifth] = OpeningComponents[4],
        [EnemyFrameId.SporeSpawnDeath] = OpeningComponents[5],
        [EnemyFrameId.SporeSpawnOpeningSeventh] = OpeningComponents[6],
        [EnemyFrameId.SporeSpawnOpenPulseFirst] = FullyOpenComponents[0],
        [EnemyFrameId.SporeSpawnOpenPulseSecond] = FullyOpenComponents[1],
        [EnemyFrameId.SporeSpawnOpenPulseThird] = FullyOpenComponents[2],
    };
}
