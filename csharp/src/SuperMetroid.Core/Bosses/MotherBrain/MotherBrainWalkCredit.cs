namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's walk credit: standing turns add credit, a full <see cref="StepCredit"/> sends her a step forward,
/// and the player's hits take credit away so sustained fire holds her back. The credit is one wrapping word read two
/// ways, and both readings are gameplay: earning credit adds without saturation and compares unsigned (a wrapped
/// credit spends more turns below one step), while a shot's cost is subtracted and kept only when the result is
/// non-negative as a signed word, otherwise the credit is emptied. Every operation on the credit lives here, so the
/// word's width and both readings are stated once.
/// </summary>
[EncodingOwner("Mother Brain walk credit word")]
public readonly record struct MotherBrainWalkCredit
{
    /// <summary>Credit for one walking step: earning reaches it to step forward, and an ordinary hit costs it.</summary>
    public const int StepCredit = 256;

    /// <summary>The cost of one Hyper Beam hit in the final phase, which recoils her when it overdraws the credit.</summary>
    public const int HyperBeamShotCost = 266;

    /// <summary>Stores the exact credit word; callers use <see cref="Of"/>.</summary>
    internal MotherBrainWalkCredit(ushort word) => Word = word;

    /// <summary>The exact credit word, for the snapshot codec and word-level parity checks.</summary>
    public ushort Word { get; }

    /// <summary>No credit at all.</summary>
    public static MotherBrainWalkCredit Empty => default;

    /// <summary>Whether no credit remains; a standing turn with no credit takes the retreating branch.</summary>
    public bool IsEmpty => Word == 0;

    /// <summary>Whether the credit is below one step, compared unsigned as earning compares it.</summary>
    public bool BelowStep => Word < StepCredit;

    /// <summary>A stated credit of 0 through 65,535, as the encounter assigns after a step or a retreat.</summary>
    public static MotherBrainWalkCredit Of(int credit) => new(checked((ushort)credit));

    /// <summary>Earns <paramref name="credit"/> on a standing turn; the sum wraps rather than saturating.</summary>
    public MotherBrainWalkCredit AddCredit(int credit) => new(unchecked((ushort)(Word + credit)));

    /// <summary>Whether a hit costing <paramref name="cost"/> leaves a credit that is non-negative as a signed word.</summary>
    public bool CoversShot(int cost) => unchecked((short)(Word - cost)) >= 0;

    /// <summary>Spends a hit's <paramref name="cost"/>: the remainder when <see cref="CoversShot"/>, otherwise no credit.</summary>
    public MotherBrainWalkCredit SpendForShot(int cost) =>
        CoversShot(cost) ? new(unchecked((ushort)(Word - cost))) : Empty;

    /// <inheritdoc />
    public override string ToString() => Word.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
