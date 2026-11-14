using SuperMetroid.Core.Rooms.LevelData;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Extracts what Samus's glitch paths read past their data, decoded from the player's cartridge into each read's domain
/// result (owner question 3, #1474), and loads it as the Samus overread services.
/// </summary>
public static class SamusOverreadFiles
{
    /// <summary>
    /// Decodes every glitch read and writes the results as JSON. No manifest is written (#1693): the document names its own
    /// format, and the cartridge it was decoded from is the installation receipt's. The colours Samus's palette loaders read
    /// past their tables are not decoded: each of her colour cycles shows only its own colours (#1736).
    /// </summary>
    /// <param name="bus">Cartridge source for the reads.</param>
    /// <param name="directory">Destination directory, created if absent; an existing file is replaced.</param>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        SamusOverreadDocument document = SamusOverreadDecoding.Decode(cartridge);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, SamusOverreadFormat.JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, SamusOverreadFormat.FileName), bytes);
    }

    /// <summary>Loads the installed file, as the player may have edited it.</summary>
    /// <param name="directory">Installed overread directory holding the file.</param>
    /// <param name="straddledTerrain">
    /// The installed terrain a Wave shot's scan past the room's ceiling finds straddling two blocks, which the projectiles'
    /// group of services carries (#1612).
    /// </param>
    /// <exception cref="InvalidDataException">The file's format or shape is invalid.</exception>
    public static SamusOverreadServices Load(string directory, StraddledTerrain<RoomTerrainBlock> straddledTerrain) =>
        SamusOverreadServicesReader.Read(LoadDocument(directory), straddledTerrain);

    /// <summary>
    /// Loads what an animation frame past a pose's program reads from the overread directory, which the import reads to
    /// compose the frames Samus's body can show (#1513).
    /// </summary>
    /// <param name="directory">The installed overread directory.</param>
    /// <exception cref="InvalidDataException">The file's format or an animation successor is invalid.</exception>
    internal static FramesBeyondAnimation<SamusPoseId, SamusPoseAnimationStep> LoadFramesBeyondAnimation(string directory) =>
        SamusOverreadServicesReader.ReadFramesBeyondAnimation(LoadDocument(directory));

    /// <summary>Reads the overread document in <paramref name="directory"/>; its readers check its format.</summary>
    private static SamusOverreadDocument LoadDocument(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        byte[] bytes = File.ReadAllBytes(Path.Combine(directory, SamusOverreadFormat.FileName));
        return Document(new MemoryStream(bytes, writable: false));
    }

    /// <summary>Parses an overread document.</summary>
    private static SamusOverreadDocument Document(Stream json) =>
        JsonAssetDocument.Read<SamusOverreadDocument>(json, SamusOverreadFormat.JsonOptions, "Samus overread");
}
