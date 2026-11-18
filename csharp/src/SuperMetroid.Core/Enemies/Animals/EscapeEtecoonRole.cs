namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The mutually exclusive roles an escape Etecoon's population entry gives it. Its values are arbitrary; the import
/// decodes each entry's role.
/// </summary>
public enum EscapeEtecoonRole
{
    /// <summary>Starts at room pixel (128, 200), runs left at 2 pixels an update and then walks or falls.</summary>
    LeftWalker,
    /// <summary>Starts at room pixel (160, 200), runs right at 2.5 pixels an update and then walks or falls.</summary>
    RightWalker,
    /// <summary>Starts stationary at room pixel (232, 200) and waits for the critters-escaped event before starting its gratitude-then-escape animation.</summary>
    WaitForEscapeEvent,
}
