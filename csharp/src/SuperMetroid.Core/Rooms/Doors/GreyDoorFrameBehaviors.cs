using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The grey door caps' shot wakes, dud sound and unlock checks.</summary>
internal static class GreyDoorFrameBehaviors
{
    /// <summary>Each routine's behavior, by routine.</summary>
    private static readonly Dictionary<GreyDoorFrameBehavior, RoomObjectFrameBehavior> Routines =
        Enum.GetValues<GreyDoorFrameBehavior>()
            .Where(routine => routine != GreyDoorFrameBehavior.None)
            .ToDictionary(routine => routine, routine => new RoomObjectFrameBehavior(
                $"grey door {routine}", (slot, context) => context.RoomObjects.GreyDoors.RunGreyDoorFrameBehavior(
                    slot, routine, context.Frame.EnemyDeaths, context.Frame.EnemyDeathQuota)));

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IRoomObjectFrameBehavior Of(GreyDoorFrameBehavior routine) =>
        routine == GreyDoorFrameBehavior.None ? RoomObjectFrameBehaviors.Inert
            : Routines.TryGetValue(routine, out RoomObjectFrameBehavior? behavior) ? behavior
            : throw new InvalidOperationException($"Undefined {nameof(GreyDoorFrameBehavior)} {routine}.");
}
