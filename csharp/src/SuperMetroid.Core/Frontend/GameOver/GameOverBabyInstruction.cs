namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>One step of the game-over Baby pose program, independent of cartridge memory.</summary>
/// <param name="Step">Ordinal of this step in play order, zero through <see cref="GameOverBabyAnimation.StepCount"/> minus one.</param>
/// <param name="Duration">Menu updates this pose is held, two through ten.</param>
/// <param name="Frame">Semantic pose selecting editable Baby OAM artwork.</param>
/// <param name="Palette">Semantic phase selecting an editable sixteen-color Baby palette.</param>
/// <param name="SoundAfter">Cry queued when this step's countdown expires, or None when no cry follows.</param>
/// <param name="RestartAfter">Whether this is the final step, after which the program restarts at step zero.</param>
public readonly record struct GameOverBabyInstruction(
    int Step,
    int Duration,
    GameOverBabyFrame Frame,
    GameOverBabyPalette Palette,
    GameOverBabySound SoundAfter,
    bool RestartAfter)
{
    /// <summary>The step that follows this one: the next ordinal, or step zero after the final step.</summary>
    public int NextStep => RestartAfter ? 0 : Step + 1;
}
