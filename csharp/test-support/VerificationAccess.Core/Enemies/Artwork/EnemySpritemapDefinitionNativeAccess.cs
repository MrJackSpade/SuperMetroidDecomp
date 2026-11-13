using SuperMetroid.AssetExtraction.World.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification's native view of an installed composition, for oracles that derive frames from the cartridge.</summary>
internal static class EnemySpritemapDefinitionNativeAccess
{
    /// <summary>The native view of a composition.</summary>
    extension(EnemyFrameName definition)
    {
        /// <summary>
        /// Where the cartridge stores the composition's frame, and the name composition documents gave it before frames
        /// were named for their meaning (#1465), which oracles derive from that place; its current name when it kept it.
        /// </summary>
        internal NativeEnemySpritemapDefinition Native => new(definition.Bank, definition.Pointer, definition.LegacyName);

        /// <summary>
        /// The name composition documents gave the frame before frames were named for their meaning (#1465), or before
        /// #1623 named them for the animation segment or table key that first shows them; its current name when it kept it.
        /// </summary>
        internal string LegacyName =>
            EnemyFrameLegacyNameDefinitions.OrdinaryCompositions.TryGetValue(definition.Frame, out string? ordinary) ? ordinary
            : EnemyFrameLegacyNameDefinitions.ExtendedCompositions.TryGetValue(definition.Frame, out string? extended) ? extended
            : EnemyFrameLegacyNameDefinitions.BeforeSegmentNames.TryGetValue(definition.Frame, out string? before) ? before
            : definition.Name;
    }
}
