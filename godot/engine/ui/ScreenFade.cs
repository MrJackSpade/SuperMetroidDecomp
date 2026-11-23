using Godot;

namespace Engine.Ui;

/// <summary>
/// Fading the whole screen to a colour or back over a set number of updates, and reporting when it is done. Room
/// transitions, death, pausing and cinematics use it. It does not know why it was asked.
/// </summary>
public partial class ScreenFade : CanvasLayer
{
    /// <summary>The colour the screen fades to.</summary>
    [Export]
    public Color Colour { get; set; } = Colors.Black;

    private ColorRect? _cover;
    private int _updates = 1;
    private int _elapsed = 1;
    private bool _towardCovered;

    /// <summary>How covered the screen is, out of <see cref="Steps"/>.</summary>
    public int Covered => _towardCovered ? _elapsed : _updates - _elapsed;

    /// <summary>How many steps the fade in progress, or the last one, has.</summary>
    public int Steps => _updates;

    /// <summary>Whether the fade asked for has finished.</summary>
    public bool IsDone => _elapsed >= _updates;

    /// <summary>Whether the screen is fully covered.</summary>
    public bool IsCovered => IsDone && _towardCovered;

    /// <summary>Starts covering the screen over <paramref name="updates"/> updates.</summary>
    public void FadeOut(int updates) => Begin(updates, towardCovered: true);

    /// <summary>Starts uncovering the screen over <paramref name="updates"/> updates.</summary>
    public void FadeIn(int updates) => Begin(updates, towardCovered: false);

    /// <summary>Plays one update of the fade.</summary>
    public void Step()
    {
        if (!IsDone)
            _elapsed++;
        Show();
    }

    /// <summary>Starts a fade.</summary>
    private void Begin(int updates, bool towardCovered)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(updates);
        _updates = updates;
        _elapsed = 0;
        _towardCovered = towardCovered;
        Show();
    }

    /// <summary>Shows the cover at its present strength.</summary>
    private new void Show()
    {
        if (_cover is null)
        {
            _cover = new ColorRect { Name = "Cover", MouseFilter = Control.MouseFilterEnum.Ignore };
            _cover.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_cover);
        }

        _cover.Color = new Color(Colour.R, Colour.G, Colour.B, (float)Covered / _updates);
    }
}
