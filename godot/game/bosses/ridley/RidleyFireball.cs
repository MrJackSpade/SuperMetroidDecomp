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
/// to leave another behind it, and its data names its "final_timeline". The cue "stop" brings any of them to rest,
/// "fly" sets a fireball off again, and "end" takes any of them away. A flame leaps one step along its surface as
/// it appears and burns there. A flame does the harm of the fireball it came of; a flare does its own shot's.
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
    private bool _leapt;
    private int _flameDamage;

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
        if (Is == "fireball")
            _flameDamage = Hit.Damage;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        switch (cue)
        {
            case "stop":
                _stopped = true;
                break;
            case "fly":
                _stopped = false;
                break;
            case "end":
                Leave(room);
                break;
            case "flames":
                Pixels speed = Pixels.FromWhole(FlameSpeed);
                foreach (Pixels way in new[] { speed, -speed })
                    LeaveFire(room, Flame, RunsAcross ? new PixelVelocity(way, Pixels.Zero) : new PixelVelocity(Pixels.Zero, -way), Afterburns, RunsAcross, _flameDamage);
                break;
            case "next":
                if (Afterburns > 0)
                    LeaveFire(room, Flame, _speed, Afterburns - 1, RunsAcross, _flameDamage);
                Afterburns = Math.Max(0, Afterburns - 1);
                break;
            default:
                throw new InvalidOperationException($"Ridley's fire does not know the cue \"{cue}\".");
        }
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (Is == "flame")
        {
            // It leaps one step along its surface as it appears, and dies there if that is inside terrain.
            if (_leapt)
                return;
            _leapt = true;
            Position += _speed.OverOneUpdate;
            if (room.Terrain.IsSolidIn(Area, Engine.Collision.SlopeTreatment.Wall))
                PlayTimeline(FinalTimeline);
            return;
        }

        if (_stopped)
            return;
        switch (Is)
        {
            case "fireball":
                bool struckWall = MoveAcrossMeetingTerrain(room, _speed.X);
                bool struckFloor = !struckWall && MoveDownMeetingTerrain(room, _speed.Y);
                if (!struckWall && !struckFloor)
                    break;
                // Where it strikes a wall its flames run up and down; on a floor or ceiling, across. It goes on as
                // the flare: the flare holds the place the fireball held among what the enemies have launched, and
                // has its first turn here, within the fireball's, as the cartridge's fireball carries on where it is
                // with the flare's part to play. The flames it sends off then take places after its own, and each
                // has its first turn on the update it appears.
                if (Afterburns > 0)
                {
                    RidleyFireball flare = Fire(room, struckWall ? FlareDown : FlareAcross, PixelVelocity.Zero, Afterburns, !struckWall, null);
                    room.SpawnInPlaceOf(this, flare);
                    Leave(room);
                    flare.Step();
                    break;
                }

                Leave(room);
                break;

            case "flare":
                break;

            default:
                throw new InvalidOperationException($"Ridley's fire is a \"{Is}\", which is none of the things it can be.");
        }
    }

    /// <summary>Leaves another piece of fire where it is, doing the harm given, or its own shot's when none is.</summary>
    private void LeaveFire(Room room, string shotFile, PixelVelocity speed, int afterburns, bool runsAcross, int? damage) =>
        room.Spawn(Fire(room, shotFile, speed, afterburns, runsAcross, damage));

    /// <summary>Makes another piece of fire where it is, not yet in the room.</summary>
    private RidleyFireball Fire(Room room, string shotFile, PixelVelocity speed, int afterburns, bool runsAcross, int? damage)
    {
        ShotType shot = ShotType.Read(room.Content.ReadData(shotFile));
        var fire = (RidleyFireball)shot.LaunchDoing(damage ?? shot.Damage, Position, speed, Side, Launcher, Hit.Source);
        (fire.Afterburns, fire.RunsAcross, fire._flameDamage) = (afterburns, runsAcross, _flameDamage);
        return fire;
    }
}
