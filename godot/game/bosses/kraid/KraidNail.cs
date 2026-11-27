using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the two nails Kraid flicks off. It sets off a while after it is placed, even while Kraid is still rising,
/// and again a while after Kraid has grown to its full height. It drifts across the room on a slant,
/// turning back from the walls, the floor and ceiling, and Kraid's own body. As likely as not one starts from the far
/// left instead, waiting there unseen until the top spike has passed. It can be shot down, and breaks on Samus when
/// it hits her; another follows.
/// </summary>
/// <remarks>It plays "spinning". Its data gives "ledges": the left edge of Kraid's body it turns back from, for each height below "top".</remarks>
public partial class KraidNail : Enemy
{
    /// <summary>How many updates after Kraid has grown it sets off again.</summary>
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
    private bool _kraidHadGrown;

    /// <summary>The colours of Kraid's parts last shown on it.</summary>
    private ColourChange? _colours;

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
        // One that follows another is made in Kraid's parts' colours, as the old game's nail takes the shading its
        // place was left in. As the room is set up, Kraid shows them on the nails as he enters it.
        Room.Enemies.OfType<Kraid>().FirstOrDefault()?.ShowPartColoursOn(this);
        // Its wait to set off starts as it is placed, whether or not Kraid has grown: as in the old game, it may fly
        // while Kraid is still rising, out of the view.
        (_phase, _wait) = (Phase.WaitBeforeLaunch, LaunchDelay);
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!_away)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnTouchedPlayer() => Die();

    /// <summary>The effect of the second death the cartridge gives one a blow of Samus's own body has killed while another is to follow it.</summary>
    [Export, DataValue("second_death_effect")]
    public string SecondDeathEffect { get; set; } = "";

    private bool _killedByHerBody;

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        _killedByHerBody = hit.CarriesAny(Game.Data.HitKinds.OfHerBody);
        return true;
    }

    /// <inheritdoc/>
    protected override void OnDied()
    {
        // The cartridge's nail runs its death after its touch wherever the touch left its place taken. A blow of her
        // body that killed a nail another follows leaves the place waiting for that other, so the death runs again on
        // it: its explosion, the small one, stands at the room's corner and drops nothing. It is a death's remains
        // all the same: it is heard, and holds one of the room's places while it plays and for the pause a spent
        // drop keeps. (It counts a second kill too, which nothing in Kraid's room reads.)
        if (_killedByHerBody && RespawnsWhenKilled)
        {
            Sound(SoundOfDeath);
            Room.Spawn(Remains.Of(FrameTimeline.Read(Room.Content.ReadData(SecondDeathEffect)), PixelPosition.Origin, DropTable.Nothing));
        }
    }

    /// <summary>
    /// Shows it in the colours of Kraid's parts at once, in place of those it showed before. A change is worked out for
    /// it by the picture it shows, so while it shows none it is shown none.
    /// </summary>
    public void ShowColours(ColourChange colours)
    {
        ArgumentNullException.ThrowIfNull(colours);
        if (Visual is not { } visual || visual.Picture.Length == 0 || ReferenceEquals(_colours, colours))
            return;
        colours.ShowOn(visual, nameof(Kraid));
        _colours = colours;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_kraid is null)
        {
            // A nail comes back as a new one after it is shot, and Kraid may have gone meanwhile: its first turn then
            // finds none in the room, and it does as a nail whose Kraid has gone does. No more follow.
            if (Room.Enemies.OfType<Kraid>().FirstOrDefault() is not { } found)
            {
                RespawnsWhenKilled = false;
                Die();
                return;
            }

            _kraid = found;
            _kraidHadGrown = found.HasGrown;
        }

        Kraid kraid = _kraid;
        if (!IsInstanceValid(kraid) || kraid.IsFinished)
        {
            // With Kraid gone no more follow.
            RespawnsWhenKilled = false;
            Die();
            return;
        }

        if (kraid.HasGrown && !_kraidHadGrown)
        {
            // As Kraid finishes growing, each nail stops where it is and waits its own time before it sets off again.
            _kraidHadGrown = true;
            (_phase, _wait) = (Phase.WaitBeforeLaunch, FirstLaunchDelay);
        }

        switch (_phase)
        {
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
                bool turned = Room.Mover.MoveAcross(MovingBody(), Flight.X, FliesAcross).Blocked;
                if (!turned && !Flight.X.IsNegative && Body is { } body)
                    turned = Position.X.Whole + body.Rect.Width / 2 >= kraid.Position.X.Whole + LedgeLeft(Position.Y.Whole - kraid.Position.Y.Whole);
                if (turned)
                    Flight = Flight.WithX(-Flight.X);
                if (Room.Mover.MoveDown(MovingBody(), Flight.Y, Flies).Blocked)
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

    /// <summary>
    /// How its flight is tried, as the cartridge moves an enemy: only where each move would end, and sloped ground that
    /// holds things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Flies =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>How its flight across is tried: as the rest of it, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions FliesAcross = Flies with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"Kraid's nail \"{Name}\" has no body collider.");
}
