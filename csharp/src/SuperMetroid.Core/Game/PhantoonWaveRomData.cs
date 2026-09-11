namespace SuperMetroid.Core.Game;

/// <summary>Native wavy-Phantoon HDMA definitions from bank $88.</summary>
public static class PhantoonWaveRomData
{
    /// <summary>$88:E4BD wave setup initializes HDMA phase to minus two before the first increment.</summary>
    public const ushort InitialPhase = 0xfffe;
    /// <summary>$A7:D535 spawns the 64-line introductory wave.</summary>
    public const ushort IntroMode = 2;
    /// <summary>$A7:DA51 spawns the 128-line final-death wave.</summary>
    public const ushort DeathMode = 1;
    /// <summary>$A0:B443: signed sine words, kSinCosTable8bit_Sext + 64, read by $88:E567.</summary>
    public const int SineWords = 0xa0b443;
    /// <summary>$88:E567 PreInstruction_WavyPhantoon: phase is a nine-bit byte offset into signed word samples.</summary>
    public const int PhaseMask = 0x01ff;
    /// <summary>$88:E4C6: proven mode bit selecting the 128-line rather than 64-line wave.</summary>
    public const int LongWaveModeBit = 1;
    /// <summary>$88:E567 PreInstruction_WavyPhantoon: normal-mode half-cycle has 32 samples.</summary>
    public const int ShortHalfCycle = 32;
    /// <summary>$88:E567 PreInstruction_WavyPhantoon: long-mode half-cycle has 64 samples.</summary>
    public const int LongHalfCycle = 64;
}
