using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// What the eye of an eye door spits. It forms in front of the eye, then flies at where Samus was, gathering speed
/// all the way, and bursts on terrain. A shot stops it where it is in a cloud of fire, which its settings play as its
/// end. It harms her at a touch from the update it is spat, forming, flying and bursting alike, as the cartridge's
/// does, which is never told to pass her by.
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
                // As the cartridge has it: a speed of one pixel along its aim, by the sine in whole 256ths, and a sixteenth
                // of that gained each update, rounded down, so leftward and upward a little more.
                int heading = CartridgeAngles.AimAngle(room.Player.Position.X.Whole - Position.X.Whole, room.Player.Position.Y.Whole - Position.Y.Whole);
                int across = CartridgeAngles.SignedSine(heading + Turns.PerTurn / 4), down = CartridgeAngles.SignedSine(heading + Turns.PerTurn / 2);
                _speed = new PixelVelocity(TwoFiftySixths(across), TwoFiftySixths(down));
                _gain = new PixelVelocity(TwoFiftySixths(Math.DivRem(across, 16, out int acrossLeft) - (acrossLeft < 0 ? 1 : 0)),
                    TwoFiftySixths(Math.DivRem(down, 16, out int downLeft) - (downLeft < 0 ? 1 : 0)));
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

    /// <summary>So many 256ths of a pixel.</summary>
    private static Pixels TwoFiftySixths(int count) => Pixels.FromWhole(count).Share(1, 256);
}
