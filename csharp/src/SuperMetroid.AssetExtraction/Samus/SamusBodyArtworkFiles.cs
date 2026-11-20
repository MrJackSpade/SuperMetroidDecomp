using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Decodes Samus's split body tables once into named half pictures, each an indexed PNG, and the decoded body JSON, which
/// are the body component's import data (#1636, #1638, #2556), and installs what the game binds of the body: each pose's art origin and every body frame the body sheet
/// pictures, with where that frame places the body.
/// </summary>
/// <remarks>
/// Each half picture occupies sixteen 4-bpp tiles (64 by 16 pixels) in its own PNG.
/// Unused tiles are zero. Transfer sizes and what each pose frame shows live in the decoded JSON;
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
    /// Import-data JSON filename of the decoded body (#1638, #2556): what each pose frame shows, the half pictures' transfer
    /// sizes, the compositions, whose parts are packed sprite placements, and the half pictures' provenance hashes.
    /// </summary>
    private const string ArtworkFileName = "samus-body-artwork.json";
    /// <summary>
    /// Manifest revision containing source hashes, selectors, spritemaps stored as sprite parts, and body placements.
    /// Revision nine holds each pose's placement and its frames' own placements instead of the cartridge's offset tables
    /// (#1517). Revision ten installs with the body sheet that pictures the glitch balls, their knockbacks, the
    /// Draygon-held ball and its stand-up, which the Baby's Hyper Beam freeze and rainbow stop can install (#1535).
    /// Revision eleven keys each pose's frame list and half-table bases by the pose's name rather than its place in the
    /// cartridge's pose tables, and installs none for the records no pose names (#1595). Revision twelve installs no
    /// character page: the body halves, the arm cannon's cover tiles and the death explosion's characters are import
    /// data (#1636). Revision thirteen installs only what the game binds of the body: the extracted manifest, with its
    /// spritemaps, and the atmospheric lists and the arm cannon's appearances are import data (#1638). Revision fourteen
    /// installs the body without the cartridge's digest and the arm cannon and death documents without manifests (#1693).
    /// Revision fifteen keys each pose's frames by their names in the pose's animation, the segment and the frame within it,
    /// and installs the frames each pose shows with the picture frame each draws (#2048). Revision sixteen names each
    /// pose's art origin <c>artOrigin</c> (#2113). Revision seventeen records a frame that names no lower half with no
    /// lower group, as null (#2558). Revision eighteen stores the import data decoded: each pose frame's named
    /// half pictures, placement and compositions, one image per half picture, and no cartridge table (#2556).
    /// </summary>
    private const int FormatVersion = 18;
    /// <summary>Each half picture occupies eight 8x8 tiles across its PNG row.</summary>
    private const int TileWidth = 64;
    /// <summary>Two tile rows reserve sixteen graphics tiles for one half picture.</summary>
    private const int DefinitionHeight = 16;

    /// <summary>Serializes strict camel-case manifests and rejects unknown JSON members.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
        Converters = { new SamusPoseIdJsonConverter() },
    };

    /// <summary>
    /// Decodes Samus's body tables once (#2556) and writes each half picture's PNG and the decoded body JSON to the import
    /// data, together with linked atmospheric, death, and arm-cannon artwork, and installs what the game binds of the body
    /// (#1638).
    /// </summary>
    /// <param name="bus">Non-null cartridge address space supplying native body DMA definitions, pose/frame tables, spritemaps, offsets, and linked artwork.</param>
    /// <param name="directory">Stock output directory, created if needed; the installed body JSON is overwritten and the half pictures and the extracted JSON go to its import-data directory (#1636, #1638), while linked extractors retain their own file-creation policies.</param>
    /// <param name="sourceCartridgeSha256">Source cartridge SHA-256 recorded in body and linked manifests; installed loading requires the supported revision.</param>
    /// <remarks>Each half picture uses a zero-padded 64x16 four-bit character slot. Gameplay and animation-delay bytes are not exported.</remarks>
    /// <exception cref="InvalidDataException">Native DMA bounds, sizes, spritemaps, or linked cartridge sources are invalid.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        // The import decides once which frames her body can show and which body frames the sheet pictures, from the frames
        // her animation shows as the game's layout of the programs reads them; the tables are decoded for those frames.
        SamusShownFrames shown = new SamusShownFrameAnalysis(SamusAnimationLayout.Read(bus)).Installed();
        SamusBodyDecoded decoded = SamusBodyTableDecoding.Decode(bus, shown, out IReadOnlyDictionary<SamusBodyPicture, SamusBodyTileTransfer> transfers);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        var pictures = new Dictionary<string, PictureEntry>(StringComparer.Ordinal);
        foreach ((SamusBodyPicture picture, SamusBodyTileTransfer transfer) in transfers)
        {
            byte[] planar = new byte[SamusBodyCharacterArtwork.BytesPerDefinitionSlot];
            transfer.Planar.Span.CopyTo(planar);
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, TileWidth / 8, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            string name = FileName(picture);
            byte[] bytes = png.ToArray();
            File.WriteAllBytes(Path.Combine(importData, name), bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
            pictures.Add(picture.Name, new PictureEntry(transfer.FirstSize, transfer.SecondSize));
        }
        var manifest = new Manifest(FormatVersion, sourceCartridgeSha256,
            decoded.ArtOrigins.ToDictionary(),
            decoded.Frames.ToDictionary(pose => pose.Key, pose =>
            {
                SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose.Key);
                return pose.Value.ToDictionary(frame => animation.NameOf(SamusAnimationLayout.FrameAt(animation, frame.Key)),
                    frame => new FrameEntry(frame.Value.OriginRise, frame.Value.Pictures.Top.Name, frame.Value.Pictures.Bottom?.Name,
                        frame.Value.TopComposition?.Name, frame.Value.BottomComposition?.Name), StringComparer.Ordinal);
            }),
            pictures,
            decoded.Compositions.ToDictionary(pair => pair.Key.Name, pair => pair.Value, StringComparer.Ordinal),
            decoded.Explosion.ToDictionary(pair => pair.Key, pair => pair.Value?.Name, StringComparer.Ordinal),
            decoded.Atmosphere.ToDictionary(pair => pair.Key, pair => pair.Value?.Name, StringComparer.Ordinal),
            hashes);
        SamusAtmosphericArtworkFiles.Extract(bus, importData, sourceCartridgeSha256);
        SamusDeathPaletteArtworkFiles.Extract(bus, directory, importData);
        SamusDeathTileArtworkFiles.Extract(bus, importData);
        SamusArmCannonArtworkFiles.Extract(bus, importData);
        File.WriteAllBytes(Path.Combine(importData, ArtworkFileName),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        // The installation holds the frames the body sheet pictures with their placements: the game binds those and nothing else.
        InstalledSamusBody installed = LoadForImport(directory, importData).InstalledBody(shown);
        SamusArmCannonArtworkFiles.Install(directory, installed.ArmCannon);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new GameManifest(FormatVersion,
                SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose =>
                {
                    SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
                    return new PoseFrames(installed.ArtOrigin(pose),
                        installed.FramesOf(pose).ToDictionary(pair => animation.NameOf(pair.Key), pair => pair.Value, StringComparer.Ordinal),
                        installed.ShownFrames.PictureFrames[pose].OrderBy(pair => pair.Key, animation.FrameOrder).ToDictionary(
                            pair => animation.NameOf(pair.Key), pair => animation.NameOf(pair.Value), StringComparer.Ordinal));
                })), JsonOptions));
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
            if (manifest.Version != FormatVersion || manifest.Poses is null || !SamusPoseNumbers.CoversRecordedPoses(manifest.Poses) ||
                manifest.Poses.Values.Any(pose => pose is null || pose.Frames is null || pose.Shows is null))
                throw new InvalidDataException("Samus body manifest does not match this installation.");
            return new InstalledSamusBody(manifest.Poses.ToDictionary(pose => pose.Key, pose => pose.Value.ArtOrigin),
                manifest.Poses.ToDictionary(pose => pose.Key, pose =>
                {
                    SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose.Key);
                    return (IReadOnlyDictionary<SamusAnimationFrame, int>)pose.Value.Frames.ToDictionary(
                        frame => animation.FrameNamed(frame.Key), frame => frame.Value);
                }),
                new SamusShownFrames(manifest.Poses.ToDictionary(pose => pose.Key, pose =>
                {
                    SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose.Key);
                    return (IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>)pose.Value.Shows.ToDictionary(
                        frame => animation.FrameNamed(frame.Key), frame => animation.FrameNamed(frame.Value));
                })),
                deathSteps, armCannon);
        });
    }

    /// <summary>
    /// Validates and compiles the Samus body as the import reads it (#1638, #2556): the decoded body JSON, atmosphere and
    /// arm cannon from the import data <paramref name="importData"/>, and the installed death steps in
    /// <paramref name="directory"/>. No half picture is read (#1636).
    /// </summary>
    /// <param name="directory">The component directory, holding the installed death documents and the arm cannon's manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the decoded body, atmosphere and arm cannon.</param>
    /// <exception cref="InvalidDataException">Metadata or linked artwork are invalid; diagnostics retain the offending filename.</exception>
    private static SamusBodySources LoadForImport(string directory, string importData)
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
    /// Validates and compiles the Samus body as the import reads it and its half pictures, from which the body sheets are
    /// composed: the decoded documents and the half pictures' images in the import data <paramref name="importData"/>
    /// (#1636, #1638, #2556).
    /// </summary>
    /// <param name="directory">The component directory, holding the installed death documents and the arm cannon's manifest.</param>
    /// <param name="importData">The component's import-data directory, holding the decoded documents and the half pictures.</param>
    /// <returns>The half pictures' characters, bound to the body catalog.</returns>
    /// <remarks>Transfer sizes and references are validated, and unused slot tiles must stay zero.</remarks>
    /// <exception cref="InvalidDataException">Metadata, sizes, slot padding, or linked artwork are invalid; diagnostics retain the offending filename.</exception>
    public static SamusBodyCharacterArtwork LoadCharacters(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        SamusArtworkFile manifestFile = SamusArtworkFile.Read(Path.Combine(importData, ArtworkFileName));
        Manifest manifest = manifestFile.Json<Manifest>(JsonOptions);
        return manifestFile.WithContext(() =>
        {
            ValidateManifest(manifest);
            return SamusBodyCharacterArtwork.Create(Parts(directory, importData, manifest), LoadPictures(importData, manifest));
        });
    }

    /// <summary>The decoded body with its compositions and all linked Samus document catalogs, as the import reads them.</summary>
    /// <exception cref="InvalidDataException">A frame name names no frame of its pose's animation, or two name one frame.</exception>
    private static SamusBodyArtworkParts Parts(string directory, string importData, Manifest manifest)
    {
        var frames = new Dictionary<SamusPoseId, IReadOnlyDictionary<int, SamusBodyDecodedFrame>>();
        foreach ((SamusPoseId pose, Dictionary<string, FrameEntry> entries) in manifest.Poses)
        {
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            var decoded = new Dictionary<int, SamusBodyDecodedFrame>();
            foreach ((string name, FrameEntry entry) in entries)
            {
                int frame = SamusAnimationLayout.Offset(animation, animation.FrameNamed(name));
                SamusSpritemapId? top = Composition(entry.TopComposition), bottom = Composition(entry.BottomComposition);
                if (!decoded.TryAdd(frame, new SamusBodyDecodedFrame(new SamusBodyFramePictures(new SamusBodyPicture(entry.Top),
                        entry.Bottom is { } lower ? new SamusBodyPicture(lower) : null), entry.OriginRise, top, bottom)))
                    throw new InvalidDataException($"Samus body frame {name} of {pose} names a frame another name names.");
            }
            frames.Add(pose, decoded);
        }
        var decodedBody = new SamusBodyDecoded(manifest.ArtOrigins, frames,
            manifest.Compositions.ToDictionary(pair => new SamusSpritemapId(pair.Key), pair => pair.Value),
            manifest.Explosion.ToDictionary(pair => pair.Key, pair => Composition(pair.Value), StringComparer.Ordinal),
            manifest.Atmosphere.ToDictionary(pair => pair.Key, pair => Composition(pair.Value), StringComparer.Ordinal));
        return decodedBody.Parts(SamusAtmosphericArtworkFiles.Load(importData), SamusDeathPaletteArtworkFiles.LoadSteps(directory),
            SamusArmCannonArtworkFiles.LoadForImport(importData));

        static SamusSpritemapId? Composition(string? name) => name is null ? null : new SamusSpritemapId(name);
    }

    /// <summary>Re-encodes each half picture's image in <paramref name="importData"/> into native planar bytes and rejects painted padding tiles.</summary>
    private static Dictionary<SamusBodyPicture, SamusBodyTileTransfer> LoadPictures(string importData, Manifest manifest)
    {
        var result = new Dictionary<SamusBodyPicture, SamusBodyTileTransfer>();
        foreach ((string pictureName, PictureEntry entry) in manifest.Pictures)
        {
            var picture = new SamusBodyPicture(pictureName);
            string name = FileName(picture);
            if (!manifest.Hashes.ContainsKey(name))
                throw new InvalidDataException($"Samus body manifest omits {name}.");
            // Sizes belong to JSON, not to the PNG codec. Reject them under the
            // manifest's context before reading any image that uses the sizes.
            if (entry is null || entry.FirstSize == 0 || entry.FirstSize + entry.SecondSize > SamusBodyCharacterArtwork.BytesPerDefinitionSlot ||
                (entry.FirstSize + entry.SecondSize) % 32 != 0)
                throw new InvalidDataException($"Samus body picture {name} has invalid sizes.");
            SamusArtworkFile selected = SamusArtworkFile.Read(Path.Combine(importData, name));
            result.Add(picture, selected.Compile(stream =>
            {
                IndexedPngImage image = IndexedPng.Read(stream, new(TileWidth, DefinitionHeight));
                byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, new(TileWidth, DefinitionHeight), 4);
                int byteCount = entry.FirstSize + entry.SecondSize;
                if (planar.AsSpan(byteCount).IndexOfAnyExcept((byte)0) >= 0)
                    throw new InvalidDataException($"Samus body PNG {name} paints unused tiles.");
                return new SamusBodyTileTransfer(checked((ushort)entry.FirstSize), checked((ushort)entry.SecondSize), planar[..byteCount]);
            }));
        }
        return result;
    }

    /// <summary>Checks source revision, manifest version, required tables, and that every frame's names are pictures and compositions the document holds.</summary>
    private static void ValidateManifest(Manifest manifest)
    {
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.ArtOrigins is null || manifest.Poses is null || manifest.Pictures is null || manifest.Compositions is null ||
            manifest.Explosion is null || manifest.Atmosphere is null || manifest.Hashes is null ||
            !SamusPoseNumbers.CoversRecordedPoses(manifest.ArtOrigins) ||
            !SamusPoseNumbers.CoversRecordedPoses(manifest.Poses) ||
            manifest.Poses.Values.Any(pose => pose is null || pose.Values.Any(frame => frame is null ||
                !manifest.Pictures.ContainsKey(frame.Top) || (frame.Bottom is { } bottom && !manifest.Pictures.ContainsKey(bottom)))) ||
            manifest.Pictures.Keys.Any(name => Path.GetFileName(FileName(new SamusBodyPicture(name))) != FileName(new SamusBodyPicture(name))) ||
            manifest.Compositions.Values.Any(entry => entry is null || entry.Any(part => part is null)))
            throw new InvalidDataException("Samus body manifest does not match this installation.");
    }

    /// <summary>Names the import-data image of <paramref name="picture"/> by the picture's name.</summary>
    private static string FileName(SamusBodyPicture picture) => $"{picture.Name}.png";

    /// <summary>
    /// What the installed body JSON holds (#1638): each pose's art origin and the body frames the body sheet pictures.
    /// </summary>
    /// <param name="Version">The body JSON's revision.</param>
    /// <param name="Poses">Each pose with a record's art origin and pictured frames, keyed by the pose's name.</param>
    internal sealed record GameManifest(int Version, Dictionary<SamusPoseId, PoseFrames> Poses);

    /// <summary>One pose's art origin, the body frames the body sheet pictures of it and the frames it shows.</summary>
    /// <param name="ArtOrigin">
    /// The pose's art origin, -128 through 127: how many pixels above Samus's center the arm cannon, the Grapple beam, the
    /// charge flare and the speed-booster echoes are drawn.
    /// </param>
    /// <param name="Frames">
    /// Each pictured animation frame, by its name in the pose's animation, its segment's name and the frame within the
    /// segment: how many pixels above Samus's drawn point the frame's body origin sits, negative below it.
    /// </param>
    /// <param name="Shows">
    /// Each animation frame the pose shows, by its name in the pose's animation, with the name of the picture frame it draws.
    /// </param>
    internal sealed record PoseFrames(int ArtOrigin, Dictionary<string, int> Frames, Dictionary<string, string> Shows);

    /// <summary>
    /// The decoded body, import data (#1638, #2556): each pose's art origin and what each of its picture frames shows, the
    /// half pictures' transfer sizes, the compositions, and stock PNG hashes. No cartridge table survives into it.
    /// </summary>
    /// <param name="Version">The document's revision.</param>
    /// <param name="SourceCartridgeSha256">The cartridge the document was decoded from.</param>
    /// <param name="ArtOrigins">Each pose with a record's art origin.</param>
    /// <param name="Poses">Each pose with a record's decoded picture frames, by the frame's name in the pose's animation.</param>
    /// <param name="Pictures">Each half picture's transfer sizes, by its name, which also names its image.</param>
    /// <param name="Compositions">Each composition's sprite parts, frontmost first, by its name.</param>
    /// <param name="Explosion">The composition each suit-explosion sheet frame shows, or none, by the sheet frame's name.</param>
    /// <param name="Atmosphere">The composition each diving-splash and bubbles sheet frame shows, or none, by the sheet frame's name.</param>
    /// <param name="Hashes">Each half picture image's SHA-256, by file name.</param>
    internal sealed record Manifest(int Version, string SourceCartridgeSha256,
        Dictionary<SamusPoseId, int> ArtOrigins,
        Dictionary<SamusPoseId, Dictionary<string, FrameEntry>> Poses,
        Dictionary<string, PictureEntry> Pictures,
        Dictionary<string, SpriteVisualPiece[]> Compositions,
        Dictionary<string, string?> Explosion,
        Dictionary<string, string?> Atmosphere,
        Dictionary<string, string> Hashes);

    /// <summary>What one decoded picture frame shows.</summary>
    /// <param name="OriginRise">How many pixels above Samus's drawn point the frame places the body's origin, negative below it.</param>
    /// <param name="Top">The name of the top half picture the frame selects.</param>
    /// <param name="Bottom">The name of the bottom half picture the frame selects, or none.</param>
    /// <param name="TopComposition">The name of the composition the top half shows, or none.</param>
    /// <param name="BottomComposition">The name of the composition the bottom half shows, or none.</param>
    internal sealed record FrameEntry(int OriginRise, string Top, string? Bottom, string? TopComposition, string? BottomComposition);

    /// <summary>The two transfer lengths of one half picture, which share its image's slot.</summary>
    internal sealed record PictureEntry(int FirstSize, int SecondSize)
    {
        /// <summary>The first transfer's byte count, 0 through 65535 (a DMA size).</summary>
        public int FirstSize { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, FirstSize);

        /// <summary>The second transfer's byte count, 0 through 65535 (a DMA size).</summary>
        public int SecondSize { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SecondSize);
    }
}
