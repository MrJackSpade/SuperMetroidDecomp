namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// What Samus's Speed Booster echoes are doing (#2324): ordinary boosting captures her position into the first and second
/// echo in turn; cancelling the boost sends both departing toward her, which stops capture until they have gone. A Samus
/// who has not boosted captures the first echo next.
/// </summary>
public enum SpeedEchoPhase
{
    /// <summary>The next capture fills the first echo.</summary>
    CapturingFirst,

    /// <summary>The next capture fills the second echo.</summary>
    CapturingSecond,

    /// <summary>The echoes are departing toward Samus; nothing is captured.</summary>
    Departing,
}
