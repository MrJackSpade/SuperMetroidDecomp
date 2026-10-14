namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The retail dispatcher word at WRAM <c>$0998</c>, named from bank <c>$82</c>'s
/// 45-entry <c>kGameStateFuncs</c> table.
/// </summary>
/// <remarks>
/// These are sequential states, not combinable bits. Keeping every native numeric value
/// makes debugger watches and future save-state comparison line up with the original game.
/// </remarks>
public enum SuperMetroidGameState : ushort
{
    /// <summary>Initializes the title-sequence dispatcher and demo set; native <c>GameState_0_ResetStart</c> at <c>$82:8AE4</c>.</summary>
    Reset = 0x00,
    /// <summary>Runs the title opening and title screen, including the idle transition into attract demos; native <c>GameState_1_TitleSequence</c> at <c>$82:8B08</c>.</summary>
    OpeningCinematic = 0x01,
    /// <summary>Runs the selected save's options menus before new-game or map selection; native <c>GameState_2_GameOptionsMenu</c> at <c>$82:EB9F</c>.</summary>
    GameOptionsMenu = 0x02,
    /// <summary>Preserves the unused no-operation dispatcher entry <c>GameState_3_Nothing</c> at <c>$82:8B0D</c>.</summary>
    Unused03 = 0x03,
    /// <summary>Runs save-slot selection, copying, and deletion menus; native <c>GameState_4_FileSelectMenus</c> at <c>$82:89E5</c>.</summary>
    FileSelectMenus = 0x04,
    /// <summary>Runs area and saved-location map selection before loading an existing save; native <c>GameState_5_FileSelectMap</c> at <c>$82:89EA</c>.</summary>
    FileSelectMap = 0x05,
    /// <summary>
    /// Loads a saved starting room or the post-Ceres Zebes landing, including graphics and enemy initialization waits;
    /// uses <c>GameState_6_1F_28_LoadingGameData_SetupNewGame_LoadDemoData</c> at <c>$82:8000</c>.
    /// </summary>
    LoadingGameData = 0x06,
    /// <summary>Runs gameplay while the initial room brightens, then enters state eight; native <c>GameState_7_MainGameplayFadingIn</c> at <c>$82:8B20</c>.</summary>
    MainGameplayFadeIn = 0x07,
    /// <summary>Runs the ordinary ordered Samus, enemy, projectile, PLM, and room-update loop; native <c>GameState_8_MainGameplay</c> at <c>$82:8B44</c>.</summary>
    MainGameplay = 0x08,
    /// <summary>
    /// Runs the door-entry function, including any downward-elevator delay, before calling state ten in the same dispatch;
    /// native <c>GameState_9_HitADoorBlock</c> at <c>$82:E169</c>.
    /// </summary>
    HitDoorBlock = 0x09,
    /// <summary>
    /// Initializes the door transition, pauses source-room enemies, and cancels movement sounds before selecting state eleven;
    /// native <c>GameState_A_LoadingNextRoom</c> at <c>$82:E1B7</c>, not a separately displayed ordinary-door frame.
    /// </summary>
    LoadingNextRoomA = 0x0a,
    /// <summary>Runs the multi-update room-loading, scrolling, and fade transition until gameplay resumes; native <c>GameState_B_LoadingNextRoom</c> at <c>$82:E288</c>.</summary>
    LoadingNextRoomB = 0x0b,
    /// <summary>Continues ordinary gameplay while darkening for pause; native <c>GameState_C_Pausing_NormalGameplayDarkening</c> at <c>$82:8CCF</c>.</summary>
    PausingDarkening = 0x0c,
    /// <summary>Installs pause graphics and hooks under forced blank, backing up gameplay graphics and cancelling effects; native <c>GameState_D_Pausing_LoadingPauseScreen</c> at <c>$82:8CEF</c>.</summary>
    Pausing = 0x0d,
    /// <summary>Draws and brightens the prepared pause screen without resuming gameplay; native <c>GameState_E_Paused_LoadingPauseScreen</c> at <c>$82:90C8</c>.</summary>
    PausedA = 0x0e,
    /// <summary>Accepts interactive pause-map and equipment input while gameplay remains paused; native <c>GameState_F_Paused_MapAndItemScreens</c> at <c>$82:90E8</c>.</summary>
    PausedB = 0x0f,
    /// <summary>Draws the pause screen during its fade to forced blank after Start requests unpause; native <c>GameState_10_Unpausing_LoadingNormalGameplay</c> at <c>$82:9324</c>.</summary>
    UnpausingA = 0x10,
    /// <summary>Restores gameplay graphics, reconciles equipped items, and runs the unpause hook under forced blank; native <c>GameState_11_Unpausing_LoadingNormalGameplay</c> at <c>$82:9367</c>.</summary>
    UnpausingB = 0x11,
    /// <summary>Resumes ordinary gameplay while brightening, then explicitly restores state eight; native <c>GameState_12_Unpausing_NormalGameplayBrightening</c> at <c>$82:93A1</c>.</summary>
    Unpausing = 0x12,
    /// <summary>Runs the final frozen gameplay pass and prepares Samus and palette state for death; native <c>GameState_13_DeathSequence_Start</c> at <c>$82:DC80</c>.</summary>
    DeathSequenceStart = 0x13,
    /// <summary>Fades the surroundings to black while preserving Samus's suit palette, then queues death music; native <c>GameState_14_DeathSequence_BlackOutSurroundings</c> at <c>$82:DCE0</c>.</summary>
    DeathBlackOutSurroundings = 0x14,
    /// <summary>Draws the fatal Samus pose until the queued death music has been processed; native <c>GameState_15_DeathSequence_WaitForMusic</c> at <c>$82:DD71</c>.</summary>
    DeathWaitForMusic = 0x15,
    /// <summary>Holds the starting death animation for its update-counted pre-flash timer; native <c>GameState_16_DeathSequence_PreFlashing</c> at <c>$82:DD87</c>.</summary>
    DeathPreFlashing = 0x16,
    /// <summary>Runs Samus's death-animation flashing before the suit explosion; native <c>GameState_17_DeathSequence_Flashing</c> at <c>$82:DD9A</c>.</summary>
    DeathFlashing = 0x17,
    /// <summary>Animates the exploding suit and palette whiteout before the final fade; native <c>GameState_18_DeathSequence_ExplosionWhiteOut</c> at <c>$82:DDAF</c>.</summary>
    DeathExplosionWhiteOut = 0x18,
    /// <summary>Fades the completed death display to black before the game-over menu; also the native Zebes time-up destination, <c>GameState_19_DeathSequence_BlackOut</c> at <c>$82:DDC7</c>.</summary>
    DeathFinalBlackOut = 0x19,
    /// <summary>Runs the Continue-or-title menu after death; native <c>GameState_1A_GameOverScreen</c> at <c>$82:89E0</c>.</summary>
    GameOverMenu = 0x1a,
    /// <summary>Transfers automatic reserve energy into Samus's health one point per update while frozen, then restores ordinary gameplay; native <c>GameState_1B_ReserveTankAuto</c> at <c>$82:DC10</c>.</summary>
    ReserveTanksAuto = 0x1b,
    /// <summary>Preserves the unreachable native entry <c>UNUSED_GameState_1C_828B3F</c> at <c>$82:8B3F</c>, whose bank-$91 handler treats the crystal-flash ammunition index as a function pointer.</summary>
    Unused1c = 0x1c,
    /// <summary>Preserves the debug-build-only game-over handler, a no-op in retail; native <c>GameState_1D_DebugGameOverMenu</c> at <c>$82:89DB</c>.</summary>
    DebugGameOverMenu = 0x1d,
    /// <summary>Runs the new-game narrated introduction before Ceres gameplay; uses the shared cinematic wrapper <c>GameState_1E_22_25_Intro_CeresGoesBoom_SamusGoesToZebes</c> at <c>$82:8B0E</c>.</summary>
    IntroCinematic = 0x1e,
    /// <summary>Initializes new-game Samus, Ceres starting-room, and gameplay graphics state through the shared state-$06/$1F/$28 loader at <c>$82:8000</c>.</summary>
    SetUpNewGame = 0x1f,
    /// <summary>Continues Ceres gameplay during the elevator departure delay before darkening; native <c>GameState_20_MadeItToCeresElevator</c> at <c>$82:8367</c>.</summary>
    MadeItToCeresElevator = 0x20,
    /// <summary>Continues gameplay while fading out from Ceres, then saves the departure checkpoint and starts its destruction cinematic; native <c>GameState_21_BlackoutFromCeres</c> at <c>$82:8388</c>.</summary>
    BlackoutFromCeres = 0x21,
    /// <summary>Runs successful Ceres destruction and Samus's flight to Zebes before state six loads the landing; uses the shared state-$1E/$22/$25 cinematic wrapper at <c>$82:8B0E</c>.</summary>
    CeresGoesBoom = 0x22,
    /// <summary>Continues gameplay while whitening the palette after either escape timer expires; native <c>GameState_23_TimeUpWhiteOut</c> at <c>$82:8411</c>.</summary>
    TimeUp = 0x23,
    /// <summary>
    /// Despite the legacy name, fades the already whitened time-up display to black, then selects Ceres destruction with Samus
    /// or the Zebes death fade; native <c>GameState_23_TimeUpBlackOut</c> at <c>$82:8431</c> is dispatcher state $24.
    /// </summary>
    WhitingOutFromTimeUp = 0x24,
    /// <summary>Runs the fatal Ceres destruction cinematic when Samus has not escaped; uses the shared state-$1E/$22/$25 cinematic wrapper at <c>$82:8B0E</c>.</summary>
    CeresGoesBoomWithSamus = 0x25,
    /// <summary>Continues the departing gunship's gameplay while darkening after Zebes escape, then hands off to ending graphics; native <c>GameState_26_SamusEscapesFromZebes</c> at <c>$82:84BD</c>.</summary>
    SamusEscapesFromZebes = 0x26,
    /// <summary>Runs Zebes destruction, ending results, staff credits, and the completion reward sequence; native <c>GameState_27_EndingAndCredits</c> at <c>$82:8B13</c>.</summary>
    EndingAndCredits = 0x27,
    /// <summary>Loads the next attract-demo room and recorded initial gameplay state through the shared state-$06/$1F/$28 loader at <c>$82:8000</c>.</summary>
    TransitionToDemoA = 0x28,
    /// <summary>Runs the demo's first gameplay pass and NMI setup, then makes the display fully bright and enters playback; native <c>GameState_29_TransitionToDemo</c> at <c>$82:852D</c>.</summary>
    TransitionToDemoB = 0x29,
    /// <summary>Runs recorded attract-demo gameplay until its timer or newly pressed real input ends playback; native <c>GameState_2A_PlayingDemo</c> at <c>$82:8548</c>.</summary>
    PlayingDemo = 0x2a,
    /// <summary>Unloads demo gameplay and graphics state while determining whether another scene should play; native <c>GameState_2B_UnloadGameData</c> at <c>$82:8593</c>.</summary>
    TransitionFromDemoA = 0x2b,
    /// <summary>Routes completed demos to another demo load, a fresh title sequence, or the input-skipped title screen; native <c>GameState_2C_TransitionFromDemo</c> at <c>$82:85FB</c>.</summary>
    TransitionFromDemoB = 0x2c,
}
