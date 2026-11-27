using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A falling spark, dripped by a Spark: it falls, drifting a little to one side and leaving a trail, and where it
/// meets the floor it skips up and skitters off sideways, four times as fast, until the timeline it skitters by ends.
/// </summary>
public partial class FallingSpark : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.25;

    /// <summary>The whole pixels of falling speed at which it stops gaining more.</summary>
    [Export, DataValue("fall_speed_limit")]
    public int FallSpeedLimit { get; set; } = 4;

    /// <summary>How many times faster it moves across after meeting the floor.</summary>
    [Export, DataValue("skitter_times")]
    public int SkitterTimes { get; set; } = 4;

    /// <summary>The upward speed it skips with on meeting the floor.</summary>
    [Export, DataValue("skip_speed")]
    public double SkipSpeed { get; set; } = 0.5;

    /// <summary>How far it is lifted clear of the floor it met, in pixels.</summary>
    [Export, DataValue("skip_lift")]
    public int SkipLift { get; set; } = 2;

    /// <summary>How many updates pass between one piece of its trail and the next.</summary>
    [Export, DataValue("trail_every")]
    public int TrailEvery { get; set; } = 4;

    /// <summary>The data file of the timeline it is shown by once it has met the floor, which ends it; empty to go on as it was.</summary>
    [Export, DataValue("skitter_timeline")]
    public string SkitterTimeline { get; set; } = "";

    /// <summary>The data file of the timeline of each piece of its trail.</summary>
    [Export, DataValue("trail_effect")]
    public string TrailEffect { get; set; } = "";

    private bool _skipped;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room) => DataValues.Fill(this, room.Content);

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!Velocity.Y.IsNegative)
        {
            // Its settings let it through terrain, so that meeting the floor is its own to act on.
            if (!_skipped && room.Terrain.IsSolidIn(Area.MovedBy(0, Math.Max(1, Velocity.Y.Whole)), SlopeTreatment.Wall))
            {
                _skipped = true;
                Velocity = new PixelVelocity(Velocity.X * SkitterTimes, -Pixels.FromNumber(SkipSpeed));
                Position -= PixelOffset.FromWhole(0, SkipLift);
                if (SkitterTimeline.Length > 0)
                    PlayTimeline(SkitterTimeline);
                return;
            }

            Pixels faster = Velocity.Y + Pixels.FromNumber(Gravity);
            if (faster.Whole < FallSpeedLimit)
                Velocity = Velocity.WithY(faster);
        }

        // Its trail is timed by the short count, which a door's scroll starts over, as the old game's sparks are, and
        // is left where this update's move takes it.
        if (TrailEffect.Length > 0 && room.Session.Clock.ShortUpdates % TrailEvery == 0)
            room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(TrailEffect)), Position + Velocity.OverOneUpdate));
    }
}
