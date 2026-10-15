namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Bank-$88 semi-transparency owner definitions, independent of the wavy-scroll channel.</summary>
public static class PhantoonBlendingRomData
{
    /// <summary>$88:E458 compares only the low control byte against $FF to delete the HDMA owner.</summary>
    public const byte DeleteControl = 0xff;
    /// <summary>The mouth's control word as the wavy fade-in starts (<c>Phantoon_SpawnFireballsBeforeFight</c>, $A7:D508): low byte one keeps the blending owner idle.</summary>
    public const ushort WavyFadeInControl = 0x8001;
    /// <summary>The mouth's control word once the death graphics clear: its <see cref="DeleteControl"/> low byte deletes the blending owner.</summary>
    public const ushort DeleteControlWord = 0xffff;
    /// <summary>$A7:CE96 installs a one-frame empty table, then the pre-instruction; neither setup call executes it.</summary>
    public const int SetupCalls = 2;
}
