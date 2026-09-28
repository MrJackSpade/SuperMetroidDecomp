namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$AA Golden Torizo combat lists that remain cartridge-backed
/// until their control, visual, and physical data have independent owners.
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
}
