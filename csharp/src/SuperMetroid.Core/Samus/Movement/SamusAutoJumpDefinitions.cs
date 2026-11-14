namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Predicates belonging to the one-shot auto-jump input.</summary>
public static class SamusAutoJumpDefinitions
{
    /// <summary>Compares the nonzero auto-jump timer against nine using signed wrapped subtraction.</summary>
    public const int TimerComparisonLimit = 9;
}
