using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts timed-projectile visual parts only; physical fields and instruction programs stay compiled.</summary>
public static class ProjectileSpriteExtractor
{
    /// <summary>Decodes all catalogued bank-$93 Samus-projectile OAM compositions and validates the resulting visual catalog.</summary>
    /// <param name="bus">Cartridge-import-capable address space containing the native projectile spritemaps.</param>
    /// <returns>New UTF-8 JSON bytes containing named frames with signed pixel offsets, 8- or 16-pixel part sizes, tile references, palettes, priorities, and flips.</returns>
    /// <remarks>Physical projectile fields, animation timing, and instruction programs are not exported.</remarks>
    /// <exception cref="InvalidDataException">A native frame exceeds the OAM part limit or its extracted composition fails catalog validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] json = ExtractFrames(bus, default, useProjectilePointers: true);
        _ = ProjectileSpriteCatalog.Load(new MemoryStream(json));
        return json;
    }

    /// <summary>Decodes the requested bank-$93 spritemaps into a versioned visual-part document.</summary>
    /// <param name="bus">Import address space containing the native spritemaps.</param>
    /// <param name="requiredPointers">Spritemap pointers to decode when the canonical projectile list is not requested.</param>
    /// <param name="useProjectilePointers">Whether to ignore <paramref name="requiredPointers"/> and use the complete compiled projectile pointer catalog.</param>
    /// <returns>The UTF-8 sprite-frame JSON document.</returns>
    internal static byte[] ExtractFrames(ISnesAddressSpace bus, ReadOnlySpan<ushort> requiredPointers, bool useProjectilePointers = false)
    {
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        int requiredCount = useProjectilePointers ? ProjectileSpriteDefinitions.NativePointers.Length : requiredPointers.Length;
        for (int pointerIndex = 0; pointerIndex < requiredCount; pointerIndex++)
        {
            ushort id = useProjectilePointers ? ProjectileSpriteDefinitions.NativePointers[pointerIndex] : requiredPointers[pointerIndex];
            int address = 0x930000 | id;
            int count = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            if (count > ProjectileSpriteDefinitions.MaximumParts) throw new InvalidDataException($"Projectile sprite {id:X4} exceeds OAM capacity.");
            var parts = new SpriteVisualPart[count];
            for (int i = 0; i < count; i++)
            {
                int part = address + 2 + i * 5;
                var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), part));
                var a = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), part + 3));
                parts[i] = new() { OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)bus.ReadCartridgeByte(part + 2)),
                    TileColumn = a.TileNumber % ProjectileSpriteDefinitions.TileColumns, TileRow = a.TileNumber / ProjectileSpriteDefinitions.TileColumns,
                    Size = x.IsLarge ? 16 : 8, Palette = a.PaletteIndex, Priority = a.Priority, FlipX = a.FlipHorizontally, FlipY = a.FlipVertically };
            }
            frames.Add(ProjectileSpriteDefinitions.Name(id), parts);
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new ProjectileSpriteDocument { Version = ProjectileSpriteDefinitions.Version, Frames = frames },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
        return json;
    }
}
