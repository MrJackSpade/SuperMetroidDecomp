using Engine.Content;
using Engine.World;
using Godot;

namespace Game;

/// <summary>
/// The light that puts a suit on Samus as she collects it (<see cref="CollectingSuitState"/>), which steps it once on
/// each of her updates. It is a child of hers that her scene must have: its stages say how long collecting a suit lasts
/// (until it has gone out) and on which update the suit is first seen on her (the one it shows it on), as the
/// cartridge's pickup lasts as long as its light.
/// </summary>
/// <remarks>
/// <para>After a wait, a narrow beam reaches in from the top and the bottom of the screen until the two halves meet;
/// it widens evenly, and then, with rounded sides, to the whole screen while its colour whitens; the suit is shown;
/// it closes from the top and the bottom while its colour settles to the suit's; and its middle line is left, which
/// narrows from both ends to nothing. Its edges are columns of the screen's row of 256: the line's left edge starts
/// at the last column and the first narrowing carries it round to the row's start, as the cartridge's does.</para>
/// <para>It adds its colour to everything of the room beneath it. Its rows are counted from the top of the screen, its
/// middle line half way down its own lines.</para>
/// <para>Its data, "suit_light" in hers: "wait", the updates before it appears; "lines", how many lines it spans; "appearing_lines",
/// how many it reaches in from each end each update; "narrow", the column the narrow beam stands at; "even_until", the column
/// the even widening's left edge goes past before it ends; "curve_from" ("left", "right"), where the rounded widening
/// starts; "curve_width", how far its rounded sides reach beside it at its middle; "thinning_from" ("left", "right"),
/// where the middle line starts; "thinning_step", how far each end comes in each update; "thinned_at", the column its
/// left edge reaches as it ends; "speed", "speed_gain", "speed_loss" and "least_speed", in 256ths of a pixel an
/// update; "white", and "whitening" and "settling", the levels a colour moves each update; and "suits", by the suit's
/// name, the colour it "starts" in and the one it "settles" to. Colours are levels of red, green and blue up to
/// "full_level".</para>
/// </remarks>
public partial class SamusSuitLight : Node2D
{
    private enum Stage
    {
        Waiting,
        Appearing,
        Widening,
        Curving,
        Revealing,
        Closing,
        Thinning,
        Done,
    }

    private enum Shape
    {
        None,
        Growing,
        Column,
        Curved,
        Closing,
        Line,
    }

    /// <summary>The columns of the screen's row, round which an edge is carried.</summary>
    private const int RowColumns = 256;

    private int _wait, _lines, _appearingLines, _narrow, _evenUntil, _curveLeft, _curveRight, _curveWidth;
    private int _thinningLeft, _thinningRight, _thinningStep, _thinnedAt;
    private int _startSpeed, _speedGain, _speedLoss, _leastSpeed;
    private int _whitening, _settling, _fullLevel;
    private int[] _white = [];
    private readonly Dictionary<string, (int[] Starts, int[] Settles)> _suits = new(StringComparer.Ordinal);

    private Stage _stage = Stage.Done;
    private int _waited, _covered, _left, _right, _speed;
    private int[] _colour = [];
    private int[] _settlesTo = [];
    private (Shape Shape, int Left, int Right, int Progress) _beam;
    private bool _steppedThisUpdate;
    private (Shape Shape, int Left, int Right, int Progress) _shown;
    private Color _shownColour;
    private Vector2 _screenCorner;

    /// <summary>Reads the light's values from her data.</summary>
    public void ReadValues(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        (_wait, _lines, _appearingLines, _narrow, _evenUntil) =
            (data.Int("wait"), data.Int("lines"), data.Int("appearing_lines"), data.Int("narrow"), data.Int("even_until"));
        (_curveLeft, _curveRight, _curveWidth) = (data.Get("curve_from").Int("left"), data.Get("curve_from").Int("right"), data.Int("curve_width"));
        (_thinningLeft, _thinningRight, _thinningStep, _thinnedAt) =
            (data.Get("thinning_from").Int("left"), data.Get("thinning_from").Int("right"), data.Int("thinning_step"), data.Int("thinned_at"));
        (_startSpeed, _speedGain, _speedLoss, _leastSpeed) = (data.Int("speed"), data.Int("speed_gain"), data.Int("speed_loss"), data.Int("least_speed"));
        (_whitening, _settling, _fullLevel) = (data.Int("whitening"), data.Int("settling"), data.Int("full_level"));
        _white = Levels(data.Get("white"));
        _suits.Clear();
        foreach ((string suit, DataNode colours) in data.Get("suits").Entries())
            _suits[suit] = (Levels(colours.Get("starts")), Levels(colours.Get("settles")));
        Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        ZIndex = RoomDepth.Overlay;
        ZAsRelative = false;
    }

    /// <summary>Begins the light for the suit named <paramref name="suit"/>, as she is put where it shines on her.</summary>
    /// <exception cref="InvalidOperationException">Her data gives the light no colours for that suit.</exception>
    public void Begin(string suit)
    {
        if (!_suits.TryGetValue(suit, out (int[] Starts, int[] Settles) colours))
            throw new InvalidOperationException($"Samus's data gives the suit's light no colours for \"{suit}\".");
        (_stage, _waited, _covered, _left, _right, _speed) = (Stage.Waiting, 0, 0, 0, 0, _startSpeed);
        (_colour, _settlesTo) = ([.. colours.Starts], colours.Settles);
        _beam = (Shape.None, 0, 0, 0);
    }

    /// <summary>Whether the light has gone out, which ends the collecting of the suit.</summary>
    public bool IsDone => _stage == Stage.Done;

    /// <summary>Moves the light on by one update.</summary>
    /// <returns>Whether this update is the one on which the light shows the suit on her.</returns>
    public bool Step()
    {
        _steppedThisUpdate = true;
        bool reveals = _stage == Stage.Revealing;
        int lines = _lines / 2;
        switch (_stage)
        {
            case Stage.Waiting:
                if (++_waited >= _wait)
                    _stage = Stage.Appearing;
                break;

            case Stage.Appearing:
                _covered += _appearingLines;
                _beam = (Shape.Growing, _narrow, _narrow, _covered);
                if (_covered >= lines)
                    (_stage, _left, _right) = (Stage.Widening, _narrow, _narrow);
                break;

            case Stage.Widening:
            {
                int left = _left - Whole(_speed);
                (_left, _right) = (InRow(left), InRow(_right + Whole(_speed)));
                _beam = (Shape.Column, _left, _right, 0);
                if (left < _evenUntil)
                    (_stage, _left, _right) = (Stage.Curving, _curveLeft, _curveRight);
                break;
            }

            case Stage.Curving:
            {
                for (int channel = 0; channel < _colour.Length; channel++)
                    _colour[channel] = Math.Min(_colour[channel] + _whitening, _white[channel]);
                int left = _left - Whole(_speed), right = RowColumns - 1;
                if (left <= 0)
                    left = 0;
                else
                    right = Math.Min(_right + Whole(_speed), RowColumns - 1);
                (_left, _right) = (InRow(left), InRow(right));
                _beam = (Shape.Curved, _left, _right, 0);
                _speed += _speedGain;
                if (left == 0 && right == RowColumns - 1)
                    (_stage, _speed, _covered) = (Stage.Revealing, _speed >> 1, 0);
                break;
            }

            case Stage.Revealing:
                // The suit is shown; the light stands as it is.
                _stage = Stage.Closing;
                break;

            case Stage.Closing:
                for (int channel = 0; channel < _colour.Length; channel++)
                {
                    if (_colour[channel] != _settlesTo[channel])
                        _colour[channel] -= _settling;
                }

                _covered += Whole(_speed);
                _beam = (Shape.Closing, _beam.Left, _beam.Right, _covered);
                _speed = Math.Max(_speed - _speedLoss, _leastSpeed);
                if (_covered >= lines)
                    (_stage, _left, _right) = (Stage.Thinning, _thinningLeft, _thinningRight);
                break;

            case Stage.Thinning:
                // An edge carried past either end of the screen's row comes round from the other, as the cartridge's
                // edges, a byte each, do.
                (_left, _right) = ((_left + _thinningStep) % RowColumns, (_right - _thinningStep + RowColumns) % RowColumns);
                _beam = (Shape.Line, _left, _right, 0);
                if (_left >= _thinnedAt)
                    _stage = Stage.Done;
                break;

            case Stage.Done:
                _beam = (Shape.None, 0, 0, 0);
                break;

            default:
                throw new InvalidOperationException($"The suit's light has no stage {_stage}.");
        }

        return reveals;
    }

    /// <summary>
    /// Shows the light as this update left it, at the end of her update: nothing on an update it was not stepped, as
    /// once she is let go.
    /// </summary>
    public void Show(Samus samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        _shown = _steppedThisUpdate ? _beam : (Shape.None, 0, 0, 0);
        _steppedThisUpdate = false;
        _shownColour = _colour.Length == 3
            ? new Color(_colour[0] / (float)_fullLevel, _colour[1] / (float)_fullLevel, _colour[2] / (float)_fullLevel)
            : Colors.Black;
        Camera view = samus.Room.Session.Camera;
        _screenCorner = new Vector2(view.Left - samus.Position.X.Whole, view.Top - samus.Position.Y.Whole);
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        (Shape shape, int left, int right, int progress) = _shown;
        if (shape == Shape.None || left > right)
            return;
        int lines = _lines, middle = _lines / 2;
        switch (shape)
        {
            case Shape.Growing:
                int grown = Math.Min(progress, lines);
                if (grown <= 0)
                    return;
                Row(left, right, 0, grown);
                Row(left, right, lines - grown, grown);
                break;

            case Shape.Column:
                Row(left, right, 0, lines);
                break;

            case Shape.Curved:
                CurvedRows(left, right, 0, lines + 1);
                break;

            case Shape.Closing:
            {
                // Wiped from the top, at least its first line, and one line fewer from the bottom.
                int first = Math.Clamp(progress, 1, lines), bottom = Math.Clamp(progress - 1, 1, lines);
                CurvedRows(left, right, first, Math.Max(lines - bottom, first));
                break;
            }

            case Shape.Line:
                Row(left, right, middle, 1);
                break;

            default:
                throw new InvalidOperationException($"The suit's light has no shape {shape}.");
        }
    }

    /// <summary>
    /// Lights, on each line from <paramref name="first"/> up to <paramref name="end"/>, the column widened by the
    /// light's rounded sides: an oval about its middle line reaching <c>curve_width</c> beside the column and half its
    /// lines above and below, a pixel exactly on the oval's edge within it.
    /// </summary>
    private void CurvedRows(int left, int right, int first, int end)
    {
        long across = _curveWidth, down = _lines / 2;
        for (int line = first; line < end; line++)
        {
            long from = line - down;
            long room = across * across * (down * down - from * from);
            if (room < 0)
                continue;
            int reach = 0;
            while ((long)(reach + 1) * (reach + 1) * down * down <= room)
                reach++;
            Row(left - reach, right + reach, line, 1);
        }
    }

    /// <summary>Lights the columns <paramref name="left"/> to <paramref name="right"/> of <paramref name="count"/> lines from <paramref name="line"/>.</summary>
    private void Row(int left, int right, int line, int count) =>
        DrawRect(new Rect2(_screenCorner + new Vector2(left, line), new Vector2(right - left + 1, count)), _shownColour);

    private static int Whole(int speed) => speed >> 8;

    private static int InRow(int column) => ((column % RowColumns) + RowColumns) % RowColumns;

    private int[] Levels(DataNode colour)
    {
        int[] levels = [.. colour.Items().Select(level => level.AsInt())];
        return levels.Length == 3 ? levels : throw colour.Problem("is not three levels, of red, green and blue");
    }
}
