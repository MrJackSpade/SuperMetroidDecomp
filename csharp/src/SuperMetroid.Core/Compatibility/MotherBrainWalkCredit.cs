namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Mother Brain's walk credit (#1735): standing turns add credit, a full <see cref="StepCredit"/> sends her a step
/// forward, and the player's hits take credit away so sustained fire holds her back. The credit is a wrapping count
/// (<see cref="WrappingCounter"/>), and how it wraps is gameplay: earning credit adds without a ceiling, so credit earned
/// past the count's top starts again from zero and she spends more turns below one step, while a hit's cost is kept
/// only when the remainder does not read as overdrawn, the game reading a remainder as overdrawn when it reads below
/// zero, as any count from 32768 up does; an overdrawn remainder empties the credit instead. Every operation on the
/// credit lives here, so both rules are stated once.
/// </summary>
/// <remarks>
/// Consulted on Mother Brain's standing turns while she walks toward her target in her second phase and during the
/// phase-three sequence, which earn credit and test it for a step or for none left, and by her shot reactions when a
/// hit lands, which spend it. Evidence: the original walk routines add the earned credit with no ceiling and compare it
/// unsigned, and the shot reactions subtract the cost and test the result's sign. Phase three earns credit on every
/// standing turn while she walks toward her target, so the credit is not bounded by one step; the Mother Brain
/// encoding-domain check (<c>MotherBrainEncodingDomainsVerification</c>) exercises both rules at the count's ends and on both sides of the point where a remainder reads below zero.
/// </remarks>
[CompatibilityService("Mother Brain's walk credit wraps past its top when earned, and a hit that leaves it reading below zero empties it")]
public readonly record struct MotherBrainWalkCredit
{
    /// <summary>Credit for one walking step: earning reaches it to step forward, and an ordinary hit costs it.</summary>
    public const int StepCredit = 256;

    /// <summary>The cost of one Hyper Beam hit in the final phase, which recoils her when it overdraws the credit.</summary>
    public const int HyperBeamShotCost = 266;

    /// <summary>Stores the credit, 0 through <see cref="WrappingCounter.LargestCount"/>; callers use <see cref="Of"/>.</summary>
    internal MotherBrainWalkCredit(int credit) => Credit = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, credit);

    /// <summary>The credit, 0 through <see cref="WrappingCounter.LargestCount"/>, for the snapshot codec and parity checks.</summary>
    public int Credit { get; }

    /// <summary>No credit at all.</summary>
    public static MotherBrainWalkCredit Empty => default;

    /// <summary>Whether no credit remains; a standing turn with no credit takes the retreating branch.</summary>
    public bool IsEmpty => Credit == 0;

    /// <summary>Whether the credit is below one step.</summary>
    public bool BelowStep => Credit < StepCredit;

    /// <summary>A stated credit of 0 through <see cref="WrappingCounter.LargestCount"/>, as the encounter assigns after a step or a retreat.</summary>
    public static MotherBrainWalkCredit Of(int credit) => new(credit);

    /// <summary>Earns <paramref name="credit"/> on a standing turn; past the count's top the sum starts again from zero.</summary>
    /// <param name="credit">The credit a standing turn earns, 0 through <see cref="WrappingCounter.LargestCount"/>.</param>
    public MotherBrainWalkCredit AddCredit(int credit) => new(WrappingCounter.AddCount(Credit, credit));

    /// <summary>Whether a hit costing <paramref name="cost"/> leaves a remainder that does not read as overdrawn.</summary>
    /// <param name="cost">The hit's cost, 0 through <see cref="WrappingCounter.LargestCount"/>.</param>
    public bool CoversShot(int cost) => !WrappingCounter.ReadsBelowZero(Remainder(cost));

    /// <summary>Spends a hit's <paramref name="cost"/>: the remainder when <see cref="CoversShot"/>, otherwise no credit.</summary>
    /// <param name="cost">The hit's cost, 0 through <see cref="WrappingCounter.LargestCount"/>.</param>
    public MotherBrainWalkCredit SpendForShot(int cost) =>
        CoversShot(cost) ? new(Remainder(cost)) : Empty;

    /// <summary>The credit less <paramref name="cost"/>, wrapping below zero to the count's top.</summary>
    private int Remainder(int cost) => WrappingCounter.SubtractCount(Credit, cost);

    /// <inheritdoc />
    public override string ToString() => Credit.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
