using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Right-wrap atlas placement at $8C:BAA9. Stock tile choices and
/// complete parts are calculated; independently edited choices remain inputs.</summary>
internal sealed class EndingLogoWrapParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Supplied tile choices that differ from the calculated 25-part stock wrap.</summary>
    private readonly EndingLogoWrapSelection[]? selection;
    /// <summary>Retains edited tile choices while representing an exact stock selection with no array.</summary>
    private EndingLogoWrapParts(EndingLogoWrapSelection[] supplied)
    {
        bool stock = supplied.Length == 25;
        for (int index = 0; stock && index < supplied.Length; index++)
            stock = supplied[index] == StockSelection(index);
        selection = stock ? null : supplied;
    }

    /// <summary>The stock wrap's 25 pieces in native OAM order: the half-turned closing arc (tips before spans), the caps
    /// right to left with the cropped left cap, the upper span, the left corner, the inner edge right to left, the patch
    /// bottom-right to top-left, the side junctions and endpoints, and the inner side bottom to top.</summary>
    private static readonly EndingLogoWrapSelection[] Stock =
    [
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.ClosingInnerRightTip), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.ClosingOuterLeftTip), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.ClosingRightSpan), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.ClosingLeftSpan), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.ClosingMiddleSpan), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.ClosingInnerLeftTip), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.ClosingRightCorner), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.RightCap), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.LeftCap), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.LeftCap), true),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.UpperSpanLeft), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.UpperSpanRight), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapCorner.LeftCorner), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.InnerEdgeRight), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.InnerEdgeLeft), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.PatchBottomRight), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.PatchBottomLeft), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.PatchTopRight), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.PatchTopLeft), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapCorner.LowerSideEndpoint), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapCorner.InnerSideJunction), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.InnerSideBottom), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapTile.InnerSideTop), false),
        new EndingLogoWrapSelection(Tile(EndingLogoWrapCorner.OuterSideJunction), false), new EndingLogoWrapSelection(Tile(EndingLogoWrapCorner.UpperSideEndpoint), false),
    ];

    /// <summary>The atlas tile number of <paramref name="tile"/>.</summary>
    private static int Tile(EndingLogoWrapTile tile) => EndingLogoWrapTileDefinitions.Tiles[tile];

    /// <summary>The atlas tile number of <paramref name="corner"/>.</summary>
    private static int Tile(EndingLogoWrapCorner corner) => EndingLogoWrapTileDefinitions.Corners[corner];

    /// <summary>Selects a native tile and crop flag in the wrap's 25-part OAM order.</summary>
    internal static EndingLogoWrapSelection StockSelection(int index) =>
        (uint)index < Stock.Length ? Stock[index] : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Calculates stock placement only when every supplied tile can be placed by the wrap geometry.</summary>
    internal static SpriteComposition CalculateIfMatching(SpriteComposition supplied)
    {
        var selection = new EndingLogoWrapSelection[supplied.PartCount];
        for (int index = 0; index < selection.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int tile = part.Part.Character;
            bool cropped = tile == Tile(EndingLogoWrapTile.LeftCap) && !part.Part.Large;
            if (!TryPlace(tile, cropped, out _)) return supplied;
            selection[index] = new EndingLogoWrapSelection(tile, cropped);
        }
        return supplied.CalculateIfMatching(new EndingLogoWrapParts(selection));
    }

    /// <summary>Places a selected atlas piece with native size, offsets, palette, and half-turn where needed.</summary>
    private static bool TryPlace(int tile, bool cropped, out CompiledSpritePart part)
    {
        int x, y;
        bool large = false, rotate = false;
        int column = tile % 16, row = tile / 16;
        bool isPiece = EndingLogoWrapTileDefinitions.TilePieces.TryGetValue(tile, out EndingLogoWrapTile piece);
        if (isPiece && piece is EndingLogoWrapTile.ClosingRightSpan or EndingLogoWrapTile.ClosingMiddleSpan or
            EndingLogoWrapTile.ClosingLeftSpan or EndingLogoWrapTile.ClosingOuterLeftTip or EndingLogoWrapTile.ClosingRightCorner or
            EndingLogoWrapTile.ClosingInnerLeftTip or EndingLogoWrapTile.ClosingInnerRightTip)
        {
            // Closing arc: a packed two-row region, half-turned into the lower edge.
            x = 8 * (column - 9) - 39;
            y = 8 * row - 57;
            large = piece is EndingLogoWrapTile.ClosingRightSpan or EndingLogoWrapTile.ClosingMiddleSpan or
                EndingLogoWrapTile.ClosingLeftSpan or EndingLogoWrapTile.ClosingRightCorner;
            rotate = true;
        }
        else if (isPiece && piece is EndingLogoWrapTile.LeftCap or EndingLogoWrapTile.RightCap)
        {
            // Overlapping top caps and an eight-pixel crop of the left cap.
            x = cropped ? 8 : 16 + 8 * (tile - Tile(EndingLogoWrapTile.LeftCap));
            y = cropped ? -24 : -32;
            large = !cropped;
        }
        else if (isPiece && piece is EndingLogoWrapTile.UpperSpanLeft or EndingLogoWrapTile.UpperSpanRight)
        {
            x = 8 * (tile - Tile(EndingLogoWrapTile.UpperSpanLeft)); y = -16; large = true;
        }
        else if (isPiece && piece is EndingLogoWrapTile.PatchTopLeft or EndingLogoWrapTile.PatchTopRight or
            EndingLogoWrapTile.PatchBottomLeft or EndingLogoWrapTile.PatchBottomRight)
        {
            // Upper-right patch is a two-by-two small-tile rectangle.
            x = 32 + 8 * (column - 8);
            y = -16 + 8 * (row - 6);
        }
        else if (isPiece && piece is EndingLogoWrapTile.InnerEdgeLeft or EndingLogoWrapTile.InnerEdgeRight)
        {
            x = -8 + 8 * (tile - Tile(EndingLogoWrapTile.InnerEdgeLeft)); y = 0;
        }
        else if (isPiece && piece is EndingLogoWrapTile.InnerSideTop or EndingLogoWrapTile.InnerSideBottom)
        {
            x = 24; y = -8 + 8 * row;
        }
        else
        {
            // Separately packed corners and vertical-edge endpoints.
            if (!EndingLogoWrapTileDefinitions.CornerPieces.TryGetValue(tile, out EndingLogoWrapCorner corner))
            {
                part = default;
                return false;
            }
            switch (corner)
            {
                case EndingLogoWrapCorner.LeftCorner: x = -8; y = -8; break;
                case EndingLogoWrapCorner.InnerSideJunction: x = 24; y = 8; break;
                case EndingLogoWrapCorner.LowerSideEndpoint: x = 32; y = 24; large = true; break;
                case EndingLogoWrapCorner.OuterSideJunction: x = 32; y = 8; large = true; break;
                case EndingLogoWrapCorner.UpperSideEndpoint: x = 32; y = -8; large = true; break;
                default: throw new InvalidOperationException($"Undefined {nameof(EndingLogoWrapCorner)} {tile:X2}.");
            }
        }
        int size = large ? 16 : 8;
        if (rotate) { x = -x - size; y = -y - size; }
        part = new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), large,
            tile, 0, 3, rotate
                ? TileFlip.Horizontal | TileFlip.Vertical : 0), true);
        return true;
    }

    /// <summary>Number of selected OAM pieces, 25 for the native wrap.</summary>
    public int Count => selection?.Length ?? 25;
    /// <summary>Calculated OAM part for a selected tile and optional cropped cap.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var chosen = selection is null ? StockSelection(index) : selection[index];
            _ = TryPlace(chosen.Tile, chosen.Cropped, out CompiledSpritePart part);
            return part;
        }
    }
    /// <summary>Yields placed pieces in their native drawing order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same placed-piece sequence to nongeneric collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
