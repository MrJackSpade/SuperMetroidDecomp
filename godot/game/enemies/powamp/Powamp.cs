using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Powamp: a little creature under a balloon. It rests, puffs its balloon up, drifts up a way, wobbling, lets the
/// air out and sinks back. The grapple beam can latch onto it, and while it is held it climbs further and stays up.
/// When it dies its balloon swells and it bursts into eight spikes.
/// </summary>
/// <remarks>
/// Its timelines are "body_slow" and "body_fast", and for the visual named "Balloon" the timelines "deflated",
/// "inflating", "inflating_from_halfway" and "sinking", whose cues "pose:deflated", "pose:halfway" and "pose:full"
/// say how full the balloon is. Its data gives how far above the body the balloon sits in each pose under
/// "balloon_heights", and its wobble under "wiggle". Its shot is "spike".
/// </remarks>
public partial class Powamp : Enemy, ILatchable
{
    /// <summary>How far above where it rests it climbs while the grapple beam holds it, in pixels.</summary>
    [Export]
    public int GrappleTravelDistance { get; set; } = 64;

    /// <summary>Whether nothing can touch it: neither Samus nor her shots.</summary>
    [Export]
    public bool Untouchable { get; set; }

    /// <summary>How far above where it rests it climbs by itself, in pixels.</summary>
    [Export, DataValue("rise_distance")]
    public int RiseDistance { get; set; } = 64;

    /// <summary>How many updates it rests between climbs.</summary>
    [Export, DataValue("rest_updates")]
    public int RestUpdates { get; set; } = 60;

    /// <summary>How many updates its balloon takes to fill, and to empty.</summary>
    [Export, DataValue("inflation_updates")]
    public int InflationUpdates { get; set; } = 10;

    /// <summary>How many updates pass between a fatal hit and its bursting.</summary>
    [Export, DataValue("death_delay")]
    public int DeathDelay { get; set; } = 32;

    /// <summary>How many updates each step of its wobble lasts.</summary>
    [Export, DataValue("wiggle_step_updates")]
    public int WiggleStepUpdates { get; set; } = 5;

    /// <summary>How far it climbs each update, as a number of pixels.</summary>
    [Export, DataValue("rise_speed")]
    public double RiseSpeed { get; set; } = 0.5;

    /// <summary>How far it sinks each update, as a number of pixels.</summary>
    [Export, DataValue("sink_speed")]
    public double SinkSpeed { get; set; } = 1;

    /// <summary>The speed each spike gains each update along each way it flies, as a number of pixels.</summary>
    [Export, DataValue("spike_quickening")]
    public double SpikeQuickening { get; set; } = 0.125;

    /// <summary>
    /// The death it dies in once it has burst, by the data file of its timeline: the one named when its burst's last
    /// spike took the first of the room's places for what the enemies launch, or any place after the third, or none.
    /// </summary>
    [Export(PropertyHint.File), DataValue("burst_death")]
    public string BurstDeath { get; set; } = "";

    /// <summary>The death it dies in when its burst's last spike took the second of the room's places.</summary>
    [Export(PropertyHint.File), DataValue("burst_death_second_place")]
    public string BurstDeathSecondPlace { get; set; } = "";

    /// <summary>The death it dies in when its burst's last spike took the third of the room's places.</summary>
    [Export(PropertyHint.File), DataValue("burst_death_third_place")]
    public string BurstDeathThirdPlace { get; set; } = "";

    private enum Doing
    {
        Resting,
        Inflating,
        Rising,
        FinishingWiggle,
        HeldRising,
        HeldFinishingWiggle,
        HeldResting,
        Deflating,
        Sinking,
        FatallyHit,
        Dying,
    }

    /// <summary>
    /// How its moves up and down are tried, as the cartridge moves an enemy: only where each would end, and sloped
    /// ground that holds things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    private readonly Dictionary<string, int> _balloonHeights = new(StringComparer.Ordinal);
    private readonly List<int> _wiggle = [];
    private Doing _doing;
    private Countdown _wait;
    private Countdown _wiggleWait = Countdown.Of(1);
    private int _wiggleStep;
    private Pixels _downSpeed;
    private PixelPosition _rest;
    private TimelinePlayer? _balloon;
    private string _pose = "halfway";
    private bool _balloonSinking = true;
    private bool _latched;
    private bool _burstsAsItDies;

    /// <inheritdoc/>
    public bool CanBeLatched => !IsDead && !IsDying && !Untouchable;

    /// <inheritdoc/>
    public PixelPosition LatchPoint => Position;

    /// <inheritdoc/>
    public void Latched(PixelPosition at) => _latched = true;

    /// <inheritdoc/>
    public void Unlatched() => _latched = false;

    /// <summary>
    /// One untouchable, or on its way to bursting, lets every hit pass. It still counts as one that can be hit, as the
    /// cartridge's goes on heeding Samus then, so the time a hit left it unhittable for runs on.
    /// </summary>
    public override HitResult TakeHit(Hit hit, Collider part) =>
        Untouchable || IsDying ? HitResult.Ignored : base.TakeHit(hit, part);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _balloonHeights.Clear();
        foreach ((string pose, DataNode height) in data.Get("balloon_heights").Entries())
            _balloonHeights[pose] = height.AsInt();
        _wiggle.Clear();
        _wiggle.AddRange(data.List("wiggle").Select(offset => offset.AsInt()));
        if (_wiggle.Count == 0)
            throw data.Problem("must list at least one wiggle offset");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _rest = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _wait = Countdown.Of(RestUpdates);
        Play("body_slow");
        StartBalloon("deflated", sinking: true, pose: "halfway");
        ShowBalloon();
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        // Killed by Samus's own body, it dies there and then and bursts as it goes; anything else swells its balloon
        // first. Her shots name her as their source as her body does, so it is the hit's kinds that tell the two apart.
        if (hit.CarriesAny(Game.Data.HitKinds.OfHerBody))
        {
            _burstsAsItDies = true;
            return true;
        }

        _doing = Doing.FatallyHit;
        return false;
    }

    /// <summary>
    /// Killed by Samus's body, it bursts once its remains are made, so that its explosion takes its place among the
    /// room's places before its spikes do, as the cartridge's touch kills it before the spikes are thrown.
    /// </summary>
    protected override void OnDied()
    {
        if (_burstsAsItDies)
            Burst();
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!Untouchable && !IsDying)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Resting:
                if (Tick())
                {
                    StartBalloon("inflating", sinking: false, pose: "deflated");
                    _doing = Doing.Inflating;
                    _wait = Countdown.Of(InflationUpdates);
                }

                break;

            case Doing.Inflating:
                if (Tick())
                {
                    _doing = Doing.Rising;
                    _downSpeed = -Pixels.FromNumber(RiseSpeed);
                    Play("body_fast", restart: true);
                }

                break;

            case Doing.Rising:
                if (_latched)
                {
                    _doing = Doing.HeldRising;
                    Rise(held: true);
                }
                else
                {
                    Rise(held: false);
                }

                break;

            case Doing.HeldRising:
                if (!_latched)
                    _doing = Doing.FinishingWiggle;
                else
                    Rise(held: true);
                break;

            case Doing.FinishingWiggle:
                FinishWiggle(held: false);
                break;

            case Doing.HeldFinishingWiggle:
                if (!_latched)
                    _doing = Doing.FinishingWiggle;
                else
                    FinishWiggle(held: true);
                break;

            case Doing.HeldResting:
                if (!_latched)
                    StartDeflating();
                break;

            case Doing.Deflating:
                if (Tick())
                {
                    _doing = Doing.Sinking;
                    _downSpeed = Pixels.FromNumber(SinkSpeed);
                }

                break;

            case Doing.Sinking:
                Drop(_downSpeed);
                if (Position.Y.Whole >= _rest.Y.Whole)
                {
                    Position = new PixelPosition(Position.X, Position.Y.WithWhole(_rest.Y.Whole));
                    _doing = Doing.Resting;
                    _wait = Countdown.Of(RestUpdates);
                    Play("body_slow", restart: true);
                }

                break;

            case Doing.FatallyHit:
                // A balloon that was emptying swells again before the end.
                if (_balloonSinking && _pose == "halfway")
                    StartBalloon("inflating_from_halfway", sinking: false, pose: "deflated");
                else if (_balloonSinking && _pose == "deflated")
                    StartBalloon("inflating", sinking: false, pose: "deflated");
                _doing = Doing.Dying;
                _wait = Countdown.Of(DeathDelay);
                break;

            case Doing.Dying:
                if (Tick())
                {
                    // It dies in the explosion the place its burst's last spike took names, as the cartridge reads
                    // the number that spike's making left behind: the second and third places name two of their own,
                    // and every other place, or a last spike the room had no place for, the first.
                    Die(Burst() switch
                    {
                        1 => BurstDeathSecondPlace,
                        2 => BurstDeathThirdPlace,
                        _ => BurstDeath,
                    });
                    return;
                }

                break;

            default:
                throw new InvalidOperationException($"Undefined Powamp stage {_doing}.");
        }

        ShowBalloon();
    }

    /// <summary>Whether a fatal hit has landed and it is on its way to bursting.</summary>
    private bool IsDying => _doing is Doing.FatallyHit or Doing.Dying;

    /// <summary>Counts one update of its wait, and returns whether that ended it.</summary>
    private bool Tick()
    {
        if (_wait.IsOver)
            return false;
        _wait = _wait.Stepped();
        return _wait.IsOver;
    }

    /// <summary>Climbs one update, wobbling, until it is as high as it goes or something stops it.</summary>
    private void Rise(bool held)
    {
        Wiggle(stopWhenCentred: false);
        bool stopped = Drop(_downSpeed);
        bool highEnough = _rest.Y.Whole - (held ? GrappleTravelDistance : RiseDistance) >= Position.Y.Whole;
        if (!stopped && !highEnough)
            return;
        if (_wiggle[_wiggleStep] != 0)
            _doing = held ? Doing.HeldFinishingWiggle : Doing.FinishingWiggle;
        else if (held)
            _doing = Doing.HeldResting;
        else
            StartDeflating();
    }

    /// <summary>Carries on climbing until its wobble is back in the middle.</summary>
    private void FinishWiggle(bool held)
    {
        if (!Wiggle(stopWhenCentred: true))
            Drop(_downSpeed);
        else if (held)
            _doing = Doing.HeldResting;
        else
            StartDeflating();
    }

    /// <summary>
    /// Moves its wobble on when the step it is on has lasted long enough. Asked to stop when centred, it returns true
    /// once it has come to a step in the middle, and stays there.
    /// </summary>
    private bool Wiggle(bool stopWhenCentred)
    {
        if (_wiggleWait.IsOver)
            return false;
        _wiggleWait = _wiggleWait.Stepped();
        if (_wiggleWait.IsRunning)
            return false;
        _wiggleWait = Countdown.Of(WiggleStepUpdates);
        Position = new PixelPosition(Position.X.WithWhole(_rest.X.Whole + _wiggle[_wiggleStep]), Position.Y);
        if (stopWhenCentred && _wiggle[_wiggleStep] == 0)
            return true;
        _wiggleStep = (_wiggleStep + 1) % _wiggle.Count;
        return false;
    }

    /// <summary>Starts letting the air out of its balloon.</summary>
    private void StartDeflating()
    {
        _doing = Doing.Deflating;
        _wait = Countdown.Of(InflationUpdates);
        StartBalloon("sinking", sinking: true, pose: "full");
    }

    /// <summary>Starts the balloon on one of its timelines, in the pose it has until the timeline says otherwise.</summary>
    private void StartBalloon(string timeline, bool sinking, string pose)
    {
        _balloon = new TimelinePlayer(Timelines.Get(timeline));
        _balloonSinking = sinking;
        _pose = pose;
    }

    /// <summary>Moves the balloon on by one update and puts it above the body, as high as its pose says.</summary>
    private void ShowBalloon()
    {
        if (_balloon is null || GetNodeOrNull<ActorVisual>("Balloon") is not { } visual)
            return;
        foreach (string cue in _balloon.Step())
        {
            if (cue.StartsWith("pose:", StringComparison.Ordinal))
                _pose = cue["pose:".Length..];
        }

        if (!_balloonHeights.TryGetValue(_pose, out int height))
            throw new InvalidOperationException($"The Powamp \"{Name}\" has no balloon height for the pose \"{_pose}\".");
        visual.Position = new Vector2(0, height);
        visual.Show(_balloon.Shown);
        if (GetNodeOrNull<Collider>("BalloonHurtbox") is { } hurtbox)
            hurtbox.Rect = new PixelRect(-hurtbox.Size.X / 2, height - hurtbox.Size.Y / 2, hurtbox.Size.X, hurtbox.Size.Y);
    }

    /// <summary>Moves it down by <paramref name="distance"/>, upward when less than nothing; returns whether something stopped it.</summary>
    private bool Drop(Pixels distance) => Room.Mover.MoveDown(MovingBody(), distance, Moves).Blocked;

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Powamp \"{Name}\" has no body collider.");

    /// <summary>
    /// Throws its eight spikes out, one each way, and returns the place the last of them took among the room's places
    /// for what the enemies launch, counted from none; null when the room had no place for it.
    /// </summary>
    private int? Burst()
    {
        GetNodeOrNull<ActorVisual>("Balloon")?.SetShown(false);
        Pixels push = Pixels.FromNumber(SpikeQuickening);
        (int Across, int Down)[] ways = [(-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1)];
        PixelPosition from = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        int? last = null;
        foreach ((int across, int down) in ways)
            last = Room.PlaceOf(Fire("spike", from, new PixelVelocity(push * across, push * down)));
        return last;
    }
}
