using Godot;

namespace Engine.Ui;

/// <summary>
/// How dark the whole screen is beneath whatever shows play, kept by the session so that it outlasts whichever mode
/// set it: so many levels of darkness, and the pace of the fade that is stepping it. A mode that darkens or brightens
/// the screen steps this and leaves it where it stands; whoever fades it next goes on from there, at the pace it sets.
/// It is drawn over everything, and only while play is on screen or kept beneath the mode in control: a mode that has
/// let play go shows its own screen.
/// </summary>
public sealed class ScreenDarkness
{
    /// <summary>The layer its cover is drawn on, over every other.</summary>
    public const int CoverLayer = 100;

    private readonly ScreenFade _cover;

    /// <summary>How many updates the fade waits before each step.</summary>
    private int _waitBeforeEachStep;

    /// <summary>How many updates it still waits before its next step.</summary>
    private int _wait;

    /// <summary>Whether what is shown is other than <see cref="Dark"/>, until the next update opens.</summary>
    private bool _showsAnother;

    /// <summary>The screen's darkness, drawn under <paramref name="host"/>, in <paramref name="levels"/> levels from fully lit to black.</summary>
    public ScreenDarkness(Node host, int levels)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(levels);
        Levels = levels;
        _cover = new ScreenFade { Name = "ScreenDarkness", Layer = CoverLayer };
        host.AddChild(_cover);
        _cover.Cover(0, levels);
    }

    /// <summary>How many levels there are from fully lit to black.</summary>
    public int Levels { get; }

    /// <summary>How many levels dark the screen is: none when fully lit.</summary>
    public int Dark { get; private set; }

    /// <summary>Whether the screen is black.</summary>
    public bool IsBlack => Dark == Levels;

    /// <summary>Whether the screen is fully lit.</summary>
    public bool IsFullyLit => Dark == 0;

    /// <summary>Shows the screen <paramref name="dark"/> levels dark, the fade's pace left as it is.</summary>
    public void Show(int dark)
    {
        if (dark < 0 || dark > Levels)
            throw new ArgumentOutOfRangeException(nameof(dark), dark, $"The screen is from none to {Levels} levels dark.");
        Dark = dark;
        _showsAnother = false;
        _cover.Cover(dark, Levels);
    }

    /// <summary>
    /// Shows the screen <paramref name="dark"/> levels dark through this update and <paramref name="after"/> levels
    /// dark from the next, which is what it is from now on to whoever asks: for a fade whose last step is seen for
    /// one update and then leaves the screen as it found it.
    /// </summary>
    public void ShowThenLeave(int dark, int after)
    {
        Show(after);
        if (dark == after)
            return;
        if (dark < 0 || dark > Levels)
            throw new ArgumentOutOfRangeException(nameof(dark), dark, $"The screen is from none to {Levels} levels dark.");
        _showsAnother = true;
        _cover.Cover(dark, Levels);
    }

    /// <summary>Sets the fade's pace: how many updates it waits before each step, and before its first from now.</summary>
    public void SetPace(int waitBeforeEachStep, int firstWait)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(waitBeforeEachStep);
        ArgumentOutOfRangeException.ThrowIfNegative(firstWait);
        (_waitBeforeEachStep, _wait) = (waitBeforeEachStep, firstWait);
    }

    /// <summary>Shows the screen <paramref name="dark"/> levels dark and sets the fade's pace, the wait coming before its first step too.</summary>
    public void Restart(int dark, int waitBeforeEachStep)
    {
        Show(dark);
        SetPace(waitBeforeEachStep, waitBeforeEachStep);
    }

    /// <summary>Plays one update of darkening: once its wait is over the screen is a level darker. Returns whether it is black.</summary>
    public bool Darken()
    {
        if (!Waits())
            Show(Math.Min(Levels, Dark + 1));
        return IsBlack;
    }

    /// <summary>Plays one update of brightening: once its wait is over the screen is a level lighter. Returns whether it is fully lit.</summary>
    public bool Brighten()
    {
        if (!Waits())
            Show(Math.Max(0, Dark - 1));
        return IsFullyLit;
    }

    /// <summary>Opens an update: what was shown for one update only gives way to how dark the screen is.</summary>
    internal void OpenUpdate()
    {
        if (_showsAnother)
            Show(Dark);
    }

    /// <summary>Draws the darkness or not: the session draws it only while play is on screen or kept.</summary>
    internal void DrawWhile(bool drawn) => _cover.Visible = drawn;

    /// <summary>Counts the wait before a step, and says whether this update is spent waiting.</summary>
    private bool Waits()
    {
        if (_wait > 0)
        {
            _wait--;
            return true;
        }

        _wait = _waitBeforeEachStep;
        return false;
    }
}
