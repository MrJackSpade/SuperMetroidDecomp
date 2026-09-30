using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the native cannon-cover pose/OAM selectors and twelve character tiles.</summary>
public static class SamusArmCannonArtworkFiles
{
    private const string ManifestFileName = "samus-arm-cannon-manifest.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        int[] pointers = Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount)
            .Select(pose => (int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                SamusRenderingRomData.ArmCannon.PoseDrawingDataPointers + pose * 2)).ToArray();
        int[] drawingData = Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount)
            .Select(index => (int)bus.ReadCartridgeByte(SamusRenderingRomData.Banks.Movement |
                (SamusArmCannonArtworkFormat.DrawingDataStart + index))).ToArray();
        int[] attributes = Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount)
            .Select(direction => (int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                SamusRenderingRomData.ArmCannon.SpriteAttributes + direction * 2)).ToArray();
        int[][] sources = Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount)
            .Select(direction =>
            {
                ushort list = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    SamusRenderingRomData.ArmCannon.TileListPointers + direction * 2);
                return Enumerable.Range(0, SamusArmCannonArtworkFormat.FramesPerDirection)
                    .Select(frame => (int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                        SamusRenderingRomData.Banks.Movement | (list + frame * 2))).ToArray();
            }).ToArray();
        var document = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PosePointers = pointers,
            DrawingData = drawingData,
            SpriteAttributes = attributes,
            TileSources = sources,
        };
        byte[] planar = new byte[SamusArmCannonArtworkFormat.TileSourcePointers.Length *
            SamusRenderingRomData.ArmCannon.TileUploadByteCount];
        for (int tile = 0; tile < SamusArmCannonArtworkFormat.TileSourcePointers.Length; tile++)
            for (int offset = 0; offset < SamusRenderingRomData.ArmCannon.TileUploadByteCount; offset++)
                planar[tile * SamusRenderingRomData.ArmCannon.TileUploadByteCount + offset] =
                    bus.ReadCartridgeByte(SamusRenderingRomData.Banks.CharacterData |
                        (SamusArmCannonArtworkFormat.TileSourcePointers[tile] + offset));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            SamusArmCannonArtworkFormat.TileSourcePointers.Length,
            out int width, out int height);
        using var pngStream = new MemoryStream();
        IndexedPng.Write(pngStream, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] png = pngStream.ToArray();
        byte[] json = SamusArmCannonArtworkCatalog.Write(document);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.TileFileName), png);
        File.WriteAllBytes(Path.Combine(directory, SamusArmCannonArtworkFormat.JsonFileName), json);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new Manifest(
                SamusArmCannonArtworkFormat.Version, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(json)),
                Convert.ToHexString(SHA256.HashData(png))), JsonOptions));
        SamusArmCannonArtworkCatalog roundTrip = Load(directory, null);
        for (int tile = 0; tile < SamusArmCannonArtworkFormat.TileSourcePointers.Length; tile++)
        {
            if (!roundTrip.TryResolveTile(SamusRenderingRomData.Banks.CharacterData |
                    SamusArmCannonArtworkFormat.TileSourcePointers[tile],
                    SamusRenderingRomData.ArmCannon.TileUploadByteCount, out var decoded) ||
                !decoded.Span.SequenceEqual(planar.AsSpan(
                    tile * SamusRenderingRomData.ArmCannon.TileUploadByteCount,
                    SamusRenderingRomData.ArmCannon.TileUploadByteCount)))
                throw new InvalidDataException($"Arm-cannon tile {tile} changed in PNG round-trip.");
        }
    }

    public static SamusArmCannonArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(stockDirectory, ManifestFileName));
        Manifest manifest = manifestFile.Json<Manifest>(JsonOptions);
        return manifestFile.WithContext(() => LoadArtwork());

        SamusArmCannonArtworkCatalog LoadArtwork()
        {
            if (manifest.Version != SamusArmCannonArtworkFormat.Version ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Arm-cannon manifest does not match the pinned cartridge.");
            SamusArtworkFile stockJson = SamusArtworkFile.Stock(Path.Combine(stockDirectory,
                SamusArmCannonArtworkFormat.JsonFileName), manifest.JsonSha256);
            SamusArtworkFile stockPng = SamusArtworkFile.Stock(Path.Combine(stockDirectory,
                SamusArmCannonArtworkFormat.TileFileName), manifest.PngSha256);
            _ = Compile(stockJson, stockPng);
            return Compile(stockJson.Select(overrideDirectory), stockPng.Select(overrideDirectory));
        }

        static SamusArmCannonArtworkCatalog Compile(SamusArtworkFile json, SamusArtworkFile png)
        {
            var placement = json.Compile(SamusArmCannonArtworkCatalog.LoadPlacement);
            var tiles = png.Compile(stream => RoomCharacterAtlas.Load(stream,
                SamusArmCannonArtworkFormat.TileSourcePointers.Length *
                    SamusRenderingRomData.ArmCannon.TileUploadByteCount));
            return SamusArmCannonArtworkCatalog.FromPlacement(placement, tiles);
        }
    }

    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        string JsonSha256, string PngSha256);
}
