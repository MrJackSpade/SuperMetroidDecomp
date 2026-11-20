namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A step that shows a frontend screen (#2554): the step carries the screen it shows, so the screen exists exactly while the
/// session is at that step.
/// </summary>
internal interface IScreenPhase : IGamePhase
{
    /// <summary>The menu or cinematic the step shows.</summary>
    IFrontendScreen Screen { get; }
}
