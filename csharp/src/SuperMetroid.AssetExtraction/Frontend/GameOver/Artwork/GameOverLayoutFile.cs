using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>
/// The installed game-over layout (#1638): the Baby's and the cursor's anchors and the cursor's frame duration, which the
/// game binds. The page, compositions, Baby colours and palette selectors, whose entries are the original's packed tile and
/// sprite placements, are import data (<see cref="GameOverPresentationFormat.ArtworkFileName"/>).
/// </summary>
public static class GameOverLayoutFile
{
    /// <summary>Supported layout revision.</summary>
    public const int Version = 1;

    /// <summary>The layout of the extracted artwork document <paramref name="artwork"/>, as the installation holds it.</summary>
    /// <exception cref="InvalidDataException">The artwork document does not decode.</exception>
    internal static byte[] Extract(byte[] artwork)
    {
        GameOverPresentationDocument document = JsonAssetDocument.Read<GameOverPresentationDocument>(artwork, ArtworkJson.Options)
            ?? throw new InvalidDataException("Game-over presentation document is null.");
        return JsonSerializer.SerializeToUtf8Bytes(new GameOverPlacementDocument
        {
            Version = Version, BabyAnchor = document.BabyAnchor, CursorX = document.CursorX, YesCursorY = document.YesCursorY,
            NoCursorY = document.NoCursorY, CursorFrameDuration = document.CursorFrameDuration,
        }, ArtworkJson.Options);
    }

    /// <summary>Reads and validates the installed layout the game binds.</summary>
    /// <exception cref="InvalidDataException">The JSON, version, coordinates or duration are invalid.</exception>
    public static GameOverPresentation Read(Stream json)
    {
        GameOverPlacementDocument document;
        try { document = JsonAssetDocument.Read<GameOverPlacementDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Game-over layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid game-over layout JSON.", error); }
        if (document.Version != Version)
            throw new InvalidDataException($"Game-over layout version must be {Version}.");
        return Presentation(new GameOverPlacements
        {
            BabyAnchor = document.BabyAnchor, CursorX = document.CursorX, YesCursorY = document.YesCursorY,
            NoCursorY = document.NoCursorY, CursorFrameDuration = document.CursorFrameDuration,
        });
    }

    /// <summary>The presentation <paramref name="placements"/> names, once its coordinates and duration are checked.</summary>
    /// <exception cref="InvalidDataException">A coordinate is outside 0..255, or the duration is below one update.</exception>
    internal static GameOverPresentation Presentation(GameOverPlacements placements)
    {
        ValidateCoordinate(placements.BabyAnchor.X, "Baby anchor X");
        ValidateCoordinate(placements.BabyAnchor.Y, "Baby anchor Y");
        ValidateCoordinate(placements.CursorX, nameof(placements.CursorX));
        ValidateCoordinate(placements.YesCursorY, nameof(placements.YesCursorY));
        ValidateCoordinate(placements.NoCursorY, nameof(placements.NoCursorY));
        if (placements.CursorFrameDuration < 1)
            throw new InvalidDataException("Game-over cursor frame duration must be at least one update.");
        return new GameOverPresentation(placements);
    }

    /// <summary>Rejects a screen coordinate outside the 0–255 range used by authored menu layout.</summary>
    private static void ValidateCoordinate(int value, string name)
    {
        if ((uint)value > byte.MaxValue)
            throw new InvalidDataException($"{name} must be 0..255.");
    }
}
