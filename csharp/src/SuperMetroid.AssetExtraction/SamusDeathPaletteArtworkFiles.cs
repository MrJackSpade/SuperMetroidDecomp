using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the authored bank-$9B fatal-damage color and color-selection data.</summary>
public static class SamusDeathPaletteArtworkFiles
{
    public const string ArtworkFileName = "samus-death-palettes.json";
    public const string ManifestFileName = "samus-death-palettes-manifest.json";
    private const int FormatVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        var suited = new ushort[SamusDeathPaletteArtworkCatalog.SuitCount][][];
        for (int suit = 0; suit < suited.Length; suit++)
        {
            suited[suit] = new ushort[SamusPaletteRomData.Death.PaletteCount][];
            for (int palette = 0; palette < suited[suit].Length; palette++)
            {
                ushort pointer = ReadWord(bus, SamusPaletteRomData.Death.SuitPointers +
                    suit * SamusPaletteRomData.Death.PaletteCount * sizeof(ushort) +
                    palette * sizeof(ushort));
                suited[suit][palette] = ReadPalette(bus, pointer);
            }
        }
        var suitless = new ushort[SamusPaletteRomData.Death.PaletteCount][];
        for (int palette = 0; palette < suitless.Length; palette++)
        {
            ushort pointer = ReadWord(bus,
                SamusPaletteRomData.Death.SuitlessPointers + palette * sizeof(ushort));
            suitless[palette] = ReadPalette(bus, pointer);
        }
        var whiteout = new ushort[SamusPaletteRomData.Death.WhiteoutShadeCount];
        for (int shade = 0; shade < whiteout.Length; shade++)
            whiteout[shade] = ReadWord(bus,
                SamusPaletteRomData.Death.WhiteoutShades + shade * sizeof(ushort));
        var selectors = new ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        for (int frame = 0; frame < selectors.Length; frame++)
            selectors[frame] = bus.ReadByte(
                SamusPaletteRomData.Death.ExplosionTimingAndPaletteIndices + frame * 2 + 1);
        var document = new ArtworkDocument(FormatVersion, suited, suitless, whiteout, selectors);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, ArtworkFileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(FormatVersion,
                sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(bytes))), JsonOptions));
        _ = Load(directory, null);
    }

    public static SamusDeathPaletteArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArtworkManifest manifest = Read<ArtworkManifest>(
            File.ReadAllBytes(Path.Combine(stockDirectory, ManifestFileName)));
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Samus death-palette manifest does not match the pinned cartridge.");
        byte[] stock = File.ReadAllBytes(Path.Combine(stockDirectory, ArtworkFileName));
        if (!string.Equals(manifest.ArtworkSha256, Convert.ToHexString(SHA256.HashData(stock)),
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Stock Samus death-palette artwork failed its manifest hash.");
        _ = Build(Read<ArtworkDocument>(stock));
        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, ArtworkFileName);
        return Build(Read<ArtworkDocument>(overridePath is not null && File.Exists(overridePath)
            ? File.ReadAllBytes(overridePath) : stock));
    }

    private static SamusDeathPaletteArtworkCatalog Build(ArtworkDocument document)
    {
        if (document.Version != FormatVersion || document.Suited is null ||
            document.Suitless is null || document.Whiteout is null ||
            document.ExplosionPaletteIndices is null)
            throw new InvalidDataException("Samus death-palette artwork has an invalid structure.");
        return new SamusDeathPaletteArtworkCatalog(document.Suited, document.Suitless,
            document.Whiteout, document.ExplosionPaletteIndices);
    }

    private static ushort[] ReadPalette(ISnesAddressSpace bus, ushort pointer)
    {
        var colors = new ushort[SamusDeathPaletteArtworkCatalog.ColorCount];
        for (int color = 0; color < colors.Length; color++)
            colors[color] = ReadWord(bus, SamusPaletteRomData.Banks.Palette |
                unchecked((ushort)(pointer + color * sizeof(ushort))));
        return colors;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static T Read<T>(byte[] bytes)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions) ??
                throw new InvalidDataException($"Samus death {typeof(T).Name} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid Samus death {typeof(T).Name}.", error);
        }
    }

    private sealed record ArtworkDocument(int Version, ushort[][][] Suited,
        ushort[][] Suitless, ushort[] Whiteout, ushort[] ExplosionPaletteIndices);
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256, string ArtworkSha256);
}
