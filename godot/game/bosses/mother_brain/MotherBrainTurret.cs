using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the turrets in the ceiling of Mother Brain's room. While it is in view it turns, a step at a time, to and
/// fro between the two ways it is allowed to point, and every so often fires the way it points. Nothing harms it;
/// when Mother Brain's tank breaks she has them all put out.
/// </summary>
/// <remarks>
/// The ways it can point are numbered round from left: 0 left, 1 down and left, 2 down, 3 down and right, 4 right,
/// 5 up and right, 6 up, 7 up and left. It plays "pointing_0" to "pointing_7". Its shots are "bullet_0" to
/// "bullet_7", each with the speed and look of a bullet going that way; its data gives "muzzles", where a bullet
/// leaves it for each way, in the same order.
/// </remarks>
public partial class MotherBrainTurret : Enemy
{
    /// <summary>The way it points to begin with.</summary>
    [Export]
    public int StartsPointing { get; set; } = 2;

    /// <summary>The first and last of the ways it may point; it turns between them through the ways from the first round to the last.</summary>
    [Export]
    public Vector2I Sweep { get; set; } = new(1, 3);

    /// <summary>The least number of updates between one turn and the next; chance may make it up to 255.</summary>
    [Export, DataValue("least_turn_wait")]
    public int LeastTurnWait { get; set; } = 32;

    /// <summary>The least number of updates between one shot and the next; chance may make it up to 255.</summary>
    [Export, DataValue("least_fire_wait")]
    public int LeastFireWait { get; set; } = 128;

    /// <summary>The effect it leaves when it is put out, by the data file of its timeline.</summary>
    [Export, DataValue("smoke_effect")]
    public string SmokeEffect { get; set; } = "";

    /// <summary>How far right of its place the point lies that must be in view for it to turn and fire.</summary>
    [Export, DataValue("view_across")]
    public int ViewAcross { get; set; } = 4;

    /// <summary>How far below its place that point lies.</summary>
    [Export, DataValue("view_down")]
    public int ViewDown { get; set; } = 16;

    /// <summary>How far from the view's left edge that point may lie.</summary>
    [Export, DataValue("view_width")]
    public int ViewWidth { get; set; } = 264;

    /// <summary>How far from the view's top that point may lie.</summary>
    [Export, DataValue("view_height")]
    public int ViewHeight { get; set; } = 256;

    private readonly List<PixelOffset> _muzzles = [];
    private int _pointing;
    private int _turning = 1;
    private int _turnWait;
    private int _fireWait;

    /// <summary>
    /// What holds its place among the room's places for what enemies launch: on the cartridge a turret is one of the
    /// enemies' shots, and one the room has no place for is not there.
    /// </summary>
    private Effect? _place;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => false;

    /// <summary>Puts it out: with smoke if it is in view, and silently if not.</summary>
    public void PutOut()
    {
        if (IsDead)
            return;

        // It gives its place back first, and the smoke, one of the enemies' shots as well, takes the highest free.
        if (_place is { } place)
            Room.Remove(place);
        _place = null;
        if (IsWatched() && SmokeEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(SmokeEffect)), Position, holdsPlace: true));
        Vanish();
    }

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _muzzles.Clear();
        _muzzles.AddRange(data.List("muzzles").Select(muzzle => PixelOffset.FromWhole(muzzle.Int("x"), muzzle.Int("y"))));
        if (_muzzles.Count != 8)
            throw data.Problem("must give a muzzle for each of the eight ways it can point");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        // It takes its place as it is set up; with none free it is not there at all, and draws nothing.
        Effect place = Effect.HoldingPlace(Position);
        if (!Room.Spawn(place))
        {
            Vanish();
            return;
        }

        _place = place;
        AlwaysActive = true;
        _pointing = StartsPointing;
        _turnWait = Math.Max(Random.Draw().Part(0, 8), LeastTurnWait);
        _fireWait = Math.Max(Random.Draw().Part(0, 8), LeastFireWait);
        Play($"pointing_{_pointing}", restart: true);
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!IsWatched())
            return;
        if (--_turnWait <= 0)
        {
            _turnWait = Math.Max(Random.Draw().Part(0, 8), LeastTurnWait);
            // It turns on round; at the end of its sweep it turns back.
            int next = (_pointing + _turning + 8) % 8;
            if (!IsAllowed(next))
            {
                _turning = -_turning;
                next = (_pointing + _turning + 8) % 8;
            }

            _pointing = next;
            Play($"pointing_{_pointing}", restart: true);
        }

        if (--_fireWait > 0)
            return;
        _fireWait = Math.Max(Random.Draw().Part(0, 8), LeastFireWait);
        string shot = $"bullet_{_pointing}";
        Pixels speed = Shot(shot).Speed;
        (int across, int down) = _pointing switch
        {
            0 => (-1, 0),
            1 => (-1, 1),
            2 => (0, 1),
            3 => (1, 1),
            4 => (1, 0),
            5 => (1, -1),
            6 => (0, -1),
            _ => (-1, -1),
        };
        Fire(shot, Position + _muzzles[_pointing], new PixelVelocity(Scaled(speed, across), Scaled(speed, down)));
    }

    /// <summary>Whether the point a little right of and below it is in view: only then does it turn, fire, or smoke as it is put out.</summary>
    private bool IsWatched()
    {
        PixelRect view = Room.Session.Camera.View;
        int across = Position.X.Whole + ViewAcross - view.Left, down = Position.Y.Whole + ViewDown - view.Top;
        return across >= 0 && across < ViewWidth && down >= 0 && down < ViewHeight;
    }

    /// <summary>Whether a way lies within its sweep, going round from the first way to the last.</summary>
    private bool IsAllowed(int way)
    {
        int length = (Sweep.Y - Sweep.X + 8) % 8;
        return (way - Sweep.X + 8) % 8 <= length;
    }

    private static Pixels Scaled(Pixels speed, int way) => way == 0 ? Pixels.Zero : way > 0 ? speed : -speed;
}
