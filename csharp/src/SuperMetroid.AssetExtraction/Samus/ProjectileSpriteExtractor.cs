using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts timed-projectile visual parts only; physical fields and instruction programs stay compiled.</summary>
public static class ProjectileSpriteExtractor
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>
    /// Decodes all catalogued bank-$93 Samus-projectile OAM compositions, each once: a record the cartridge stores again
    /// holds the composition it copies, which it must store byte for byte (#2295).
    /// </summary>
    /// <param name="bus">Cartridge-import-capable address space containing the native projectile spritemaps.</param>
    /// <returns>New UTF-8 JSON bytes containing named frames with signed pixel offsets, 8- or 16-pixel part sizes, tile references, palettes, priorities, and flips.</returns>
    /// <remarks>Physical projectile fields, animation timing, and instruction programs are not exported.</remarks>
    /// <exception cref="InvalidDataException">
    /// A native frame exceeds the OAM part limit, a record held as another composition's copy stores other bytes, or two
    /// compositions store the same bytes.
    /// </exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Dictionary<ProjectileSpriteId, ushort> first = ProjectileSpriteLocations.Projectiles();
        foreach ((int address, ProjectileSpriteId sprite) in ProjectileSpriteLocations.StoredProjectiles())
        {
            if (!RecordBytes(bus, Bank(address)).SequenceEqual(RecordBytes(bus, Bank(first[sprite]))))
                throw new InvalidDataException($"The projectile composition record at $93:{address:X4} is held as {sprite.Name}, " +
                    $"stored at $93:{first[sprite]:X4}, but the two records differ: a record is one composition's copy only when it " +
                    "stores it byte for byte (#2295).");
        }
        return ExtractFrames(bus, ProjectileSpriteDefinitions.All, first);
    }

    /// <summary>The bus address of bank-$93 composition record <paramref name="address"/>.</summary>
    private static SnesAddress Bank(int address) =>
        SnesAddress.FromBusAddress(CartridgeBankWindow.At(SamusSourceDefinitions.ProjectileBank, address));

    /// <summary>The bytes of the composition record at <paramref name="address"/>: its part count, then five bytes per part.</summary>
    private static byte[] RecordBytes(ISnesAddressSpace bus, SnesAddress address)
    {
        int start = (int)address;
        int count = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start);
        if (count > ProjectileSpriteDefinitions.MaximumParts) throw new InvalidDataException($"The projectile composition record at {start:X6} exceeds OAM capacity.");
        return [.. Enumerable.Range(start, sizeof(ushort) + 5 * count).Select(bus.ReadCartridgeByte)];
    }

    /// <summary>Decodes the <paramref name="required"/> compositions, each from the bank-$93 record <paramref name="locations"/> places it at, into named OAM parts.</summary>
    internal static byte[] ExtractFrames(ISnesAddressSpace bus, IReadOnlyList<ProjectileSpriteId> required,
        IReadOnlyDictionary<ProjectileSpriteId, ushort> locations)
    {
        Ensure.NotNull(locations);
        return ExtractFrames(bus, required, locations.ToDictionary(entry => entry.Key, entry => Bank(entry.Value)));
    }

    /// <summary>
    /// Decodes the <paramref name="required"/> compositions, each from the record <paramref name="locations"/> places it at,
    /// into named OAM parts, writing each composition once (#1635): two compositions whose records store the same bytes are
    /// one picture.
    /// </summary>
    /// <exception cref="InvalidDataException">A record exceeds the OAM part limit, or two records store the same bytes.</exception>
    internal static byte[] ExtractFrames(ISnesAddressSpace bus, IReadOnlyList<ProjectileSpriteId> required,
        IReadOnlyDictionary<ProjectileSpriteId, SnesAddress> locations)
    {
        Ensure.NotNull(locations);
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        var written = new Dictionary<string, ProjectileSpriteId>(StringComparer.Ordinal);
        foreach (ProjectileSpriteId id in required)
        {
            int address = (int)locations[id];
            string bytes = Convert.ToHexString(RecordBytes(bus, locations[id]));
            if (!written.TryAdd(bytes, id))
                throw new InvalidDataException($"Projectile compositions {written[bytes].Name} at {(int)locations[written[bytes]]:X6} and {id.Name} " +
                    $"at {address:X6} store the same parts, byte for byte, and both were about to be written. A composition is " +
                    "written once (#1635, #2295): a record storing one again holds the composition it copies.");
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
