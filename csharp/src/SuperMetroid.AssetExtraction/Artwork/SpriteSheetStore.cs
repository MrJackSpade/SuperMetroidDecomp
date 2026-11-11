using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One family's installed sheet manifests (#1392, #1633, #1635): in the family's directory, each sheet's manifest, with a
/// provenance manifest of their hashes. The sheets' pictures are the installation's picture store's
/// (<see cref="InstalledPictureStore"/>): each manifest names where the store holds each frame's picture, and a sheet read
/// from the family is assembled from the store. A player edits the installed files in place.
/// </summary>
internal sealed class SpriteSheetStore
{
    /// <summary>
    /// The provenance manifest's schema revision: 2 listed each sheet's image and frame manifest, and apart from them its
    /// recolour-region and blend maps (#1633); 3 lists each sheet's manifest only, as the pictures are the picture store's (#1635).
    /// </summary>
    private const int FormatVersion = 3;

    /// <summary>The directory of the family's sheet manifests.</summary>
    private readonly string directory;

    /// <summary>The provenance manifest's file name.</summary>
    private readonly string manifestFileName;

    /// <summary>Each installed sheet manifest's hash, as the provenance lists it.</summary>
    private readonly Dictionary<string, string> hashes;

    /// <summary>The installation's picture store, which holds the sheets' pictures.</summary>
    private readonly InstalledPictureStore pictures;

    /// <summary>Opens a family's manifests for reading.</summary>
    private SpriteSheetStore(string directory, string manifestFileName, Dictionary<string, string> hashes, InstalledPictureStore pictures)
    {
        this.directory = directory;
        this.manifestFileName = manifestFileName;
        this.hashes = hashes;
        this.pictures = pictures;
    }

    /// <summary>Writes a family's sheet manifests in the given order and the provenance manifest of their hashes.</summary>
    /// <param name="directory">The family's directory, created if absent.</param>
    /// <param name="manifestFileName">The provenance manifest's file name.</param>
    /// <param name="manifests">Each sheet's installed name and manifest.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    /// <exception cref="IOException">A manifest's file already exists.</exception>
    internal static void Write(string directory, string manifestFileName, IReadOnlyList<KeyValuePair<string, byte[]>> manifests,
        string sourceCartridgeSha256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.NotNull(manifests);
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string name, byte[] bytes) in manifests)
        {
            string file = FrameFile(name);
            using (var stream = new FileStream(Path.Combine(directory, file), FileMode.CreateNew, FileAccess.Write))
                stream.Write(bytes);
            hashes.Add(file, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        File.WriteAllBytes(Path.Combine(directory, manifestFileName), JsonSerializer.SerializeToUtf8Bytes(
            new Provenance(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions));
    }

    /// <summary>
    /// Opens a family's sheets for reading, after checking its provenance manifest, with the picture store installed beside
    /// the family's directory.
    /// </summary>
    /// <param name="directory">The family's installed sheet manifests.</param>
    /// <param name="manifestFileName">The provenance manifest's file name.</param>
    /// <param name="sheetCount">The sheets the directory holds, a manifest each.</param>
    /// <exception cref="InvalidDataException">The manifest does not describe this installation's sheets, or the store is invalid.</exception>
    internal static SpriteSheetStore Open(string directory, string manifestFileName, int sheetCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, manifestFileName);
        Provenance manifest = JsonSerializer.Deserialize<Provenance>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Sprite sheet manifest {manifestPath} is empty.");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != sheetCount)
            throw new InvalidDataException($"Sprite sheet manifest {manifestPath} does not describe this installation.");
        string content = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory)))
            ?? throw new ArgumentException($"Sheet directory {directory} has no installation above it.", nameof(directory));
        return new(directory, manifestFileName, manifest.Sha256,
            InstalledPictureStore.Open(Path.Combine(content, PictureStoreFiles.DirectoryName)));
    }

    /// <summary>
    /// Loads one sheet from its installed manifest and the picture store, as the player may have edited them. The image is
    /// assembled from the store when it is first drawn (#1430).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The provenance omits the manifest, the manifest is invalid, or a frame names a page or rectangle the store lacks.
    /// </exception>
    internal SpriteSheet Read(string name)
    {
        string file = FrameFile(name);
        if (!hashes.ContainsKey(file))
            throw new InvalidDataException($"Sprite sheet manifest {Path.Combine(directory, manifestFileName)} omits {Path.Combine(directory, file)}.");
        // A manifest a player edited in place is used as edited (#1532); it fails only where it is decoded, when it does not decode.
        InstalledSheetDocument document = SpriteSheetFile.ReadDocument(
            new MemoryStream(File.ReadAllBytes(Path.Combine(directory, file)), writable: false), name);
        pictures.RequireHolds(name, document);
        return SpriteSheetFile.Load(document, name, () => pictures.Assemble(document));
    }

    /// <summary>A sheet's manifest file name.</summary>
    internal static string FrameFile(string name) => name + ".json";

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>The stock manifests' provenance: schema, cartridge, and each sheet manifest's hash.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="SourceCartridgeSha256">The cartridge the files were extracted from.</param>
    /// <param name="Sha256">Each sheet manifest's hash, by file name.</param>
    private sealed record Provenance(int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256);
}
