namespace SuperMetroid.Core.Progression;

/// <summary>
/// The startup route a save resumes by after file selection: the opening cinematic for a new game, the file-select area
/// map and saved checkpoint for the main game, or one of the two Ceres checkpoints.
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
