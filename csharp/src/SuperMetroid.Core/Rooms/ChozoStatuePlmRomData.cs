namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$84 identities shared by Chozo enemy requests and terrain collision.</summary>
internal static class ChozoStatuePlmRomData
{
    /// <summary>$84:D3D7, replace the two Wrecked Ship spike slopes with ordinary slopes.</summary>
    public const ushort TransformSpikesToSlopes = 0xd3d7;
    /// <summary>$84:D3F4, restore the same two blocks to spike collision.</summary>
    public const ushort RevertSlopesToSpikes = 0xd3f4;
    /// <summary>$84:D155, restore the lowered acid's base height on room re-entry.</summary>
    public const ushort SetLoweredAcidHeight = 0xd155;
    /// <summary>$84:D15C, wait for blank air at (4,8), then install Lower Norfair BTS $83.</summary>
    public const ushort WaitForLowerNorfairHand = 0xd15c;
    /// <summary>$84:D158 writes this lowered-acid base Y coordinate.</summary>
    public const ushort LoweredAcidY = 0x2d2;
    /// <summary>$84:D3D8 and $D3F5 use level-data byte offset $1608.</summary>
    public const int FirstSlopeBlockIndex = 0x1608 / 2;
    /// <summary>$84:D3DB writes slope BTS $12 at the first slope.</summary>
    public const byte FirstSlopeBts = 0x12;
    /// <summary>$84:D3E4 writes slope BTS $13 at the adjacent slope.</summary>
    public const byte SecondSlopeBts = 0x13;
    /// <summary>$84:D616 special-solid hand selector in Wrecked Ship's area table.</summary>
    public static readonly RoomBlockBehavior WreckedShipHandBts = new(0x80);
    /// <summary>$84:D180 special-solid hand selector in Norfair's area table.</summary>
    public static readonly RoomBlockBehavior LowerNorfairHandBts = new(0x83);
    /// <summary>$84:8DA0 default draw pointer remains in A entering hardcoded setup $D108.</summary>
    public const ushort HardcodedCrumbleSetupLevelWord = 0x0da0;
    /// <summary>$84:D15F-$D16A probes this column for the lowered hand trigger.</summary>
    public const int LowerNorfairTriggerX = 4;
    /// <summary>$84:D15F-$D16A probes this row for the lowered hand trigger.</summary>
    public const int LowerNorfairTriggerY = 8;
    /// <summary>$84:D17B requires the complete blank-air level word, not only type zero.</summary>
    public const ushort BlankAirLevelWord = 0x00ff;
}
