using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="GunshipMotionDefinitions"/> that only verification reads.</summary>
internal static class GunshipMotionDefinitionsConstants
{
    /// <summary>$A2:A622 ShipBrakesMovementData: seventeen signed word Y deltas.</summary>
    public const int BrakeReferenceAddress = 0xa2a622;
    /// <summary>$A2:A7D0 ProcessShipHover.YVelocity: four signed bytes at stride two.</summary>
    public const int HoverDeltaReferenceAddress = 0xa2a7d0;
    /// <summary>$A2:A7CF ProcessShipHover.timer: four unsigned bytes at stride two.</summary>
    public const int HoverTimerReferenceAddress = 0xa2a7cf;

    /// <summary>Provides the native gunship brake, hover displacement, and hover timer reference addresses.</summary>
    extension(GunshipMotionDefinitions)
    {
        /// <inheritdoc cref="GunshipMotionDefinitionsConstants.BrakeReferenceAddress"/>
        internal static int BrakeReferenceAddress => GunshipMotionDefinitionsConstants.BrakeReferenceAddress;
        /// <inheritdoc cref="GunshipMotionDefinitionsConstants.HoverDeltaReferenceAddress"/>
        internal static int HoverDeltaReferenceAddress => GunshipMotionDefinitionsConstants.HoverDeltaReferenceAddress;
        /// <inheritdoc cref="GunshipMotionDefinitionsConstants.HoverTimerReferenceAddress"/>
        internal static int HoverTimerReferenceAddress => GunshipMotionDefinitionsConstants.HoverTimerReferenceAddress;
    }
}
