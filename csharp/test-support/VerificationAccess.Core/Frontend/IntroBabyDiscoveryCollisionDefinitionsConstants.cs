using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroBabyDiscoveryCollisionDefinitions"/> that only verification reads.</summary>
internal static class IntroBabyDiscoveryCollisionDefinitionsConstants
{
    /// <summary>$8C:C083, first byte copied by the discovery room setup.</summary>
    public const int SourceAddress = 0x8cc083;
    /// <summary>The native setup copies exactly $300 bytes.</summary>
    public const int SourceByteCount = IntroBabyDiscoveryCollisionDefinitions.Columns * IntroBabyDiscoveryCollisionDefinitions.SourceRows * sizeof(ushort);

    /// <summary>Provides the source span of the intro Baby discovery collision records.</summary>
    extension(IntroBabyDiscoveryCollisionDefinitions)
    {
        /// <inheritdoc cref="IntroBabyDiscoveryCollisionDefinitionsConstants.SourceAddress"/>
        internal static int SourceAddress => IntroBabyDiscoveryCollisionDefinitionsConstants.SourceAddress;
        /// <inheritdoc cref="IntroBabyDiscoveryCollisionDefinitionsConstants.SourceByteCount"/>
        internal static int SourceByteCount => IntroBabyDiscoveryCollisionDefinitionsConstants.SourceByteCount;
    }
}
