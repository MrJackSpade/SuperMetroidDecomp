namespace SuperMetroid.Core.Rooms;

/// <summary>Named cartridge constants for bank-$84 downward gates.</summary>
internal static class DownwardGatePlmRomData
{
    /// <summary>Five room blocks are marked solid while the closed gate occupies its column.</summary>
    public const byte GateHeightInBlocks = 5;
    /// <summary>Native BTS behavior written to every block in the closed gate column.</summary>
    public const byte ClosedGateBts = 0x10;
    /// <summary>Library-two sound queued for a rejected non-blue gate trigger shot.</summary>
    public const byte RejectedShotSound = 0x57;
    /// <summary>Native sound byte embedded in the gate's movement instructions.</summary>
    public const byte MovementSound = 0x0e;
}
