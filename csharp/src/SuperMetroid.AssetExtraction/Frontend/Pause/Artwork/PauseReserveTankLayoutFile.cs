using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The installed reserve-strip layout (#1638): the six anchors the game draws the strip at. The strip's compositions, whose
/// parts are the original's packed sprite placements, are import data (<see cref="PauseReserveTankFormat.ArtworkFileName"/>).
/// </summary>
public static class PauseReserveTankLayoutFile
{
    /// <summary>Supported layout revision.</summary>
    public const int Version = 1;

    /// <summary>The layout of the extracted artwork document <paramref name="artwork"/>, as the installation holds it.</summary>
    /// <exception cref="InvalidDataException">The artwork document does not decode.</exception>
    internal static byte[] Extract(byte[] artwork)
    {
        PauseReserveTankDocument document = JsonAssetDocument.Read<PauseReserveTankDocument>(artwork, ArtworkJson.Options)
            ?? throw new InvalidDataException("Reserve tank document is null.");
        return JsonSerializer.SerializeToUtf8Bytes(new PauseReserveTankLayoutDocument { Version = Version, Anchors = document.Anchors },
            ArtworkJson.Options);
    }

    /// <summary>Reads and validates the installed layout: the anchors the game binds.</summary>
    /// <exception cref="InvalidDataException">The JSON, version or anchors are invalid.</exception>
    public static PauseReserveTankPresentation Read(Stream json)
    {
        PauseReserveTankLayoutDocument document;
        try { document = JsonAssetDocument.Read<PauseReserveTankLayoutDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Reserve tank layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid reserve tank layout JSON.", error); }
        if (document.Version != Version)
            throw new InvalidDataException($"Reserve tank layout requires version {Version}.");
        return new PauseReserveTankPresentation(Anchors(document.Anchors));
    }

    /// <summary>The six strip anchors <paramref name="anchors"/> names, each a screen pixel.</summary>
    /// <exception cref="InvalidDataException">There are not six anchors, or one is off screen.</exception>
    internal static Position16<SpritePixels>[] Anchors(Position16<SpritePixels>[]? anchors)
    {
        if (anchors is null || anchors.Length != PauseReserveTankDefinitions.AnchorCount)
            throw new InvalidDataException("Reserve tanks require six anchors.");
        foreach (var point in anchors)
            if (point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException("Reserve tank anchor requires screen coordinates X=0..255/Y=0..223.");
        return anchors;
    }
}
