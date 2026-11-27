using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// The room shaking. A shake has a look (how far it throws what it reaches, which way, and what of the room it
/// reaches) and a run (a number of updates, or no length at all), and either may be changed apart from the other:
/// what starts a shake gives both (<see cref="Shake"/>), and something may change the look of whatever runs
/// (<see cref="Change"/>), run the look the room has for a length (<see cref="RunFor"/>) or with none
/// (<see cref="RunOn"/>), or take the length from a shake that has one (<see cref="KeepOn"/>). While it runs the room
/// is shown thrown to one side and the other: two updates to the far side, then two to its own, over and over. A
/// shake with a length takes its side from the updates it has left, so that one started with a length that is two
/// more than a multiple of four shows far, near, near, far, far; one with no length begins from the far side, or
/// goes on from where its length had it. It changes nothing in play but what asks whether the room shakes.
/// </summary>
public partial class Earthquake : RoomEffect
{
    /// <summary>How many updates the two sides take in turn, two each.</summary>
    private const int Sides = 4;

    /// <summary>A shake is stepped once all that may start it has run: the room is thrown on the update it is asked to shake.</summary>
    protected internal override bool StepsAsUpdateEnds => true;

    /// <summary>How far the view is offset, in pixels, by the shake last asked for.</summary>
    public int Strength { get; private set; }

    /// <summary>How many updates the shake last started was to last; nought for one only kept going, or with no length.</summary>
    public int Updates { get; private set; }

    /// <summary>
    /// The updates left in the shake; for one with no length, where it is in its sides, counted the same way and
    /// begun again each time it runs out.
    /// </summary>
    private Countdown _left = Countdown.Over;

    /// <summary>Whether the shake has no length: it runs until it is given one.</summary>
    private bool _endless;

    /// <summary>How far what the shake reaches is thrown this update.</summary>
    public Vector2I Thrown { get; private set; }

    /// <summary>What of the room the shake last asked for throws about.</summary>
    public ShakeReach Reach { get; private set; }

    /// <summary>Whether the shake throws the room's tiles.</summary>
    public bool ThrowsTiles => Reach != ShakeReach.BackdropsAndThings;

    /// <summary>Whether the shake throws the room's backdrops.</summary>
    public bool ThrowsBackdrops => Reach != ShakeReach.Tiles;

    /// <summary>Whether the shake throws what the room's enemies have launched and left.</summary>
    public bool ThrowsThings => Reach is ShakeReach.Everything or ShakeReach.BackdropsAndThings;

    /// <summary>
    /// Whether the room is shaking: a shake has updates left, or has no length. The update a shake takes its last,
    /// the room is still shown thrown, and is shaking no longer; the effect stays active one update more only to show
    /// it still again.
    /// </summary>
    public bool IsShaking => IsActive && _left.IsRunning;

    /// <summary>Whether a shake has been started and has yet to take its first update: it has its whole length left.</summary>
    public bool HasJustBegun => IsActive && !_endless && _left.Remaining == Updates;

    /// <summary>The way things are thrown about by the shake last asked for.</summary>
    public ShakeWay Way { get; private set; }

    /// <summary>Starts a shake of the given strength, length, way and reach.</summary>
    public void Shake(int strength, int updates, ShakeWay way, ShakeReach reach)
    {
        Change(strength, way, reach);
        RunFor(updates);
    }

    /// <summary>
    /// Changes the shake's look, leaving its run: one that is running goes on, as long as it was going to, with the
    /// new strength, way and reach, and a still room keeps the look for whatever next runs its shake.
    /// </summary>
    public void Change(int strength, ShakeWay way, ShakeReach reach) => (Strength, Way, Reach) = (strength, way, reach);

    /// <summary>
    /// Runs the shake for <paramref name="updates"/> updates with the look it has, in place of whatever run it had:
    /// a shake with no length is given this one. No updates stills the room, its look kept.
    /// </summary>
    public void RunFor(int updates)
    {
        Updates = updates;
        _endless = false;
        _left = Countdown.Of(updates);
        SetActive(true);
    }

    /// <summary>
    /// Runs the shake with the look it has and no length, from the far side: it goes on until something gives it a
    /// length (<see cref="RunFor"/>, <see cref="Shake"/>).
    /// </summary>
    public void RunOn()
    {
        Updates = 0;
        _endless = true;
        _left = Countdown.Of(Sides - 1);
        SetActive(true);
    }

    /// <summary>
    /// Takes its length from the shake: with whatever it had left, none included, it goes on from the side it was at
    /// with no length, until something gives it one. One with no length already goes on as it was. It is no beginning
    /// of a shake (<see cref="HasJustBegun"/>).
    /// </summary>
    public void KeepOn()
    {
        if (_endless)
            return;
        Updates = 0;
        _endless = true;
        if (_left.IsOver)
            _left = Countdown.Of(Sides);
        SetActive(true);
    }

    /// <summary>
    /// Keeps the room shaking with a shake of the given strength, way and reach for at least <paramref name="atLeast"/>
    /// updates more, without starting its length over: something that shakes the room for as long as it goes on asks
    /// this on each update. The length left runs in rounds of twice <paramref name="atLeast"/>; one in the first
    /// half of its round is moved on by <paramref name="atLeast"/>, and one in the second half is left. Asked every
    /// update it never runs out, and once the asking stops it runs out within a round. A shake with no length only
    /// takes the look and goes on. It is no beginning of a shake (<see cref="HasJustBegun"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="atLeast"/> is not a power of two.</exception>
    public void Sustain(int strength, int atLeast, ShakeWay way, ShakeReach reach)
    {
        if (atLeast <= 0 || (atLeast & (atLeast - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(atLeast), atLeast, "A shake is kept going by a power of two of updates.");
        Change(strength, way, reach);
        if (!_endless)
        {
            Updates = 0;
            _left = Countdown.Of(_left.Remaining | atLeast);
        }

        SetActive(true);
    }

    /// <summary>A room is set up still: a shake begins when it is asked for.</summary>
    protected override void OnSetup(Room room)
    {
        _left = Countdown.Over;
        _endless = false;
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        if (_left.IsOver)
        {
            Thrown = Vector2I.Zero;
            SetActive(false);
            return;
        }

        // The far side is the room shown the other way from its shake's own.
        int by = _left.Remaining % Sides >= Sides / 2 ? Strength : -Strength;
        Thrown = new Vector2I(Way == ShakeWay.Down ? 0 : by, Way == ShakeWay.Across ? 0 : by);
        _left = _left.Stepped();

        // One with no length takes its sides round again.
        if (_endless && _left.IsOver)
            _left = Countdown.Of(Sides);
    }
}
