using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// Installs and loads the opening-scene documents; its character pages (#1636), the opening's and the Ceres approach's
/// colours (#1637) and its layouts and sprite compositions, whose entries are the original's packed tile and sprite
/// placements (#1638), are import data, which the extraction writes to the component's import-data directory and only
/// the import's read reads.
/// </summary>
public static class IntroCinematicArtworkFiles
{
    /// <summary>
    /// Manifest revision for the complete opening and Ceres artwork resource set; 22 installs no character page (#1636),
    /// 23 installs the Japanese text's next-page arrow compositions (#1655), 24 installs no palette (#1637), 25 installs no
    /// layout (#1638), 26 installs neither, and 27 installs no sprite composition (#1638).
    /// </summary>
    private const int FormatVersion = 27;

    /// <summary>
    /// The component's character pages (#1636) and the colour documents only the import pre-colours sheets from (#1637),
    /// which the extraction writes to its import-data directory; the manifest lists their hashes.
    /// </summary>
    private static readonly string[] ImportDataFiles =
    [
        IntroCinematicArtworkFormat.BackgroundFileName, IntroCinematicArtworkFormat.IntroObjectFileName,
        IntroCinematicArtworkFormat.CinematicObjectFileName, CeresFlightArtworkFileFormat.FloorFileName,
        CeresFlightArtworkFileFormat.ObjectFileName, ZebesRevealArtworkFormat.CharacterFileName,
        IntroCinematicPaletteFile.FileName, CeresFlightPaletteFormat.FileName,
    ];

    /// <summary>
    /// The component's sprite compositions (#1638), whose parts are the original's packed sprite placements: the opening's,
    /// the Ceres approach's and the Ceres destruction scene's. The extraction writes them to its import-data directory and
    /// only the import's read reads them; the manifest lists their hashes.
    /// </summary>
    private static readonly string[] CompositionFiles =
    [
        IntroCaretSpriteFormat.FileName, IntroNextPageArrowSpriteFormat.FileName, IntroMotherBrainSpriteFormat.FileName,
        IntroMotherBrainExplosionSpriteFormat.FileName, IntroRinkaSpriteFormat.FileName, IntroEggEffectSpriteFormat.FileName,
        IntroDiscoveryActorSpriteFormat.FileName, IntroScientistSpriteFormat.FileName, CeresFlightSpriteFormat.FileName,
        CeresDestructionSpriteFormat.FileName,
    ];

    /// <summary>
    /// The component's layouts (#1638): the scene pages, portrait, first narration card, subtitle line, eye rectangles and
    /// Zebes reveal map, whose entries are the original's packed tile placements. The extraction writes them to its import-data
    /// directory and only the import's read reads them; the manifest lists their hashes.
    /// </summary>
    private static readonly string[] LayoutFiles =
    [
        IntroCinematicArtworkFormat.PortraitTilemapFileName, IntroCinematicArtworkFormat.InitialNarrationTilemapFileName,
        .. Enumerable.Range(0, IntroCinematicArtworkFormat.BackgroundPageCount).Select(IntroCinematicArtworkFormat.BackgroundPageFileName),
        IntroFinalLineTilemapFormat.FileName, IntroEyeTilemapFormat.FileName, ZebesRevealArtworkFormat.MapFileName,
    ];

    /// <summary>Creates the stock opening-cinematic, Ceres-flight, and destruction-scene PNG and JSON resources with their provenance manifest.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying character graphics, tilemaps, palettes, sprites, and scene artwork.</param>
    /// <param name="directory">Output directory, created if needed; each resource and the manifest require a previously absent filename. The character pages, palettes, layouts and compositions go to its import-data directory.</param>
    /// <param name="sourceCartridgeSha256">Nonempty source-cartridge SHA-256 recorded with the generated stock-resource hashes.</param>
    /// <remarks>Checks native transfer dimensions, eye-frame draw geometry, and palette representation. Scene timing and dispatch remain compiled.</remarks>
    /// <exception cref="InvalidDataException">Native artwork, geometry, or palettes fail validation.</exception>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        // A page, a palette, a layout or a composition goes to the import data, which the installer deletes before it
        // publishes; a document is installed.
        string PathOf(string name) => Path.Combine(
            ImportDataFiles.Contains(name) || LayoutFiles.Contains(name) || CompositionFiles.Contains(name) ? importData : directory, name);

        var hashes = new Dictionary<string, string>();
        WriteSheet(IntroCinematicArtworkFormat.BackgroundFileName,
            RomDataReader.Decompress(CartridgeImportSource.Require(bus), IntroCinematicSourceDefinitions.BackgroundCharacters,
                maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundByteCount),
            IntroCinematicArtworkFormat.BackgroundByteCount);
        WriteSheet(IntroCinematicArtworkFormat.IntroObjectFileName,
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), IntroCinematicSourceDefinitions.IntroObjectCharacters,
                IntroCinematicArtworkFormat.IntroObjectByteCount),
            IntroCinematicArtworkFormat.IntroObjectByteCount);
        WriteSheet(IntroCinematicArtworkFormat.CinematicObjectFileName,
            RomDataReader.Decompress(CartridgeImportSource.Require(bus), IntroCinematicSourceDefinitions.ObjectCharacters,
                maximumOutputBytes: IntroCinematicArtworkFormat.CinematicObjectByteCount),
            IntroCinematicArtworkFormat.CinematicObjectByteCount);
        WritePage(IntroCinematicArtworkFormat.PortraitTilemapFileName,
            RomDataReader.Decompress(CartridgeImportSource.Require(bus), IntroCinematicSourceDefinitions.SamusHeadTilemap,
                maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundPageByteCount));
        WritePage(IntroCinematicArtworkFormat.InitialNarrationTilemapFileName,
            RomDataReader.Decompress(CartridgeImportSource.Require(bus), IntroCinematicSourceDefinitions.FirstNarrationTilemap,
                maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundPageByteCount));
        byte[] backgroundPages = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            IntroCinematicSourceDefinitions.BackgroundPageTilemaps,
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
        WriteNextPageArrowSprites();
        WriteMotherBrainSprites();
        WriteMotherBrainExplosionSprites();
        WriteRinkaSprites();
        WriteEggEffectSprites();
        WriteDiscoveryActorSprites();
        WriteScientistSprites();
        IntroCinematicPalette opening = WritePalette();
        CeresFlightPalette? flight = null;
        foreach ((string name, byte[] file) in CeresFlightArtworkExtractor.Extract(bus))
        {
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(file);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(file)));
            if (name == CeresFlightPaletteFormat.FileName)
                flight = CeresFlightPaletteFile.Read(new MemoryStream(file, writable: false));
        }
        // The game reads the scenes' backdrops, taken once from their extracted colours, and neither palette (#1544).
        WriteFile(IntroBackdropFiles.FileName, IntroBackdropFiles.Extract(opening,
            flight ?? throw new InvalidDataException("The Ceres flight extraction wrote no palette.")));
        foreach ((string name, byte[] file) in CeresDestructionArtworkExtractor.Extract(bus))
        {
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
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
                IntroCinematicArtworkFormat.TileColumns, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, indexes, SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WritePage(string name, ReadOnlySpan<byte> native)
        {
            if (native.Length != IntroCinematicArtworkFormat.BackgroundPageByteCount)
                throw new InvalidDataException($"Intro tilemap {name} must contain one $0800-byte page.");
            byte[] encoded = RoomBackgroundTilemapExtractor.Encode(native);
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteFile(string name, byte[] encoded)
        {
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        IntroCinematicPalette WritePalette()
        {
            var native = new byte[CgramPalettes.ByteCount];
            for (int index = 0; index < native.Length; index++)
                native[index] = bus.ReadCartridgeByte(IntroCinematicSourceDefinitions.Palette + index);
            var colors = new PaletteRgb5[CgramPalettes.ColorCount];
            for (int index = 0; index < colors.Length; index++)
            {
                int word = BinaryPrimitives.ReadUInt16LittleEndian(
                    native.AsSpan(index * sizeof(ushort)));
                if (!RgbColor.IsColorWord(checked((ushort)word)))
                    throw new InvalidDataException($"Opening palette color {index} has an unrepresentable high bit.");
                colors[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            using var json = new MemoryStream();
            IntroCinematicPaletteFile.Write(json, new IntroCinematicPaletteDocument
            {
                Version = IntroCinematicPaletteFile.Version,
                Colors = colors,
            });
            byte[] encoded = json.ToArray();
            WriteFile(IntroCinematicPaletteFile.FileName, encoded);
            return IntroCinematicPaletteFile.Read(new MemoryStream(encoded, writable: false));
        }

        void WriteFinalLine()
        {
            var cells = new RoomBackgroundTilemapCell[IntroFinalLineTilemapFormat.CellCount];
            for (int index = 0; index < cells.Length; index++)
            {
                int source = IntroCinematicSourceDefinitions.FinalTextLine + index * sizeof(ushort);
                var word = new SnesBgTilemapWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(source));
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
            string name = IntroFinalLineTilemapFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteEyeFrames()
        {
            var frames = new IntroEyeTilemapFrame[IntroEyeTilemapFormat.FrameCount];
            for (int frame = 0; frame < frames.Length; frame++)
            {
                int pointer = FrontendSourceDefinitions.IntroEyeFrameRecords +
                    frame * FrontendSourceDefinitions.IntroEyeFrameRecordStride;
                int source = (int)SnesAddress.InBank(IntroCinematicSourceDefinitions.SpritemapBank, pointer);
                int function = CartridgeImportSource.Require(bus).ReadCartridgeWord(source);
                if (function != IntroCinematicSourceDefinitions.PortraitTilemapDrawFunction ||
                    bus.ReadCartridgeByte(source + 2) != IntroEyeTilemapFormat.Columns ||
                    bus.ReadCartridgeByte(source + 3) != IntroEyeTilemapFormat.Rows)
                    throw new InvalidDataException($"Opening eye frame {frame} changed its native draw shape.");
                var cells = new RoomBackgroundTilemapCell[IntroEyeTilemapFormat.CellsPerFrame];
                for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                {
                    int wordSource = source + 4 + cellIndex * sizeof(ushort);
                    var word = new SnesBgTilemapWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(wordSource));
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
            string name = IntroEyeTilemapFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteCaretSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in IntroCaretSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
            using var json = new MemoryStream();
            IntroCaretSpritePresentation.Write(json, new IntroCaretSpriteDocument
            {
                Version = IntroCaretSpriteVersion.Current.Revision,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            string name = IntroCaretSpriteFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteNextPageArrowSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in IntroNextPageArrowSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
            using var json = new MemoryStream();
            IntroNextPageArrowSpritePresentation.Write(json, new IntroNextPageArrowSpriteDocument
            {
                Version = IntroNextPageArrowSpriteFormat.Version,
                Frames = frames,
            });
            WriteFile(IntroNextPageArrowSpriteFormat.FileName, json.ToArray());
        }

        void WriteMotherBrainSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in
                IntroMotherBrainSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), IntroMotherBrainSpriteDefinitions.StockPartCount,
                    definition.Name));
            using var json = new MemoryStream();
            IntroMotherBrainSpritePresentation.Write(json, new IntroMotherBrainSpriteDocument
            {
                Version = IntroMotherBrainSpriteFormat.Version,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            string name = IntroMotherBrainSpriteFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteMotherBrainExplosionSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in
                IntroMotherBrainExplosionSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
            using var json = new MemoryStream();
            IntroMotherBrainExplosionSpritePresentation.Write(json,
                new IntroMotherBrainExplosionSpriteDocument
                {
                    Version = IntroMotherBrainExplosionSpriteFormat.Version,
                    Frames = frames,
                });
            byte[] encoded = json.ToArray();
            string name = IntroMotherBrainExplosionSpriteFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteRinkaSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in IntroRinkaSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), IntroRinkaSpriteDefinitions.StockPartCount,
                    definition.Name));
            using var json = new MemoryStream();
            IntroRinkaSpritePresentation.Write(json, new IntroRinkaSpriteDocument
            {
                Version = IntroRinkaSpriteFormat.Version,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            string name = IntroRinkaSpriteFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteEggEffectSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in
                IntroEggEffectSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), IntroEggEffectSpriteDefinitions.StockPartCount,
                    definition.Name));
            using var json = new MemoryStream();
            IntroEggEffectSpritePresentation.Write(json, new IntroEggEffectSpriteDocument
            {
                Version = IntroEggEffectSpriteFormat.Version,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            string name = IntroEggEffectSpriteFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteDiscoveryActorSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in
                IntroDiscoveryActorSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
            using var json = new MemoryStream();
            IntroDiscoveryActorSpritePresentation.Write(json,
                new IntroDiscoveryActorSpriteDocument
                {
                    Version = IntroDiscoveryActorSpriteFormat.Version,
                    Frames = frames,
                });
            byte[] encoded = json.ToArray();
            string name = IntroDiscoveryActorSpriteFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }

        void WriteScientistSprites()
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
            foreach (CinematicSpriteFrameDefinition definition in
                IntroScientistSpriteDefinitions.Frames)
                frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(bus,
                    CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
            using var json = new MemoryStream();
            IntroScientistSpritePresentation.Write(json, new IntroScientistSpriteDocument
            {
                Version = IntroScientistSpriteFormat.Version,
                Frames = frames,
            });
            byte[] encoded = json.ToArray();
            string name = IntroScientistSpriteFormat.FileName;
            using (var output = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                output.Write(encoded);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(encoded)));
        }
    }

    /// <summary>
    /// Checks the manifest's provenance and coverage, then loads the opening's source artwork, which only the import reads:
    /// every installed document as installed. No character page (#1636) or palette (#1544, #1637) is read: they are import
    /// data.
    /// </summary>
    public static IntroCinematicSourceArtwork Load(string directory) => new(ReadParts(directory));

    /// <summary>
    /// Checks the manifest's provenance and coverage, then loads the opening as the import composes from it: the game's
    /// artwork with its characters from the component's import data <paramref name="importData"/> (#1636) and, from the
    /// same import data (#1637), the opening's and the Ceres approach's colours, which only the import pre-colours sheets in
    /// (#1544).
    /// </summary>
    /// <param name="directory">The component's directory, holding the manifest and the documents.</param>
    /// <param name="importData">The component's import-data directory, holding the character pages and the palettes.</param>
    public static ImportIntroCinematicArtwork LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        IntroCinematicArtworkParts parts = ReadParts(directory);
        return new(parts, ReadSprites(importData), ReadLayouts(importData), ReadCharacters(importData, parts.CeresFlight),
            ReadColours(importData, IntroCinematicPaletteFile.FileName, "opening palette", IntroCinematicPaletteFile.Read),
            ReadColours(importData, CeresFlightPaletteFormat.FileName, "Ceres flight palette", CeresFlightPaletteFile.Read));
    }

    /// <summary>The opening's and the Ceres destruction scene's compositions from the import data <paramref name="importData"/> (#1638).</summary>
    /// <exception cref="InvalidDataException">A composition document does not decode; the error names its file.</exception>
    private static IntroCinematicSprites ReadSprites(string importData)
    {
        return new IntroCinematicSprites(
            Sprites(IntroCaretSpriteFormat.FileName, "opening caret sprites", IntroCaretSpritePresentation.Load),
            Sprites(IntroNextPageArrowSpriteFormat.FileName, "intro next-page arrow sprites", IntroNextPageArrowSpritePresentation.Load),
            Sprites(IntroMotherBrainSpriteFormat.FileName, "intro Mother Brain sprites", IntroMotherBrainSpritePresentation.Load),
            Sprites(IntroMotherBrainExplosionSpriteFormat.FileName, "intro Mother Brain explosion sprites", IntroMotherBrainExplosionSpritePresentation.Load),
            Sprites(IntroRinkaSpriteFormat.FileName, "intro Rinka sprites", IntroRinkaSpritePresentation.Load),
            Sprites(IntroEggEffectSpriteFormat.FileName, "intro egg effect sprites", IntroEggEffectSpritePresentation.Load),
            Sprites(IntroDiscoveryActorSpriteFormat.FileName, "intro discovery actor sprites", IntroDiscoveryActorSpritePresentation.Load),
            Sprites(IntroScientistSpriteFormat.FileName, "intro scientist sprite artwork", IntroScientistSpritePresentation.Load),
            Sprites(CeresDestructionSpriteFormat.FileName, "destruction sprites", CeresDestructionSpritePresentation.Load));

        T Sprites<T>(string name, string description, Func<Stream, T> read)
        {
            string path = Path.Combine(importData, name);
            try
            {
                return read(new MemoryStream(File.ReadAllBytes(path), writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid {description} {path}: {error.Message}", error);
            }
        }
    }

    /// <summary>The opening's layouts from the import data <paramref name="importData"/> (#1638).</summary>
    /// <exception cref="InvalidDataException">A layout does not decode; the error names its file.</exception>
    private static IntroCinematicLayouts ReadLayouts(string importData)
    {
        return new IntroCinematicLayouts(
            [.. Enumerable.Range(0, IntroCinematicArtworkFormat.BackgroundPageCount)
                .Select(page => Page(IntroCinematicArtworkFormat.BackgroundPageFileName(page), IntroCinematicArtworkFormat.BackgroundPageByteCount))],
            Page(IntroCinematicArtworkFormat.PortraitTilemapFileName, IntroCinematicArtworkFormat.BackgroundPageByteCount),
            Page(IntroCinematicArtworkFormat.InitialNarrationTilemapFileName, IntroCinematicArtworkFormat.BackgroundPageByteCount),
            Layout(IntroFinalLineTilemapFormat.FileName, "opening divider", IntroFinalLineTilemap.Load),
            Layout(IntroEyeTilemapFormat.FileName, "opening eye art", IntroEyeTilemapPresentation.Load),
            Page(ZebesRevealArtworkFormat.MapFileName, ZebesRevealArtworkFormat.MapByteCount));

        RoomBackgroundTilemapAtlas Page(string name, int byteCount) =>
            Layout(name, "intro tilemap", stream => RoomBackgroundTilemapAtlas.Load(stream, byteCount));

        T Layout<T>(string name, string description, Func<MemoryStream, T> read)
        {
            string path = Path.Combine(importData, name);
            try
            {
                return read(new MemoryStream(File.ReadAllBytes(path), writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid {description} {path}: {error.Message}", error);
            }
        }
    }

    /// <summary>
    /// The opening's characters from the import's pages in <paramref name="importData"/>, the approach's, with its
    /// compositions, bound to <paramref name="flight"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">A page does not decode; the error names its file.</exception>
    private static IntroCinematicCharacters ReadCharacters(string importData, CeresFlightSourceArtwork flight)
    {
        string floorPath = Path.Combine(importData, CeresFlightArtworkFileFormat.FloorFileName);
        string objectPath = Path.Combine(importData, CeresFlightArtworkFileFormat.ObjectFileName);
        string spritesPath = Path.Combine(importData, CeresFlightSpriteFormat.FileName);
        CeresFlightCharacters ceresFlight;
        try
        {
            ceresFlight = CeresFlightCharacters.Read(new MemoryStream(File.ReadAllBytes(floorPath), writable: false),
                new MemoryStream(File.ReadAllBytes(objectPath), writable: false),
                new MemoryStream(File.ReadAllBytes(spritesPath), writable: false), flight);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid Ceres flight characters ({floorPath}, {objectPath}, {spritesPath}): {error.Message}", error);
        }
        return new IntroCinematicCharacters(
            Sheet(IntroCinematicArtworkFormat.BackgroundFileName, IntroCinematicArtworkFormat.BackgroundByteCount),
            Sheet(IntroCinematicArtworkFormat.IntroObjectFileName, IntroCinematicArtworkFormat.IntroObjectByteCount),
            Sheet(IntroCinematicArtworkFormat.CinematicObjectFileName, IntroCinematicArtworkFormat.CinematicObjectByteCount),
            Sheet(ZebesRevealArtworkFormat.CharacterFileName, ZebesRevealArtworkFormat.CharacterByteCount),
            ceresFlight);

        RoomCharacterAtlas Sheet(string name, int nativeByteCount)
        {
            string path = Path.Combine(importData, name);
            try
            {
                return RoomCharacterAtlasPng.Read(new MemoryStream(File.ReadAllBytes(path), writable: false), nativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid intro page {path}: {error.Message}", error);
            }
        }
    }

    /// <summary>
    /// Checks the manifest's provenance and coverage, then loads what the game binds of the opening (#1616): its backdrops and
    /// the Ceres scenes' actor placements, each read as installed. The game draws every picture from the installed sheets, so
    /// it reads none of the artwork they are composed from.
    /// </summary>
    public static IntroCinematicArtwork LoadCinematic(string directory)
    {
        RequireManifest(directory);
        IntroBackdrops backdrops = ReadBackdrops(directory);
        return new IntroCinematicArtwork(backdrops.Opening, new CeresFlightArtwork(backdrops.CeresFlight, ReadFlightActors(directory)),
            ReadCeresDestruction(directory));
    }

    /// <summary>The colours extracted as <paramref name="name"/> in the import data <paramref name="directory"/>, read by <paramref name="read"/>.</summary>
    /// <exception cref="InvalidDataException">The document does not decode; the error names its file.</exception>
    private static T ReadColours<T>(string directory, string name, string description, Func<Stream, T> read)
    {
        string path = Path.Combine(directory, name);
        try
        {
            return read(new MemoryStream(File.ReadAllBytes(path), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid {description} {path}: {error.Message}", error);
        }
    }

    /// <summary>Checks that the manifest in <paramref name="directory"/> names this cartridge and every installed file.</summary>
    /// <exception cref="InvalidDataException">The manifest does not decode or does not describe this installation.</exception>
    private static void RequireManifest(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, IntroCinematicArtworkFormat.ManifestFileName);
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
    }

    /// <summary>Checks the manifest's provenance and coverage, then reads every installed document the import composes from as installed.</summary>
    private static IntroCinematicArtworkParts ReadParts(string directory)
    {
        RequireManifest(directory);
        IntroBackdrops backdrops = ReadBackdrops(directory);
        return new IntroCinematicArtworkParts { Backdrop = backdrops.Opening, CeresFlight = LoadCeresFlight(), CeresDestruction = ReadCeresDestruction(directory), CeresDestructionMaps = LoadCeresDestructionMaps() };

        CeresFlightSourceArtwork LoadCeresFlight()
        {
            (string mapPath, byte[] map) = ReadInstalled(CeresFlightArtworkFileFormat.MapFileName);
            (string actorsPath, byte[] actors) = ReadInstalled(CeresFlightActorLayoutFormat.FileName);
            try
            {
                return CeresFlightSourceArtwork.Read(
                    new MemoryStream(map, writable: false),
                    backdrops.CeresFlight,
                    new MemoryStream(actors, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Ceres flight artwork ({mapPath}, {actorsPath}): {error.Message}", error);
            }
        }

        CeresDestructionMaps LoadCeresDestructionMaps()
        {
            (string path, byte[] selected) = ReadInstalled(CeresDestructionMapFormat.FileName);
            try
            {
                return CeresDestructionMaps.Load(new MemoryStream(selected, writable: false));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid destruction maps {path}: {error.Message}", error);
            }
        }

        SelectedArtworkFile ReadInstalled(string name)
        {
            string installedPath = Path.Combine(directory, name);
            return new SelectedArtworkFile(installedPath, File.ReadAllBytes(installedPath));
        }
    }

    /// <summary>The opening's and the Ceres scenes' installed backdrops.</summary>
    /// <exception cref="InvalidDataException">The backdrop document does not decode; the error names its file.</exception>
    private static IntroBackdrops ReadBackdrops(string directory)
    {
        (string path, byte[] selected) = ReadInstalledFile(directory, IntroBackdropFiles.FileName);
        try
        {
            return IntroBackdropFiles.Read(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid opening backdrops {path}: {error.Message}", error);
        }
    }

    /// <summary>The Ceres approach's installed rear-view actor placements.</summary>
    /// <exception cref="InvalidDataException">The placement document does not decode; the error names its file.</exception>
    private static CeresFlightActorPlacements ReadFlightActors(string directory)
    {
        (string path, byte[] selected) = ReadInstalledFile(directory, CeresFlightActorLayoutFormat.FileName);
        try
        {
            return CeresFlightActorLayoutFile.Read(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid Ceres flight actors {path}: {error.Message}", error);
        }
    }

    /// <summary>The destruction and Zebes-reveal actors' installed placements.</summary>
    /// <exception cref="InvalidDataException">A placement document does not decode; the error names both files.</exception>
    private static CeresDestructionArtwork ReadCeresDestruction(string directory)
    {
        (string actorsPath, byte[] actors) = ReadInstalledFile(directory, CeresRevealActorLayoutFormat.FileName);
        (string destructionActorsPath, byte[] destructionActors) = ReadInstalledFile(directory, CeresDestructionActorLayoutFormat.FileName);
        try
        {
            return CeresDestructionArtworkFile.Read(
                new MemoryStream(actors, writable: false),
                new MemoryStream(destructionActors, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid destruction artwork ({actorsPath}, {destructionActorsPath}): {error.Message}",
                error);
        }
    }

    /// <summary>The bytes installed as <paramref name="name"/> in <paramref name="directory"/>, with the path they were read from.</summary>
    private static SelectedArtworkFile ReadInstalledFile(string directory, string name)
    {
        string installedPath = Path.Combine(directory, name);
        return new SelectedArtworkFile(installedPath, File.ReadAllBytes(installedPath));
    }

    /// <summary>Lists every resource that must be present and hashed by the intro artwork manifest.</summary>
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
        IntroNextPageArrowSpriteFormat.FileName,
        IntroMotherBrainSpriteFormat.FileName,
        IntroMotherBrainExplosionSpriteFormat.FileName,
        IntroRinkaSpriteFormat.FileName,
        IntroEggEffectSpriteFormat.FileName,
        IntroDiscoveryActorSpriteFormat.FileName,
        IntroScientistSpriteFormat.FileName,
        .. Enumerable.Range(0, IntroCinematicArtworkFormat.BackgroundPageCount)
            .Select(IntroCinematicArtworkFormat.BackgroundPageFileName),
        IntroCinematicPaletteFile.FileName,
        IntroBackdropFiles.FileName,
        CeresFlightArtworkFileFormat.FloorFileName,
        CeresFlightArtworkFileFormat.MapFileName,
        CeresFlightArtworkFileFormat.ObjectFileName,
        CeresFlightPaletteFormat.FileName,
        CeresFlightSpriteFormat.FileName,
        CeresFlightActorLayoutFormat.FileName,
        CeresDestructionMapFormat.FileName,
        ZebesRevealArtworkFormat.MapFileName,
        ZebesRevealArtworkFormat.CharacterFileName,
        CeresDestructionSpriteFormat.FileName,
        CeresRevealActorLayoutFormat.FileName,
        CeresDestructionActorLayoutFormat.FileName,
    ];

    /// <summary>Manifest JSON settings shared by extraction and installation.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Records resource hashes and cartridge provenance for one extracted stock artwork set.</summary>
    private sealed record IntroCinematicArtworkManifest(
        int Version, string SourceCartridgeSha256, Dictionary<string, string> StockSha256);
}
