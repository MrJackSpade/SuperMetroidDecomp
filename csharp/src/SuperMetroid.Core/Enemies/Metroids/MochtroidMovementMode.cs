namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Mochtroid's one-frame movement selector. Main AI clears it
/// before dispatch, so touch collision must republish <see cref="TouchingSamus"/> every frame
/// for the creature to remain latched on.
/// </summary>
public enum MochtroidMovementMode
{
    /// <summary>Uses proportional free flight toward Samus.</summary>
    NotTouchingSamus = 0,
    /// <summary>Moves one pixel per axis toward Samus while contact is republished.</summary>
    TouchingSamus = 1,
    /// <summary>Runs the shipped but unreferenced four-direction shake movement.</summary>
    Shaking = 2,
}
