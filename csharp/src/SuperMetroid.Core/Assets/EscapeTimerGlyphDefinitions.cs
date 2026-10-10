namespace SuperMetroid.Core.Assets;

/// <summary>Native timer glyph grouping and calculated outline/shape relationships. The exact residual font contours, four edge decisions and chosen typography have a narrow approved nonsense disposition: digit values do not determine this bitmap typeface. RGB colors, timing and other artwork are excluded.</summary>
internal static class EscapeTimerGlyphDefinitions
{
    /// <summary>$B0:C000..C2BF: decimal/separator fill pen1. Narrow approved chosen-paint identity: geometry does not determine its palette-slot number; RGB colors remain independently REQUIRED.</summary>
    internal const byte DigitFillPen = 1;
    /// <summary>$B0:C2C0..C31F: TIME fill pen2. Narrow approved chosen-paint identity, independently reviewed against native seed and OBJ palette5 routing; RGB colors remain independently REQUIRED.</summary>
    internal const byte LabelFillPen = 2;
    /// <summary>$B0:C000..C31F: outline pen14. Narrow approved chosen-paint identity; outline width is reviewed original-font design; RGB colors remain independently REQUIRED.</summary>
    internal const byte OutlinePen = 14;
    /// <summary>Native timer glyphs have a one-pixel eight-neighbor outline; selected width remains reviewed original-font design.</summary>
    internal const int ChosenOutlineWidth = 1;

    /// <summary>$B0:C2A0: the double separator repeats the single mark from$B0:C280 with a three-pixel advance; this chosen spacing remains reviewed original-font design.</summary>
    internal const int ChosenSeparatorAdvance = 3;

    /// <summary>$B0:C000/C140 and C020/C160: zero ring and one stem use two-pixel strokes; selected metric remains reviewed original-font design.</summary>
    internal const int ChosenDigitStrokeWidth = 2;
    /// <summary>$B0:C100: upper-eight fill ends at the eight-pixel half boundary; this chosen bowl extent remains reviewed original-font design.</summary>
    internal const int ChosenUpperEightFillEnd = 8;

    /// <summary>$B0:C040/C180: digit-two cap ends before row5; selected cap/stem join remains reviewed original-font design.</summary>
    internal const int ChosenTwoCapEnd = 5;
    /// <summary>$B0:C040/C180: digit-two right stem ends before row7; selected diagonal join remains reviewed original-font design.</summary>
    internal const int ChosenTwoStemEnd = 7;
    /// <summary>$B0:C040/C180: digit-two descending band advances one column per two rows; selected slope remains reviewed original-font design.</summary>
    internal const int ChosenTwoSlope = 2;

    /// <summary>$B0:C220: digit-seven terminal vertical stem begins at row10; selected start and join remain reviewed original-font design.</summary>
    internal const int ChosenSevenTerminalStart = 10;
    /// <summary>$B0:C220: digit-seven terminal stem begins at column2; selected horizontal placement remains reviewed original-font design.</summary>
    internal const int ChosenSevenTerminalX = 2;

    /// <summary>$B0:C2C0..C31F: TIME label stem width2 is a chosen reviewed original-font design metric independent of the digit stroke.</summary>
    internal const int ChosenLabelStemWidth = 2;
    /// <summary>$B0:C2C0: T begins at label origin and spans six columns; chosen width remains reviewed original-font design.</summary>
    internal const int ChosenLabelTWidth = 6;
    /// <summary>$B0:C2C0/C2E0: I begins at label column7; chosen placement remains reviewed original-font design.</summary>
    internal const int ChosenLabelIOrigin = 7;
    /// <summary>$B0:C300: E begins at label column18; chosen placement remains reviewed original-font design.</summary>
    internal const int ChosenLabelEOrigin = 18;
    /// <summary>$B0:C300: E spans five columns; chosen width remains reviewed original-font design.</summary>
    internal const int ChosenLabelEWidth = 5;
    /// <summary>$B0:C2C0..C31F: label fill occupies rows1..5; chosen vertical placement remains reviewed original-font design.</summary>
    internal const int ChosenLabelTop = 1, ChosenLabelHeight = 5;
    /// <summary>$B0:C300: E middle arm ends one column before its outer arms; chosen shape policy remains reviewed original-font design.</summary>
    internal const int ChosenLabelMiddleInset = 1;

    /// <summary>$B0:C160: the final lower tile row is one transparent padding row; selected baseline spacing remains reviewed original-font design.</summary>
    internal const int ChosenBottomPadding = 1;

    /// <summary>
    /// Native zero and upper-eight use rounded outer boxes with rectangular inner holes; digit1 uses a centered stem, left flag and wider foot.
    /// Digit2 joins a rounded cap, right stem, descending band and foot. Their selected metrics, endpoints and join policies remain reviewed original-font design; independent
    /// pixel edits are separate from this calculated default footprint. Digit7 only calculates its cap and terminal vertical stem; rows3..9 retain reviewed original-font contour membership.
    /// </summary>
    internal static bool TryDefaultFill(int pixel, out bool fill)
    {
        int tile = Tile(pixel);
        if (tile >= 22)
        {
            int labelX = (tile - 22) * 8 + pixel % 8, labelY = pixel / EscapeTimerTileAtlasFormat.Width;
            bool inHeight = labelY >= ChosenLabelTop && labelY < ChosenLabelTop + ChosenLabelHeight;
            if (labelX < ChosenLabelTWidth)
            {
                int stemLeft = (ChosenLabelTWidth - ChosenLabelStemWidth) / 2;
                fill = inHeight && (labelY == ChosenLabelTop || labelX >= stemLeft && labelX < stemLeft + ChosenLabelStemWidth);
                return true;
            }
            if (labelX >= ChosenLabelIOrigin && labelX < ChosenLabelIOrigin + ChosenLabelStemWidth)
            {
                fill = inHeight;
                return true;
            }
            if (labelX >= ChosenLabelEOrigin && labelX < ChosenLabelEOrigin + ChosenLabelEWidth)
            {
                int localX = labelX - ChosenLabelEOrigin;
                fill = inHeight && (localX < ChosenLabelStemWidth || labelY == ChosenLabelTop ||
                    labelY == ChosenLabelTop + ChosenLabelHeight - 1 ||
                    labelY == ChosenLabelTop + ChosenLabelHeight / 2 && localX < ChosenLabelEWidth - ChosenLabelMiddleInset);
                return true;
            }
            fill = false;
            return false;
        }
        if (tile is not (0 or 1 or 2 or 7 or 8 or 10 or 11 or 12 or 17)) { fill = false; return false; }
        int x = pixel % 8, y = pixel / EscapeTimerTileAtlasFormat.Width + (tile >= 10 ? 8 : 0);
        int left = (8 - ChosenDigitStrokeWidth) / 2;
        int bottom = tile == 8 ? ChosenUpperEightFillEnd : 2 * 8 - ChosenBottomPadding - ChosenOutlineWidth;
        if (tile is 7 or 17)
        {
            int capEnd = ChosenOutlineWidth + ChosenDigitStrokeWidth;
            if (y >= capEnd && y < ChosenSevenTerminalStart) { fill = false; return false; }
            bool cap = x >= ChosenOutlineWidth && x < 8 - ChosenOutlineWidth &&
                y >= ChosenOutlineWidth && y < capEnd;
            bool terminal = x >= ChosenSevenTerminalX && x < ChosenSevenTerminalX + ChosenDigitStrokeWidth &&
                y >= ChosenSevenTerminalStart && y < bottom;
            fill = cap || terminal;
            return true;
        }
        bool RoundedRing()
        {
            int outerLeft = ChosenOutlineWidth, outerRight = 8 - ChosenOutlineWidth;
            bool outer = x >= outerLeft && x < outerRight && y >= ChosenOutlineWidth && y < bottom;
            bool corner = (x == outerLeft || x == outerRight - 1) && (y == ChosenOutlineWidth || y == bottom - 1);
            bool hole = x >= outerLeft + ChosenDigitStrokeWidth && x < outerRight - ChosenDigitStrokeWidth &&
                y >= ChosenOutlineWidth + ChosenDigitStrokeWidth && y < bottom - ChosenDigitStrokeWidth;
            return outer && !corner && !hole;
        }
        if (tile is 0 or 8 or 10) { fill = RoundedRing(); return true; }
        if (tile is 2 or 12)
        {
            bool cap = RoundedRing() && y < ChosenTwoCapEnd;
            bool rightStem = x >= 8 - ChosenOutlineWidth - ChosenDigitStrokeWidth && x < 8 - ChosenOutlineWidth &&
                y >= ChosenOutlineWidth + ChosenDigitStrokeWidth && y < ChosenTwoStemEnd;
            int diagonalCenter = ChosenTwoSlope * ChosenOutlineWidth + bottom - 1;
            bool diagonal = y >= ChosenTwoStemEnd - 1 && y < bottom - ChosenDigitStrokeWidth &&
                Math.Abs(ChosenTwoSlope * x + y - diagonalCenter) <= ChosenDigitStrokeWidth;
            bool lowerFoot = x >= ChosenOutlineWidth && x < 8 - ChosenOutlineWidth &&
                y >= bottom - ChosenDigitStrokeWidth && y < bottom;
            fill = cap || rightStem || diagonal || lowerFoot;
            return true;
        }
        bool stem = x >= left && x < left + ChosenDigitStrokeWidth && y >= ChosenOutlineWidth && y < bottom;
        bool flag = x == left - ChosenOutlineWidth && y >= ChosenDigitStrokeWidth && y < 2 * ChosenDigitStrokeWidth;
        bool foot = x >= left - ChosenOutlineWidth && x < left + ChosenDigitStrokeWidth + ChosenOutlineWidth &&
            y >= bottom - ChosenDigitStrokeWidth && y < bottom;
        fill = stem || flag || foot;
        return true;
    }
    /// <summary>Twenty digit half-tiles, then two separator tiles and the three-tile TIME label.</summary>
    internal static byte FillInk(int pixel) => Tile(pixel) < 22 ? DigitFillPen : LabelFillPen;

    /// <summary>Native lower six/eight tiles B0:C200/C240 share zero lower B0:C140; zero upper B0:C000 reflects horizontally and lower rows0..6 reflect upper rows6..0. Upper eight at B0:C100 also reflects horizontally; the double separator repeats the single mark. Remaining original-font contours and spacing have the bounded reviewed disposition.</summary>
    internal static int SourcePixel(int pixel)
    {
        int tile = Tile(pixel), x = pixel % 8, y = pixel / EscapeTimerTileAtlasFormat.Width;
        if (tile is 16 or 18) tile = 10;
        if (tile == 10 && y < 7) { tile = 0; y = 6 - y; }
        if (tile is 0 or 8) x = Math.Min(x, 7 - x);
        if (tile == 21) { tile = 20; if (x >= 8 / 2) x -= ChosenSeparatorAdvance; }
        return y * EscapeTimerTileAtlasFormat.Width + tile * 8 + x;
    }
    /// <summary>Validates an atlas pixel index and returns its zero-based 8-pixel tile column.</summary>
    private static int Tile(int pixel)
    {
        if ((uint)pixel >= EscapeTimerTileAtlasFormat.Width * EscapeTimerTileAtlasFormat.Height) throw new ArgumentOutOfRangeException(nameof(pixel));
        return pixel % EscapeTimerTileAtlasFormat.Width / 8;
    }

    /// <summary>
    /// $80:9FE8..A07A and $B0:C000..C31F: decimal halves form ten8x16 glyphs,
    /// separators are8x8 and the TIME label is24x8. Only non-fill pixels are
    /// reconstructed; exactly four remaining native edge decisions and275 source fill sites retain the reviewed original-font design, including the specified blank complement.
    /// </summary>
    internal static byte Outline(int pixel, Func<int, bool> isFill)
    {
        int tile = Tile(pixel), x = pixel % 8, y = pixel / EscapeTimerTileAtlasFormat.Width;
        int width = tile >= 22 ? 24 : 8, height = tile < 20 ? 16 : 8;
        int glyphX = tile >= 22 ? (tile - 22) * 8 + x : x;
        int glyphY = tile is >= 10 and < 20 ? y + 8 : y;
        for (int dy = -ChosenOutlineWidth; dy <= ChosenOutlineWidth; dy++)
        for (int dx = -ChosenOutlineWidth; dx <= ChosenOutlineWidth; dx++)
        {
            int nx = glyphX + dx, ny = glyphY + dy;
            if ((uint)nx >= width || (uint)ny >= height) continue;
            int neighborTile = tile < 20 ? tile % 10 + ny / 8 * 10 : tile >= 22 ? 22 + nx / 8 : tile;
            int neighbor = ny % 8 * EscapeTimerTileAtlasFormat.Width + neighborTile * 8 + nx % 8;
            if (isFill(neighbor)) return OutlinePen;
        }
        return 0;
    }
}
