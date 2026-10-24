using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts timed-projectile visual parts only; physical fields and instruction programs stay compiled.</summary>
[CpuBusBoundary("codec: ROM bank-$93 projectile spritemaps → projectile sprite frames")]
public static class ProjectileSpriteExtractor
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Decodes all catalogued bank-$93 Samus-projectile OAM compositions and validates the resulting visual catalog.</summary>
    /// <param name="bus">Cartridge-import-capable address space containing the native projectile spritemaps.</param>
    /// <returns>New UTF-8 JSON bytes containing named frames with signed pixel offsets, 8- or 16-pixel part sizes, tile references, palettes, priorities, and flips.</returns>
    /// <remarks>Physical projectile fields, animation timing, and instruction programs are not exported.</remarks>
    /// <exception cref="InvalidDataException">A native frame exceeds the OAM part limit or its extracted composition fails catalog validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] json = ExtractFrames(bus, ProjectileSpriteDefinitions.All, ProjectileSpriteLocations.Projectiles());
        _ = ProjectileSpriteCatalog.Load(new MemoryStream(json));
        return json;
    }

    /// <summary>Decodes the <paramref name="required"/> compositions, each from the bank-$93 record <paramref name="locations"/> places it at, into named OAM parts.</summary>
    internal static byte[] ExtractFrames(ISnesAddressSpace bus, IReadOnlyList<ProjectileSpriteId> required,
        IReadOnlyDictionary<ProjectileSpriteId, ushort> locations)
    {
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (ProjectileSpriteId id in required)
        {
            int address = CartridgeBankWindow.At(SamusSourceDefinitions.ProjectileBank, locations[id]);
            int count = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
            if (count > ProjectileSpriteDefinitions.MaximumParts) throw new InvalidDataException($"Projectile sprite {id.Name} exceeds OAM capacity.");
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
            frames.Add(id.Name, parts);
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new ProjectileSpriteDocument { Version = ProjectileSpriteDefinitions.Version, Frames = frames },
            CamelCaseIndentedJson);
        return json;
    }
}
