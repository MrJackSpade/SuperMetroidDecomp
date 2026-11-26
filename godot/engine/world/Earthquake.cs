using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// The room shaking. It lasts a set number of updates each time it starts, and while it lasts the room is shown
/// thrown to one side and the other: two updates to the far side, then two to its own, over and over, by the updates
/// it has left, so that one started with a length that is two more than a multiple of four shows far, near, near,
/// far, far. It changes nothing in play but what asks whether the room shakes.
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

    /// <summary>
    /// Whether the room is shaking: a shake has updates left. The update a shake takes its last, the room is still
    /// shown thrown, and is shaking no longer; the effect stays active one update more only to show it still again.
    /// </summary>
    public bool IsShaking => IsActive && _left.IsRunning;

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

        // The far side is the room shown the other way from its shake's own.
        const int sides = 4;
        int by = _left.Remaining % sides >= sides / 2 ? Strength : -Strength;
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
