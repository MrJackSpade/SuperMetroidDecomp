namespace SuperMetroid.Core.Frontend;

/// <summary>The state a game load enters once its waits finish.</summary>
internal enum GameLoadingCompletion
{
    /// <summary>Begins the ordinary gameplay fade once the loading waits have finished.</summary>
    GameplayFadeIn,
    /// <summary>Begins the Ceres arrival fade once the loading waits have finished.</summary>
    CeresArrivalFadeIn,
    /// <summary>Publishes gameplay immediately after the loading waits finish.</summary>
    MainGameplay,
}
