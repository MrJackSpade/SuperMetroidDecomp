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

    /// <summary><c>Spritemap_Tripper_Frozen_MovingLeft</c> at $A3:A009.</summary>
    internal const ushort FrozenMovingLeft = 0xa009;
    /// <summary><c>Spritemap_Tripper_Frozen_MovingRight</c> at $A3:A015.</summary>
    internal const ushort FrozenMovingRight = 0xa015;

    /// <summary>$A3:9F14-9F25 tests the complete X-movement word for zero.
    /// Every nonzero value chooses the right map, including values outside the normal dispatcher domain.</summary>
    internal static ushort FrozenFrame(PlatformHorizontalMovement direction) =>
        direction == PlatformHorizontalMovement.Left ? FrozenMovingLeft : FrozenMovingRight;
    internal static EnemySpritemapDefinition[] Frames() =>
    [
        new(Bank, MovingTripperFrame(0, 0), "tripper_moving_left_0"),
        new(Bank, MovingTripperFrame(0, 1), "tripper_moving_left_1"),
        new(Bank, MovingTripperFrame(0, 2), "tripper_moving_left_2"),
        new(Bank, MovingTripperFrame(1, 0), "tripper_moving_right_0"),
        new(Bank, MovingTripperFrame(1, 1), "tripper_moving_right_1"),
        new(Bank, MovingTripperFrame(1, 2), "tripper_moving_right_2"),
        new(Bank, StillTripperFrame(0, 0), "tripper_still_moving_left_0"),
        new(Bank, StillTripperFrame(0, 1), "tripper_still_moving_left_1"),
        new(Bank, StillTripperFrame(0, 2), "tripper_still_moving_left_2"),
        new(Bank, StillTripperFrame(1, 0), "tripper_still_moving_right_0"),
        new(Bank, StillTripperFrame(1, 1), "tripper_still_moving_right_1"),
        new(Bank, StillTripperFrame(1, 2), "tripper_still_moving_right_2"),
        new(Bank, FrozenMovingLeft,
            "tripper_frozen_moving_left"),
        new(Bank, FrozenMovingRight,
            "tripper_frozen_moving_right"),
        new(Bank, KamerFrame(0), "tripper_kamer_platform_0"),
        new(Bank, KamerFrame(1), "tripper_kamer_platform_1"),
        new(Bank, KamerFrame(2), "tripper_kamer_platform_2"),
        new(Bank, KamerFrame(3), "tripper_kamer_platform_3"),
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
        if (program < 4) return KamerFrame(frame);
        int pose = (frame & 1) == 0 ? 0 : (frame + 1) / 2;
        int facing = program & 1;
        // Moving Tripper maps each have three entries, 17 bytes per map.
        if (program < 6) return MovingTripperFrame(facing, pose);
        // Still maps have four, three, four entries: 22/17/22 bytes.
        return StillTripperFrame(facing, pose);
    }
    // These record-layout calculations serve runtime selection and export identities.
    private static ushort KamerFrame(int frame) => (ushort)(0xa021 + 12 * frame);
    private static ushort MovingTripperFrame(int facing, int pose) =>
        (ushort)(0x9f29 + 51 * facing + 17 * pose);
    private static ushort StillTripperFrame(int facing, int pose) =>
        (ushort)(0x9f8f + 61 * facing + 22 * pose - (pose == 2 ? 5 : 0));
}
