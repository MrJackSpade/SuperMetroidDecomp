using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of Ridley's fireballs, and the fire it leaves. A fireball flies straight until it meets terrain. The first of
/// a burst then leaves a flare where it struck, which sends a flame racing away each way along the surface; each
/// flame leaves another behind it, a few times over, and dies where it runs into terrain.
/// </summary>
/// <remarks>
/// Its data says what it "is": a "fireball", a "flare" or a "flame", and names the shots it leaves, "flare_across",
/// "flare_down" and "flame". A flare's timeline gives the cue "flames" to send its flames off; a flame's gives "next"
/// to leave another behind it, and its data names its "final_timeline". The cues "stop" and "end" bring any of them
/// to rest and take it away.
/// </remarks>
public partial class RidleyFireball : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>What it is: "fireball", "flare" or "flame".</summary>
    [Export, DataValue("is")]
    public string Is { get; set; } = "fireball";

    /// <summary>The data file of the shot of a flare whose flames run across, left where a fireball strikes a floor.</summary>
    [Export, DataValue("flare_across")]
    public string FlareAcross { get; set; } = "";

    /// <summary>The data file of the shot of a flare whose flames run up and down, left where a fireball strikes a wall.</summary>
    [Export, DataValue("flare_down")]
    public string FlareDown { get; set; } = "";

    /// <summary>The data file of the shot of a flame.</summary>
    [Export, DataValue("flame")]
    public string Flame { get; set; } = "";

    /// <summary>The timeline of a flame dying where its surface ends.</summary>
    [Export, DataValue("final_timeline")]
    public string FinalTimeline { get; set; } = "";

    /// <summary>How fast a flame races away, in pixels an update.</summary>
    [Export, DataValue("flame_speed")]
    public int FlameSpeed { get; set; } = 14;

    private PixelVelocity _speed;
    private bool _stopped;

    /// <summary>How many more flames follow from it, one after another.</summary>
    public int Afterburns { get; set; }

    /// <summary>Whether a flare's flames run across rather than up and down.</summary>
    public bool RunsAcross { get; set; }

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
        // It moves itself, and meets terrain its own way.
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        switch (cue)
        {
            case "stop":
                _stopped = true;
                break;
            case "end":
                Leave(room);
                break;
            case "flames":
                Pixels speed = Pixels.FromWhole(FlameSpeed);
                foreach (Pixels way in new[] { speed, -speed })
                    LeaveFire(room, Flame, RunsAcross ? new PixelVelocity(way, Pixels.Zero) : new PixelVelocity(Pixels.Zero, -way), Afterburns, RunsAcross);
                break;
            case "next":
                if (Afterburns > 0)
                    LeaveFire(room, Flame, _speed, Afterburns - 1, RunsAcross);
                Afterburns = Math.Max(0, Afterburns - 1);
                break;
            default:
                throw new InvalidOperationException($"Ridley's fire does not know the cue \"{cue}\".");
        }
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_stopped)
            return;
        switch (Is)
        {
            case "fireball":
                bool struckWall = MoveAcrossMeetingTerrain(room, _speed.X);
                bool struckFloor = !struckWall && MoveDownMeetingTerrain(room, _speed.Y);
                if (!struckWall && !struckFloor)
                    break;
                // Where it strikes a wall its flames run up and down; on a floor or ceiling, across.
                if (Afterburns > 0)
                    LeaveFire(room, struckWall ? FlareDown : FlareAcross, PixelVelocity.Zero, Afterburns, !struckWall);
                Leave(room);
                break;

            case "flare":
                break;

            case "flame":
                // It races along its surface, and dies where it runs into terrain.
                Position += _speed.OverOneUpdate;
                if (room.Terrain.IsSolidIn(Area, Engine.Collision.SlopeTreatment.Wall))
                {
                    _stopped = true;
                    PlayTimeline(FinalTimeline);
                }

                break;

            default:
                throw new InvalidOperationException($"Ridley's fire is a \"{Is}\", which is none of the things it can be.");
        }
    }

    /// <summary>Leaves another piece of fire where it is.</summary>
    private void LeaveFire(Room room, string shotFile, PixelVelocity speed, int afterburns, bool runsAcross)
    {
        var fire = (RidleyFireball)ShotType.Read(room.Content.ReadData(shotFile)).Launch(Position, speed, Side, Launcher, Hit.Source);
        (fire.Afterburns, fire.RunsAcross) = (afterburns, runsAcross);
        room.Spawn(fire);
    }
}
