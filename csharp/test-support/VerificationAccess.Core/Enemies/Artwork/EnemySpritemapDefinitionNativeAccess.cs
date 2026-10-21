namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification's native view of an installed composition, for oracles that derive frames from the cartridge.</summary>
internal static class EnemySpritemapDefinitionNativeAccess
{
    /// <summary>The native view of a composition.</summary>
    extension(EnemySpritemapDefinition definition)
    {
        /// <summary>Where the cartridge stores the composition's frame, and its name.</summary>
        internal NativeEnemySpritemapDefinition Native => new(definition.Bank, definition.Pointer, definition.Name);
    }
}
