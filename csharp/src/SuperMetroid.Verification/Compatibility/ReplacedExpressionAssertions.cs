namespace SuperMetroid.Verification.Compatibility;

/// <summary>
/// The comparison the exhaustive checks of named random chances, choices, counter phases and cadences (#1619) make at
/// every input: the named rule's answer against the threshold or mask expression it replaced.
/// </summary>
internal static class ReplacedExpressionAssertions
{
    /// <summary>
    /// Fails, naming the rule and the input, when <paramref name="named"/> differs from <paramref name="replaced"/>. The
    /// failure message is only built for a difference, so a check over every input stays cheap.
    /// </summary>
    /// <param name="replaced">What the replaced expression answered for the input.</param>
    /// <param name="named">What the named rule answers for the input.</param>
    /// <param name="rule">The rule compared.</param>
    /// <param name="input">The sample or counter value compared at.</param>
    internal static void AssertSameAt<T>(T replaced, T named, string rule, int input)
    {
        if (!EqualityComparer<T>.Default.Equals(replaced, named))
            AssertEqual(replaced, named, $"{rule} at {input}");
    }
}
