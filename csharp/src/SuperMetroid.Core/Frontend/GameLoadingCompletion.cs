namespace SuperMetroid.Core.Frontend;

/// <summary>Dispatcher state selected when a native loading dispatch finishes its waits.</summary>
internal enum GameLoadingCompletion
{
    /// <summary>Identifies gameplay fade in.</summary>
    GameplayFadeIn,
    /// <summary>Identifies ceres arrival fade in.</summary>
    CeresArrivalFadeIn,
    /// <summary>Identifies main gameplay.</summary>
    MainGameplay,
}
