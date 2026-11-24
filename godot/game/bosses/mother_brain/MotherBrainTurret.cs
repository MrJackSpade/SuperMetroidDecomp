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

    private readonly List<PixelOffset> _muzzles = [];
    private int _pointing;
    private int _turning = 1;
    private int _turnWait;
    private int _fireWait;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => false;

    /// <summary>Puts it out: with smoke if it is in view, and silently if not.</summary>
    public void PutOut()
    {
        if (IsDead)
            return;
        if (IsInView(16) && SmokeEffect.Length > 0)
            LeaveEffect(SmokeEffect, Position);
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
        AlwaysActive = true;
        _pointing = StartsPointing;
        _turnWait = Math.Max(Random.Last.Part(0, 8), LeastTurnWait);
        _fireWait = Math.Max(Random.Last.Part(8, 8), LeastFireWait);
        Play($"pointing_{_pointing}", restart: true);
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!IsInView(16))
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

    /// <summary>Whether a way lies within its sweep, going round from the first way to the last.</summary>
    private bool IsAllowed(int way)
    {
        int length = (Sweep.Y - Sweep.X + 8) % 8;
        return (way - Sweep.X + 8) % 8 <= length;
    }

    private static Pixels Scaled(Pixels speed, int way) => way == 0 ? Pixels.Zero : way > 0 ? speed : -speed;
}
