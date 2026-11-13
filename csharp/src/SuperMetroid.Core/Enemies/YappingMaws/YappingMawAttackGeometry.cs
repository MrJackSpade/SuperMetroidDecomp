namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The geometry of the Yapping Maw's curved attack: how far it reaches and where it holds Samus.</summary>
public static class YappingMawAttackGeometry
{
    /// <summary>
    /// The longest target length the neutral maw keeps: a shorter distance to Samus is used as it is, a longer one is
    /// capped here before the link radius is computed.
    /// </summary>
    public const int MaximumTargetDistance = 64;

    /// <summary>Where a maw aiming <paramref name="aim"/> holds Samus, from its mouth.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="aim"/> is not one of the eight aims.</exception>
    internal static Offset16<RoomPixels> HeldSamusOffset(YappingMawAim aim) => Ensure.Listed(YappingMawHoldDefinitions.HeldSamusOffsets, aim);
}
