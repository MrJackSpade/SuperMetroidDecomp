using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One family's installed sheet manifests (#1392, #1633, #1635): in the family's directory, each sheet's manifest. The sheets'
/// pictures are the installation's picture store's (<see cref="InstalledPictureStore"/>): each manifest names where the store
/// holds each frame's picture, and a sheet read from the family is assembled from the store. No provenance manifest lists the
/// family's sheets (#1693): the family's own catalog or manifest names them. A player edits the installed files in place.
/// </summary>
internal sealed class SpriteSheetStore
{
    /// <summary>The directory of the family's sheet manifests.</summary>
    private readonly string directory;

    /// <summary>The installation's picture store, which holds the sheets' pictures.</summary>
    private readonly InstalledPictureStore pictures;

    /// <summary>Opens a family's manifests for reading.</summary>
    private SpriteSheetStore(string directory, InstalledPictureStore pictures)
    {
        this.directory = directory;
        this.pictures = pictures;
    }

    /// <summary>Writes a family's sheet manifests in the given order.</summary>
    /// <param name="directory">The family's directory, created if absent.</param>
    /// <param name="manifests">Each sheet's installed name and manifest.</param>
    /// <exception cref="IOException">A manifest's file already exists.</exception>
    internal static void Write(string directory, IReadOnlyList<KeyValuePair<string, byte[]>> manifests)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Ensure.NotNull(manifests);
        Directory.CreateDirectory(directory);
        foreach ((string name, byte[] bytes) in manifests)
        {
            using var stream = new FileStream(Path.Combine(directory, FrameFile(name)), FileMode.CreateNew, FileAccess.Write);
            stream.Write(bytes);
        }
    }

    /// <summary>Opens a family's sheets for reading, with the picture store installed beside the family's directory.</summary>
    /// <param name="directory">The family's installed sheet manifests.</param>
    /// <exception cref="InvalidDataException">The store is invalid.</exception>
    internal static SpriteSheetStore Open(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string content = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory)))
            ?? throw new ArgumentException($"Sheet directory {directory} has no installation above it.", nameof(directory));
        return new(directory, InstalledPictureStore.Open(Path.Combine(content, PictureStoreFiles.DirectoryName)));
    }

    /// <summary>
    /// Loads one sheet from its installed manifest and the picture store, as the player may have edited them. The image is
    /// assembled from the store when it is first drawn (#1430).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The family holds no manifest of the sheet, the manifest is invalid, or a frame names a page or rectangle the store lacks.
    /// </exception>
    internal SpriteSheet Read(string name)
    {
        string path = Path.Combine(directory, FrameFile(name));
        if (!File.Exists(path))
            throw new InvalidDataException($"Sprite sheet {name} has no installed manifest {path}.");
        // A manifest a player edited in place is used as edited (#1532); it fails only where it is decoded, when it does not decode.
        InstalledSheetDocument document = SpriteSheetFile.ReadDocument(new MemoryStream(File.ReadAllBytes(path), writable: false), name);
        pictures.RequireHolds(name, document);
        return SpriteSheetFile.Load(document, name, () => pictures.Assemble(name, document));
    }

    /// <summary>A sheet's manifest file name.</summary>
    internal static string FrameFile(string name) => name + ".json";
}
