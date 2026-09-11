namespace SuperMetroid.Core.Game;

/// <summary>Bank-$88 semi-transparency owner definitions, independent of the wavy-scroll channel.</summary>
public static class PhantoonBlendingRomData
{
    /// <summary>$88:E44E tests bit 14 of the Phantoon layer word to select configuration $1A.</summary>
    public const ushort SemiTransparentBit = 0x4000;
    /// <summary>$88:E458 compares only the low control byte against $FF to delete the HDMA owner.</summary>
    public const byte DeleteControl = 0xff;
    /// <summary>$A7:CE96 installs a one-frame empty table, then the pre-instruction; neither setup call executes it.</summary>
    public const int SetupCalls = 2;
}
