using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Exports named enemy visual frames, leaving instruction control in code.</summary>
internal static class EnemySpritemapFiles
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Exports each named enemy spritemap and validates the resulting JSON
    /// against the runtime catalog before returning the asset bytes.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition definition in EnemySpritemapDefinitions.Frames)
        {
            SnesAddress location = EnemyFrameCartridgeLookup.Of(definition.Frame);
            SpriteVisualPart[] parts = ExtractParts(bus, location.Bank, location.Offset);
            if (!frames.TryAdd(definition.Name, parts))
                throw new InvalidDataException(
                    $"Duplicate extracted enemy composition {definition.Name}.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
        {
            Version = EnemySpritemapSchema.Current.Revision,
            Frames = frames,
            DisplayFrames = EnemySpritemapDefinitions.Frames
                .ToDictionary(frame => frame.Name, frame => frame.Name,
                    StringComparer.Ordinal),
        }, CamelCaseIndentedJson);
        _ = EnemySpritemapDocuments.Load(new MemoryStream(json, writable: false));
        return json;
    }

    /// <summary>Decodes one ordinary OAM frame, including components of extended frames.</summary>
    internal static SpriteVisualPart[] ExtractParts(
        ISnesAddressSpace bus, int bank, int pointer)
    {
        int count = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, pointer)));
        if (count > EnemySpritemapCompiler.MaximumParts)
            throw new InvalidDataException(
                $"Enemy spritemap ${bank:X2}:{pointer:X4} has {count} parts.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int address = unchecked((ushort)(pointer + 2 + index * 5));
            var x = new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, address))));
            var y = new SnesSpritemapYByte(bus.ReadCartridgeByte(((int)SnesAddress.InBank(bank, unchecked((ushort)(address + 2))))));
            var attributes = new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, unchecked((ushort)(address + 3))))));
            parts[index] = new SpriteVisualPart
            {
                OffsetX = x.SignedOffset,
                OffsetY = y.SignedOffset,
                TileColumn = attributes.TileNumber % EnemySpritemapCompiler.TileColumns,
                TileRow = attributes.TileNumber / EnemySpritemapCompiler.TileColumns,
                Size = x.IsLarge ? 16 : 8,
                Priority = attributes.Priority,
                Palette = attributes.PaletteIndex,
                FlipX = attributes.FlipHorizontally,
                FlipY = attributes.FlipVertically,
            };
        }
        return parts;
    }
}
