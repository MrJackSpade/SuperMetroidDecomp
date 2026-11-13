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
    public const string FileName = "ending-backdrops.json";

    /// <summary>Revision 2 of the backdrop document, the only revision the reader accepts: 2 holds the space's backdrops (#1637).</summary>
    public const int Version = 2;

    /// <summary>Writes the backdrops the cartridge's ending scenes show.</summary>
    /// <param name="bus">Cartridge import source for the scenes' colours.</param>
    /// <param name="directory">Destination directory, created if absent; the document must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the document; loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">A scene's colour has an unrepresentable high bit, or a frame of the space's colours is missing.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        EndingPaletteSet colours = EndingPaletteImport.Read(bus);
        EndingSpaceBackdrops space = EndingSpaceBackdropColours.From(
            RoomPaletteFxPresentationFile.Read(new MemoryStream(RoomPaletteFxPresentationExtractor.Extract(bus), writable: false)));
        Directory.CreateDirectory(directory);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new EndingBackdropDocument
        {
            Version = Version,
            SourceCartridgeSha256 = sourceCartridgeSha256,
            Escape = PaletteRgb5.From(colours[EndingPaletteId.Escape].Backdrop),
            Explosion = PaletteRgb5.From(colours[EndingPaletteId.Explosion].Backdrop),
            Credits = PaletteRgb5.From(colours[EndingPaletteId.Credits].Backdrop),
            PostCredits = PaletteRgb5.From(colours[EndingPaletteId.PostCredits].Backdrop),
            SpaceBehindExplodingPlanet = [.. EndingSpaceBackdropColours.InFrameOrder(space).Select(PaletteRgb5.From)],
        }, ArtworkJson.Options);
        using var output = new FileStream(Path.Combine(directory, FileName), FileMode.CreateNew, FileAccess.Write);
        output.Write(json);
    }

    /// <summary>Loads the installed backdrops without cartridge access.</summary>
    /// <param name="directory">Installed directory holding the backdrop document.</param>
    /// <exception cref="InvalidDataException">
    /// The document is invalid, names another version or cartridge, holds a backdrop for other than every frame of the
    /// space's colours, or a colour has a channel outside 0..31.
    /// </exception>
    public static EndingBackdrops Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, FileName);
        EndingBackdropDocument document = JsonAssetDocument.Read<EndingBackdropDocument>(
            new MemoryStream(File.ReadAllBytes(path), writable: false), ArtworkJson.Options, $"ending backdrops {path}");
        if (document.Version != Version
            || !string.Equals(document.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Ending backdrops {path} do not describe this installation.");
        if (document.SpaceBehindExplodingPlanet is not { Length: > 0 } space || space.Length != EndingSpaceBackdropColours.FrameCount)
            throw new InvalidDataException($"Ending backdrops {path}: the space behind the exploding planet requires a backdrop for each of its {EndingSpaceBackdropColours.FrameCount} frames.");
        return new EndingBackdrops(Colour(document.Escape, nameof(document.Escape)), Colour(document.Explosion, nameof(document.Explosion)),
            Colour(document.Credits, nameof(document.Credits)), Colour(document.PostCredits, nameof(document.PostCredits)),
            EndingSpaceBackdropColours.FromFrameOrder([.. space.Select((colour, frame) => Colour(colour, $"space frame {frame}"))]));

        RgbColor Colour(PaletteRgb5? colour, string scene) =>
            colour is { Red: >= 0 and <= FadeLevelColour.FullLevel, Green: >= 0 and <= FadeLevelColour.FullLevel, Blue: >= 0 and <= FadeLevelColour.FullLevel }
                ? colour.ToRgbColor()
                : throw new InvalidDataException($"Ending backdrops {path}: the {scene} backdrop requires red, green and blue in 0..{FadeLevelColour.FullLevel}.");
    }
}
