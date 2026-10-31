namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Dispatch-table indexes stored in ordinary Metroid variable F. These are indexes,
/// not function addresses: main AI uses the word directly to select one of four
/// movement routines.
/// </summary>
public enum MetroidAiFunction
{
    /// <summary>Index 0 chases Samus: proportional 16.16 attraction toward Samus's X and eight pixels above her Y, capped at three pixels per update.</summary>
    Homing = 0,
    /// <summary>Index 1 latches onto Samus: byte-oriented close-range seeking entered on touch before the attachment threshold is reached.</summary>
    ClosingOnSamus = 1,
    /// <summary>Index 2 holds on to Samus: hard-lock position to Samus's X and Y minus eight pixels, clearing movement velocities.</summary>
    AttachedToSamus = 2,
    /// <summary>Index 3 is bombed off Samus: perform four displacement steps after bomb collision detaches the Metroid, then resume homing.</summary>
    PowerBombEscape = 3,
}
