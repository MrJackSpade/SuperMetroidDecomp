using Engine.Input;
using Godot;

namespace Engine.Ui;

/// <summary>
/// A list of lines to choose from, one of them marked. Up and down move the mark, which goes round at the ends;
/// confirm and cancel are reported to whoever steps it. It holds no words of its own and acts on nothing.
/// </summary>
public partial class ChoiceList : VBoxContainer
{
    /// <summary>What a step of the list came to.</summary>
    public enum Outcome
    {
        /// <summary>Nothing happened.</summary>
        Nothing,

        /// <summary>The mark moved.</summary>
        Moved,

        /// <summary>The marked line was confirmed.</summary>
        Confirmed,

        /// <summary>The list was cancelled.</summary>
        Cancelled,
    }

    private readonly List<PixelText> _labels = [];
    private IReadOnlyList<string> _lines = [];

    /// <summary>The font its lines are drawn in.</summary>
    public PixelFont? Font { get; set; }

    /// <summary>What marks the chosen line, written before it.</summary>
    [Export]
    public string Mark { get; set; } = "> ";

    /// <summary>The tint of the lines that are not chosen; white leaves them as the chosen one is.</summary>
    [Export]
    public Color UnchosenTint { get; set; } = Colors.White;

    /// <summary>Which line is marked, counted from nought.</summary>
    public int Chosen { get; private set; }

    /// <summary>How many lines there are.</summary>
    public int Count => _lines.Count;

    /// <summary>Shows these lines, with the mark on the line numbered <paramref name="chosen"/>.</summary>
    public void Present(IReadOnlyList<string> lines, int chosen = 0)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
            throw new ArgumentException("A list to choose from needs at least one line.", nameof(lines));
        ArgumentOutOfRangeException.ThrowIfNegative(chosen);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(chosen, lines.Count);
        _lines = lines;
        Chosen = chosen;
        foreach (PixelText label in _labels)
            label.QueueFree();
        _labels.Clear();
        foreach (string _ in lines)
        {
            var label = new PixelText { Font = Font };
            _labels.Add(label);
            AddChild(label);
        }

        ShowLines();
    }

    /// <summary>Plays one update of input on the list and says what it came to.</summary>
    public Outcome Step(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (_lines.Count == 0)
            throw new InvalidOperationException("The list has been given no lines to choose from.");
        if (input.WasPressed(GameAction.Confirm))
            return Outcome.Confirmed;
        if (input.WasPressed(GameAction.Cancel))
            return Outcome.Cancelled;
        int move = input.WasPressed(GameAction.Down) ? 1 : input.WasPressed(GameAction.Up) ? -1 : 0;
        if (move == 0)
            return Outcome.Nothing;
        Chosen = (Chosen + move + _lines.Count) % _lines.Count;
        ShowLines();
        return Outcome.Moved;
    }

    /// <summary>Moves the mark so many lines down, or up when negative, round the list's ends.</summary>
    public void Move(int by)
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("The list has been given no lines to choose from.");
        Chosen = (((Chosen + by) % _lines.Count) + _lines.Count) % _lines.Count;
        ShowLines();
    }

    private void ShowLines()
    {
        string blank = new(' ', Mark.Length);
        for (int line = 0; line < _lines.Count; line++)
        {
            _labels[line].Text = (line == Chosen ? Mark : blank) + _lines[line];
            _labels[line].Modulate = line == Chosen ? Colors.White : UnchosenTint;
        }
    }
}
