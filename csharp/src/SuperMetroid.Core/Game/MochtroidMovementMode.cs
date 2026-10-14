namespace SuperMetroid.Core.Game;

/// <summary>
/// The one-frame movement selector stored in Mochtroid variable F. Main AI clears this word
/// before dispatch, so touch collision must republish <see cref="TouchingSamus"/> every frame
/// for the creature to remain latched on.
/// </summary>
public enum MochtroidMovementMode : ushort
{
    /// <summary>Uses proportional free flight toward Samus.</summary>
    NotTouchingSamus = 0,
    /// <summary>Moves one pixel per axis toward Samus while contact is republished.</summary>
    TouchingSamus = 1,
    /// <summary>Runs the shipped but unreferenced four-direction shake routine.</summary>
    Shaking = 2,
}
