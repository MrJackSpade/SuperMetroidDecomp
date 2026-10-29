namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss's body layer as the boss draws it (#1466): the parts placed on it, each showing the cells no later part covers,
/// so every cell shows the last part drawn over it. It starts with every cell showing the boss's blank. A boss that
/// assembles a picture before showing it (Crocomire's melt, Mother Brain's staged body) keeps a second canvas and copies
/// cells from it. The canvas names its parts; the installed sheet they are drawn from is bound when it is published.
/// </summary>
internal sealed class BossBodyCanvas
{
    /// <summary>The parts showing, each over cells no other covers.</summary>
    private readonly List<PlacedBossBodyPart> placements = [];

    /// <summary>The name of the part the canvas clears with.</summary>
    private readonly string blank;

    /// <summary>The last published layer, until a part is placed or another sheet is bound.</summary>
    [NonSerialized]
    private BossBodyLayer? published;

    /// <summary>The sheet <see cref="published"/> was drawn from.</summary>
    [NonSerialized]
    private BossBodySheet? publishedFrom;

    /// <summary>Creates a canvas of <paramref name="rows"/> rows of <paramref name="sheet"/>'s boss, every cell showing its blank.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rows"/> is not positive.</exception>
    public BossBodyCanvas(BossBodySheet sheet, int rows)
    {
        Ensure.NotNull(sheet);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        Owner = sheet.Owner;
        Rows = rows;
        blank = sheet.Blank.Name;
        placements.Add(new PlacedBossBodyPart(blank, BossBodyFootprint.OfRows(0, rows)));
    }

    /// <summary>The boss whose sheet the canvas is drawn from.</summary>
    public BossBodyOwner Owner { get; }

    /// <summary>The canvas's rows.</summary>
    public int Rows { get; }

    /// <summary>Draws every part of <paramref name="frame"/>, each over the cells it covers.</summary>
    /// <exception cref="InvalidDataException">A part leaves the canvas.</exception>
    public void Draw(BossBodyFrame frame)
    {
        foreach (BossBodyPart part in Ensure.NotNull(frame).Parts)
            Place(part.Name, part.Footprint);
    }

    /// <summary>Clears the <paramref name="count"/> cells from <paramref name="firstCell"/> to the blank.</summary>
    /// <exception cref="InvalidDataException">The cells leave the canvas.</exception>
    public void Clear(int firstCell, int count) => Place(blank, BossBodyFootprint.OfCells(firstCell, count));

    /// <summary>Shows here what <paramref name="source"/> shows on the <paramref name="count"/> cells from <paramref name="firstCell"/>.</summary>
    /// <exception cref="InvalidDataException">The canvases belong to different bosses, or the cells leave either canvas.</exception>
    public void Copy(BossBodyCanvas source, int firstCell, int count)
    {
        Ensure.NotNull(source);
        if (source.Owner != Owner)
            throw new InvalidDataException($"{Owner}'s body cannot show {source.Owner}'s.");
        BossBodyFootprint cells = BossBodyFootprint.OfCells(firstCell, count);
        if (cells.End > source.Rows)
            throw new InvalidDataException($"A copy of {count} cells from cell {firstCell} leaves the {source.Rows}-row canvas it copies.");
        foreach (PlacedBossBodyPart placement in source.placements.ToArray())
            Place(placement.Part, placement.Shows.Within(cells));
    }

    /// <summary>Places the part named <paramref name="part"/> over <paramref name="cells"/>, which no earlier part shows from then on.</summary>
    /// <exception cref="InvalidDataException">The cells leave the canvas.</exception>
    private void Place(string part, BossBodyFootprint cells)
    {
        if (cells.IsEmpty)
            return;
        if (cells.End > Rows)
            throw new InvalidDataException($"{Owner}'s body part {part} covers rows to {cells.End}, past the {Rows}-row canvas.");
        for (int index = placements.Count - 1; index >= 0; index--)
        {
            BossBodyFootprint left = placements[index].Shows.Without(cells);
            if (left.IsEmpty)
                placements.RemoveAt(index);
            else
                placements[index] = placements[index] with { Shows = left };
        }
        placements.Add(new PlacedBossBodyPart(part, cells));
        published = null;
    }

    /// <summary>The canvas as it stands, drawn from its boss's sheet in <paramref name="sheets"/>, shared until the next part is placed.</summary>
    /// <exception cref="InvalidDataException">The boss's sheet has no part the canvas names.</exception>
    public BossBodyLayer Publish(BossBodySheets sheets) => Publish(Ensure.NotNull(sheets).Of(Owner));

    /// <summary>The canvas as it stands, drawn from <paramref name="sheet"/>, shared until the next part is placed.</summary>
    /// <exception cref="InvalidDataException">The sheet is another boss's, or has no part the canvas names.</exception>
    public BossBodyLayer Publish(BossBodySheet sheet)
    {
        Ensure.NotNull(sheet);
        if (sheet.Owner != Owner)
            throw new InvalidDataException($"{Owner}'s body cannot be drawn from {sheet.Owner}'s sheet.");
        if (published is null || !ReferenceEquals(publishedFrom, sheet))
        {
            published = new BossBodyLayer(Rows, [.. placements.Select(placement => new BossBodyPlacement(sheet.Part(placement.Part), placement.Shows))]);
            publishedFrom = sheet;
        }
        return published;
    }
}
