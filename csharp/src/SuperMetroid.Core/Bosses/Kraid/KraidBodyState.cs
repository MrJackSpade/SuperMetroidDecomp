namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body's own state beside what every part holds: the stage he is in and the phase of it he runs, his growth's
/// next ceiling rock and the rocks' wait of his rise through the floor.
/// </summary>
[ArchitectureReview("back-reference", "A part of Kraid's encounter (#2218): the encounter owns this part's state for the room and the part reads the encounter it joined at setup, so the two share one lifetime.")]
public sealed class KraidBodyState : KraidPartState
{
    /// <summary>Makes the body of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the body is part of.</param>
    /// <param name="reference">Kraid's reference to the body, which it joins as it sets up.</param>
    internal KraidBodyState(KraidEnemyState kraid, KraidPartReference reference)
        : base(kraid, reference)
    {
    }

    /// <summary>
    /// The stage the body is in (#2208), which names the type of the phase it runs; setting a stage's phase enters that
    /// stage. The body's setup puts it in its rise, or in its death in a room he was found defeated in.
    /// </summary>
    public KraidBodyStage Stage => step.Stage;

    /// <summary>The stage the body is in with the phase of it it runs, one value (#2560): no phase of another stage is held.</summary>
    private BossStagePhase<KraidBodyStage> step = new BossStagePhase<KraidBodyStage>.Of<KraidRisePhase>(KraidBodyStage.Rising, KraidRisePhase.RestrictSamusToFirstScreen);

    /// <summary>The phase of his rise through the floor the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not rising.</exception>
    public KraidRisePhase RisePhase
    {
        get => step is BossStagePhase<KraidBodyStage>.Of<KraidRisePhase> { Stage: KraidBodyStage.Rising } inStage ? inStage.Phase : throw NotIn(KraidBodyStage.Rising);
        internal set => step = new BossStagePhase<KraidBodyStage>.Of<KraidRisePhase>(KraidBodyStage.Rising, value);
    }

    /// <summary>The phase of his fight the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not fighting.</exception>
    public KraidFightPhase FightPhase
    {
        get => step is BossStagePhase<KraidBodyStage>.Of<KraidFightPhase> { Stage: KraidBodyStage.Fighting } inStage ? inStage.Phase : throw NotIn(KraidBodyStage.Fighting);
        internal set => step = new BossStagePhase<KraidBodyStage>.Of<KraidFightPhase>(KraidBodyStage.Fighting, value);
    }

    /// <summary>The phase of his growth the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not growing.</exception>
    public KraidGrowthPhase GrowthPhase
    {
        get => step is BossStagePhase<KraidBodyStage>.Of<KraidGrowthPhase> { Stage: KraidBodyStage.Growing } inStage ? inStage.Phase : throw NotIn(KraidBodyStage.Growing);
        internal set => step = new BossStagePhase<KraidBodyStage>.Of<KraidGrowthPhase>(KraidBodyStage.Growing, value);
    }

    /// <summary>The phase of his death the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not dying.</exception>
    public KraidDeathPhase DeathPhase
    {
        get => step is BossStagePhase<KraidBodyStage>.Of<KraidDeathPhase> { Stage: KraidBodyStage.Dying } inStage ? inStage.Phase : throw NotIn(KraidBodyStage.Dying);
        internal set => step = new BossStagePhase<KraidBodyStage>.Of<KraidDeathPhase>(KraidBodyStage.Dying, value);
    }

    /// <summary>
    /// Whether Kraid fights (#1452, #1737): his fight and growth. Only then does his body's touch hurt Samus and a mouth hit
    /// that leaves him without health start his death; his rise through the floor and his death lie outside it.
    /// </summary>
    internal bool Fights => Stage is KraidBodyStage.Fighting or KraidBodyStage.Growing;

    /// <summary>
    /// Whether Samus's shots collide with Kraid: while he fights, and through his death's fade, the update that shows his
    /// body and the one that starts his sinking. While he rises through the floor and once he sinks, his mouth and body
    /// passes let every shot through.
    /// </summary>
    internal bool ShotsCollide => Fights || Stage == KraidBodyStage.Dying && KraidDeathPhaseDefinitions.ShotsCollide.Contains(DeathPhase);

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

    /// <summary>The failure of reading a stage's phase while the body is in another.</summary>
    private InvalidOperationException NotIn(KraidBodyStage stage) =>
        new($"Kraid's body is in its {Stage} stage, not {stage}.");
}
