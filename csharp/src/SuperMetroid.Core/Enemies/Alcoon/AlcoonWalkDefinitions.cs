namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The random length of an Alcoon's walk.</summary>
internal static class AlcoonWalkDefinitions
{
    /// <summary>
    /// How many walk cycles an Alcoon starting to walk takes before its next volley, chosen from four rolls (#2510): a roll
    /// of one through three takes that many cycles and a roll of none two.
    /// </summary>
    internal static readonly WeightedRandomChoice<int> WalkCycles = WeightedRandomChoice<int>.InTurn(new RandomSampleField(0, 2), 2, 1, 2, 3);

    /// <summary>How far an Alcoon walking either way moves each stride: two pixels.</summary>
    internal static SignedSixteenSixteen StrideDistance { get; } = SignedSixteenSixteen.FromWholePixels(2);
}
