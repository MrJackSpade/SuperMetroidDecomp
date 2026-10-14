namespace SuperMetroid.Core.Game;

/// <summary>
/// The startup routes $82:EEB4 selects from a saved dispatcher word, valued by the word that
/// names each. Any word the dispatcher does not name resumes the main game.
/// </summary>
public enum SaveLoadingGameState : ushort
{
    /// <summary>The opening cinematic and a new Ceres start.</summary>
    OpeningCinematic = SaveLoadingGameStates.OpeningCinematic,
    /// <summary>The file-select area map and saved checkpoint.</summary>
    MainGame = SaveLoadingGameStates.MainGame,
    /// <summary>The initial Ceres-elevator arrival.</summary>
    CeresElevatorArrival = SaveLoadingGameStates.CeresElevatorArrival,
    /// <summary>The Ceres-destruction cinematic.</summary>
    CeresDestruction = SaveLoadingGameStates.CeresDestruction,
}
