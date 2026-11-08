namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The random field a Yard reads while it crawls.</summary>
internal static class YardDecisions
{
    /// <summary>The random bit that makes a crawling Yard hold its previous pose for another step: bit zero.</summary>
    internal static readonly RandomSampleField RepeatPreviousPose = new(0, 1);
}
