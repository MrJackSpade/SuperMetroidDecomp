namespace SuperMetroid.Core.Fx;

/// <summary>Verification's named entry points within the bank-$87 Wrecked Ship treadmill instruction streams.</summary>
internal static class AnimatedTileInstructionListPointers
{
    /// <summary>Rightward treadmill boss-wait entry at $87:81E1.</summary>
    public const ushort WreckedShipTreadmillRightwardsWait = 0x81e1;
    /// <summary>Leftward treadmill boss-wait entry at $87:81F7.</summary>
    public const ushort WreckedShipTreadmillLeftwardsWait = 0x81f7;
    /// <summary>Leftward treadmill four-frame loop at $87:81F9.</summary>
    public const ushort WreckedShipTreadmillLeftwardsLoop = 0x81f9;
    /// <summary>Rightward treadmill four-frame loop at $87:81E3.</summary>
    public const ushort WreckedShipTreadmillRightwardsLoop = 0x81e3;
}
