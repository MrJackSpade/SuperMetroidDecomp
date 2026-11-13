namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The random choices of Phantoon's fight: its timers, patterns and figure-eight starts.</summary>
internal static class PhantoonRandomDefinitions
{
    /// <summary>Which of the eight vulnerable-window timers an opened eye tracks Samus for: the sample modulo 8.</summary>
    internal static readonly RandomSampleField VulnerableWindowChoice = new(0, 3);

    /// <summary>Which of the eight shot patterns and eye markers a hit in the vulnerable window starts: the sample modulo 8.</summary>
    internal static readonly RandomSampleField ShotPattern = new(0, 3);

    /// <summary>Whether the figure eight after the eye closes runs reversed: taken by an odd sample.</summary>
    internal static readonly RandomSampleField FigureEightReversed = new(0, 1);

    /// <summary>Which of the four casual flame patterns Phantoon's mouth spits next: the sample modulo 4.</summary>
    internal static readonly RandomSampleField CasualFlamePattern = new(0, 2);

    /// <summary>Whether the next figure eight starts on the right, at X 208: taken by an odd sample.</summary>
    internal static readonly RandomSampleField FigureEightStartsRight = new(0, 1);

    /// <summary>Which of the eight hiding timers Phantoon waits before its flame rain: the sample modulo 8.</summary>
    internal static readonly RandomSampleField RainHidingChoice = new(0, 3);

    /// <summary>Which of the eight flame rain patterns and placements Phantoon reappears with: the sample modulo 8.</summary>
    internal static readonly RandomSampleField FlameRainPattern = new(0, 3);

    /// <summary>Which of the eight eye-closed timers the second round's pattern waits: the sample modulo 8.</summary>
    internal static readonly RandomSampleField EyeClosedChoice = new(0, 3);
}
