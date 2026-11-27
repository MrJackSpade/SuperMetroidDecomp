using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Polyp: a vent in the floor of a lava cave that lobs glowing rocks up into the air while Samus is near, each a
/// random height and drift, with a random wait between.
/// </summary>
/// <remarks>
/// Its timeline is "stationary". Its shot is "rock". Its data lists the sixteen speeds across a rock may leave with
/// under "rock_drifts", and the eight waits between rocks under "cooldowns".
/// </remarks>
public partial class Polyp : Enemy
{
    /// <summary>How near Samus must be, in pixels across and down, for it to lob rocks.</summary>
    [Export, DataValue("reach")]
    public int Reach { get; set; } = 64;

    /// <summary>The mark of the speed curve its lowest lob starts from; a higher lob starts up to fifteen marks further on.</summary>
    [Export, DataValue("first_launch_mark")]
    public int FirstLaunchMark { get; set; } = 28;

    private enum Doing
    {
        WaitingForSamus,
        AboutToLob,
        Cooling,
    }

    private readonly List<Pixels> _drifts = [];
    private readonly List<int> _cooldowns = [];
    private Countdown _cooldown;
    private Doing _doing;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _drifts.Clear();
        _drifts.AddRange(data.List("rock_drifts").Select(drift => drift.AsPixels()));
        _cooldowns.Clear();
        _cooldowns.AddRange(data.List("cooldowns").Select(wait => wait.AsInt()));
        if (_drifts.Count != 16 || _cooldowns.Count != 8)
            throw data.Problem("must list sixteen rock drifts and eight cooldowns");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("stationary");
        Random.Restart(17);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.WaitingForSamus:
                // Samus near sets it to lob on its next update, wherever she is then.
                if (Math.Abs(Target.X.Whole - Position.X.Whole) < Reach && Math.Abs(Target.Y.Whole - Position.Y.Whole) < Reach)
                    _doing = Doing.AboutToLob;
                return;

            case Doing.AboutToLob:
                // Three fresh numbers: the drift, the height, and the wait before the next rock.
                Pixels drift = _drifts[Random.Draw().Part(1, 4)];
                int launchMark = FirstLaunchMark + Random.Draw().Part(1, 4);
                var rock = (PolypRock)Fire("rock", Position, new PixelVelocity(drift, Pixels.Zero));
                rock.StartAt(launchMark);
                _cooldown = Countdown.Of(_cooldowns[Random.Draw().Part(1, 3)]);
                _doing = Doing.Cooling;
                return;

            case Doing.Cooling:
                if (_cooldown.IsOver)
                    return;
                _cooldown = _cooldown.Stepped();
                if (_cooldown.IsOver)
                    _doing = Doing.WaitingForSamus;
                return;

            default:
                throw new InvalidOperationException($"Undefined Polyp stage {_doing}.");
        }
    }
}
