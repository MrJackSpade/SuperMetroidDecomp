using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts the common $FFFE extended-enemy BG2 stream format. Only ordered
/// tilemap writes enter JSON; component hitbox pointers remain cartridge-derived
/// engine data and must never be interpreted as appearance overrides.
/// </summary>
internal static class EnemyBg2FrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus, byte bank,
        ReadOnlySpan<EnemyBg2FrameDefinition> definitions, int version,
        int maximumComponents, string family, bool allowMixedOam = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, EnemyBg2WriteDocument[]>(StringComparer.Ordinal);
        foreach (EnemyBg2FrameDefinition frame in definitions)
        {
            int root = (bank << 16) | frame.Pointer;
            int componentCount = bus.ReadCartridgeByte(root);
            if (componentCount < 1 ||
                componentCount > maximumComponents ||
                componentCount > EnemyBg2FrameLayout.MaximumComponents ||
                bus.ReadCartridgeByte(root + 1) != 0)
                throw new InvalidDataException(
                    $"{family} frame ${bank:X2}:{frame.Pointer:X4} has an invalid extended header.");
            var writes = new List<EnemyBg2WriteDocument>();
            for (int component = 0; component < componentCount; component++)
            {
                int record = root + 2 + component * 8;
                ushort stream = ReadWord(bus, record + 4);
                if (ReadWord(bus, (bank << 16) | stream) !=
                    EnemyBg2FrameLayout.StreamMarker)
                {
                    if (allowMixedOam)
                        continue;
                    throw new InvalidDataException(
                        $"{family} frame ${bank:X2}:{frame.Pointer:X4} component {component} is not a BG2 stream.");
                }
                // Crocomire stores nonzero OAM-style offsets even on BG2
                // components. ProcessExtendedTilemap ignores both fields.
                if (!allowMixedOam &&
                    (ReadWord(bus, record) != 0 || ReadWord(bus, record + 2) != 0))
                    throw new InvalidDataException(
                        $"{family} frame ${bank:X2}:{frame.Pointer:X4} uses an unexpected BG2 component offset.");
                ushort cursor = unchecked((ushort)(stream + 2));
                bool terminated = false;
                for (int command = 0;
                     command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
                {
                    int address = (bank << 16) | cursor;
                    ushort destination = ReadWord(bus, address);
                    if (destination == 0xffff)
                    {
                        terminated = true;
                        break;
                    }
                    int count = ReadWord(bus, address + 2);
                    int relative = destination - EnemyBg2FrameLayout.WorkingRamBase;
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
                    var tiles = new int[count];
                    for (int tile = 0; tile < count; tile++)
                        tiles[tile] = ReadWord(bus, address + 4 + tile * 2);
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
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        });
        _ = EnemyBg2FrameCatalog.Load(new MemoryStream(json, writable: false),
            definitions, version, family);
        return json;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);
}
