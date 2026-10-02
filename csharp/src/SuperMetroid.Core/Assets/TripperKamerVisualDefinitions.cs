using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Eighteen bank-$A3 OAM compositions used by the shared Tripper/Kamer platform
/// dispatcher. The native programs and movement state own direction and timing;
/// the two frozen Tripper compositions are selected directly by its shot AI.
/// </summary>
internal static class TripperKamerVisualDefinitions
{
    /// <summary>Native Tripper/Kamer spritemap bank $A3.</summary>
    internal const byte Bank = 0xa3;

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, 0x9f29, "tripper_moving_left_0"),
        new(Bank, 0x9f3a, "tripper_moving_left_1"),
        new(Bank, 0x9f4b, "tripper_moving_left_2"),
        new(Bank, 0x9f5c, "tripper_moving_right_0"),
        new(Bank, 0x9f6d, "tripper_moving_right_1"),
        new(Bank, 0x9f7e, "tripper_moving_right_2"),
        new(Bank, 0x9f8f, "tripper_still_moving_left_0"),
        new(Bank, 0x9fa5, "tripper_still_moving_left_1"),
        new(Bank, 0x9fb6, "tripper_still_moving_left_2"),
        new(Bank, 0x9fcc, "tripper_still_moving_right_0"),
        new(Bank, 0x9fe2, "tripper_still_moving_right_1"),
        new(Bank, 0x9ff3, "tripper_still_moving_right_2"),
        new(Bank, RoomEnemySystem.TripperFrozenMovingLeftSpritemap,
            "tripper_frozen_moving_left"),
        new(Bank, RoomEnemySystem.TripperFrozenMovingRightSpritemap,
            "tripper_frozen_moving_right"),
        new(Bank, 0xa021, "tripper_kamer_platform_0"),
        new(Bank, 0xa02d, "tripper_kamer_platform_1"),
        new(Bank, 0xa039, "tripper_kamer_platform_2"),
        new(Bank, 0xa045, "tripper_kamer_platform_3"),
    ];

    /// <summary>Resolves the four forward Kamer frames or Tripper's alternating
    /// neutral/first/neutral/second poses from native sprite-record sizes.</summary>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (!PlatformInstructionProgramDefinitions.IsPresentationWord(operandAddress))
            throw new InvalidDataException(
                $"Tripper/Kamer visual operand $A3:{operandAddress:X4} is not compiled.");
        int offset = operandAddress - PlatformInstructionProgramDefinitions.KamerMovingLeft;
        int program = offset / 22;
        int frame = (offset % 22 - 4) / 4;
        // Kamer maps have two OAM entries: 2 + 2*5 bytes each.
        if (program < 4) return (ushort)(0xa021 + 12 * frame);
        int pose = (frame & 1) == 0 ? 0 : (frame + 1) / 2;
        int facing = program & 1;
        // Moving Tripper maps each have three entries, 17 bytes per map.
        if (program < 6) return (ushort)(0x9f29 + 51 * facing + 17 * pose);
        // Still maps have four, three, four entries: 22/17/22 bytes.
        return (ushort)(0x9f8f + 61 * facing + 22 * pose - (pose == 2 ? 5 : 0));
    }
}
