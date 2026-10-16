using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The grey door caps' shot wakes, dud sound and unlock checks.</summary>
internal static class GreyDoorPreInstructions
{
    /// <summary>Each routine's behavior, by routine.</summary>
    private static readonly Dictionary<GreyDoorPreInstruction, PlmPreInstruction> Routines =
        Enum.GetValues<GreyDoorPreInstruction>()
            .Where(routine => routine != GreyDoorPreInstruction.None)
            .ToDictionary(routine => routine, routine => new PlmPreInstruction(
                $"grey door {routine}", (ushort)routine, (slot, context) => context.Plms.GreyDoors.RunGreyDoorPreInstruction(
                    slot, routine, context.Frame.EnemyDeaths, context.Frame.EnemyDeathQuota)));

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IPlmPreInstruction Of(GreyDoorPreInstruction routine) =>
        routine == GreyDoorPreInstruction.None ? PlmPreInstructions.Inert
            : Routines.TryGetValue(routine, out PlmPreInstruction? behavior) ? behavior
            : throw new InvalidOperationException($"Undefined {nameof(GreyDoorPreInstruction)} {routine}.");
}
