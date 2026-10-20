namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Eighteen bank-$A3 OAM compositions used by the shared Tripper/Kamer platform
/// dispatcher. The typed programs and movement state own direction and timing;
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
    /// <summary>Enumerates the moving, stationary, and frozen Tripper maps followed by
    /// the four Kamer platform poses for artwork extraction.</summary>
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

    /// <summary>Locates one Kamer pose in the contiguous two-entry OAM records.</summary>
    private static ushort KamerFrame(int frame) => (ushort)(0xa021 + 12 * frame);
    /// <summary>Locates a three-entry moving Tripper pose; each facing has three records.</summary>
    private static ushort MovingTripperFrame(int facing, int pose) =>
        (ushort)(0x9f29 + 51 * facing + 17 * pose);
    /// <summary>Locates a stationary Tripper pose within each facing's
    /// four-entry, three-entry, four-entry record sequence.</summary>
    private static ushort StillTripperFrame(int facing, int pose) =>
        (ushort)(0x9f8f + 61 * facing + 22 * pose - (pose == 2 ? 5 : 0));
}
