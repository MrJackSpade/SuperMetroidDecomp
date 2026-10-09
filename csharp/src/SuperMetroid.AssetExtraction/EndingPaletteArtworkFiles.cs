using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs native ending colors as independently replaceable RGB5 JSON files.</summary>
public static class EndingPaletteArtworkFiles
{
    /// <summary>Version of the ending palette stock manifest schema.</summary>
    private const int ManifestVersion = 2;

    /// <summary>Exports all seven ending palette roles to RGB5 JSON and a hashed manifest, verifying native color-byte round-trips and flattening logo fades in destination order.</summary>
    /// <param name="bus">Cartridge import source for palette words and the logo crossfade's pointer-selected backward copies.</param>
    /// <param name="directory">Destination directory, created if absent; palette files and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; installation loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">A native color has an unrepresentable high bit or palette JSON does not reproduce its native transfer bytes.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
        {
            int count = EndingPaletteDefinitions.ColorCount(id);
            byte[] native = ReadNativePalette(bus, id, count);
            var colors = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                ushort word = BinaryPrimitives.ReadUInt16LittleEndian(
                    native.AsSpan(index * sizeof(ushort)));
                if ((word & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Ending {id} palette color {index} has an unrepresentable high bit.");
                colors[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            using var json = new MemoryStream();
            EndingPalette.Write(json, id, new EndingPaletteDocument
            {
                Version = EndingPaletteDefinitions.Version,
                Colors = colors,
            });
            byte[] file = json.ToArray();
            EndingPalette roundTrip = EndingPalette.Load(
                new MemoryStream(file, writable: false), id);
            if (!roundTrip.Transfer.Span.SequenceEqual(native))
                throw new InvalidDataException($"Ending {id} palette JSON changed native colors.");
            string name = EndingPaletteDefinitions.FileName(id);
            using (var output = new FileStream(Path.Combine(directory, name),
                       FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }
        using var manifest = new FileStream(Path.Combine(directory,
            EndingPaletteDefinitions.ManifestFileName), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new Manifest(ManifestVersion, sourceCartridgeSha256, hashes), JsonOptions);
    }

    /// <summary>Checks the complete stock palette manifest and file digests, then compiles independently selected RGB5 palette files without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing all seven palette-role JSON files and their provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory with per-role palette replacements; each absent file retains verified stock.</param>
    /// <returns>The selected ending colors in native transfer order, with palette selection and animation timing unchanged.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance or coverage, a stock digest, or a selected palette's format, count, or RGB5 colors is invalid.</exception>
    public static EndingPaletteCatalog Load(string stockDirectory,
        string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory,
            EndingPaletteDefinitions.ManifestFileName);
        Manifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath),
                JsonOptions) ?? throw new InvalidDataException("Ending palette manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid ending palette manifest {manifestPath}.", error);
        }
        EndingPaletteId[] ids = Enum.GetValues<EndingPaletteId>();
        if (manifest.Version != ManifestVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.StockSha256 is null || manifest.StockSha256.Count != ids.Length ||
            ids.Any(id => !manifest.StockSha256.ContainsKey(EndingPaletteDefinitions.FileName(id))))
            throw new InvalidDataException(
                $"Ending palette manifest {manifestPath} does not describe this installation.");

        return new EndingPaletteCatalog(LoadOne(EndingPaletteId.Escape),
            LoadOne(EndingPaletteId.PostCredits), LoadOne(EndingPaletteId.Credits),
            LoadOne(EndingPaletteId.Explosion), LoadOne(EndingPaletteId.FinalGunship),
            LoadOne(EndingPaletteId.LogoInitial), LoadOne(EndingPaletteId.LogoCrossfade));

        EndingPalette LoadOne(EndingPaletteId id)
        {
            string name = EndingPaletteDefinitions.FileName(id);
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)),
                    manifest.StockSha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Stock ending palette {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            try
            {
                return EndingPalette.Load(new MemoryStream(selectedPath == stockPath
                    ? stock : File.ReadAllBytes(selectedPath), writable: false), id);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid ending palette {selectedPath}: {error.Message}", error);
            }
        }
    }

    /// <summary>Checks stock palette provenance, seven-role coverage and digests, and each role's RGB5 schema and color count; ignores overrides.</summary>
    /// <param name="directory">Installed stock ending palette directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>
    /// $8B:E58A copies each logo fade palette backwards from its selected $8C pointer.
    /// Flatten the native destination order as [step][BG or OBJ palette][color], keeping the
    /// pointer table and copy direction in code while making every color replaceable.
    /// </summary>
    internal static byte[] ReadNativePalette(ISnesAddressSpace bus, EndingPaletteId id,
        int count)
    {
        if (id != EndingPaletteId.LogoCrossfade)
            return RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                EndingPaletteDefinitions.SourceAddress(id), count * sizeof(ushort));

        var native = new byte[count * sizeof(ushort)];
        for (int step = 0; step < EndingLogoDefinitions.PaletteSteps; step++)
        for (int palette = 0; palette < 2; palette++)
        for (int color = 0; color < 16; color++)
        {
            int pointer = EndingLogoPalettePointerDefinitions.Source(step, palette);
            ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                0x8c0000 | (pointer - (15 - color) * sizeof(ushort)));
            int index = (step * 2 + palette) * 16 + color;
            BinaryPrimitives.WriteUInt16LittleEndian(
                native.AsSpan(index * sizeof(ushort)), word);
        }
        return native;
    }

    /// <summary>Camel-case JSON settings for RGB5 palette documents and their manifest.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Source provenance and stock digests for independently installed ending palettes.</summary>
    /// <param name="Version">Ending palette manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 of the cartridge revision used for extraction.</param>
    /// <param name="StockSha256">Palette JSON hashes keyed by their stable role filenames.</param>
    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> StockSha256);
}
