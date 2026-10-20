using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// Converts between a save's loading-game-state word (<c>SRAMMirror_LoadingGameState</c>, $7E:D914) and the startup route
/// $82:EEB4 selects from it after file selection.
/// </summary>
public static class SaveLoadingGameStateCodec
{
    /// <summary>The saved dispatcher word that selects <paramref name="route"/>.</summary>
    /// <remarks>
    /// $0000 selects the opening cinematic and a new Ceres start. $0005 resumes through the file-select area map and saved
    /// checkpoint; gunship landing publishes it at $A2:A9A0-$A9A3 before saving. $001F resumes at the initial Ceres-elevator
    /// arrival; FlyToCeres_StartGameAtCeres publishes it at $8B:C100-$C103 and $82:8026 selects the Ceres loader. $0022
    /// resumes the Ceres-destruction cinematic, not the activated Ceres station; escape blackout publishes it at
    /// $82:83D6-$83D9, $82:EECC-$EED4 selects CeresGoesBoom_Initial and $82:802B later selects Zebes station eighteen.
    /// </remarks>
    public static int Word(SaveLoadingGameState route) => route switch
    {
        SaveLoadingGameState.OpeningCinematic => (ushort)SuperMetroidGameState.Reset,
        SaveLoadingGameState.MainGame => (ushort)SuperMetroidGameState.FileSelectMap,
        SaveLoadingGameState.CeresElevatorArrival => (ushort)SuperMetroidGameState.SetUpNewGame,
        SaveLoadingGameState.CeresDestruction => (ushort)SuperMetroidGameState.CeresGoesBoom,
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Undefined startup route."),
    };

    /// <summary>The startup route a saved word selects; any word the dispatcher does not name resumes the main game.</summary>
    public static SaveLoadingGameState Route(int word) =>
        Enum.GetValues<SaveLoadingGameState>().FirstOrDefault(route => Word(route) == word, SaveLoadingGameState.MainGame);
}
