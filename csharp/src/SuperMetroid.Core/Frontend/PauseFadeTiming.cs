namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge definition data for the four bank-$82 pause fades.</summary>
internal static class PauseFadeTiming
{
    /// <summary>
    /// $90:EA6E, $82:8D2F, $82:A5C9 and $82:937C initialize both fade words
    /// to one. $80:8924/$80:894D consume a counter-only frame before each change.
    /// </summary>
    public const int CounterReload = 1;

    /// <summary>INIDISP's fully lit four-bit brightness value.</summary>
    public const byte FullyLit = 15;
}
