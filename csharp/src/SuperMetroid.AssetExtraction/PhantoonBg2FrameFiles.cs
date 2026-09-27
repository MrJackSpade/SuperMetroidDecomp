using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts Phantoon's extended-frame BG2 writes while deliberately omitting
/// its physical hitbox lists and native instruction/control-flow records.
/// </summary>
internal static class PhantoonBg2FrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, PhantoonBg2WriteDocument[]>(StringComparer.Ordinal);
        foreach (PhantoonBg2FrameDefinition frame in PhantoonBg2FrameDefinitions.Frames)
        {
            int root = (PhantoonBg2FrameDefinitions.Bank << 16) | frame.Pointer;
            int componentCount = bus.ReadByte(root);
            if (componentCount is < 1 or > 2 || bus.ReadByte(root + 1) != 0)
                throw new InvalidDataException(
                    $"Phantoon frame $A7:{frame.Pointer:X4} has an invalid extended header.");
            var writes = new List<PhantoonBg2WriteDocument>();
            for (int component = 0; component < componentCount; component++)
            {
                int record = root + 2 + component * 8;
                if (ReadWord(bus, record) != 0 || ReadWord(bus, record + 2) != 0)
                    throw new InvalidDataException(
                        $"Phantoon frame $A7:{frame.Pointer:X4} uses an unexpected BG2 component offset.");
                ushort stream = ReadWord(bus, record + 4);
                if (ReadWord(bus,
                        (PhantoonBg2FrameDefinitions.Bank << 16) | stream) !=
                    PhantoonBg2FrameDefinitions.StreamMarker)
                    throw new InvalidDataException(
                        $"Phantoon frame $A7:{frame.Pointer:X4} component {component} is not a BG2 stream.");
                ushort cursor = unchecked((ushort)(stream + 2));
                bool terminated = false;
                for (int command = 0; command < 128; command++)
                {
                    int address = (PhantoonBg2FrameDefinitions.Bank << 16) | cursor;
                    ushort destination = ReadWord(bus, address);
                    if (destination == 0xffff)
                    {
                        terminated = true;
                        break;
                    }
                    int count = ReadWord(bus, address + 2);
                    int relative = destination - PhantoonBg2FrameDefinitions.WorkingRamBase;
                    if (relative < 0 || (relative & 1) != 0 || count is < 1 or > 32)
                        throw new InvalidDataException(
                            $"Phantoon BG2 stream $A7:{stream:X4} has an invalid tile run.");
                    int first = relative >> 1;
                    int x = first % PhantoonBg2FrameDefinitions.TilemapWidth;
                    int y = first / PhantoonBg2FrameDefinitions.TilemapWidth;
                    if (y >= PhantoonBg2FrameDefinitions.TilemapHeight ||
                        x + count > PhantoonBg2FrameDefinitions.TilemapWidth)
                        throw new InvalidDataException(
                            $"Phantoon BG2 stream $A7:{stream:X4} crosses its tilemap row.");
                    var tiles = new int[count];
                    for (int tile = 0; tile < count; tile++)
                        tiles[tile] = ReadWord(bus, address + 4 + tile * 2);
                    writes.Add(new PhantoonBg2WriteDocument
                    {
                        X = x,
                        Y = y,
                        Tiles = tiles,
                    });
                    cursor = unchecked((ushort)(cursor + 4 + count * 2));
                }
                if (!terminated)
                    throw new InvalidDataException(
                        $"Phantoon BG2 stream $A7:{stream:X4} has no terminator.");
            }
            frames.Add(frame.Name, writes.ToArray());
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new PhantoonBg2FrameDocument
        {
            Version = PhantoonBg2FrameDefinitions.Version,
            Frames = frames,
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        });
        _ = PhantoonBg2FrameCatalog.Load(new MemoryStream(json, writable: false));
        return json;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
}
