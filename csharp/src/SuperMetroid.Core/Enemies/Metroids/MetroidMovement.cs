namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The ordinary Metroid's movement: main AI runs one of its four movement behaviours for the selected member.
/// </summary>
public enum MetroidMovement
{
    /// <summary>Chases Samus: proportional 1/65536-pixel attraction toward Samus's X and eight pixels above her Y, capped at three pixels per update.</summary>
    Homing = 0,
    /// <summary>Latches onto Samus: fixed-step close-range seeking entered on touch before the attachment threshold is reached.</summary>
    ClosingOnSamus = 1,
    /// <summary>Holds on to Samus: hard-lock position to Samus's X and Y minus eight pixels, clearing movement velocities.</summary>
    AttachedToSamus = 2,
    /// <summary>Bombed off Samus: perform four displacement steps after bomb collision detaches the Metroid, then resume homing.</summary>
    PowerBombEscape = 3,
}
