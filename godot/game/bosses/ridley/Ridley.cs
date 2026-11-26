using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;
using static Game.Compatibility.SeekingSteps;

namespace Game.Enemies;

/// <summary>
/// Ridley. It fights twice, aboard Ceres station and in its lair in Norfair, and is one enemy set up for one or the
/// other. Both fights share what is here: a body that flies with a weight of its own, wings that beat faster the
/// faster it goes, a tail that sways and lashes, fireballs from its mouth, and the way it first shows itself, eyes
/// in the dark and then the rest. What it does with them in each fight is told beside this, a file for each.
/// </summary>
/// <remarks>
/// <para>Its body plays "standing_left", "standing_right", "opening_roar_left", "opening_roar_right",
/// "turn_left_to_right", "turn_right_to_left", "fireballing_left", "fireballing_right",
/// "transition_to_flying_left" and "transition_to_flying_right", and what its fight adds. Their cues are "roar",
/// "stop_roaring", "face_forward", "flip:" and the side it now faces, "claws:" and "standing", "half" or "extended",
/// "move:" with pixels across and down, "aim", "fireball:afterburn", "fireball:plain", "cut_short" and "lift_off".</para>
/// <para>Its visual "Wings" shows the pictures its data lists as "wing_pictures", in turn. The visuals "Tail0" to
/// "Tail6" are its tail's links, with the pictures its data lists as "tail_pictures" and, for the tip, one of
/// "tail_tip_pictures" by the way it points: the turn is shared out evenly among them from the angle
/// "tail_tip_arc_start", in turn toward the right. Its hurtboxes "TailTip" and "TailJoint" stop shots. Its shot is
/// "fireball".</para>
/// <para>It has one set of pictures, and the colours it is in are changes of their colours, shown on all its
/// visuals at once. Its data's "colours" gives them: "hidden", in which none of it is seen, from the moment it is
/// set up; "eyes", each step of its eyes fading in alone; and "body", each step of the rest of it fading in but the
/// last, which is its own colours as its fight gives them. The fade is one of opacity: it stands where it always
/// stands among the room's things, unseen and then seen, and what is behind it shows through it until it is. Under
/// "pose_boxes" its data gives, for each picture of
/// its body, the parts of it that can be touched and shot in that pose, each from its place; they are its colliders
/// "Part1" and on, which shots meet, and "Part1Touch" and on, which hurt. "flight_bounds" gives the least and
/// greatest places its middle may be ("left", "top", "right", "bottom"), in the room's pixels.</para>
/// </remarks>
public partial class Ridley : Enemy
{
    /// <summary>Whether this is its fight aboard Ceres station, not the one in Norfair.</summary>
    [Export]
    public bool OnCeres { get; set; }

    /// <summary>The music of the fight, by name.</summary>
    [Export]
    public string FightMusic { get; set; } = "";

    /// <summary>The least and greatest places its middle may be, in the room's pixels.</summary>
    private Rect2I FlightBounds { get; set; }

    /// <summary>How many updates pass, once Samus is in the room, before its eyes show.</summary>
    [Export, DataValue("reveal_delay")]
    public int RevealDelay { get; set; } = 171;

    /// <summary>How many steps its eyes, and then its body, take to come out of the dark.</summary>
    [Export, DataValue("fade_steps")]
    public int FadeSteps { get; set; } = 16;

    /// <summary>How many updates each step of its eyes' showing lasts.</summary>
    [Export, DataValue("eye_fade_step_updates")]
    public int EyeFadeStepUpdates { get; set; } = 1;

    /// <summary>How many updates its eyes glow alone before the rest of it begins to show.</summary>
    [Export, DataValue("eye_hold_updates")]
    public int EyeHoldUpdates { get; set; } = 48;

    /// <summary>How many updates each step of its body's showing lasts.</summary>
    [Export, DataValue("body_fade_step_updates")]
    public int BodyFadeStepUpdates { get; set; } = 2;

    /// <summary>The angle the first of its tail tip's pictures begins at, in 256ths of a turn from straight down toward the right.</summary>
    [Export, DataValue("tail_tip_arc_start")]
    public int TailTipArcStart { get; set; } = -8;

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
    private enum Reveal
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
    private enum Claws
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
    private readonly Dictionary<string, List<PixelRect>> _poseBoxes = new(StringComparer.Ordinal);

    /// <summary>The name the colours it is in are changed under on its visuals.</summary>
    private const string OwnColours = "RidleyColours";

    /// <summary>Its colours in the dark it waits in, and at each step of its eyes' and then its body's coming out of it.</summary>
    private ColourChange _hiddenColours = ColourChange.None;
    private readonly List<ColourChange> _eyeColours = [];
    private readonly List<ColourChange> _bodyColours = [];

    /// <summary>The colours of its showing itself, until it has shown itself.</summary>
    private ColourChange? _fadeColours;

    /// <summary>For each of its visuals, the colours last shown on it.</summary>
    private readonly Dictionary<ActorVisual, ColourChange> _shownColours = [];

    private string _pose = "";
    private string _posed = "";
    private int _wingPose;
    private int _wingCount = 32;
    private int _wait;
    private int _fade;
    private PixelVelocity _fireballSpeed;

    /// <summary>Its tail.</summary>
    private RidleyTail Tail { get; } = new();

    /// <summary>Where it has got to in showing itself.</summary>
    private Reveal Revealing { get; set; }

    /// <summary>Whether it has left the ground and its fight has begun.</summary>
    private bool HasLiftedOff => Revealing == Reveal.Done;

    /// <summary>Whether it is turning about, facing out of the screen.</summary>
    private bool IsTurning { get; set; }

    /// <summary>Whether it is in the middle of a roar.</summary>
    private bool IsRoaring { get; set; }

    /// <summary>How its claws hang.</summary>
    private Claws ClawHeight { get; set; }

    /// <summary>Whether its last move was stopped by the edge of where it may fly.</summary>
    private bool HitBounds { get; set; }

    /// <summary>Whether it flies and its tail moves; off while it is still unseen, and once it has broken up.</summary>
    private bool IsInMotion { get; set; }

    /// <summary>Whether its body and tail pass through Samus and her shots.</summary>
    private bool IsIntangible { get; set; } = true;

    /// <summary>Whether it carries Samus in its claws.</summary>
    private bool CarriesSamus => !OnCeres && _holdsSamus;

    /// <summary>The way it faces, as its timelines name it.</summary>
    private string Side => Facing == Facing.Right ? "right" : "left";

    /// <summary>Where its claws are.</summary>
    private PixelPosition ClawPosition => Position + PixelOffset.FromWhole(
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
        _poseBoxes.Clear();
        foreach ((string picture, DataNode boxes) in data.Get("pose_boxes").Entries())
            _poseBoxes[picture] = [.. boxes.Items().Select(box => new PixelRect(box.Int("left"), box.Int("top"), box.Int("width"), box.Int("height")))];
        DataNode bounds = data.Get("flight_bounds");
        FlightBounds = new Rect2I(bounds.Int("left"), bounds.Int("top"), bounds.Int("right") - bounds.Int("left"), bounds.Int("bottom") - bounds.Int("top"));
        DataNode colours = data.Get("colours");
        _hiddenColours = ColourChange.Read(colours.Get("hidden"));
        _eyeColours.Clear();
        _eyeColours.AddRange(colours.List("eyes").Select(ColourChange.Read));
        _bodyColours.Clear();
        _bodyColours.AddRange(colours.List("body").Select(ColourChange.Read));
        if (_eyeColours.Count != FadeSteps || _bodyColours.Count != FadeSteps - 1)
            throw colours.Problem("must give its eyes' colours for each step of their showing, and its body's for each step of its showing but the last");
        if (OnCeres)
        {
            _retreatColours = ColourChange.Read(data.Get("retreat_colours"));
            _ownColours = ColourChange.Read(colours.Get("own"));
            ReadCeresBands(data);
        }
        else
        {
            ReadNorfairValues(data);
        }
    }

    /// <summary>The colours it is in.</summary>
    private ColourChange Colours => _fadeColours ?? (OnCeres ? _ceresBandColours ?? _ownColours : _band.Colours);

    /// <summary>
    /// Shows the colours it is in on each of its visuals, at once. A change is worked out for a visual by the pictures it
    /// shows, so one that shows none yet is shown them once it does.
    /// </summary>
    private void ShowColours()
    {
        ColourChange colours = Colours;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
        {
            if (visual.Picture.Length == 0 || (_shownColours.TryGetValue(visual, out ColourChange? shown) && ReferenceEquals(shown, colours)))
                continue;
            colours.ShowOn(visual, OwnColours);
            _shownColours[visual] = colours;
        }
    }

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step)
    {
        _pose = step.Picture;
        return step.Picture;
    }

    /// <summary>Puts the parts of it that can be touched and shot where the pose it shows has them.</summary>
    private void PlacePose()
    {
        if (_pose == _posed)
            return;
        _posed = _pose;
        if (!_poseBoxes.TryGetValue(_pose, out List<PixelRect>? boxes))
            throw new InvalidOperationException($"Ridley's data gives no parts to touch and shoot for its picture \"{_pose}\".");
        foreach (Collider collider in EntityNode.CollidersOf(this))
        {
            string name = collider.Name.ToString();
            if (!name.StartsWith("Part", StringComparison.Ordinal))
                continue;
            string number = name["Part".Length..];
            if (number.EndsWith("Touch", StringComparison.Ordinal))
                number = number[..^"Touch".Length];
            int part = int.Parse(number, System.Globalization.CultureInfo.InvariantCulture) - 1;
            collider.Enabled = part < boxes.Count;
            if (part < boxes.Count)
                collider.Rect = boxes[part];
        }
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part) => OnCeres ? CeresTakeHit(hit, part) : base.TakeHit(hit, part);

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
        // The map is hidden for as long as his room lasts, and nothing of it is explored there.
        Room.MapShown = false;
        if (OnCeres)
            EnterCeres();
        else
            EnterNorfair();

        // It arrives unseen: every colour of it shown wholly see-through.
        _fadeColours = _hiddenColours;
        ShowColours();
    }

    /// <inheritdoc/>
    protected override void Behave() => BehaveAs(hurt: false);

    /// <summary>
    /// Hurt, it is not held still: the cartridge runs its own hurt behaviour in place of its behaviour, which in
    /// Norfair is the same at half the pace, its tail and its colours on every update.
    /// </summary>
    protected override void WhileHurt() => BehaveAs(hurt: true);

    /// <summary>One update of it, in its hurt or out of it.</summary>
    private void BehaveAs(bool hurt)
    {
        PlacePose();
        if (OnCeres)
            BehaveOnCeres();
        else
            BehaveInNorfair(hurt);
        ShowColours();
    }

    /// <summary>Called as its roar begins, before it takes off.</summary>
    private void OnOpeningRoar()
    {
        if (OnCeres)
            _roarHold = RoarHoldUpdates;
        else
            _revealLeft = ArenaRevealUpdates;
    }

    /// <summary>Whether it may take off yet, asked each update once its roar has begun.</summary>
    private bool MayTakeOff() => OnCeres ? --_roarHold <= 0 : NorfairMayTakeOff();

    /// <summary>Called as it leaves the ground.</summary>
    private void OnLiftOff()
    {
        if (OnCeres)
            _ceresPhase = CeresPhase.LiftoffAccelerating;
        else
            _phase = Phase.EnterArena;
    }

    /// <summary>Asked at the points where its fireballing may be cut short: the timeline to play instead, or none.</summary>
    private string? CutFireballingShort() => OnCeres ? CeresCutFireballingShort() : null;

    /// <summary>Its showing itself, one update of it. Its fight calls this until it has lifted off.</summary>
    private void StepReveal()
    {
        switch (Revealing)
        {
            case Reveal.Delay:
                if (--_wait <= 0)
                    (Revealing, _fade) = (Reveal.FadeInEyes, 0);
                break;

            case Reveal.FadeInEyes:
                // Its eyes fade in a step at a time, and then glow alone a while.
                _fadeColours = _eyeColours[Math.Min(FadeSteps - 1, _fade / EyeFadeStepUpdates)];
                if (++_fade < FadeSteps * EyeFadeStepUpdates + EyeHoldUpdates)
                    break;
                (Revealing, _fade) = (Reveal.FadeInBody, 0);
                IsInMotion = true;
                break;

            case Reveal.FadeInBody:
                // The rest of it fades in a step at a time; its last step is its own colours. Each step, the first
                // too, shows on the last of its updates, as the old game's fade starts a step's length late.
                if (++_fade % BodyFadeStepUpdates != 0)
                    break;
                int bodyStep = _fade / BodyFadeStepUpdates - 1;
                if (bodyStep < FadeSteps - 1)
                {
                    _fadeColours = _bodyColours[bodyStep];
                    break;
                }

                _fadeColours = null;
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
    private void OnOwnCue(string[] cue) =>
        throw new InvalidOperationException($"Ridley's timeline \"{Playing}\" gives it the cue \"{string.Join(':', cue)}\", which it does not know.");

    /// <summary>Where its fireballs leave its mouth.</summary>
    private PixelPosition Muzzle => Position + PixelOffset.FromWhole(Facing == Facing.Left ? -25 : 25, -43);

    /// <summary>
    /// Aims its next fireball at Samus, within the arc ahead of and below it. Angles here are in 256ths of a turn from
    /// straight down, growing toward the right: the cartridge's aim a quarter turn on.
    /// </summary>
    private void AimFireball()
    {
        // The cartridge aims from whole pixels to whole pixels.
        PixelPosition samus = Room.Player.Position, muzzle = Muzzle;
        int angle = Turns.Wrap(CartridgeAngles.AimAngle(samus.X.Whole - muzzle.X.Whole, samus.Y.Whole - muzzle.Y.Whole) + Turns.PerTurn / 4);
        if (Facing == Facing.Left)
            angle = angle is < 64 or >= 235 ? 235 : Math.Max(angle, 176);
        else
            angle = angle is < 21 or >= 192 ? 21 : Math.Min(angle, 80);
        _fireballSpeed = AlongHeading(In256ths(Shot("fireball").Speed), angle);
    }

    /// <summary>
    /// A speed in 256ths of a pixel split along a heading from straight down toward the right, as the cartridge splits
    /// it: across by the heading's sine, down by the sine a quarter turn on.
    /// </summary>
    private static PixelVelocity AlongHeading(int speedIn256ths, int heading) => new(
        From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, heading)),
        From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, heading + Turns.PerTurn / 4)));

    /// <summary>A distance in the 256ths of a pixel the cartridge holds a speed in, what is finer dropped.</summary>
    private static int In256ths(Pixels distance) => distance.Whole * 256 + distance.Fraction / 256;

    /// <summary>A distance from a count of 256ths of a pixel.</summary>
    private static Pixels From256ths(int count) => Pixels.FromParts(count >> 8, (count & 255) * 256);

    /// <summary>Starts it on a timeline that has one for each way it faces.</summary>
    private void PlayFacing(string timeline) => Play($"{timeline}_{Side}", restart: true);

    /// <summary>
    /// Pulls its speed toward a place, each way by a step that grows with the distance, the smaller the divisor the
    /// harder; a speed carrying it away from the place is checked first.
    /// </summary>
    private void FlyToward(PixelPosition target, int divisor, Pixels reversalBoost = default)
    {
        divisor = Math.Clamp(divisor, 1, 16);
        Velocity = new PixelVelocity(
            Seek(Velocity.X, Position.X.Whole, target.X.Whole, divisor, reversalBoost),
            Seek(Velocity.Y, Position.Y.Whole, target.Y.Whole, divisor, reversalBoost));
    }

    /// <remarks>
    /// The cartridge adds the steps one at a time (<see cref="SeekingSteps.Raise"/>, <see cref="SeekingSteps.Lower"/>); the reversal's boost and
    /// its check each start afresh, dropping any unit owed before them.
    /// </remarks>
    private static Pixels Seek(Pixels speed, int at, int target, int divisor, Pixels reversalBoost)
    {
        int fromTarget = at - target;
        if (fromTarget == 0)
            return speed;
        Pixels step = Pixels.FromNumber(Math.Max(1, Math.Abs(fromTarget) / divisor) / 256.0);
        Pixels reversal = Pixels.FromNumber(0.03125);
        bool owed = WrapReversesOrder(at, target);
        if (fromTarget > 0)
        {
            if (!speed.IsNegative)
            {
                owed = false;
                Lower(ref speed, ref owed, reversalBoost);
                owed = false;
                Lower(ref speed, ref owed, reversal);
                Lower(ref speed, ref owed, step);
            }

            Lower(ref speed, ref owed, step);
            return speed < -MostSpeed ? -MostSpeed : speed;
        }

        if (speed.IsNegative)
        {
            owed = false;
            Raise(ref speed, ref owed, reversalBoost);
            owed = false;
            Raise(ref speed, ref owed, reversal);
            Raise(ref speed, ref owed, step);
        }

        Raise(ref speed, ref owed, step);
        return speed >= MostSpeed ? MostSpeed : speed;
    }

    /// <summary>
    /// Pulls its speed toward a place as <see cref="FlyToward"/> does, but more simply: a speed carrying it away from
    /// the place is checked by three steps and no more.
    /// </summary>
    private void FlyPlainlyToward(PixelPosition target, int divisor)
    {
        divisor = Math.Clamp(divisor, 1, 16);
        Velocity = new PixelVelocity(
            SeekPlainly(Velocity.X, Position.X.Whole, target.X.Whole, divisor),
            SeekPlainly(Velocity.Y, Position.Y.Whole, target.Y.Whole, divisor));
    }

    /// <remarks>The cartridge adds the steps one at a time, as <see cref="Seek"/> does, with nothing starting afresh.</remarks>
    private static Pixels SeekPlainly(Pixels speed, int at, int target, int divisor)
    {
        int fromTarget = at - target;
        if (fromTarget == 0)
            return speed;
        Pixels step = Pixels.FromNumber(Math.Max(1, Math.Abs(fromTarget) / divisor) / 256.0);
        bool owed = WrapReversesOrder(at, target);
        if (fromTarget < 0)
        {
            if (speed.IsNegative)
            {
                Raise(ref speed, ref owed, step);
                Raise(ref speed, ref owed, step);
            }

            Raise(ref speed, ref owed, step);
            return speed > MostSpeed ? MostSpeed : speed;
        }

        if (!speed.IsNegative)
        {
            Lower(ref speed, ref owed, step);
            Lower(ref speed, ref owed, step);
        }

        Lower(ref speed, ref owed, step);
        return speed < -MostSpeed ? -MostSpeed : speed;
    }

    /// <summary>
    /// Sets its speed along a heading that turns toward a target and stops there, with its speed moving toward a speed.
    /// Headings are in 256ths of a 256th of a turn, from straight down toward the right.
    /// </summary>
    private void SetSwoopSpeed(int turn, bool clockwise, int target, Pixels speed)
    {
        Pixels step = Pixels.FromNumber(0.125);
        _swoopSpeed = _swoopSpeed < speed
            ? (_swoopSpeed + step > speed ? speed : _swoopSpeed + step)
            : (_swoopSpeed - step < speed ? speed : _swoopSpeed - step);
        int full = Turns.PerTurn * 256;
        int heading = SwoopHeading + (clockwise ? -turn : turn);
        int difference = ((heading - target) % full + full + full / 2) % full - full / 2;
        if (clockwise ? difference < 0 : difference >= 0)
            heading = target;
        SwoopHeading = heading;
        Velocity = AlongHeading(In256ths(_swoopSpeed), Turns.Wrap(heading >> 8));
    }

    /// <summary>The heading of its swoop.</summary>
    private int SwoopHeading { get; set; }

    private Pixels _swoopSpeed;

    /// <summary>Whether its middle is within a reach of a place, allowing for its own size.</summary>
    private bool IsNear(PixelPosition place, int reachAcross, int reachDown)
    {
        PixelRect body = Body?.Rect ?? default;
        return Math.Abs(Position.X.Whole - place.X.Whole) < body.Width / 2 + reachAcross + 1 &&
            Math.Abs(Position.Y.Whole - place.Y.Whole) < body.Height / 2 + reachDown + 1;
    }

    /// <summary>
    /// Moves it by its speed, stopping it dead at the edges of where it may fly, beats its wings, and moves its tail.
    /// Its fight calls this each update that it is in motion.
    /// </summary>
    private void StepBody()
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
    private void OnMoving(PixelPosition proposed)
    {
        if (OnCeres)
            ShakeOnHittingWall(proposed);
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
            // Its wings flash with its body.
            if (Visual is { } body)
                wings.Tint(body.Modulate);
        }
    }

    /// <summary>Steers the tail by what it is doing: as it sways and lashes, or through its part in the pogo.</summary>
    private void SteerTail()
    {
        if (OnCeres || Tail.Action is RidleyTail.Doing.None or RidleyTail.Doing.Neutral)
            SteerTailPlainly();
        else
            StepPogoTail();
    }

    /// <summary>Steers the tail as it sways and lashes.</summary>
    private void SteerTailPlainly()
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
    private bool TailMayWhip => OnCeres ? _fighting : _mode != Mode.NotFighting;

    /// <summary>Aims a lash of its tail at Samus, or at a missile of hers that is close to its tip.</summary>
    private void AimTailWhip(int turnsBeyond)
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
        // The cartridge aims from whole pixels to whole pixels, a quarter turn on so as to count from straight down.
        PixelPosition root = Tail.Base.Position;
        int toward = CartridgeAngles.AimAngle(aim.X.Whole - root.X.Whole, aim.Y.Whole - root.Y.Whole);
        Tail.AimWhip(Turns.Wrap(toward + Turns.PerTurn / 4), Facing == Facing.Right, turnsBeyond);
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
            visual.Show(index < _tailPictures.Count ? _tailPictures[index] : _tailTipPictures[TailTipPicture]);
            // Its tail flashes with its body.
            if (Visual is { } shown)
                visual.Tint(shown.Modulate);
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

    /// <summary>Which of its tail tip's pictures shows, by the way the tip points.</summary>
    private int TailTipPicture => Turns.Wrap(Tail.Tip.Pointing - TailTipArcStart) * _tailTipPictures.Count / Turns.PerTurn;

    /// <summary>Puts one of the armoured places of its tail where the tail is.</summary>
    private void PlaceArmour(string name, PixelPosition at, int reach)
    {
        if (GetNodeOrNull<Collider>(name) is not { } collider)
            return;
        PixelOffset from = at - Position;
        collider.Rect = new PixelRect(from.X.Whole - reach, from.Y.Whole - reach, reach * 2, reach * 2);
    }
}
