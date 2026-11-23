namespace Game.Enemies;

/// <summary>
/// How a corpse crumbles: its picture comes apart a row at a time from the bottom up, each row sliding down through
/// the ones below it and vanishing as it reaches the ground, each a little after the one beneath.
/// </summary>
public sealed class CorpseRot
{
    /// <summary>How many updates after the row below it each row starts to fall.</summary>
    private const int DelayPerRow = 2;

    /// <summary>How many rows a falling row drops each update.</summary>
    private const int FallPerUpdate = 2;

    /// <summary>For how many updates before it falls a row already smears down onto the one below.</summary>
    private const int LeadIn = 4;

    private readonly Fall[] _falls;
    private readonly int?[] _rows;

    /// <summary>A corpse of <paramref name="rowCount"/> rows, whole.</summary>
    public CorpseRot(int rowCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowCount, 2);
        _falls = new Fall[rowCount];
        _rows = new int?[rowCount];
        for (int row = 0; row < rowCount; row++)
        {
            _falls[row] = new Fall { Row = rowCount - 1 - row, Delay = row * DelayPerRow };
            _rows[row] = row;
        }
    }

    /// <summary>Which row of the picture shows at each row down from its top; null where nothing is left.</summary>
    public IReadOnlyList<int?> Rows => _rows;

    /// <summary>
    /// Crumbles the corpse for one update, calling <paramref name="rowFallen"/> for each row that reaches the ground.
    /// Returns whether any of it is left.
    /// </summary>
    public bool Step(Action rowFallen)
    {
        ArgumentNullException.ThrowIfNull(rowFallen);
        int last = _falls.Length - 1;
        for (int index = 0; index < _falls.Length; index++)
        {
            Fall fall = _falls[index];
            if (fall.HasFallen)
                continue;
            if (fall.Delay > 0)
            {
                fall.Delay--;
                if (fall.Delay < LeadIn)
                    Move(fall.Row, leaving: index >= last - 1);
                continue;
            }

            Move(fall.Row, leaving: true);
            int next = fall.Row + FallPerUpdate;
            if (next < last)
            {
                fall.Row = next;
                continue;
            }

            rowFallen();
            if (index == last)
                return false;
            fall.HasFallen = true;
        }

        return true;
    }

    /// <summary>Slides what shows at a row down onto the row below it, leaving the row empty or still showing.</summary>
    private void Move(int row, bool leaving)
    {
        if (row + 1 < _rows.Length)
            _rows[row + 1] = _rows[row];
        if (leaving)
            _rows[row] = null;
    }

    private sealed class Fall
    {
        public int Row { get; set; }

        public int Delay { get; set; }

        public bool HasFallen { get; set; }
    }
}
