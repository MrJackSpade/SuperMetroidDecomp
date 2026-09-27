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

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0xe92c, "face_block_neutral"),
        new(Bank, 0xe942, "face_block_samus_left_1"),
        new(Bank, 0xe958, "face_block_samus_left_2"),
        new(Bank, 0xe96e, "face_block_samus_right_1"),
        new(Bank, 0xe984, "face_block_samus_right_2"),
    ];

    /// <summary>Only the seven visual operands in the three native programs.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (BlueBrinstarFaceBlockInstructionProgramDefinitions.IsPresentationWord(
                operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Face-block visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
