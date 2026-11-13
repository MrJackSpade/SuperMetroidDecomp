namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid's route from the ceiling to Samus: each leg's waypoint and steering.</summary>
internal static class BabyMetroidRoute
{
    /// <summary>Combines an authored waypoint with its movement step and whether it is the final leg.</summary>
    private static BabyMetroidRouteRecord Record(int index) => new(
        BabyMetroidRouteDefinitions.Waypoints[index],
        BabyMetroidRouteDefinitions.AccelerationDivisor,
        index < BabyMetroidRouteDefinitions.FirstExtraEightLeg
            ? BabyMetroidRouteMovementFunction.GradualAccelerationExtraSixteen
            : BabyMetroidRouteMovementFunction.GradualAccelerationExtraEight,
        EndsWithLatchOntoSamus: index + 1 == BabyMetroidRouteDefinitions.RecordCount);

    /// <summary>Returns the authored route leg <paramref name="leg"/>, counted from zero.</summary>
    internal static BabyMetroidRouteRecord GetRecord(int leg) =>
        (uint)leg < BabyMetroidRouteDefinitions.RecordCount
            ? Record(leg)
            : throw new InvalidDataException($"Baby route leg {leg} is not one of the {BabyMetroidRouteDefinitions.RecordCount} authored legs.");

    /// <summary>Resolves a movement step to its wrong-way horizontal speed addition.</summary>
    internal static int GetWrongWayOffScreenXSpeed(BabyMetroidRouteMovementFunction movementFunction) =>
        BabyMetroidRouteDefinitions.WrongWayOffScreenXSpeeds.TryGetValue(movementFunction, out int speed)
            ? speed
            : throw new InvalidDataException($"Baby route names unknown movement function {movementFunction}.");
}
