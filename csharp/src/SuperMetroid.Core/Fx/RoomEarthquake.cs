namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room's earthquake and its one owner: the shake every producer starts, changes, sustains or keeps going, the
/// frames it has left, and the foreground, background and enemy-projectile displacement it shows each update.
/// </summary>
/// <remarks>
/// A producer gives the earthquake a shake and a length (<see cref="Start"/>), runs it on with the shake it already has
/// (<see cref="RunFor"/>), changes its shake without touching its length (<see cref="ChangeShake"/>), sustains it
/// (<see cref="Sustain"/>) or keeps it going indefinitely (<see cref="KeepShaking"/>). The shake and its length are
/// separate: a later producer may replace either while the other runs on, so a room shaken by a Super Missile between
/// Kraid's rise setup and his rise shakes with the missile's shake for the length of the rise.
/// </remarks>
internal sealed class RoomEarthquake
{
    /// <summary>
    /// The frames the earthquake has left, decremented by <see cref="Advance"/> while it shakes and time is not frozen.
    /// A continuous earthquake keeps counting below zero through the countdown's wrap: the side it shows follows the
    /// count, so the alternation runs on unbroken.
    /// </summary>
    private WrappingCountdown _remaining;

    /// <summary>
    /// Whether the room shakes continuously (<see cref="KeepShaking"/>): it shakes whatever frames are left, until a
    /// producer gives it a new length.
    /// </summary>
    private bool _continuous;

    /// <summary>
    /// The shake the earthquake shows while it runs: the one its last producer gave it, or none in a room no producer has
    /// shaken yet.
    /// </summary>
    public EarthquakeShake? Shake { get; private set; }

    /// <summary>Whether the room is shaking: it has frames left or shakes continuously.</summary>
    public bool IsShaking => _continuous || !_remaining.IsZero;

    /// <summary>
    /// The frames the earthquake has left, read where an owner moves with the room's shake: Mother Brain's brain, the
    /// door Ridley's Ceres scene draws, and the rocks Kraid's rise throws up.
    /// </summary>
    /// <exception cref="InvalidOperationException">The room shakes continuously and has no length to read.</exception>
    public int RemainingFrames => _continuous
        ? throw new InvalidOperationException("A continuously shaking room has no frames left to read.")
        : _remaining.RemainingFrames;

    /// <summary>
    /// Displacement produced by the most recent <see cref="Advance"/>. It is deliberately a delta rather than
    /// mutated camera state: the game adjusts its layer scroll copies after camera logic, without
    /// moving the world viewport.
    /// </summary>
    public RoomShakeFrameResult LastRoomShake { get; internal set; }

    /// <summary>
    /// Whether the room shakes with <paramref name="shake"/> and exactly <paramref name="framesLeft"/> frames left: an
    /// earthquake a producer started for that long and that has not yet shaken a frame of it.
    /// </summary>
    internal bool IsShakingWith(EarthquakeShake shake, int framesLeft) =>
        !_continuous && Shake == shake && _remaining.IsAt(framesLeft);

    /// <summary>Starts the earthquake: <paramref name="shake"/> for <paramref name="frames"/> frames, replacing what ran.</summary>
    /// <param name="shake">The shake to show.</param>
    /// <param name="frames">The frames to shake, 0 through 65535; zero stops the earthquake.</param>
    internal void Start(EarthquakeShake shake, int frames)
    {
        Shake = shake;
        RunFor(frames);
    }

    /// <summary>
    /// Runs the earthquake for <paramref name="frames"/> frames with the shake it already has, replacing its length and
    /// ending a continuous shake.
    /// </summary>
    /// <param name="frames">The frames to shake, 0 through 65535; zero stops the earthquake.</param>
    /// <exception cref="InvalidOperationException">No producer has given the room a shake to run.</exception>
    internal void RunFor(int frames)
    {
        if (frames != 0 && Shake is null)
            throw new InvalidOperationException("The room's earthquake was run on before any producer gave it a shake.");
        _remaining = WrappingCountdown.Start(frames);
        _continuous = false;
    }

    /// <summary>Stops the earthquake; its shake stays for a producer that runs it on.</summary>
    internal void Stop() => RunFor(0);

    /// <summary>Changes the shake the earthquake shows, leaving its length running.</summary>
    internal void ChangeShake(EarthquakeShake shake) => Shake = shake;

    /// <summary>
    /// Shakes the room with <paramref name="shake"/> for at least <see cref="RoomEarthquakeDefinitions.SustainedShakeFrames"/>
    /// more frames without restarting its length (<see cref="WrappingCountdown.Sustained"/>): rising lava and acid, and the
    /// Tourian statues, renew their earthquake this way every update they shake the room.
    /// </summary>
    internal void Sustain(EarthquakeShake shake)
    {
        Shake = shake;
        _remaining = _remaining.Sustained(RoomEarthquakeDefinitions.SustainedShakeFrames);
    }

    /// <summary>
    /// Keeps the room shaking without re-arming its length every frame (#1415): the Zebes escape's outdoor rooms do this
    /// on every update. The earthquake then shakes until a producer gives it a new length.
    /// </summary>
    internal void KeepShaking() => _continuous = true;

    /// <summary>
    /// Whether the shake shows the side of the origin opposite its displacement: the sides alternate every
    /// <see cref="RoomEarthquakeDefinitions.StepsPerShakeSide"/> frames as the earthquake runs down, starting from the far
    /// side.
    /// </summary>
    private bool ShowsOppositeSide => _remaining.IsInAlternatePhase(RoomEarthquakeDefinitions.StepsPerShakeSide);

    /// <summary>The shake of a shaking room.</summary>
    private EarthquakeShake RunningShake => Shake ?? throw new InvalidOperationException(
        "The room shakes with no shake given by any producer.");

    /// <summary>The room-shake offset for projectiles, evaluated at each projectile draw pass before the earthquake's frame step.</summary>
    internal Offset16<RoomPixels> GetEnemyProjectileShake(bool timeIsFrozen)
    {
        if (!IsShaking || timeIsFrozen)
            return default;
        RoomShakeDefinition definition = RunningShake.Displacement;
        return ShowsOppositeSide
            ? -definition.Projectile
            : definition.Projectile;
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

        EarthquakeShake shake = RunningShake;
        RoomShakeDefinition definition = shake.Displacement;
        Offset16<RoomPixels> foreground = definition.Foreground;
        Offset16<BackgroundPixels> background = definition.Background;

        // The earthquake alternates which side of the origin is shown, the far side being the negated displacement.
        if (ShowsOppositeSide)
        {
            foreground = -foreground;
            background = -background;
        }

        _remaining = _remaining.Decrement().Remaining;
        LastRoomShake = new RoomShakeFrameResult(foreground, background);
        return shake.ShakesEnemies;
    }
}
