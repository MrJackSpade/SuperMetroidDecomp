namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$90 beam callback tables and their translated entry points.</summary>
public static class SamusBeamPreInstructionCodes
{
    /// <summary>$90:B96E, FireUnchargedBeam's low-nibble-indexed callback words.</summary>
    public const int UnchargedTable = 0x90b96e;
    /// <summary>$90:BA3E, FireChargedBeam's low-nibble-indexed callback words.</summary>
    public const int ChargedTable = 0x90ba3e;
    /// <summary>$90:AEF3, ProjPreInstr_Beam_NoWaveBeam: ordinary terrain-stopping beam motion.</summary>
    public const ushort NoWave = 0xaef3;
    /// <summary>$90:B0E4, ProjPreInstr_BeamOrIceWave: Wave movement with a three-frame trail reload.</summary>
    public const ushort WaveThreeFrameTrail = 0xb0e4;
    /// <summary>$90:B0C3, ProjPreInstr_WavePlasmaEtc: Wave movement with a four-frame trail reload.</summary>
    public const ushort WaveFourFrameTrail = 0xb0c3;
}
