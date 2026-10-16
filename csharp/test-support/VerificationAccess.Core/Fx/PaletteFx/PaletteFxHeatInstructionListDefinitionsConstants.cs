using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="PaletteFxHeatInstructionListDefinitions"/> that only verification reads.</summary>
internal static class PaletteFxHeatInstructionListDefinitionsConstants
{
    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.gravity</c> at
    /// <c>$8D:E3E0</c>.
    /// </summary>
    public const ushort GravitySourceTable = 0xe3e0;
    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.power</c> at
    /// <c>$8D:E420</c>.
    /// </summary>
    public const ushort PowerSourceTable = 0xe420;
    /// <summary>
    /// <c>PreInstruction_PaletteFXObject_SamusInHeat.InstListPointers.varia</c> at
    /// <c>$8D:E400</c>.
    /// </summary>
    public const ushort VariaSourceTable = 0xe400;

    /// <summary>Provides the native gravity, power, and Varia heat palette source tables.</summary>
    extension(PaletteFxHeatInstructionListDefinitions)
    {
        /// <inheritdoc cref="PaletteFxHeatInstructionListDefinitionsConstants.GravitySourceTable"/>
        internal static ushort GravitySourceTable => PaletteFxHeatInstructionListDefinitionsConstants.GravitySourceTable;
        /// <inheritdoc cref="PaletteFxHeatInstructionListDefinitionsConstants.PowerSourceTable"/>
        internal static ushort PowerSourceTable => PaletteFxHeatInstructionListDefinitionsConstants.PowerSourceTable;
        /// <inheritdoc cref="PaletteFxHeatInstructionListDefinitionsConstants.VariaSourceTable"/>
        internal static ushort VariaSourceTable => PaletteFxHeatInstructionListDefinitionsConstants.VariaSourceTable;
    }
    /// <summary>
    /// Applies the cartridge's Gravity-before-Varia equipment priority and returns the
    /// program selected for the published heat phase.
    /// </summary>
    public static ushort ResolveForEquippedItems(SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems, ushort phase)
    {
        PaletteFxHeatSuit suit = equippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? PaletteFxHeatSuit.Gravity
            : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? PaletteFxHeatSuit.Varia
                : PaletteFxHeatSuit.Power;
        return PaletteFxHeatInstructionListDefinitions.Resolve(suit, phase);
    }
}
