namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Post-credits BG3 scroll program and artwork identities.</summary>
internal static class EndingWavySamusDefinitions
{
    /// <summary>$88:EC48 / Spawn_WavySamus_HDMAObject seeds unsigned 8.8 amplitude $4000.</summary>
    internal const int Amplitude = 0x4000;
    /// <summary>$88:EC4E / Spawn_WavySamus_HDMAObject seeds phase delta eight, doubled to a byte index.</summary>
    internal const int PhaseByteStep = 8 * 2;
    /// <summary>$88:ECA2 / Instruction_Setup_WavySamus initializes the phase to minus two.</summary>
    internal const int InitialPhase = -2;
    /// <summary>$88:ECD1 / PreInstruction_WavySamus advances four sine-table bytes per first-half sample.</summary>
    internal const int SampleByteStep = 4;
    /// <summary>$88:ECD6 / PreInstruction_WavySamus produces 64 words and a sign-inverted second half.</summary>
    internal const int HalfCycle = 64;
    /// <summary>$8B:E193 / CinematicFunction_PostCredits_WavySamus advances BG3Y by two per call.</summary>
    internal const int VerticalStep = 2;
}
