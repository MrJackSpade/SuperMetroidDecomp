namespace SuperMetroid.Core.Progression;

/// <summary>
/// The phase the game is in: the title and menus, loading, gameplay, a door transition, pause, death, the cinematics and
/// the attract demos. The frontend runs exactly one phase per update.
/// </summary>
/// <remarks>
/// These are sequential states, not combinable bits. Their values are arbitrary: nothing reads, compares or orders them,
/// and every transition names the state it enters.
/// </remarks>
public enum SuperMetroidGameState
{
    /// <summary>Starts the title sequence and the attract-demo set at power-on or after a soft reset.</summary>
    Reset,
    /// <summary>Runs the title opening and title screen, including the idle transition into attract demos.</summary>
    OpeningCinematic,
    /// <summary>Runs the selected save's options menus before new-game or map selection.</summary>
    GameOptionsMenu,
    /// <summary>Runs save-slot selection, copying, and deletion menus.</summary>
    FileSelectMenus,
    /// <summary>Runs area and saved-location map selection before loading an existing save.</summary>
    FileSelectMap,
    /// <summary>Loads a saved starting room or the post-Ceres Zebes landing, including its graphics and enemy setup waits.</summary>
    LoadingGameData,
    /// <summary>Runs gameplay while the initial room brightens, then enters <see cref="MainGameplay"/>.</summary>
    MainGameplayFadeIn,
    /// <summary>Runs the ordinary ordered Samus, enemy, projectile, room-object and room-update loop.</summary>
    MainGameplay,
    /// <summary>Runs the door-entry step, including any downward-elevator delay, before <see cref="LoadingNextRoomA"/> in the same update.</summary>
    HitDoorBlock,
    /// <summary>
    /// Begins the door transition, pauses the source room's enemies, and cancels movement sounds before
    /// <see cref="LoadingNextRoomB"/>; not a separately displayed ordinary-door frame.
    /// </summary>
    LoadingNextRoomA,
    /// <summary>Runs the multi-update room-loading, scrolling, and fade transition until gameplay resumes.</summary>
    LoadingNextRoomB,
    /// <summary>Continues ordinary gameplay while darkening for pause.</summary>
    PausingDarkening,
    /// <summary>Prepares the pause screen on a blank display, backing up gameplay graphics and cancelling effects.</summary>
    Pausing,
    /// <summary>Draws and brightens the prepared pause screen without resuming gameplay.</summary>
    PausedA,
    /// <summary>Accepts interactive pause-map and equipment input while gameplay remains paused.</summary>
    PausedB,
    /// <summary>Draws the pause screen during its fade to a blank display after Start requests unpause.</summary>
    UnpausingA,
    /// <summary>Restores gameplay graphics, reconciles equipped items, and runs the unpause hook on a blank display.</summary>
    UnpausingB,
    /// <summary>Resumes ordinary gameplay while brightening, then restores <see cref="MainGameplay"/>.</summary>
    Unpausing,
    /// <summary>Runs the final frozen gameplay pass and prepares Samus and her colours for death.</summary>
    DeathSequenceStart,
    /// <summary>Fades the surroundings to black while preserving Samus's suit colours, then queues death music.</summary>
    DeathBlackOutSurroundings,
    /// <summary>Draws the fatal Samus pose until the queued death music has been processed.</summary>
    DeathWaitForMusic,
    /// <summary>Holds the starting death animation for its update-counted pre-flash timer.</summary>
    DeathPreFlashing,
    /// <summary>Runs Samus's death-animation flashing before the suit explosion.</summary>
    DeathFlashing,
    /// <summary>Animates the exploding suit and the white-out before the final fade.</summary>
    DeathExplosionWhiteOut,
    /// <summary>Fades the completed death display to black before the game-over menu; also where the Zebes time-up ends.</summary>
    DeathFinalBlackOut,
    /// <summary>Runs the Continue-or-title menu after death.</summary>
    GameOverMenu,
    /// <summary>Transfers automatic reserve energy into Samus's health one point per update while frozen, then restores ordinary gameplay.</summary>
    ReserveTanksAuto,
    /// <summary>Runs the new-game narrated introduction before Ceres gameplay.</summary>
    IntroCinematic,
    /// <summary>Sets up a new game's Samus, the Ceres starting room, and gameplay graphics.</summary>
    SetUpNewGame,
    /// <summary>Continues Ceres gameplay during the elevator departure delay before darkening.</summary>
    MadeItToCeresElevator,
    /// <summary>Continues gameplay while fading out from Ceres, then saves the departure checkpoint and starts its destruction cinematic.</summary>
    BlackoutFromCeres,
    /// <summary>Runs successful Ceres destruction and Samus's flight to Zebes before <see cref="LoadingGameData"/> loads the landing.</summary>
    CeresGoesBoom,
    /// <summary>Continues gameplay while whitening the colours after either escape timer expires.</summary>
    TimeUp,
    /// <summary>
    /// Despite the name, fades the already whitened time-up display to black, then selects Ceres destruction with Samus
    /// or the Zebes death fade.
    /// </summary>
    WhitingOutFromTimeUp,
    /// <summary>Runs the fatal Ceres destruction cinematic when Samus has not escaped.</summary>
    CeresGoesBoomWithSamus,
    /// <summary>Continues the departing gunship's gameplay while darkening after the Zebes escape, then hands off to the ending.</summary>
    SamusEscapesFromZebes,
    /// <summary>Runs Zebes destruction, ending results, staff credits, and the completion reward sequence.</summary>
    EndingAndCredits,
    /// <summary>Loads the next attract-demo room and its recorded initial gameplay state.</summary>
    TransitionToDemoA,
    /// <summary>Runs the demo's first gameplay pass, then makes the display fully bright and enters playback.</summary>
    TransitionToDemoB,
    /// <summary>Runs recorded attract-demo gameplay until its timer or a newly pressed real input ends playback.</summary>
    PlayingDemo,
    /// <summary>Unloads demo gameplay while determining whether another scene should play.</summary>
    TransitionFromDemoA,
    /// <summary>Routes completed demos to another demo load, a fresh title sequence, or the input-skipped title screen.</summary>
    TransitionFromDemoB,
}
