using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts Samus's split body DMA into indexed PNG atlases and editable selector JSON.</summary>
/// <remarks>
/// Each definition occupies sixteen 4-bpp tiles (64 by 16 pixels) in its set's PNG.
/// Unused tiles are zero. Native transfer sizes and pose/frame choices live in JSON;
/// no game mechanics or animation delay bytes are represented here.
/// </remarks>
public static class SamusBodyArtworkFiles
{
    /// <summary>Body-art JSON filename containing split-DMA definitions, pose/frame selectors, spritemaps, vertical offsets, and stock PNG provenance hashes.</summary>
    public const string ManifestFileName = "samus-body.json";
    /// <summary>Manifest revision containing source hashes, selectors, spritemaps stored as sprite parts, and vertical offsets.</summary>
    private const int FormatVersion = 7;
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

    /// <summary>Writes Samus's upper/lower body PNG sets and visual selector JSON, together with linked atmospheric, death, and arm-cannon artwork.</summary>
    /// <param name="bus">Non-null cartridge address space supplying native body DMA definitions, pose/frame tables, spritemaps, offsets, and linked artwork.</param>
    /// <param name="directory">Stock output directory, created if needed; body PNGs and body JSON are overwritten, while linked extractors retain their own file-creation policies.</param>
    /// <param name="sourceCartridgeSha256">Source cartridge SHA-256 recorded in body and linked manifests; installed loading requires the supported revision.</param>
    /// <remarks>Each DMA definition uses a zero-padded 64x16 four-bit character slot. Builds the complete catalog before publishing body JSON to reject invalid visual references; gameplay and animation-delay bytes are not exported.</remarks>
    /// <exception cref="InvalidDataException">Native DMA bounds, sizes, spritemaps, visual references, or linked artwork fail validation.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        ushort[] topPointers = ReadPointers(bus,
            SamusSourceDefinitions.TopDefinitionListPointers.Value,
            InstalledSamusBodyArtwork.TopSetCount);
        ushort[] bottomPointers = ReadPointers(bus,
            SamusSourceDefinitions.BottomDefinitionListPointers.Value,
            InstalledSamusBodyArtwork.BottomSetCount);
        ushort[] posePointers = ReadPointers(bus,
            SamusSourceDefinitions.AnimationDefinitionListPointers.Value,
            InstalledSamusBodyArtwork.PoseCount);
        int[] topSetStarts = [.. topPointers.Select(pointer => BodyRecord(pointer))];
        int[] bottomSetStarts = [.. bottomPointers.Select(pointer => BodyRecord(pointer))];
        int[] poseFirstFrames = [.. posePointers.Select(pointer => BodyFrame(pointer))];
        sbyte[] graphicsYOffsets = Enumerable.Range(0, InstalledSamusBodyArtwork.PoseCount)
            .Select(pose => unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.PoseDefinitions.Value +
                pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4)))
            .ToArray();
        var frames = new SamusBodyFrameSelection[InstalledSamusBodyArtwork.FrameCount];
        for (int index = 0; index < frames.Length; index++)
        {
            int address = SamusSourceDefinitions.BodyFrames.Value + index * 4;
            frames[index] = new SamusBodyFrameSelection(bus.ReadCartridgeByte(address),
                bus.ReadCartridgeByte(address + 1), bus.ReadCartridgeByte(address + 2), bus.ReadCartridgeByte(address + 3));
        }

        ushort[] allPointers = topPointers.Concat(bottomPointers).OrderBy(value => value).ToArray();
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        DefinitionEntry[][] top = ExtractHalf(bus, directory, topPointers, allPointers, true, hashes);
        DefinitionEntry[][] bottom = ExtractHalf(bus, directory, bottomPointers, allPointers, false, hashes);
        ushort[] spritemapTopBases = ReadPointers(bus,
            SamusSourceDefinitions.SpritemapTopBases.Value, InstalledSamusBodyArtwork.PoseCount);
        ushort[] spritemapBottomBases = ReadPointers(bus,
            SamusSourceDefinitions.SpritemapBottomBases.Value, InstalledSamusBodyArtwork.PoseCount);
        ushort[] spritemapPointers = ReadPointers(bus,
            SamusSourceDefinitions.SpritemapPointers.Value, InstalledSamusSpritemaps.EntryCount);
        // Compositions install in record order, so a composition's identity is its place among the referenced records.
        ushort[] compositions = [.. spritemapPointers.Distinct().Where(pointer => pointer != 0).Order()];
        SpriteVisualPart[][] spritemaps = [.. compositions.Select(pointer => ReadSpritemap(bus, pointer))];
        int?[] spritemapEntries = [.. spritemapPointers.Select(pointer =>
            pointer == 0 ? (int?)null : Array.BinarySearch(compositions, pointer))];
        ushort[] landingYOffsets = Enumerable.Range(0,
            SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount)
            .Select(index => (ushort)bus.ReadCartridgeByte(
                SamusSourceDefinitions.LandingVerticalOffsets.Value + index)).ToArray();
        sbyte[] postureYOffsets = Enumerable.Range(0,
            SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount)
            .Select(index => unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value + index))).ToArray();
        sbyte[] drainedYOffsets = Enumerable.Range(0,
            SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount)
            .Select(index => unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.DrainedVerticalOffsets.Value + index))).ToArray();
        var manifest = new Manifest(FormatVersion, sourceCartridgeSha256,
            topSetStarts, bottomSetStarts, poseFirstFrames, graphicsYOffsets,
            frames, top, bottom, spritemapTopBases, spritemapBottomBases,
            spritemapEntries, spritemaps, landingYOffsets, postureYOffsets,
            drainedYOffsets, hashes);
        SamusAtmosphericArtworkFiles.Extract(bus, directory, sourceCartridgeSha256);
        SamusDeathPaletteArtworkFiles.Extract(bus, directory, sourceCartridgeSha256);
        SamusDeathTileArtworkFiles.Extract(bus, directory, sourceCartridgeSha256);
        SamusArmCannonArtworkFiles.Extract(bus, directory, sourceCartridgeSha256);
        // Constructing the catalog catches missing/invalid references before publication.
        _ = BuildCatalog(directory, manifest, null);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    /// <summary>Validates the complete stock Samus body catalog before compiling selected body JSON, PNGs, and linked artwork.</summary>
    /// <param name="stockDirectory">Directory containing supported-revision body metadata, hash-checked PNG sets, and all linked Samus artwork.</param>
    /// <param name="overrideDirectory">Optional directory of independently selected same-named JSON or PNG replacements; missing files retain stock content.</param>
    /// <returns>A cartridge-free catalog of visual pose/frame mappings, spritemaps, offsets, linked artwork and the selected characters' identity.</returns>
    /// <remarks>Body JSON overrides must retain stock PNG hashes, definition-set counts, native source addresses, and spritemap identities. Transfer sizes and references remain validated, and unused PNG-slot tiles must stay zero.</remarks>
    /// <exception cref="InvalidDataException">Stock validation fails or selected metadata, identities, hashes, sizes, PNG padding, or linked artwork are invalid; diagnostics retain the offending filename.</exception>
    public static InstalledSamusBodyArtwork Load(string stockDirectory, string? overrideDirectory) =>
        LoadCharacters(stockDirectory, overrideDirectory).Body;

    /// <summary>Validates the complete stock Samus body catalog, then compiles the selected body and its characters, from which the body sheets are composed.</summary>
    /// <param name="stockDirectory">Directory containing supported-revision body metadata, hash-checked PNG sets, and all linked Samus artwork.</param>
    /// <param name="overrideDirectory">Optional directory of independently selected same-named JSON or PNG replacements; missing files retain stock content.</param>
    /// <returns>The selected split-DMA characters, bound to the selected body catalog.</returns>
    /// <exception cref="InvalidDataException">Stock validation fails or selected metadata, identities, hashes, sizes, PNG padding, or linked artwork are invalid; diagnostics retain the offending filename.</exception>
    public static SamusBodyCharacterArtwork LoadCharacters(string stockDirectory, string? overrideDirectory)
    {
        SamusArtworkFile stockFile = SamusArtworkFile.Read(Path.Combine(stockDirectory, ManifestFileName));
        Manifest stock = stockFile.Json<Manifest>(JsonOptions);
        // Always verify installed stock first; a replacement never conceals corruption.
        _ = stockFile.WithContext(() =>
        {
            ValidateManifest(stock);
            return BuildCatalog(stockDirectory, stock, null);
        });
        SamusArtworkFile selectedFile = stockFile.Select(overrideDirectory);
        return selectedFile.WithContext(() => LoadSelected(selectedFile.Json<Manifest>(JsonOptions)));

        SamusBodyCharacterArtwork LoadSelected(Manifest selected)
        {
            ValidateManifest(selected);
            if (selected.Hashes.Count != stock.Hashes.Count ||
                stock.Hashes.Any(entry => !selected.Hashes.TryGetValue(entry.Key, out string? hash) ||
                    !string.Equals(hash, entry.Value, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidDataException("Samus body override must retain stock PNG provenance hashes.");
            if (selected.Top.Length != stock.Top.Length || selected.Bottom.Length != stock.Bottom.Length ||
                !selected.TopSetStarts.SequenceEqual(stock.TopSetStarts) ||
                !selected.BottomSetStarts.SequenceEqual(stock.BottomSetStarts) ||
                selected.Top.Where((set, i) => set.Length != stock.Top[i].Length).Any() ||
                selected.Bottom.Where((set, i) => set.Length != stock.Bottom[i].Length).Any())
                throw new InvalidDataException("Samus body override changes native definition identities.");
            for (int set = 0; set < stock.Top.Length; set++)
                for (int position = 0; position < stock.Top[set].Length; position++)
                    if (selected.Top[set][position].SourceAddress != stock.Top[set][position].SourceAddress)
                        throw new InvalidDataException("Samus body override changes a native top source identity.");
            for (int set = 0; set < stock.Bottom.Length; set++)
                for (int position = 0; position < stock.Bottom[set].Length; position++)
                    if (selected.Bottom[set][position].SourceAddress != stock.Bottom[set][position].SourceAddress)
                        throw new InvalidDataException("Samus body override changes a native bottom source identity.");
            if (!selected.SpritemapEntries.SequenceEqual(stock.SpritemapEntries) ||
                selected.Spritemaps.Length != stock.Spritemaps.Length)
                throw new InvalidDataException("Samus body override changes native spritemap identities.");
            return BuildCatalog(stockDirectory, selected, overrideDirectory);
        }
    }

    /// <summary>Compiles and validates the complete installed Samus body and linked artwork with player overrides disabled.</summary>
    /// <param name="stockDirectory">Stock Samus artwork directory containing body metadata, PNG sets, atmospheric, death, and arm-cannon resources.</param>
    /// <remarks>Checks provenance, metadata, visual references, image hashes and padding, and linked catalogs without cartridge access or file writes.</remarks>
    /// <exception cref="InvalidDataException">The body installation or any linked artwork resource is invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Exports each upper or lower DMA set as a padded PNG and records its native source sizes.</summary>
    private static DefinitionEntry[][] ExtractHalf(ISnesAddressSpace bus, string directory,
        ushort[] pointers, ushort[] sortedPointers, bool upperHalf,
        Dictionary<string, string> hashes)
    {
        var sets = new DefinitionEntry[pointers.Length][];
        for (int set = 0; set < pointers.Length; set++)
        {
            int start = pointers[set];
            int order = Array.IndexOf(sortedPointers, pointers[set]);
            int end = order + 1 == sortedPointers.Length ? SamusSourceDefinitions.FirstBodyDefinition + SamusBodyDefinitionLayout.RecordCount * SamusSourceDefinitions.BodyDefinitionBytes : sortedPointers[order + 1];
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
                sets[set][position] = new DefinitionEntry(sourceAddress, firstSize, secondSize);
            }
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, TileWidth / 8,
                out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            string name = FileName(upperHalf, set);
            byte[] bytes = png.ToArray();
            File.WriteAllBytes(Path.Combine(directory, name), bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        return sets;
    }

    /// <summary>Combines selected body halves with spritemaps and all linked Samus artwork catalogs.</summary>
    private static SamusBodyCharacterArtwork BuildCatalog(string stockDirectory, Manifest manifest,
        string? overrideDirectory)
    {
        SamusBodyTileTransfer[][] top = LoadHalf(stockDirectory, overrideDirectory,
            manifest, true);
        SamusBodyTileTransfer[][] bottom = LoadHalf(stockDirectory, overrideDirectory,
            manifest, false);
        var spritemaps = new InstalledSamusSpritemaps(manifest.SpritemapTopBases,
            manifest.SpritemapBottomBases,
            [.. manifest.SpritemapEntries.Select(entry => entry is { } ordinal ? new SamusSpritemapId(ordinal) : (SamusSpritemapId?)null)],
            [.. manifest.Spritemaps.Select(parts => new SamusSpritemapDefinition(
                [.. parts.Select(part => part.ToSpritePart(SpriteArtwork.CharactersPerRow))]))]);
        InstalledSamusAtmosphericArtwork atmosphere = SamusAtmosphericArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        InstalledSamusDeathPalettes deathPalettes = SamusDeathPaletteArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        SamusDeathTileAtlas deathTiles = SamusDeathTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        InstalledSamusArmCannonArtwork armCannon = SamusArmCannonArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        return SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = manifest.TopSetStarts, BottomSetStarts = manifest.BottomSetStarts, PoseFirstFrames = manifest.PoseFirstFrames, GraphicsYOffsets = manifest.GraphicsYOffsets, Frames = manifest.Frames, Spritemaps = spritemaps, Atmosphere = atmosphere, DeathPalettes = deathPalettes, DeathTilesContentIdentity = deathTiles.ContentIdentity, ArmCannon = armCannon, LandingYOffsets = manifest.LandingYOffsets, PostureYOffsets = manifest.PostureYOffsets, DrainedYOffsets = manifest.DrainedYOffsets }, top, bottom);
    }

    /// <summary>Re-encodes selected PNG sets into native planar bytes and rejects painted padding tiles.</summary>
    private static SamusBodyTileTransfer[][] LoadHalf(string stockDirectory,
        string? overrideDirectory, Manifest manifest, bool upperHalf)
    {
        DefinitionEntry[][] metadata = upperHalf ? manifest.Top : manifest.Bottom;
        var result = new SamusBodyTileTransfer[metadata.Length][];
        for (int set = 0; set < metadata.Length; set++)
        {
            string name = FileName(upperHalf, set);
            if (!manifest.Hashes.TryGetValue(name, out string? expectedHash))
                throw new InvalidDataException($"Samus body manifest omits the provenance hash for {name}.");
            // Sizes belong to JSON, not to the PNG codec. Reject them under the
            // manifest's context before reading any image that uses the sizes.
            foreach (DefinitionEntry entry in metadata[set])
                if (entry is null || entry.FirstSize == 0 ||
                    entry.FirstSize + entry.SecondSize > SamusBodyCharacterArtwork.BytesPerDefinitionSlot ||
                    (entry.FirstSize + entry.SecondSize) % 32 != 0)
                    throw new InvalidDataException($"Samus body definition {name} has invalid sizes.");
            SamusArtworkFile selected = SamusArtworkFile.Stock(Path.Combine(stockDirectory, name), expectedHash)
                .Select(overrideDirectory);
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
            manifest.GraphicsYOffsets is null ||
            manifest.Frames is null || manifest.Top is null || manifest.Bottom is null ||
            manifest.SpritemapTopBases is null || manifest.SpritemapBottomBases is null ||
            manifest.SpritemapEntries is null || manifest.Spritemaps is null ||
            manifest.LandingYOffsets is null || manifest.PostureYOffsets is null ||
            manifest.DrainedYOffsets is null ||
            manifest.Hashes is null || manifest.Top.Length != InstalledSamusBodyArtwork.TopSetCount ||
            manifest.Bottom.Length != InstalledSamusBodyArtwork.BottomSetCount ||
            manifest.GraphicsYOffsets.Length != InstalledSamusBodyArtwork.PoseCount ||
            manifest.Top.Any(set => set is null || set.Length == 0 || set.Any(entry => entry is null)) ||
            manifest.Bottom.Any(set => set is null || set.Length == 0 || set.Any(entry => entry is null)) ||
            manifest.Spritemaps.Any(entry => entry is null || entry.Any(part => part is null)))
            throw new InvalidDataException("Samus body manifest does not match this installation.");
    }

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

    /// <summary>Names the editable upper or lower body PNG by native set index.</summary>
    private static string FileName(bool upperHalf, int set) =>
        $"{(upperHalf ? "top" : "bottom")}-{set:X2}.png";

    /// <summary>Persists split-DMA identities, pose selectors, spritemaps, offsets, and stock PNG hashes.</summary>
    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        int[] TopSetStarts, int[] BottomSetStarts, int[] PoseFirstFrames,
        sbyte[] GraphicsYOffsets,
        SamusBodyFrameSelection[] Frames, DefinitionEntry[][] Top,
        DefinitionEntry[][] Bottom, ushort[] SpritemapTopBases,
        ushort[] SpritemapBottomBases, int?[] SpritemapEntries,
        SpriteVisualPart[][] Spritemaps, ushort[] LandingYOffsets,
        sbyte[] PostureYOffsets, sbyte[] DrainedYOffsets,
        Dictionary<string, string> Hashes);

    /// <summary>Retains one native DMA source address and the two transfer lengths sharing its PNG slot.</summary>
    private sealed record DefinitionEntry(int SourceAddress, int FirstSize, int SecondSize)
    {
        /// <summary>The first transfer's byte count, 0 through 65535 (a DMA size).</summary>
        public int FirstSize { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, FirstSize);

        /// <summary>The second transfer's byte count, 0 through 65535 (a DMA size).</summary>
        public int SecondSize { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SecondSize);
    }
}
