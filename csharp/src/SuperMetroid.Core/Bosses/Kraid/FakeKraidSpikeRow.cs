namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Fake Kraid's three body-spike ports, each launching from its own spike clock. Main AI visits the clocks top, middle,
/// bottom, one per frame, then the top again; nothing reads the values or their order.
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
