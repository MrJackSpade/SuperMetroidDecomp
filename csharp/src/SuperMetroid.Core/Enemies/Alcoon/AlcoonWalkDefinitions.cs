namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The random length of an Alcoon's walk.</summary>
internal static class AlcoonWalkDefinitions
{
    /// <summary>
    /// The random field that sets how many steps an Alcoon starting to walk takes: bits zero and one, a zero reading as
    /// two steps.
    /// </summary>
    internal static readonly RandomSampleField StepCount = new(0, 2);
}
