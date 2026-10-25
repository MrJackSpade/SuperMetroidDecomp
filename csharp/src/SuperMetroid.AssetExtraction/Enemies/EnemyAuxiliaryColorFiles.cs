using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Import-only conversion of the four bounded enemy color animations.</summary>
[CpuBusBoundary("codec: ROM enemy auxiliary color animation tables → EnemyAuxiliaryColorCatalog")]
public static class EnemyAuxiliaryColorFiles
{
    /// <summary>Imports the face-block glow, drained sidehopper, and Golden Torizo body and belly palette rows.</summary>
    /// <param name="source">Cartridge-import-capable address space supplying the four native color tables.</param>
    /// <returns>New UTF-8 JSON bytes containing the fixed row dimensions with red, green, and blue channels masked to 0..31.</returns>
    /// <remarks>Reads each family's native row stride, including the sidehopper's fifteen supplied colors per sixteen-word row. Runtime AI retains frame selection and cadence.</remarks>
    public static byte[] Extract(ISnesAddressSpace source)
    {
        var palettes = new Dictionary<EnemyAuxiliaryPalette, PaletteRgb5[][]>();
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        foreach (EnemyAuxiliaryPaletteDefinition definition in EnemyAuxiliaryColorDefinitions.All)
        {
            EnemyAuxiliaryPaletteSource stored = EnemySourceDefinitions.AuxiliaryPalettes[definition.Id];
            var rows = new PaletteRgb5[definition.FrameCount][];
            for (int frame = 0; frame < rows.Length; frame++)
            {
                rows[frame] = new PaletteRgb5[definition.ColorCount];
                for (int color = 0; color < rows[frame].Length; color++)
                {
                    ushort word = RomDataReader.ReadWordFixedBank(cartridge, stored.FirstRow.Value +
                        (frame * stored.RowStrideColors + color) * sizeof(ushort));
                    rows[frame][color] = new PaletteRgb5
                    {
                        Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31,
                    };
                }
            }
            palettes.Add(definition.Id, rows);
        }
        return EnemyAuxiliaryColorFiles.Write(new EnemyAuxiliaryColorDocument
        {
            Version = EnemyAuxiliaryColorFormat.Version, Palettes = palettes,
        });
    }

    /// <summary>Loads all four named palette families at the supported version, validating each definition's row/color counts and RGB5 channels 0..31 while rejecting duplicate or unknown properties; compiles owned rows independently of cartridge storage.</summary>
    /// <param name="source">UTF-8 JSON stream containing enum-named palette families; integer enum identities are not accepted.</param>
    /// <returns>The immutable auxiliary-color catalog, retaining supplied rows when they differ from calculated stock relationships.</returns>
    public static EnemyAuxiliaryColorCatalog Read(Stream source)
    {
        Ensure.NotNull(source);
        EnemyAuxiliaryColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(source);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyAuxiliaryColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Enemy auxiliary colors are null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid enemy auxiliary color JSON.", error); }
        if (document.Version != EnemyAuxiliaryColorFormat.Version || document.Palettes is null ||
            document.Palettes.Count != EnemyAuxiliaryColorDefinitions.All.Length)
            throw new InvalidDataException("Enemy auxiliary colors require version one and all four named palettes.");
        var result = new Dictionary<EnemyAuxiliaryPalette, Bgr555[][]>();
        foreach (EnemyAuxiliaryPaletteDefinition definition in EnemyAuxiliaryColorDefinitions.All)
        {
            if (!document.Palettes.TryGetValue(definition.Id, out PaletteRgb5[][]? rows) ||
                rows is null || rows.Length != definition.FrameCount)
                throw new InvalidDataException($"Palette {definition.Id} requires {definition.FrameCount} frames.");
            var compiled = new Bgr555[rows.Length][];
            for (int frame = 0; frame < rows.Length; frame++)
            {
                PaletteRgb5[]? row = rows[frame];
                if (row is null || row.Length != definition.ColorCount)
                    throw new InvalidDataException($"Palette {definition.Id} frame {frame} requires {definition.ColorCount} colors.");
                compiled[frame] = new Bgr555[row.Length];
                for (int color = 0; color < row.Length; color++)
                {
                    PaletteRgb5? rgb = row[color];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                        throw new InvalidDataException($"Palette {definition.Id} frame {frame} color {color} requires RGB5 channels 0..31.");
                    compiled[frame][color] = rgb.ToBgr555();
                }
            }
            result.Add(definition.Id, compiled);
        }
        return new EnemyAuxiliaryColorCatalog(result);
    }

    /// <summary>Serializes the auxiliary-color document as indented camel-case UTF-8 JSON with named palette identities and validates it through <see cref="Read"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and every palette family's complete RGB5 rows.</param>
    /// <returns>Validated auxiliary-color JSON bytes.</returns>
    public static byte[] Write(EnemyAuxiliaryColorDocument document)
    {
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Read(new MemoryStream(data, writable: false));
        return data;
    }

    /// <summary>Rejects case-insensitive duplicate JSON keys before deserialization can discard an earlier color edit.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.OrdinalIgnoreCase,
            name => new InvalidDataException($"Duplicate auxiliary palette property {name}."));

    /// <summary>Uses strict camel-case JSON with named palette identities and no unmapped properties.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter<EnemyAuxiliaryPalette>(allowIntegerValues: false) },
    };
}
