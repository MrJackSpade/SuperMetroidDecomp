namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body's own state beside what every part holds: the stage he is in and the phase of it he runs, his
/// head-animation timer, his growth's next ceiling rock and the rising-rock timer of his rise through the floor.
/// </summary>
public sealed class KraidBodyState : KraidPartState
{
    /// <summary>Makes the body of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the body is part of.</param>
    internal KraidBodyState(KraidEnemyState kraid)
        : base(kraid)
    {
    }

    /// <summary>
    /// The stage the body is in (#2208), which names the type of the phase it runs; setting a stage's phase enters that
    /// stage. The body's setup puts it in its rise, or in its death in a room he was found defeated in.
    /// </summary>
    public KraidBodyStage Stage { get; private set; }

    /// <summary>The phase of his rise through the floor the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not rising.</exception>
    public KraidRisePhase RisePhase
    {
        get => Stage == KraidBodyStage.Rising ? field : throw NotIn(KraidBodyStage.Rising);
        internal set
        {
            field = value;
            Stage = KraidBodyStage.Rising;
        }
    }

    /// <summary>The phase of his fight the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not fighting.</exception>
    public KraidFightPhase FightPhase
    {
        get => Stage == KraidBodyStage.Fighting ? field : throw NotIn(KraidBodyStage.Fighting);
        internal set
        {
            field = value;
            Stage = KraidBodyStage.Fighting;
        }
    }

    /// <summary>The phase of his growth the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not growing.</exception>
    public KraidGrowthPhase GrowthPhase
    {
        get => Stage == KraidBodyStage.Growing ? field : throw NotIn(KraidBodyStage.Growing);
        internal set
        {
            field = value;
            Stage = KraidBodyStage.Growing;
        }
    }

    /// <summary>The phase of his death the body runs.</summary>
    /// <exception cref="InvalidOperationException">The body is not dying.</exception>
    public KraidDeathPhase DeathPhase
    {
        get => Stage == KraidBodyStage.Dying ? field : throw NotIn(KraidBodyStage.Dying);
        internal set
        {
            field = value;
            Stage = KraidBodyStage.Dying;
        }
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
    internal bool ShotsCollide => Fights || Stage == KraidBodyStage.Dying && DeathPhase is KraidDeathPhase.Initialize or
        KraidDeathPhase.FadeOut or KraidDeathPhase.ShowBody or KraidDeathPhase.StartSinking;

    /// <summary>
    /// The body's next growth ceiling rock, one of <see cref="KraidCeilingRocks.Count"/> rocks; equal to the count once
    /// every rock has fallen.
    /// </summary>
    public int CeilingRock
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, KraidCeilingRocks.Count, value);
    }

    /// <summary>The updates left in the body's current rising-rock phase; a rock falls at each multiple of its cadence.</summary>
    public int RisingRockTimer { get; internal set; }

    /// <summary>The body's head-animation timer.</summary>
    public int HeadTimer { get; internal set; }

    /// <summary>The failure of reading a stage's phase while the body is in another.</summary>
    private InvalidOperationException NotIn(KraidBodyStage stage) =>
        new($"Kraid's body is in its {Stage} stage, not {stage}.");
}
