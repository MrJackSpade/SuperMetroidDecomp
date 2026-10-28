namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Chooses a Zoa's animation by its facing and movement flags.</summary>
internal static class ZoaPrograms
{
    /// <summary>The animation <paramref name="selector"/> selects; unsupported bits are rejected.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> is outside the four-entry table.</exception>
    internal static ZoaProgram Program(ZoaAnimationSelector selector) =>
        ZoaAnimationDefinitions.Programs.TryGetValue(selector, out ZoaProgram program)
            ? program
            : throw new InvalidDataException(
                $"Zoa animation selector {selector} exceeds its four-entry table.");
}
