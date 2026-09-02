namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 PLM header pointers consumed by room integration code.</summary>
internal static class RoomPlmHeaders
{
    /// <summary>Clear Crocomire bridge PLM at $84:B747.</summary>
    public const ushort ClearCrocomireBridge = 0xb747;

    /// <summary>Crumble one Crocomire bridge block PLM at $84:B74B.</summary>
    public const ushort CrumbleCrocomireBridgeBlock = 0xb74b;

    /// <summary>Clear one Crocomire bridge block PLM at $84:B74F.</summary>
    public const ushort ClearCrocomireBridgeBlock = 0xb74f;

    /// <summary>Clear Crocomire's invisible wall PLM at $84:B753.</summary>
    public const ushort ClearCrocomireInvisibleWall = 0xb753;

    /// <summary>Create Crocomire's invisible wall PLM at $84:B757.</summary>
    public const ushort CreateCrocomireInvisibleWall = 0xb757;

    /// <summary>Crumble Spore Spawn's ceiling PLM at $84:B78F.</summary>
    public const ushort CrumbleSporeSpawnCeiling = 0xb78f;

    /// <summary>Clear Spore Spawn's ceiling PLM at $84:B793.</summary>
    public const ushort ClearSporeSpawnCeiling = 0xb793;

    /// <summary>Clear Botwoon's wall PLM at $84:B797.</summary>
    public const ushort ClearBotwoonWall = 0xb797;

    /// <summary>Crumble Botwoon's wall PLM at $84:B79B.</summary>
    public const ushort CrumbleBotwoonWall = 0xb79b;
}

/// <summary>Named bank-$84 PLM instruction-list pointers shared across room systems.</summary>
internal static class RoomPlmInstructionLists
{
    /// <summary>Clear Crocomire bridge instruction list at $84:AFCA.</summary>
    public const ushort ClearCrocomireBridge = 0xafca;

    /// <summary>Crumble Crocomire bridge block instruction list at $84:AFD0.</summary>
    public const ushort CrumbleCrocomireBridgeBlock = 0xafd0;

    /// <summary>Clear Crocomire bridge block instruction list at $84:AFD6.</summary>
    public const ushort ClearCrocomireBridgeBlock = 0xafd6;

    /// <summary>Clear Crocomire invisible wall instruction list at $84:AFDC.</summary>
    public const ushort ClearCrocomireInvisibleWall = 0xafdc;

    /// <summary>Create Crocomire invisible wall instruction list at $84:AFE2.</summary>
    public const ushort CreateCrocomireInvisibleWall = 0xafe2;
}
