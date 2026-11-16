namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What the reward Samus's jump animations have their owner do (#2197).</summary>
internal enum EndingRewardJumpCue
{
    /// <summary>Launches Samus's jump.</summary>
    Launch,
    /// <summary>Readies the suited Samus's separate head.</summary>
    PrepareHead,
    /// <summary>Launches the suited Samus's head.</summary>
    LaunchHead,
    /// <summary>Starts Samus's body flying up on her jump.</summary>
    StartBodyFlight,
    /// <summary>Starts the suited Samus's head flying up ahead of her body.</summary>
    StartHeadFlight,
    /// <summary>Fires Samus's post-credits shot.</summary>
    Shoot,
}
