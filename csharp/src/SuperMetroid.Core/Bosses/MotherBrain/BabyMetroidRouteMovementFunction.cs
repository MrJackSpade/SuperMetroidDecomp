namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Bank-<c>$A9</c> movement callbacks named by the Baby Metroid route records.</summary>
internal enum BabyMetroidRouteMovementFunction : ushort
{
    /// <summary><c>$A9:F45F</c>, gradual acceleration with wrong-way extra <c>$0008</c>.</summary>
    GradualAccelerationExtraEight = 0xf45f,

    /// <summary><c>$A9:F466</c>, gradual acceleration with wrong-way extra <c>$0010</c>.</summary>
    GradualAccelerationExtraSixteen = 0xf466,
}
