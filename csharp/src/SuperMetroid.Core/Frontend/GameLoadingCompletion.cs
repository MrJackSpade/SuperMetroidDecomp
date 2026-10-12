namespace SuperMetroid.Core.Frontend;

/// <summary>Dispatcher state selected when a native loading dispatch finishes its waits.</summary>
internal enum GameLoadingCompletion
{
    /// <summary>Begins the ordinary gameplay fade once the loading NMIs have finished.</summary>
    GameplayFadeIn,
    /// <summary>Begins the Ceres arrival fade once the loading NMIs have finished.</summary>
    CeresArrivalFadeIn,
    /// <summary>Publishes gameplay immediately after the loading waits finish.</summary>
    MainGameplay,
}
