namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$84 identities shared by Chozo enemy requests and terrain collision.</summary>
internal static class ChozoStatuePlmRomData
{
    /// <summary>$84:D6EE, Wrecked Ship hand; setup $D616 writes solid special BTS $80.</summary>
    public const ushort WreckedShipHand = 0xd6ee;
    /// <summary>$84:D6F2, downward morph contact with the Wrecked Ship hand.</summary>
    public const ushort WreckedShipTrigger = 0xd6f2;
    /// <summary>$84:D6F8, clear the walking statue's slope access using the ROM draw list.</summary>
    public const ushort ClearSlopeAccess = 0xd6f8;
    /// <summary>$84:D6FC, block slope access before and after the walking sequence.</summary>
    public const ushort BlockSlopeAccess = 0xd6fc;
    /// <summary>$84:D6D6, sleeping Lower Norfair hand controller.</summary>
    public const ushort LowerNorfairHand = 0xd6d6;
    /// <summary>$84:D6DA, Space Jump/downward morph admission for Lower Norfair.</summary>
    public const ushort LowerNorfairTrigger = 0xd6da;
    /// <summary>$84:D113, crumbling plug and walking statue's crumbling footstep terrain.</summary>
    public const ushort CrumblePlug = 0xd113;
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
