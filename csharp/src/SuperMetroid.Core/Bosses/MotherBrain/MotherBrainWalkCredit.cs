namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's walk credit: standing turns add credit, a full <see cref="StepCredit"/> sends her a step forward,
/// and the player's hits take credit away so sustained fire holds her back. The credit counts around
/// <see cref="CreditSpan"/> units, and how it does is gameplay: earning credit adds without saturation, so a credit
/// earned past the span starts again from zero and spends more turns below one step, while a shot's cost is kept only
/// when the remainder is not overdrawn, the game reading any remainder in the upper half of the span as overdrawn,
/// otherwise the credit is emptied. Every operation on the credit lives here, so the span and both rules are stated
/// once.
/// </summary>
/// <remarks>
/// Evidence: the original walk routines add the earned credit with no saturation and compare it unsigned, and the shot
/// reactions subtract the cost and test the result's sign. Phase three earns credit on every standing turn while she
/// walks toward her target, so the credit is not bounded by one step; the encoding-domain checks exercise both rules
/// at the span's ends.
/// </remarks>
[EncodingOwner("Mother Brain walk credit")]
public readonly record struct MotherBrainWalkCredit
{
    /// <summary>Credit for one walking step: earning reaches it to step forward, and an ordinary hit costs it.</summary>
    public const int StepCredit = 256;

    /// <summary>The cost of one Hyper Beam hit in the final phase, which recoils her when it overdraws the credit.</summary>
    public const int HyperBeamShotCost = 266;

    /// <summary>The units the credit counts around: a credit earned past the last starts again from zero.</summary>
    public const int CreditSpan = 65536;

    /// <summary>Stores the credit, 0 through <see cref="CreditSpan"/> less one; callers use <see cref="Of"/>.</summary>
    internal MotherBrainWalkCredit(int credit) => Credit = Ensure.BetweenInclusive(0, CreditSpan - 1, credit);

    /// <summary>The credit, 0 through <see cref="CreditSpan"/> less one, for the snapshot codec and parity checks.</summary>
    public int Credit { get; }

    /// <summary>No credit at all.</summary>
    public static MotherBrainWalkCredit Empty => default;

    /// <summary>Whether no credit remains; a standing turn with no credit takes the retreating branch.</summary>
    public bool IsEmpty => Credit == 0;

    /// <summary>Whether the credit is below one step.</summary>
    public bool BelowStep => Credit < StepCredit;

    /// <summary>A stated credit of 0 through <see cref="CreditSpan"/> less one, as the encounter assigns after a step or a retreat.</summary>
    public static MotherBrainWalkCredit Of(int credit) => new(credit);

    /// <summary>Earns <paramref name="credit"/> on a standing turn; past the span the sum starts again from zero.</summary>
    public MotherBrainWalkCredit AddCredit(int credit) => new((Credit + Ensure.AtLeastZero(credit)) % CreditSpan);

    /// <summary>Whether a hit costing <paramref name="cost"/> leaves a remainder that is not overdrawn.</summary>
    public bool CoversShot(int cost) => Remainder(cost) < CreditSpan / 2;

    /// <summary>Spends a hit's <paramref name="cost"/>: the remainder when <see cref="CoversShot"/>, otherwise no credit.</summary>
    public MotherBrainWalkCredit SpendForShot(int cost) =>
        CoversShot(cost) ? new(Remainder(cost)) : Empty;

    /// <summary>The credit less <paramref name="cost"/>, counted around the span.</summary>
    private int Remainder(int cost) => ((Credit - Ensure.AtLeastZero(cost)) % CreditSpan + CreditSpan) % CreditSpan;

    /// <inheritdoc />
    public override string ToString() => Credit.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
