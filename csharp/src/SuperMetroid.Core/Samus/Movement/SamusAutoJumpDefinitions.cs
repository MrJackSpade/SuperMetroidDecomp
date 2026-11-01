namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Predicates belonging to the one-shot auto-jump input handler.</summary>
public static class SamusAutoJumpDefinitions
{
    /// <summary>Compares the nonzero auto-jump timer against nine using signed 16-bit subtraction.</summary>
    public const int TimerComparisonLimit = 9;
}
