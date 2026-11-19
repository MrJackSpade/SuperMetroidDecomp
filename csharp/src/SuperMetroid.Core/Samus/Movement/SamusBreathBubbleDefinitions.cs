namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How the bubbles Samus breathes out underwater choose their sound (#1548).</summary>
internal static class SamusBreathBubbleDefinitions
{
    /// <summary>Whether a breath's bubbles make the low bubbling sound rather than the high one (#2479): one breath in two.</summary>
    internal static readonly RandomChance SoundsLow = RandomChance.Highest(1, new RandomSampleField(0, 1));
}
