using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The scrolling credits roll (#1555): its rows of staff-line pictures, laid out by <see cref="CreditsRollRows"/>, and how
/// far the roll has scrolled up the screen.
/// </summary>
/// <remarks>
/// The roll scrolls half a pixel an update and reaches one more row each time it scrolls eight pixels; it finishes on
/// the boundary after its last row. The screen shows every row at its place on the roll, the roll's first row starting
/// <see cref="StartRows"/> rows below the view's top, moved up by the distance scrolled; nothing wraps, and no row is
/// held or replaced.
/// </remarks>
internal sealed class CreditsObjectState
{
    /// <summary>Rows below the view's top the roll's first row starts at, one screen's worth, so each row comes up from below the view.</summary>
    internal const int StartRows = 32;
    /// <summary>The distance the roll has scrolled up, in pixels and fractions of a pixel.</summary>
    private SignedSixteenSixteen scrolled;
    /// <summary>The whole pixels scrolled when the roll last reached a row.</summary>
    private int scrolledAtLastRow;
    /// <summary>The pixels the roll scrolls between rows.</summary>
    private const int RowPixels = ScreenGeometry.BackgroundTileSizePixels;
    /// <summary>The roll's rows.</summary>
    private readonly int rowCount;
    /// <summary>The number of rows the roll has reached.</summary>
    internal int NextSourceRow { get; private set; }

    /// <summary>Starts the roll of <paramref name="rows"/> rows with none reached.</summary>
    public CreditsObjectState(int rows) => rowCount = Ensure.BetweenInclusive(0, CreditsRollRows.Pictures.Length, rows);

    /// <summary>Whether the roll still scrolls and may reach rows.</summary>
    public bool Enabled { get; private set; } = true;
    /// <summary>Set when scrolling passes the end of the roll.</summary>
    public bool Finished { get; private set; }
    /// <summary>The whole pixels the roll has scrolled up.</summary>
    public int ScrolledPixels => scrolled.WholePixels;

    /// <summary>Runs one credits step: scrolls, and reaches the next row or finishes on each row boundary.</summary>
    public void Step()
    {
        if (!Enabled)
            return;

        scrolled += EndingMotionDefinitions.CreditsScrollVelocity;

        // A new row is reached whenever the roll scrolls eight whole pixels, i.e. every sixteen updates.
        if (ScrolledPixels - (scrolledAtLastRow + RowPixels) < 0)
            return;

        scrolledAtLastRow = ScrolledPixels;
        if (NextSourceRow >= rowCount)
        {
            Finished = true;
            Enabled = false;
        }
        else
            NextSourceRow++;
    }

    /// <summary>
    /// The roll: each row's line picture from <paramref name="rows"/> at its place, <see cref="StartRows"/> rows below the
    /// roll's top, in <paramref name="shading"/>, the ending's colours (#1512); the screen shows it moved up by
    /// <see cref="ScrolledPixels"/>.
    /// </summary>
    internal PicturedPlane Plane(SpriteSheet rows, PictureShading shading)
    {
        Ensure.NotNull(rows);
        Ensure.NotNull(shading);
        ImmutableArray<string?> pictures = CreditsRollRows.Pictures;
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        for (int row = 0; row < rowCount; row++)
        {
            if (pictures[row] is not { } picture)
                continue;
            placed.Add(new PlacedPlanePicture(rows.Frame(picture), Offset16<PlanePixels>.Of(0, (row + StartRows) * RowPixels), shading));
        }
        return new PicturedPlane(placed.ToImmutable());
    }
}
