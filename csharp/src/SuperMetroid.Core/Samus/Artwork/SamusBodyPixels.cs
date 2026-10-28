using static SuperMetroid.Core.Samus.Artwork.SamusBodyPixelDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which body pixels are canvas padding, diagnostic drawing, morph contour or shared anatomy, and where a shared pixel comes from.</summary>
internal static class SamusBodyPixels
{
    /// <summary>Whether a cell of the named $9B..9F body allocations is transparent padding.</summary>
    internal static bool IsBlank(bool upper, int set, int position, int tile) =>
        BlankCells.Contains(new SamusBodyTileKey(upper, set, position, tile));

    /// <summary>$9E:AC40/C0A0/C0C0 and $9D:B8E0 contain the same bounded
    /// Ep diagnostic drawing. Its selected pen1 and compact letter metrics are
    /// bitmap-font design; the three E bars and p bowl/stem calculate directly.</summary>
    internal static bool TryDiagnosticByte(bool upper, int set, int position, int index, out int value)
    {
        value = 0;
        if (set != 1 || !(upper ? position is 4 or 22 or 23 : position == 16) || (uint)index >= TileBytes)
            return false;
        if (index >= 16 || (index & 1) != 0) return true;
        int y = index / 2;
        for (int x = 0; x < 8; x++)
        {
            bool capitalE = y <= 4 && (x == 0 || x <= 2 && (y & 1) == 0);
            bool lowerP = x == 4 && y >= 3 || x is >= 4 and <= 6 && y is 3 or 5 || x == 7 && y == 4;
            if (capitalE || lowerP) value |= (byte)(128 >> x);
        }
        return true;
    }

    /// <summary>$9C:8000..84FF, eight morph drawings: a diameter-16 circle
    /// sampled at pixel centres. $92:BAB3 uses one large OBJ plus its small overlay;
    /// $92:BAE3 instead orders four small quadrants 4,3,2,1 and overlay0.
    /// The selected interior material and seam painting remain independent inputs.</summary>
    internal static byte ContourMask(bool upper, int set, int position, int index)
    {
        if (!upper || set != 10 || (uint)position >= 8 || (uint)index >= 5 * TileBytes)
            return byte.MaxValue;
        int tile = index / TileBytes;
        int quadrant;
        if (position == 4)
        {
            if (tile == 0) return byte.MaxValue;
            quadrant = 4 - tile;
        }
        else
        {
            if (tile == 2) return byte.MaxValue;
            quadrant = tile < 2 ? tile : tile - 1;
        }
        int y = (quadrant / 2) * 8 + (index % 16) / 2;
        int mask = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            int x = (quadrant % 2) * 8 + bit;
            int dx = 2 * x - 15, dy = 2 * y - 15;
            if (dx * dx + dy * dy <= 16 * 16) mask |= 1 << (7 - bit);
        }
        return (byte)mask;
    }

    /// <summary>$9E:E9C0..F6BF cable allocation: the second quartet repeats
    /// the first, exchanging the two16-pixel strips in its wide members.
    /// The bright terminal quartet repeats sources8/9/A/9; final entry10 repeats9.
    /// These are the exact bounded picture arrangements, not an unused-data exemption.</summary>
    internal static bool TrySourceByte(bool upper, int set, int position, int index,
        out int sourcePosition, out int sourceIndex)
    {
        sourcePosition = position;
        sourceIndex = index;
        if (!upper && set == 3)
        {
            if (position is >= 4 and <= 7)
            {
                sourcePosition = position - 4;
                if ((position & 1) != 0)
                {
                    int tile = index / TileBytes;
                    sourceIndex = (tile / 4 * 4 + (tile + 2) % 4) * TileBytes + index % TileBytes;
                }
                return true;
            }
            if (position is 11 or 16) sourcePosition = 9;
            else if (position is >= 12 and <= 15) sourcePosition = position - 4;
            return sourcePosition != position;
        }
        if (!SharesPreviousAnatomy(upper, set, position, index / TileBytes)) return false;
        sourcePosition = position - 1;
        return true;
    }

    /// <summary>Upright/inverted grapple upper sets3/5 share the same clockwise
    /// anatomical patch domains; anticlockwise sets4/6 share their corresponding
    /// domains. The earlier angle supplies the unchanged patch, while the arm
    /// and other redrawn patches retain their independently supplied content.</summary>
    internal static bool SharesPreviousAnatomy(bool upper, int set, int position, int tile)
    {
        if (!upper) return false;
        // $9C:D2C0..DFC0: drained rocking/standing breathing keeps the same
        // torso image; selected OAM origins and lower halves provide the motion.
        if (set == 7 && position < 12)
            return position switch
            {
                1 or 2 or 7 or 8 => tile < 9,
                3 or 9 => tile < 2,
                4 => tile is < 4 or >= 7 and < 10,
                5 or 10 or 11 => tile < 10,
                _ => false,
            };
        if (set is 3 or 5)
        {
            int column = tile % 6;
            return tile < 12 && position switch
            {
                4 => column is 4 or 5,
                5 or 12 or 14 => column >= 2,
                6 or 11 or 13 => column < 2,
                16 => column is 2 or 3,
                _ => false,
            };
        }
        if (set is 4 or 6)
        {
            int column = tile % 8;
            return tile < 16 && position switch
            {
                1 => column < 4 || tile is 12 or 13,
                3 or 14 => true,
                6 => column < 4,
                11 => tile == 4,
                12 => column < 4 || tile is 14 or 15,
                16 => column < 4 || tile is 4 or 5 or 6,
                _ => false,
            };
        }
        return false;
    }
}
