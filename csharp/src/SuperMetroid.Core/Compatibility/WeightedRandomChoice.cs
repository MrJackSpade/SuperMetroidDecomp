using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A choice a random sample makes among weighted outcomes (#1619): the values one field of the sample can read, split in
/// order from the lowest into runs, each run choosing its outcome. The original made these choices by comparing its random
/// word, or a field of it, with ascending thresholds; a catalog names the choice, the field it reads and each outcome's
/// share of the field's values instead of the thresholds, and this type is the one place that maps a sample to one.
/// </summary>
/// <remarks>
/// Made at every such choice, on the sample the choice reads, through the named choice its family's catalog gives. A choice
/// between taking one outcome or not is a <see cref="RandomChance"/>. Evidence: each choice's original chain of threshold
/// comparisons, which a weighted choice reproduces for every one of the 65536 samples; the SMV replays compare the random
/// value and every outcome it drives on every update.
/// </remarks>
/// <typeparam name="TOutcome">What the choice chooses.</typeparam>
[CompatibilityService("which outcome a random sample chooses among weighted outcomes, exactly as the original compared its random word, or a field of it, with ascending thresholds")]
public sealed class WeightedRandomChoice<TOutcome>
{
    /// <summary>The outcomes in order from the field's lowest values, each with its share.</summary>
    private readonly ImmutableArray<WeightedRandomOutcome<TOutcome>> _outcomes;

    /// <summary>A choice that reads <paramref name="field"/> and gives its values, from the lowest, to <paramref name="outcomes"/> in order.</summary>
    /// <param name="field">The field of the sample the choice reads.</param>
    /// <param name="outcomes">Each outcome with how many of the field's values choose it, in order from the lowest values.</param>
    /// <exception cref="ArgumentException">An outcome has no values, or the shares do not add up to every value the field can read.</exception>
    public WeightedRandomChoice(RandomSampleField field, params ReadOnlySpan<WeightedRandomOutcome<TOutcome>> outcomes)
    {
        int covered = 0;
        foreach (WeightedRandomOutcome<TOutcome> outcome in outcomes)
            covered += Ensure.BetweenInclusive(1, field.Outcomes, outcome.Values);
        if (outcomes.Length < 2 || covered != field.Outcomes)
        {
            throw new ArgumentException(
                $"A weighted random choice gives all {field.Outcomes} values of its field to two or more outcomes; these give {covered} to {outcomes.Length}.",
                nameof(outcomes));
        }

        Field = field;
        _outcomes = [.. outcomes];
    }

    /// <summary>The field of the sample the choice reads.</summary>
    public RandomSampleField Field { get; }

    /// <summary>The outcome <paramref name="sample"/> chooses.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    public TOutcome Choose(int sample) => ChooseAt(Field.Of(sample));

    /// <summary>
    /// The outcome <paramref name="sample"/> chooses once its field's roll has advanced <paramref name="advancedBy"/> places
    /// (#1619), as <see cref="RandomSampleField.OfAdvancedBy"/> advances it: a choice the original made by adding a counter
    /// to the field it drew before comparing the sum with its thresholds.
    /// </summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    /// <param name="advancedBy">The counter added to the field's roll, zero or more.</param>
    public TOutcome Choose(int sample, int advancedBy) => ChooseAt(Field.OfAdvancedBy(sample, advancedBy));

    /// <summary>The outcome the field's value <paramref name="fieldValue"/> chooses.</summary>
    /// <param name="fieldValue">A value of <see cref="Field"/>, 0 through its outcomes less one.</param>
    private TOutcome ChooseAt(int fieldValue)
    {
        int value = fieldValue;
        foreach (WeightedRandomOutcome<TOutcome> outcome in _outcomes)
        {
            if (value < outcome.Values)
                return outcome.Outcome;
            value -= outcome.Values;
        }

        throw new InvalidOperationException($"Field value {fieldValue} lies past the choice's {Field.Outcomes} values.");
    }
}
