namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Speed Booster escape lavaquake's waits and lava advance.</summary>
internal static class SpeedBoosterEscapePreInstructions
{
    /// <summary>Each routine's behavior, by routine.</summary>
    private static readonly Dictionary<SpeedBoosterEscapePreInstruction, PlmPreInstruction> Routines =
        Enum.GetValues<SpeedBoosterEscapePreInstruction>()
            .Where(routine => routine != SpeedBoosterEscapePreInstruction.None)
            .ToDictionary(routine => routine, routine => new PlmPreInstruction(
                $"Speed Booster escape {routine}", (ushort)routine,
                (slot, context) => context.Plms.SpeedBoosterEscape.RunSpeedBoosterEscapePreInstruction(slot, routine)));

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IPlmPreInstruction Of(SpeedBoosterEscapePreInstruction routine) =>
        routine == SpeedBoosterEscapePreInstruction.None ? PlmPreInstructions.Inert
            : Routines.TryGetValue(routine, out PlmPreInstruction? behavior) ? behavior
            : throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterEscapePreInstruction)} {routine}.");
}
