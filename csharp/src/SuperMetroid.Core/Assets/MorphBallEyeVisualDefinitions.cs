using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Distinct bank-$A8 OAM compositions for the Morph Ball surveillance eye and
/// its directional mount. Activation logic and animation cadence remain compiled.
/// </summary>
internal static class MorphBallEyeVisualDefinitions
{
    /// <summary>Native eye-body and mount spritemap bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Lists the eye's active, transition, closed, and directional mount OAM compositions.</summary>
    /// <returns>Named bank-$A8 spritemap definitions used by Morph Ball eye presentation.</returns>
    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0x9210, "morph_eye_active_0"),
        new(Bank, 0x9217, "morph_eye_active_1"),
        new(Bank, 0x921e, "morph_eye_active_2"),
        new(Bank, 0x9225, "morph_eye_active_3"),
        new(Bank, 0x922c, "morph_eye_active_4"),
        new(Bank, 0x9233, "morph_eye_active_5"),
        new(Bank, 0x923a, "morph_eye_active_6"),
        new(Bank, 0x9209, "morph_eye_active_7"),
        new(Bank, 0x9202, "morph_eye_active_8"),
        new(Bank, 0x91fb, "morph_eye_active_9"),
        new(Bank, 0x91f4, "morph_eye_active_10"),
        new(Bank, 0x91ed, "morph_eye_active_11"),
        new(Bank, 0x91e6, "morph_eye_active_12"),
        new(Bank, 0x91df, "morph_eye_active_13"),
        new(Bank, 0x9257, "morph_eye_right_transition"),
        new(Bank, 0x9241, "morph_eye_right_closed"),
        new(Bank, 0x9283, "morph_eye_left_transition"),
        new(Bank, 0x926d, "morph_eye_left_closed"),
        new(Bank, 0x9299, "morph_eye_mount_right"),
        new(Bank, 0x92a5, "morph_eye_mount_down"),
        new(Bank, 0x92b1, "morph_eye_mount_left"),
        new(Bank, 0x92bd, "morph_eye_mount_up"),
    ];

    /// <summary>Only the 36 body and mount visual operands in native eye programs.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (MorphBallEyeInstructionProgramDefinitions.IsPresentationWord(operandAddress) &&
            CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress, out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Morph Ball eye visual operand $A8:{operandAddress:X4} is not compiled.");
    }
}
