namespace Engine.Actors.Components;

/// <summary>
/// A picture crumbling away: it comes apart a row at a time from the bottom up, each row sliding down through the
/// ones below it and vanishing as it reaches the ground, each a little after the one beneath. It may wait a while
/// before it starts. It only works out which rows show where; an <see cref="ActorVisual"/> shows them.
/// </summary>
public sealed class RowCrumble
{
    /// <summary>How many updates after the row below it each row starts to fall.</summary>
    private const int DelayPerRow = 2;

    /// <summary>How many rows a falling row drops each update.</summary>
    private const int FallPerUpdate = 2;

    /// <summary>For how many updates before it falls a row already smears down onto the one below.</summary>
    private const int LeadIn = 4;

    private readonly Fall[] _falls;
    private readonly int?[] _rows;
    private int _wait;

    /// <summary>A picture of <paramref name="rowCount"/> rows, whole, that starts to crumble after <paramref name="waitUpdates"/>.</summary>
    public RowCrumble(int rowCount, int waitUpdates = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowCount, 2);
        ArgumentOutOfRangeException.ThrowIfNegative(waitUpdates);
        _wait = waitUpdates;
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

    /// <summary>Whether it has started to come apart.</summary>
    public bool HasStarted => _wait == 0;

    /// <summary>Whether nothing of it is left.</summary>
    public bool IsOver { get; private set; }

    /// <summary>Ends any wait: it comes apart from the next update.</summary>
    public void StartNow() => _wait = 0;

    /// <summary>
    /// Plays one update: the wait runs down, or the picture crumbles, calling <paramref name="rowFallen"/> for each
    /// row that reaches the ground. Returns whether it crumbled this update.
    /// </summary>
    public bool Step(Action rowFallen)
    {
        ArgumentNullException.ThrowIfNull(rowFallen);
        if (IsOver)
            return false;
        if (_wait > 0 && --_wait > 0)
            return false;
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
            {
                IsOver = true;
                return true;
            }

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
