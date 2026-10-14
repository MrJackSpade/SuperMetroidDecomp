namespace SuperMetroid.Core.Frontend;

/// <summary>Domain-owned transitions over <see cref="SuperMetroidGameState"/>.</summary>
public static class SuperMetroidGameStates
{
    /// <summary>
    /// The dispatcher state one above <paramref name="state"/>, as the native <c>INC $0998</c>
    /// advances it. <see cref="SuperMetroidGameState.TransitionFromDemoB"/> is the last table entry and has no successor.
    /// </summary>
    public static SuperMetroidGameState Next(this SuperMetroidGameState state) => state switch
    {
        SuperMetroidGameState.Reset => SuperMetroidGameState.OpeningCinematic,
        SuperMetroidGameState.OpeningCinematic => SuperMetroidGameState.GameOptionsMenu,
        SuperMetroidGameState.GameOptionsMenu => SuperMetroidGameState.Unused03,
        SuperMetroidGameState.Unused03 => SuperMetroidGameState.FileSelectMenus,
        SuperMetroidGameState.FileSelectMenus => SuperMetroidGameState.FileSelectMap,
        SuperMetroidGameState.FileSelectMap => SuperMetroidGameState.LoadingGameData,
        SuperMetroidGameState.LoadingGameData => SuperMetroidGameState.MainGameplayFadeIn,
        SuperMetroidGameState.MainGameplayFadeIn => SuperMetroidGameState.MainGameplay,
        SuperMetroidGameState.MainGameplay => SuperMetroidGameState.HitDoorBlock,
        SuperMetroidGameState.HitDoorBlock => SuperMetroidGameState.LoadingNextRoomA,
        SuperMetroidGameState.LoadingNextRoomA => SuperMetroidGameState.LoadingNextRoomB,
        SuperMetroidGameState.LoadingNextRoomB => SuperMetroidGameState.PausingDarkening,
        SuperMetroidGameState.PausingDarkening => SuperMetroidGameState.Pausing,
        SuperMetroidGameState.Pausing => SuperMetroidGameState.PausedA,
        SuperMetroidGameState.PausedA => SuperMetroidGameState.PausedB,
        SuperMetroidGameState.PausedB => SuperMetroidGameState.UnpausingA,
        SuperMetroidGameState.UnpausingA => SuperMetroidGameState.UnpausingB,
        SuperMetroidGameState.UnpausingB => SuperMetroidGameState.Unpausing,
        SuperMetroidGameState.Unpausing => SuperMetroidGameState.DeathSequenceStart,
        SuperMetroidGameState.DeathSequenceStart => SuperMetroidGameState.DeathBlackOutSurroundings,
        SuperMetroidGameState.DeathBlackOutSurroundings => SuperMetroidGameState.DeathWaitForMusic,
        SuperMetroidGameState.DeathWaitForMusic => SuperMetroidGameState.DeathPreFlashing,
        SuperMetroidGameState.DeathPreFlashing => SuperMetroidGameState.DeathFlashing,
        SuperMetroidGameState.DeathFlashing => SuperMetroidGameState.DeathExplosionWhiteOut,
        SuperMetroidGameState.DeathExplosionWhiteOut => SuperMetroidGameState.DeathFinalBlackOut,
        SuperMetroidGameState.DeathFinalBlackOut => SuperMetroidGameState.GameOverMenu,
        SuperMetroidGameState.GameOverMenu => SuperMetroidGameState.ReserveTanksAuto,
        SuperMetroidGameState.ReserveTanksAuto => SuperMetroidGameState.Unused1c,
        SuperMetroidGameState.Unused1c => SuperMetroidGameState.DebugGameOverMenu,
        SuperMetroidGameState.DebugGameOverMenu => SuperMetroidGameState.IntroCinematic,
        SuperMetroidGameState.IntroCinematic => SuperMetroidGameState.SetUpNewGame,
        SuperMetroidGameState.SetUpNewGame => SuperMetroidGameState.MadeItToCeresElevator,
        SuperMetroidGameState.MadeItToCeresElevator => SuperMetroidGameState.BlackoutFromCeres,
        SuperMetroidGameState.BlackoutFromCeres => SuperMetroidGameState.CeresGoesBoom,
        SuperMetroidGameState.CeresGoesBoom => SuperMetroidGameState.TimeUp,
        SuperMetroidGameState.TimeUp => SuperMetroidGameState.WhitingOutFromTimeUp,
        SuperMetroidGameState.WhitingOutFromTimeUp => SuperMetroidGameState.CeresGoesBoomWithSamus,
        SuperMetroidGameState.CeresGoesBoomWithSamus => SuperMetroidGameState.SamusEscapesFromZebes,
        SuperMetroidGameState.SamusEscapesFromZebes => SuperMetroidGameState.EndingAndCredits,
        SuperMetroidGameState.EndingAndCredits => SuperMetroidGameState.TransitionToDemoA,
        SuperMetroidGameState.TransitionToDemoA => SuperMetroidGameState.TransitionToDemoB,
        SuperMetroidGameState.TransitionToDemoB => SuperMetroidGameState.PlayingDemo,
        SuperMetroidGameState.PlayingDemo => SuperMetroidGameState.TransitionFromDemoA,
        SuperMetroidGameState.TransitionFromDemoA => SuperMetroidGameState.TransitionFromDemoB,
        _ => throw new InvalidOperationException($"{nameof(SuperMetroidGameState)} {(int)state} has no successor."),
    };
}
