using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The state of Fake Kraid. “Mini-Kraid” is the name its item drops and projectiles use; both names refer to
/// the same enemy definition.
/// </summary>
public sealed class FakeKraidEnemyState
{
    /// <summary>Mini-Kraid's animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FakeKraidSegment> Playback { get; set; }

    /// <summary>Signed whole-pixel walk delta.</summary>
    public int WalkDelta { get; internal set; }

    /// <summary>Signed four-pixel facing marker.</summary>
    public int FacingDelta { get; internal set; }

    /// <summary>
    /// Walk steps remaining before the next forced reversal. A step expires when its decrement reaches zero, and the
    /// expiry reloads it.
    /// </summary>
    public int WalkStepTimer { get; internal set; }

    /// <summary>Number of animation decision points before the next spit attack; it stops at zero.</summary>
    public int SpitDecisionTimer { get; internal set; }

    /// <summary>
    /// Each body port's independent spike clock. Main AI visits exactly one clock per frame, top, middle, bottom, then
    /// top again, counting a running clock down to zero without wrapping and reloading it when it is found at zero.
    /// </summary>
    internal Dictionary<FakeKraidSpikeRow, int> SpikeTimers { get; } = [];

    /// <summary>The body port whose spike clock main AI visits on the next frame.</summary>
    internal FakeKraidSpikeRow NextSpikeRow { get; set; }

}
