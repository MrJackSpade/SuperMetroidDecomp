using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Installs the ending scenes' backdrops (#1544): the solid colour each scene shows behind every layer, as one JSON
/// document a player edits in place. The import takes each from the first colour of the scene's whole screen of colours,
/// and the backdrops of the space behind the exploding planet from the first colour of each frame of the space's colour
/// effect (#1637); the game loads no other ending colour, as every picture it draws is pre-coloured.
/// </summary>
public static class EndingBackdropFiles
{
    /// <summary>The installed document's file name.</summary>
    internal const string FileName = "ending-backdrops.json";

    /// <summary>
    /// Revision 4 of the backdrop document, the only revision the reader accepts: 2 holds the space's backdrops (#1637), 3
    /// names no cartridge, which the installation receipt records (#1693), and 4 writes each colour as <c>#rrggbb</c> (#2564).
    /// </summary>
    public const int Version = 4;

    /// <summary>Writes the backdrops the cartridge's ending scenes show.</summary>
    /// <param name="bus">Cartridge import source for the scenes' colours.</param>
    /// <param name="directory">Destination directory, created if absent; the document must not already exist.</param>
    /// <exception cref="InvalidDataException">A scene's colour has an unrepresentable high bit, or a frame of the space's colours is missing.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        EndingPaletteSet colours = EndingPaletteImport.Read(bus);
        EndingSpaceBackdrops space = EndingSpaceBackdropColours.From(
            RoomPaletteFxPresentationFile.Read(new MemoryStream(RoomPaletteFxPresentationExtractor.Extract(bus), writable: false)));
        Directory.CreateDirectory(directory);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new EndingBackdropDocument
        {
            Version = Version,
            Escape = InstalledColourText.Format(colours[EndingPaletteId.Escape].Backdrop),
            Explosion = InstalledColourText.Format(colours[EndingPaletteId.Explosion].Backdrop),
            Credits = InstalledColourText.Format(colours[EndingPaletteId.Credits].Backdrop),
            PostCredits = InstalledColourText.Format(colours[EndingPaletteId.PostCredits].Backdrop),
            SpaceBehindExplodingPlanet = [.. EndingSpaceBackdropColours.InFrameOrder(space).Select(InstalledColourText.Format)],
        }, ArtworkJson.Options);
        using var output = new FileStream(Path.Combine(directory, FileName), FileMode.CreateNew, FileAccess.Write);
        output.Write(json);
    }

    /// <summary>Loads the installed backdrops without cartridge access.</summary>
    /// <param name="directory">Installed directory holding the backdrop document.</param>
    /// <exception cref="InvalidDataException">
    /// The document is invalid, names another version, holds a backdrop for other than every frame of the
    /// space's colours, or a colour is not written as <c>#rrggbb</c> or lies off the game's fade scale.
    /// </exception>
    public static EndingBackdrops Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, FileName);
        EndingBackdropDocument document = JsonAssetDocument.Read<EndingBackdropDocument>(
            new MemoryStream(File.ReadAllBytes(path), writable: false), ArtworkJson.Options, $"ending backdrops {path}");
        if (document.Version != Version)
            throw new InvalidDataException($"Ending backdrops {path} are revision {document.Version}, not {Version}.");
        if (document.SpaceBehindExplodingPlanet is not { Length: > 0 } space || space.Length != EndingSpaceBackdropColours.FrameCount)
            throw new InvalidDataException($"Ending backdrops {path}: the space behind the exploding planet requires a backdrop for each of its {EndingSpaceBackdropColours.FrameCount} frames.");
        return new EndingBackdrops(Colour(document.Escape, nameof(document.Escape)), Colour(document.Explosion, nameof(document.Explosion)),
            Colour(document.Credits, nameof(document.Credits)), Colour(document.PostCredits, nameof(document.PostCredits)),
            EndingSpaceBackdropColours.FromFrameOrder([.. space.Select((colour, frame) => Colour(colour, $"space frame {frame}"))]));

        RgbColor Colour(string? colour, string scene) => InstalledColourText.ParseOnFadeScale(colour, $"Ending backdrops {path}: the {scene} backdrop");
    }
}
