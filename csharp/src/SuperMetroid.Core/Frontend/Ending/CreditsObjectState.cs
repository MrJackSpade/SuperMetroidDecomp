using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The single scrolling credits object at <c>$8B:9932-$8B:99FD</c>.
/// </summary>
/// <remarks>
/// The roll's rows are the staff lines' pre-composed pictures (#1467), laid out by <see cref="CreditsRollRows"/>. The
/// object keeps the native half-pixel cadence and takes one more row each time the roll scrolls eight pixels; the
/// screen shows the last <see cref="RowsHeld"/> rows taken, each at its place on the roll, so a row is taken one screen
/// below the view and leaves the plane only after it has scrolled out of view at the top.
/// </remarks>
internal sealed class CreditsObjectState
{
    /// <summary>The rows the screen holds: one screen of eight-pixel rows, which the newest rows replace as they are taken.</summary>
    internal const int RowsHeld = 32;
    /// <summary>The roll's plane in cells: a screen wide, and as tall as the sixteen-bit scroll reaches.</summary>
    private static readonly Extent16<TilemapCells> PlaneCells = new(32, PicturedPlane.MaximumCells);
    /// <summary>Native 16.16 credits scroll; only the vertical axis moves, half a pixel per accepted update, wrapping at sixteen bits.</summary>
    private FixedPosition<TilePlanePixels> scroll;
    /// <summary>Last whole scroll position that caused a row to be taken.</summary>
    private Position16<TilePlanePixels> previousCopiedScroll;
    /// <summary>The scroll advance, one eight-pixel row, after which the next row is taken.</summary>
    private static readonly Offset16<TilePlanePixels> RowAdvance = new(0, ScreenGeometry.BackgroundTileSizePixels);
    /// <summary>The roll's rows.</summary>
    private readonly int rowCount;
    /// <summary>Next row of the roll to take.</summary>
    internal int NextSourceRow { get; private set; }

    /// <summary>Starts the roll of <paramref name="rows"/> rows with none taken.</summary>
    public CreditsObjectState(int rows) => rowCount = Ensure.BetweenInclusive(0, CreditsRollRows.Pictures.Length, rows);

    /// <summary>Whether the credits object still advances and may take rows.</summary>
    public bool Enabled { get; private set; } = true;
    /// <summary>Set when scrolling reaches the end of the roll.</summary>
    public bool Finished { get; private set; }
    /// <summary>Whole-pixel credits scroll position the roll's plane is drawn at.</summary>
    public Position16<TilePlanePixels> ScrollPosition => scroll.Whole;

    /// <summary>Runs one <c>CreditsObject_Process</c> call: scrolls, and takes the next row or finishes on each row boundary.</summary>
    public void Step()
    {
        if (!Enabled)
            return;

        // Half a pixel per accepted frame, carrying into the whole word and wrapping at 32 bits.
        scroll = scroll.AdvanceY(EndingMotionDefinitions.CreditsScrollVelocity);

        // A new row is taken whenever the scroll advances eight whole pixels, i.e. every sixteen NTSC frames. The
        // wrapped signed distance is the cartridge's test.
        if ((ScrollPosition - (previousCopiedScroll + RowAdvance)).Y < 0)
            return;

        previousCopiedScroll = ScrollPosition;
        if (NextSourceRow >= rowCount)
        {
            Finished = true;
            Enabled = false;
        }
        else
            NextSourceRow++;
    }

    /// <summary>
    /// The roll's plane: each held row's line picture from <paramref name="rows"/>, one screen below the place the roll's
    /// row has on the plane, so a row taken as the scroll reaches it sits just below the view.
    /// </summary>
    internal PicturedPlane Plane(SpriteSheet rows)
    {
        Ensure.NotNull(rows);
        ImmutableArray<string?> pictures = CreditsRollRows.Pictures;
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        for (int row = Math.Max(0, NextSourceRow - RowsHeld); row < NextSourceRow; row++)
        {
            if (pictures[row] is not { } picture)
                continue;
            int top = (row + RowsHeld) * ScreenGeometry.BackgroundTileSizePixels;
            placed.Add(new PlacedPlanePicture(rows.Frame(picture), new Position16<TilePlanePixels>(0, top)));
        }
        return new PicturedPlane(PlaneCells, placed.ToImmutable());
    }
}
