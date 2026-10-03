namespace SuperMetroid.Core.Rooms;

/// <summary>Native continuation operands for the Chozo Speed Booster pickup.</summary>
internal static class SpeedBoosterPickupDefinitions
{
    /// <summary>
    /// $84:E63B, Instruction_PLM_FXYVelocity_FFE0: signed 8.8 upward liquid velocity
    /// written after the Chozo Speed Booster acquisition message returns.
    /// </summary>
    internal const ushort LavaRiseVelocity = 0xffe0;
}
