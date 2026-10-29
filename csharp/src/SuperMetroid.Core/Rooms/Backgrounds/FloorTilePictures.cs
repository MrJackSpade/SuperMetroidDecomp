using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What each of the 256 tiles a floor map can name shows (#1392): its pre-coloured picture and the picture's corner;
/// nothing where its characters were never loaded and hold zeros; or, where characters nothing pictures were loaded over
/// it, that it is unpictured.
/// </summary>
public sealed class FloorTilePictures
{
    /// <summary>The tiles a floor map can name.</summary>
    public const int Tiles = 256;

    /// <summary>Each Ceres room sheet's tiles, once read, so a map keeps one table while its sheet stands.</summary>
    private static readonly ConditionalWeakTable<FloorTileSheet, FloorTilePictures> roomSheets = new();

    /// <summary>What each tile of a Ceres room's <paramref name="sheet"/> shows: its picture, or nothing where it is not pictured.</summary>
    public static FloorTilePictures Of(FloorTileSheet sheet) => roomSheets.GetValue(Ensure.NotNull(sheet), tiles => new FloorTilePictures(
        [.. Enumerable.Range(0, Tiles).Select(tile => !tiles.Pictured[tile] ? (BgTilePicture?)null : new BgTilePicture(tiles.Picture, FloorTileSheet.Corner(tile)))],
        [.. Enumerable.Range(0, Tiles).Select(tile => !tiles.Pictured[tile])]));

    /// <summary>Holds each tile's picture, and which tiles hold characters nothing pictures.</summary>
    /// <exception cref="ArgumentException">The pictures or the unpictured tiles do not cover every tile.</exception>
    public FloorTilePictures(ImmutableArray<BgTilePicture?> pictures, ImmutableArray<bool> unpictured)
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
}
