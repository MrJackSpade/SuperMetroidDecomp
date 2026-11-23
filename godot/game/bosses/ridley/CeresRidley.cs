using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Ridley aboard Ceres station, the baby Metroid's capsule in its claws. It cannot be killed here: it hovers, then
/// breathes fire, lunges at Samus or swoops across the room, and hovers again. After enough hits it makes as if to
/// leave, drops the capsule, snatches it back up and flies off; if Samus's energy runs low it simply goes. Once it is
/// gone the station's self-destruct begins, which is for others to run: it tells them so.
/// </summary>
/// <remarks>
/// As well as what both fights share it plays "ceres_lunge_left", "ceres_lunge_right", "retrieve_baby_left",
/// "retrieve_baby_right", "fireballing_end_left" and "fireballing_end_right". Its visual "Baby" is the capsule, which
/// plays "baby_idle" and "baby_expressive"; their cue "reacts" livens it when it is falling, and "cries" has it cry
/// or, as often as not when it is still, settle.
/// </remarks>
public partial class CeresRidley : RidleyBody
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

    /// <summary>The capsule's cry, by name.</summary>
    [Export, DataValue("baby_cry_sound")]
    public string BabyCrySound { get; set; } = "";

    private enum Phase
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

    private Phase _phase;
    private bool _fighting;
    private int _timer;
    private int _hoverWait;
    private int _fireballWait;
    private int _roarHold;
    private int _hits;
    private PixelPosition _fireballBase;
    private Baby _baby;
    private PixelPosition _babyPosition;
    private Pixels _babyFall;
    private TimelinePlayer? _babyTimeline;

    /// <inheritdoc/>
    protected override bool TailMayWhip => _fighting;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (part.Name == "TailTip" || part.Name == "TailJoint")
            return base.TakeHit(hit, part);
        // Nothing harms it here; each hit only counts toward driving it off.
        FlashFor(HitFlashUpdates);
        _hits++;
        return HitResult.Hurt;
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        base.OnEnterRoom();
        Position = PixelPosition.FromWhole(186, 169);
        Facing = Facing.Left;
        PlayFacing("standing");
        Tail.WhipsWhenIdle = true;
        _baby = Baby.InArms;
        _babyPosition = Position + PixelOffset.FromWhole(-16, 22);
        _babyTimeline = new TimelinePlayer(Timelines.Get("baby_idle"));
        Room.Session.Sounds.RequestMusic("");
    }

    /// <inheritdoc/>
    protected override void OnOpeningRoar() => _roarHold = RoarHoldUpdates;

    /// <inheritdoc/>
    protected override bool MayTakeOff() => --_roarHold <= 0;

    /// <inheritdoc/>
    protected override void OnLiftOff() => _phase = Phase.LiftoffAccelerating;

    /// <inheritdoc/>
    protected override string? CutFireballingShort()
    {
        // With Samus nearly spent it stops breathing fire.
        if (Room.Player.Inventory.Amount(Inventory.Energy) >= SamusLowEnergy)
            return null;
        _timer = 9;
        return $"fireballing_end_{Side}";
    }

    /// <inheritdoc/>
    protected override void OnMoving(PixelPosition proposed)
    {
        // Flying hard into the left wall shakes the room.
        Pixels fastest = Velocity.X.Abs() > Velocity.Y.Abs() ? Velocity.X.Abs() : Velocity.Y.Abs();
        if (proposed.X.Whole < FlightBounds.Position.X && fastest >= Pixels.FromNumber(2.5))
            Room.Shake(3, 12);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        if (!HasLiftedOff)
            StepReveal();
        else
            StepFight(samus);
        if (_phase == Phase.Gone)
            return;
        if (IsInMotion)
        {
            StepBody();
            // Now and then its tail curls tighter or looser.
            RandomDraw chance = Random.Last;
            if (chance.Part(0, 8) == 255)
                Tail.LinkSpacing = 8 + chance.Part(8, 4);
        }

        StepBaby();
    }

    /// <summary>One update of its fight.</summary>
    private void StepFight(Player samus)
    {
        int y = Position.Y.Whole;
        switch (_phase)
        {
            case Phase.LiftoffAccelerating:
                Velocity = Velocity.WithY(Velocity.Y - Pixels.FromNumber(0.0625));
                if (y >= 112)
                    break;
                _phase = Phase.LiftoffDecelerating;
                goto case Phase.LiftoffDecelerating;

            case Phase.LiftoffDecelerating:
                Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(0.078125));
                if (y >= 80)
                    break;
                BeginHover();
                _fighting = true;
                break;

            case Phase.Hovering:
                if (_hits >= HitsToDriveOff)
                {
                    _fighting = false;
                    _phase = Phase.FakeRetreatMove;
                    goto case Phase.FakeRetreatMove;
                }

                if (samus.Inventory.Amount(Inventory.Energy) < SamusLowEnergy)
                {
                    _fighting = false;
                    BeginRetreat();
                    goto case Phase.Retreat;
                }

                FlyPlainlyToward(HoverPoint, 16);
                if (!IsNear(HoverPoint, 8, 8) && --_hoverWait > 0)
                    break;
                // Of sixteen chances six are to breathe fire, five to lunge and five to swoop.
                int roll = Random.Last.Part(0, 4);
                _phase = roll is >= 4 and < 9 ? Phase.LungeSetup : roll is >= 10 and < 15 ? Phase.SwoopSetup : Phase.FireballMove;
                _fireballWait = 48;
                break;

            case Phase.FireballMove:
                Pixels least = Pixels.FromNumber(0.5);
                Velocity = Velocity.WithY(Velocity.Y.IsNegative
                    ? (Velocity.Y < -least ? Velocity.Y : -least)
                    : (Velocity.Y > least ? Velocity.Y : least));
                FlyPlainlyToward(PixelPosition.FromWhole(Position.X.Whole, 88), 16);
                if (y < 80)
                {
                    if (--_fireballWait <= 0)
                        _phase = Phase.SwoopSetup;
                    break;
                }

                if (y >= 128)
                    break;
                _fireballBase = PixelPosition.FromWhole(Position.X.Whole, y);
                PlayFacing("fireballing");
                (_phase, _timer) = (Phase.FireballShooting, 225);
                goto case Phase.FireballShooting;

            case Phase.FireballShooting:
                // It wavers about where it began to breathe fire.
                RandomDraw draw = Random.Last;
                int waver = draw.Part(15, 1) == 1 ? -draw.Part(0, 3) : draw.Part(0, 3);
                FlyPlainlyToward(_fireballBase + PixelOffset.FromWhole(waver, waver + (waver < 0 ? 1 : 0)), 16);
                if (--_timer <= 0)
                    BeginHover();
                break;

            case Phase.LungeSetup:
                PlayFacing("ceres_lunge");
                (_phase, _timer) = (Phase.Lunge, 65);
                goto case Phase.Lunge;

            case Phase.Lunge:
                PixelPosition over = samus.Position.Y.Whole < 132
                    ? PixelPosition.FromWhole(samus.Position.X.Whole, 64)
                    : PixelPosition.FromWhole(samus.Position.X.Whole, samus.Position.Y.Whole - 68);
                FlyPlainlyToward(over, 3);
                bool reached = IsNear(over, 2, 2);
                if (--_timer <= 0 || reached)
                    BeginHover();
                break;

            case Phase.SwoopSetup:
                (_phase, SwoopHeading) = (Phase.SwoopMove, 0);
                Tail.WhipsWhenIdle = false;
                goto case Phase.SwoopMove;

            case Phase.SwoopMove:
                FlyPlainlyToward(PixelPosition.FromWhole(192, 80), 15);
                if (Position.Y.Whole < 96)
                    (_phase, _timer, SwoopHeading) = (Phase.SwoopDive, 11, 0);
                break;

            case Phase.SwoopDive:
                Swoop(32, -4 * 256, Phase.SwoopSweep, 37);
                break;

            case Phase.SwoopSweep:
                Swoop(512, -64 * 256, Phase.SwoopClimb, 29);
                if (_phase == Phase.SwoopClimb)
                    Tail.WhipOrdered = true;
                break;

            case Phase.SwoopClimb:
                Swoop(512, -120 * 256, Phase.SwoopFinalClimb, 2);
                break;

            case Phase.SwoopFinalClimb:
                SetSwoopSpeed(768, true, -120 * 256, Pixels.FromWhole(3));
                if (--_timer > 0)
                    break;
                BeginHover();
                Tail.WhipsWhenIdle = true;
                break;

            case Phase.FakeRetreatMove:
                FlyPlainlyToward(PixelPosition.FromWhole(192, 128), 15);
                if (Position.X.Whole >= 192)
                    _phase = Phase.FakeRetreatRising;
                break;

            case Phase.FakeRetreatRising:
                RaiseCeiling();
                FlyPlainlyToward(RetreatPoint, 15);
                if (Position.Y.Whole >= 32)
                    break;
                _baby = Baby.Dropping;
                (_phase, _timer) = (Phase.WaitBeforeRetrieving, 22);
                break;

            case Phase.WaitBeforeRetrieving:
                if (--_timer > 0)
                    break;
                PlayFacing("retrieve_baby");
                _phase = Phase.RetrieveBaby;
                goto case Phase.RetrieveBaby;

            case Phase.RetrieveBaby:
                FlyPlainlyToward(_babyPosition - PixelOffset.FromWhole(10, 56), 4);
                PixelOffset fromFeet = _babyPosition - (Position + PixelOffset.FromWhole(14, 66));
                if (Math.Abs(fromFeet.X.Whole) >= 8 || Math.Abs(fromFeet.Y.Whole) >= 8)
                    break;
                _baby = Baby.InFeet;
                Velocity = Velocity.WithY(Pixels.FromWhole(-2));
                BeginRetreat();
                break;

            case Phase.Retreat:
                FlyPlainlyToward(RetreatPoint, 15);
                if (Position.Y.Whole >= -128)
                    break;
                (_phase, _timer) = (Phase.RetreatDelay, 65);
                goto case Phase.RetreatDelay;

            case Phase.RetreatDelay:
                if (_timer > 0 && --_timer > 0)
                    break;
                Velocity = PixelVelocity.Zero;
                Tail.Action = RidleyTail.Doing.None;
                _phase = Phase.Gone;
                foreach (NodePath path in ToldWhenGone)
                    (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"Ridley is to tell \"{path}\" that it has gone, but that is nothing that can be told.")).Triggered(GoneMessage);
                Vanish();
                break;

            case Phase.Gone:
                break;

            default:
                throw new InvalidOperationException($"Ridley on Ceres has no phase {_phase}.");
        }
    }

    /// <summary>One update of a leg of its swoop, turning leftward toward a heading. Moves on to the next leg when its time is up.</summary>
    private void Swoop(int turn, int heading, Phase next, int nextUpdates)
    {
        SetSwoopSpeed(turn, true, heading, Pixels.FromWhole(3));
        if (--_timer <= 0)
            (_phase, _timer) = (next, nextUpdates);
    }

    private void BeginHover() => (_phase, _hoverWait) = (Phase.Hovering, 124);

    private void BeginRetreat()
    {
        _phase = Phase.Retreat;
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
        visual.Show(_babyTimeline.Current.Picture);
    }
}
