namespace SuperMetroid.Core.Frontend;

/// <summary>
/// What the game-data load (<see cref="Progression.SuperMetroidGameState.LoadingGameData"/>) loads: a save continued from
/// the file-select map, or Samus's landing on Zebes once Ceres is destroyed.
/// </summary>
internal enum GameDataLoadRoute
{
    /// <summary>A save continued from the file-select area map: the game loads the save's checkpoint.</summary>
    SavedCheckpoint,
    /// <summary>The Ceres destruction cinematic has finished: the game loads the Zebes landing.</summary>
    ZebesLanding,
}
