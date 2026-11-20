using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Four editable 3x2 Samus-eye BG2 rectangles; blink timing stays in code.</summary>
public sealed class IntroEyeTilemapPresentation
{
    /// <summary>The four installed 3-by-2 eye patches, each its six row-major tiles, by the look it shows.</summary>
    private readonly Dictionary<IntroEyeFrame, BackgroundTile[]> frames;

    /// <summary>Retains the four installed eye rectangles as loaded.</summary>
    private IntroEyeTilemapPresentation(Dictionary<IntroEyeFrame, BackgroundTile[]> frames) => this.frames = frames;

    /// <summary>Gets the six tiles of the eye drawing that shows <paramref name="eye"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eye"/> is no eye drawing.</exception>
    internal ReadOnlySpan<BackgroundTile> FrameTiles(IntroEyeFrame eye) =>
        frames.TryGetValue(eye, out BackgroundTile[]? tiles) ? tiles : throw new ArgumentOutOfRangeException(nameof(eye));

    /// <summary>Loads and validates the four ordered Samus portrait eye rectangles.</summary>
    public static IntroEyeTilemapPresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        IntroEyeTilemapDocument document = JsonAssetDocument.Read<IntroEyeTilemapDocument>(
            json, ArtworkJson.Options, "opening eye tilemap");
        if (document.Version != IntroEyeTilemapFormat.Version ||
            document.Frames is not { Length: IntroEyeTilemapFormat.FrameCount })
            throw new InvalidDataException("Opening eye tilemap requires four ordered frames.");

        var frames = new Dictionary<IntroEyeFrame, BackgroundTile[]>();
        for (int place = 0; place < IntroEyeTilemapFormat.FrameCount; place++)
        {
            IntroEyeFrame frame = IntroEyeTilemapFormat.Eyes[place];
            IntroEyeTilemapFrame? source = document.Frames[place];
            if (source is null || source.Id != IntroEyeTilemapFormat.FrameId(frame) ||
                source.Cells is not { Length: IntroEyeTilemapFormat.CellsPerFrame })
                throw new InvalidDataException($"Opening eye frame {frame} needs its stable ID and six cells.");
            var tiles = new BackgroundTile[IntroEyeTilemapFormat.CellsPerFrame];
            frames.Add(frame, tiles);
            for (int cellIndex = 0; cellIndex < tiles.Length; cellIndex++)
            {
                tiles[cellIndex] = source.Cells[cellIndex]?.Placement() ?? throw new InvalidDataException(
                    $"Opening eye frame {frame} cell {cellIndex} has an invalid tile or palette.");
            }
        }
        return new IntroEyeTilemapPresentation(frames);
    }

    /// <summary>Writes an opening-eye tilemap document as JSON.</summary>
    public static void Write(Stream json, IntroEyeTilemapDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        json.Write(bytes);
    }
}
