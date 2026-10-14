using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="OwtchMovementDefinitions"/> that only verification reads.</summary>
internal static class OwtchMovementDefinitionsConstants
{
    /// <summary>$A2:A3DD OwtchConstants_XDistanceRanges, eight unsigned patrol half-widths.</summary>
    public const int DistanceReferenceAddress = 0xa2a3dd;
    /// <summary>$A2:A3ED OwtchConstants_undergroundTimers, six unsigned burial durations.</summary>
    public const int TimerReferenceAddress = 0xa2a3ed;

    /// <summary>Provides native Owtch movement distance and timer reference addresses.</summary>
    extension(OwtchMovementDefinitions)
    {
        /// <inheritdoc cref="OwtchMovementDefinitionsConstants.DistanceReferenceAddress"/>
        internal static int DistanceReferenceAddress => OwtchMovementDefinitionsConstants.DistanceReferenceAddress;
        /// <inheritdoc cref="OwtchMovementDefinitionsConstants.TimerReferenceAddress"/>
        internal static int TimerReferenceAddress => OwtchMovementDefinitionsConstants.TimerReferenceAddress;
    }
}
