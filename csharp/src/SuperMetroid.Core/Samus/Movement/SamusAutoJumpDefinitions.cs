namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Predicates belonging to the one-shot auto-jump input.</summary>
public static class SamusAutoJumpDefinitions
{
    /// <summary>The held updates below which a pending auto-jump still reads as a fresh Jump press.</summary>
    public const int TimerComparisonLimit = 9;
}
