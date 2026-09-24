using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports named enemy visual frames, leaving instruction control in code.</summary>
internal static class EnemySpritemapFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition definition in EnemySpritemapDefinitions.Frames)
        {
            SpriteVisualPart[] parts = ExtractParts(bus, definition.Bank,
                definition.Pointer);
            if (!frames.TryAdd(definition.Name, parts))
                throw new InvalidDataException(
                    $"Duplicate extracted enemy composition {definition.Name}.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.Version,
            Frames = frames,
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        });
        _ = EnemySpritemapCatalog.Load(new MemoryStream(json, writable: false));
        return json;
    }

    /// <summary>Decodes one ordinary OAM frame, including components of extended frames.</summary>
    internal static SpriteVisualPart[] ExtractParts(
        ISnesAddressSpace bus, byte bank, ushort pointer)
    {
        int count = ReadWord(bus, bank, pointer);
        if (count > EnemySpritemapDefinitions.MaximumParts)
            throw new InvalidDataException(
                $"Enemy spritemap ${bank:X2}:{pointer:X4} has {count} parts.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            ushort address = unchecked((ushort)(pointer + 2 + index * 5));
            var x = new SnesSpritemapXWord(ReadWord(bus, bank, address));
            byte y = bus.ReadByte((bank << 16) |
                unchecked((ushort)(address + 2)));
            var attributes = new SnesObjAttributeWord(ReadWord(bus,
                bank, unchecked((ushort)(address + 3))));
            parts[index] = new SpriteVisualPart
            {
                OffsetX = x.SignedOffset,
                OffsetY = unchecked((sbyte)y),
                TileColumn = attributes.TileNumber % EnemySpritemapDefinitions.TileColumns,
                TileRow = attributes.TileNumber / EnemySpritemapDefinitions.TileColumns,
                Size = x.IsLarge ? 16 : 8,
                Priority = attributes.Priority,
                Palette = attributes.PaletteIndex,
                FlipX = attributes.FlipHorizontally,
                FlipY = attributes.FlipVertically,
            };
        }
        return parts;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, byte bank, ushort address) =>
        (ushort)(bus.ReadByte((bank << 16) | address) |
            bus.ReadByte((bank << 16) | unchecked((ushort)(address + 1))) << 8);
}
