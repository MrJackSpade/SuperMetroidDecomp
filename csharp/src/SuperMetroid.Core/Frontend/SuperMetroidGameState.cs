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
    Reset = 0x00,
    OpeningCinematic = 0x01,
    GameOptionsMenu = 0x02,
    Unused03 = 0x03,
    FileSelectMenus = 0x04,
    FileSelectMap = 0x05,
    LoadingGameData = 0x06,
    MainGameplayFadeIn = 0x07,
    MainGameplay = 0x08,
    HitDoorBlock = 0x09,
    LoadingNextRoomA = 0x0a,
    LoadingNextRoomB = 0x0b,
    PausingDarkening = 0x0c,
    Pausing = 0x0d,
    PausedA = 0x0e,
    PausedB = 0x0f,
    UnpausingA = 0x10,
    UnpausingB = 0x11,
    Unpausing = 0x12,
    DeathSequenceStart = 0x13,
    DeathBlackOutSurroundings = 0x14,
    DeathWaitForMusic = 0x15,
    DeathPreFlashing = 0x16,
    DeathFlashing = 0x17,
    DeathExplosionWhiteOut = 0x18,
    DeathFinalBlackOut = 0x19,
    GameOverMenu = 0x1a,
    ReserveTanksAuto = 0x1b,
    Unused1c = 0x1c,
    DebugGameOverMenu = 0x1d,
    IntroCinematic = 0x1e,
    SetUpNewGame = 0x1f,
    MadeItToCeresElevator = 0x20,
    BlackoutFromCeres = 0x21,
    CeresGoesBoom = 0x22,
    TimeUp = 0x23,
    WhitingOutFromTimeUp = 0x24,
    CeresGoesBoomWithSamus = 0x25,
    SamusEscapesFromZebes = 0x26,
    EndingAndCredits = 0x27,
    TransitionToDemoA = 0x28,
    TransitionToDemoB = 0x29,
    PlayingDemo = 0x2a,
    TransitionFromDemoA = 0x2b,
    TransitionFromDemoB = 0x2c,
}
