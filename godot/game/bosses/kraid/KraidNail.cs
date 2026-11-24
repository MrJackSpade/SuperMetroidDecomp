using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the two nails Kraid flicks off once it stands at its full height. It drifts across the room on a slant,
/// turning back from the walls, the floor and ceiling, and Kraid's own body. As likely as not one starts from the far
/// left instead, waiting there unseen until the top spike has passed. It can be shot down, and breaks on Samus when
/// it hits her; another follows.
/// </summary>
/// <remarks>It plays "spinning". Its data gives "ledges": the left edge of Kraid's body it turns back from, for each height below "top".</remarks>
public partial class KraidNail : Enemy
{
    /// <summary>How many updates after Kraid has grown it first sets off.</summary>
    [Export]
    public int FirstLaunchDelay { get; set; } = 64;

    /// <summary>How many updates one that follows another waits before it sets off.</summary>
    [Export, DataValue("launch_delay")]
    public int LaunchDelay { get; set; } = 64;

    /// <summary>How far below Kraid's arm it sets off from, in pixels.</summary>
    [Export, DataValue("below_arm")]
    public int BelowArm { get; set; } = 128;

    /// <summary>How far left of Kraid's place it sets off from beside the body, in pixels, before it is put on a block's edge.</summary>
    [Export, DataValue("launch_reach")]
    public int LaunchReach { get; set; } = 64;

    /// <summary>The left edge of Kraid's body that turns back one above every ledge, from Kraid's place, in pixels.</summary>
    [Export, DataValue("past_ledges_left")]
    public int PastLedgesLeft { get; set; }

    /// <summary>Where one that starts from the far left waits, in the room's pixels.</summary>
    [Export]
    public Vector2I FarLeftStart { get; set; } = new(50, 240);

    private enum Phase
    {
        WaitForKraid,
        WaitBeforeLaunch,
        Launch,
        WaitForSpike,
        Fly,
    }

    private readonly List<(int Top, int Left)> _ledges = [];
    private Kraid? _kraid;
    private Phase _phase;
    private int _wait;
    private bool _away = true;

    /// <summary>Whether it started from the far left this time.</summary>
    public bool StartedFarLeft { get; private set; }

    /// <summary>The speed it flies at.</summary>
    public PixelVelocity Flight { get; private set; }

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _ledges.Clear();
        _ledges.AddRange(data.List("ledges").Select(ledge => (ledge.Int("top"), ledge.Int("left"))));
        if (_ledges.Count == 0)
            throw data.Problem("must give at least one ledge of Kraid's body");
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !_away && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        Play("spinning");
        SetAway(true);
        _phase = Phase.WaitForKraid;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!_away)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnTouchedPlayer() => Die();

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step) => Kraid.Shaded(step.Picture, _kraid?.Shade ?? "");

    /// <inheritdoc/>
    protected override void Behave()
    {
        _kraid ??= Room.Enemies.OfType<Kraid>().FirstOrDefault()
            ?? throw new InvalidOperationException($"The nail \"{Name}\" is in a room with no Kraid.");
        Kraid kraid = _kraid;
        if (!IsInstanceValid(kraid) || kraid.IsFinished)
        {
            // With Kraid gone no more follow.
            RespawnsWhenKilled = false;
            Die();
            return;
        }

        switch (_phase)
        {
            case Phase.WaitForKraid:
                if (!kraid.HasGrown)
                    break;
                // One that has waited for Kraid to grow waits its own time more; one that follows another, the usual time.
                (_phase, _wait) = (Phase.WaitBeforeLaunch, Updates <= 1 ? LaunchDelay : FirstLaunchDelay);
                break;

            case Phase.WaitBeforeLaunch:
                if (--_wait <= 0)
                    _phase = Phase.Launch;
                break;

            case Phase.Launch:
                Launch(kraid);
                break;

            case Phase.WaitForSpike:
                KraidLint? top = Room.Enemies.OfType<KraidLint>().MinBy(lint => lint.Height);
                if (top is not null && top.Position.X.Whole < 256)
                    break;
                _phase = Phase.Fly;
                SetAway(false);
                break;

            case Phase.Fly:
                bool turned = MoveAcrossBy(Flight.X);
                if (!turned && !Flight.X.IsNegative && Body is { } body)
                    turned = Position.X.Whole + body.Rect.Width / 2 >= kraid.Position.X.Whole + LedgeLeft(Position.Y.Whole - kraid.Position.Y.Whole);
                if (turned)
                    Flight = Flight.WithX(-Flight.X);
                if (MoveDownBy(Flight.Y))
                    Flight = Flight.WithY(-Flight.Y);
                break;

            default:
                throw new InvalidOperationException($"Kraid's nail has no phase {_phase}.");
        }
    }

    /// <summary>Sets it off: leftward, and up or down, whichever its fellow is not going.</summary>
    private void Launch(Kraid kraid)
    {
        KraidNail? fellow = Room.Enemies.OfType<KraidNail>().FirstOrDefault(nail => nail != this && !nail.IsDead);
        bool fellowRising = fellow is not null && fellow.Flight.Y.IsNegative;
        Flight = new PixelVelocity(Pixels.FromWhole(-1), Pixels.FromWhole(fellowRising ? 1 : -1));
        _phase = Phase.Fly;
        SetAway(false);
        if (Random.Last.Part(0, 1) != 1 || fellow is { StartedFarLeft: true })
        {
            // It starts beside Kraid's body, on a block's edge, below the arm.
            StartedFarLeft = false;
            int y = kraid.ArmPosition.Y.Whole + BelowArm;
            int x = (kraid.Position.X.Whole - LaunchReach) / 16 * 16;
            Position = PixelPosition.FromWhole(x, y);
            return;
        }

        StartedFarLeft = true;
        Position = PixelPosition.FromWhole(FarLeftStart.X, FarLeftStart.Y);
        Flight = new PixelVelocity(Pixels.FromWhole(1), Pixels.Zero);
        _phase = Phase.WaitForSpike;
        SetAway(true);
    }

    /// <summary>The left edge of Kraid's body that turns it back, from Kraid's place, at a height from Kraid's place.</summary>
    private int LedgeLeft(int relativeY)
    {
        foreach ((int top, int left) in _ledges)
        {
            if (top < relativeY)
                return left;
        }

        return PastLedgesLeft;
    }

    /// <summary>Puts it out of play, unseen and untouchable, or brings it back.</summary>
    private void SetAway(bool away)
    {
        _away = away;
        Visual?.SetShown(!away);
    }
}
