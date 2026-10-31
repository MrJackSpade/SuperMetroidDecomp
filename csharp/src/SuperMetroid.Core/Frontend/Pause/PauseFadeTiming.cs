namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Cartridge definition data for the four bank-$82 pause fades.</summary>
internal static class PauseFadeTiming
{
    /// <summary>
    /// $90:EA6E, $82:8D2F, $82:A5C9 and $82:937C initialize both fade words
    /// to one. $80:8924/$80:894D consume a counter-only frame before each change.
    /// </summary>
    public const int CounterReload = 1;

    /// <summary>The fully lit screen brightness level, the top of the 0 (black) through 15 range.</summary>
    public const int FullyLit = 15;
}
