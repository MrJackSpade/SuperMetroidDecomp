using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One ordinary enemy's required installation identity and complete DMA source.</summary>
public readonly record struct EnemyTileSourceDefinition(ushort DefinitionPointer, int SourceAddress, int ByteCount);

/// <summary>Immutable required tile/color identities from all compiled retail graphics sets.</summary>
public static class EnemyTileSourceDefinitions
{
    /// <summary>Every distinct bank-$A0 graphics-set definition, with its native source and masked byte length.</summary>
    public static IReadOnlyList<EnemyTileSourceDefinition> All { get; } = Build();

    private static System.Collections.ObjectModel.ReadOnlyCollection<EnemyTileSourceDefinition> Build()
    {
        EnemyTileSourceDefinition[] definitions = RoomEnemyGraphicsSetDefinitions.Pointers
            .SelectMany(pointer => RoomEnemyGraphicsSetDefinitions.Get(pointer).Records.ToArray())
            .Select(record => record.DefinitionPointer).Distinct().Order()
            .Select(pointer => {
                RoomEnemyDefinition header = RoomEnemyDefinitionCatalog.Get(pointer);
                return new EnemyTileSourceDefinition(pointer, header.TileDataAddress,
                    header.TileDataSize & EnemyTileArtworkFormat.TileByteCountMask);
            }).ToArray();
        string keys = string.Join(",", definitions.Select(definition => $"{definition.DefinitionPointer:X4}"));
        if (definitions.Length != EnemyTileArtworkFormat.RetailDefinitionCount ||
            Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(keys))) != EnemyTileArtworkFormat.RetailDefinitionIdsSha256)
            throw new InvalidDataException("Compiled enemy artwork sources differ from the pinned installation identities.");
        return Array.AsReadOnly(definitions);
    }
}
