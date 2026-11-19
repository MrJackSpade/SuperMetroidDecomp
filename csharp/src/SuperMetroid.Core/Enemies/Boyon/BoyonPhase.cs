namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Where a Boyon stands in its rest-and-bounce cycle (#2432): the first update after its load derives its arc, then it
/// rests until Samus comes near, bounces, rising and then falling (#2531), lands at its arc's baseline and either bounces
/// again or settles back to rest.
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
    /// <summary>
    /// The rise of a bounce, with its bouncing animation: it moves up its curve an entry an update until it passes the
    /// top of the arc and falls.
    /// </summary>
    Rising,
    /// <summary>
    /// The fall of a bounce, with its bouncing animation: it moves down its curve an entry an update until it lands at
    /// its baseline.
    /// </summary>
    Falling,
    /// <summary>
    /// Landed at its baseline from a bounce, its bouncing animation still playing: it bounces again, restarting that
    /// animation, if Samus is near on its next update, and otherwise settles back to rest.
    /// </summary>
    Landed,
}
