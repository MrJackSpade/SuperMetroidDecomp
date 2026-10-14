using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Right-wrap atlas placement at $8C:BAA9. Stock tile choices and
/// complete parts are calculated; independently edited choices remain inputs.</summary>
internal sealed class EndingLogoWrapParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Supplied tile choices that differ from the calculated 25-part stock wrap.</summary>
    private readonly (int Tile, bool Cropped)[]? selection;
    /// <summary>Retains edited tile choices while representing an exact stock selection with no array.</summary>
    private EndingLogoWrapParts((int Tile, bool Cropped)[] supplied)
    {
        bool stock = supplied.Length == 25;
        for (int index = 0; stock && index < supplied.Length; index++)
            stock = supplied[index] == StockSelection(index);
        selection = stock ? null : supplied;
    }

    /// <summary>Native ordering of the seven half-turned closing-arc pieces.</summary>
    private enum ClosingPiece
    {
        /// <summary>Inner tip on the right of the closing arc.</summary>
        InnerRightTip,
        /// <summary>Outer tip on the left of the closing arc.</summary>
        OuterLeftTip,
        /// <summary>Right section between the closing tips.</summary>
        RightSpan,
        /// <summary>Left section between the closing tips.</summary>
        LeftSpan,
        /// <summary>Center section of the closing arc.</summary>
        MiddleSpan,
        /// <summary>Inner tip on the left of the closing arc.</summary>
        InnerLeftTip,
        /// <summary>Corner at the right end of the closing arc.</summary>
        RightCorner,
    }

    /// <summary>Selects a native tile and crop flag in the wrap's 25-part OAM order.</summary>
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
        if (index == 12) return ((int)EndingLogoWrapCorner.LeftCorner, false);
        if (index < 15) return (0x6b - (index - 13), false); // inner edge, right to left
        if (index < 19)
        {
            int cell = index - 15;
            return (0x79 - cell / 2 * 16 - cell % 2, false); // patch, bottom-right to top-left
        }
        if (index == 19) return ((int)EndingLogoWrapCorner.LowerSideEndpoint, false);
        if (index == 20) return ((int)EndingLogoWrapCorner.InnerSideJunction, false);
        if (index < 23) return (0x10 - (index - 21) * 16, false); // inner side, bottom to top
        return (index == 23 ? (int)EndingLogoWrapCorner.OuterSideJunction : (int)EndingLogoWrapCorner.UpperSideEndpoint, false);
    }

    /// <summary>Calculates stock placement only when every supplied tile can be placed by the wrap geometry.</summary>
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

    /// <summary>Places a selected atlas piece with native size, offsets, palette, and half-turn where needed.</summary>
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
            if (!Enum.IsDefined((EndingLogoWrapCorner)tile))
            {
                part = default;
                return false;
            }
            switch ((EndingLogoWrapCorner)tile)
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
        part = new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
            SnesObjAttributeWord.Create(tile, 0, 3, rotate
                ? SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical : 0), true);
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
