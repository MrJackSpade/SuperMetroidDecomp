using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs native ending colors as independently replaceable RGB5 JSON files.</summary>
public static class EndingPaletteArtworkFiles
{
    private const int ManifestVersion = 1;

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
            byte[] native = RomDataReader.ReadFixedBank(bus,
                EndingPaletteDefinitions.SourceAddress(id), count * sizeof(ushort));
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
            LoadOne(EndingPaletteId.Explosion), LoadOne(EndingPaletteId.FinalGunship));

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

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, string> StockSha256);
}
