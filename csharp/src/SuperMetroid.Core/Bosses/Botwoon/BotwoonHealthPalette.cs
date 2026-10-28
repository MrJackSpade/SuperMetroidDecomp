namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's health-band palette progression: when its health has fallen past the next band's threshold.</summary>
internal static class BotwoonHealthPalette
{
    /// <summary>
    /// Returns whether the current health is below the threshold selected by an even native
    /// byte offset $00..$0E. The signed subtraction matches the cartridge's CMP/BPL result.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_B3.asm:
    /// $B3:981B has threshold(i)=375*(8-i), i=0..7. Divide the validated even byte
    /// offset by two before multiplying; subtract in sixteen bits and interpret the
    /// result as signed, matching CMP/BPL at $B3:983B. All ushort health values remain
    /// supported, including wrapped differences. Offset 16 is caller-owned completion,
    /// not an extrapolated threshold.
    /// </remarks>
    public static bool ShouldAdvance(int phase, int health)
    {
        if (phase is < 0 or >= BotwoonHealthPaletteDefinitions.CompletePhase)
            throw new InvalidDataException($"Botwoon palette phase {phase} is outside the eight retail threshold entries.");

        int threshold = BotwoonHealthPaletteDefinitions.HealthPerThreshold * (BotwoonHealthPaletteDefinitions.PaletteCount - phase);
        return unchecked((short)(health - threshold)) < 0;
    }
}
