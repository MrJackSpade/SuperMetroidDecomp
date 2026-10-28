namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>Chooses a Brinstar Pipe Bug's program by its strength, facing and shooting.</summary>
internal static class PipeBugPrograms
{
    /// <summary>The rising or shooting program of a normal or strong Pipe Bug with <paramref name="selector"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> is outside the four-entry table.</exception>
    internal static PipeBugProgram BrinstarProgram(bool strong, PipeBugAnimationSelector selector) =>
        PipeBugDefinitions.BrinstarPrograms.TryGetValue(new PipeBugProgramKey(strong, selector), out PipeBugProgram program)
            ? program
            : throw new InvalidDataException(
                $"Pipe Bug animation selector {selector} exceeds its four-entry table.");
}
