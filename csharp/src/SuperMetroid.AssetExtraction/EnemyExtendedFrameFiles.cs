using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts walking/wall/ninja-Pirate visual components without exporting their
/// native hitbox pointers or any instruction/callback words.
/// </summary>
internal static class EnemyExtendedFrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, EnemyExtendedVisualComponent[]>(
            StringComparer.Ordinal);
        foreach (EnemyExtendedFrameDefinition definition in
                 EnemyExtendedFrameDefinitions.Frames)
        {
            byte bank = definition.Bank;
            ushort pointer = definition.Pointer;
            int baseAddress = (bank << 16) | pointer;
            int count = bus.ReadByte(baseAddress);
            byte padding = bus.ReadByte((bank << 16) |
                unchecked((ushort)(pointer + 1)));
            if (count is < 1 or > EnemyExtendedFrameDefinitions.MaximumComponents ||
                padding != 0)
                throw new InvalidDataException(
                    $"Extended frame ${bank:X2}:{pointer:X4} has invalid component header.");
            var components = new EnemyExtendedVisualComponent[count];
            for (int index = 0; index < count; index++)
            {
                ushort record = unchecked((ushort)(pointer + 2 + index * 8));
                short x = unchecked((short)ReadWord(bus, bank, record));
                short y = unchecked((short)ReadWord(bus, bank,
                    unchecked((ushort)(record + 2))));
                ushort spritePointer = ReadWord(bus, bank,
                    unchecked((ushort)(record + 4)));
                if (ReadWord(bus, bank, spritePointer) == 0xfffe)
                    throw new InvalidDataException(
                        $"Extended frame ${bank:X2}:{pointer:X4} contains a BG2 command, not OAM.");
                components[index] = new EnemyExtendedVisualComponent
                {
                    OffsetX = x,
                    OffsetY = y,
                    Parts = EnemySpritemapFiles.ExtractParts(bus, bank,
                        spritePointer),
                };
            }
            if (!frames.TryAdd(definition.Name, components))
                throw new InvalidDataException(
                    $"Duplicate extended enemy frame {definition.Name}.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyExtendedFrameDocument
            {
                Version = EnemyExtendedFrameDefinitions.Version,
                Frames = frames,
                DisplayFrames = EnemyExtendedFrameDefinitions.Frames.ToArray()
                    .ToDictionary(frame => frame.Name, frame => frame.Name,
                        StringComparer.Ordinal),
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            });
        _ = EnemyExtendedFrameCatalog.Load(new MemoryStream(json, writable: false));
        return json;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, byte bank, ushort address) =>
        (ushort)(bus.ReadByte((bank << 16) | address) |
            bus.ReadByte((bank << 16) | unchecked((ushort)(address + 1))) << 8);
}
