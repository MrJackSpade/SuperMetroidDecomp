using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Development-tool reads of native enemy headers; never linked by player hosts.</summary>
internal static class RoomEnemyDefinitionImporterTooling
{
    /// <summary>Header offset of the bank byte that qualifies the header's code and palette words.</summary>
    private const int BankOffset = 12;

    /// <summary>The bank of the enemy code and palette a header names.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">Bank-$A0 pointer to the native enemy definition.</param>
    internal static byte Bank(ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        Ensure.NotNull(bus);
        return CartridgeImportSource.Require(bus).ReadCartridgeByte(
            SuperMetroid.AssetExtraction.CartridgeTables.EnemySourceDefinitions.EnemyHeaderBank | unchecked((ushort)(EnemyDefinitionIds.HeaderPointer(pointer) + BankOffset)));
    }
}
