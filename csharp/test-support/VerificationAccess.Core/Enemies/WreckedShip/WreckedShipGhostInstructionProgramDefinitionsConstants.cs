
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Cartridge values of <see cref="WreckedShipGhostInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class WreckedShipGhostInstructionProgramDefinitionsConstants
{
    /// <summary>The first non-program word after <c>InstList_Coven</c>, at $A8:9A9C.</summary>
    public const ushort FirstAdjacentConstant = 0x9a9c;

    /// <summary>Provides the first neighboring constant that bounds Wrecked Ship ghost instruction extraction.</summary>
    extension(WreckedShipGhostInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="WreckedShipGhostInstructionProgramDefinitionsConstants.FirstAdjacentConstant"/>
        internal static ushort FirstAdjacentConstant => WreckedShipGhostInstructionProgramDefinitionsConstants.FirstAdjacentConstant;
    }
}
