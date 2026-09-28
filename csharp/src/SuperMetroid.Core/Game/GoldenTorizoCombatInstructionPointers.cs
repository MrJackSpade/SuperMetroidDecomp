namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$AA Golden Torizo combat-list entry points. Each address has
/// an independent compiled owner only after its control, visual, and physical
/// records have been migrated together.
/// </summary>
internal static class GoldenTorizoCombatInstructionPointers
{
    /// <summary>
    /// <c>InstList_GoldenTorizo_WalkingLeft_RightLegMoving</c> at $AA:D20D;
    /// the return target after one left-leg walking cycle.
    /// </summary>
    internal const ushort WalkingLeftRightLeg = 0xd20d;

    /// <summary>
    /// <c>InstList_GoldenTorizo_WalkingLeft_LeftLegMoving</c> at $AA:D259;
    /// the awakened statue's first ordinary combat destination.
    /// </summary>
    internal const ushort WalkingLeftLeftLeg = 0xd259;

    /// <summary>
    /// <c>InstList_GoldenTorizo_DodgeTurningRight</c> at $AA:D2AD.
    /// </summary>
    internal const ushort DodgeTurningRight = 0xd2ad;

    /// <summary>
    /// <c>InstList_GoldenTorizo_TurningRight</c> at $AA:D2BF.
    /// </summary>
    internal const ushort TurningRight = 0xd2bf;

    /// <summary>
    /// <c>InstList_GoldenTorizo_WalkingRight_LeftLegMoving</c> at $AA:D2C9.
    /// </summary>
    internal const ushort WalkingRightLeftLeg = 0xd2c9;

    /// <summary>
    /// <c>InstList_GoldenTorizo_WalkingRight_RightLegMoving</c> at $AA:D315.
    /// </summary>
    internal const ushort WalkingRightRightLeg = 0xd315;
}
