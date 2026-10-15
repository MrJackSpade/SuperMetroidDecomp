namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The three bank-$84 Mother Brain escape-gate draw lists.</summary>
internal enum MotherBrainEscapeGateDraw : ushort
{
    /// <summary>Fully open vertical gate draw at $84:9473.</summary>
    Open = 0x9473,
    /// <summary>Half-closed vertical gate draw at $84:947F.</summary>
    HalfClosed = 0x947f,
    /// <summary>Fully closed vertical gate draw at $84:948B.</summary>
    Closed = 0x948b,
}
