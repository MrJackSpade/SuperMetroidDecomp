using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Cartridge values of <see cref="PaletteFxHeatAnimationScriptDefinitions"/> that only verification reads.</summary>
internal static class PaletteFxHeatAnimationScriptDefinitionsConstants
{
    /// <summary>
    /// <c>FrameBehavior_PaletteFXObject_SamusInHeat.InstListPointers.gravity</c> at
    /// <c>$8D:E3E0</c>.
    /// </summary>
    public const ushort GravitySourceTable = 0xe3e0;
    /// <summary>
    /// <c>FrameBehavior_PaletteFXObject_SamusInHeat.InstListPointers.power</c> at
    /// <c>$8D:E420</c>.
    /// </summary>
    public const ushort PowerSourceTable = 0xe420;
    /// <summary>
    /// <c>FrameBehavior_PaletteFXObject_SamusInHeat.InstListPointers.varia</c> at
    /// <c>$8D:E400</c>.
    /// </summary>
    public const ushort VariaSourceTable = 0xe400;

    /// <summary>Provides the native gravity, power, and Varia heat palette source tables.</summary>
    extension(PaletteFxHeatAnimationScriptDefinitions)
    {
        /// <inheritdoc cref="PaletteFxHeatAnimationScriptDefinitionsConstants.GravitySourceTable"/>
        internal static ushort GravitySourceTable => PaletteFxHeatAnimationScriptDefinitionsConstants.GravitySourceTable;
        /// <inheritdoc cref="PaletteFxHeatAnimationScriptDefinitionsConstants.PowerSourceTable"/>
        internal static ushort PowerSourceTable => PaletteFxHeatAnimationScriptDefinitionsConstants.PowerSourceTable;
        /// <inheritdoc cref="PaletteFxHeatAnimationScriptDefinitionsConstants.VariaSourceTable"/>
        internal static ushort VariaSourceTable => PaletteFxHeatAnimationScriptDefinitionsConstants.VariaSourceTable;
    }
    /// <summary>
    /// Applies the cartridge's Gravity-before-Varia equipment priority and returns the
    /// program selected for the published heat phase.
    /// </summary>
    public static ushort ResolveForEquippedItems(SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, ushort phase)
    {
        ColourEffectHeatSuit suit = equippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? ColourEffectHeatSuit.Gravity
            : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? ColourEffectHeatSuit.Varia
                : ColourEffectHeatSuit.Power;
        return PaletteFxHeatAnimationScriptDefinitions.Resolve(suit, phase);
    }
}
