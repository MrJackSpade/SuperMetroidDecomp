namespace SuperMetroid.Core.Progression;

/// <summary>
/// The startup routes the game selects from a saved loading-state word after file selection; <see cref="SaveLoadingGameStateCodec"/> converts
/// between them. Any word the dispatcher does not name resumes the main game.
/// </summary>
public enum SaveLoadingGameState
{
    /// <summary>The opening cinematic and a new Ceres start.</summary>
    OpeningCinematic,
    /// <summary>The file-select area map and saved checkpoint.</summary>
    MainGame,
    /// <summary>The initial Ceres-elevator arrival.</summary>
    CeresElevatorArrival,
    /// <summary>The Ceres-destruction cinematic.</summary>
    CeresDestruction,
}
