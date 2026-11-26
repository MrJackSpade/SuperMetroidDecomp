using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Beetom: a small leech. Far from Samus it idles, crawls and hops about by chance; near her it lunges. One that
/// reaches her clings on and saps her energy, riding with her until she has shaken it off by changing what she holds
/// down often enough, or has frozen it. She keeps full control all the while.
/// </summary>
/// <remarks>
/// Its timelines are "crawling_left" and "crawling_right", "hop_left" and "hop_right", and "draining_left" and
/// "draining_right". Its leaps follow the speed curve its data names.
/// </remarks>
public partial class Beetom : Enemy
{
    /// <summary>How near across Samus must be, in pixels, for it to lunge at her.</summary>
    [Export, DataValue("lunge_distance")]
    public int LungeDistance { get; set; } = 96;

    /// <summary>How many times Samus must change what she holds down to shake it off.</summary>
    [Export, DataValue("shake_off_count")]
    public int ShakeOffCount { get; set; } = 64;

    /// <summary>How many updates it idles for.</summary>
    [Export, DataValue("idle_updates")]
    public int IdleUpdates { get; set; } = 33;

    /// <summary>How many updates one spell of crawling lasts.</summary>
    [Export, DataValue("crawl_updates")]
    public int CrawlUpdates { get; set; } = 65;

    /// <summary>How far ahead of itself it feels for floor while crawling, in pixels.</summary>
    [Export, DataValue("ledge_reach")]
    public int LedgeReach { get; set; } = 8;

    /// <summary>How far it travels across each update of crawling, as a number of pixels.</summary>
    [Export, DataValue("crawl_speed")]
    public double CrawlSpeed { get; set; } = 0.25;

    /// <summary>How far it travels across each update of a hop.</summary>
    [Export, DataValue("hop_speed")]
    public double HopSpeed { get; set; } = 0.25;

    /// <summary>How far it travels across each update of a lunge.</summary>
    [Export, DataValue("lunge_speed")]
    public double LungeSpeed { get; set; } = 3;

    /// <summary>How high a short hop carries it, in pixels.</summary>
    [Export, DataValue("short_hop_height")]
    public int ShortHopHeight { get; set; } = 48;

    /// <summary>How high a long hop carries it, in pixels.</summary>
    [Export, DataValue("long_hop_height")]
    public int LongHopHeight { get; set; } = 64;

    /// <summary>How high a lunge carries it, in pixels.</summary>
    [Export, DataValue("lunge_height")]
    public int LungeHeight { get; set; } = 48;

    /// <summary>How many marks of the curve a short hop moves along each update, rising and falling.</summary>
    [Export, DataValue("short_hop_marks")]
    public int ShortHopMarks { get; set; } = 4;

    /// <summary>How many marks of the curve a long hop moves along each update, rising and falling.</summary>
    [Export, DataValue("long_hop_marks")]
    public int LongHopMarks { get; set; } = 5;

    /// <summary>How many marks of the curve its lunge is aimed with.</summary>
    [Export, DataValue("lunge_aim_marks")]
    public int LungeAimMarks { get; set; } = 3;

    /// <summary>How many marks of the curve a lunge moves along each update while rising.</summary>
    [Export, DataValue("lunge_rising_marks")]
    public int LungeRisingMarks { get; set; } = 5;

    /// <summary>How many marks of the curve a lunge moves along each update while falling.</summary>
    [Export, DataValue("lunge_falling_marks")]
    public int LungeFallingMarks { get; set; } = 3;

    /// <summary>How many marks of the curve it moves along each update once shaken off.</summary>
    [Export, DataValue("flung_marks")]
    public int FlungMarks { get; set; } = 1;

    /// <summary>The mark of the curve its falls speed up to and stay at.</summary>
    [Export, DataValue("fastest_mark")]
    public int FastestMark { get; set; } = 64;

    /// <summary>How far above Samus's middle it rides while it clings, in pixels.</summary>
    [Export, DataValue("cling_above")]
    public int ClingAbove { get; set; } = 4;

    /// <summary>How far across it is thrown the moment it is shaken off, in pixels.</summary>
    [Export, DataValue("thrown_distance")]
    public int ThrownDistance { get; set; } = 16;

    /// <summary>How far across it travels each update after being shaken off, as a number of pixels.</summary>
    [Export, DataValue("flung_speed")]
    public double FlungSpeed { get; set; } = 2;

    /// <summary>How far it falls each update when something has stopped a leap, as a number of pixels.</summary>
    [Export, DataValue("drop_speed")]
    public double DropSpeed { get; set; } = 3;

    /// <summary>How many updates pass between the bites that take Samus's energy.</summary>
    [Export, DataValue("bite_every")]
    public int BiteEvery { get; set; } = 64;

    /// <summary>How many updates pass between the sounds of it feeding.</summary>
    [Export, DataValue("feeding_sound_every")]
    public int FeedingSoundEvery { get; set; } = 8;

    /// <summary>The least energy Samus must have for its feeding to be heard.</summary>
    [Export, DataValue("feeding_sound_least_energy")]
    public int FeedingSoundLeastEnergy { get; set; } = 30;

    /// <summary>The sound of it feeding, by name.</summary>
    [Export, DataValue("feeding_sound")]
    public string FeedingSound { get; set; } = "";

    /// <summary>The data file of the speed curve its leaps follow.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    // The stages whose names begin with "Start" each take an update of their own, as the cartridge's do.
    private enum Doing
    {
        Deciding,
        ChoosingFarAway,
        StartIdling,
        StartCrawling,
        StartShortHop,
        StartLongHop,
        StartLunge,
        StartClinging,
        StartDropping,
        StartFlung,
        Idling,
        Crawling,
        ShortHopRising,
        ShortHopFalling,
        LongHopRising,
        LongHopFalling,
        LungeRising,
        LungeFalling,
        Clinging,
        Dropping,
        Flung,
    }

    private SpeedCurve? _curve;
    private Doing _doing;
    private Facing _heading;
    private Countdown _wait;
    private int _place;
    private int _shakesLeft;
    private bool _clings;
    private GameInput _lastInput = GameInput.None;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        Facing = Facing.Left;
        _shakesLeft = ShakeOffCount;
        _lastInput = Room.Player.Input;
        Random.Restart(23);
        Play(TowardPlayer == Facing.Left ? "crawling_left" : "crawling_right");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Deciding:
                _doing = Math.Abs(Target.X.Whole - Position.X.Whole) < LungeDistance ? Doing.StartLunge : Doing.ChoosingFarAway;
                return;

            case Doing.ChoosingFarAway:
                ChooseFarAway();
                return;

            case Doing.StartIdling:
                _wait = Countdown.Of(IdleUpdates);
                _doing = Doing.Idling;
                return;

            case Doing.StartCrawling:
                _doing = Doing.Crawling;
                Play(_heading == Facing.Left ? "crawling_left" : "crawling_right", restart: true);
                return;

            case Doing.StartShortHop:
                StartLeap(Doing.ShortHopRising, ShortHopHeight, ShortHopMarks);
                return;

            case Doing.StartLongHop:
                StartLeap(Doing.LongHopRising, LongHopHeight, LongHopMarks);
                return;

            case Doing.StartLunge:
                _heading = TowardPlayer;
                Facing = _heading;
                StartLeap(Doing.LungeRising, LungeHeight, LungeAimMarks);
                return;

            case Doing.StartClinging:
                Play(_heading == Facing.Left ? "draining_left" : "draining_right", restart: true);
                _doing = Doing.Clinging;
                return;

            case Doing.StartDropping:
                Play(Facing == Facing.Right ? "crawling_right" : "crawling_left", restart: true);
                _doing = Doing.Dropping;
                return;

            case Doing.StartFlung:
                _place = 0;
                _doing = Doing.Flung;
                return;

            case Doing.Idling:
                _wait = _wait.Stepped();
                if (_wait.IsOver)
                    _doing = Doing.Deciding;
                return;

            case Doing.Crawling:
                Crawl();
                return;

            case Doing.ShortHopRising:
                Rise(ShortHopMarks, Doing.ShortHopFalling);
                LeapAcross(HopSpeed);
                return;

            case Doing.ShortHopFalling:
                Fall(ShortHopMarks);
                LeapAcross(HopSpeed);
                return;

            case Doing.LongHopRising:
                Rise(LongHopMarks, Doing.LongHopFalling);
                LeapAcross(HopSpeed);
                return;

            case Doing.LongHopFalling:
                Fall(LongHopMarks);
                LeapAcross(HopSpeed);
                return;

            case Doing.LungeRising:
                Rise(LungeRisingMarks, Doing.LungeFalling);
                LeapAcross(LungeSpeed);
                return;

            case Doing.LungeFalling:
                Fall(LungeFallingMarks);
                LeapAcross(LungeSpeed);
                return;

            case Doing.Clinging:
                Cling();
                return;

            case Doing.Dropping:
                if (!MoveDownBy(Pixels.FromNumber(DropSpeed)))
                    return;
                _heading = Facing;
                _doing = Doing.StartCrawling;
                return;

            case Doing.Flung:
                if (MoveDownBy(Curve.SpeedAt(_place)))
                    _doing = Doing.Deciding;
                else
                    SpeedUpFall(FlungMarks);
                if (!MoveAcrossBy(Facing.Opposite().Toward(Pixels.FromNumber(FlungSpeed))))
                    return;
                Facing = Facing.Opposite();
                _doing = Doing.StartDropping;
                return;

            default:
                throw new InvalidOperationException($"Undefined Beetom stage {_doing}.");
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (Room.Player.IsInvincible)
            return;
        if (!_clings)
        {
            _heading = Facing;
            _doing = Doing.StartClinging;
            _clings = true;
            _shakesLeft = ShakeOffCount;

            // Once it has latched on it stands in front of her, and goes on standing so after it is flung off.
            Standing = Standing.InFront;
        }

        // A power that makes Samus's body a weapon meets it as it would any enemy.
        if (Room.Player.BodyPowers.Any(power => power.IsActive))
        {
            base.Touch(part);
            Room.Player.BeInvincibleFor(0);
            return;
        }

        if (Room.Session.EnemyPasses % FeedingSoundEvery == FeedingSoundEvery - 1
            && Room.Player.Inventory.Amount(Inventory.Energy) >= FeedingSoundLeastEnergy)
            Sound(FeedingSound);
        if (Updates % BiteEvery == BiteEvery - 1)
            Room.Player.Sap(new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Source = this });
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part) => Struck();

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part) => Struck();

    /// <inheritdoc/>
    protected override void OnStatusBegan(string status)
    {
        // Frozen while it clings, it lets go and drops.
        if (status == Frozen && _doing == Doing.Clinging)
            _doing = Doing.StartDropping;
    }

    /// <summary>Answers anything that lands on it: its next touch counts as taking hold afresh.</summary>
    private void Struck() => _clings = false;

    /// <summary>Picks what to do far from Samus, by one fresh random number: idle, or a short or long hop either way.</summary>
    private void ChooseFarAway()
    {
        RandomDraw draw = Random.Draw();
        switch (draw.Part(0, 3))
        {
            case 0 or 1:
                _doing = Doing.StartIdling;
                break;
            case 2 or 4:
                (_doing, _heading) = (Doing.StartShortHop, Facing.Left);
                break;
            case 5:
                (_doing, _heading) = (Doing.StartShortHop, Facing.Right);
                break;
            case 6:
                (_doing, _heading) = (Doing.StartLongHop, Facing.Left);
                break;
            default:
                (_doing, _heading) = (Doing.StartLongHop, Facing.Right);
                break;
        }

        Facing = draw.Part(0, 1) == 1 ? Facing.Right : Facing.Left;
    }

    /// <summary>Starts a leap the way it is heading: one that climbs <paramref name="height"/> pixels.</summary>
    private void StartLeap(Doing rising, int height, int marks)
    {
        _place = Curve.PlaceForClimb(Pixels.FromWhole(height), marks * SpeedCurve.PartsPerMark);
        _doing = rising;
        Play(_heading == Facing.Left ? "hop_left" : "hop_right", restart: true);
    }

    /// <summary>Crawls one update the way it is heading, turning round at a ledge or a wall.</summary>
    private void Crawl()
    {
        _wait = _wait.Stepped();
        if (_wait.IsOver)
        {
            _wait = Countdown.Of(CrawlUpdates);
            _doing = Doing.Deciding;
            return;
        }

        // It feels for floor a little ahead of itself: with none there, it turns back, a pixel higher than it was.
        PixelOffset reach = PixelOffset.FromWhole(_heading == Facing.Left ? -LedgeReach : LedgeReach, 0);
        Position += reach;
        bool floorAhead = MoveDownBy(Pixels.FromWhole(1));
        Position -= reach;
        if (!floorAhead)
        {
            Position -= PixelOffset.FromWhole(0, 1);
            _heading = _heading.Opposite();
            _doing = Doing.StartCrawling;
            return;
        }

        if (!MoveAcrossBy(_heading.Toward(Pixels.FromNumber(CrawlSpeed))))
            return;
        _heading = _heading.Opposite();
        _doing = Doing.StartCrawling;
    }

    /// <summary>Rises one update of a leap; a ceiling drops it, and the top of the leap turns it to falling.</summary>
    private void Rise(int marks, Doing falling)
    {
        if (MoveDownBy(-Curve.SpeedAt(_place)))
        {
            _doing = Doing.StartDropping;
            return;
        }

        _place -= marks * SpeedCurve.PartsPerMark;
        if (_place >= 0)
            return;
        _place = 0;
        _doing = falling;
    }

    /// <summary>Falls one update of a leap; the floor sets it deciding again.</summary>
    private void Fall(int marks)
    {
        if (MoveDownBy(Curve.SpeedAt(_place)))
        {
            _doing = Doing.Deciding;
            return;
        }

        SpeedUpFall(marks);
    }

    /// <summary>Moves a fall on along the curve, as far as its fastest mark.</summary>
    private void SpeedUpFall(int marks) =>
        _place = Math.Min(FastestMark * SpeedCurve.PartsPerMark, _place + marks * SpeedCurve.PartsPerMark);

    /// <summary>Moves across one update of a leap; a wall turns it round and drops it.</summary>
    private void LeapAcross(double speed)
    {
        if (!MoveAcrossBy(_heading.Toward(Pixels.FromNumber(speed))))
            return;
        Facing = Facing.Opposite();
        _doing = Doing.StartDropping;
    }

    /// <summary>Rides on Samus until she has shaken it off, and is then thrown clear.</summary>
    private void Cling()
    {
        if (_shakesLeft != 0)
        {
            Position = PixelPosition.FromWhole(Target.X.Whole, Target.Y.Whole - ClingAbove);
            GameInput input = Room.Player.Input;
            if (!input.HoldsSameAs(_lastInput))
            {
                _lastInput = input;
                _shakesLeft--;
            }

            return;
        }

        // Thrown against a wall, it comes back off it twice as far.
        if (MoveAcrossBy(_heading.Opposite().Toward(Pixels.FromWhole(ThrownDistance))))
        {
            Facing = _heading.Opposite();
            MoveAcrossBy(_heading.Toward(Pixels.FromWhole(2 * ThrownDistance)));
        }

        _clings = false;
        _doing = Doing.StartFlung;
    }

    /// <summary>The speed curve its leaps follow.</summary>
    private SpeedCurve Curve => _curve ?? throw new InvalidOperationException($"The Beetom \"{Name}\" is not in a room.");
}
