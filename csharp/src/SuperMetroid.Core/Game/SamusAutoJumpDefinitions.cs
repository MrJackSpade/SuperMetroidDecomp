namespace SuperMetroid.Core.Game;

/// <summary>Cartridge predicates belonging to the one-shot auto-jump input handler.</summary>
public static class SamusAutoJumpDefinitions
{
    /// <summary>$90:E926 compares nonzero $0AF4 against nine using signed 16-bit subtraction.</summary>
    public const ushort TimerComparisonLimit = 9;
}
