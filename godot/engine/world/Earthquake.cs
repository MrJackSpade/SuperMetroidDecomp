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
    /// <summary>A shake is stepped once all that may start it has run: the room is thrown on the update it is asked to shake.</summary>
    protected internal override bool StepsAsUpdateEnds => true;

    /// <summary>How far the view is offset, in pixels, by the shake last asked for.</summary>
    public int Strength { get; private set; }

    /// <summary>How many updates the shake last started was to last; nought for one only kept going.</summary>
    public int Updates { get; private set; }

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

    /// <summary>The way the view is thrown about by the shake last asked for.</summary>
    public ShakeWay Way { get; private set; }

    /// <summary>Starts a shake of the given strength, length and way.</summary>
    public void Shake(int strength, int updates, ShakeWay way = ShakeWay.Across)
    {
        Strength = strength;
        Updates = updates;
        Way = way;
        _left = Countdown.Of(updates);
        SetActive(true);
    }

    /// <summary>
    /// Keeps the room shaking with a shake of the given strength and way for at least <paramref name="atLeast"/>
    /// updates more, without starting its length over: something that shakes the room for as long as it goes on asks
    /// this on each update. The length left runs in rounds of twice <paramref name="atLeast"/>; one in the first
    /// half of its round is moved on by <paramref name="atLeast"/>, and one in the second half is left. Asked every
    /// update it never runs out, and once the asking stops it runs out within a round. It is no beginning of a shake
    /// (<see cref="HasJustBegun"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="atLeast"/> is not a power of two.</exception>
    public void Sustain(int strength, int atLeast, ShakeWay way = ShakeWay.Across)
    {
        if (atLeast <= 0 || (atLeast & (atLeast - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(atLeast), atLeast, "A shake is kept going by a power of two of updates.");
        (Strength, Way, Updates) = (strength, way, 0);
        _left = Countdown.Of(_left.Remaining | atLeast);
        SetActive(true);
    }

    /// <summary>A room is set up still: a shake begins when it is asked for.</summary>
    protected override void OnSetup(Room room) => _left = Countdown.Over;

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
}
