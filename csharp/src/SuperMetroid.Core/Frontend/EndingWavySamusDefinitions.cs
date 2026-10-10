using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>Post-credits BG3 scroll program and artwork identities.</summary>
internal static class EndingWavySamusDefinitions
{
    /// <summary>$8B:8400 / Setup_PPU_Credits selects the transformation map at VRAM word $2400.</summary>
    internal const ushort TilemapWord = 0x2400;
    /// <summary>$8B:8413 / Setup_PPU_Credits selects BG3 characters at VRAM word $2000.</summary>
    internal const ushort CharacterWord = 0x2000;
    /// <summary>$88:EC48 / Spawn_WavySamus_HDMAObject seeds unsigned 8.8 amplitude $4000.</summary>
    private const int Amplitude = 0x4000;
    /// <summary>$88:EC4E / Spawn_WavySamus_HDMAObject seeds phase delta eight, doubled to a byte index.</summary>
    private const int PhaseByteStep = 8 * 2;
    /// <summary>$88:ECA2 / Instruction_Setup_WavySamus initializes the phase to minus two.</summary>
    private const int InitialPhase = -2;
    /// <summary>$88:ECD1 / PreInstruction_WavySamus advances four sine-table bytes per first-half sample.</summary>
    private const int SampleByteStep = 4;
    /// <summary>$88:ECD6 / PreInstruction_WavySamus produces 64 words and a sign-inverted second half.</summary>
    private const int HalfCycle = 64;
    /// <summary>$8B:E193 / CinematicFunction_PostCredits_WavySamus advances BG3Y by two per call.</summary>
    private const int VerticalStep = 2;

    /// <summary>
    /// Builds the 224 BG3 line-scroll records for a post-credits waiting-backdrop age using its native wave phase and vertical step.
    /// </summary>
    /// <param name="age">Elapsed waiting-backdrop calls; age one is setup-only and the sine displacement begins at age two.</param>
    /// <returns>Per-scanline horizontal wave displacement and age-derived vertical scroll.</returns>
    internal static BackgroundLineScroll[] ScrollsAtAge(int age)
    {
        var result = new BackgroundLineScroll[224];
        // Global HDMA precedes the cinematic dispatcher. Age one executes setup only;
        // age two is the first pre-instruction. Derive this deterministic owner from
        // the existing cinematic countdown so debugger restores retain exact phase.
        int phase = InitialPhase + PhaseByteStep * (age - 1);
        for (int line = 0; line < result.Length; line++)
        {
            int displacement = 0;
            if (age >= 2)
            {
                int sample = line % HalfCycle;
                short sine = EnemyTrigonometryTables.SignedSine(unchecked((byte)((phase + sample * SampleByteStep) >> 1)));
                int magnitude = Math.Abs((int)sine) * Amplitude >> 16;
                displacement = sine < 0 ? -magnitude : magnitude;
                if ((line / HalfCycle & 1) != 0) displacement = -displacement;
            }
            result[line] = new(unchecked((ushort)displacement), unchecked((ushort)(age * VerticalStep)));
        }
        return result;
    }
}
