namespace SuperMetroid.Core.Samus;

/// <summary>
/// The frame handler's beta, <c>SamusNewStateHandler</c> (WRAM $0A44), as derived from the port's ownership flags. The
/// pointer holds exactly one handler at a time.
/// </summary>
internal enum SamusBetaHandler
{
    /// <summary>
    /// <c>$90:E725 SamusNewStateHandler_Normal</c>: movement, the timer/hack handler, animation, pose transitions,
    /// periodic damage and the low-energy check.
    /// </summary>
    Normal,

    /// <summary>
    /// Another locked handler, such as <c>$90:E8EC SamusNewStateHandler_RidingElevator</c> or Mother Brain's
    /// <c>RTL_90E8D9</c>: no low-energy check.
    /// </summary>
    Locked,

    /// <summary><c>$90:E8DC SamusNewStateHandler_SamusIsLocked</c>, installed by Samus command zero: no animation or pose transitions.</summary>
    ScriptLocked,

    /// <summary><c>RTL_90E8D6</c>, installed by command six (refill and map stations): neither moves nor animates Samus.</summary>
    RefillStation,

    /// <summary><c>RTL_90E8CD</c>, installed by suit command $15: the empty beta of the suit transformation.</summary>
    SuitPickup,

    /// <summary><c>$90:E7F5 SamusNewStateHandler_TitleDemo</c>: the attract demo's beta, without periodic damage or energy checks.</summary>
    TitleDemo,

    /// <summary>
    /// <c>$90:E902 SamusNewStateHandler_EnteringExitingGunship</c>: its own low-health checker and no escape-timer
    /// handler.
    /// </summary>
    Gunship,

    /// <summary>The death sequence's game states ($15-$18), which call neither handler.</summary>
    Fatal,
}
