using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Covern: a ghost of the Wrecked Ship. Unseen, it watches how Samus moves; when she has stood still for a while,
/// or kept one course for a moment, it appears ahead of her, flickering in, bobs there for a time, able to hurt and
/// be hurt, and fades away again.
/// </summary>
/// <remarks>
/// Its timeline is "floating". Its data lists its flicker under "flicker": each step's length in updates, and whether
/// the step leaves it shown. Its colours are changes of its pictures' colours: "dark", as it starts, and one for each
/// update's step of its "brightening" from the dark to white, its "materialising" from white into its own colours and
/// its "vanishing" to white; each ends on the update after its last step.
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

    /// <summary>The name its colours are shown under.</summary>
    private const string FadeColours = "CovernFade";

    private readonly List<(int Updates, bool Shows)> _flicker = [];
    private ColourChange _dark = ColourChange.None;
    private ColourChange[] _brightening = [];
    private ColourChange[] _materialising = [];
    private ColourChange[] _vanishing = [];
    private Doing _doing;
    private Countdown _wait;
    private Countdown _still;
    private Countdown _steady;
    private Countdown _flickerWait;
    private int _flickerStep;
    private ColourChange[] _fade = [];
    private int _fadeStep;
    private int _appearedAtY;
    private (int X, int Y) _lastSamus;
    private (int X, int Y) _course;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => _doing == Doing.Bobbing && base.IsHitBy(side);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _flicker.Clear();
        _flicker.AddRange(data.List("flicker").Select(step => (step.Int("updates"), step.Flag("shows"))));
        _dark = ColourChange.Read(data.Get("dark"));
        _brightening = [.. data.List("brightening").Select(ColourChange.Read)];
        _materialising = [.. data.List("materialising").Select(ColourChange.Read)];
        _vanishing = [.. data.List("vanishing").Select(ColourChange.Read)];
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("floating");
        Visual?.SetShown(false);
        _wait = Countdown.Of(ReappearanceDelay + FirstExtraDelay);
        // It starts dark, and its first appearance brightens it from there.
        BeginFade(_brightening);
        if (Visual is { } visual)
            _dark.ShowOn(visual, FadeColours);
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
                // It brightens from the dark to white once; already white, as it is after vanishing, it goes
                // straight on to its own colours.
                Flicker();
                if (StepFade(_brightening))
                    return;
                _doing = Doing.Materialising;
                BeginFade(_materialising);
                return;

            case Doing.Materialising:
                bool settling = StepFade(_materialising);
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
                BeginFade(_vanishing);
                return;

            case Doing.Vanishing:
                if (StepFade(_vanishing))
                    return;
                _doing = Doing.Away;
                Visual?.SetShown(false);
                _wait = Countdown.Of(ReappearanceDelay);
                return;

            default:
                throw new InvalidOperationException($"Undefined Covern stage {_doing}.");
        }
    }

    /// <summary>Starts its colours on the steps of <paramref name="fade"/>.</summary>
    private void BeginFade(ColourChange[] fade) => (_fade, _fadeStep) = (fade, 0);

    /// <summary>
    /// Shows its colours a step on along <paramref name="fade"/>, if they are on that fade's steps and it has one left,
    /// and returns whether they changed: a fade ends on the update they do not.
    /// </summary>
    private bool StepFade(ColourChange[] fade)
    {
        if (!ReferenceEquals(_fade, fade) || _fadeStep >= fade.Length)
            return false;
        ColourChange colours = fade[_fadeStep++];
        if (Visual is { } visual)
            colours.ShowOn(visual, FadeColours);
        return true;
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
