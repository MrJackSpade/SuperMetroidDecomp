using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>Reads and writes the installed ending palette document.</summary>
public static class EndingPaletteFile
{
    /// <summary>Validates RGB5 JSON against the chosen resource's exact color count and compiles an independent compiled color representation.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <param name="id">Palette role determining the required full resource size, not the size of an individual CGRAM transfer.</param>
    /// <returns>Selected colors copied from the document; a logo crossfade matching the interpolation rule is represented by its endpoints without changing any supplied colors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
    /// <exception cref="InvalidDataException">The JSON is invalid or ambiguous, the version or color count is wrong, or a color is null or has a channel outside 0..31.</exception>
    public static EndingPalette Read(Stream json, EndingPaletteId id)
    {
        EndingPaletteDocument document = JsonAssetDocument.Read<EndingPaletteDocument>(
            json, ArtworkJson.Options, $"ending {id} palette");
        int count = EndingPaletteDefinitions.ColorCount(id);
        if (document.Version != EndingPaletteDefinitions.Version ||
            document.Colors is null || document.Colors.Length != count)
            throw new InvalidDataException($"Ending {id} palette requires {count} RGB5 colors.");
        var compiled = new Bgr555[count];
        for (int index = 0; index < count; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ending {id} palette color {index} requires red, green and blue in 0..31.");
            compiled[index] = color.ToBgr555();
        }
        return EndingPalette.FromColours(id, compiled);
    }

    /// <summary>Serializes and validates the entire selected palette before writing its UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; trailing bytes are not truncated.</param>
    /// <param name="id">Palette role used to validate the document's color count.</param>
    /// <param name="document">Authored color collection read for serialization, not retained by the writer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Read"/>'s schema or RGB5 validation.</exception>
    public static void Write(Stream json, EndingPaletteId id, EndingPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false), id);
        json.Write(bytes);
    }
}
