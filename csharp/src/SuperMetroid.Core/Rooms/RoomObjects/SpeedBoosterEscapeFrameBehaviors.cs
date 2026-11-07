namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster escape lavaquake's waits and lava advance.</summary>
internal static class SpeedBoosterEscapeFrameBehaviors
{
    /// <summary>Each routine's behavior, by routine.</summary>
    private static readonly Dictionary<SpeedBoosterEscapeFrameBehavior, RoomObjectFrameBehavior> Routines =
        Enum.GetValues<SpeedBoosterEscapeFrameBehavior>()
            .Where(routine => routine != SpeedBoosterEscapeFrameBehavior.None)
            .ToDictionary(routine => routine, routine => new RoomObjectFrameBehavior(
                $"Speed Booster escape {routine}",
                (slot, context) => context.RoomObjects.SpeedBoosterEscape.RunSpeedBoosterEscapeFrameBehavior(slot, routine)));

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IRoomObjectFrameBehavior Of(SpeedBoosterEscapeFrameBehavior routine) =>
        routine == SpeedBoosterEscapeFrameBehavior.None ? RoomObjectFrameBehaviors.Inert
            : Routines.TryGetValue(routine, out RoomObjectFrameBehavior? behavior) ? behavior
            : throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterEscapeFrameBehavior)} {routine}.");
}
