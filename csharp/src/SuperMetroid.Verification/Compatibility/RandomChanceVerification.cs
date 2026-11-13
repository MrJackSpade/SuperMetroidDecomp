using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Compatibility;

/// <summary>
/// Confirms the rules of the random chance and the weighted random choice (#1619): a chance is taken by exactly its run of
/// the field's values at the bottom or the top of the field's range, a weighted choice gives each run of values to its
/// outcome in order, and neither accepts shares that leave a value undecided or decide nothing.
/// </summary>
internal static class RandomChanceVerification
{
    /// <summary>A field of the sample's bits 4-9, so the checks see a field that neither starts at the lowest bit nor ends at the top.</summary>
    private static readonly RandomSampleField MiddleField = new(4, 6);

    /// <summary>Counters a roll is advanced by: none, within the field's range, past it, and the largest an enemy's count reaches.</summary>
    private static readonly int[] AdvancingCounters = [0, 1, 63, 64, 100, 65535];

    /// <summary>Compares chances and choices on a middle field and the whole sample with their threshold expressions, and checks their rejections.</summary>
    internal static void VerifyRandomChanceRules()
    {
        RandomChance lowest = RandomChance.Lowest(5, MiddleField);
        RandomChance highest = RandomChance.Highest(7, MiddleField);
        RandomChance wholeLowest = RandomChance.Lowest(1365, RandomSampleField.Whole);
        RandomChance wholeHighest = RandomChance.Highest(1, RandomSampleField.Whole);
        var choice = new WeightedRandomChoice<char>(MiddleField, new(3, 'a'), new(60, 'b'), new(1, 'c'));
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            int field = (sample >> 4) & 63;
            AssertSameAt(field < 5, lowest.IsTakenBy(sample), "lowest five of the middle field", sample);
            AssertSameAt(field >= 57, highest.IsTakenBy(sample), "highest seven of the middle field", sample);
            AssertSameAt(sample < 1365, wholeLowest.IsTakenBy(sample), "lowest 1365 samples", sample);
            AssertSameAt(sample == 65535, wholeHighest.IsTakenBy(sample), "highest sample", sample);
            AssertSameAt(field < 3 ? 'a' : field < 63 ? 'b' : 'c', choice.Choose(sample), "weighted choice of the middle field", sample);
            foreach (int counter in AdvancingCounters)
            {
                int advanced = (field + counter) % 64;
                AssertSameAt(advanced, MiddleField.OfAdvancedBy(sample, counter), $"middle field advanced by {counter}", sample);
                AssertSameAt(advanced < 5, lowest.IsTakenBy(sample, counter), $"lowest five of the middle field advanced by {counter}", sample);
                AssertSameAt(advanced < 3 ? 'a' : advanced < 63 ? 'b' : 'c', choice.Choose(sample, counter), $"weighted choice advanced by {counter}", sample);
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(() => MiddleField.OfAdvancedBy(0, -1), "a roll advanced by a negative count");

        AssertEqual(5, lowest.TakingValues, "a chance counts the values that take it");
        AssertEqual(MiddleField, highest.Field, "a chance keeps the field it reads");
        AssertEqual(65536, RandomSampleField.Whole.Outcomes, "the whole sample reads every value");
        AssertThrows<ArgumentOutOfRangeException>(() => RandomChance.Lowest(0, MiddleField), "a chance no value takes");
        AssertThrows<ArgumentOutOfRangeException>(() => RandomChance.Highest(64, MiddleField), "a chance every value takes");
        AssertThrows<ArgumentException>(() => new WeightedRandomChoice<char>(MiddleField, new(3, 'a'), new(60, 'b')), "shares leaving a value undecided");
        AssertThrows<ArgumentException>(() => new WeightedRandomChoice<char>(MiddleField, new WeightedRandomOutcome<char>(64, 'a')), "a choice with one outcome");
        AssertThrows<ArgumentException>(() => new WeightedRandomChoice<char>(MiddleField, new(0, 'a'), new(64, 'b')), "an outcome with no values");
        Console.WriteLine("Random chance rules: every sample takes its chances and choices as their threshold expressions, and undecided shares are rejected.");
    }
}
