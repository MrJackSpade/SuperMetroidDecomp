using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="DraygonIntroDanceDefinitions"/> that only verification reads.</summary>
internal static class DraygonIntroDanceDefinitionsConstants
{
    /// <summary>
    /// The four reachable words of <c>MovementLatencyForEachEvirSpriteObject</c> at
    /// <c>$A5:A19F-$A5:A1A6</c>, ordered by sprite slots 28 through 31.
    /// </summary>
    /// <remarks>The native loop at $A5:A13E indexes with 2*(slot-28).</remarks>
    public const int NativeMovementLatencyAddress = 0xa5a19f;
    /// <summary>Native start of <c>DraygonFightIntroDanceData</c> at <c>$A5:CE07</c>.</summary>
    public const int NativeMovementStreamAddress = 0xa5ce07;

    /// <summary>Provides Draygon's native intro movement stream and latency addresses for dance timing checks.</summary>
    extension(DraygonIntroDanceDefinitions)
    {
        /// <inheritdoc cref="DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress"/>
        internal static int NativeMovementLatencyAddress => DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        /// <inheritdoc cref="DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress"/>
        internal static int NativeMovementStreamAddress => DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress;
    }
}
