namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How long a pending auto-jump still reads as a fresh Jump press.</summary>
public static class SamusAutoJumpDefinitions
{
    /// <summary>The held updates below which a pending auto-jump still reads as a fresh Jump press.</summary>
    public const int TimerComparisonLimit = 9;
}
