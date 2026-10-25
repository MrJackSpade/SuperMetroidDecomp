using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Exports the Ceres-door actor's direct tile DMA and authored RGB5 rows.</summary>
[CpuBusBoundary("codec: ROM Ceres-door tile DMA and RGB5 rows → Ceres door visual files")]
public static class CeresDoorVisualFiles
{
    /// <summary>Exports the Ceres door tile DMA as indexed PNG and its normal, escape, animated, and Mode 7 appearances as JSON, verifying the tile round-trip.</summary>
    /// <param name="bus">Supported-cartridge address space containing the Ceres door tile, palette, and Mode 7 frame sources.</param>
    /// <param name="directory">Destination directory receiving the tile PNG and color JSON; existing files are replaced.</param>
    /// <returns>Uppercase SHA-256 digests of the exact tile and color files written.</returns>
    public static CeresDoorVisualHashes Extract(ISnesAddressSpace bus,
        string directory)
    {
        Ensure.NotNull(bus);
        byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresSourceDefinitions.Door.Characters, CeresDoorVisualDefinitions.CharacterBytes);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var image = new MemoryStream();
        IndexedPng.Write(image, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] tilePng = image.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlasPng.Read(
            new MemoryStream(tilePng, writable: false), planar.Length);
        if (!roundtrip.Characters.Span.SequenceEqual(planar))
            throw new InvalidDataException("Ceres-door PNG changed the native tile DMA.");

        byte[] paletteJson = Write(new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = ReadColors(bus, CeresSourceDefinitions.Door.NormalColors,
                CeresDoorVisualDefinitions.SetupColorCount),
            Escape = ReadColors(bus, CeresSourceDefinitions.Door.EscapeColors,
                CeresDoorVisualDefinitions.SetupColorCount),
            Animation = Enumerable.Range(0, CeresDoorVisualDefinitions.AnimationRowCount)
                .Select(row => ReadColors(bus,
                    CeresSourceDefinitions.Door.AnimationColors +
                        row * CeresSourceDefinitions.Door.AnimationRowBytes,
                    CeresDoorVisualDefinitions.AnimationColorCount)).ToArray(),
            Mode7DoorFrames =
            [
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    CeresSourceDefinitions.Door.PlatformFrames,
                    CeresDoorVisualDefinitions.PlatformCellCount)
                    .Select(value => (int)value).ToArray(),
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    CeresSourceDefinitions.Door.PlatformFrames + CeresSourceDefinitions.Door.PlatformFrameBytes,
                    CeresDoorVisualDefinitions.PlatformCellCount)
                    .Select(value => (int)value).ToArray(),
            ],
        });
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, CeresDoorVisualFormat.TilesFileName), tilePng);
        File.WriteAllBytes(Path.Combine(directory, CeresDoorVisualFormat.ColorsFileName), paletteJson);
        return new CeresDoorVisualHashes(Convert.ToHexString(SHA256.HashData(tilePng)),
            Convert.ToHexString(SHA256.HashData(paletteJson)));
    }

    /// <summary>Compiles an in-memory Ceres door PNG/JSON pair into owned runtime visual data without cartridge access.</summary>
    /// <param name="tilePng">Indexed PNG bytes for the fixed-size Ceres door tile transfer.</param>
    /// <param name="paletteJson">Versioned RGB5 and Mode 7 appearance document bytes.</param>
    /// <returns>The validated selected Ceres door visual catalog.</returns>
    public static CeresDoorVisualCatalog Load(byte[] tilePng, byte[] paletteJson) =>
        Read(new MemoryStream(tilePng, writable: false),
            new MemoryStream(paletteJson, writable: false));

    /// <summary>Loads the indexed 4bpp door sheet and version-1 visual JSON, validating native transfer lengths, RGB5 channels, and eight-bit Mode 7 character selectors without importing door timing or variant logic.</summary>
    /// <param name="tilePng">Caller-owned <c>ceres-door-tiles.png</c> stream compiling to $0400 bytes.</param>
    /// <param name="paletteJson">Caller-owned <c>ceres-door-colors.json</c> stream containing setup colors, animation rows, and Mode 7 strip cells.</param>
    /// <returns>The compiled visual snapshot; streams are consumed from their current positions and left open.</returns>
    public static CeresDoorVisualCatalog Read(Stream tilePng, Stream paletteJson)
    {
        Ensure.NotNull(tilePng);
        Ensure.NotNull(paletteJson);
        CeresDoorVisualDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CeresDoorVisualDocument>(paletteJson, Options)
                ?? throw new InvalidDataException("Ceres-door visual palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres-door visual palette JSON.", error);
        }
        if (document.Version != 1 || document.Animation is null ||
            document.Animation.Length != CeresDoorVisualDefinitions.AnimationRowCount)
            throw new InvalidDataException("Ceres-door visuals require version 1 and eight animation rows.");
        Bgr555[] normal = Compile(document.Normal, CeresDoorVisualDefinitions.SetupColorCount,
            "normal");
        Bgr555[] escape = Compile(document.Escape, CeresDoorVisualDefinitions.SetupColorCount,
            "escape");
        Bgr555[][] animation = document.Animation.Select((row, index) =>
            Compile(row, CeresDoorVisualDefinitions.AnimationColorCount,
                $"animation row {index}")).ToArray();
        byte[][] platformFrames = CompileMode7Frames(document.Mode7DoorFrames);
        RoomCharacterAtlas tiles = RoomCharacterAtlasPng.Read(tilePng, CeresDoorVisualDefinitions.CharacterBytes);
        return CeresDoorVisualCatalog.Load(tiles, normal, escape, animation,
            platformFrames);
    }

    /// <summary>Serializes visual JSON as indented camel-case UTF-8, validating version, palette dimensions, RGB5 bounds, and Mode 7 frame bytes before returning it; PNG admission is a separate boundary.</summary>
    public static byte[] Write(CeresDoorVisualDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        // The PNG is validated independently; validate color structure here.
        if (document.Version != 1 || document.Animation is null ||
            document.Animation.Length != CeresDoorVisualDefinitions.AnimationRowCount)
            throw new InvalidDataException("Ceres-door visuals require version 1 and eight animation rows.");
        _ = Compile(document.Normal, CeresDoorVisualDefinitions.SetupColorCount, "normal");
        _ = Compile(document.Escape, CeresDoorVisualDefinitions.SetupColorCount, "escape");
        for (int row = 0; row < document.Animation.Length; row++)
            _ = Compile(document.Animation[row], CeresDoorVisualDefinitions.AnimationColorCount,
                $"animation row {row}");
        _ = CompileMode7Frames(document.Mode7DoorFrames);
        return json;
    }

    /// <summary>Decodes packed cartridge RGB5 words and rejects colors with an unsupported high bit.</summary>
    private static PaletteRgb5[] ReadColors(ISnesAddressSpace bus, int sourceAddress,
        int count)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), sourceAddress,
            count * sizeof(ushort));
        var colors = new PaletteRgb5[count];
        for (int index = 0; index < count; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * 2));
            if (!Bgr555.IsColorWord(word))
                throw new InvalidDataException(
                    $"Ceres-door color ${sourceAddress + index * 2:X6} has an unrepresentable high bit.");
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }

    /// <summary>Validates exactly two four-cell Mode 7 strips and converts their character selectors to bytes.</summary>
    private static byte[][] CompileMode7Frames(int[][]? source)
    {
        if (source is null || source.Length != CeresDoorVisualDefinitions.PlatformFrameCount)
            throw new InvalidDataException("Ceres-door visuals require two Mode-7 tilemap frames.");
        var result = new byte[source.Length][];
        for (int frame = 0; frame < source.Length; frame++)
        {
            int[]? values = source[frame];
            if (values is null || values.Length != CeresDoorVisualDefinitions.PlatformCellCount)
                throw new InvalidDataException($"Ceres-door Mode-7 frame {frame} needs four bytes.");
            result[frame] = new byte[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                if ((uint)values[index] > byte.MaxValue)
                    throw new InvalidDataException(
                        $"Ceres-door Mode-7 frame {frame} value {index} is not a byte.");
                result[frame][index] = (byte)values[index];
            }
        }
        return result;
    }

    /// <summary>Validates a palette's size and RGB5 channel bounds before packing its colors.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int expectedCount, string name)
    {
        if (source is null || source.Length != expectedCount)
            throw new InvalidDataException($"Ceres-door {name} needs {expectedCount} RGB5 colors.");
        var result = new Bgr555[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Ceres-door {name} color {index} must be RGB5.");
            result[index] = color.ToBgr555();
        }
        return result;
    }

    /// <summary>Strict camel-case JSON settings shared by the door visual loader and writer.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
