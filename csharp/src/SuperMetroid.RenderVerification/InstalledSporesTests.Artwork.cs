using System.Text.Json.Nodes;
using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class InstalledSporesTests
{
    private static void CheckArtwork(RoomFxAnimatedTileAtlas stock, string maps, string romPath,
        string nativeArtworkDirectory)
    {
        using var currentPng = File.OpenRead(Path.Combine(maps, RoomFxAnimatedTileAtlasFormat.FileName));
        IndexedPngImage image = IndexedPng.Read(currentPng, RoomFxAnimatedTileAtlasFormat.Width, 8);
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 2);
        // Appending spores must not shift any earlier strip: every segment of the installed sheet,
        // frames and shared treadmill/statue strips alike, equals its native cartridge bytes.
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(romPath);
        int segmentOffset = 0;
        foreach (RoomFxAtlasSegment segment in RoomFxAnimatedTileAtlasFormat.Segments)
        {
            for (int index = 0; index < segment.ByteCount; index++)
                Require(planar[segmentOffset + index] == SuperMetroid.Core.Hardware.SnesCartridgeImportExtensions.ReadCartridgeByte(rom, segment.SourceAddress + index),
                    $"Installed sheet segment ${segment.SourceAddress:X6} differs from its native bytes at +{index}.");
            segmentOffset += segment.ByteCount;
        }
        Require(segmentOffset == planar.Length, "Installed sheet length differs from its native segment list.");
        int sporesOffset = RoomFxAnimatedTileAtlasFormat.PreSporesWidth * 2;
        var controls = RoomFxAnimatedTileMechanicsDefinitions.All.Single(
            value => value.ObjectPointer == AnimatedTileObjectPointers.Spores);
        for (int frame = 0; frame < 3; frame++)
        {
            int source = RoomFxAnimatedTileArtworkDefinitions.SourceAddress(controls, controls.Frames[frame].InstructionPointer);
            Require(stock.TryResolve(source, 48, out var bytes), "Spore frame identity is absent.");
            byte[] pinnedArt = File.ReadAllBytes(Path.Combine(Path.GetFullPath(nativeArtworkDirectory),
                $"AnimatedTiles_Spores_{frame}.bin"));
            Require(bytes.Span.SequenceEqual(pinnedArt), "Extracted spores differ from pinned native artwork.");
            Require(bytes.Span.SequenceEqual(planar.AsSpan(sporesOffset + frame * 48, 48)),
                "Spore transfer does not resolve the appended PNG segment.");
        }
        foreach (int width in new[] { RoomFxAnimatedTileAtlasFormat.LegacyWidth,
            RoomFxAnimatedTileAtlasFormat.PreStatueWidth, RoomFxAnimatedTileAtlasFormat.PreSporesWidth })
        {
            var pixels = new byte[width * 8];
            for (int y = 0; y < 8; y++)
                image.Pixels.AsSpan(y * image.Width, width).CopyTo(pixels.AsSpan(y * width));
            pixels[0] = (byte)((pixels[0] + 1) % 4);
            using var legacyPng = new MemoryStream();
            IndexedPng.Write(legacyPng, width, 8, pixels, image.Palette);
            legacyPng.Position = 0;
            RoomFxAnimatedTileAtlas legacy = RoomFxAnimatedTileAtlas.Load(legacyPng, stock);
            byte[] expected = (byte[])planar.Clone();
            SnesPlanarTileEncoder.Encode(pixels, width, 8, 2).CopyTo(expected, 0);
            int offset = 0;
            foreach (var segment in RoomFxAnimatedTileAtlasFormat.Segments)
            {
                if (segment.IsFrame)
                {
                    Require(legacy.TryResolve(segment.SourceAddress, segment.ByteCount, out var actual), "Legacy frame vanished.");
                    Require(actual.Span.SequenceEqual(expected.AsSpan(offset, segment.ByteCount)),
                        "Legacy sheet edit/tail inheritance changed a transfer.");
                }
                offset += segment.ByteCount;
            }
            // Statue frames overlap the middle strip and must retain their original
            // aliases even though spores now occupy the final nine characters.
            foreach (var statue in TourianStatueAnimatedTileMechanicsDefinitions.All)
            foreach (ushort operand in statue.SourceOperandPointers)
            {
                int source = TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(statue, operand);
                int statueOffset = RoomFxAnimatedTileAtlasFormat.PreStatueTileCount * 16 +
                    source - TourianStatueAnimatedTileArtworkDefinitions.FirstSource;
                Require(legacy.TryResolve(source, statue.TransferByteCount, out var actual) &&
                    actual.Span.SequenceEqual(expected.AsSpan(statueOffset, statue.TransferByteCount)),
                    "Appending spores shifted an existing statue alias.");
            }
        }
        Console.WriteLine("Pinned three-frame spores art and unchanged historical atlas prefix pass; all three older PNG geometries retain edits and stock tails.");
    }

    private static void WriteOverrides(string maps, string overrides)
    {
        using var input = File.OpenRead(Path.Combine(maps, RoomFxAnimatedTileAtlasFormat.FileName));
        IndexedPngImage image = IndexedPng.Read(input, RoomFxAnimatedTileAtlasFormat.Width, 8);
        for (int y = 0; y < 8; y++)
        for (int x = RoomFxAnimatedTileAtlasFormat.PreSporesWidth; x < image.Width; x++)
            image.Pixels[y * image.Width + x] = (byte)((x + y) % 3 + 1);
        using (var output = File.Create(Path.Combine(overrides, RoomFxAnimatedTileAtlasFormat.FileName)))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);

        JsonNode tilemaps = JsonNode.Parse(File.ReadAllBytes(Path.Combine(maps, RoomFxLayer3TilemapFormat.FileName)))!;
        foreach (JsonNode? cell in tilemaps["pages"]!["Spores"]!.AsArray())
        {
            cell!["tileColumn"] = 16; cell["tileRow"] = 2;
            cell["palette"] = 6; cell["flipX"] = true;
        }
        File.WriteAllText(Path.Combine(overrides, RoomFxLayer3TilemapFormat.FileName), tilemaps.ToJsonString());
        JsonNode palettes = JsonNode.Parse(File.ReadAllBytes(Path.Combine(maps, RoomFxPaletteBlendDefinitions.FileName)))!;
        foreach (JsonNode? blend in palettes["blends"]!.AsObject().Select(pair => pair.Value))
        foreach (JsonNode? color in blend!.AsArray())
        {
            color!["red"] = 3; color["green"] = 6; color["blue"] = 9;
        }
        File.WriteAllText(Path.Combine(overrides, RoomFxPaletteBlendDefinitions.FileName), palettes.ToJsonString());
    }

    private static void CheckFileFailures(string maps, string overrides)
    {
        string manifestPath = Path.Combine(maps, AreaMapCatalogFormat.ManifestFile);
        byte[] manifestBytes = File.ReadAllBytes(manifestPath);
        int failures = 0;
        foreach (string file in new[] { RoomFxAnimatedTileAtlasFormat.FileName,
            RoomFxLayer3TilemapFormat.FileName, RoomFxPaletteBlendDefinitions.FileName })
        {
            string path = Path.Combine(overrides, file);
            byte[] original = File.ReadAllBytes(path);
            File.WriteAllBytes(path, [0, 1, 2]);
            try
            {
                Reject(path);
            }
            finally { File.WriteAllBytes(path, original); }
            string baselinePath = Path.Combine(maps, file);
            byte[] stockBytes = File.ReadAllBytes(baselinePath);
            try
            {
                File.Delete(baselinePath); Reject(baselinePath);
                File.WriteAllBytes(baselinePath, [0, 1, 2]); Reject(baselinePath);
                JsonNode manifest = JsonNode.Parse(manifestBytes)!;
                manifest["sha256"]![file] = Convert.ToHexString(SHA256.HashData(new byte[] { 0, 1, 2 }));
                File.WriteAllText(manifestPath, manifest.ToJsonString());
                Reject(baselinePath); // A valid edit must not conceal hash-valid malformed stock.
            }
            finally
            {
                File.WriteAllBytes(baselinePath, stockBytes);
                File.WriteAllBytes(manifestPath, manifestBytes);
            }
        }
        Console.WriteLine($"{failures} missing/corrupt/hash-valid-malformed/override failures report the exact selected filename and retain bad data.");

        void Reject(string expectedPath)
        {
            bool rejected = false;
            try { _ = AreaMapPresentationCatalog.Load(maps, overrides); }
            catch (Exception error) when (error is IOException or InvalidDataException)
            {
                rejected = true;
                Require(error.Message.Contains(Path.GetFullPath(expectedPath), StringComparison.OrdinalIgnoreCase),
                    $"Bad presentation diagnostic omits selected path {expectedPath}: {error}");
            }
            Require(rejected, "Malformed/missing spore presentation was silently ignored.");
            failures++;
        }
    }
}
