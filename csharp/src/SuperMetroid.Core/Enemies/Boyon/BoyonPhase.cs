namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Where a Boyon stands in its rest-and-bounce cycle (#2432): the first update after its load derives its arc, then it
/// rests until Samus comes near, bounces, lands at its arc's baseline and either bounces again or settles back to rest.
/// </summary>
public enum BoyonPhase
{
    /// <summary>
    /// Freshly loaded: its first update derives its arc's baseline and does nothing else, so its bounces start an update
    /// later than its load.
    /// </summary>
    DerivingArc,
    /// <summary>Resting at its baseline with its idle animation, until Samus comes near.</summary>
    Resting,
    /// <summary>Bouncing along its arc with its bouncing animation, until it lands at its baseline.</summary>
    Bouncing,
    /// <summary>
    /// Landed at its baseline from a bounce, its bouncing animation still playing: it bounces again, restarting that
    /// animation, if Samus is near on its next update, and otherwise settles back to rest.
    /// </summary>
    Landed,
}
