using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs opening-scene PNGs and tilemap JSON separately from player overrides.</summary>
public static class IntroCinematicArtworkFiles
{
    private const int FormatVersion = 14;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);

        var hashes = new Dictionary<string, string>();
        WriteSheet(IntroCinematicArtworkFormat.BackgroundFileName,
            RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.BackgroundCharacters,
                maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundByteCount),
            IntroCinematicArtworkFormat.BackgroundByteCount);
        WriteSheet(IntroCinematicArtworkFormat.IntroObjectFileName,
            RomDataReader.ReadFixedBank(bus, IntroCinematicRomData.Assets.IntroObjectCharacters,
                IntroCinematicArtworkFormat.IntroObjectByteCount),
            IntroCinematicArtworkFormat.IntroObjectByteCount);
        WriteSheet(IntroCinematicArtworkFormat.CinematicObjectFileName,
            RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.ObjectCharacters,
                maximumOutputBytes: IntroCinematicArtworkFormat.CinematicObjectByteCount),
            IntroCinematicArtworkFormat.CinematicObjectByteCount);
        WritePage(IntroCinematicArtworkFormat.PortraitTilemapFileName,
            RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.SamusHeadTilemap,
                maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundPageByteCount));
        WritePage(IntroCinematicArtworkFormat.InitialNarrationTilemapFileName,
            RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.FirstNarrationTilemap,
                maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundPageByteCount));
        byte[] backgroundPages = RomDataReader.Decompress(bus,
            IntroCinematicRomData.Assets.BackgroundPageTilemaps,
            maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundPageCount *
                IntroCinematicArtworkFormat.BackgroundPageByteCount);
        if (backgroundPages.Length != IntroCinematicArtworkFormat.BackgroundPageCount *
            IntroCinematicArtworkFormat.BackgroundPageByteCount)
            throw new InvalidDataException("Opening cinematic BG tilemap has the wrong number of pages.");
        for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
            WritePage(IntroCinematicArtworkFormat.BackgroundPageFileName(page),
                backgroundPages.AsSpan(page * IntroCinematicArtworkFormat.BackgroundPageByteCount,
                    IntroCinematicArtworkFormat.BackgroundPageByteCount));
        WriteFinalLine();
        WriteEyeFrames();
        WriteCaretSprites();
        WriteMotherBrainSprites();
        WriteMotherBrainExplosionSprites();
        WritePalette();
        foreach ((string name, byte[] file) in CeresFlightArtworkExtractor.Extract(bus))
        {
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }
        foreach ((string name, byte[] file) in CeresDestructionArtworkExtractor.Extract(bus))
        {
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
        }

        var manifest = new IntroCinematicArtworkManifest(FormatVersion, sourceCartridgeSha256, hashes);
        using var stream = new FileStream(Path.Combine(directory, IntroCinematicArtworkFormat.ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(stream, manifest, JsonOptions);
        return;

        void WriteSheet(string name, byte[] planar, int expectedByteCount)
        {
            if (planar.Length != expectedByteCount)
                throw new InvalidDataException(
                    $"Intro sheet {name} has {planar.Length} tile bytes, expected {expectedByteCount}.");
            byte[] indexes = SnesGraphics.DecodePlanarTiles(planar, 4,
                IntroCinematicArtworkFormat.TileColumns, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, indexes, SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();
            RoomCharacterAtlas roundTrip = RoomCharacterAtlas.Load(
                new MemoryStream(encoded, writable: false), expectedByteCount);
            if (!roundTrip.Transfer.Span.SequenceEqual(planar))
                throw new InvalidDataException($"Intro PNG {name} did not round-trip its cartridge tiles.");
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WritePage(string name, ReadOnlySpan<byte> native)
        {
            if (native.Length != IntroCinematicArtworkFormat.BackgroundPageByteCount)
                throw new InvalidDataException($"Intro tilemap {name} must contain one $0800-byte page.");
            byte[] encoded = RoomBackgroundTilemapExtractor.Encode(native);
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WritePalette()
        {
            var native = new byte[SnesCgram.ByteCount];
            for (int index = 0; index < native.Length; index++)
                native[index] = bus.ReadByte(IntroCinematicRomData.Assets.Palette + index);
            var colors = new PaletteRgb5[SnesCgram.ColorCount];
            for (int index = 0; index < colors.Length; index++)
            {
                ushort word = BinaryPrimitives.ReadUInt16LittleEndian(
                    native.AsSpan(index * sizeof(ushort)));
                if ((word & 0x8000) != 0)
                    throw new InvalidDataException($"Opening palette color {index} has an unrepresentable high bit.");
                colors[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            using var json = new MemoryStream();
            IntroCinematicPalette.Write(json, new IntroCinematicPaletteDocument
            {
                Version = IntroCinematicPaletteFormat.Version,
                Colors = colors,
            });
            byte[] encoded = json.ToArray();
            if (!IntroCinematicPalette.Load(new MemoryStream(encoded, writable: false))
                .Transfer.Span.SequenceEqual(native))
                throw new InvalidDataException("Opening palette JSON did not round-trip cartridge colors.");
            string name = IntroCinematicPaletteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteFinalLine()
        {
            var native = new ushort[IntroFinalLineTilemapFormat.CellCount];
            var cells = new RoomBackgroundTilemapCell[native.Length];
            for (int index = 0; index < native.Length; index++)
            {
                int source = IntroCinematicRomData.Assets.FinalTextLine + index * sizeof(ushort);
                var word = new SnesBgTilemapWord((ushort)(bus.ReadByte(source) |
                    bus.ReadByte(source + 1) << 8));
                native[index] = word.Raw;
                cells[index] = new RoomBackgroundTilemapCell
                {
                    TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                    TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                    Palette = word.PaletteIndex,
                    Priority = word.HasPriority,
                    FlipX = word.FlipHorizontally,
                    FlipY = word.FlipVertically,
                };
            }
            using var json = new MemoryStream();
            IntroFinalLineTilemap.Write(json, new IntroFinalLineTilemapDocument
            {
                Version = IntroFinalLineTilemapFormat.Version,
                Cells = cells,
            });
            byte[] encoded = json.ToArray();
            if (!IntroFinalLineTilemap.Load(new MemoryStream(encoded, writable: false))
                .Words.Span.SequenceEqual(native))
                throw new InvalidDataException("Opening divider tilemap JSON did not round-trip cartridge words.");
            string name = IntroFinalLineTilemapFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteEyeFrames()
        {
            var frames = new IntroEyeTilemapFrame[IntroEyeTilemapFormat.FrameCount];
            var native = new ushort[frames.Length][];
            for (int frame = 0; frame < frames.Length; frame++)
            {
                ushort pointer = (ushort)(IntroEyeAnimationDefinitions.FrameStartPointer +
                    frame * IntroEyeAnimationDefinitions.FrameStride);
                int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, pointer);
                ushort function = (ushort)(bus.ReadByte(source) | bus.ReadByte(source + 1) << 8);
                if (function != CinematicCodePointers.IndirectInstruction_DrawToPortraitTilemap ||
                    bus.ReadByte(source + 2) != IntroEyeTilemapFormat.Columns ||
                    bus.ReadByte(source + 3) != IntroEyeTilemapFormat.Rows)
                    throw new InvalidDataException($"Opening eye frame {frame} changed its native draw shape.");
                native[frame] = new ushort[IntroEyeTilemapFormat.CellsPerFrame];
                var cells = new RoomBackgroundTilemapCell[IntroEyeTilemapFormat.CellsPerFrame];
                for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                {
                    int wordSource = source + 4 + cellIndex * sizeof(ushort);
                    var word = new SnesBgTilemapWord((ushort)(bus.ReadByte(wordSource) |
                        bus.ReadByte(wordSource + 1) << 8));
                    native[frame][cellIndex] = word.Raw;
                    cells[cellIndex] = new RoomBackgroundTilemapCell
                    {
                        TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                        TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                        Palette = word.PaletteIndex,
                        Priority = word.HasPriority,
                        FlipX = word.FlipHorizontally,
                        FlipY = word.FlipVertically,
                    };
                }
                frames[frame] = new IntroEyeTilemapFrame
                {
                    Id = IntroEyeTilemapFormat.FrameId(frame),
                    Cells = cells,
                };
            }
            using var json = new MemoryStream();
            IntroEyeTilemapPresentation.Write(json, new IntroEyeTilemapDocument
            {
                Version = IntroEyeTilemapFormat.Version,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            IntroEyeTilemapPresentation roundTrip = IntroEyeTilemapPresentation.Load(
                new MemoryStream(encoded, writable: false));
            for (int frame = 0; frame < frames.Length; frame++)
                if (!roundTrip.FrameWords(frame).SequenceEqual(native[frame]))
                    throw new InvalidDataException($"Opening eye frame {frame} did not round-trip cartridge tile words.");
            string name = IntroEyeTilemapFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteCaretSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (IntroCaretFrameDefinition definition in IntroCaretSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    definition.Pointer, definition.StockPartCount, definition.Name));
            using var json = new MemoryStream();
            IntroCaretSpritePresentation.Write(json, new IntroCaretSpriteDocument
            {
                Version = IntroCaretSpriteFormat.Version,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            string name = IntroCaretSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteMotherBrainSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (IntroMotherBrainSpriteFrameDefinition definition in
                IntroMotherBrainSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    definition.Pointer, IntroMotherBrainSpriteDefinitions.StockPartCount,
                    definition.Name));
            using var json = new MemoryStream();
            IntroMotherBrainSpritePresentation.Write(json, new IntroMotherBrainSpriteDocument
            {
                Version = IntroMotherBrainSpriteFormat.Version,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            string name = IntroMotherBrainSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteMotherBrainExplosionSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (IntroMotherBrainExplosionSpriteFrameDefinition definition in
                IntroMotherBrainExplosionSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    definition.Pointer, definition.StockPartCount, definition.Name));
            using var json = new MemoryStream();
            IntroMotherBrainExplosionSpritePresentation.Write(json,
                new IntroMotherBrainExplosionSpriteDocument
                {
                    Version = IntroMotherBrainExplosionSpriteFormat.Version,
                    Frames = frames,
                });
            byte[] encoded = json.ToArray();
            string name = IntroMotherBrainExplosionSpriteFormat.FileName;
            using (var output = new FileStream(Path.Combine(directory, name),
                FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }
    }

    /// <summary>Checks every stock hash before selecting independently editable PNGs.</summary>
    public static IntroCinematicArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, IntroCinematicArtworkFormat.ManifestFileName);
        IntroCinematicArtworkManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<IntroCinematicArtworkManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Intro artwork manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid intro artwork manifest {manifestPath}.", error);
        }
        string[] names = AllFileNames();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.StockSha256 is null || manifest.StockSha256.Count != names.Length ||
            names.Any(name => !manifest.StockSha256.ContainsKey(name)))
            throw new InvalidDataException($"Intro artwork manifest {manifestPath} does not describe this installation.");

        return new IntroCinematicArtworkCatalog(
            LoadSheet(names[0], IntroCinematicArtworkFormat.BackgroundByteCount),
            LoadSheet(names[1], IntroCinematicArtworkFormat.IntroObjectByteCount),
            LoadSheet(names[2], IntroCinematicArtworkFormat.CinematicObjectByteCount),
            Enumerable.Range(0, IntroCinematicArtworkFormat.BackgroundPageCount)
                .Select(page => LoadPage(IntroCinematicArtworkFormat.BackgroundPageFileName(page)))
                .ToArray(),
            LoadPage(IntroCinematicArtworkFormat.PortraitTilemapFileName),
            LoadPage(IntroCinematicArtworkFormat.InitialNarrationTilemapFileName),
            LoadFinalLine(),
            LoadEyeFrames(),
            LoadCaretSprites(),
            LoadMotherBrainSprites(),
            LoadMotherBrainExplosionSprites(),
            LoadPalette(),
            LoadCeresFlight(),
            LoadCeresDestruction());

        RoomCharacterAtlas LoadSheet(string name, int nativeByteCount)
        {
            (string selectedPath, byte[] selected) = ReadSelected(name);
            try
            {
                return RoomCharacterAtlas.Load(new MemoryStream(selected, writable: false), nativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid intro PNG {selectedPath}: {error.Message}", error);
            }
        }

        RoomBackgroundTilemapAtlas LoadPage(string name)
        {
            (string selectedPath, byte[] selected) = ReadSelected(name);
            try
            {
                return RoomBackgroundTilemapAtlas.Load(new MemoryStream(selected, writable: false),
                    IntroCinematicArtworkFormat.BackgroundPageByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid intro tilemap {selectedPath}: {error.Message}", error);
            }
        }

        CeresFlightArtworkCatalog LoadCeresFlight()
        {
            (string mode7Path, byte[] mode7) = ReadSelected(CeresFlightArtworkFormat.Mode7FileName);
            (string mapPath, byte[] map) = ReadSelected(CeresFlightArtworkFormat.MapFileName);
            (string objectPath, byte[] objects) = ReadSelected(CeresFlightArtworkFormat.ObjectFileName);
            (string palettePath, byte[] palette) = ReadSelected(CeresFlightPaletteFormat.FileName);
            try
            {
                return CeresFlightArtworkCatalog.Load(
                    new MemoryStream(mode7, writable: false),
                    new MemoryStream(map, writable: false),
                    new MemoryStream(objects, writable: false),
                    new MemoryStream(palette, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Ceres flight artwork ({mode7Path}, {mapPath}, {objectPath}, {palettePath}): {error.Message}", error);
            }
        }

        IntroCinematicPalette LoadPalette()
        {
            (string path, byte[] selected) = ReadSelected(IntroCinematicPaletteFormat.FileName);
            try
            {
                return IntroCinematicPalette.Load(new MemoryStream(selected, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid opening palette {path}: {error.Message}", error);
            }
        }

        IntroFinalLineTilemap LoadFinalLine()
        {
            (string path, byte[] selected) = ReadSelected(IntroFinalLineTilemapFormat.FileName);
            try
            {
                return IntroFinalLineTilemap.Load(new MemoryStream(selected, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid opening divider {path}: {error.Message}", error);
            }
        }

        IntroEyeTilemapPresentation LoadEyeFrames()
        {
            (string path, byte[] selected) = ReadSelected(IntroEyeTilemapFormat.FileName);
            try
            {
                return IntroEyeTilemapPresentation.Load(new MemoryStream(selected, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid opening eye art {path}: {error.Message}", error);
            }
        }

        IntroCaretSpritePresentation LoadCaretSprites()
        {
            (string path, byte[] selected) = ReadSelected(IntroCaretSpriteFormat.FileName);
            try
            {
                return IntroCaretSpritePresentation.Load(new MemoryStream(selected, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid opening caret sprites {path}: {error.Message}", error);
            }
        }

        IntroMotherBrainSpritePresentation LoadMotherBrainSprites()
        {
            (string path, byte[] selected) = ReadSelected(IntroMotherBrainSpriteFormat.FileName);
            try
            {
                return IntroMotherBrainSpritePresentation.Load(
                    new MemoryStream(selected, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid intro Mother Brain sprites {path}: {error.Message}", error);
            }
        }

        IntroMotherBrainExplosionSpritePresentation LoadMotherBrainExplosionSprites()
        {
            (string path, byte[] selected) = ReadSelected(
                IntroMotherBrainExplosionSpriteFormat.FileName);
            try
            {
                return IntroMotherBrainExplosionSpritePresentation.Load(
                    new MemoryStream(selected, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid intro Mother Brain explosion sprites {path}: {error.Message}", error);
            }
        }

        CeresDestructionArtworkCatalog LoadCeresDestruction()
        {
            (string ceresPath, byte[] ceres) = ReadSelected(CeresDestructionArtworkFormat.CeresMapFileName);
            (string zebesMapPath, byte[] zebesMap) = ReadSelected(CeresDestructionArtworkFormat.ZebesMapFileName);
            (string zebesPngPath, byte[] zebesPng) = ReadSelected(CeresDestructionArtworkFormat.ZebesCharacterFileName);
            try
            {
                return CeresDestructionArtworkCatalog.Load(
                    new MemoryStream(ceres, writable: false),
                    new MemoryStream(zebesMap, writable: false),
                    new MemoryStream(zebesPng, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid destruction artwork ({ceresPath}, {zebesMapPath}, {zebesPngPath}): {error.Message}",
                    error);
            }
        }

        (string Path, byte[] Bytes) ReadSelected(string name)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), manifest.StockSha256[name],
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock intro resource {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            return (selectedPath, selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath));
        }
    }

    private static string[] AllFileNames() =>
    [
        IntroCinematicArtworkFormat.BackgroundFileName,
        IntroCinematicArtworkFormat.IntroObjectFileName,
        IntroCinematicArtworkFormat.CinematicObjectFileName,
        IntroCinematicArtworkFormat.PortraitTilemapFileName,
        IntroCinematicArtworkFormat.InitialNarrationTilemapFileName,
        IntroFinalLineTilemapFormat.FileName,
        IntroEyeTilemapFormat.FileName,
        IntroCaretSpriteFormat.FileName,
        IntroMotherBrainSpriteFormat.FileName,
        IntroMotherBrainExplosionSpriteFormat.FileName,
        .. Enumerable.Range(0, IntroCinematicArtworkFormat.BackgroundPageCount)
            .Select(IntroCinematicArtworkFormat.BackgroundPageFileName),
        IntroCinematicPaletteFormat.FileName,
        CeresFlightArtworkFormat.Mode7FileName,
        CeresFlightArtworkFormat.MapFileName,
        CeresFlightArtworkFormat.ObjectFileName,
        CeresFlightPaletteFormat.FileName,
        CeresDestructionArtworkFormat.CeresMapFileName,
        CeresDestructionArtworkFormat.ZebesMapFileName,
        CeresDestructionArtworkFormat.ZebesCharacterFileName,
    ];

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record IntroCinematicArtworkManifest(
        int Version, string SourceCartridgeSha256, Dictionary<string, string> StockSha256);
}
