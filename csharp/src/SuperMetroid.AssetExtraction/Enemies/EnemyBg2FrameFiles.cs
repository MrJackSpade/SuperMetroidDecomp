using SuperMetroid.Core;
using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Extracts the common $FFFE extended-enemy BG2 stream format. Only ordered
/// tilemap writes enter JSON; component hitbox pointers remain cartridge-derived
/// engine data and must never be interpreted as appearance overrides.
/// </summary>
internal static class EnemyBg2FrameFiles
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>
    /// Extracts ordered BG2 tile writes from each extended frame, skipping mixed OAM components when allowed; each
    /// cartridge tilemap entry is decoded once into the editable cell of the tile it places.
    /// </summary>
    internal static byte[] Extract(ISnesAddressSpace bus,
        ImmutableArray<EnemySpritemapDefinition> definitions, int version,
        int maximumComponents, string family, bool allowMixedOam = false)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, EnemyBg2WriteDocument[]>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition frame in definitions)
        {
            SnesAddress location = EnemyFrameCartridgeLookup.Of(frame.Frame);
            int bank = location.Bank;
            int root = (int)location;
            int componentCount = bus.ReadCartridgeByte(root);
            if (componentCount < 1 ||
                componentCount > maximumComponents ||
                componentCount > EnemyBg2FrameLayout.MaximumComponents ||
                bus.ReadCartridgeByte(root + 1) != 0)
                throw new InvalidDataException(
                    $"{family} frame {frame.Frame} at {location} has an invalid extended header.");
            var writes = new List<EnemyBg2WriteDocument>();
            for (int component = 0; component < componentCount; component++)
            {
                int record = root + 2 + component * 8;
                int stream = CartridgeImportSource.Require(bus).ReadCartridgeWord((record + 4));
                if (CartridgeImportSource.Require(bus).ReadCartridgeWord((((int)SnesAddress.InBank(bank, stream)))) !=
                    EnemySourceDefinitions.Bg2StreamMarker)
                {
                    if (allowMixedOam)
                        continue;
                    throw new InvalidDataException(
                        $"{family} frame {frame.Frame} at {location} component {component} is not a BG2 stream.");
                }
                // Mixed body roots store OAM-style offsets even on BG2
                // components. ProcessExtendedTilemap ignores both fields.
                if (!allowMixedOam &&
                    (CartridgeImportSource.Require(bus).ReadCartridgeWord(record) != 0 || CartridgeImportSource.Require(bus).ReadCartridgeWord((record + 2)) != 0))
                    throw new InvalidDataException(
                        $"{family} frame {frame.Frame} at {location} uses an unexpected BG2 component offset.");
                int cursor = unchecked((ushort)(stream + 2));
                bool terminated = false;
                for (int command = 0;
                     command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
                {
                    int address = ((int)SnesAddress.InBank(bank, cursor));
                    int destination = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
                    if (destination == CartridgeFormatDefinitions.ListTerminator)
                    {
                        terminated = true;
                        break;
                    }
                    int count = CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 2));
                    int relative = destination - EnemySourceDefinitions.Bg2WorkingTilemap;
                    if (relative < 0 || (relative & 1) != 0 ||
                        count is < 1 or > EnemyBg2FrameLayout.TilemapWidth)
                        throw new InvalidDataException(
                            $"{family} BG2 stream ${bank:X2}:{stream:X4} has an invalid tile run.");
                    int first = relative >> 1;
                    int x = first % EnemyBg2FrameLayout.TilemapWidth;
                    int y = first / EnemyBg2FrameLayout.TilemapWidth;
                    if (y >= EnemyBg2FrameLayout.TilemapHeight ||
                        x + count > EnemyBg2FrameLayout.TilemapWidth)
                        throw new InvalidDataException(
                            $"{family} BG2 stream ${bank:X2}:{stream:X4} crosses its tilemap row.");
                    var tiles = new RoomBackgroundTilemapCell[count];
                    for (int tile = 0; tile < count; tile++)
                        tiles[tile] = BackgroundTileCells.Of(CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 4 + tile * 2)));
                    writes.Add(new EnemyBg2WriteDocument
                    {
                        X = x,
                        Y = y,
                        Tiles = tiles,
                    });
                    cursor = unchecked((ushort)(cursor + 4 + count * 2));
                }
                if (!terminated)
                    throw new InvalidDataException(
                        $"{family} BG2 stream ${bank:X2}:{stream:X4} has no terminator.");
            }
            if (writes.Count == 0 || !frames.TryAdd(frame.Name, writes.ToArray()))
                throw new InvalidDataException($"Duplicate {family} BG2 frame {frame.Name}.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new EnemyBg2FrameDocument
        {
            Version = version,
            Frames = frames,
        }, CamelCaseIndentedJson);
        _ = EnemyBg2Frames.Load(new MemoryStream(json, writable: false),
            definitions, version, family);
        return json;
    }
}
