using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Extracts Samus's split body DMA into indexed PNG atlases and selector JSON, which are the body component's import data
/// (#1636, #1638), and installs what the game binds of the body: each pose's art origin and every body frame the body sheet
/// pictures, with where that frame places the body.
/// </summary>
/// <remarks>
/// Each definition occupies sixteen 4-bpp tiles (64 by 16 pixels) in its set's PNG.
/// Unused tiles are zero. Native transfer sizes and pose/frame choices live in the extracted JSON;
/// no game mechanics or animation delay bytes are represented here.
/// </remarks>
public static class SamusBodyArtworkFiles
{
    /// <summary>
    /// Installed body JSON filename (#1638): each pose's art origin and every body frame the body sheet pictures, with where
    /// that frame places the body, which is what the game binds of the body and what names the body sheet's frames.
    /// </summary>
    public const string ManifestFileName = "samus-body.json";
    /// <summary>
    /// Import-data JSON filename of the extracted body (#1638): split-DMA definitions, pose/frame selectors, spritemaps, whose
    /// parts are packed sprite placements, body placements, and the body pages' provenance hashes.
    /// </summary>
    public const string ArtworkFileName = "samus-body-artwork.json";
    /// <summary>
    /// Manifest revision containing source hashes, selectors, spritemaps stored as sprite parts, and body placements.
    /// Revision nine holds each pose's placement and its frames' own placements instead of the cartridge's offset tables
    /// (#1517). Revision ten installs with the body sheet that pictures the glitch balls, their knockbacks, the
    /// Draygon-held ball and its stand-up, which the Baby's Hyper Beam freeze and rainbow stop can install (#1535).
    /// Revision eleven keys each pose's frame list and half-table bases by the pose's name rather than its place in the
    /// cartridge's pose tables, and installs none for the records no pose names (#1595). Revision twelve installs no
    /// character page: the body halves, the arm cannon's cover tiles and the death explosion's characters are import
    /// data (#1636). Revision thirteen installs only what the game binds of the body: the extracted manifest, with its
    /// spritemaps, and the atmospheric lists and the arm cannon's appearances are import data (#1638).
    /// </summary>
    private const int FormatVersion = 13;
    /// <summary>Each split-DMA definition occupies eight 8x8 tiles across its PNG row.</summary>
    private const int TileWidth = 64;
    /// <summary>Two tile rows reserve sixteen graphics tiles for one DMA definition.</summary>
    private const int DefinitionHeight = 16;

    /// <summary>Serializes strict camel-case manifests and rejects unknown JSON members.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>
    /// Writes Samus's upper/lower body PNG sets and visual selector JSON to the import data, together with linked
    /// atmospheric, death, and arm-cannon artwork, and installs what the game binds of the body (#1638).
    /// </summary>
    /// <param name="bus">Non-null cartridge address space supplying native body DMA definitions, pose/frame tables, spritemaps, offsets, and linked artwork.</param>
    /// <param name="directory">Stock output directory, created if needed; the installed body JSON is overwritten and the body pages and the extracted JSON go to its import-data directory (#1636, #1638), while linked extractors retain their own file-creation policies.</param>
    /// <param name="sourceCartridgeSha256">Source cartridge SHA-256 recorded in body and linked manifests; installed loading requires the supported revision.</param>
    /// <remarks>Each DMA definition uses a zero-padded 64x16 four-bit character slot. Gameplay and animation-delay bytes are not exported.</remarks>
    /// <exception cref="InvalidDataException">Native DMA bounds, sizes, spritemaps, or linked cartridge sources are invalid.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        ushort[] topPointers = ReadPointers(bus,
            SamusSourceDefinitions.TopDefinitionListPointers.Value,
            SamusBodySources.TopSetCount);
        ushort[] bottomPointers = ReadPointers(bus,
            SamusSourceDefinitions.BottomDefinitionListPointers.Value,
            SamusBodySources.BottomSetCount);
        ushort[] posePointers = ReadPointers(bus,
            SamusSourceDefinitions.AnimationDefinitionListPointers.Value,
            SamusBodySources.PoseCount);
        int[] topSetStarts = [.. topPointers.Select(pointer => BodyRecord(pointer))];
        int[] bottomSetStarts = [.. bottomPointers.Select(pointer => BodyRecord(pointer))];
        // The pose tables are read by the cartridge's pose numbers and installed by pose name (#1595).
        Dictionary<SamusPoseId, int> poseFirstFrames = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose,
            pose => BodyFrame(posePointers[SamusPoseNumbers.Number(pose)]));
        int[] graphicsYOffsets = Enumerable.Range(0, SamusBodySources.PoseCount)
            .Select(pose => (int)unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.PoseDefinitions.Value +
                pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4)))
            .ToArray();
        var frames = new SamusBodyFrameSelection[SamusBodySources.FrameCount];
        for (int index = 0; index < frames.Length; index++)
        {
            int address = SamusSourceDefinitions.BodyFrames.Value + index * 4;
            frames[index] = new SamusBodyFrameSelection(bus.ReadCartridgeByte(address),
                bus.ReadCartridgeByte(address + 1), bus.ReadCartridgeByte(address + 2), bus.ReadCartridgeByte(address + 3));
        }

        ushort[] allPointers = topPointers.Concat(bottomPointers).OrderBy(value => value).ToArray();
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        DefinitionEntry[][] top = ExtractHalf(bus, importData, topPointers, allPointers, true, hashes);
        DefinitionEntry[][] bottom = ExtractHalf(bus, importData, bottomPointers, allPointers, false, hashes);
        Dictionary<SamusPoseId, int> spritemapTopBases = ByPose(ReadPointers(bus,
            SamusSourceDefinitions.SpritemapTopBases.Value, SamusBodySources.PoseCount));
        Dictionary<SamusPoseId, int> spritemapBottomBases = ByPose(ReadPointers(bus,
            SamusSourceDefinitions.SpritemapBottomBases.Value, SamusBodySources.PoseCount));
        ushort[] spritemapPointers = ReadPointers(bus,
            SamusSourceDefinitions.SpritemapPointers.Value, SamusSpritemapSources.EntryCount);
        // Compositions install in record order, so a composition's identity is its place among the referenced records.
        ushort[] compositions = [.. spritemapPointers.Distinct().Where(pointer => pointer != 0).Order()];
        SpriteVisualPart[][] spritemaps = [.. compositions.Select(pointer => ReadSpritemap(bus, pointer))];
        int?[] spritemapEntries = [.. spritemapPointers.Select(pointer =>
            pointer == 0 ? (int?)null : Array.BinarySearch(compositions, pointer))];
        int[] landingOffsets = Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.LandingByteCount)
            .Select(index => (int)bus.ReadCartridgeByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + index)).ToArray();
        int[] postureOffsets = Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.PostureByteCount)
            .Select(index => (int)unchecked((sbyte)bus.ReadCartridgeByte(SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value + index))).ToArray();
        int[] drainedOffsets = Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.DrainedByteCount)
            .Select(index => (int)unchecked((sbyte)bus.ReadCartridgeByte(SamusSourceDefinitions.DrainedVerticalOffsets.Value + index))).ToArray();
        // The placement tables are decoded here, once; the installation holds each pose's and frame's placement (#1517).
        Dictionary<SamusPoseId, SamusBodyPosePlacement> placements =
            SamusBodyPlacementDecoding.Decode(graphicsYOffsets, landingOffsets, postureOffsets, drainedOffsets);
        var manifest = new Manifest(FormatVersion, sourceCartridgeSha256,
            topSetStarts, bottomSetStarts, poseFirstFrames, placements,
            frames, top, bottom, spritemapTopBases, spritemapBottomBases,
            spritemapEntries, spritemaps, hashes);
        SamusAtmosphericArtworkFiles.Extract(bus, importData, sourceCartridgeSha256);
        SamusDeathPaletteArtworkFiles.Extract(bus, directory, importData, sourceCartridgeSha256);
        SamusDeathTileArtworkFiles.Extract(bus, directory, importData, sourceCartridgeSha256);
        SamusArmCannonArtworkFiles.Extract(bus, directory, importData, sourceCartridgeSha256);
        File.WriteAllBytes(Path.Combine(importData, ArtworkFileName),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        // The import decides once which body frames the sheet pictures, from the frames her animation shows with what it
        // reads past its programs, and installs them with their placements: the game binds those and nothing else.
        LaidOutAnimationContinuation<SamusPoseId, SamusPoseAnimationStep> continuation =
            SamusOverreadServicesReader.ReadAnimationContinuation(SamusOverreadDecoding.Decode(CartridgeImportSource.Require(bus)));
        InstalledSamusBody installed = LoadForImport(directory, importData).InstalledBody(continuation);
        SamusArmCannonArtworkFiles.Install(directory, installed.ArmCannon);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new GameManifest(FormatVersion, sourceCartridgeSha256,
                SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose,
                    pose => new PoseFrames(installed.GraphicsYOffset(pose), installed.FramesOf(pose).ToDictionary()))), JsonOptions));
    }

    /// <summary>
    /// Validates and compiles what the game binds of the installed Samus body, as the player may have edited it (#1638):
    /// each pose's art origin, every body frame the body sheet pictures with where it places the body, her death steps and
    /// the arm cannon's layout. No import data is read.
    /// </summary>
    /// <param name="directory">Directory containing the supported-revision body JSON and the linked installed Samus documents.</param>
    /// <exception cref="InvalidDataException">The body JSON or a linked document is invalid; diagnostics retain the offending filename.</exception>
    public static InstalledSamusBody Load(string directory)
    {
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(directory, ManifestFileName));
        GameManifest manifest = manifestFile.Json<GameManifest>(JsonOptions);
        SuperMetroid.Core.Samus.Death.SamusDeathSteps deathSteps = SamusDeathPaletteArtworkFiles.LoadSteps(directory);
        SamusArmCannonLayout armCannon = SamusArmCannonArtworkFiles.Load(directory);
        return manifestFile.WithContext(() =>
        {
            if (manifest.Version != FormatVersion ||
                !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
                manifest.Poses is null || !SamusPoseNumbers.CoversRecordedPoses(manifest.Poses) ||
                manifest.Poses.Values.Any(pose => pose is null || pose.Frames is null))
                throw new InvalidDataException("Samus body manifest does not match this installation.");
            return new InstalledSamusBody(manifest.Poses.ToDictionary(pose => pose.Key, pose => pose.Value.GraphicsYOffset),
                manifest.Poses.ToDictionary(pose => pose.Key, pose => (IReadOnlyDictionary<int, int>)pose.Value.Frames),
                deathSteps, armCannon);
        });
    }

    /// <summary>
    /// Validates and compiles the Samus body as the import reads it (#1638): the extracted body JSON, atmosphere and arm cannon
    /// from the import data <paramref name="importData"/>, and the installed death steps in <paramref name="directory"/>. No
    /// character page is read (#1636).
    /// </summary>
    /// <param name="directory">The component directory, holding the installed death documents and the arm cannon's manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the extracted body, atmosphere and arm cannon.</param>
    /// <exception cref="InvalidDataException">Metadata or linked artwork are invalid; diagnostics retain the offending filename.</exception>
    public static SamusBodySources LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(importData, ArtworkFileName));
        Manifest manifest = manifestFile.Json<Manifest>(JsonOptions);
        return manifestFile.WithContext(() =>
        {
            ValidateManifest(manifest);
            return new SamusBodySources(Parts(directory, importData, manifest));
        });
    }

    /// <summary>
    /// Validates and compiles the Samus body as the import reads it and its characters, from which the body sheets are
    /// composed: the extracted documents and the body pages in the import data <paramref name="importData"/> (#1636, #1638).
    /// </summary>
    /// <param name="directory">The component directory, holding the installed death documents and the arm cannon's manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the extracted documents and the body pages.</param>
    /// <returns>The split-DMA characters, bound to the body catalog.</returns>
    /// <remarks>Transfer sizes and references are validated, and unused page-slot tiles must stay zero.</remarks>
    /// <exception cref="InvalidDataException">Metadata, sizes, page padding, or linked artwork are invalid; diagnostics retain the offending filename.</exception>
    public static SamusBodyCharacterArtwork LoadCharacters(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(importData, ArtworkFileName));
        Manifest manifest = manifestFile.Json<Manifest>(JsonOptions);
        return manifestFile.WithContext(() =>
        {
            ValidateManifest(manifest);
            return SamusBodyCharacterArtwork.Create(Parts(directory, importData, manifest),
                LoadHalf(importData, manifest, true), LoadHalf(importData, manifest, false));
        });
    }

    /// <summary>Exports each upper or lower DMA set as a padded page into <paramref name="importData"/> and records its native source sizes.</summary>
    private static DefinitionEntry[][] ExtractHalf(ISnesAddressSpace bus, string importData,
        ushort[] pointers, ushort[] sortedPointers, bool upperHalf,
        Dictionary<string, string> hashes)
    {
        var sets = new DefinitionEntry[pointers.Length][];
        for (int set = 0; set < pointers.Length; set++)
        {
            int start = pointers[set];
            int order = Array.IndexOf(sortedPointers, pointers[set]);
            int end = order + 1 == sortedPointers.Length ? SamusSourceDefinitions.FirstBodyDefinition + SamusBodySources.RecordCount * SamusSourceDefinitions.BodyDefinitionBytes : sortedPointers[order + 1];
            if (start < SnesBusLayout.RomWindowStart || end <= start || (end - start) % 7 != 0)
                throw new InvalidDataException($"Samus body set {set:X2} has invalid native bounds.");
            int count = (end - start) / 7;
            byte[] planar = new byte[count * SamusBodyCharacterArtwork.BytesPerDefinitionSlot];
            sets[set] = new DefinitionEntry[count];
            for (int position = 0; position < count; position++)
            {
                int definitionAddress = CartridgeBankWindow.At(SamusSourceDefinitions.AnimationBank, start + position * 7);
                int sourceOffset = CartridgeImportSource.Require(bus).ReadCartridgeWord(definitionAddress);
                int sourceBank = bus.ReadCartridgeByte(definitionAddress + 2);
                int firstSize = CartridgeImportSource.Require(bus).ReadCartridgeWord((definitionAddress + 3));
                int secondSize = CartridgeImportSource.Require(bus).ReadCartridgeWord((definitionAddress + 5));
                int byteCount = firstSize + secondSize;
                if (sourceOffset < SnesBusLayout.RomWindowStart || firstSize == 0 || byteCount >
                    SamusBodyCharacterArtwork.BytesPerDefinitionSlot || byteCount % 32 != 0 ||
                    sourceOffset + byteCount > CartridgeFormatDefinitions.BankByteCount)
                    throw new InvalidDataException($"Invalid Samus body DMA at ${definitionAddress:X6}.");
                int sourceAddress = ((int)SnesAddress.InBank(sourceBank, sourceOffset));
                for (int i = 0; i < byteCount; i++)
                    planar[position * SamusBodyCharacterArtwork.BytesPerDefinitionSlot + i] =
                        bus.ReadCartridgeByte(sourceAddress + i);
                sets[set][position] = new DefinitionEntry(firstSize, secondSize);
            }
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, TileWidth / 8,
                out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            string name = FileName(upperHalf, set);
            byte[] bytes = png.ToArray();
            File.WriteAllBytes(Path.Combine(importData, name), bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        return sets;
    }

    /// <summary>The body's tables with its spritemaps and all linked Samus document catalogs, as the import reads them.</summary>
    private static SamusBodyArtworkParts Parts(string directory, string importData, Manifest manifest)
    {
        var spritemaps = new SamusSpritemapSources(manifest.SpritemapTopBases,
            manifest.SpritemapBottomBases,
            [.. manifest.SpritemapEntries.Select(entry => entry is { } ordinal ? new SamusSpritemapId(ordinal) : (SamusSpritemapId?)null)],
            [.. manifest.Spritemaps.Select(parts => new SamusSpritemapDefinition(
                [.. parts.Select(part => part.ToSpritePart(SpriteArtwork.CharactersPerRow))]))]);
        SamusAtmosphericSourceArtwork atmosphere = SamusAtmosphericArtworkFiles.Load(importData);
        SuperMetroid.Core.Samus.Death.SamusDeathSteps deathSteps = SamusDeathPaletteArtworkFiles.LoadSteps(directory);
        SamusArmCannonSourceArtwork armCannon = SamusArmCannonArtworkFiles.LoadForImport(directory, importData);
        return new SamusBodyArtworkParts { TopSetStarts = manifest.TopSetStarts, BottomSetStarts = manifest.BottomSetStarts, PoseFirstFrames = manifest.PoseFirstFrames, Placements = manifest.Placements, Frames = manifest.Frames, Spritemaps = spritemaps, Atmosphere = atmosphere, DeathSteps = deathSteps, ArmCannon = armCannon };
    }

    /// <summary>Re-encodes the page sets in <paramref name="importData"/> into native planar bytes and rejects painted padding tiles.</summary>
    private static SamusBodyTileTransfer[][] LoadHalf(string importData, Manifest manifest, bool upperHalf)
    {
        DefinitionEntry[][] metadata = upperHalf ? manifest.Top : manifest.Bottom;
        var result = new SamusBodyTileTransfer[metadata.Length][];
        for (int set = 0; set < metadata.Length; set++)
        {
            string name = FileName(upperHalf, set);
            if (!manifest.Hashes.ContainsKey(name))
                throw new InvalidDataException($"Samus body manifest omits {name}.");
            // Sizes belong to JSON, not to the PNG codec. Reject them under the
            // manifest's context before reading any image that uses the sizes.
            foreach (DefinitionEntry entry in metadata[set])
                if (entry is null || entry.FirstSize == 0 ||
                    entry.FirstSize + entry.SecondSize > SamusBodyCharacterArtwork.BytesPerDefinitionSlot ||
                    (entry.FirstSize + entry.SecondSize) % 32 != 0)
                    throw new InvalidDataException($"Samus body definition {name} has invalid sizes.");
            SamusArtworkFile selected = SamusArtworkFile.Read(Path.Combine(importData, name));
            int height = checked(metadata[set].Length * DefinitionHeight);
            result[set] = selected.Compile(stream => DecodeSet(stream));

            SamusBodyTileTransfer[] DecodeSet(Stream stream)
            {
                IndexedPngImage image = IndexedPng.Read(stream, new(TileWidth, checked((ushort)height)));
                byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, new(TileWidth, checked((ushort)height)), 4);
                var definitions = new SamusBodyTileTransfer[metadata[set].Length];
                for (int position = 0; position < definitions.Length; position++)
                {
                    DefinitionEntry entry = metadata[set][position];
                    int byteCount = entry.FirstSize + entry.SecondSize;
                    ReadOnlySpan<byte> slot = planar.AsSpan(
                        position * SamusBodyCharacterArtwork.BytesPerDefinitionSlot,
                        SamusBodyCharacterArtwork.BytesPerDefinitionSlot);
                    if (slot[byteCount..].IndexOfAnyExcept((byte)0) >= 0)
                        throw new InvalidDataException($"Samus body PNG {name} paints unused tiles in definition {position}.");
                    definitions[position] = new SamusBodyTileTransfer(
                        checked((ushort)entry.FirstSize), checked((ushort)entry.SecondSize),
                        slot[..byteCount].ToArray());
                }
                return definitions;
            }
        }
        return result;
    }

    /// <summary>Checks source revision, manifest version, required tables, and nonempty DMA sets.</summary>
    private static void ValidateManifest(Manifest manifest)
    {
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) || manifest.TopSetStarts is null ||
            manifest.BottomSetStarts is null || manifest.PoseFirstFrames is null ||
            manifest.Placements is null ||
            manifest.Frames is null || manifest.Top is null || manifest.Bottom is null ||
            manifest.SpritemapTopBases is null || manifest.SpritemapBottomBases is null ||
            manifest.SpritemapEntries is null || manifest.Spritemaps is null ||
            manifest.Hashes is null || manifest.Top.Length != SamusBodySources.TopSetCount ||
            manifest.Bottom.Length != SamusBodySources.BottomSetCount ||
            !SamusPoseNumbers.CoversRecordedPoses(manifest.Placements) ||
            !SamusPoseNumbers.CoversRecordedPoses(manifest.PoseFirstFrames) ||
            !SamusPoseNumbers.CoversRecordedPoses(manifest.SpritemapTopBases) ||
            !SamusPoseNumbers.CoversRecordedPoses(manifest.SpritemapBottomBases) ||
            manifest.Placements.Values.Any(placement => placement is null) ||
            manifest.Top.Any(set => set is null || set.Length == 0 || set.Any(entry => entry is null)) ||
            manifest.Bottom.Any(set => set is null || set.Length == 0 || set.Any(entry => entry is null)) ||
            manifest.Spritemaps.Any(entry => entry is null || entry.Any(part => part is null)))
            throw new InvalidDataException("Samus body manifest does not match this installation.");
    }

    /// <summary>The entry of <paramref name="table"/>, laid out by the cartridge's pose numbers, for each pose with a record.</summary>
    private static Dictionary<SamusPoseId, int> ByPose(ushort[] table) =>
        SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose => (int)table[SamusPoseNumbers.Number(pose)]);

    /// <summary>Reads a contiguous little-endian pointer table from the cartridge.</summary>
    private static ushort[] ReadPointers(ISnesAddressSpace bus, int start, int count) =>
        Enumerable.Range(0, count).Select(i => CartridgeImportSource.Require(bus).ReadCartridgeWord((start + i * 2))).ToArray();

    /// <summary>The record of the body DMA definition at bank-$92 <paramref name="pointer"/>.</summary>
    private static int BodyRecord(int pointer) =>
        pointer >= SamusSourceDefinitions.FirstBodyDefinition && (pointer - SamusSourceDefinitions.FirstBodyDefinition) % SamusSourceDefinitions.BodyDefinitionBytes == 0
            ? (pointer - SamusSourceDefinitions.FirstBodyDefinition) / SamusSourceDefinitions.BodyDefinitionBytes
            : throw new InvalidDataException($"Samus body definition group ${pointer:X4} is not a record.");

    /// <summary>The frame selector at bank-$92 <paramref name="pointer"/>.</summary>
    private static int BodyFrame(int pointer) =>
        pointer >= SamusSourceDefinitions.FirstBodyFrame && (pointer - SamusSourceDefinitions.FirstBodyFrame) % 4 == 0
            ? (pointer - SamusSourceDefinitions.FirstBodyFrame) / 4
            : throw new InvalidDataException($"Samus pose frame list ${pointer:X4} is not a frame selector.");

    /// <summary>Imports a bounded bank-$92 Samus map, decoding its five-byte records into stored sprite parts.</summary>
    private static SpriteVisualPart[] ReadSpritemap(ISnesAddressSpace bus, int pointer)
    {
        int address = CartridgeBankWindow.At(SamusSourceDefinitions.AnimationBank, pointer);
        int count = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
        if (pointer < SamusSourceDefinitions.FirstSpritemap || count > 128 || pointer + 2 + count * 5 > CartridgeFormatDefinitions.BankByteCount)
            throw new InvalidDataException($"Invalid Samus spritemap at $92:{pointer:X4}.");
        var parts = new SpriteVisualPart[count];
        for (int i = 0; i < count; i++)
        {
            int part = address + 2 + i * 5;
            var record = new SpritemapPart(new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(part)),
                new SnesSpritemapYByte(bus.ReadCartridgeByte(part + 2)), CartridgeImportSource.Require(bus).ReadCartridgeWord(part + 3));
            parts[i] = SpriteVisualPart.FromSpritePart(record.ToPart(), SpriteArtwork.CharactersPerRow);
        }
        return parts;
    }

    /// <summary>Names the upper or lower body page by native set index.</summary>
    private static string FileName(bool upperHalf, int set) =>
        $"{(upperHalf ? "top" : "bottom")}-{set:X2}.png";

    /// <summary>
    /// What the installed body JSON holds (#1638): each pose's art origin and the body frames the body sheet pictures.
    /// </summary>
    /// <param name="Version">The body JSON's revision.</param>
    /// <param name="SourceCartridgeSha256">The cartridge the body was extracted from.</param>
    /// <param name="Poses">Each pose with a record's art origin and pictured frames, keyed by the pose's name.</param>
    internal sealed record GameManifest(int Version, string SourceCartridgeSha256, Dictionary<SamusPoseId, PoseFrames> Poses);

    /// <summary>One pose's art origin and the body frames the body sheet pictures of it.</summary>
    /// <param name="GraphicsYOffset">
    /// The pose's art origin, -128 through 127: how many pixels above Samus's center the arm cannon, the Grapple beam, the
    /// charge flare and the speed-booster echoes are drawn.
    /// </param>
    /// <param name="Frames">
    /// Each pictured animation frame, 0 through 255: how many pixels above Samus's drawn point the frame's body origin sits,
    /// negative below it.
    /// </param>
    internal sealed record PoseFrames(int GraphicsYOffset, Dictionary<int, int> Frames);

    /// <summary>
    /// The extracted body, import data (#1638): split-DMA identities, pose selectors, each pose's and frame's body placement,
    /// spritemaps, and stock PNG hashes.
    /// </summary>
    internal sealed record Manifest(int Version, string SourceCartridgeSha256,
        int[] TopSetStarts, int[] BottomSetStarts, Dictionary<SamusPoseId, int> PoseFirstFrames,
        Dictionary<SamusPoseId, SamusBodyPosePlacement> Placements,
        SamusBodyFrameSelection[] Frames, DefinitionEntry[][] Top,
        DefinitionEntry[][] Bottom, Dictionary<SamusPoseId, int> SpritemapTopBases,
        Dictionary<SamusPoseId, int> SpritemapBottomBases, int?[] SpritemapEntries,
        SpriteVisualPart[][] Spritemaps,
        Dictionary<string, string> Hashes);

    /// <summary>Retains one native DMA source address and the two transfer lengths sharing its PNG slot.</summary>
    internal sealed record DefinitionEntry(int FirstSize, int SecondSize)
    {
        /// <summary>The first transfer's byte count, 0 through 65535 (a DMA size).</summary>
        public int FirstSize { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, FirstSize);

        /// <summary>The second transfer's byte count, 0 through 65535 (a DMA size).</summary>
        public int SecondSize { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SecondSize);
    }
}
