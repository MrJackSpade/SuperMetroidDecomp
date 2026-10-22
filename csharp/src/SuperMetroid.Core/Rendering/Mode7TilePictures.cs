using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What each of the 256 tiles a Mode 7 map can name shows (#1392): its pre-coloured picture and the picture's corner;
/// nothing where its characters were never loaded and hold zeros; or, where characters nothing pictures were loaded over
/// it, that it is unpictured.
/// </summary>
public sealed class Mode7TilePictures
{
    /// <summary>The tiles a Mode 7 map can name.</summary>
    public const int Tiles = 256;

    /// <summary>Each Ceres room sheet's tiles, once read, so a map keeps one table while its sheet stands.</summary>
    private static readonly ConditionalWeakTable<Mode7TileSheet, Mode7TilePictures> roomSheets = new();

    /// <summary>What each tile of a Ceres room's <paramref name="sheet"/> shows: its picture, or nothing where it is not pictured.</summary>
    public static Mode7TilePictures Of(Mode7TileSheet sheet) => roomSheets.GetValue(Ensure.NotNull(sheet), tiles => new Mode7TilePictures(
        [.. Enumerable.Range(0, Tiles).Select(tile => tiles.Shown[tile] is null ? (BgTilePicture?)null : new BgTilePicture(tiles.Picture, Mode7TileSheet.Corner(tile)))],
        [.. Enumerable.Range(0, Tiles).Select(tile => tiles.Shown[tile] is null)]));

    /// <summary>Holds each tile's picture, and which tiles hold characters nothing pictures.</summary>
    /// <exception cref="ArgumentException">The pictures or the unpictured tiles do not cover every tile.</exception>
    public Mode7TilePictures(ImmutableArray<BgTilePicture?> pictures, ImmutableArray<bool> unpictured)
    {
        Ensure.LengthEqual(pictures.AsSpan(), Tiles);
        Ensure.LengthEqual(unpictured.AsSpan(), Tiles);
        Pictures = pictures;
        Unpictured = unpictured;
        Sheets = [.. pictures.OfType<BgTilePicture>().Select(tile => tile.Picture).Distinct()];
    }

    /// <summary>Each tile's picture, or null where it shows nothing pictured.</summary>
    public ImmutableArray<BgTilePicture?> Pictures { get; }

    /// <summary>Whether each tile holds characters nothing pictures, rather than the zeros of characters never loaded.</summary>
    public ImmutableArray<bool> Unpictured { get; }

    /// <summary>The pictures the tiles come from, each once.</summary>
    public ImmutableArray<SpriteSheetFrame> Sheets { get; }

    /// <summary>What tile <paramref name="tile"/> shows at <paramref name="pixel"/> of its character; false where its characters were never loaded.</summary>
    /// <exception cref="InvalidDataException">The tile holds characters nothing pictures.</exception>
    public bool TrySample(int tile, Position16<CharacterPixels> pixel, out BgSample sample)
    {
        if (Pictures[tile] is not { } picture)
        {
            sample = default;
            return Unpictured[tile] ? throw new InvalidDataException($"The Mode 7 map names tile {tile}, whose characters nothing pictured shows.") : false;
        }
        sample = BgSample.OfPicture(picture.Picture, new Position16<ImagePixels>((ushort)(picture.Corner.X + pixel.X), (ushort)(picture.Corner.Y + pixel.Y)));
        return true;
    }
}
