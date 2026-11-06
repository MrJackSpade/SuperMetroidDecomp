using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss body as a gameplay frame shows it (#1466, #1514): pre-coloured pictures placed where they show on the body's
/// area, which repeats beyond its edges; the view places the area on screen. The pieces never overlap, and a pixel no
/// piece covers draws nothing (#1513).
/// </summary>
/// <param name="Pieces">The pieces showing.</param>
/// <param name="Area">The area the body covers before it repeats, in whole pages.</param>
public sealed record ShownBossBody(ImmutableArray<ShownBossBodyPiece> Pieces, Extent16<BackgroundPixels> Area)
{
    /// <summary>Pixels across and down a page of the body's area.</summary>
    private const int PagePixels = BossBodyDefinitions.ColumnCount * BossBodyDefinitions.CellPixels;

    /// <summary>
    /// The pieces <paramref name="picture"/> shows on a body covering <paramref name="area"/>, its top edge on the body's
    /// row <paramref name="top"/>, over the cells <paramref name="shows"/> covers: each run of cells a row shows, joined with
    /// the same run of the rows below it on the same page, is one rectangle. The body's pages lie left to right and then
    /// downward across the area; rows on a page beyond it do not show. The picture repeats across from the body's left
    /// edge and down from row <paramref name="top"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The picture is empty, or the area is not whole pages.</exception>
    public static IReadOnlyList<ShownBossBodyPiece> PiecesOf(SpriteSheetFrame picture, int top, BossBodyFootprint shows, Extent16<BackgroundPixels> area)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(shows);
        if (picture.IsEmpty)
            throw new ArgumentException("A boss body part that shows nothing has no picture.", nameof(picture));
        if (area.X == 0 || area.Y == 0 || area.X % PagePixels != 0 || area.Y % PagePixels != 0)
            throw new ArgumentException($"A boss body covers whole pages, not {area.X}x{area.Y} pixels.", nameof(area));
        int pagesAcross = area.X / PagePixels;
        int end = Math.Min(shows.End, pagesAcross * (area.Y / PagePixels) * BossBodyDefinitions.PageRows);
        var pieces = new List<ShownBossBodyPiece>();
        var open = new List<CellRun>();
        for (int row = shows.Top; row < end; row++)
        {
            // A page's first row starts every rectangle afresh: the page above it may lie elsewhere on the area.
            if (row % BossBodyDefinitions.PageRows == 0)
            {
                foreach (CellRun ended in open)
                    pieces.Add(Piece(ended, row));
                open.Clear();
            }
            uint mask = shows.Row(row);
            var continuing = new List<CellRun>();
            int column = 0;
            while (column < BossBodyDefinitions.ColumnCount)
            {
                if ((mask >> column & 1) == 0)
                {
                    column++;
                    continue;
                }
                int first = column;
                while (column < BossBodyDefinitions.ColumnCount && (mask >> column & 1) != 0)
                    column++;
                int index = open.FindIndex(run => run.FirstColumn == first && run.Columns == column - first);
                if (index >= 0)
                {
                    continuing.Add(open[index]);
                    open.RemoveAt(index);
                }
                else
                {
                    continuing.Add(new CellRun(first, column - first, row));
                }
            }
            foreach (CellRun ended in open)
                pieces.Add(Piece(ended, row));
            open = continuing;
        }
        foreach (CellRun ended in open)
            pieces.Add(Piece(ended, end));
        return pieces;

        // The rectangle of `run`'s cells from its first row up to `endRow`, on its page's place in the area.
        ShownBossBodyPiece Piece(CellRun run, int endRow)
        {
            const int cell = BossBodyDefinitions.CellPixels;
            int page = run.FirstRow / BossBodyDefinitions.PageRows;
            int x = page % pagesAcross * PagePixels + run.FirstColumn * cell;
            int y = page / pagesAcross * PagePixels + run.FirstRow % BossBodyDefinitions.PageRows * cell;
            int pictureX = run.FirstColumn * cell % picture.Size.X;
            int pictureY = ((run.FirstRow - top) * cell % picture.Size.Y + picture.Size.Y) % picture.Size.Y;
            return new ShownBossBodyPiece(picture, new Position16<BackgroundPixels>(x, y),
                new Extent16<BackgroundPixels>(run.Columns * cell, (endRow - run.FirstRow) * cell), new Position16<ImagePixels>(pictureX, pictureY));
        }
    }

    /// <summary>A run of a row's cells, continuing down through the rows that show the same run.</summary>
    /// <param name="firstColumn">The run's first column.</param>
    /// <param name="columns">The cells in the run.</param>
    /// <param name="firstRow">The first row showing the run.</param>
    private sealed class CellRun(int firstColumn, int columns, int firstRow)
    {
        /// <summary>The run's first column.</summary>
        public int FirstColumn { get; } = firstColumn;

        /// <summary>The cells in the run.</summary>
        public int Columns { get; } = columns;

        /// <summary>The first row showing the run.</summary>
        public int FirstRow { get; } = firstRow;
    }
}
