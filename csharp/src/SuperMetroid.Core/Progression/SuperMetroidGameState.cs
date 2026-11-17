namespace SuperMetroid.Core.Progression;

/// <summary>
/// The sequence the game is running: the title, file selection, loading, gameplay, a door transition, pause, the death
/// sequence and game-over menu, the cinematics and the attract demos. The frontend runs one update of exactly one sequence
/// per update; a sequence made of several steps owns which step it is on.
/// </summary>
/// <remarks>
/// These are sequential states, not combinable bits. Their values are arbitrary: nothing reads, compares or orders them,
/// and every transition names the sequence it enters and, for a sequence of several steps, the step.
/// </remarks>
public enum SuperMetroidGameState
{
    /// <summary>
    /// The title sequence: power-on or a soft reset starts it, then the title opening and title screen run, including the
    /// idle hand-off to the attract demos.
    /// </summary>
    Title,
    /// <summary>Choosing a game: the save slots with their copying and clearing, the selected game's options, and a saved game's area map.</summary>
    FileSelect,
    /// <summary>Loading the chosen game, a continued save's checkpoint or the landing on Zebes, then holding the black screen for the load's waits.</summary>
    LoadingGame,
    /// <summary>Gameplay running while the first room brightens, then <see cref="MainGameplay"/>.</summary>
    GameplayFadeIn,
    /// <summary>The ordinary ordered Samus, enemy, projectile, room-object and room update.</summary>
    MainGameplay,
    /// <summary>Going through a door: reaching it, including any downward elevator's delay, then loading, scrolling into and fading in the next room.</summary>
    DoorTransition,
    /// <summary>The pause: gameplay darkens, the pause screen is prepared, fades in, takes map and equipment input, fades out, and gameplay is restored and brightens.</summary>
    Pause,
    /// <summary>Automatic reserve energy flowing into Samus's health one point per update while gameplay is frozen, then <see cref="MainGameplay"/>.</summary>
    ReserveTankRefill,
    /// <summary>Samus's death: a final frozen gameplay pass, the surroundings blacking out, the death music, her death animation and the final black-out.</summary>
    DeathSequence,
    /// <summary>The Continue-or-title menu after death.</summary>
    GameOverMenu,
    /// <summary>The new game's narrated introduction before Ceres gameplay.</summary>
    IntroCinematic,
    /// <summary>Ceres gameplay continuing while Samus holds on the departing elevator, then while the screen fades to black.</summary>
    CeresDeparture,
    /// <summary>Ceres exploding: the gunship's flight to Zebes once Samus escaped, or the end of the game with her aboard.</summary>
    CeresDestruction,
    /// <summary>An escape timer has run out: gameplay continues while the colours whiten, then the whitened screen fades to black.</summary>
    TimeUp,
    /// <summary>The ending: the departing gunship's gameplay while the screen darkens after the Zebes escape, then Zebes's destruction, the results and the credits.</summary>
    Ending,
    /// <summary>An attract demo: loading its room, playing its recording, and returning to another demo or the title.</summary>
    AttractDemo,
}
