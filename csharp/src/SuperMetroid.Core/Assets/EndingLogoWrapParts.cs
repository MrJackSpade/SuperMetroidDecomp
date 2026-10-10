using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Right-wrap atlas placement at $8C:BAA9. Stock tile choices and
/// complete parts are calculated; independently edited choices remain inputs.</summary>
internal sealed class EndingLogoWrapParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Null when the standard 25-part atlas layout is in use; otherwise stores
    /// the caller's tile and crop choices for a customized composition.</summary>
    private readonly (int Tile, bool Cropped)[]? selection;

    /// <summary>Retains custom choices, but uses the compact stock representation when
    /// the supplied sequence exactly matches the native 25-piece layout.</summary>
    /// <param name="supplied">Tile and crop choices in the composition's part order.</param>
    private EndingLogoWrapParts((int Tile, bool Cropped)[] supplied)
    {
        bool stock = supplied.Length == 25;
        for (int index = 0; stock && index < supplied.Length; index++)
            stock = supplied[index] == StockSelection(index);
        selection = stock ? null : supplied;
    }

    /// <summary>Names the seven entries in the rotated closing arc's native tile order.</summary>
    private enum ClosingPiece
    {
        /// <summary>First arc entry, forming the inner tip at the right end.</summary>
        InnerRightTip,
        /// <summary>Second arc entry, forming the outer tip at the left end.</summary>
        OuterLeftTip,
        /// <summary>Wide right-hand segment of the closing arc.</summary>
        RightSpan,
        /// <summary>Wide left-hand segment of the closing arc.</summary>
        LeftSpan,
        /// <summary>Central wide segment joining the two sides of the arc.</summary>
        MiddleSpan,
        /// <summary>Inner tip that closes the left side of the arc.</summary>
        InnerLeftTip,
        /// <summary>Corner segment that joins the arc to the right wrap.</summary>
        RightCorner
    }

    /// <summary>Maps a zero-based part position to the stock atlas tile and crop choice.
    /// Positions follow the native closing arc, top edge, patch, and right-side ordering.</summary>
    /// <param name="index">Zero-based position in the standard 25-part composition.</param>
    /// <returns>The atlas tile number and whether the tile is cropped for that position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The position is outside the stock composition.</exception>
    internal static (int Tile, bool Cropped) StockSelection(int index)
    {
        if ((uint)index >= 25) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 7)
        {
            // Closing arc is rotated. Its native order puts tips before spans.
            int tile = (ClosingPiece)index switch
            {
                ClosingPiece.InnerRightTip => 0x2c,
                ClosingPiece.OuterLeftTip => 0x0f,
                ClosingPiece.RightSpan => 0x09,
                ClosingPiece.LeftSpan => 0x0d,
                ClosingPiece.MiddleSpan => 0x0b,
                ClosingPiece.InnerLeftTip => 0x2b,
                ClosingPiece.RightCorner => 0x29,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
            return (tile, false);
        }
        if (index < 9) return (0x49 - (index - 7), false); // caps, right to left
        if (index == 9) return (0x48, true); // cropped left cap
        if (index < 12) return (0x4c + 2 * (index - 10), false); // upper span, left to right
        if (index == 12) return (EndingLogoWrapAtlas.LeftCorner, false);
        if (index < 15) return (0x6b - (index - 13), false); // inner edge, right to left
        if (index < 19)
        {
            int cell = index - 15;
            return (0x79 - cell / 2 * 16 - cell % 2, false); // patch, bottom-right to top-left
        }
        if (index == 19) return (EndingLogoWrapAtlas.LowerSideEndpoint, false);
        if (index == 20) return (EndingLogoWrapAtlas.InnerSideJunction, false);
        if (index < 23) return (0x10 - (index - 21) * 16, false); // inner side, bottom to top
        return (index == 23 ? EndingLogoWrapAtlas.OuterSideJunction : EndingLogoWrapAtlas.UpperSideEndpoint, false);
    }

    /// <summary>Recalculates placement for a composition whose tiles match this atlas layout.
    /// An unrecognized tile is left untouched because its placement cannot be inferred here.</summary>
    /// <param name="supplied">The composition to inspect and, when recognized, reposition.</param>
    /// <returns>The recalculated composition when every tile is supported; otherwise the original instance.</returns>
    internal static SpriteComposition CalculateIfMatching(SpriteComposition supplied)
    {
        var selection = new (int Tile, bool Cropped)[supplied.PartCount];
        for (int index = 0; index < selection.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int tile = part.Attributes.TileNumber;
            if (!TryPlace(tile, tile == 0x48 && !part.X.IsLarge, out _)) return supplied;
            selection[index] = (tile, tile == 0x48 && !part.X.IsLarge);
        }
        return supplied.CalculateIfMatching(new EndingLogoWrapParts(selection));
    }

    /// <summary>Builds the positioned sprite part for a tile used by the right-wrap atlas.</summary>
    /// <param name="tile">Atlas tile number whose wrap position is being calculated.</param>
    /// <param name="cropped">Whether the left top cap uses its cropped placement.</param>
    /// <param name="part">Receives the compiled part when the tile has a known wrap placement.</param>
    /// <returns><see langword="true"/> when the tile is recognized; otherwise <see langword="false"/>.</returns>
    private static bool TryPlace(int tile, bool cropped, out CompiledSpritePart part)
    {
        int x, y;
        bool large = false, rotate = false;
        if (tile is 0x09 or 0x0b or 0x0d or 0x0f or 0x29 or 0x2b or 0x2c)
        {
            // Closing arc: a packed two-row region, half-turned into the lower edge.
            x = 8 * ((tile & 15) - 9) - 39;
            y = 8 * (tile / 16) - 57;
            large = tile is 0x09 or 0x0b or 0x0d or 0x29;
            rotate = true;
        }
        else if (tile is 0x48 or 0x49)
        {
            // Overlapping top caps and an eight-pixel crop of the left cap.
            x = cropped ? 8 : 16 + 8 * (tile - 0x48);
            y = cropped ? -24 : -32;
            large = !cropped;
        }
        else if (tile is 0x4c or 0x4e)
        {
            x = 8 * (tile - 0x4c); y = -16; large = true;
        }
        else if (tile is 0x68 or 0x69 or 0x78 or 0x79)
        {
            // Upper-right patch is a two-by-two small-tile rectangle.
            x = 32 + 8 * ((tile & 15) - 8);
            y = -16 + 8 * (tile / 16 - 6);
        }
        else if (tile is 0x6a or 0x6b)
        {
            x = -8 + 8 * (tile - 0x6a); y = 0;
        }
        else if (tile is 0x00 or 0x10)
        {
            x = 24; y = -8 + 8 * (tile / 16);
        }
        else
        {
            // Separately packed corners and vertical-edge endpoints.
            switch (tile)
            {
                case EndingLogoWrapAtlas.LeftCorner: x = -8; y = -8; break;
                case EndingLogoWrapAtlas.InnerSideJunction: x = 24; y = 8; break;
                case EndingLogoWrapAtlas.LowerSideEndpoint: x = 32; y = 24; large = true; break;
                case EndingLogoWrapAtlas.OuterSideJunction: x = 32; y = 8; large = true; break;
                case EndingLogoWrapAtlas.UpperSideEndpoint: x = 32; y = -8; large = true; break;
                default: part = default; return false;
            }
        }
        int size = large ? 16 : 8;
        if (rotate) { x = -x - size; y = -y - size; }
        part = new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
            SnesObjAttributeWord.Create(tile, 0, 3, rotate
                ? SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical : 0), true);
        return true;
    }

    /// <summary>Number of parts represented by the custom selection, or all 25 stock parts.</summary>
    public int Count => selection?.Length ?? 25;

    /// <summary>Gets the positioned part at a zero-based composition position.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside this selection.</exception>
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
    /// <summary>Enumerates positioned parts in the composition's native order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Separately packed corner and side pieces in the ending logo atlas.</summary>
internal static class EndingLogoWrapAtlas
{
    /// <summary>Tile$5B, eight-pixel left corner of the right-wrap composition.</summary>
    internal const int LeftCorner = 0x5b;
    /// <summary>Tile$6C, eight-pixel junction at the inner side of the right wrap.</summary>
    internal const int InnerSideJunction = 0x6c;
    /// <summary>Tile$20, sixteen-pixel lower endpoint of the right side.</summary>
    internal const int LowerSideEndpoint = 0x20;
    /// <summary>Tile$6D, sixteen-pixel outer side junction.</summary>
    internal const int OuterSideJunction = 0x6d;
    /// <summary>Tile$01, sixteen-pixel upper endpoint of the right side.</summary>
    internal const int UpperSideEndpoint = 0x01;
}
