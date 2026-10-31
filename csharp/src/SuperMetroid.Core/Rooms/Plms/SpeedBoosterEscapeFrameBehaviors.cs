namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Speed Booster escape lavaquake's waits and lava advance.</summary>
internal static class SpeedBoosterEscapeFrameBehaviors
{
    /// <summary>Each routine's behavior, by routine.</summary>
    private static readonly Dictionary<SpeedBoosterEscapeFrameBehavior, PlmFrameBehavior> Routines =
        Enum.GetValues<SpeedBoosterEscapeFrameBehavior>()
            .Where(routine => routine != SpeedBoosterEscapeFrameBehavior.None)
            .ToDictionary(routine => routine, routine => new PlmFrameBehavior(
                $"Speed Booster escape {routine}",
                (slot, context) => context.Plms.SpeedBoosterEscape.RunSpeedBoosterEscapeFrameBehavior(slot, routine)));

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IPlmFrameBehavior Of(SpeedBoosterEscapeFrameBehavior routine) =>
        routine == SpeedBoosterEscapeFrameBehavior.None ? PlmFrameBehaviors.Inert
            : Routines.TryGetValue(routine, out PlmFrameBehavior? behavior) ? behavior
            : throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterEscapeFrameBehavior)} {routine}.");
}
