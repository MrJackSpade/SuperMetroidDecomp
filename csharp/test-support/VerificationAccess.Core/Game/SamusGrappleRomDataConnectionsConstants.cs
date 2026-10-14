using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SamusGrappleRomData.Connections"/> that only verification reads.</summary>
internal static class SamusGrappleRomDataConnectionsConstants
{
    /// <summary>Number of shot directions represented by each four-byte connection table.</summary>
    public const int DirectionCount = SamusGrappleRomData.Firing.DirectionCount;
    /// <summary>Bytes in one special-angle record.</summary>
    public const int SpecialAngleRecordByteCount = 10;
    /// <summary>Number of fixed-angle special connection records.</summary>
    public const int SpecialAngleRecordCount = 8;
    /// <summary>Special-angle records used for fixed, swing, and wall-grab reactions.</summary>
    public const int SpecialAngleTable = 0x9bc43e;

    /// <summary>Provides grapple connection direction count and special-angle table layout.</summary>
    extension(SamusGrappleRomData.Connections)
    {
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.DirectionCount"/>
        internal static int DirectionCount => SamusGrappleRomDataConnectionsConstants.DirectionCount;
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordByteCount"/>
        internal static int SpecialAngleRecordByteCount => SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordByteCount;
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordCount"/>
        internal static int SpecialAngleRecordCount => SamusGrappleRomDataConnectionsConstants.SpecialAngleRecordCount;
        /// <inheritdoc cref="SamusGrappleRomDataConnectionsConstants.SpecialAngleTable"/>
        internal static int SpecialAngleTable => SamusGrappleRomDataConnectionsConstants.SpecialAngleTable;
    }
}
