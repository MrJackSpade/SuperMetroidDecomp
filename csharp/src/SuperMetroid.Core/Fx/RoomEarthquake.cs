using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room's earthquake and its one owner: the shake every producer starts, changes, sustains or keeps going, the
/// frames it has left, and the foreground, background and enemy-projectile displacement it shows each update.
/// </summary>
/// <remarks>
/// A producer gives the earthquake a shake and a length (<see cref="Start"/>), runs it on with the shake it already has
/// (<see cref="RunFor"/>), changes its shake without touching its length (<see cref="ChangeShake"/>), sustains it
/// (<see cref="Sustain"/>), starts a shake that lasts until another producer gives it a length (<see cref="ShakeIndefinitely"/>)
/// or keeps it going indefinitely (<see cref="KeepShaking"/>). The shake and its length are
/// separate: a later producer may replace either while the other runs on, so a room shaken by a Super Missile between
/// Kraid's rise setup and his rise shakes with the missile's shake for the length of the rise.
/// </remarks>
internal sealed class RoomEarthquake
{
    /// <summary>
    /// How the earthquake runs (#2560): for a length, whose frames left <see cref="Advance"/> counts down while time is not
    /// frozen and whose count selects the side it shows, or continuously (<see cref="KeepShaking"/>), standing at a place in
    /// the sides it shows in turn (<see cref="RoomEarthquakeDefinitions.ShakeSides"/>) and moving on one each frame it
    /// shakes until a producer gives it a new length.
    /// </summary>
    private Run _run = new Run.ForLength(default);

    /// <summary>
    /// The shake the earthquake shows while it runs: the one its last producer gave it, or
    /// <see cref="RoomEarthquakeDefinitions.UnshakenRoomShake"/> in a room no producer has shaken yet, which the Zebes
    /// escape's continuous shaking shows until a producer gives one.
    /// </summary>
    public EarthquakeShake Shake { get; private set; } = RoomEarthquakeDefinitions.UnshakenRoomShake;

    /// <summary>Whether the room is shaking: it has frames left or shakes continuously.</summary>
    public bool IsShaking => _run is Run.Continuous or Run.ForLength { Remaining.IsZero: false };

    /// <summary>
    /// The frames the earthquake has left, read where an owner moves with the room's shake: Mother Brain's brain, the
    /// door Ridley's Ceres scene draws, and the rocks Kraid's rise throws up.
    /// </summary>
    /// <exception cref="InvalidOperationException">The room shakes continuously and has no length to read.</exception>
    public int RemainingFrames => _run is Run.ForLength length
        ? length.Remaining.RemainingFrames
        : throw new InvalidOperationException("A continuously shaking room has no frames left to read.");

    /// <summary>
    /// Displacement produced by the most recent <see cref="Advance"/>: the shake moves the shown layers without moving
    /// the camera.
    /// </summary>
    public RoomShakeFrameResult LastRoomShake { get; private set; }

    /// <summary>
    /// Whether the room shakes with <paramref name="shake"/> and exactly <paramref name="framesLeft"/> frames left: an
    /// earthquake a producer started for that long and that has not yet shaken a frame of it.
    /// </summary>
    internal bool IsShakingWith(EarthquakeShake shake, int framesLeft) =>
        _run is Run.ForLength length && Shake == shake && length.Remaining.IsAt(framesLeft);

    /// <summary>Starts the earthquake: <paramref name="shake"/> for <paramref name="frames"/> frames, replacing what ran.</summary>
    /// <param name="shake">The shake to show.</param>
    /// <param name="frames">The frames to shake, up to <see cref="WrappingCountdown.LongestStart"/>; zero stops the earthquake.</param>
    internal void Start(EarthquakeShake shake, int frames)
    {
        Shake = shake;
        RunFor(frames);
    }

    /// <summary>
    /// Runs the earthquake for <paramref name="frames"/> frames with the shake it already has, replacing its length and
    /// ending a continuous shake.
    /// </summary>
    /// <param name="frames">The frames to shake, up to <see cref="WrappingCountdown.LongestStart"/>; zero stops the earthquake.</param>
    internal void RunFor(int frames)
    {
        _run = new Run.ForLength(WrappingCountdown.Start(frames));
    }

    /// <summary>Stops the earthquake; its shake stays for a producer that runs it on.</summary>
    internal void Stop() => RunFor(0);

    /// <summary>Changes the shake the earthquake shows, leaving its length running.</summary>
    internal void ChangeShake(EarthquakeShake shake) => Shake = shake;

    /// <summary>
    /// Shakes the room with <paramref name="shake"/> for at least <see cref="RoomEarthquakeDefinitions.SustainedShakeFrames"/>
    /// more frames without restarting its length (<see cref="WrappingCountdown.Sustained"/>): rising lava and acid, and the
    /// Tourian statues, renew their earthquake this way every update they shake the room. A continuous shake shakes on as
    /// it was, with the new shake.
    /// </summary>
    internal void Sustain(EarthquakeShake shake)
    {
        Shake = shake;
        if (_run is Run.ForLength length)
            _run = new Run.ForLength(length.Remaining.Sustained(RoomEarthquakeDefinitions.SustainedShakeFrames));
    }

    /// <summary>
    /// Shakes the room with <paramref name="shake"/> until a producer gives the earthquake a length (#2013): the Zebes
    /// escape's room quakes and Mother Brain's escape rumble. The sides alternate from the first of them, the far side, as
    /// those quakes have always shown it.
    /// </summary>
    internal void ShakeIndefinitely(EarthquakeShake shake)
    {
        Shake = shake;
        _run = new Run.Continuous(CyclingSequence<EarthquakeSide>.StartOf(RoomEarthquakeDefinitions.ShakeSides));
    }

    /// <summary>
    /// Keeps the room shaking without re-arming its length every frame (#1415): the Zebes escape's outdoor rooms do this
    /// on every update. The earthquake then shakes until a producer gives it a new length, its sides alternating on from
    /// where the frames it had left put them.
    /// </summary>
    internal void KeepShaking()
    {
        if (_run is Run.ForLength length)
            _run = new Run.Continuous(length.Remaining.PlaceIn(RoomEarthquakeDefinitions.ShakeSides));
    }

    /// <summary>
    /// Whether the shake shows the side of the origin opposite its displacement this frame: the side a continuous shake
    /// stands at, or the one the frames left select as the earthquake runs down
    /// (<see cref="RoomEarthquakeDefinitions.ShakeSides"/>).
    /// </summary>
    private EarthquakeSide ShownSide => _run switch
    {
        Run.Continuous continuous => continuous.Sides.Current,
        Run.ForLength length => length.Remaining.PlaceIn(RoomEarthquakeDefinitions.ShakeSides).Current,
        _ => throw new InvalidOperationException($"Undefined earthquake run {_run}."),
    };

    /// <summary>The room-shake offset for projectiles, evaluated at each projectile draw pass before the earthquake's frame step.</summary>
    internal Offset16<RoomPixels> GetEnemyProjectileShake(bool timeIsFrozen)
    {
        if (!IsShaking || timeIsFrozen)
            return default;
        return ShownSide.Showing(Shake.Displacement.Projectile);
    }

    /// <summary>
    /// The earthquake part of room shaking: this frame's background displacement and
    /// frame step. Returns whether the shake also shakes the actors selected for processing.
    /// </summary>
    internal bool Advance(bool timeIsFrozen)
    {
        LastRoomShake = default;

        // Frozen time leaves both the earthquake's length and the layer scrolls untouched.
        if (!IsShaking || timeIsFrozen)
            return false;

        EarthquakeShake shake = Shake;
        RoomShakeDefinition definition = shake.Displacement;
        Offset16<RoomPixels> foreground = definition.Foreground;
        Offset16<BackgroundPixels> background = definition.Background;

        // The earthquake alternates which side of the origin is shown, the far side being the negated displacement.
        EarthquakeSide side = ShownSide;
        foreground = side.Showing(foreground);
        background = side.Showing(background);

        _run = _run switch
        {
            Run.Continuous continuous => new Run.Continuous(continuous.Sides.Next),
            Run.ForLength length => new Run.ForLength(length.Remaining.Decrement().Remaining),
            _ => throw new InvalidOperationException($"Undefined earthquake run {_run}."),
        };
        LastRoomShake = new RoomShakeFrameResult(foreground, background);
        return shake.ShakesEnemies;
    }

    /// <summary>How the earthquake runs (#2560), one value: each case carries only what it counts.</summary>
    private abstract record Run
    {
        /// <summary>The earthquake shakes for a length.</summary>
        /// <param name="Remaining">The frames it has left.</param>
        internal sealed record ForLength(WrappingCountdown Remaining) : Run;

        /// <summary>The earthquake shakes until a producer gives it a length.</summary>
        /// <param name="Sides">Where it stands in the sides it shows in turn.</param>
        internal sealed record Continuous(CyclingSequence<EarthquakeSide> Sides) : Run;
    }
}
