using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The fixed room-object spawns embedded in Kraid's growth and sinking steps.
/// Their irregular order is visible: the ceiling breaks from scattered points as the
/// body rises, rather than sweeping monotonically across the room.
/// </summary>
public static class KraidObjectDefinitions
{
    /// <summary>
    /// The final room-object spawn of Kraid's death setup:
    /// start the animated spike sweep at (5,27), independently of defeated-room clearing.
    /// </summary>
    public static readonly RoomObjectSpawnRequest<KraidRoomMutation> LiveDeathSpikes = new(new(5, 27), KraidRoomMutation.CrumbleSpikes);

    /// <summary>Clears the ceiling blocks from (2,18).</summary>
    private static RoomObjectSpawnRequest<KraidRoomMutation> ClearCeiling => new(new(2, 18), KraidRoomMutation.ClearCeiling);

    /// <summary>Clears the spike blocks from (5,27).</summary>
    private static RoomObjectSpawnRequest<KraidRoomMutation> ClearSpikes => new(new(5, 27), KraidRoomMutation.ClearSpikes);

    /// <summary>
    /// The nine updates of Kraid's ceiling crumble, indexed by how many ceiling rocks have fallen: the ceiling steps
    /// it selects. Each block lies directly above its paired rock center on row 18; column 2 shows the left-edge
    /// background, and the other columns their odd or even column's.
    /// </summary>
    public static IReadOnlyList<RoomObjectSpawnRequest<KraidRoomMutation>> GrowthCeiling { get; } = new List<RoomObjectSpawnRequest<KraidRoomMutation>>
    {
        new(new(6, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn),
        new(new(13, 18), KraidRoomMutation.CrumbleCeilingInOddColumn),
        new(new(2, 18), KraidRoomMutation.CrumbleCeilingAtLeftEdge),
        new(new(10, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn),
        new(new(5, 18), KraidRoomMutation.CrumbleCeilingInOddColumn),
        new(new(12, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn),
        new(new(3, 18), KraidRoomMutation.CrumbleCeilingInOddColumn),
        new(new(11, 18), KraidRoomMutation.CrumbleCeilingInOddColumn),
        new(new(4, 18), KraidRoomMutation.CrumbleCeilingInEvenColumn),
    }.AsReadOnly();

    /// <summary>
    /// The fixed room objects spawned by defeated-room initialization, in the order it spawns them.
    /// </summary>
    public static IReadOnlyList<RoomObjectSpawnRequest<KraidRoomMutation>> DefeatedRoom { get; } = [ClearCeiling, ClearSpikes];

    /// <summary>
    /// The platform mutation paired with each sinking-schedule step. The surviving platforms share the ceiling row and
    /// alternate background blocks by column parity, as in the sinking steps' authored arguments.
    /// </summary>
    public static IReadOnlyDictionary<KraidSinkStep, RoomObjectSpawnRequest<KraidRoomMutation>> SinkMutations { get; } =
        new Dictionary<KraidSinkStep, RoomObjectSpawnRequest<KraidRoomMutation>>
        {
            [KraidSinkStep.CrumbleLeftPlatformLeft] = new RoomObjectSpawnRequest<KraidRoomMutation>(new(7, 18), KraidRoomMutation.CrumblePlatformInOddColumn),
            [KraidSinkStep.CrumbleLeftPlatformMiddle] = new RoomObjectSpawnRequest<KraidRoomMutation>(new(8, 18), KraidRoomMutation.CrumblePlatformInEvenColumn),
            [KraidSinkStep.CrumbleLeftPlatformRight] = new RoomObjectSpawnRequest<KraidRoomMutation>(new(9, 18), KraidRoomMutation.CrumblePlatformInOddColumn),
            [KraidSinkStep.CrumbleRightPlatformLeft] = new RoomObjectSpawnRequest<KraidRoomMutation>(new(14, 18), KraidRoomMutation.CrumblePlatformInEvenColumn),
            [KraidSinkStep.CrumbleRightPlatformMiddle] = new RoomObjectSpawnRequest<KraidRoomMutation>(new(15, 18), KraidRoomMutation.CrumblePlatformInOddColumn),
            [KraidSinkStep.CrumbleRightPlatformRight] = new RoomObjectSpawnRequest<KraidRoomMutation>(new(16, 18), KraidRoomMutation.CrumblePlatformInEvenColumn),
        };
}
