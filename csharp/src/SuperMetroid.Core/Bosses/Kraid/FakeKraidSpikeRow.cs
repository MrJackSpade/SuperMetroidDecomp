namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Mutually exclusive body-spike ports selected by the three native timers.</summary>
internal enum FakeKraidSpikeRow
{
    /// <summary>Top timer at native byte offset0; launch offset word at86:9E7D.</summary>
    Top,
    /// <summary>Middle timer at native byte offset2; launch offset word at86:9E7F.</summary>
    Middle,
    /// <summary>Bottom timer at native byte offset4; launch offset word at86:9E81.</summary>
    Bottom,
}
