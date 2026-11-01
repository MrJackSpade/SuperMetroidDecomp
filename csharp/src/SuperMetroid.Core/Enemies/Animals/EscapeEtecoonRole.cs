namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// Mutually exclusive authored roles selected by the escape Etecoon's even parameter-one
/// byte offset. Odd restored values select the same role after the game clears bit zero.
/// </summary>
public enum EscapeEtecoonRole
{
    /// <summary>Table byte selector 0: starts at room pixel (128, 200), runs left and then walks or falls, with signed 8.8 X velocity -2 pixels per update.</summary>
    LeftWalker = 0,
    /// <summary>Table byte selector 2: starts at room pixel (160, 200), runs right and then walks or falls, with signed 8.8 X velocity +2.5 pixels per update.</summary>
    RightWalker = 2,
    /// <summary>Table byte selector 4: starts stationary at room pixel (232, 200) and waits for the critters-escaped event before starting its gratitude-then-escape animation.</summary>
    WaitForEscapeEvent = 4,
}
