using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts the starting gameplay colours and the common room-sprite colours as import data (#1637): only the import reads
/// them, to pre-colour the sheets that show them, so the document goes to the component's import-data directory
/// (<see cref="ImportDataDirectory.Of"/>), which the installer deletes before it publishes. The installation holds none of it.
/// </summary>
public static class GameplayBasePaletteFiles
{
    /// <summary>
    /// Where the sprite colours begin in the gameplay colours: the upper half, whose first row receives the common
    /// sprite colours at every room load.
    /// </summary>
    private const int FirstSpriteColour = CgramPalettes.ColorCount / 2;

    /// <summary>Writes the starting 256 colours and the 16 common-sprite colours into the component's import data.</summary>
    /// <param name="bus">Cartridge source for the starting and common-sprite colours.</param>
    /// <param name="directory">The component's directory; the document goes to its import-data directory, created if absent.</param>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string importData = ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        var document = new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.InitialPalette, CgramPalettes.ColorCount),
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.CommonSpritePalette,
                GameplayBasePaletteFormat.SpriteColorCount));
        File.WriteAllBytes(Path.Combine(importData, GameplayBasePaletteFormat.ArtworkFileName), Write(document));
    }

    /// <summary>Compiles the import's base-palette document, which only the import reads (#1637).</summary>
    /// <param name="importData">The component's import-data directory, holding the base-palette document.</param>
    /// <returns>An owned colour catalog for the starting colours and the common sprites.</returns>
    /// <exception cref="InvalidDataException">The document, its version, or its palette counts and channels are invalid.</exception>
    public static GameplayBasePalette LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        string path = Path.Combine(importData, GameplayBasePaletteFormat.ArtworkFileName);
        try
        {
            return Read(new MemoryStream(File.ReadAllBytes(path), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid gameplay base palette {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and validates the full initial and common-sprite RGB5 palettes of a base-palette document.</summary>
    /// <param name="json">The caller-owned stream holding the gameplay base-palette document.</param>
    /// <returns>The selected colours, detached from the document.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, palette lengths or RGB5 channels are invalid.</exception>
    public static GameplayBasePalette Read(Stream json)
    {
        GameplayBasePaletteDocument document = JsonAssetDocument.Read<GameplayBasePaletteDocument>(
            json, GameplayBasePaletteFormat.JsonOptions, "gameplay base palette");
        if (document.Version != GameplayBasePaletteFormat.Version)
            throw new InvalidDataException("Gameplay base palette has an unsupported version.");
        RgbColor[] initial = Compile(document.Initial, CgramPalettes.ColorCount, "initial");
        RgbColor[] commonSprites = Compile(document.CommonSprites, GameplayBasePaletteFormat.SpriteColorCount, "commonSprites");
        return new GameplayBasePalette(GameplayColourImage.Decode(initial), commonSprites);
    }

    /// <summary>Serializes a gameplay base-palette document as JSON.</summary>
    public static byte[] Write(GameplayBasePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            GameplayBasePaletteFormat.JsonOptions);
        return bytes;
    }

    /// <summary>Checks palette length and channel bounds before converting the document's colours to RGB.</summary>
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

    /// <summary>
    /// The eight sprite palette rows as a room load leaves them: the initial image's upper half, with the common sprite
    /// colours in the first row. The enemy-projectile row a room load copies is the initial image's own.
    /// </summary>
    internal static RgbColor[] RoomLoadSpriteColours(ISnesAddressSpace bus)
    {
        var document = new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.InitialPalette, CgramPalettes.ColorCount),
            ReadColors(bus, GameplayBasePaletteSourceDefinitions.CommonSpritePalette, GameplayBasePaletteFormat.SpriteColorCount));
        RgbColor[] initial = Compile(document.Initial, CgramPalettes.ColorCount, "initial");
        int first = FirstSpriteColour;
        RgbColor[] colours = initial.AsSpan(first, CgramPalettes.ColorCount - first).ToArray();
        Compile(document.CommonSprites, GameplayBasePaletteFormat.SpriteColorCount, "commonSprites").CopyTo(colours, 0);
        return colours;
    }

    /// <summary>Reads <paramref name="count"/> colours from the cartridge at <paramref name="address"/>.</summary>
    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int address, int count)
    {
        var colors = new PaletteRgb5[count];
        for (int color = 0; color < count; color++)
        {
            int word = CartridgeImportSource.Require(bus).ReadCartridgeWord(address + color * 2);
            colors[color] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }
}
