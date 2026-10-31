using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The grey door caps' shot wakes, dud sound and unlock checks.</summary>
internal static class GreyDoorFrameBehaviors
{
    /// <summary>Each routine's behavior, by routine.</summary>
    private static readonly Dictionary<GreyDoorFrameBehavior, PlmFrameBehavior> Routines =
        Enum.GetValues<GreyDoorFrameBehavior>()
            .Where(routine => routine != GreyDoorFrameBehavior.None)
            .ToDictionary(routine => routine, routine => new PlmFrameBehavior(
                $"grey door {routine}", (slot, context) => context.Plms.GreyDoors.RunGreyDoorFrameBehavior(
                    slot, routine, context.Frame.EnemyDeaths, context.Frame.EnemyDeathQuota)));

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IPlmFrameBehavior Of(GreyDoorFrameBehavior routine) =>
        routine == GreyDoorFrameBehavior.None ? PlmFrameBehaviors.Inert
            : Routines.TryGetValue(routine, out PlmFrameBehavior? behavior) ? behavior
            : throw new InvalidOperationException($"Undefined {nameof(GreyDoorFrameBehavior)} {routine}.");
}
