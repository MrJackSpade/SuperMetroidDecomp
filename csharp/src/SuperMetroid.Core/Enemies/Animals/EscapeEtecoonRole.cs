namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The mutually exclusive roles an escape Etecoon's population entry gives it. Its values are arbitrary; the import
/// decodes each entry's role.
/// </summary>
public enum EscapeEtecoonRole
{
    /// <summary>Starts at its place in <see cref="EscapeEtecoonDefinitions.Initializations"/>, runs left and then walks or falls.</summary>
    LeftWalker,
    /// <summary>Starts at its place in <see cref="EscapeEtecoonDefinitions.Initializations"/>, runs right and then walks or falls.</summary>
    RightWalker,
    /// <summary>Starts stationary at its place in <see cref="EscapeEtecoonDefinitions.Initializations"/> and waits for the critters-escaped event before starting its gratitude-then-escape animation.</summary>
    WaitForEscapeEvent,
}
