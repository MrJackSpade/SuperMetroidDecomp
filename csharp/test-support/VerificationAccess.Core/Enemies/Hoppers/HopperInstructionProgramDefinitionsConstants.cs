
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Cartridge values of <see cref="HopperInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class HopperInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_DessgeegaLarge_Landed_UpsideDown</c> at $A3:B25D.</summary>
    public const ushort LargeDessgeegaLandedCeiling = 0xb25d;
    /// <summary>The final hopper physics-table word immediately before the first program.</summary>
    public const ushort LastAdjacentPhysicsWord = 0xaa74;

    /// <summary>Provides the large Dessgeega ceiling-landing program and adjacent physics boundary for Hopper checks.</summary>
    extension(HopperInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="HopperInstructionProgramDefinitionsConstants.LargeDessgeegaLandedCeiling"/>
        internal static ushort LargeDessgeegaLandedCeiling => HopperInstructionProgramDefinitionsConstants.LargeDessgeegaLandedCeiling;
        /// <inheritdoc cref="HopperInstructionProgramDefinitionsConstants.LastAdjacentPhysicsWord"/>
        internal static ushort LastAdjacentPhysicsWord => HopperInstructionProgramDefinitionsConstants.LastAdjacentPhysicsWord;
    }
}
