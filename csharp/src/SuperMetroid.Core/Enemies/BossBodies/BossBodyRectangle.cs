namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A rectangle of a boss body's area (#1515): where a frame's picture shows and covers what lies beneath it, or where a
/// shown picture is placed.
/// </summary>
/// <param name="Corner">The body pixel its top-left corner lies on.</param>
/// <param name="Size">Its width and height in body pixels.</param>
public readonly record struct BossBodyRectangle(Position16<BackgroundPixels> Corner, Extent16<BackgroundPixels> Size)
{
    /// <summary>The body column of its left edge.</summary>
    public int Left => Corner.X;

    /// <summary>The body row of its top edge.</summary>
    public int Top => Corner.Y;

    /// <summary>The body column just past its right edge.</summary>
    public int Right => Corner.X + Size.X;

    /// <summary>The body row just past its bottom edge.</summary>
    public int Bottom => Corner.Y + Size.Y;

    /// <summary>Whether it covers no pixel.</summary>
    public bool IsEmpty => Size.X == 0 || Size.Y == 0;

    /// <summary>The rectangle from column <paramref name="left"/> and row <paramref name="top"/> up to, not including, <paramref name="right"/> and <paramref name="bottom"/>; empty where they do not lie past the first.</summary>
    public static BossBodyRectangle Between(int left, int top, int right, int bottom) =>
        right <= left || bottom <= top ? default : new(new(left, top), new(right - left, bottom - top));

    /// <summary>The part of this rectangle inside <paramref name="other"/>; empty where they do not meet.</summary>
    public BossBodyRectangle Within(BossBodyRectangle other) =>
        Between(Math.Max(Left, other.Left), Math.Max(Top, other.Top), Math.Min(Right, other.Right), Math.Min(Bottom, other.Bottom));

    /// <summary>
    /// The parts of this rectangle outside every one of <paramref name="others"/>, as rectangles that do not overlap: the
    /// rows above and below each, then the columns beside it.
    /// </summary>
    public IReadOnlyList<BossBodyRectangle> Without(IEnumerable<BossBodyRectangle> others)
    {
        List<BossBodyRectangle> left = IsEmpty ? [] : [this];
        foreach (BossBodyRectangle other in Ensure.NotNull(others))
        {
            if (other.IsEmpty)
                continue;
            var next = new List<BossBodyRectangle>(left.Count);
            foreach (BossBodyRectangle piece in left)
            {
                if (piece.Within(other).IsEmpty)
                {
                    next.Add(piece);
                    continue;
                }
                int top = Math.Max(piece.Top, other.Top), bottom = Math.Min(piece.Bottom, other.Bottom);
                foreach (BossBodyRectangle remains in new[]
                {
                    Between(piece.Left, piece.Top, piece.Right, top),
                    Between(piece.Left, bottom, piece.Right, piece.Bottom),
                    Between(piece.Left, top, Math.Min(piece.Right, other.Left), bottom),
                    Between(Math.Max(piece.Left, other.Right), top, piece.Right, bottom),
                })
                {
                    if (!remains.IsEmpty)
                        next.Add(remains);
                }
            }
            left = next;
        }
        return left;
    }
}
