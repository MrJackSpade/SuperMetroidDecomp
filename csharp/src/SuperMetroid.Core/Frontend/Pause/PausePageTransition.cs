namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>A pause-page change in progress (#2560): the page it leads to and the stage it has reached.</summary>
/// <param name="Destination">The page the change shows once it loads.</param>
/// <param name="Stage">The fade or load stage the change has reached.</param>
internal readonly partial record struct PausePageTransition(PauseScreenMode Destination, PausePageTransitionStage Stage);
