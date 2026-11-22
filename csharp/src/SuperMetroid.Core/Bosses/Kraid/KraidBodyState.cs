using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body's own state beside what every part holds: the stage he is in and the phase of it he runs, his growth's
/// next ceiling rock and the rocks' wait of his rise through the floor.
/// </summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed partial class KraidBodyState : KraidPartState
{
    /// <summary>Makes the body of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the body is part of.</param>
    /// <param name="place">The place of the body, which his body found as his room lists it.</param>
    internal KraidBodyState(KraidEnemyState kraid, RoomEnemyPlace place)
        : base(kraid, place)
    {
    }

    /// <summary>
    /// The stage the body is in and the phase of it it runs (#2208, #2554). The body's setup puts it in its rise, or in its
    /// sunk death in a room he was found defeated in.
    /// </summary>
    public KraidStageRun Run { get; internal set; } = new KraidStageRun.Rising(KraidRisePhase.RestrictSamusToFirstScreen);

    /// <summary>The name of the stage the body is in.</summary>
    public KraidBodyStage Stage => Run.Stage;

    /// <summary>
    /// Whether Kraid fights (#1452, #1737): his fight and growth. Only then does his body's touch hurt Samus and a mouth hit
    /// that leaves him without health start his death; his rise through the floor and his death lie outside it.
    /// </summary>
    internal bool Fights => Run is KraidStageRun.Fighting or KraidStageRun.Growing;

    /// <summary>
    /// The next rock his growth knocks from the ceiling (#2400), walked through <see cref="KraidCeilingRockDefinitions.Rocks"/>
    /// from the first; none once every rock has fallen.
    /// </summary>
    public ListStep<KraidCeilingRock>? NextCeilingRock { get; internal set; } = ListStep<KraidCeilingRock>.FirstOf(KraidCeilingRockDefinitions.Rocks);

    /// <summary>
    /// The wait of the rise's current rock phase (#2538): <see cref="KraidArenaDefinitions.RiseSlowRocksUpdates"/>, then
    /// <see cref="KraidArenaDefinitions.RiseFastRocksUpdates"/>; each phase moves on on its last update, and throws up a
    /// rock on the updates its cadence finds in the updates left.
    /// </summary>
    public BossCountdown RisingRocksWait { get; } = new();
}
