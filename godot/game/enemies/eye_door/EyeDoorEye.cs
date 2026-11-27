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
/// while its lid is up: three, or one super missile, destroy it. Its door's cap is held shut for as long as it lives.
/// </summary>
/// <remarks>
/// <code>
/// "wake_reach": { "x": 6, "y": 4 }, "in_front_reach": { "x": 1, "y": 4 }, "pulse_reach": { "x": 6, "y": 16 },
/// "spit_from": { "left": { "x": -8, "y": 16 }, "right": { "x": 24, "y": 16 } }, "sweat_from": { "x": -8, "y": 16 },
/// "sweat_launch": { "left": { "x": -0.25, "y": 2 }, "right": { "x": 0.25, "y": 2 } },
/// "smoke": { "x": 0, "y": -8, "across": { "from": 0, "digits": 4 }, "down": { "from": 8, "digits": 5 } },
/// "puff_at": { "x": 8, "y": 8 }
/// </code>
/// It stands on the top-left corner of the upper of its two blocks, and everything is measured from there. The
/// reaches are in blocks, across and down, counted between Samus's block and its own. Smoke is left at "smoke" plus,
/// across and down, so many binary digits of the number last drawn, starting so many digits up.
/// Its timelines are "closed", "opening", "watching", whose cue "shoot" spits, "blinking", "flinching", whose cues
/// "smoke" and "sweat" leave those, and "destroyed"; each ends with the cue "done". The cues "open" and "shut" say
/// whether its lid is up, which is when a shot can land on it. "part_still", "part_pulse_left" and
/// "part_pulse_right" are for its visuals "Middle" and "Bottom", the rest of the growth around its door, in whose
/// pictures' paths "{part}" is "middle" or "bottom". In every picture's path "{side}" is "left" or "right", the way
/// it faces: the way its door opens from. Its shots are "spit" and "sweat".
/// </remarks>
public partial class EyeDoorEye : Enemy
{
    /// <summary>The fact recorded once it is destroyed; it is placed only while that is not recorded.</summary>
    [Export]
    public string DestroyedFact { get; set; } = "";

    /// <summary>How many missiles destroy it.</summary>
    [Export, DataValue("hits_to_destroy")]
    public int HitsToDestroy { get; set; } = 3;

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

    /// <summary>The shot that has reached it and that it has yet to take, in its own turn; a later one takes the place of an earlier.</summary>
    private Hit? _strike;
    private bool _takesShots;
    private bool _lidUp;
    private int _hits;
    private TimelinePlayer? _parts;
    private Vector2I _wakeReach, _inFrontReach, _pulseReach;
    private PixelOffset _spitFromLeft, _spitFromRight, _sweatFrom, _smokeAt, _puffAt;
    private PixelVelocity _sweatLeft, _sweatRight;
    private (int From, int Digits) _smokeAcross, _smokeDown;

    /// <summary>The way it faces, as its pictures' paths name it.</summary>
    private string Side => Facing == Facing.Left ? "left" : "right";

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        static Vector2I Blocks(DataNode at) => new(at.Int("x"), at.Int("y"));
        static PixelOffset Offset(DataNode at) => PixelOffset.FromWhole(at.Int("x"), at.Int("y"));
        static PixelVelocity Speed(DataNode at) => new(at.Distance("x"), at.Distance("y"));
        static (int From, int Digits) Field(DataNode field) => (field.Int("from"), field.Int("digits"));
        _wakeReach = Blocks(data.Get("wake_reach"));
        _inFrontReach = Blocks(data.Get("in_front_reach"));
        _pulseReach = Blocks(data.Get("pulse_reach"));
        _spitFromLeft = Offset(data.Get("spit_from").Get("left"));
        _spitFromRight = Offset(data.Get("spit_from").Get("right"));
        _sweatFrom = Offset(data.Get("sweat_from"));
        _sweatLeft = Speed(data.Get("sweat_launch").Get("left"));
        _sweatRight = Speed(data.Get("sweat_launch").Get("right"));
        DataNode smoke = data.Get("smoke");
        _smokeAt = Offset(smoke);
        _smokeAcross = Field(smoke.Get("across"));
        _smokeDown = Field(smoke.Get("down"));
        _puffAt = Offset(data.Get("puff_at"));
    }

    /// <summary>It is a fitting of its room, as the cartridge's is: its turn comes with the room's objects.</summary>
    public override bool ActsWithObjects => true;

    /// <summary>
    /// A shot that reaches it is kept until its own turn, where it is taken before its pictures count on; of several
    /// only the last is kept. What the shot makes of the meeting is settled here: a missile has struck, anything else
    /// has stopped on it.
    /// </summary>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        // Destroyed, it is no longer there to be shot: its door's blocks are a blue cap's from that moment, and a
        // shot goes on to the cap while its last pictures are drawn over it.
        if (_doing == Doing.Destroyed)
            return HitResult.Ignored;

        // With its lid down it is as solid as the wall: a shot stops on it and that is all.
        if (!_takesShots || !_lidUp)
            return HitResult.Absorbed;
        _strike = hit;
        return hit.Carries(new HitKind(HitKinds.SuperMissile)) || hit.Carries(new HitKind(HitKinds.Missile)) ? HitResult.Hurt : HitResult.Absorbed;
    }

    /// <summary>
    /// Takes the shot it has yet to take, while it takes shots: a missile counts a hit and a super missile every hit
    /// it needs, and any other shot only makes the dud's sound. Returns whether it was hit, which takes its turn: its
    /// pictures do not count on through it.
    /// </summary>
    private bool TakeStrike()
    {
        if (!_takesShots || _strike is not { } hit)
            return false;
        _strike = null;
        bool super = hit.Carries(new HitKind(HitKinds.SuperMissile));
        if (!super && !hit.Carries(new HitKind(HitKinds.Missile)))
        {
            Sound(DudSound);
            return false;
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
            return true;
        }

        _takesShots = false;
        if (DestroyedFact.Length > 0)
            Room.Session.Progress.Record(DestroyedFact);
        LeavePuff();
        LeavePuff();
        LeaveSmoke();
        LeaveSmoke();
        LeaveSmoke();
        LeaveSmoke();
        // Its flashes, to the doorway cleared and to a shut blue cap, are drawn over its cap, blue from now on: a cap
        // opened meanwhile shows only once they are done, as on the cartridge.
        Begin(Doing.Destroyed, "destroyed");
        return true;
    }

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step) => step.Picture.Replace("{side}", Side, StringComparison.Ordinal);

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // It is part of the door; touching it does nothing. Nor does the growth round it, whose old blocks are spikes
        // with no reaction, which hurt neither her touch nor her grapple swing.
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_takesShots, _lidUp, _hits, _strike) = (false, false, 0, null);
        Begin(Doing.Closed, "closed");
        // The growth looks for Samus on its first update.
        _parts = null;
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "open":
                _lidUp = true;
                break;
            case "shut":
                _lidUp = false;
                break;
            case "shoot":
                Fire("spit", Position + (Facing == Facing.Left ? _spitFromLeft : _spitFromRight), PixelVelocity.Zero);
                Sound(SpitSound);
                break;
            case "smoke":
                LeaveSmoke();
                LeaveSmoke();
                break;
            case "sweat":
                Fire("sweat", Position + _sweatFrom, Facing == Facing.Left ? _sweatLeft : _sweatRight);
                break;
            case "done":
                OnTimelineDone();
                break;
            default:
                throw new InvalidOperationException($"The eye of an eye door's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // A shot is taken before its pictures count on, and one that hits it takes the turn.
        HoldsTimeline = TakeStrike();
        StepParts();
    }

    /// <summary>Decides what it does next, each time what it was doing has played out.</summary>
    private void OnTimelineDone()
    {
        switch (_doing)
        {
            case Doing.Closed:
                if (IsSamusWithin(_wakeReach))
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
                if (IsSamusWithin(_wakeReach))
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
                // It was part of the door, not something killed: it is simply gone.
                Vanish();
                break;

            default:
                throw new InvalidOperationException($"The eye of an eye door has no step {_doing}.");
        }
    }

    /// <summary>It watches Samus, unless she is right in front of it, when it keeps its lid down.</summary>
    private void Watch()
    {
        if (IsSamusWithin(_inFrontReach))
            Begin(Doing.Blinking, "blinking");
        else
            Begin(Doing.Watching, "watching");
    }

    private void Begin(Doing doing, string timeline)
    {
        _doing = doing;
        Play(timeline, restart: true);
    }

    /// <summary>
    /// Leaves a puff of smoke somewhere about it: where the number last drawn puts it. Another number is then drawn,
    /// if the smoke was made.
    /// </summary>
    private void LeaveSmoke()
    {
        RandomDraw roll = Random.Last;
        if (Leave(SmokeEffect, Position + _smokeAt + PixelOffset.FromWhole(roll.Part(_smokeAcross.From, _smokeAcross.Digits), roll.Part(_smokeDown.From, _smokeDown.Digits))))
            Random.Draw();
    }

    /// <summary>Leaves the puff of its destruction on its middle. A number is drawn for it too, if it was made, as for any smoke of its.</summary>
    private void LeavePuff()
    {
        if (Leave(PuffEffect, Position + _puffAt))
            Random.Draw();
    }

    /// <summary>
    /// Leaves smoke of its: behind everything that stands in the room, where the cartridge draws the eye door's smoke.
    /// The smoke holds one of the room's places for what its enemies launch, as the cartridge's is one of its enemies'
    /// shots, and with none free it is not made. Returns whether it was.
    /// </summary>
    private bool Leave(string effect, PixelPosition at) =>
        effect.Length > 0 && Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), at, holdsPlace: true, standing: Standing.Hindmost));

    /// <summary>Whether Samus is within so many blocks of it, across and down.</summary>
    private bool IsSamusWithin(Vector2I blocks)
    {
        PixelPosition samus = Room.Player.Position;
        return Math.Abs((samus.X.Whole >> 4) - (Position.X.Whole >> 4)) <= blocks.X
            && Math.Abs((samus.Y.Whole >> 4) - (Position.Y.Whole >> 4)) <= blocks.Y;
    }

    /// <summary>
    /// The growth around its door pulses while Samus is anywhere near, a beat at a time. It goes the moment the eye
    /// is destroyed. Each beat, and its first, is chosen on the update after the last ended, and shows from then.
    /// </summary>
    private void StepParts()
    {
        if (_parts is null || _parts.IsOver)
            _parts = new TimelinePlayer(Timelines.Get(IsSamusWithin(_pulseReach) ? $"part_pulse_{Side}" : "part_still"));
        _parts.Step();

        foreach (string name in new[] { "Middle", "Bottom" })
        {
            if (GetNodeOrNull<ActorVisual>(name) is not { } visual)
                continue;
            visual.SetShown(_doing != Doing.Destroyed);
            visual.Show(_parts.Shown.Picture
                .Replace("{part}", name.ToLowerInvariant(), StringComparison.Ordinal)
                .Replace("{side}", Side, StringComparison.Ordinal));
        }
    }
}
