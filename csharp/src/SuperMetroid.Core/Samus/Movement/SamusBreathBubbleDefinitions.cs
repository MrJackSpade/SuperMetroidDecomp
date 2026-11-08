namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How the bubbles Samus breathes out underwater choose their sound (#1548).</summary>
internal static class SamusBreathBubbleDefinitions
{
    /// <summary>The random selection, set for the low bubbling sound and clear for the high one.</summary>
    internal static readonly RandomSampleField LowSound = new(0, 1);
}
