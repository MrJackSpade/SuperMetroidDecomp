namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's figure-eight path, one authored unit step at a time.</summary>
public static class PhantoonPath
{
    /// <summary>The path's places in order, each the signed unit step he takes from it to the next place.</summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> Places { get; } = [.. PhantoonPathDefinitions.Directions.Select(StepOf)];

    /// <summary>The place <paramref name="place"/> of his path, counting from its start.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The path has no such place.</exception>
    public static ListStep<Offset16<RoomPixels>> At(int place) => ListStep<Offset16<RoomPixels>>.At(Places, place);

    /// <summary>The signed unit step the keypad <paramref name="direction"/> stands for.</summary>
    /// <exception cref="InvalidOperationException">The path names a direction no step stands for.</exception>
    private static Offset16<RoomPixels> StepOf(char direction) =>
        PhantoonPathDefinitions.Steps.TryGetValue(direction, out Offset16<RoomPixels> step)
            ? step
            : throw new InvalidOperationException($"Phantoon's path names the direction '{direction}', which no step stands for.");
}
