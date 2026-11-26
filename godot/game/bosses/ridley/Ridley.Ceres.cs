using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Ridley's fight aboard Ceres station, the baby Metroid's capsule in its claws. It cannot be killed here: it hovers, then
/// breathes fire, lunges at Samus or swoops across the room, and hovers again. After enough hits it makes as if to
/// leave, drops the capsule, snatches it back up and flies off; if Samus's energy runs low it simply goes. Once it is
/// gone the station's self-destruct begins, which is for others to run: it tells them so.
/// </summary>
/// <remarks>
/// As well as what both fights share it plays "ceres_lunge_left", "ceres_lunge_right", "retrieve_baby_left",
/// "retrieve_baby_right", "fireballing_end_left" and "fireballing_end_right". Its visual "Baby" is the capsule,
/// the first of its visuals and so drawn behind all the rest of it: while it is unseen the capsule shows alone, and
/// once it has faded in it holds the capsule in front of itself no more than its claws do. The capsule
/// plays "baby_idle" and "baby_expressive"; their cue "reacts" livens it when it is falling, and "cries" has it cry
/// or, as often as not when it is still, settle. Its data's "retreat_colours" is the change of colours its going
/// gives the chamber's own pictures, which stays for as long as the room is there; under its "colours", "own" is
/// the change of its own pictures' colours it shows once it has shown itself. Its "health_bands" each give the
/// fewest "hits" in a band and the "colours" it shows from then on, while it fights, in place of its own.
/// </remarks>
public partial class Ridley
{
    /// <summary>The things told once it has flown off, so that the station's last moments may begin.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenGone { get; set; } = [];

    /// <summary>What they are told.</summary>
    [Export]
    public string GoneMessage { get; set; } = "start";

    /// <summary>How many hits drive it off.</summary>
    [Export, DataValue("hits_to_drive_off")]
    public int HitsToDriveOff { get; set; } = 100;

    /// <summary>Below this energy of Samus's it stops fighting and goes.</summary>
    [Export, DataValue("samus_low_energy")]
    public int SamusLowEnergy { get; set; } = 30;

    /// <summary>How many updates its opening roar holds it on the ground.</summary>
    [Export, DataValue("roar_hold_updates")]
    public int RoarHoldUpdates { get; set; } = 253;

    /// <summary>How many updates each hit makes it flash.</summary>
    [Export, DataValue("hit_flash_updates")]
    public int HitFlashUpdates { get; set; } = 13;

    /// <summary>The colours its going gives the chamber.</summary>
    private ColourChange _retreatColours = ColourChange.None;

    /// <summary>Its own colours, once it has shown itself.</summary>
    private ColourChange _ownColours = ColourChange.None;

    /// <summary>The bands of hits taken that show a look of its health, most hits first.</summary>
    private readonly List<CeresBand> _ceresBands = [];

    /// <summary>The colours of the band of hits it shows, or none before the first.</summary>
    private ColourChange? _ceresBandColours;

    private sealed record CeresBand(int Hits, ColourChange Colours);

    /// <summary>The capsule's cry, by name.</summary>
    [Export, DataValue("baby_cry_sound")]
    public string BabyCrySound { get; set; } = "";

    private enum CeresPhase
    {
        LiftoffAccelerating,
        LiftoffDecelerating,
        Hovering,
        FireballMove,
        FireballShooting,
        LungeSetup,
        Lunge,
        SwoopSetup,
        SwoopMove,
        SwoopDive,
        SwoopSweep,
        SwoopClimb,
        SwoopFinalClimb,
        FakeRetreatMove,
        FakeRetreatRising,
        WaitBeforeRetrieving,
        RetrieveBaby,
        Retreat,
        RetreatDelay,
        Gone,
    }

    private enum Baby
    {
        InArms,
        InFeet,
        Dropping,
        Dropped,
        Lying,
    }

    private static readonly PixelPosition HoverPoint = PixelPosition.FromWhole(192, 100);
    private static readonly PixelPosition RetreatPoint = PixelPosition.FromWhole(192, -128);

    private CeresPhase _ceresPhase;
    private bool _fighting;
    private int _ceresTimer;
    private int _hoverWait;
    private int _fireballWait;
    private int _roarHold;
    private int _hits;
    private PixelPosition _fireballBase;
    private Baby _baby;
    private PixelPosition _babyPosition;
    private Pixels _babyFall;
    private TimelinePlayer? _babyTimeline;

    /// <summary>What a hit does to it aboard Ceres.</summary>
    private HitResult CeresTakeHit(Hit hit, Collider part)
    {
        if (part.Name == "TailTip" || part.Name == "TailJoint")
            return base.TakeHit(hit, part);
        // Nothing harms it here; each hit only counts toward driving it off.
        FlashFor(HitFlashUpdates);
        _hits++;
        // The look of its health shows at once.
        ShowCeresBand();
        ShowColours();
        return HitResult.Hurt;
    }

    /// <summary>Reads the bands of hits taken that show a look of its health.</summary>
    private void ReadCeresBands(DataNode data)
    {
        _ceresBands.Clear();
        _ceresBands.AddRange(data.List("health_bands").Select(band => new CeresBand(band.Int("hits"), ColourChange.Read(band.Get("colours")))).OrderByDescending(band => band.Hits));
        if (_ceresBands.Count == 0)
            throw data.Problem("must list the bands of hits taken that show a look of its health");
    }

    /// <summary>While it fights, takes the colours of the band its hits have reached; before the first its own stay.</summary>
    private void ShowCeresBand()
    {
        if (!_fighting)
            return;
        if (_ceresBands.FirstOrDefault(band => _hits >= band.Hits) is { } reached)
            _ceresBandColours = reached.Colours;
    }

    /// <summary>Sets it up for its fight aboard Ceres.</summary>
    private void EnterCeres()
    {
        Position = PixelPosition.FromWhole(186, 169);
        Facing = Facing.Left;
        PlayFacing("standing");
        Tail.WhipsWhenIdle = true;
        _baby = Baby.InArms;
        _babyPosition = Position + PixelOffset.FromWhole(-16, 22);
        _babyTimeline = new TimelinePlayer(Timelines.Get("baby_idle"));
        Room.Session.Sounds.RequestMusic("");
    }

    /// <summary>Aboard Ceres its fireballing may be cut short: the timeline to play instead, or none.</summary>
    private string? CeresCutFireballingShort()
    {
        // With Samus nearly spent it stops breathing fire.
        if (Room.Player.Inventory.Amount(Inventory.Energy) >= SamusLowEnergy)
            return null;
        _ceresTimer = 9;
        return $"fireballing_end_{Side}";
    }

    /// <summary>Aboard Ceres, flying hard into the left wall shakes the room.</summary>
    private void ShakeOnHittingWall(PixelPosition proposed)
    {
        // Flying hard into the left wall shakes the room.
        Pixels fastest = Velocity.X.Abs() > Velocity.Y.Abs() ? Velocity.X.Abs() : Velocity.Y.Abs();
        if (proposed.X.Whole < FlightBounds.Position.X && fastest >= Pixels.FromNumber(2.5))
            Room.Shake(3, 12);
    }

    /// <summary>One update of it aboard Ceres.</summary>
    private void BehaveOnCeres()
    {
        Player samus = Room.Player;
        if (!HasLiftedOff)
            StepReveal();
        else
            StepCeresFight(samus);
        if (_ceresPhase == CeresPhase.Gone)
            return;
        if (IsInMotion)
        {
            StepBody();
            // Now and then its tail curls tighter or looser: when the upper half of the number last drawn is as
            // great as it can be, by as much as its lowest digits say.
            RandomDraw chance = Random.Last;
            if (chance.Part(8, 8) == 255)
                Tail.LinkSpacing = 8 + chance.Part(0, 4);
            ShowCeresBand();
        }

        StepBaby();
    }

    /// <summary>One update of its fight.</summary>
    private void StepCeresFight(Player samus)
    {
        int y = Position.Y.Whole;
        switch (_ceresPhase)
        {
            case CeresPhase.LiftoffAccelerating:
                Velocity = Velocity.WithY(Velocity.Y - Pixels.FromNumber(0.0625));
                if (y >= 112)
                    break;
                _ceresPhase = CeresPhase.LiftoffDecelerating;
                goto case CeresPhase.LiftoffDecelerating;

            case CeresPhase.LiftoffDecelerating:
                Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(0.078125));
                if (y >= 80)
                    break;
                BeginHover();
                _fighting = true;
                break;

            case CeresPhase.Hovering:
                if (_hits >= HitsToDriveOff)
                {
                    _fighting = false;
                    _ceresPhase = CeresPhase.FakeRetreatMove;
                    goto case CeresPhase.FakeRetreatMove;
                }

                if (samus.Inventory.Amount(Inventory.Energy) < SamusLowEnergy)
                {
                    _fighting = false;
                    BeginRetreat();
                    goto case CeresPhase.Retreat;
                }

                FlyPlainlyToward(HoverPoint, 16);
                if (!IsNear(HoverPoint, 8, 8) && --_hoverWait > 0)
                    break;
                // Of sixteen chances six are to breathe fire, five to lunge and five to swoop.
                int roll = Random.Last.Part(0, 4);
                _ceresPhase = roll is >= 4 and < 9 ? CeresPhase.LungeSetup : roll is >= 10 and < 15 ? CeresPhase.SwoopSetup : CeresPhase.FireballMove;
                _fireballWait = 48;
                break;

            case CeresPhase.FireballMove:
                Pixels least = Pixels.FromNumber(0.5);
                Velocity = Velocity.WithY(Velocity.Y.IsNegative
                    ? (Velocity.Y < -least ? Velocity.Y : -least)
                    : (Velocity.Y > least ? Velocity.Y : least));
                FlyPlainlyToward(PixelPosition.FromWhole(Position.X.Whole, 88), 16);
                if (y < 80)
                {
                    if (--_fireballWait <= 0)
                        _ceresPhase = CeresPhase.SwoopSetup;
                    break;
                }

                if (y >= 128)
                    break;
                _fireballBase = PixelPosition.FromWhole(Position.X.Whole, y);
                PlayFacing("fireballing");
                (_ceresPhase, _ceresTimer) = (CeresPhase.FireballShooting, 225);
                goto case CeresPhase.FireballShooting;

            case CeresPhase.FireballShooting:
                // It wavers about where it began to breathe fire.
                RandomDraw draw = Random.Last;
                int waver = draw.Part(15, 1) == 1 ? -draw.Part(0, 3) : draw.Part(0, 3);
                FlyPlainlyToward(_fireballBase + PixelOffset.FromWhole(waver, waver + (waver < 0 ? 1 : 0)), 16);
                if (--_ceresTimer <= 0)
                    BeginHover();
                break;

            case CeresPhase.LungeSetup:
                PlayFacing("ceres_lunge");
                (_ceresPhase, _ceresTimer) = (CeresPhase.Lunge, 65);
                goto case CeresPhase.Lunge;

            case CeresPhase.Lunge:
                PixelPosition over = samus.Position.Y.Whole < 132
                    ? PixelPosition.FromWhole(samus.Position.X.Whole, 64)
                    : PixelPosition.FromWhole(samus.Position.X.Whole, samus.Position.Y.Whole - 68);
                FlyPlainlyToward(over, 3);
                bool reached = IsNear(over, 2, 2);
                if (--_ceresTimer <= 0 || reached)
                    BeginHover();
                break;

            case CeresPhase.SwoopSetup:
                (_ceresPhase, SwoopHeading) = (CeresPhase.SwoopMove, 0);
                Tail.WhipsWhenIdle = false;
                goto case CeresPhase.SwoopMove;

            case CeresPhase.SwoopMove:
                FlyPlainlyToward(PixelPosition.FromWhole(192, 80), 15);
                if (Position.Y.Whole < 96)
                    (_ceresPhase, _ceresTimer, SwoopHeading) = (CeresPhase.SwoopDive, 11, 0);
                break;

            case CeresPhase.SwoopDive:
                CeresSwoop(32, -4 * 256, CeresPhase.SwoopSweep, 37);
                break;

            case CeresPhase.SwoopSweep:
                CeresSwoop(512, -64 * 256, CeresPhase.SwoopClimb, 29);
                if (_ceresPhase == CeresPhase.SwoopClimb)
                    Tail.WhipOrdered = true;
                break;

            case CeresPhase.SwoopClimb:
                CeresSwoop(512, -120 * 256, CeresPhase.SwoopFinalClimb, 2);
                break;

            case CeresPhase.SwoopFinalClimb:
                SetSwoopSpeed(768, true, -120 * 256, Pixels.FromWhole(3));
                if (--_ceresTimer > 0)
                    break;
                BeginHover();
                Tail.WhipsWhenIdle = true;
                break;

            case CeresPhase.FakeRetreatMove:
                FlyPlainlyToward(PixelPosition.FromWhole(192, 128), 15);
                if (Position.X.Whole >= 192)
                    _ceresPhase = CeresPhase.FakeRetreatRising;
                break;

            case CeresPhase.FakeRetreatRising:
                RaiseCeiling();
                FlyPlainlyToward(RetreatPoint, 15);
                if (Position.Y.Whole >= 32)
                    break;
                _baby = Baby.Dropping;
                (_ceresPhase, _ceresTimer) = (CeresPhase.WaitBeforeRetrieving, 22);
                break;

            case CeresPhase.WaitBeforeRetrieving:
                if (--_ceresTimer > 0)
                    break;
                PlayFacing("retrieve_baby");
                _ceresPhase = CeresPhase.RetrieveBaby;
                goto case CeresPhase.RetrieveBaby;

            case CeresPhase.RetrieveBaby:
                FlyPlainlyToward(_babyPosition - PixelOffset.FromWhole(10, 56), 4);
                PixelOffset fromFeet = _babyPosition - (Position + PixelOffset.FromWhole(14, 66));
                if (Math.Abs(fromFeet.X.Whole) >= 8 || Math.Abs(fromFeet.Y.Whole) >= 8)
                    break;
                _baby = Baby.InFeet;
                Velocity = Velocity.WithY(Pixels.FromWhole(-2));
                BeginRetreat();
                break;

            case CeresPhase.Retreat:
                FlyPlainlyToward(RetreatPoint, 15);
                if (Position.Y.Whole >= -128)
                    break;
                (_ceresPhase, _ceresTimer) = (CeresPhase.RetreatDelay, 65);
                goto case CeresPhase.RetreatDelay;

            case CeresPhase.RetreatDelay:
                if (_ceresTimer > 0 && --_ceresTimer > 0)
                    break;
                Velocity = PixelVelocity.Zero;
                Tail.Action = RidleyTail.Doing.None;
                _ceresPhase = CeresPhase.Gone;

                // The chamber takes its colours on this update; its own it keeps.
                Room.Recolour("RidleyRetreat", _retreatColours);
                foreach (NodePath path in ToldWhenGone)
                    (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"Ridley is to tell \"{path}\" that it has gone, but that is nothing that can be told.")).Triggered(GoneMessage);
                Vanish();
                break;

            case CeresPhase.Gone:
                break;

            default:
                throw new InvalidOperationException($"Ridley on Ceres has no phase {_ceresPhase}.");
        }
    }

    /// <summary>One update of a leg of its swoop, turning leftward toward a heading. Moves on to the next leg when its time is up.</summary>
    private void CeresSwoop(int turn, int heading, CeresPhase next, int nextUpdates)
    {
        SetSwoopSpeed(turn, true, heading, Pixels.FromWhole(3));
        if (--_ceresTimer <= 0)
            (_ceresPhase, _ceresTimer) = (next, nextUpdates);
    }

    private void BeginHover() => (_ceresPhase, _hoverWait) = (CeresPhase.Hovering, 124);

    private void BeginRetreat()
    {
        _ceresPhase = CeresPhase.Retreat;
        RaiseCeiling();
    }

    /// <summary>Lets it fly up out of the room.</summary>
    private void RaiseCeiling()
    {
        Rect2I bounds = FlightBounds;
        FlightBounds = new Rect2I(bounds.Position.X, -192, bounds.Size.X, bounds.End.Y + 192);
    }

    /// <summary>Moves the capsule: in its arms or its feet, or falling to the floor.</summary>
    private void StepBaby()
    {
        switch (_baby)
        {
            case Baby.InArms:
                _babyPosition = Position + PixelOffset.FromWhole(-16, 22);
                break;

            case Baby.InFeet:
                _babyPosition = Position + PixelOffset.FromWhole(14, 66);
                break;

            case Baby.Dropping:
                _babyPosition = PixelPosition.FromWhole(_babyPosition.X.Whole, _babyPosition.Y.Whole);
                _babyFall = Pixels.Zero;
                _baby = Baby.Dropped;
                goto case Baby.Dropped;

            case Baby.Dropped:
                _babyFall += Pixels.FromNumber(0.03125);
                _babyPosition += new PixelOffset(Pixels.Zero, _babyFall);
                if (_babyPosition.Y.Whole < 192)
                    break;
                _babyPosition = PixelPosition.FromWhole(_babyPosition.X.Whole, 192);
                _babyFall = Pixels.Zero;
                _baby = Baby.Lying;
                break;

            case Baby.Lying:
                break;

            default:
                throw new InvalidOperationException($"The capsule has nothing to do called {_baby}.");
        }

        if (_babyTimeline is not { } timeline)
            return;
        foreach (string cue in timeline.Step())
        {
            if (cue == "reacts" && !_babyFall.IsZero)
            {
                _babyTimeline = new TimelinePlayer(Timelines.Get("baby_expressive"));
                break;
            }

            if (cue != "cries")
                continue;
            if (_babyFall.IsZero && Random.Last.Part(0, 1) == 1)
            {
                _babyTimeline = new TimelinePlayer(Timelines.Get("baby_idle"));
                break;
            }

            Sound(BabyCrySound);
        }

        if (GetNodeOrNull<ActorVisual>("Baby") is not { } visual)
            return;
        PixelOffset from = _babyPosition - Position;
        visual.Position = new Vector2(from.X.Whole, from.Y.Whole);
        visual.Show(_babyTimeline.Shown);
    }
}
