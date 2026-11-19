using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The state of Fake Kraid, the small Kraid in the room before Kraid's.
/// </summary>
public sealed class FakeKraidEnemyState
{
    /// <summary>Fake Kraid's animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FakeKraidSegment> Playback { get; set; }

    /// <summary>The side Fake Kraid walks toward; a wall or his walk clock running out turns him back.</summary>
    public HorizontalDirection Walking { get; internal set; }

    /// <summary>
    /// The side Fake Kraid faces: toward Samus, as each walk cue last found her (#2214); his spits and spikes fly this way.
    /// </summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// His walk steps before he turns round (#2538), counted by his walk animation's steps: he turns, without stepping, on
    /// its last step, which starts it again.
    /// </summary>
    public BossCountdown WalkTurnWait { get; } = new();

    /// <summary>
    /// His walk steps before he spits again (#2538), counted by his walk animation's steps. It holds once it has run out,
    /// and his next action choice then spits and starts it again.
    /// </summary>
    public HeldBossCountdown SpitWait { get; } = new();

    /// <summary>
    /// Each body port's spike clock (#2538). Main AI visits exactly one clock an update, top, middle, bottom, then top
    /// again, and each spikes on its clock's last visit, which starts the clock again.
    /// </summary>
    internal Dictionary<FakeKraidSpikeRow, BossCountdown> SpikeWaits { get; } = [];

    /// <summary>The body port whose spike clock main AI visits on the next frame.</summary>
    internal FakeKraidSpikeRow NextSpikeRow { get; set; }

}
