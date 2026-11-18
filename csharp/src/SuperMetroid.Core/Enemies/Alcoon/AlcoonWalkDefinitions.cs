namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The random length of an Alcoon's walk.</summary>
internal static class AlcoonWalkDefinitions
{
    /// <summary>
    /// The random field that sets how many walk cycles an Alcoon starting to walk takes before its next volley, a zero
    /// reading as two cycles.
    /// </summary>
    internal static readonly RandomSampleField WalkCycleCount = new(0, 2);

    /// <summary>The walk cycles an Alcoon takes when <see cref="WalkCycleCount"/> reads zero.</summary>
    internal const int WalkCyclesForZeroSample = 2;

    /// <summary>How far an Alcoon walking either way moves each stride: two pixels.</summary>
    internal static SignedSixteenSixteen StrideDistance { get; } = SignedSixteenSixteen.FromWholePixels(2);
}
