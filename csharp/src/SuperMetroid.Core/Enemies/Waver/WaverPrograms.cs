namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Chooses a Waver's animation by its facing and spinning.</summary>
internal static class WaverPrograms
{
    /// <summary>The animation <paramref name="selector"/> selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> is outside the four-entry table.</exception>
    internal static WaverProgram Program(WaverAnimationSelector selector) =>
        WaverAnimationDefinitions.Programs.TryGetValue(selector, out WaverProgram program)
            ? program
            : throw new InvalidDataException(
                $"Waver animation selector {selector} exceeds its four-entry table.");
}
