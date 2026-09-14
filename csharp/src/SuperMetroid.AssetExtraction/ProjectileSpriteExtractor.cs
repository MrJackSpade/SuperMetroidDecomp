using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts timed-projectile visual parts only; physical fields and instruction programs stay compiled.</summary>
public static class ProjectileSpriteExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        {
            int address = 0x930000 | id;
            int count = RomDataReader.ReadWordFixedBank(bus, address);
            if (count > ProjectileSpriteDefinitions.MaximumParts) throw new InvalidDataException($"Projectile sprite {id:X4} exceeds OAM capacity.");
            var parts = new SpriteVisualPart[count];
            for (int i = 0; i < count; i++)
            {
                int part = address + 2 + i * 5;
                var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(bus, part));
                var a = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, part + 3));
                parts[i] = new() { OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)bus.ReadByte(part + 2)),
                    TileColumn = a.TileNumber % ProjectileSpriteDefinitions.TileColumns, TileRow = a.TileNumber / ProjectileSpriteDefinitions.TileColumns,
                    Size = x.IsLarge ? 16 : 8, Palette = a.PaletteIndex, Priority = a.Priority, FlipX = a.FlipHorizontally, FlipY = a.FlipVertically };
            }
            frames.Add(ProjectileSpriteDefinitions.Name(id), parts);
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new ProjectileSpriteDocument { Version = ProjectileSpriteDefinitions.Version, Frames = frames },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
        _ = ProjectileSpriteCatalog.Load(new MemoryStream(json));
        return json;
    }
}
