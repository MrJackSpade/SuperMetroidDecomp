using System.Security.Cryptography;
using System.Text;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Immutable required tile/color identities from all compiled retail graphics sets.</summary>
public static class EnemyTileSourceDefinitions
{
    /// <summary>Every distinct bank-$A0 graphics-set definition, with its native source and masked byte length.</summary>
    public static IReadOnlyList<EnemyTileSourceDefinition> All { get; } = Build();

    /// <summary>Deduplicates graphics-set enemy definitions, extracts masked tile lengths, and verifies the pinned retail identity set.</summary>
    private static System.Collections.ObjectModel.ReadOnlyCollection<EnemyTileSourceDefinition> Build()
    {
        EnemyTileSourceDefinition[] definitions = RoomEnemyGraphicsSetDefinitions.All
            .SelectMany(set => set.Records.ToArray())
            .Select(record => record.DefinitionId).Distinct().Order()
            .Select(pointer => {
                RoomEnemyDefinition header = RoomEnemyDefinitionCatalog.Get(pointer);
                return new EnemyTileSourceDefinition(pointer, header.TileDataAddress,
                    header.TileDataSize & EnemyTileArtworkFormat.TileByteCountMask);
            }).ToArray();
        string keys = string.Join(",", definitions.Select(definition => $"{(int)definition.DefinitionId:X4}"));
        if (definitions.Length != EnemyTileArtworkFormat.RetailDefinitionCount ||
            Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(keys))) != EnemyTileArtworkFormat.RetailDefinitionIdsSha256)
            throw new InvalidDataException("Compiled enemy artwork sources differ from the pinned installation identities.");
        return Array.AsReadOnly(definitions);
    }
}
