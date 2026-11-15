namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// How Fake Kraid's spit or one of his spikes moves itself (#1843).
/// </summary>
internal enum FakeKraidProjectileMotion
{
    /// <summary>Flies Fake Kraid's spit with room collision, then capped gravity.</summary>
    SpitFlying,

    /// <summary>Flies a Fake Kraid spike horizontally until it strikes a wall.</summary>
    SpikeFlying,
}
