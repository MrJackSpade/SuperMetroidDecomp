namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Fake Kraid's three body-spike ports, each launching from its own spike clock. Main AI visits the clocks in the
/// declared order, one per frame, wrapping from the last to the first; nothing else reads the values.
/// </summary>
internal enum FakeKraidSpikeRow
{
    /// <summary>The top port, visited first.</summary>
    Top,
    /// <summary>The middle port, visited after the top.</summary>
    Middle,
    /// <summary>The bottom port, visited after the middle.</summary>
    Bottom,
}
