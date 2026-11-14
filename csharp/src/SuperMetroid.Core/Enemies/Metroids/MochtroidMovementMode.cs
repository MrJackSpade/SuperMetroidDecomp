namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// How the Mochtroid moves this frame. Main AI clears it
/// before dispatch, so touch collision must republish <see cref="TouchingSamus"/> every frame
/// for the creature to remain latched on.
/// </summary>
public enum MochtroidMovementMode
{
    /// <summary>Uses proportional free flight toward Samus.</summary>
    NotTouchingSamus,
    /// <summary>Moves one pixel per axis toward Samus while contact is republished.</summary>
    TouchingSamus,
}
