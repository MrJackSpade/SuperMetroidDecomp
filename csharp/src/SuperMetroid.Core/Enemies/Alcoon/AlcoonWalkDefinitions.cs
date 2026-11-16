namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The random length of an Alcoon's walk.</summary>
internal static class AlcoonWalkDefinitions
{
    /// <summary>
    /// The random field that sets how many steps an Alcoon starting to walk takes: the sample modulo 4, a zero reading as
    /// two steps.
    /// </summary>
    internal static readonly RandomSampleField StepCount = new(0, 2);

    /// <summary>The steps an Alcoon takes when <see cref="StepCount"/> reads zero.</summary>
    internal const int StepsForZeroSample = 2;

    /// <summary>How far an Alcoon walking either way moves each movement step: two pixels.</summary>
    internal static SignedSixteenSixteen StepDistance { get; } = SignedSixteenSixteen.FromWholePixels(2);
}
