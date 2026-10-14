using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SpacetimeBeamCopyDefinitions"/> that only verification reads.</summary>
internal static class SpacetimeBeamCopyDefinitionsConstants
{
    /// <summary>$00:FFFF ends the immutable source before the long word carries into low WRAM.</summary>
    public const int LastSourceAddress = 0x00ffff;

    /// <summary>Provides the final native source address for spacetime beam copy checks.</summary>
    extension(SpacetimeBeamCopyDefinitions)
    {
        /// <inheritdoc cref="SpacetimeBeamCopyDefinitionsConstants.LastSourceAddress"/>
        internal static int LastSourceAddress => SpacetimeBeamCopyDefinitionsConstants.LastSourceAddress;
    }
}
