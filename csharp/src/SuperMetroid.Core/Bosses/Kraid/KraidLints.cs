namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Starts Kraid's post-growth lint attacks: the delay before each part first produces lint.</summary>
internal static class KraidLints
{
    /// <summary>The initial delay of lint part <paramref name="part"/>.</summary>
    internal static int InitialDelay(KraidLintPart part) =>
        KraidLintInitializationDefinitions.InitialDelays.TryGetValue(part, out int delay)
            ? delay
            : throw new InvalidOperationException($"Undefined KraidLintPart {part}.");
}
