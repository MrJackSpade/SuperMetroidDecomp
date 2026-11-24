using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// What the eye of an eye door spits. It forms in front of the eye, then flies at where Samus was, gathering speed
/// all the way, and bursts on terrain.
/// </summary>
/// <remarks>
/// Its timeline gives the cue "aim" to take aim, "fly" to set off, "stop" to come to rest, and "end" to take it
/// away. Its data names its "burst_timeline".
/// </remarks>
public partial class EyeDoorSpit : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The timeline of its burst, by its data file.</summary>
    [Export, DataValue("burst_timeline")]
    public string BurstTimeline { get; set; } = "";

    private PixelVelocity _speed;
    private PixelVelocity _gain;
    private bool _flying;

    /// <inheritdoc/>
    protected override bool DeliversHitNow => _flying && base.DeliversHitNow;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        switch (cue)
        {
            case "aim":
                PixelOffset to = room.Player.Position - Position;
                int angle = Turns.AngleOf(to.X.Whole, to.Y.Whole);
                Pixels speed = Pixels.FromWhole(1);
                _speed = new PixelVelocity(Turns.Cosine(angle, speed), Turns.Sine(angle, speed));
                _gain = new PixelVelocity(_speed.X.Share(1, 16), _speed.Y.Share(1, 16));
                break;
            case "fly":
                _flying = true;
                break;
            case "stop":
                _flying = false;
                break;
            case "end":
                Leave(room);
                break;
            default:
                throw new InvalidOperationException($"The spit of an eye door does not know the cue \"{cue}\".");
        }
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!_flying)
            return;
        if (MoveAcrossMeetingTerrain(room, _speed.X) || MoveDownMeetingTerrain(room, _speed.Y))
        {
            Burst();
            return;
        }

        _speed = new PixelVelocity(_speed.X + _gain.X, _speed.Y + _gain.Y);

        // With its eye gone it is gone too.
        if (Launcher is EyeDoorEye { IsDestroyed: true })
            Burst();
    }

    private void Burst()
    {
        _flying = false;
        PlayTimeline(BurstTimeline);
    }
}
