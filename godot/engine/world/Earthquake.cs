using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// The room shaking. It lasts a set number of updates each time it starts, and while it lasts the view is offset back
/// and forth. It changes nothing in play.
/// </summary>
public partial class Earthquake : RoomEffect
{
    /// <summary>How far the view is offset, in pixels.</summary>
    [Export]
    public int Strength { get; set; } = 1;

    /// <summary>How many updates a shake lasts.</summary>
    [Export]
    public int Updates { get; set; } = 60;

    /// <summary>The updates left in the shake.</summary>
    private Countdown _left = Countdown.Over;

    /// <summary>How far the view is offset this update.</summary>
    public Vector2I ViewOffset { get; private set; }

    /// <summary>Whether a shake has been started and has yet to take its first update: it has its whole length left.</summary>
    public bool HasJustBegun => IsActive && _left.Remaining == Updates;

    /// <summary>The way the view is thrown about.</summary>
    [Export]
    public ShakeWay Way { get; set; } = ShakeWay.Across;

    /// <summary>Starts a shake of the given strength, length and way.</summary>
    public void Shake(int strength, int updates, ShakeWay way = ShakeWay.Across)
    {
        Strength = strength;
        Updates = updates;
        Way = way;
        _left = Countdown.Of(updates);
        SetActive(true);
    }

    /// <inheritdoc/>
    protected override void OnSetup(Room room) => _left = Countdown.Of(StartsActive ? Updates : 0);

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        if (_left.IsOver)
        {
            ViewOffset = Vector2I.Zero;
            SetActive(false);
            return;
        }

        int by = _left.Remaining % 2 == 0 ? Strength : -Strength;
        ViewOffset = new Vector2I(Way == ShakeWay.Down ? 0 : by, Way == ShakeWay.Across ? 0 : by);
        _left = _left.Stepped();
    }

    /// <inheritdoc/>
    public override void Triggered(string what)
    {
        if (what == "start")
            Shake(Strength, Updates, Way);
        else
            base.Triggered(what);
    }
}
