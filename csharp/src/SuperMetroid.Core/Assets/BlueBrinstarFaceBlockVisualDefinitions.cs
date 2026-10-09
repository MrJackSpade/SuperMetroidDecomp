using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The five distinct bank-$A8 OAM compositions selected by the face block's
/// idle, Samus-left and Samus-right programs. Activation and frame timing stay
/// in the compiled enemy implementation.
/// </summary>
internal static class BlueBrinstarFaceBlockVisualDefinitions
{
    /// <summary>Native bank of the face-block enemy and its OAM maps.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>
    /// Builds the face block's five bank-$A8 OAM definitions in selector order:
    /// neutral, then the two Samus-left and two Samus-right poses.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, MapPointer(0), "face_block_neutral"),
        new(Bank, MapPointer(1), "face_block_samus_left_1"),
        new(Bank, MapPointer(2), "face_block_samus_left_2"),
        new(Bank, MapPointer(3), "face_block_samus_right_1"),
        new(Bank, MapPointer(4), "face_block_samus_right_2"),
    ];

    /// <summary>Only the seven visual operands in the three native programs.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (BlueBrinstarFaceBlockInstructionProgramDefinitions.IsPresentationWord(operandAddress))
        {
            int offset = operandAddress - BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft;
            if (offset >= 28) return MapPointer(0);
            int frame = (offset % 14 - 2) / 4;
            return MapPointer(frame == 0 ? 0 : frame + 2 * (offset / 14));
        }
        throw new InvalidDataException(
            $"Face-block visual operand $A8:{operandAddress:X4} is not compiled.");
    }
    // Two-byte count plus four five-byte OAM records per map.
    /// <summary>Calculates a pose's pointer within the contiguous bank-$A8 map table.</summary>
    /// <param name="pose">Zero-based index in the five-map order returned by <see cref="Frames"/>.</param>
    /// <returns>The bank-relative address of that pose's OAM map.</returns>
    private static ushort MapPointer(int pose) => (ushort)(0xe92c + 22 * pose);
}
