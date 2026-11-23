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
/// What both of Ridley's fights share: a body that flies with a weight of its own, wings that beat faster the faster
/// it goes, a tail that sways and lashes, fireballs from its mouth, and the way it first shows itself, eyes in the
/// dark and then the rest. Each fight says what it does with them.
/// </summary>
/// <remarks>
/// <para>Its body plays "standing_left", "standing_right", "opening_roar_left", "opening_roar_right",
/// "turn_left_to_right", "turn_right_to_left", "fireballing_left", "fireballing_right",
/// "transition_to_flying_left" and "transition_to_flying_right", and what its fight adds. Their cues are "roar",
/// "stop_roaring", "face_forward", "flip:" and the side it now faces, "claws:" and "standing", "half" or "extended",
/// "move:" with pixels across and down, "aim", "fireball:afterburn", "fireball:plain", "cut_short" and "lift_off".</para>
/// <para>Its visual "Wings" shows the pictures its data lists as "wing_pictures", in turn. The visuals "Tail0" to
/// "Tail6" are its tail's links, with the pictures its data lists as "tail_pictures" and, for the tip, one of
/// "tail_tip_pictures" by the way it points, the first pointing straight down and the rest in turn toward the
/// right. Its hurtboxes "TailTip" and "TailJoint" stop shots. Its shot is "fireball".</para>
/// </remarks>
public abstract partial class RidleyBody : Enemy
{
    /// <summary>The music of the fight, by name.</summary>
    [Export]
    public string FightMusic { get; set; } = "";

    /// <summary>The least and greatest places its middle may be, in the room's pixels.</summary>
    [Export]
    public Rect2I FlightBounds { get; set; } = new(64, 64, 160, 352);

    /// <summary>How many updates pass, once Samus is in the room, before its eyes show.</summary>
    [Export, DataValue("reveal_delay")]
    public int RevealDelay { get; set; } = 171;

    /// <summary>How many updates its eyes, and then its body, take to come out of the dark.</summary>
    [Export, DataValue("fade_updates")]
    public int FadeUpdates { get; set; } = 16;

    /// <summary>How many updates pass between its body showing and its roar.</summary>
    [Export, DataValue("before_roar_updates")]
    public int BeforeRoarUpdates { get; set; } = 5;

    /// <summary>The harm its tail's tip does.</summary>
    [Export, DataValue("tail_damage")]
    public int TailDamage { get; set; } = 120;

    /// <summary>Its roar, by name.</summary>
    [Export, DataValue("roar_sound")]
    public string RoarSound { get; set; } = "";

    /// <summary>The steps of its showing itself.</summary>
    protected enum Reveal
    {
        Delay,
        FadeInEyes,
        FadeInBody,
        WaitBeforeRoar,
        WaitBeforeLiftoff,
        TakingOff,
        Done,
    }

    /// <summary>How its claws hang: they reach lower the further its legs are out.</summary>
    protected enum Claws
    {
        Standing,
        Half,
        Extended,
    }

    private static readonly int[] WingPaces = [12, 14, 16, 18, 28, 32, 40, 48];
    private static readonly Pixels MostSpeed = Pixels.FromWhole(5);

    private readonly List<string> _wingPictures = [];
    private readonly List<string> _tailPictures = [];
    private readonly List<string> _tailTipPictures = [];
    private int _wingPose;
    private int _wingCount = 32;
    private int _wait;
    private int _fade;
    private PixelVelocity _fireballSpeed;

    /// <summary>Its tail.</summary>
    protected RidleyTail Tail { get; } = new();

    /// <summary>Where it has got to in showing itself.</summary>
    protected Reveal Revealing { get; private set; }

    /// <summary>Whether it has left the ground and its fight has begun.</summary>
    protected bool HasLiftedOff => Revealing == Reveal.Done;

    /// <summary>Whether it is turning about, facing out of the screen.</summary>
    protected bool IsTurning { get; private set; }

    /// <summary>Whether it is in the middle of a roar.</summary>
    protected bool IsRoaring { get; private set; }

    /// <summary>How its claws hang.</summary>
    protected Claws ClawHeight { get; private set; }

    /// <summary>Whether its last move was stopped by the edge of where it may fly.</summary>
    protected bool HitBounds { get; private set; }

    /// <summary>Whether it flies and its tail moves; off while it is still unseen, and once it has broken up.</summary>
    protected bool IsInMotion { get; set; }

    /// <summary>Whether its body and tail pass through Samus and her shots.</summary>
    protected bool IsIntangible { get; set; } = true;

    /// <summary>Whether it carries Samus in its claws.</summary>
    protected virtual bool CarriesSamus => false;

    /// <summary>The way it faces, as its timelines name it.</summary>
    protected string Side => Facing == Facing.Right ? "right" : "left";

    /// <summary>Where its claws are.</summary>
    protected PixelPosition ClawPosition => Position + PixelOffset.FromWhole(
        IsTurning ? 0 : Facing == Facing.Left ? 12 : -12,
        ClawHeight switch { Claws.Standing => 35, Claws.Half => 46, _ => 56 });

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        foreach ((List<string> pictures, string name) in new[] { (_wingPictures, "wing_pictures"), (_tailPictures, "tail_pictures"), (_tailTipPictures, "tail_tip_pictures") })
        {
            pictures.Clear();
            pictures.AddRange(data.List(name).Select(picture => picture.AsText()));
        }

        if (_wingPictures.Count == 0 || _tailPictures.Count != Tail.Links.Count - 1 || _tailTipPictures.Count == 0)
            throw data.Problem("must list its wing pictures, a picture for each link of its tail but the tip, and the tip's pictures");
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !IsIntangible && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        Reaction reaction = base.ReactionTo(hit, part);
        // The tip of its tail and the joint behind it are armoured: they stop what hits them.
        return IsTailArmour(part) ? reaction with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never } : reaction;
    }

    /// <summary>Whether a part of it is one of the armoured places of its tail.</summary>
    private static bool IsTailArmour(Collider part) => part.Name == "TailTip" || part.Name == "TailJoint";

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!IsIntangible && !CarriesSamus && !IsTailArmour(part))
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        Revealing = Reveal.Delay;
        _wait = RevealDelay;
        Play($"standing_{Side}", restart: true);
        ShowFade(0, 0);
    }

    /// <summary>Called as its roar begins, before it takes off.</summary>
    protected virtual void OnOpeningRoar()
    {
    }

    /// <summary>Whether it may take off yet, asked each update once its roar has begun.</summary>
    protected abstract bool MayTakeOff();

    /// <summary>Called as it leaves the ground.</summary>
    protected abstract void OnLiftOff();

    /// <summary>Asked at the points where its fireballing may be cut short: the timeline to play instead, or none.</summary>
    protected virtual string? CutFireballingShort() => null;

    /// <summary>Its showing itself, one update of it. Its fight calls this until it has lifted off.</summary>
    protected void StepReveal()
    {
        switch (Revealing)
        {
            case Reveal.Delay:
                if (--_wait <= 0)
                    (Revealing, _fade) = (Reveal.FadeInEyes, 0);
                break;

            case Reveal.FadeInEyes:
                ShowFade(++_fade, 0);
                if (_fade < FadeUpdates)
                    break;
                (Revealing, _fade) = (Reveal.FadeInBody, 0);
                IsInMotion = true;
                break;

            case Reveal.FadeInBody:
                ShowFade(FadeUpdates, ++_fade);
                if (_fade < FadeUpdates)
                    break;
                IsIntangible = false;
                Revealing = Reveal.WaitBeforeRoar;
                _wait = BeforeRoarUpdates;
                if (FightMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(FightMusic);
                break;

            case Reveal.WaitBeforeRoar:
                if (--_wait > 0)
                    break;
                Play($"opening_roar_{Side}", restart: true);
                OnOpeningRoar();
                Revealing = Reveal.WaitBeforeLiftoff;
                break;

            case Reveal.WaitBeforeLiftoff:
                if (!MayTakeOff())
                    break;
                Play($"transition_to_flying_{Side}", restart: true);
                _wingCount = 8;
                Tail.ActivateAll();
                Tail.Action = RidleyTail.Doing.Neutral;
                Revealing = Reveal.TakingOff;
                break;

            case Reveal.TakingOff:
            case Reveal.Done:
                Velocity = PixelVelocity.Zero;
                break;

            default:
                throw new InvalidOperationException($"Ridley's showing itself has no step {Revealing}.");
        }
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "roar":
                IsRoaring = true;
                Sound(RoarSound);
                break;

            case "stop_roaring":
                IsRoaring = false;
                break;

            case "face_forward":
                IsTurning = true;
                break;

            case "flip":
                Tail.Mirror();
                IsTurning = false;
                Facing = parts[1] == "right" ? Facing.Right : Facing.Left;
                break;

            case "claws":
                ClawHeight = parts[1] switch
                {
                    "standing" => Claws.Standing,
                    "half" => Claws.Half,
                    "extended" => Claws.Extended,
                    _ => throw new InvalidOperationException($"Ridley's claws have no height \"{parts[1]}\"."),
                };
                break;

            case "move":
                Position += PixelOffset.FromWhole(
                    int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                    int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                break;

            case "aim":
                AimFireball();
                break;

            case "fireball":
                var fireball = (RidleyFireball)Fire("fireball", Muzzle, _fireballSpeed);
                fireball.Afterburns = parts[1] == "afterburn" ? 3 : 0;
                break;

            case "cut_short":
                if (CutFireballingShort() is { } instead)
                    Play(instead, restart: true);
                break;

            case "lift_off":
                Revealing = Reveal.Done;
                OnLiftOff();
                Velocity = Velocity.WithY(Pixels.FromNumber(-1.375));
                break;

            default:
                OnOwnCue(parts);
                break;
        }
    }

    /// <summary>Called for a cue that is not one both fights share.</summary>
    protected virtual void OnOwnCue(string[] cue) =>
        throw new InvalidOperationException($"Ridley's timeline \"{Playing}\" gives it the cue \"{string.Join(':', cue)}\", which it does not know.");

    /// <summary>Where its fireballs leave its mouth.</summary>
    private PixelPosition Muzzle => Position + PixelOffset.FromWhole(Facing == Facing.Left ? -25 : 25, -43);

    /// <summary>
    /// Aims its next fireball at Samus, within the arc ahead of and below it. Angles here are in 256ths of a turn from
    /// straight down, growing toward the right.
    /// </summary>
    private void AimFireball()
    {
        PixelOffset toSamus = Room.Player.Position - Muzzle;
        int angle = Turns.Wrap(Turns.PerTurn / 4 - Turns.AngleOf(toSamus.X.Whole, toSamus.Y.Whole));
        if (Facing == Facing.Left)
            angle = angle is < 64 or >= 235 ? 235 : Math.Max(angle, 176);
        else
            angle = angle is < 21 or >= 192 ? 21 : Math.Min(angle, 80);
        Pixels speed = Shot("fireball").Speed;
        _fireballSpeed = new PixelVelocity(Turns.Sine(angle, speed), Turns.Cosine(angle, speed));
    }

    /// <summary>Starts it on a timeline that has one for each way it faces.</summary>
    protected void PlayFacing(string timeline) => Play($"{timeline}_{Side}", restart: true);

    /// <summary>
    /// Pulls its speed toward a place, each way by a step that grows with the distance, the smaller the divisor the
    /// harder; a speed carrying it away from the place is checked first.
    /// </summary>
    protected void FlyToward(PixelPosition target, int divisor, Pixels reversalBoost = default)
    {
        divisor = Math.Clamp(divisor, 1, 16);
        Velocity = new PixelVelocity(
            Seek(Velocity.X, Position.X.Whole - target.X.Whole, divisor, reversalBoost),
            Seek(Velocity.Y, Position.Y.Whole - target.Y.Whole, divisor, reversalBoost));
    }

    private static Pixels Seek(Pixels speed, int fromTarget, int divisor, Pixels reversalBoost)
    {
        if (fromTarget == 0)
            return speed;
        Pixels step = Pixels.FromNumber(Math.Max(1, Math.Abs(fromTarget) / divisor) / 256.0);
        Pixels reversal = Pixels.FromNumber(0.03125);
        if (fromTarget > 0)
        {
            if (!speed.IsNegative)
                speed -= reversalBoost + reversal + step;
            speed -= step;
            return speed < -MostSpeed ? -MostSpeed : speed;
        }

        if (speed.IsNegative)
            speed += reversalBoost + reversal + step;
        speed += step;
        return speed >= MostSpeed ? MostSpeed : speed;
    }

    /// <summary>Whether its middle is within a reach of a place, allowing for its own size.</summary>
    protected bool IsNear(PixelPosition place, int reachAcross, int reachDown)
    {
        PixelRect body = Body?.Rect ?? default;
        return Math.Abs(Position.X.Whole - place.X.Whole) < body.Width / 2 + reachAcross + 1 &&
            Math.Abs(Position.Y.Whole - place.Y.Whole) < body.Height / 2 + reachDown + 1;
    }

    /// <summary>
    /// Moves it by its speed, stopping it dead at the edges of where it may fly, beats its wings, and moves its tail.
    /// Its fight calls this each update that it is in motion.
    /// </summary>
    protected void StepBody()
    {
        Rect2I bounds = FlightBounds;
        PixelPosition proposed = Position + Velocity.OverOneUpdate;
        bool outAcross = proposed.X.Whole < bounds.Position.X || proposed.X.Whole >= bounds.End.X;
        bool outDown = proposed.Y.Whole < bounds.Position.Y || proposed.Y.Whole >= bounds.End.Y;
        HitBounds = outAcross || outDown;
        OnMoving(proposed);
        Pixels x = proposed.X, y = proposed.Y;
        if (outAcross)
        {
            x = x.WithWhole(proposed.X.Whole < bounds.Position.X ? bounds.Position.X : bounds.End.X);
            Velocity = Velocity.WithX(Pixels.Zero);
        }

        if (outDown)
        {
            y = y.WithWhole(proposed.Y.Whole < bounds.Position.Y ? bounds.Position.Y : bounds.End.Y);
            Velocity = Velocity.WithY(Pixels.Zero);
        }

        Position = new PixelPosition(x, y);
        BeatWings();
        StepTail();
    }

    /// <summary>Called as it is about to move to a place, before the edges of where it may fly have stopped it.</summary>
    protected virtual void OnMoving(PixelPosition proposed)
    {
    }

    /// <summary>Its wings beat by a count that runs down faster the faster it flies, and faster still when it climbs.</summary>
    private void BeatWings()
    {
        Pixels fastest = Velocity.X.Abs() > Velocity.Y.Abs() ? Velocity.X.Abs() : Velocity.Y.Abs();
        if (!fastest.IsZero)
        {
            int pace = WingPaces[Math.Min(WingPaces.Length - 1, (int)(fastest.Whole * 4L + fastest.Fraction / (Pixels.FractionsPerPixel / 4)))];
            _wingCount -= Velocity.Y.IsNegative ? pace : pace / 2;
            if (_wingCount <= 0)
            {
                _wingCount = 32;
                _wingPose = (_wingPose + 1) % _wingPictures.Count;
            }
        }

        if (GetNodeOrNull<ActorVisual>("Wings") is { } wings)
        {
            wings.Show(_wingPictures[_wingPose].Replace("{side}", Side, StringComparison.Ordinal));
            wings.SetShown(!IsTurning);
        }
    }

    /// <summary>Steers the tail by what it is doing. A fight with more for the tail to do adds it here.</summary>
    protected virtual void SteerTail()
    {
        if (Tail.Action != RidleyTail.Doing.Neutral)
            return;
        Tail.SetLimits(Facing == Facing.Right && !IsTurning);
        Player samus = Room.Player;
        if (Tail.IsAllMoving)
        {
            bool idle = Tail.LeftwardTarget is null && Tail.RightwardTarget is null;
            bool ordered = Tail.WhipOrdered;
            Tail.WhipOrdered = false;
            if (ordered && idle)
            {
                AimTailWhip(0);
            }
            else if (Tail.WhipsWhenIdle)
            {
                // Now and then it lashes out a whole turn round; with Samus near it lashes straight at her.
                if (Random.Last.Part(0, 8) >= 240)
                    AimTailWhip(ordered ? 0 : -1);
                else if (Math.Abs(samus.Position.X.Whole - Position.X.Whole) < 128)
                    AimTailWhip(0);
            }
        }

        if (Tail.IsAtRest)
        {
            Tail.Base.Activate();
            (Tail.LeftwardTarget, Tail.RightwardTarget) = (null, null);
            Tail.SwingStep = samus.Position.X.Whole < 112 ? RidleyTail.SlowSwing : RidleyTail.FastSwing;
        }

        Tail.SwingLinks();
    }

    /// <summary>Whether it is fighting, so that its tail may lash out.</summary>
    protected virtual bool TailMayWhip => true;

    /// <summary>Aims a lash of its tail at Samus, or at a missile of hers that is close to its tip.</summary>
    protected void AimTailWhip(int turnsBeyond)
    {
        if (IsTurning || !TailMayWhip)
            return;
        PixelPosition aim = Room.Player.Position + PixelOffset.FromWhole(0, 24);
        PixelPosition tip = Tail.Tip.Position;
        Projectile? missile = Room.Projectiles.FirstOrDefault(shot =>
            shot.Side == HitSide.Player && !shot.HasEnded &&
            shot.Hit.CarriesAny([new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]) &&
            Math.Abs(shot.Position.X.Whole - tip.X.Whole) - shot.Area.Width / 2 < 64 &&
            Math.Abs(shot.Position.Y.Whole - tip.Y.Whole) - shot.Area.Height / 2 < 64);
        if (missile is not null)
            aim = missile.Position;
        PixelOffset toAim = aim - Tail.Base.Position;
        Tail.AimWhip(Turns.Wrap(Turns.PerTurn / 4 - Turns.AngleOf(toAim.X.Whole, toAim.Y.Whole)), Facing == Facing.Right, turnsBeyond);
    }

    /// <summary>Steers the tail, hangs it from the body, shows it, and lets its tip hurt Samus.</summary>
    private void StepTail()
    {
        SteerTail();
        PixelPosition hip = Position + PixelOffset.FromWhole(IsTurning ? 0 : Facing == Facing.Left ? 32 : -32, 16);
        Tail.Place(hip, IsTurning, Position.X.Whole);
        for (int index = 0; index < Tail.Links.Count; index++)
        {
            RidleyTail.Link link = Tail.Links[index];
            if (GetNodeOrNull<ActorVisual>($"Tail{index}") is not { } visual)
                continue;
            PixelOffset from = link.Position - Position;
            visual.Position = new Vector2(from.X.Whole, from.Y.Whole);
            visual.Show(index < _tailPictures.Count
                ? _tailPictures[index]
                : _tailTipPictures[link.Pointing * _tailTipPictures.Count / Turns.PerTurn]);
        }

        PlaceArmour("TailTip", Tail.Tip.Position, 14);
        PlaceArmour("TailJoint", Tail.InnerJoint.Position, 10);
        Player samus = Room.Player;
        if (CarriesSamus || samus.IsInvincible || IsIntangible || !IsInMotion)
            return;
        PixelRect body = samus.BodyRect;
        PixelOffset fromTip = samus.Position - Tail.Tip.Position;
        if (Math.Abs(fromTip.X.Whole) >= body.Width / 2 + 14 || Math.Abs(fromTip.Y.Whole) >= body.Height / 2 + 14)
            return;
        var away = PixelOffset.FromWhole(samus.Position.X.Whole >= Tail.Tip.Position.X.Whole ? 1 : -1, 0);
        samus.TakeHit(new Hit(TailDamage, HitSide.Enemies, HitKind.Contact) { Direction = away, Source = this }, samus.Hurtbox);
    }

    /// <summary>Puts one of the armoured places of its tail where the tail is.</summary>
    private void PlaceArmour(string name, PixelPosition at, int reach)
    {
        if (GetNodeOrNull<Collider>(name) is not { } collider)
            return;
        PixelOffset from = at - Position;
        collider.Rect = new PixelRect(from.X.Whole - reach, from.Y.Whole - reach, reach * 2, reach * 2);
    }

    /// <summary>Shows how far its eyes and its body have come out of the dark, each out of the updates that takes.</summary>
    private void ShowFade(int eyes, int body)
    {
        float shown = Math.Clamp((float)body / Math.Max(1, FadeUpdates), 0f, 1f);
        float glint = Math.Clamp((float)eyes / Math.Max(1, FadeUpdates), 0f, 1f);
        // Until its body shows, only a glint of it does.
        Modulate = new Color(shown, shown, shown, Math.Max(shown, glint * 0.25f));
    }
}
