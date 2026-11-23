using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Covern: a ghost of the Wrecked Ship. Unseen, it watches how Samus moves; when she has stood still for a while,
/// or kept one course for a moment, it appears ahead of her, flickering in, bobs there for a time, able to hurt and
/// be hurt, and fades away again.
/// </summary>
/// <remarks>
/// Its timeline is "floating". Its data lists its flicker under "flicker": each step's length in updates, and whether
/// the step leaves it shown.
/// </remarks>
public partial class Covern : Enemy
{
    /// <summary>How many updates it stays away between appearances.</summary>
    [Export, DataValue("reappearance_delay")]
    public int ReappearanceDelay { get; set; } = 120;

    /// <summary>How many more updates it stays away before its first appearance.</summary>
    [Export, DataValue("first_extra_delay")]
    public int FirstExtraDelay { get; set; } = 160;

    /// <summary>How many updates it bobs in view.</summary>
    [Export, DataValue("visible_updates")]
    public int VisibleUpdates { get; set; } = 120;

    /// <summary>How many updates Samus must stay within a pixel of one spot for it to appear on her.</summary>
    [Export, DataValue("still_updates")]
    public int StillUpdates { get; set; } = 64;

    /// <summary>How many updates Samus must keep one course for it to appear ahead of her.</summary>
    [Export, DataValue("steady_course_updates")]
    public int SteadyCourseUpdates { get; set; } = 16;

    /// <summary>How far ahead of Samus it appears along each way she is moving, in pixels.</summary>
    [Export, DataValue("lead_distance")]
    public int LeadDistance { get; set; } = 64;

    /// <summary>How many updates it takes to brighten out of the dark.</summary>
    [Export, DataValue("brighten_updates")]
    public int BrightenUpdates { get; set; } = 16;

    /// <summary>How many updates it then takes to settle into its own colours.</summary>
    [Export, DataValue("materialise_updates")]
    public int MaterialiseUpdates { get; set; } = 16;

    /// <summary>How many updates it takes to fade away.</summary>
    [Export, DataValue("vanish_updates")]
    public int VanishUpdates { get; set; } = 16;

    /// <summary>The speed its bobbing starts with, as a number of pixels an update.</summary>
    [Export, DataValue("bob_start_speed")]
    public double BobStartSpeed { get; set; } = 1;

    /// <summary>The speed its bobbing gains each update, back toward where it appeared.</summary>
    [Export, DataValue("bob_quickening")]
    public double BobQuickening { get; set; } = 0.09375;

    private enum Doing
    {
        Away,
        Watching,
        Brightening,
        Materialising,
        Bobbing,
        Vanishing,
    }

    private readonly List<(int Updates, bool Shows)> _flicker = [];
    private Doing _doing;
    private Countdown _wait;
    private Countdown _still;
    private Countdown _steady;
    private Countdown _flickerWait;
    private int _flickerStep;
    private int _fade;
    private int _appearedAtY;
    private (int X, int Y) _lastSamus;
    private (int X, int Y) _course;
    private float _alpha;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => _doing == Doing.Bobbing && base.IsHitBy(side);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _flicker.Clear();
        _flicker.AddRange(data.List("flicker").Select(step => (step.Int("updates"), step.Flag("shows"))));
    }

    /// <inheritdoc/>
    protected override Color RestingTint => new(1, 1, 1, _alpha);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("floating");
        Visual?.SetShown(false);
        _wait = Countdown.Of(ReappearanceDelay + FirstExtraDelay);
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (_doing == Doing.Bobbing)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Away:
                if (Ends(ref _wait))
                    _doing = Doing.Watching;
                return;

            case Doing.Watching:
                Watch();
                return;

            case Doing.Brightening:
                Flicker();
                _alpha = Math.Min(0.5f, 0.5f * ++_fade / BrightenUpdates);
                if (_fade < BrightenUpdates)
                    return;
                _doing = Doing.Materialising;
                _fade = 0;
                return;

            case Doing.Materialising:
                bool settling = _fade < MaterialiseUpdates;
                if (settling)
                    _alpha = 0.5f + 0.5f * ++_fade / MaterialiseUpdates;
                Flicker();
                if (_flickerWait.IsRunning || settling)
                    return;
                Visual?.SetShown(true);
                _doing = Doing.Bobbing;
                _appearedAtY = Position.Y.Whole;
                _wait = Countdown.Of(VisibleUpdates);
                Velocity = new PixelVelocity(Pixels.Zero, Pixels.FromNumber(BobStartSpeed));
                _course = (0, 0);
                _lastSamus = (Target.X.Whole, Target.Y.Whole);
                return;

            case Doing.Bobbing:
                Position += new PixelOffset(Pixels.Zero, Velocity.Y);
                Pixels pull = Pixels.FromNumber(BobQuickening);
                Velocity = Velocity.WithY(Velocity.Y + (Position.Y.Whole < _appearedAtY ? pull : -pull));
                if (!Ends(ref _wait))
                    return;
                _doing = Doing.Vanishing;
                _fade = 0;
                return;

            case Doing.Vanishing:
                _alpha = 1f - (float)++_fade / VanishUpdates;
                if (_fade < VanishUpdates)
                    return;
                _doing = Doing.Away;
                Visual?.SetShown(false);
                _wait = Countdown.Of(ReappearanceDelay);
                return;

            default:
                throw new InvalidOperationException($"Undefined Covern stage {_doing}.");
        }
    }

    /// <summary>Counts one update of a wait that is running, and returns whether that ended it.</summary>
    private static bool Ends(ref Countdown wait)
    {
        if (wait.IsOver)
            return false;
        wait = wait.Stepped();
        return wait.IsOver;
    }

    /// <summary>Watches Samus for one update, and appears if she has stood still, or kept one course, long enough.</summary>
    private void Watch()
    {
        (int X, int Y) samus = (Target.X.Whole, Target.Y.Whole);
        bool still = Math.Abs(samus.X - _lastSamus.X) <= 1 && Math.Abs(samus.Y - _lastSamus.Y) <= 1
            && samus.X - _lastSamus.X < 1 && samus.Y - _lastSamus.Y < 1;
        if (still)
        {
            if (Ends(ref _still))
            {
                _course = (0, 0);
                Appear(samus);
                return;
            }
        }
        else
        {
            _still = Countdown.Of(StillUpdates);
            int across = Math.Sign(samus.X - _lastSamus.X), down = Math.Sign(samus.Y - _lastSamus.Y);
            if (across != _course.X)
            {
                _course = (across, _course.Y);
                _steady = Countdown.Of(SteadyCourseUpdates);
            }
            else if (down != _course.Y)
            {
                _course = (_course.X, down);
                _steady = Countdown.Of(SteadyCourseUpdates);
            }
            else if (Ends(ref _steady))
            {
                Appear(samus);
                return;
            }
        }

        _lastSamus = samus;
    }

    /// <summary>Appears ahead of Samus along her course, and starts flickering in.</summary>
    private void Appear((int X, int Y) samus)
    {
        // Heading up and to the right, it appears level with her: the cartridge's table has it so.
        int down = _course is (1, -1) ? 0 : _course.Y;
        Position = PixelPosition.FromWhole(samus.X + _course.X * LeadDistance, samus.Y + down * LeadDistance);
        _doing = Doing.Brightening;
        _fade = 0;
        _alpha = 0;
        _flickerWait = Countdown.Of(1);
        _flickerStep = 0;
        _still = Countdown.Of(StillUpdates);
        _steady = Countdown.Of(SteadyCourseUpdates);
    }

    /// <summary>Moves its flicker on by one update: at the end of a step that shows it, it is shown.</summary>
    private void Flicker()
    {
        if (_flickerWait.IsRunning)
        {
            _flickerWait = _flickerWait.Stepped();
            if (_flickerWait.IsRunning || _flickerStep >= _flicker.Count)
                return;
            (int updates, bool shows) = _flicker[_flickerStep++];
            _flickerWait = Countdown.Of(updates);
            if (!shows)
                return;
        }

        Visual?.SetShown(true);
    }
}
