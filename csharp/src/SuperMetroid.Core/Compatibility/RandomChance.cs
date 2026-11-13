namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A chance a random choice takes (#1619): of the values one field of a random sample can read, a run at the bottom or at
/// the top of the field's range takes the chance and every other value passes it by. The original took these chances by
/// comparing its random word, or a field of it, with a threshold; a catalog names the chance, the field it reads and how
/// many of the field's values take it instead of the threshold, and this type is the one place that compares a sample with
/// one.
/// </summary>
/// <remarks>
/// Taken at every such choice, on the sample the choice reads, through the named chance its family's catalog gives for what
/// the choice decides. A choice among more than two outcomes is a <see cref="WeightedRandomChoice{TOutcome}"/>. Evidence:
/// each choice's original comparison of its random word or field with a threshold, which a chance reproduces for every one
/// of the 65536 samples; the SMV replays compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which random samples take each chance, exactly as the original compared its random word, or a field of it, with a threshold")]
public readonly record struct RandomChance
{
    /// <summary>Retains a chance taken by <paramref name="takingValues"/> values of the field, the lowest from <paramref name="first"/>.</summary>
    private RandomChance(RandomSampleField field, int first, int takingValues)
    {
        Field = field;
        First = first;
        TakingValues = takingValues;
    }

    /// <summary>The field of the sample the chance reads.</summary>
    public RandomSampleField Field { get; }

    /// <summary>How many of the values <see cref="Field"/> can read take the chance, 1 through all but one of them.</summary>
    public int TakingValues { get; }

    /// <summary>The lowest field value that takes the chance.</summary>
    private int First { get; }

    /// <summary>A chance taken by the <paramref name="values"/> lowest values <paramref name="field"/> reads.</summary>
    /// <param name="values">How many of the field's values take the chance, 1 through all but one of them.</param>
    /// <param name="field">The field of the sample the chance reads.</param>
    /// <exception cref="ArgumentOutOfRangeException">The chance would be taken by no value or by every value.</exception>
    public static RandomChance Lowest(int values, RandomSampleField field) =>
        new(field, 0, Ensure.BetweenInclusive(1, field.Outcomes - 1, values));

    /// <summary>A chance taken by the <paramref name="values"/> highest values <paramref name="field"/> reads.</summary>
    /// <param name="values">How many of the field's values take the chance, 1 through all but one of them.</param>
    /// <param name="field">The field of the sample the chance reads.</param>
    /// <exception cref="ArgumentOutOfRangeException">The chance would be taken by no value or by every value.</exception>
    public static RandomChance Highest(int values, RandomSampleField field) =>
        new(field, field.Outcomes - Ensure.BetweenInclusive(1, field.Outcomes - 1, values), values);

    /// <summary>Whether the chance is taken on <paramref name="sample"/>.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public bool IsTakenBy(int sample) => IsTakenAt(Field.Of(sample));

    /// <summary>
    /// Whether the chance is taken on <paramref name="sample"/> once its field's roll has advanced
    /// <paramref name="advancedBy"/> places (#1619), as <see cref="RandomSampleField.OfAdvancedBy"/> advances it: a chance
    /// the original took by adding a counter to the field it drew before comparing the sum with its threshold.
    /// </summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    /// <param name="advancedBy">The counter added to the field's roll, zero or more.</param>
    public bool IsTakenBy(int sample, int advancedBy) => IsTakenAt(Field.OfAdvancedBy(sample, advancedBy));

    /// <summary>Whether the chance is taken when its field reads <paramref name="value"/>.</summary>
    /// <param name="value">A value of <see cref="Field"/>, 0 through its outcomes less one.</param>
    private bool IsTakenAt(int value) => value >= First && value - First < TakingValues;
}
