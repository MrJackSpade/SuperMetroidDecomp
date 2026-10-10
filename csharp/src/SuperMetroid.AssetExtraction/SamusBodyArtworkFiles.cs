using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

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
    /// <summary>Schema version written to the Samus body artwork document.</summary>
    private const int FormatVersion = 4;
    /// <summary>Pixel width of each extracted upper or lower body atlas.</summary>
    private const int TileWidth = 64;
    /// <summary>Pixel height reserved for each native body definition.</summary>
    private const int DefinitionHeight = 16;

    /// <summary>Strict camel-case JSON settings shared by stock and override documents.</summary>
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
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        ushort[] topPointers = ReadPointers(bus,
            SamusRenderingRomData.TileTransfers.TopDefinitionListPointers,
            SamusBodyArtworkCatalog.TopSetCount);
        ushort[] bottomPointers = ReadPointers(bus,
            SamusRenderingRomData.TileTransfers.BottomDefinitionListPointers,
            SamusBodyArtworkCatalog.BottomSetCount);
        ushort[] posePointers = ReadPointers(bus,
            SamusRenderingRomData.TileTransfers.AnimationDefinitionListPointers,
            SamusBodyArtworkCatalog.PoseCount);
        sbyte[] graphicsYOffsets = Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount)
            .Select(pose => unchecked((sbyte)bus.ReadCartridgeByte(
                SamusMovementRomData.Poses.Definitions +
                pose * SamusMovementRomData.Poses.DefinitionByteCount + 4)))
            .ToArray();
        var frames = new SamusBodyFrameSelection[SamusBodyArtworkCatalog.FrameCount];
        for (int index = 0; index < frames.Length; index++)
        {
            int address = SamusBodyArtworkCatalog.FirstFrameAddress + index * 4;
            frames[index] = new SamusBodyFrameSelection(bus.ReadCartridgeByte(address),
                bus.ReadCartridgeByte(address + 1), bus.ReadCartridgeByte(address + 2), bus.ReadCartridgeByte(address + 3));
        }

        ushort[] allPointers = topPointers.Concat(bottomPointers).OrderBy(value => value).ToArray();
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        DefinitionEntry[][] top = ExtractHalf(bus, directory, topPointers, allPointers, true, hashes);
        DefinitionEntry[][] bottom = ExtractHalf(bus, directory, bottomPointers, allPointers, false, hashes);
        ushort[] spritemapTopBases = ReadPointers(bus,
            SamusSpritemapArtworkCatalog.TopBaseAddress, SamusBodyArtworkCatalog.PoseCount);
        ushort[] spritemapBottomBases = ReadPointers(bus,
            SamusSpritemapArtworkCatalog.BottomBaseAddress, SamusBodyArtworkCatalog.PoseCount);
        ushort[] spritemapPointers = ReadPointers(bus,
            SamusSpritemapArtworkCatalog.PointerTableAddress, SamusSpritemapArtworkCatalog.PointerCount);
        SamusSpritemapDefinition[] spritemaps = spritemapPointers.Distinct()
            .Where(pointer => pointer != 0).OrderBy(pointer => pointer)
            .Select(pointer => ReadSpritemap(bus, pointer)).ToArray();
        ushort[] landingYOffsets = Enumerable.Range(0,
            SamusRenderingRomData.Body.LandingVerticalOffsetByteCount)
            .Select(index => (ushort)bus.ReadCartridgeByte(
                SamusRenderingRomData.Body.LandingVerticalOffsets + index)).ToArray();
        sbyte[] postureYOffsets = Enumerable.Range(0,
            SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount)
            .Select(index => unchecked((sbyte)bus.ReadCartridgeByte(
                SamusRenderingRomData.Body.PostureTransitionVerticalOffsets + index))).ToArray();
        sbyte[] drainedYOffsets = Enumerable.Range(0,
            SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount)
            .Select(index => unchecked((sbyte)bus.ReadCartridgeByte(
                SamusRenderingRomData.Body.DrainedVerticalOffsets + index))).ToArray();
        var manifest = new Manifest(FormatVersion, sourceCartridgeSha256,
            topPointers, bottomPointers, posePointers, graphicsYOffsets,
            frames, top, bottom, spritemapTopBases, spritemapBottomBases,
            spritemapPointers, spritemaps, landingYOffsets, postureYOffsets,
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
    /// <returns>A cartridge-free catalog owning selected split-DMA characters, visual pose/frame mappings, spritemaps, offsets, and linked artwork.</returns>
    /// <remarks>Body JSON overrides must retain stock PNG hashes, definition-set counts, native source addresses, and spritemap identities. Transfer sizes and references remain validated, and unused PNG-slot tiles must stay zero.</remarks>
    /// <exception cref="InvalidDataException">Stock validation fails or selected metadata, identities, hashes, sizes, PNG padding, or linked artwork are invalid; diagnostics retain the offending filename.</exception>
    public static SamusBodyArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
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

        SamusBodyArtworkCatalog LoadSelected(Manifest selected)
        {
            ValidateManifest(selected);
            if (selected.Hashes.Count != stock.Hashes.Count ||
                stock.Hashes.Any(entry => !selected.Hashes.TryGetValue(entry.Key, out string? hash) ||
                    !string.Equals(hash, entry.Value, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidDataException("Samus body override must retain stock PNG provenance hashes.");
            if (selected.Top.Length != stock.Top.Length || selected.Bottom.Length != stock.Bottom.Length ||
                !selected.TopPointers.AsSpan().SequenceEqual(stock.TopPointers) ||
                !selected.BottomPointers.AsSpan().SequenceEqual(stock.BottomPointers) ||
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
            if (!selected.SpritemapPointers.AsSpan().SequenceEqual(stock.SpritemapPointers) ||
                selected.Spritemaps.Length != stock.Spritemaps.Length ||
                selected.Spritemaps.Where((entry, i) => entry.Pointer != stock.Spritemaps[i].Pointer).Any())
                throw new InvalidDataException("Samus body override changes native spritemap identities.");
            return BuildCatalog(stockDirectory, selected, overrideDirectory);
        }
    }

    /// <summary>Compiles and validates the complete installed Samus body and linked artwork with player overrides disabled.</summary>
    /// <param name="stockDirectory">Stock Samus artwork directory containing body metadata, PNG sets, atmospheric, death, and arm-cannon resources.</param>
    /// <remarks>Checks provenance, metadata, visual references, image hashes and padding, and linked catalogs without cartridge access or file writes.</remarks>
    /// <exception cref="InvalidDataException">The body installation or any linked artwork resource is invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Extracts one upper or lower DMA half into padded planar slots and records native identities.</summary>
    /// <param name="bus">Cartridge address space containing body DMA definitions and source graphics.</param>
    /// <param name="directory">Directory receiving the encoded atlas PNGs.</param>
    /// <param name="pointers">Native definition-list pointers for this body half.</param>
    /// <param name="sortedPointers">All upper and lower pointers, sorted to bound each list.</param>
    /// <param name="upperHalf">Whether this pass extracts upper-body definitions.</param>
    /// <param name="hashes">Receives the hash of each emitted PNG.</param>
    /// <returns>Definition metadata grouped by native pointer-list set.</returns>
    private static DefinitionEntry[][] ExtractHalf(ISnesAddressSpace bus, string directory,
        ushort[] pointers, ushort[] sortedPointers, bool upperHalf,
        Dictionary<string, string> hashes)
    {
        var sets = new DefinitionEntry[pointers.Length][];
        for (int set = 0; set < pointers.Length; set++)
        {
            int start = pointers[set];
            int order = Array.IndexOf(sortedPointers, pointers[set]);
            int end = order + 1 == sortedPointers.Length ? SamusBodyDefinitionLayout.EndOffset : sortedPointers[order + 1];
            if (start < 0x8000 || end <= start || (end - start) % 7 != 0)
                throw new InvalidDataException($"Samus body set {set:X2} has invalid native bounds.");
            int count = (end - start) / 7;
            byte[] planar = new byte[count * SamusBodyArtworkCatalog.BytesPerDefinitionSlot];
            sets[set] = new DefinitionEntry[count];
            for (int position = 0; position < count; position++)
            {
                int definitionAddress = 0x920000 | (start + position * 7);
                ushort sourceOffset = ReadWord(bus, definitionAddress);
                byte sourceBank = bus.ReadCartridgeByte(definitionAddress + 2);
                ushort firstSize = ReadWord(bus, definitionAddress + 3);
                ushort secondSize = ReadWord(bus, definitionAddress + 5);
                int byteCount = firstSize + secondSize;
                if (sourceOffset < 0x8000 || firstSize == 0 || byteCount >
                    SamusBodyArtworkCatalog.BytesPerDefinitionSlot || byteCount % 32 != 0 ||
                    sourceOffset + byteCount > 0x10000)
                    throw new InvalidDataException($"Invalid Samus body DMA at ${definitionAddress:X6}.");
                int sourceAddress = sourceBank << 16 | sourceOffset;
                for (int i = 0; i < byteCount; i++)
                    planar[position * SamusBodyArtworkCatalog.BytesPerDefinitionSlot + i] =
                        bus.ReadCartridgeByte(sourceAddress + i);
                sets[set][position] = new DefinitionEntry(sourceAddress, firstSize, secondSize);
            }
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, TileWidth / 8,
                out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
            string name = FileName(upperHalf, set);
            byte[] bytes = png.ToArray();
            File.WriteAllBytes(Path.Combine(directory, name), bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        return sets;
    }

    /// <summary>Loads body atlas pixels and combines manifest selectors with linked Samus artwork.</summary>
    /// <param name="stockDirectory">Directory containing stock body PNGs and linked artwork.</param>
    /// <param name="manifest">Validated body metadata to compile.</param>
    /// <param name="overrideDirectory">Optional directory supplying selected PNG replacements.</param>
    /// <returns>The compiled cartridge-free Samus body artwork catalog.</returns>
    private static SamusBodyArtworkCatalog BuildCatalog(string stockDirectory, Manifest manifest,
        string? overrideDirectory)
    {
        SamusBodyTileDefinition[][] top = LoadHalf(stockDirectory, overrideDirectory,
            manifest, true);
        SamusBodyTileDefinition[][] bottom = LoadHalf(stockDirectory, overrideDirectory,
            manifest, false);
        var spritemaps = new SamusSpritemapArtworkCatalog(manifest.SpritemapTopBases,
            manifest.SpritemapBottomBases, manifest.SpritemapPointers, manifest.Spritemaps);
        SamusAtmosphericArtworkCatalog atmosphere = SamusAtmosphericArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        SamusDeathPaletteArtworkCatalog deathPalettes = SamusDeathPaletteArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        SamusDeathTileAtlas deathTiles = SamusDeathTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        SamusArmCannonArtworkCatalog armCannon = SamusArmCannonArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        return new SamusBodyArtworkCatalog(manifest.TopPointers, manifest.BottomPointers,
            manifest.PosePointers, manifest.GraphicsYOffsets, manifest.Frames, top, bottom,
            spritemaps, atmosphere, deathPalettes, deathTiles, armCannon,
            manifest.LandingYOffsets, manifest.PostureYOffsets,
            manifest.DrainedYOffsets);
    }

    /// <summary>Loads each atlas in one body half and checks its identity, hash, and zero padding.</summary>
    /// <param name="stockDirectory">Directory containing the stock atlas files.</param>
    /// <param name="overrideDirectory">Optional directory containing replacements with stock filenames.</param>
    /// <param name="manifest">Body definition sets and immutable stock PNG hashes used to validate each atlas.</param>
    /// <param name="upperHalf">Whether the definitions describe the upper body.</param>
    /// <returns>Decoded body definitions grouped by set.</returns>
    private static SamusBodyTileDefinition[][] LoadHalf(string stockDirectory,
        string? overrideDirectory, Manifest manifest, bool upperHalf)
    {
        DefinitionEntry[][] metadata = upperHalf ? manifest.Top : manifest.Bottom;
        var result = new SamusBodyTileDefinition[metadata.Length][];
        for (int set = 0; set < metadata.Length; set++)
        {
            string name = FileName(upperHalf, set);
            if (!manifest.Hashes.TryGetValue(name, out string? expectedHash))
                throw new InvalidDataException($"Samus body manifest omits the provenance hash for {name}.");
            // Sizes belong to JSON, not to the PNG codec. Reject them under the
            // manifest's context before reading any image that uses the sizes.
            foreach (DefinitionEntry entry in metadata[set])
                if (entry is null || entry.FirstSize == 0 ||
                    entry.FirstSize + entry.SecondSize > SamusBodyArtworkCatalog.BytesPerDefinitionSlot ||
                    (entry.FirstSize + entry.SecondSize) % 32 != 0)
                    throw new InvalidDataException($"Samus body definition {name} has invalid sizes.");
            SamusArtworkFile selected = SamusArtworkFile.Stock(Path.Combine(stockDirectory, name), expectedHash)
                .Select(overrideDirectory);
            int height = checked(metadata[set].Length * DefinitionHeight);
            result[set] = selected.Compile(stream => DecodeSet(stream));

            SamusBodyTileDefinition[] DecodeSet(Stream stream)
            {
                IndexedPngImage image = IndexedPng.Read(stream, TileWidth, height);
                byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, TileWidth, height, 4);
                var definitions = new SamusBodyTileDefinition[metadata[set].Length];
                for (int position = 0; position < definitions.Length; position++)
                {
                    DefinitionEntry entry = metadata[set][position];
                    int byteCount = entry.FirstSize + entry.SecondSize;
                    ReadOnlySpan<byte> slot = planar.AsSpan(
                        position * SamusBodyArtworkCatalog.BytesPerDefinitionSlot,
                        SamusBodyArtworkCatalog.BytesPerDefinitionSlot);
                    if (slot[byteCount..].IndexOfAnyExcept((byte)0) >= 0)
                        throw new InvalidDataException($"Samus body PNG {name} paints unused tiles in definition {position}.");
                    definitions[position] = new SamusBodyTileDefinition(
                        entry.SourceAddress, entry.FirstSize, entry.SecondSize,
                        slot[..byteCount].ToArray());
                }
                return definitions;
            }
        }
        return result;
    }

    /// <summary>Checks manifest dimensions, pointer identities, definition sizes, and selector references.</summary>
    /// <param name="manifest">The parsed body metadata to validate.</param>
    private static void ValidateManifest(Manifest manifest)
    {
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) || manifest.TopPointers is null ||
            manifest.BottomPointers is null || manifest.PosePointers is null ||
            manifest.GraphicsYOffsets is null ||
            manifest.Frames is null || manifest.Top is null || manifest.Bottom is null ||
            manifest.SpritemapTopBases is null || manifest.SpritemapBottomBases is null ||
            manifest.SpritemapPointers is null || manifest.Spritemaps is null ||
            manifest.LandingYOffsets is null || manifest.PostureYOffsets is null ||
            manifest.DrainedYOffsets is null ||
            manifest.Hashes is null || manifest.Top.Length != SamusBodyArtworkCatalog.TopSetCount ||
            manifest.Bottom.Length != SamusBodyArtworkCatalog.BottomSetCount ||
            manifest.GraphicsYOffsets.Length != SamusBodyArtworkCatalog.PoseCount ||
            manifest.Top.Any(set => set is null || set.Length == 0 || set.Any(entry => entry is null)) ||
            manifest.Bottom.Any(set => set is null || set.Length == 0 || set.Any(entry => entry is null)) ||
            manifest.Spritemaps.Any(entry => entry is null))
            throw new InvalidDataException("Samus body manifest does not match this installation.");
    }

    /// <summary>Reads a contiguous table of native 16-bit pointers.</summary>
    /// <param name="bus">Cartridge address space containing the pointer table.</param>
    /// <param name="start">Address of the first pointer.</param>
    /// <param name="count">Number of pointers to read.</param>
    /// <returns>The pointers in table order.</returns>
    private static ushort[] ReadPointers(ISnesAddressSpace bus, int start, int count) =>
        Enumerable.Range(0, count).Select(i => ReadWord(bus, start + i * 2)).ToArray();

    /// <summary>Decodes one native Samus spritemap and its piece coordinates.</summary>
    /// <param name="bus">Cartridge address space containing the spritemap records.</param>
    /// <param name="pointer">Bank-local address of the spritemap.</param>
    /// <returns>The decoded spritemap definition.</returns>
    private static SamusSpritemapDefinition ReadSpritemap(ISnesAddressSpace bus, ushort pointer)
    {
        int address = 0x920000 | pointer;
        ushort count = ReadWord(bus, address);
        if (pointer < 0x90ED || count > 128 || pointer + 2 + count * 5 > 0x10000)
            throw new InvalidDataException($"Invalid Samus spritemap at $92:{pointer:X4}.");
        var parts = new SamusSpritePart[count];
        for (int i = 0; i < count; i++)
        {
            int part = address + 2 + i * 5;
            parts[i] = new SamusSpritePart(ReadWord(bus, part), bus.ReadCartridgeByte(part + 2),
                ReadWord(bus, part + 3));
        }
        return new SamusSpritemapDefinition(pointer, parts);
    }

    /// <summary>Reads a little-endian cartridge word.</summary>
    /// <param name="bus">Cartridge address space supplying the bytes.</param>
    /// <param name="address">Address of the low byte.</param>
    /// <returns>The decoded 16-bit word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);

    /// <summary>Builds the stable atlas filename for a body half and definition set.</summary>
    /// <param name="upperHalf">Whether the filename identifies the upper-body atlas.</param>
    /// <param name="set">Zero-based native definition-list set.</param>
    /// <returns>The corresponding PNG filename.</returns>
    private static string FileName(bool upperHalf, int set) =>
        $"{(upperHalf ? "top" : "bottom")}-{set:X2}.png";

    /// <summary>Serialized split-DMA identities, visual selectors, offsets, and stock image hashes.</summary>
    /// <param name="Version">Body artwork JSON schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 of the cartridge revision used for extraction.</param>
    /// <param name="TopPointers">Native upper-body definition-list pointers by set.</param>
    /// <param name="BottomPointers">Native lower-body definition-list pointers by set.</param>
    /// <param name="PosePointers">Native animation definition-list pointers by pose.</param>
    /// <param name="GraphicsYOffsets">Per-pose native graphics vertical offsets.</param>
    /// <param name="Frames">Visual frame-to-definition selectors.</param>
    /// <param name="Top">Upper-body DMA definitions grouped by set.</param>
    /// <param name="Bottom">Lower-body DMA definitions grouped by set.</param>
    /// <param name="SpritemapTopBases">Upper spritemap base identities by pose.</param>
    /// <param name="SpritemapBottomBases">Lower spritemap base identities by pose.</param>
    /// <param name="SpritemapPointers">Native spritemap pointers by selector slot.</param>
    /// <param name="Spritemaps">Decoded spritemap records referenced by the pointer table.</param>
    /// <param name="LandingYOffsets">Landing pose vertical offsets.</param>
    /// <param name="PostureYOffsets">Posture-transition vertical offsets.</param>
    /// <param name="DrainedYOffsets">Drained-state vertical offsets.</param>
    /// <param name="Hashes">Stock PNG hashes keyed by atlas filename.</param>
    private sealed record Manifest(int Version, string SourceCartridgeSha256,
        ushort[] TopPointers, ushort[] BottomPointers, ushort[] PosePointers,
        sbyte[] GraphicsYOffsets,
        SamusBodyFrameSelection[] Frames, DefinitionEntry[][] Top,
        DefinitionEntry[][] Bottom, ushort[] SpritemapTopBases,
        ushort[] SpritemapBottomBases, ushort[] SpritemapPointers,
        SamusSpritemapDefinition[] Spritemaps, ushort[] LandingYOffsets,
        sbyte[] PostureYOffsets, sbyte[] DrainedYOffsets,
        Dictionary<string, string> Hashes);

    /// <summary>Native source identity and two transfer lengths for one split body definition.</summary>
    /// <param name="SourceAddress">Banked source address of the first transfer.</param>
    /// <param name="FirstSize">Byte count of the first native DMA transfer.</param>
    /// <param name="SecondSize">Byte count of the following native DMA transfer.</param>
    private sealed record DefinitionEntry(int SourceAddress, ushort FirstSize, ushort SecondSize);
}
