using static SuperMetroid.Core.Bosses.Ridley.CeresRidleyEyeFadeDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Steps Ceres Ridley's eye fade through its authored row schedule.</summary>
internal static class CeresRidleyEyeFade
{
    /// <summary>
    /// Resolves one authored scheduler offset. The first sixteen frames select rows
    /// fifteen down through zero; the remaining 48 frames hold row zero before completion.
    /// </summary>
    /// <remarks>
    /// #625 / #651 exact NTSC J/U v1.0 algorithm: offset 0..15 selects row
    /// 15-offset, offset 16..63 selects row zero, and offset 64 is the $FF
    /// completion marker. All 65 bytes at $A6:E269-$E2A9 match the pinned ROM
    /// and bank-A6 disassembly. Native code reads a word at each byte offset
    /// and masks its low byte; the overlapping high byte is discarded. Offset
    /// 65 would read the adjacent eye-palette byte and is outside this schedule.
    /// VerifyCeresRidleyEyeFadeDefinitions checks every row and the terminal
    /// transition through the real fade consumer with schedule reads forbidden.
    /// </remarks>
    public static CeresRidleyEyeFadeStep Get(int offset) => offset switch
    {
        < 0 => throw new InvalidDataException($"Ceres Ridley eye-fade offset {offset} precedes the authored schedule."),
        < 16 => new(false, 15 - offset),
        < PaletteStepCount => new(false, 0),
        PaletteStepCount => new(true, 0),
        _ => throw new InvalidDataException(
            $"Ceres Ridley eye-fade offset {offset} exceeds the authored " +
            $"{PaletteStepCount + 1}-entry schedule."),
    };
}
