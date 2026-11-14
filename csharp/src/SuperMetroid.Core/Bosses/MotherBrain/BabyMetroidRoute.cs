namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid's route from the ceiling to Samus: each leg's waypoint and steering.</summary>
internal static class BabyMetroidRoute
{
    /// <summary>Combines an authored waypoint with its movement step and whether it is the final leg.</summary>
    private static BabyMetroidRouteLeg BuildLeg(int index) => new(
        BabyMetroidRouteDefinitions.Waypoints[index],
        BabyMetroidRouteDefinitions.AccelerationDivisor,
        index < BabyMetroidRouteDefinitions.FirstExtraEightLeg
            ? BabyMetroidRouteMovement.GradualAccelerationExtraSixteen
            : BabyMetroidRouteMovement.GradualAccelerationExtraEight,
        EndsWithLatchOntoSamus: index + 1 == BabyMetroidRouteDefinitions.LegCount);

    /// <summary>Returns the authored route leg <paramref name="leg"/>, counted from zero.</summary>
    internal static BabyMetroidRouteLeg GetLeg(int leg) =>
        (uint)leg < BabyMetroidRouteDefinitions.LegCount
            ? BuildLeg(leg)
            : throw new InvalidDataException($"Baby route leg {leg} is not one of the {BabyMetroidRouteDefinitions.LegCount} authored legs.");

    /// <summary>Resolves a movement step to its wrong-way horizontal speed addition.</summary>
    internal static int GetWrongWayOffScreenXSpeed(BabyMetroidRouteMovement movement) =>
        BabyMetroidRouteDefinitions.WrongWayOffScreenXSpeeds.TryGetValue(movement, out int speed)
            ? speed
            : throw new InvalidDataException($"Baby route names unknown movement {movement}.");
}
