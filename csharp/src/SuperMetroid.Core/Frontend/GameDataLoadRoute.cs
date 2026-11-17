namespace SuperMetroid.Core.Frontend;

/// <summary>
/// What the game load (<see cref="Progression.SuperMetroidGameState.LoadingGame"/>) loads: the game chosen from the menus,
/// a save continued from the file-select area map, or Samus's landing on Zebes once Ceres is destroyed.
/// </summary>
internal enum GameDataLoadRoute
{
    /// <summary>
    /// The game the menus chose: a new game's Ceres start, a save's own starting route, or the Ceres arrival a game over
    /// continues.
    /// </summary>
    SelectedGame,
    /// <summary>A save continued from the file-select area map: the game loads the save's checkpoint.</summary>
    SavedCheckpoint,
    /// <summary>The Ceres destruction cinematic has finished: the game loads the Zebes landing.</summary>
    ZebesLanding,
}
