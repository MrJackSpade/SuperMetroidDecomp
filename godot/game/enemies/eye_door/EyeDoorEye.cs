using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The eye that guards an eye door. It stays shut until Samus comes near, then opens and watches her, spitting at
/// her three times before it blinks; right in front of it, it keeps its lid down. Only missiles hurt it, and only
/// once it has opened: three, or one super missile, destroy it. Its door's cap is held shut for as long as it lives.
/// </summary>
/// <remarks>
/// Its timelines are "closed", "opening", "watching", whose cue "shoot" spits, "blinking", "flinching", whose cues
/// "smoke" and "sweat" leave those, and "destroyed"; each ends with the cue "done". "part_still" and "part_pulse"
/// are for its visuals "Middle" and "Bottom", the rest of the growth around its door. Its shots are "spit" and
/// "sweat". It faces the way its door opens from: left or right.
/// </remarks>
public partial class EyeDoorEye : Enemy
{
    /// <summary>The fact recorded once it is destroyed; it is placed only while that is not recorded.</summary>
    [Export]
    public string DestroyedFact { get; set; } = "";

    /// <summary>How many missiles destroy it.</summary>
    [Export, DataValue("hits_to_destroy")]
    public int HitsToDestroy { get; set; } = 3;

    /// <summary>How near Samus must be for it to open, in blocks across and down.</summary>
    [Export]
    public Vector2I WakeReach { get; set; } = new(6, 4);

    /// <summary>How near Samus must be for it to keep its lid down, in blocks across and down.</summary>
    [Export]
    public Vector2I InFrontReach { get; set; } = new(1, 4);

    /// <summary>How near Samus must be for the growth around it to pulse, in blocks across and down.</summary>
    [Export]
    public Vector2I PulseReach { get; set; } = new(6, 16);

    /// <summary>Where its spit leaves from when it faces left, from its middle, in pixels; mirrored when it faces right.</summary>
    [Export]
    public Vector2I SpitFrom { get; set; } = new(-16, 8);

    /// <summary>Where its sweat drips from, from its middle, in pixels.</summary>
    [Export]
    public Vector2I SweatFrom { get; set; } = new(-16, 8);

    /// <summary>The sound of a missile hurting it.</summary>
    [Export, DataValue("hit_sound")]
    public string HitSound { get; set; } = "";

    /// <summary>The sound of a shot that does nothing to it.</summary>
    [Export, DataValue("dud_sound")]
    public string DudSound { get; set; } = "";

    /// <summary>The sound of it spitting.</summary>
    [Export, DataValue("spit_sound")]
    public string SpitSound { get; set; } = "";

    /// <summary>The effect of the smoke a hit knocks out of it.</summary>
    [Export, DataValue("smoke_effect")]
    public string SmokeEffect { get; set; } = "";

    /// <summary>The effect of the puff of smoke of its destruction.</summary>
    [Export, DataValue("puff_effect")]
    public string PuffEffect { get; set; } = "";

    private enum Doing
    {
        Closed,
        Opening,
        Watching,
        Blinking,
        Flinching,
        Destroyed,
    }

    /// <summary>Whether it has been destroyed.</summary>
    public bool IsDestroyed => IsDead || _doing == Doing.Destroyed;

    private Doing _doing;
    private bool _takesShots;
    private int _hits;
    private TimelinePlayer? _parts;
    private bool _pulsing;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (!_takesShots)
            return HitResult.Absorbed;
        bool super = hit.Carries(new HitKind(HitKinds.SuperMissile));
        if (!super && !hit.Carries(new HitKind(HitKinds.Missile)))
        {
            Sound(DudSound);
            return HitResult.Absorbed;
        }

        Sound(HitSound);
        LeaveSmoke();
        LeaveSmoke();
        LeaveSmoke();
        LeaveSmoke();
        _hits = super ? HitsToDestroy : _hits + 1;
        if (_hits < HitsToDestroy)
        {
            Begin(Doing.Flinching, "flinching");
            return HitResult.Hurt;
        }

        _takesShots = false;
        if (DestroyedFact.Length > 0)
            Room.Session.Progress.Record(DestroyedFact);
        Leave(PuffEffect, Position);
        Leave(PuffEffect, Position);
        LeaveSmoke();
        LeaveSmoke();
        LeaveSmoke();
        LeaveSmoke();
        Begin(Doing.Destroyed, "destroyed");
        return HitResult.Hurt;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // It is part of the door; touching it does nothing.
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_takesShots, _hits, _pulsing) = (false, 0, false);
        Begin(Doing.Closed, "closed");
        _parts = new TimelinePlayer(Timelines.Get("part_still"));
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "shoot":
                Spit();
                break;
            case "smoke":
                LeaveSmoke();
                LeaveSmoke();
                break;
            case "sweat":
                Fire("sweat", Position + PixelOffset.FromWhole(SweatFrom.X, SweatFrom.Y),
                    new PixelVelocity(Pixels.FromNumber(Facing == Facing.Left ? -0.25 : 0.25), Pixels.FromWhole(2)));
                break;
            case "done":
                OnTimelineDone();
                break;
            default:
                throw new InvalidOperationException($"The eye of an eye door's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Behave() => StepParts();

    /// <summary>Decides what it does next, each time what it was doing has played out.</summary>
    private void OnTimelineDone()
    {
        switch (_doing)
        {
            case Doing.Closed:
                if (IsSamusWithin(WakeReach))
                {
                    _takesShots = true;
                    Begin(Doing.Opening, "opening");
                }
                else
                {
                    Begin(Doing.Closed, "closed");
                }

                break;

            case Doing.Watching:
                if (IsSamusWithin(WakeReach))
                    Watch();
                else
                    Begin(Doing.Closed, "closed");
                break;

            case Doing.Opening:
            case Doing.Blinking:
            case Doing.Flinching:
                Watch();
                break;

            case Doing.Destroyed:
                Die();
                break;

            default:
                throw new InvalidOperationException($"The eye of an eye door has no step {_doing}.");
        }
    }

    /// <summary>It watches Samus, unless she is right in front of it, when it keeps its lid down.</summary>
    private void Watch()
    {
        if (IsSamusWithin(InFrontReach))
            Begin(Doing.Blinking, "blinking");
        else
            Begin(Doing.Watching, "watching");
    }

    private void Begin(Doing doing, string timeline)
    {
        _doing = doing;
        Play(timeline, restart: true);
    }

    private void Spit()
    {
        var from = PixelOffset.FromWhole(Facing == Facing.Left ? SpitFrom.X : -SpitFrom.X, SpitFrom.Y);
        Fire("spit", Position + from, PixelVelocity.Zero);
        Sound(SpitSound);
    }

    /// <summary>Leaves a puff of smoke somewhere about it.</summary>
    private void LeaveSmoke()
    {
        RandomDraw roll = Random.Last;
        Random.Draw();
        Leave(SmokeEffect, Position + PixelOffset.FromWhole(-8 + roll.Part(0, 4), -16 + roll.Part(8, 5)));
    }

    private void Leave(string effect, PixelPosition at)
    {
        if (effect.Length > 0)
            LeaveEffect(effect, at);
    }

    /// <summary>Whether Samus is within so many blocks of it, across and down.</summary>
    private bool IsSamusWithin(Vector2I blocks)
    {
        PixelPosition samus = Room.Player.Position;
        return Math.Abs((samus.X.Whole >> 4) - (Position.X.Whole >> 4)) <= blocks.X
            && Math.Abs((samus.Y.Whole >> 4) - (Position.Y.Whole >> 4)) <= blocks.Y;
    }

    /// <summary>The growth around its door pulses while Samus is anywhere near, a beat at a time.</summary>
    private void StepParts()
    {
        if (_parts is null)
            return;
        _parts.Step();
        if (_parts.IsOver)
        {
            _pulsing = IsSamusWithin(PulseReach);
            _parts = new TimelinePlayer(Timelines.Get(_pulsing ? "part_pulse" : "part_still"));
        }

        foreach (string name in new[] { "Middle", "Bottom" })
            GetNodeOrNull<ActorVisual>(name)?.Show(_parts.Current.Picture.Replace("{part}", name.ToLowerInvariant(), StringComparison.Ordinal));
    }
}
