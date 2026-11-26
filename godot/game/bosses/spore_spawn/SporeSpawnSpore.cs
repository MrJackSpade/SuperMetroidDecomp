using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of Spore Spawn's spores: it drifts down from the ceiling along a set, wavering path, to one side or the other,
/// and is gone when it reaches the floor. Shot, it bursts where it is and drifts no further: harmless from the burst's second update,
/// and leaving what its settings say it may drop as the burst ends.
/// </summary>
/// <remarks>
/// Its data lists its path under "drift": the step it takes each update, drifting right, round and round. The
/// timeline of its burst gives the cues "harmless", from which it no longer hurts; "drop", on which it leaves its
/// drop; and "sound:" and a sound's name.
/// </remarks>
public partial class SporeSpawnSpore : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How far below where it started it is gone, in pixels.</summary>
    [Export, DataValue("fall_distance")]
    public int FallDistance { get; set; } = 248;

    /// <summary>The timeline of its burst when it is shot, by its data file.</summary>
    [Export, DataValue("burst_timeline")]
    public string BurstTimeline { get; set; } = "";

    private readonly List<PixelOffset> _drift = [];
    private bool _driftsLeft;
    private int _step;
    private int _startY;
    private bool _bursting;
    private bool _harmless;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
        _drift.Clear();
        _drift.AddRange(data.List("drift").Select(step => PixelOffset.FromWhole(step.Int("x"), step.Int("y"))));
        if (_drift.Count == 0)
            throw data.Problem("must list at least one drift step");
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !_bursting && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override bool DeliversHitNow => !_harmless && base.DeliversHitNow;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        // A shot stops on it and sets off its burst, which plays where it was shot: a shot spore drifts no further.
        _bursting = true;
        PlayTimeline(BurstTimeline);
        return HitResult.Destroyed;
    }

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        // The speed it is launched with only says which way it drifts; its path does the moving.
        _driftsLeft = Velocity.X.IsNegative;
        Velocity = PixelVelocity.Zero;
        _startY = Position.Y.Whole;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        if (cue == "harmless")
            _harmless = true;
        else if (cue == "drop")
        {
            if (Settings.Drops is { } drops)
                Pickup.Dropped(room, drops, Position);
        }
        else if (cue.StartsWith("sound:", StringComparison.Ordinal))
            room.Session.Sounds.Request(cue["sound:".Length..], channel: "enemies");
        else
            throw new InvalidOperationException($"A spore's timeline gives the cue \"{cue}\", which it does not know.");
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_bursting)
        {
            if (_harmless)
                MeetWhileHarmless(room);
            return;
        }

        PixelOffset step = _drift[_step];
        Position += _driftsLeft ? new PixelOffset(-step.X, step.Y) : step;
        _step = (_step + 1) % _drift.Count;
        if (Position.Y.Whole >= _startY + FallDistance)
        {
            Leave(room);
            return;
        }

        if (_harmless)
            MeetWhileHarmless(room);
    }

    /// <summary>
    /// A bursting spore that has turned harmless still meets Samus: it knocks her back for no damage and is gone, as the
    /// old game's spore did.
    /// </summary>
    private void MeetWhileHarmless(Room room)
    {
        var away = PixelOffset.FromWhole(room.Player.Position.X >= Position.X ? 1 : -1, 0);
        Hit touch = Hit with { Damage = 0, Direction = away };
        if (room.DeliverHit(touch, Area, from: Launcher).Any(met => met.Target == room.Player && met.Result != HitResult.Ignored))
            Leave(room);
    }
}
