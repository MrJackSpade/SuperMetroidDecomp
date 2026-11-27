using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Dragon: a seahorse that lives in lava. It waits below the surface, rises facing Samus, spits three fireballs
/// that arc toward her side, waits, and sinks again. Its wings beat all the while, drawn as a second visual.
/// </summary>
/// <remarks>
/// Its timelines are "idle_left" and "idle_right"; "attack_left" and "attack_right", whose cue "spit" lets a fireball
/// go; and "wings_left" and "wings_right" for the visual named "Wings". Its shot is "fireball".
/// </remarks>
public partial class Dragon : Enemy
{
    /// <summary>How many updates it takes to rise, and to sink, a pixel each update.</summary>
    [Export, DataValue("rise_updates")]
    public int RiseUpdates { get; set; } = 49;

    /// <summary>How many fireballs it spits each time it is up.</summary>
    [Export, DataValue("shots_per_attack")]
    public int ShotsPerAttack { get; set; } = 3;

    /// <summary>How many updates it waits after its last fireball before sinking.</summary>
    [Export, DataValue("wait_before_sinking")]
    public int WaitBeforeSinking { get; set; } = 96;

    /// <summary>How many updates it waits below before rising again.</summary>
    [Export, DataValue("wait_before_rising")]
    public int WaitBeforeRising { get; set; } = 129;

    /// <summary>Where a fireball leaves from when it faces right, across from its middle, in pixels.</summary>
    [Export, DataValue("muzzle_across")]
    public int MuzzleAcross { get; set; } = 12;

    /// <summary>How far above its middle a fireball leaves from, in pixels.</summary>
    [Export, DataValue("muzzle_up")]
    public int MuzzleUp { get; set; } = 28;

    /// <summary>The upward speed a fireball leaves with, as a number of pixels.</summary>
    [Export, DataValue("fireball_rise")]
    public double FireballRise { get; set; } = 961.0 / 256;

    /// <summary>The sound of each fireball it spits, by name: heard though the room has no place for the fireball.</summary>
    [Export, DataValue("spit_sound")]
    public string SpitSound { get; set; } = "";

    private enum Doing
    {
        WaitingToRise,
        Rising,
        Attacking,
        Spat,
        Reattacking,
        WaitingToSink,
        Sinking,
    }

    private Doing _doing;
    private Countdown _wait;
    private int _shotsLeft;
    private TimelinePlayer? _wings;

    /// <summary>
    /// The enemy the room lists after its wing, written by the rooms' generator; empty where that place holds nothing
    /// the Godot room places. On the cartridge a hit on a part of a Dragon is carried onto the enemy listed right
    /// after that part: the body's onto its own wing, and the wing's onto this one.
    /// </summary>
    [Export]
    public NodePath ListedAfterWing { get; set; } = new();

    /// <summary>The health its wing has left: the wing is a part with health of its own that only a power bomb's blast strikes.</summary>
    private int _wingHealth;

    /// <summary>
    /// Whether its wing is closed to a blast. A hit on its body, or her touch, leaves the wing the time its body then
    /// cannot be hit for, and the wing, which takes no notice of Samus, never counts that time down: so it stays
    /// closed until a later hit or touch finds the body's own time run out. The wing's own hit closes it the same way.
    /// </summary>
    private bool _wingClosed;

    /// <summary>
    /// A power bomb's blast strikes its wing before its body, as the blast sweeps the room's enemies last listed
    /// first; every hit that lands on its body then leaves the wing the body's time of not being hit.
    /// </summary>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (!IsDead && !_wingClosed && hit.Carries(new HitKind(Data.HitKinds.PowerBomb)))
            StrikeWing(hit, part);
        HitResult result = base.TakeHit(hit, part);
        if (result != HitResult.Ignored)
            _wingClosed = IsSafeFromHits;
        return result;
    }

    /// <summary>Her touch on its body leaves the wing the body's time of not being hit too, which opens the wing again once that has run out.</summary>
    protected override void Touch(Collider part)
    {
        base.Touch(part);
        _wingClosed = IsSafeFromHits;
    }

    /// <summary>
    /// The blast's hit on the wing, which answers a hit as its body does: it loses the hit's damage, is closed from
    /// then, and carries the hit onto the enemy listed after it. A wing the hit leaves alive gives that enemy the
    /// time it cannot be hit, its hurt flash and the freeze the Dragon has, in place of that enemy's own; a wing the
    /// hit destroys takes that enemy out of the room outright. (The wing's own death, its burst and the Dragon going
    /// on without it, is not carried: it needs a second blast on a wing opened again by her touch.)
    /// </summary>
    private void StrikeWing(Hit hit, Collider part)
    {
        Reaction reaction = ReactionTo(hit, part);
        int damage = reaction.DamageFrom(hit.Damage);
        _wingHealth -= damage;
        _wingClosed = true;
        if (ListedAfterWing.IsEmpty || GetNodeOrNull<Enemy>(ListedAfterWing) is not { IsDead: false } next)
            return;
        if (_wingHealth <= 0)
        {
            next.BeTakenOut();
            return;
        }

        int flash = damage > 0 ? Math.Max(0, HurtFlashUpdates - hit.FlashFewer) : 0;
        next.TakeAftermath(new HitAftermath(0, damage > 0 ? reaction.UnhittableUpdates : 0, flash, Status, StatusUpdatesLeft));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _wingHealth = MaxHealth;
        _wingClosed = false;
        Facing = Facing.Left;
        Play("idle_left");
        _wings = new TimelinePlayer(Timelines.Get("wings_left"));
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue != "spit")
            return;
        if (_doing != Doing.Attacking)
            throw new InvalidOperationException($"The Dragon \"{Name}\" finished spitting while it was {_doing}.");
        _doing = Doing.Spat;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.WaitingToRise:
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    break;
                _wait = Countdown.Of(RiseUpdates);
                _doing = Doing.Rising;
                Facing = TowardPlayer;
                Play(Named("idle"));
                FrameTimeline wings = Timelines.Get(Named("wings"));
                if (_wings is null || _wings.Timeline != wings)
                    _wings = new TimelinePlayer(wings);
                break;

            case Doing.Rising:
                _wait = _wait.Stepped();
                if (_wait.IsOver)
                {
                    _shotsLeft = ShotsPerAttack;
                    _doing = Doing.Attacking;
                }

                Position -= PixelOffset.FromWhole(0, 1);
                break;

            case Doing.Attacking:
                Play(Named("attack"));
                break;

            case Doing.Spat:
                PixelPosition muzzle = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole)
                    + PixelOffset.FromWhole(Facing == Facing.Left ? -MuzzleAcross : MuzzleAcross, -MuzzleUp);
                Fire("fireball", muzzle, new PixelVelocity(Facing.Toward(Shot("fireball").Speed), -Pixels.FromNumber(FireballRise)));
                Sound(SpitSound);
                if (--_shotsLeft != 0)
                {
                    _doing = Doing.Reattacking;
                    break;
                }

                _wait = Countdown.Of(WaitBeforeSinking);
                _doing = Doing.WaitingToSink;
                break;

            case Doing.Reattacking:
                Play(Named("attack"), restart: true);
                _doing = Doing.Attacking;
                break;

            case Doing.WaitingToSink:
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    break;
                _wait = Countdown.Of(RiseUpdates);
                _doing = Doing.Sinking;
                Play(Named("idle"));
                break;

            case Doing.Sinking:
                _wait = _wait.Stepped();
                if (_wait.IsOver)
                {
                    _wait = Countdown.Of(WaitBeforeRising);
                    _doing = Doing.WaitingToRise;
                }

                Position += PixelOffset.FromWhole(0, 1);
                break;

            default:
                throw new InvalidOperationException($"Undefined Dragon stage {_doing}.");
        }

        ShowWings();
    }

    /// <inheritdoc/>
    /// <remarks>Its wings beat on while a hurt holds its body still, as they do while it acts.</remarks>
    protected override void WhileHurt() => ShowWings();

    /// <summary>The name of one of its timelines for the way it faces.</summary>
    private string Named(string timeline) => Facing == Facing.Left ? $"{timeline}_left" : $"{timeline}_right";

    /// <summary>Moves its wings on by one update.</summary>
    private void ShowWings()
    {
        if (_wings is null || GetNodeOrNull<ActorVisual>("Wings") is not { } wings)
            return;
        _wings.Step();
        wings.Show(_wings.Shown);
    }
}
