namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How an intro Rinka flies once its animation starts it moving (#1903).</summary>
internal enum IntroRinkaFlight
{
    /// <summary>Into Samus: when it reaches her it knocks her back and goes.</summary>
    StrikesSamus,

    /// <summary>Past Samus, until it leaves the screen above or below.</summary>
    MissesSamus,
}
