namespace SuperMetroid.Core.Samus;

/// <summary>
/// The frame handler's alpha, <c>SamusCurrentStateHandler</c> (WRAM $0A42), as derived from the port's ownership
/// flags. The pointer holds exactly one handler at a time.
/// </summary>
internal enum SamusAlphaHandler
{
    /// <summary><c>$90:E695 SamusCurrentStateHandler_Normal</c>.</summary>
    Normal,

    /// <summary><c>$90:E6C9 SamusCurrentStateHandler_Demo</c>: the attract demo's alpha.</summary>
    Demo,

    /// <summary><c>$90:E713 SamusCurrentStateHandler_SamusIsLocked</c>: no pose input, radius or inside-block work.</summary>
    Locked,

    /// <summary>The death sequence's game states ($15-$18), which call neither handler.</summary>
    Fatal,

    /// <summary>Samus movement is disabled (the appearance fanfare, Ceres arrival), so no alpha tail runs.</summary>
    None,
}
