using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the starting gameplay CGRAM image and shared room-sprite palette.</summary>
[CpuBusBoundary("codec: ROM starting gameplay CGRAM image and room-sprite palette → GameplayBasePaletteCatalog")]
public static class GameplayBasePaletteFiles
{
    /// <summary>
    /// Where the sprite colours begin in the gameplay colours: the upper half, whose first row receives the common
    /// sprite colours at every room load.
    /// </summary>
    private const int FirstSpriteColour = CgramPalettes.ColorCount / 2;

    /// <summary>Writes the initial 256-color CGRAM image and 16 common-sprite colors as RGB5 JSON with a hashed manifest, then validates the resulting stock installation.</summary>
    /// <param name="bus">Cartridge source for the initial palette at $9A:8000 and common-sprite palette at $9A:FC00.</param>
    /// <param name="directory">Destination directory, created if absent; existing palette JSON and manifest are replaced.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the manifest; the final stock check requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">The generated installation has incompatible provenance, digest, palette schema, or RGB5 content.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        var document = new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.InitialPalette, CgramPalettes.ColorCount),
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.CommonSpritePalette,
                GameplayBasePaletteFormat.SpriteColorCount));
        byte[] bytes = Write(document);
        File.WriteAllBytes(Path.Combine(directory, GameplayBasePaletteFormat.ArtworkFileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, GameplayBasePaletteFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(
                GameplayBasePaletteFormat.Version, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(bytes))),
                GameplayBasePaletteFormat.JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>Validates stock provenance, digest, and both complete RGB5 arrays before compiling an optional whole-document palette replacement without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the base-palette JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose palette JSON replaces both stock arrays when present; an absent file retains stock.</param>
    /// <returns>An owned selected-color catalog for the initial CGRAM image and common sprites, also supplying enemy-projectile colors from the initial image.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, schema, or selected palette counts and RGB5 values are invalid.</exception>
    public static GameplayBasePaletteCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArtworkManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<ArtworkManifest>(File.ReadAllBytes(Path.Combine(
                stockDirectory, GameplayBasePaletteFormat.ManifestFileName)),
                GameplayBasePaletteFormat.JsonOptions) ??
                throw new InvalidDataException("Gameplay base palette manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay base palette manifest.", error);
        }
        if (manifest.Version != GameplayBasePaletteFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Gameplay base palette manifest does not match the pinned cartridge.");
        byte[] stock = File.ReadAllBytes(Path.Combine(stockDirectory,
            GameplayBasePaletteFormat.ArtworkFileName));
        if (!string.Equals(manifest.ArtworkSha256, Convert.ToHexString(SHA256.HashData(stock)),
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Stock gameplay base palette failed its manifest hash.");
        _ = Read(new MemoryStream(stock, writable: false));
        string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory,
            GameplayBasePaletteFormat.ArtworkFileName);
        byte[] selected = overridePath is not null && File.Exists(overridePath)
            ? File.ReadAllBytes(overridePath) : stock;
        return Read(new MemoryStream(selected, writable: false));
    }

    /// <summary>Checks stock manifest provenance and digest and validates the 256 initial and 16 common-sprite RGB5 colors, ignoring overrides.</summary>
    /// <param name="stockDirectory">Installed stock gameplay base-palette directory.</param>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Reads and validates the full initial and common-sprite RGB5 palettes of an installed document.</summary>
    /// <param name="json">The caller-owned stream holding the gameplay base-palette document.</param>
    /// <returns>The selected colours, detached from the document.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, palette lengths or RGB5 channels are invalid.</exception>
    public static GameplayBasePaletteCatalog Read(Stream json)
    {
        GameplayBasePaletteDocument document = JsonAssetDocument.Read<GameplayBasePaletteDocument>(
            json, GameplayBasePaletteFormat.JsonOptions, "gameplay base palette");
        if (document.Version != GameplayBasePaletteFormat.Version)
            throw new InvalidDataException("Gameplay base palette has an unsupported version.");
        return new GameplayBasePaletteCatalog(
            Compile(document.Initial, CgramPalettes.ColorCount, "initial"),
            Compile(document.CommonSprites, GameplayBasePaletteFormat.SpriteColorCount,
                "commonSprites"));
    }

    /// <summary>Validates and serializes a gameplay base-palette document as JSON.</summary>
    public static byte[] Write(GameplayBasePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            GameplayBasePaletteFormat.JsonOptions);
        _ = Read(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Checks palette length and channel bounds before converting authoring colors to packed CGRAM values.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int expected, string name)
    {
        if (source is null || source.Length != expected)
            throw new InvalidDataException($"Gameplay {name} palette requires {expected} RGB5 colors.");
        var result = new RgbColor[expected];
        for (int color = 0; color < expected; color++)
        {
            PaletteRgb5 entry = source[color] ??
                throw new InvalidDataException($"Gameplay {name} color {color} is null.");
            if ((uint)entry.Red > 31 || (uint)entry.Green > 31 || (uint)entry.Blue > 31)
                throw new InvalidDataException($"Gameplay {name} color {color} exceeds RGB5.");
            result[color] = entry.ToRgbColor();
        }
        return result;
    }

    /// <summary>Unpacks consecutive cartridge palette words into RGB5 channels for the exported base palettes.</summary>
    /// <summary>
    /// The eight sprite palette rows as a room load leaves them: the initial image's upper half, with the common sprite
    /// colours in the first row. The enemy-projectile row a room load copies is the initial image's own.
    /// </summary>
    internal static RgbColor[] RoomLoadSpriteColours(ISnesAddressSpace bus)
    {
        var document = new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.InitialPalette, CgramPalettes.ColorCount),
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.CommonSpritePalette, GameplayBasePaletteFormat.SpriteColorCount));
        GameplayBasePaletteCatalog catalog = Read(new MemoryStream(Write(document), writable: false));
        int first = FirstSpriteColour;
        RgbColor[] colours = catalog.initial.AsSpan(first, CgramPalettes.ColorCount - first).ToArray();
        catalog.commonSprites.CopyTo(colours, 0);
        return colours;
    }

    /// <summary>Reads <paramref name="count"/> colours from the cartridge at <paramref name="address"/>.</summary>
    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int address, int count)
    {
        var colors = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            ushort word = CartridgeImportSource.Require(bus).ReadCartridgeWord(address + color * 2);
            colors[color] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }

    /// <summary>Records the schema, cartridge identity, and stock palette digest checked on load.</summary>
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256,
        string ArtworkSha256);
}
