using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Exports the named enemy frames' packed sprite parts to the import data (#1638), which only the import reads to compose
/// the enemy sheets' pictures, leaving instruction control in code.
/// </summary>
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
        foreach (EnemyFrameName definition in EnemySpritemapDefinitions.Frames)
        {
            SnesAddress location = EnemyFrameCartridgeLookup.Of(definition.Frame);
            SpriteVisualPart[] parts = ExtractParts(bus, location.Bank, location.Offset);
            if (!frames.TryAdd(definition.Name, parts))
                throw new InvalidDataException(
                    $"Duplicate extracted enemy composition {definition.Name}.");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
        {
            Version = EnemySpritemapSchema.Revision,
            Frames = frames,
            DisplayFrames = EnemyFrameBindingRules.Identity(EnemySpritemapDefinitions.Frames),
        }, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.World.Artwork.EnemySpritemapDocument>(CamelCaseIndentedJson));
        return json;
    }

    /// <summary>Decodes one ordinary OAM frame, including components of extended frames, into the import data's sprite parts.</summary>
    internal static SpriteVisualPart[] ExtractParts(
        ISnesAddressSpace bus, int bank, int pointer) =>
        DecodeParts(bus, bank, pointer, (x, y, attributes) => new SpriteVisualPart
        {
            OffsetX = x.SignedOffset,
            OffsetY = y.SignedOffset,
            TileColumn = attributes.TileNumber % EnemySpritemapCompiler.TileColumns,
            TileRow = attributes.TileNumber / EnemySpritemapCompiler.TileColumns,
            Size = x.IsLarge ? 16 : 8,
            Depth = attributes.Depth,
            Palette = attributes.PaletteIndex,
            FlipX = attributes.FlipHorizontally,
            FlipY = attributes.FlipVertically,
        });

    /// <summary>
    /// Decodes one enemy projectile's OAM frame into the parts of its composition document (#2563), each naming the
    /// character picture it draws by its place among the projectile characters.
    /// </summary>
    internal static EnemyProjectilePartDocument[] ExtractProjectileParts(ISnesAddressSpace bus, int bank, int pointer) =>
        DecodeParts(bus, bank, pointer, (x, y, attributes) => new EnemyProjectilePartDocument
        {
            OffsetX = x.SignedOffset,
            OffsetY = y.SignedOffset,
            Character = attributes.TileNumber,
            Size = x.IsLarge ? 16 : 8,
            Depth = attributes.Depth,
            Palette = attributes.PaletteIndex,
            FlipX = attributes.FlipHorizontally,
            FlipY = attributes.FlipVertically,
        });

    /// <summary>Decodes the OAM frame at <paramref name="pointer"/> of <paramref name="bank"/> into one document part per OAM part, which <paramref name="part"/> forms.</summary>
    private static T[] DecodeParts<T>(ISnesAddressSpace bus, int bank, int pointer,
        Func<SnesSpritemapXWord, SnesSpritemapYByte, SnesObjAttributeWord, T> part)
    {
        int count = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, pointer)));
        if (count > EnemySpritemapCompiler.MaximumParts)
            throw new InvalidDataException(
                $"Enemy spritemap ${bank:X2}:{pointer:X4} has {count} parts.");
        var parts = new T[count];
        for (int index = 0; index < count; index++)
        {
            int address = unchecked((ushort)(pointer + 2 + index * 5));
            var x = new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, address))));
            var y = new SnesSpritemapYByte(bus.ReadCartridgeByte(((int)SnesAddress.InBank(bank, unchecked((ushort)(address + 2))))));
            var attributes = new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(((int)SnesAddress.InBank(bank, unchecked((ushort)(address + 3))))));
            parts[index] = part(x, y, attributes);
        }
        return parts;
    }
}
