namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Where Crocomire's arena raises and clears its invisible walls and bridge, and where his dead skeleton lies (#2182).</summary>
internal static partial class CrocomireArenaDefinitions
{
    /// <summary>The invisible wall at the fight's left, cleared as his skeleton breaks through the spike wall.</summary>
    internal static readonly Position16<RoomBlocks> FightWallBlock = new(32, 3);

    /// <summary>The invisible wall before the spike wall, raised as his skeleton breaks through and cleared once the arena opens.</summary>
    internal static readonly Position16<RoomBlocks> SpikeWallBlock = new(30, 3);

    /// <summary>The invisible wall the bridge's collapse raises behind Samus, cleared as his skeleton flows down the river.</summary>
    internal static readonly Position16<RoomBlocks> BridgeWallBlock = new(78, 3);

    /// <summary>The invisible wall raised as Samus reaches the hidden wall, cleared as his dead skeleton is laid down.</summary>
    internal static readonly Position16<RoomBlocks> SkeletonWallBlock = new(48, 3);

    /// <summary>The bridge's first block: its approach crumbles the bridge from here, and a defeated room clears the bridge from here.</summary>
    internal static readonly Position16<RoomBlocks> BridgeFirstBlock = new(97, 11);

    /// <summary>The blocks of the bridge the collapse clears, one after another from <see cref="BridgeFirstBlock"/>.</summary>
    internal const int BridgeBlockCount = 10;

    /// <summary>The block at which his skeleton's break-through clears the bridge again.</summary>
    internal static readonly Position16<RoomBlocks> BreakThroughBridgeBlock = new(112, 11);

    /// <summary>Where a defeated room lays his dead skeleton.</summary>
    internal static readonly Position16<RoomPixels> CorpsePosition = new(576, 144);

    /// <summary>The reach of his dead skeleton, however it was laid down.</summary>
    internal static readonly Extent16<RoomPixels> CorpseRadius = new(40, 28);
}
