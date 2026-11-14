namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The mutually exclusive roles an escape Etecoon's population entry gives it. Its values are arbitrary; the import
/// decodes each entry's role.
/// </summary>
public enum EscapeEtecoonRole
{
    /// <summary>Starts at room pixel (128, 200), runs left and then walks or falls, with signed 1/256-pixel X velocity -2 pixels per update.</summary>
    LeftWalker,
    /// <summary>Starts at room pixel (160, 200), runs right and then walks or falls, with signed 1/256-pixel X velocity +2.5 pixels per update.</summary>
    RightWalker,
    /// <summary>Starts stationary at room pixel (232, 200) and waits for the critters-escaped event before starting its gratitude-then-escape animation.</summary>
    WaitForEscapeEvent,
}
