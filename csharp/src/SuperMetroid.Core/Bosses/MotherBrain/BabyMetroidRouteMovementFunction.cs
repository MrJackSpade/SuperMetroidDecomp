namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The movement routines the Baby Metroid route records name.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1413).
/// </summary>
internal enum BabyMetroidRouteMovementFunction : ushort
{
    /// <summary><c>$A9:F45F</c>, gradual acceleration with wrong-way extra <c>$0008</c>.</summary>
    GradualAccelerationExtraEight = 1,

    /// <summary><c>$A9:F466</c>, gradual acceleration with wrong-way extra <c>$0010</c>.</summary>
    GradualAccelerationExtraSixteen,
}
