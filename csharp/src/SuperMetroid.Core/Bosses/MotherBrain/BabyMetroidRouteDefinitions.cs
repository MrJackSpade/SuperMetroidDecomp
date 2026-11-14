namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Compiled gameplay route and movement-step identities used by the Baby Metroid after it
/// releases Mother Brain. These values control physical targets and acceleration; they
/// are engine definitions rather than editable sprite or animation data.
/// </summary>
internal static class BabyMetroidRouteDefinitions
{
    /// <summary>Number of authored route legs.</summary>
    internal const int LegCount = 8;

    /// <summary>
    /// The eight authored waypoints. Each leg steers to its waypoint with the acceleration divisor
    /// <see cref="AccelerationDivisor"/>; the first five legs use the wrong-way extra-16 step and the final
    /// three the extra-8 step. Reaching the last waypoint ends the route.
    /// </summary>
    internal static readonly Position16<RoomPixels>[] Waypoints =
    [
        new(160, 120), new(304, 122), new(192, 64), new(192, 112),
        new(224, 128), new(205, 144), new(204, 160), new(203, 176),
    ];
    /// <summary>The gradual-acceleration divisor every route leg uses.</summary>
    internal const int AccelerationDivisor = 16;

    /// <summary>First route leg that switches from the extra-16 to extra-8 wrong-way correction.</summary>
    internal const int FirstExtraEightLeg = 5;

    /// <summary>The wrong-way horizontal speed addition of each movement step.</summary>
    internal static readonly Dictionary<BabyMetroidRouteMovement, int> WrongWayOffScreenXSpeeds = new()
    {
        [BabyMetroidRouteMovement.GradualAccelerationExtraEight] = 8,
        [BabyMetroidRouteMovement.GradualAccelerationExtraSixteen] = 16,
    };
}
