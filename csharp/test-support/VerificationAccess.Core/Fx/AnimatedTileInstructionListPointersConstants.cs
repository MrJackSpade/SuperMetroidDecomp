
namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values of <see cref="AnimatedTileInstructionListPointers"/> that only verification reads.</summary>
internal static class AnimatedTileInstructionListPointersConstants
{
    /// <summary>Leftward treadmill four-frame loop at $87:81F9.</summary>
    public const ushort WreckedShipTreadmillLeftwardsLoop = 0x81f9;
    /// <summary>Rightward treadmill four-frame loop at $87:81E3.</summary>
    public const ushort WreckedShipTreadmillRightwardsLoop = 0x81e3;

    /// <summary>Points verification at the left and right Wrecked Ship treadmill animation loops in the native instruction lists.</summary>
    extension(AnimatedTileInstructionListPointers)
    {
        /// <inheritdoc cref="AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillLeftwardsLoop"/>
        internal static ushort WreckedShipTreadmillLeftwardsLoop => AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillLeftwardsLoop;
        /// <inheritdoc cref="AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillRightwardsLoop"/>
        internal static ushort WreckedShipTreadmillRightwardsLoop => AnimatedTileInstructionListPointersConstants.WreckedShipTreadmillRightwardsLoop;
    }
}
