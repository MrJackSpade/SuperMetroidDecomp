namespace SuperMetroid.Core.Frontend;

/// <summary>Dispatcher state selected when a native loading dispatch finishes its waits.</summary>
internal enum GameLoadingCompletion
{
    GameplayFadeIn,
    CeresArrivalFadeIn,
    MainGameplay,
}
