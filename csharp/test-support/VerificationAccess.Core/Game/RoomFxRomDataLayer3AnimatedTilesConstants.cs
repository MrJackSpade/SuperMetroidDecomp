using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="RoomFxRomData.Layer3AnimatedTiles"/> that only verification reads.</summary>
internal static class RoomFxRomDataLayer3AnimatedTilesConstants
{
    /// <summary>First acid character frame at $87:A6A4.</summary>
    public const ushort AcidFirstFrame = 0xa6a4;
    /// <summary>First lava character frame at $87:A564.</summary>
    public const ushort LavaFirstFrame = 0xa564;
    /// <summary>First rain character frame at $87:A874.</summary>
    public const ushort RainFirstFrame = 0xa874;

    /// <summary>Provides the first acid, lava, and rain Layer 3 frame pointers for animated room effects.</summary>
    extension(RoomFxRomData.Layer3AnimatedTiles)
    {
        /// <inheritdoc cref="RoomFxRomDataLayer3AnimatedTilesConstants.AcidFirstFrame"/>
        internal static ushort AcidFirstFrame => RoomFxRomDataLayer3AnimatedTilesConstants.AcidFirstFrame;
        /// <inheritdoc cref="RoomFxRomDataLayer3AnimatedTilesConstants.LavaFirstFrame"/>
        internal static ushort LavaFirstFrame => RoomFxRomDataLayer3AnimatedTilesConstants.LavaFirstFrame;
        /// <inheritdoc cref="RoomFxRomDataLayer3AnimatedTilesConstants.RainFirstFrame"/>
        internal static ushort RainFirstFrame => RoomFxRomDataLayer3AnimatedTilesConstants.RainFirstFrame;
    }
}
