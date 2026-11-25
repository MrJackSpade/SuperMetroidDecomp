using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Torizo: a Chozo statue that comes to life. Bomb Torizo sits holding the Bombs until its hand crumbles; Golden
/// Torizo drops from above when Samus walks under it. Both walk at her, turning when she gets behind them, and from
/// their timelines spew orbs, throw sonic booms and jump back when she comes close. Bomb Torizo swipes at her when
/// she is behind it, and loses its gut and then its face as its health runs down. Golden Torizo dodges missiles,
/// catches super missiles and throws them back, sits on her when she rolls behind it, leaps at her when she keeps
/// jumping, fires beams from its eyes, staggers when stunned, and near death lets eggs out of its chest.
/// </summary>
/// <remarks>
/// <para>Its timelines run it. Their cues, with parts after colons, are: "stage:" and a stage's name; "motion:" and
/// "still", "walking", "standing" or "leaping"; "stepped:" with the way it faces and the foot that leads; "walk:"
/// with the pixels it moves ahead, less than nothing for left; "shift:" with pixels across and down; "play:" and a
/// timeline; "turn:" with the timeline for when it faces right and the one for left; "land_in:" and the timeline it
/// lands with; "landed:" with the timelines for a faceless one and for a golden one, either of which may be empty;
/// "attack:" with the timelines for spewing and for sonic booms; "branch:" and "jump_back", "sit" or "leap";
/// "interrupt:" with "swipe", "stagger", "eye_beam" or "eggs" and a timeline; "look:" and a look; "fade:" and a
/// tint's name; "swipe:" with the hand's place when it faces left; "explosions:" and "gut" or "face"; "box:" with
/// the left, top, width and height of the part of it that can be touched and shot, from its place, and 1 if that
/// part is one of its sitting and standing-up poses or 0 if not, or "box:off" for none at all; and by themselves
/// "advance_fade", "show", "hide", "lock", "unlock", "resume_walk",
/// "resume_interrupted", "blow_up_gut", "face_blown_up", "death_explosion", "finish_death", "landing_dust", "drool",
/// "turning_around", "rising", "dropping", "start_fight", "orbs", "sonic_boom", "egg", "chest_held_open",
/// "super_missile", "clear_caught", "eye_beam", "eye_beam_flames:on", "eye_beam_flames:off" and "unstun".</para>
/// <para>The names of its pictures may hold "{look}", which stands for the look its body has; a step with no
/// picture leaves it as it was shown. Its data lists its "tints" by name, each as how much of the red, green and
/// blue of its pictures shows ("r", "g", "b"), and for a golden one "health_tints", each with the least "health" it
/// is for, highest first. "scattered_drops" is the table of what it scatters when it is gone, and "drop_scatter"
/// where: a corner of the room ("x", "y") and, across and down, how many binary digits of a number drawn are added
/// and from which digit up. "fight_body", where given, is the body it has from the start of the fight. Its shots are
/// "orb", "sonic_boom", "initial_drool", "drool_0" to "drool_2", "swipe", "low_health_explosion", "death_explosion",
/// "landing_dust_left" and "landing_dust_right", and for a golden one "egg", "super_missile" and "eye_beam".</para>
/// </remarks>
public partial class Torizo : Enemy
{
    /// <summary>Whether it is the golden one.</summary>
    [Export]
    public bool Golden { get; set; }

    /// <summary>The music as it wakes, by name.</summary>
    [Export]
    public string AwakeningMusic { get; set; } = "";

    /// <summary>The music of the fight, by name.</summary>
    [Export]
    public string FightMusic { get; set; } = "";

    /// <summary>The music once it is dead, by name.</summary>
    [Export]
    public string VictoryMusic { get; set; } = "";

    /// <summary>The timeline it starts on.</summary>
    [Export, DataValue("start_timeline")]
    public string StartTimeline { get; set; } = "";

    /// <summary>The timeline of its waking.</summary>
    [Export, DataValue("awakening_timeline")]
    public string AwakeningTimeline { get; set; } = "";

    /// <summary>The look its body starts with.</summary>
    [Export, DataValue("start_look")]
    public string StartLook { get; set; } = "";

    /// <summary>The tint it starts with, by name.</summary>
    [Export, DataValue("start_tint")]
    public string StartTint { get; set; } = "";

    /// <summary>How many times it must be told to move its fade on for one to be done.</summary>
    [Export, DataValue("fade_steps")]
    public int FadeSteps { get; set; } = 16;

    /// <summary>
    /// The last value of the fades' shared count at which a step of his body's fade counts as one: the step after
    /// completes a fade by that count, whatever his own steps show.
    /// </summary>
    private const int SharedFadeLastStep = 13;

    /// <summary>Below this health it drools, and Bomb Torizo's gut blows up.</summary>
    [Export, DataValue("gut_health")]
    public int GutHealth { get; set; } = 350;

    /// <summary>Below this health Bomb Torizo's face blows up.</summary>
    [Export, DataValue("face_health")]
    public int FaceHealth { get; set; } = 100;

    /// <summary>With fewer missiles than this, Samus always gets its first attack.</summary>
    [Export, DataValue("first_attack_missiles")]
    public int FirstAttackMissiles { get; set; } = 5;

    /// <summary>How many updates it walks on with Samus behind it before it turns.</summary>
    [Export, DataValue("turn_delay")]
    public int TurnDelay { get; set; } = 72;

    /// <summary>How many orbs it spews at a time.</summary>
    [Export, DataValue("orbs_per_burst")]
    public int OrbsPerBurst { get; set; } = 3;

    /// <summary>The speed across of an orb as it is spewed, before chance has its say, as a number of pixels.</summary>
    [Export, DataValue("orb_speed_across")]
    public double OrbSpeedAcross { get; set; } = 1.5625;

    /// <summary>The upward speed of an orb as it is spewed, before chance has its say.</summary>
    [Export, DataValue("orb_speed_up")]
    public double OrbSpeedUp { get; set; } = 1.625;

    /// <summary>How near in front Samus must be for it to jump back, in pixels.</summary>
    [Export, DataValue("close_in_front")]
    public int CloseInFront { get; set; } = 32;

    /// <summary>How near behind Samus must be for Bomb Torizo to swipe, in pixels.</summary>
    [Export, DataValue("close_behind")]
    public int CloseBehind { get; set; } = 56;

    /// <summary>The golden one wakes when Samus is right of here and below it, in the room's pixels.</summary>
    [Export]
    public Vector2I WakeCorner { get; set; } = new(368, 320);

    /// <summary>How far down the room the golden one comes to rest, in pixels.</summary>
    [Export]
    public int FloorY { get; set; } = 375;

    /// <summary>At this health or less the golden one may let out its eggs.</summary>
    [Export, DataValue("eggs_health")]
    public int EggsHealth { get; set; } = 1928;

    /// <summary>Above this health the golden one staggers when stunned.</summary>
    [Export, DataValue("stagger_health")]
    public int StaggerHealth { get; set; } = 10800;

    /// <summary>How long Samus must have been space jumping, in updates, for the golden one to leap at her whatever she does.</summary>
    [Export, DataValue("space_jump_updates")]
    public int SpaceJumpUpdates { get; set; } = 360;

    /// <summary>How many steps the golden one takes before it jumps back wherever Samus is.</summary>
    [Export, DataValue("repeated_jump_steps")]
    public int RepeatedJumpSteps { get; set; } = 8;

    /// <summary>How hard the room shakes when it lands.</summary>
    [Export, DataValue("landing_shake_strength")]
    public int LandingShakeStrength { get; set; } = 2;

    /// <summary>How many things it drops when it is gone.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>The farthest down it ever feels for the ground as it walks, in pixels.</summary>
    private const int FarthestGroundReach = 15;

    private enum Stage
    {
        Idle,
        Falling,
        WaitForHand,
        Active,
        GoldenWait,
        GoldenActive,
    }

    private enum Motion
    {
        Still,
        Walking,
        Standing,
        Leaping,
    }

    private enum Breakage
    {
        Whole,
        GutBlownUp,
        GutAndFaceBlownUp,
    }

    private static readonly Pixels RestingFall = Pixels.FromWhole(1);
    private static readonly Pixels StatueGravity = Pixels.FromNumber(0.15625);
    private static readonly Pixels LeapGravity = Pixels.FromNumber(0.15625);

    private readonly Dictionary<string, Color> _tints = new(StringComparer.Ordinal);
    private readonly List<(int Health, Color Tint)> _healthTints = [];
    private readonly List<Projectile> _eggs = [];
    private Stage _stage = Stage.Falling;
    private Motion _motion;
    private Breakage _breakage;
    private Facing _leadingFoot = Facing.Left;
    private bool _turning;
    private bool _locked;
    private bool _sitting;
    private bool _caught;
    private bool _stunned;
    private bool _dying;
    private bool _fighting;
    private bool _restingFall;
    private int _decisions;
    /// <summary>
    /// How far down it feels for the ground as it walks, in pixels: one more than its stride, and as far as it ever
    /// feels from a leap until its next step, the speed of the leap being what the old game measures it by.
    /// </summary>
    private int _groundReach = 1;
    private int _spaceJumping;
    private Countdown? _turnTowardSamus;
    private Pixels _jumpGravity;
    private string _landing = "";
    private string _look = "";
    private TimelineMark _interruptedWalk;
    private TimelineMark _interrupted;
    private Color _tint = Colors.White;
    private Color _fadeFrom = Colors.White;
    private Color _fadeTo = Colors.White;
    private int _fadeStep;
    private string _shown = "";
    private DropTable _scatteredDrops = DropTable.Nothing;
    private Vector2I _dropCorner;
    private (int From, int Digits) _dropAcross, _dropDown;
    private PixelRect? _fightBody;

    /// <summary>Whether the flames its eye beams leave on the floor are let burn at the moment.</summary>
    public bool EyeBeamFlames { get; private set; }

    /// <summary>Whether it faces right.</summary>
    public bool FacesRight => Facing == Facing.Right;

    /// <summary>Gives its body a look.</summary>
    public void ShowLook(string look) => _look = look;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        static Color Tint(DataNode tint) => new(
            (float)tint.Distance("r").ToNumber(), (float)tint.Distance("g").ToNumber(), (float)tint.Distance("b").ToNumber());
        static (int From, int Digits) Field(DataNode field) => (field.Int("from"), field.Int("digits"));
        _tints.Clear();
        foreach ((string name, DataNode tint) in data.Get("tints").Entries())
            _tints[name] = Tint(tint);
        _healthTints.Clear();
        if (data.Has("health_tints"))
            _healthTints.AddRange(data.List("health_tints").Select(band => (band.Int("health"), Tint(band.Get("tint")))));
        _scatteredDrops = DropTable.Read(data.Get("scattered_drops"));
        DataNode scatter = data.Get("drop_scatter");
        _dropCorner = new Vector2I(scatter.Int("x"), scatter.Int("y"));
        _dropAcross = Field(scatter.Get("across"));
        _dropDown = Field(scatter.Get("down"));
        _fightBody = null;
        if (data.Has("fight_body"))
        {
            DataNode body = data.Get("fight_body");
            _fightBody = new PixelRect(body.Int("left"), body.Int("top"), body.Int("width"), body.Int("height"));
        }
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !_dying && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override Color RestingTint
    {
        get
        {
            if (!_fighting || _dying || _healthTints.Count == 0)
                return _tint;
            // The golden one darkens as its health runs down.
            foreach ((int health, Color tint) in _healthTints)
            {
                if (Health.Current >= health)
                    return tint;
            }

            return _healthTints[^1].Tint;
        }
    }

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step)
    {
        // A step with no picture is a wait: it leaves the Torizo as it was shown.
        if (step.Picture.Length > 0)
            _shown = step.Picture;
        return _shown.Replace("{look}", _look, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        Facing = Facing.Left;
        _look = StartLook;
        _shown = "";
        SetBox(null, sitting: false);
        _tint = Tint(StartTint);
        _motion = Motion.Still;
        Velocity = PixelVelocity.Zero;
        FallAtRest();
        if (Solid is { } solid)
            solid.Enabled = false;
        Play(StartTimeline, restart: true);
        if (Golden)
            GrantSecretCode();
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        bool bomb = hit.Carries(new HitKind(HitKinds.Bomb));
        bool shot = !bomb && IsProjectileHit(hit);
        if (shot && !Golden && (_sitting || IsFlashing || _locked))
            return HitResult.Absorbed;
        if (shot && Golden)
        {
            if (IsFlashing || (_sitting && _locked))
                return HitResult.Absorbed;
            if (!_sitting && !_locked)
            {
                if (_caught)
                {
                    _stunned = true;
                }
                else if (hit.Carries(new HitKind(HitKinds.Missile)))
                {
                    // A missile it sidesteps: the missile flies on.
                    _stage = Stage.Idle;
                    Play(FacesRight ? "golden_dodge_turning_right" : "golden_dodge_turning_left", restart: true);
                    return HitResult.Ignored;
                }
                else if (hit.Carries(new HitKind(HitKinds.SuperMissile)))
                {
                    if (FacesSamus())
                    {
                        // A super missile from in front it catches, to throw back.
                        _caught = true;
                        _stage = Stage.Idle;
                        string leg = (_leadingFoot != Facing) == FacesRight ? "left" : "right";
                        Play($"golden_facing_{Side}_catching_super_missile_{leg}_leg_forward", restart: true);
                        return HitResult.Absorbed;
                    }
                }
                else
                {
                    _stunned = true;
                }
            }
        }

        if (bomb)
        {
            if (IsFlashing || (!Golden && (_sitting || _locked)) || (Golden && _sitting && _locked))
                return HitResult.Ignored;
            if (Golden && !_sitting && !_locked)
                _stunned = true;
        }

        return base.TakeHit(hit, part);
    }

    /// <summary>Whether a hit is a shot's: a beam's, a missile's or a super missile's.</summary>
    private static bool IsProjectileHit(Hit hit) => hit.CarriesAny(
    [
        new HitKind(HitKinds.Beam), new HitKind(HitKinds.Charged), new HitKind(HitKinds.Ice), new HitKind(HitKinds.Wave),
        new HitKind(HitKinds.Spazer), new HitKind(HitKinds.Plasma), new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile),
    ]);

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        BeginDeath();
        return false;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!_dying)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        if (Golden)
            _spaceJumping = samus.State is SpinJumpingState && samus.Inventory.IsOn(Equipment.SpaceJump) ? _spaceJumping + 1 : 0;

        switch (_stage)
        {
            case Stage.Idle:
                break;

            case Stage.Falling:
                MaybeDrool();
                ApplyGravity();
                break;

            case Stage.WaitForHand:
                if (Solid is { } solid)
                    solid.Enabled = true;
                if (Room.GetChildren().OfType<BombTorizoHand>().Any(hand => !Room.IsLeaving(hand)))
                    break;
                if (AwakeningMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(AwakeningMusic);
                if (Solid is { } body)
                    body.Enabled = false;
                Play(AwakeningTimeline, restart: true);
                break;

            case Stage.Active:
                MaybeDrool();
                if (_breakage == Breakage.Whole && Health.Current < GutHealth)
                {
                    _interrupted = MarkTimeline();
                    Play("bomb_torizo_gut_blowing_up", restart: true);
                    break;
                }

                if (_breakage != Breakage.GutAndFaceBlownUp && Health.Current < FaceHealth)
                {
                    Play("bomb_torizo_face_blowing_up", restart: true);
                    break;
                }

                MoveBody();
                break;

            case Stage.GoldenWait:
                if (samus.Position.Y.Whole > WakeCorner.Y && samus.Position.X.Whole > WakeCorner.X)
                    Play(AwakeningTimeline, restart: true);
                break;

            case Stage.GoldenActive:
                MaybeDrool();
                MoveBody();
                break;

            default:
                throw new InvalidOperationException($"A Torizo has no stage {_stage}.");
        }
    }

    /// <summary>Moves its body as what it is doing says.</summary>
    private void MoveBody()
    {
        switch (_motion)
        {
            case Motion.Still:
                break;

            case Motion.Walking:
                if (_turnTowardSamus is { } wait)
                {
                    wait = wait.Stepped();
                    _turnTowardSamus = wait;
                    if (wait.IsOver)
                    {
                        _turnTowardSamus = null;
                        Play(TurnAbout(), restart: true);
                        break;
                    }
                }

                // It feels for the ground under its stride; with none there it falls.
                if (!MoveDownBy(Pixels.FromWhole(_groundReach)))
                {
                    Play($"facing_{Side}_falling", restart: true);
                    Velocity = PixelVelocity.Zero;
                    _groundReach = 1;
                    FallAtRest();
                }

                break;

            case Motion.Standing:
                ApplyGravity();
                break;

            case Motion.Leaping:
                MoveAcrossBy(Velocity.X);
                if (MoveDownBy(Velocity.Y))
                {
                    Play(_landing, restart: true);
                    FallAtRest();
                    Room.Shake(LandingShakeStrength, 32, ShakeWay.Down);
                    break;
                }

                SetFall(Velocity.Y + _jumpGravity);
                break;

            default:
                throw new InvalidOperationException($"A Torizo has no motion {_motion}.");
        }
    }

    /// <summary>Lets it fall, faster and faster; landing from a real fall shakes the room.</summary>
    private void ApplyGravity()
    {
        if (MoveDownBy(Velocity.Y))
        {
            if (!Velocity.Y.IsNegative && !_restingFall)
            {
                Room.Shake(LandingShakeStrength, 32, ShakeWay.Down);
                FallAtRest();
            }

            return;
        }

        SetFall(Velocity.Y + StatueGravity);
    }

    /// <summary>Sets its speed down; any speed but the one it rests at means it is really falling.</summary>
    private void SetFall(Pixels speed)
    {
        if (speed != Velocity.Y)
            _restingFall = false;
        Velocity = Velocity.WithY(speed);
    }

    /// <summary>Leaves it pressing down on the ground at rest.</summary>
    private void FallAtRest()
    {
        Velocity = Velocity.WithY(RestingFall);
        _restingFall = true;
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        Player samus = Room.Player;
        switch (parts[0])
        {
            case "stage":
                _stage = parts[1] switch
                {
                    "idle" => Stage.Idle,
                    "falling" => Stage.Falling,
                    "wait_for_hand" => Stage.WaitForHand,
                    "active" => Stage.Active,
                    "golden_wait" => Stage.GoldenWait,
                    "golden_active" => Stage.GoldenActive,
                    _ => throw new InvalidOperationException($"A Torizo has no stage \"{parts[1]}\"."),
                };
                break;

            case "motion":
                _motion = parts[1] switch
                {
                    "still" => Motion.Still,
                    "walking" => Motion.Walking,
                    "standing" => Motion.Standing,
                    "leaping" => Motion.Leaping,
                    _ => throw new InvalidOperationException($"A Torizo has no motion \"{parts[1]}\"."),
                };
                break;

            case "stepped":
                Facing = SideNamed(parts[1]);
                _leadingFoot = SideNamed(parts[2]);
                _turning = false;
                _decisions++;
                break;

            case "walk":
                int stride = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                _groundReach = Math.Min(Math.Abs(stride) + 1, FarthestGroundReach);
                if (MoveAcrossBy(Pixels.FromWhole(stride)))
                {
                    // A wall turns it about at once.
                    _turnTowardSamus = null;
                    Play(TurnAbout(), restart: true);
                }
                else if (FacesAwayFromSamus())
                {
                    _turnTowardSamus ??= Countdown.Of(TurnDelay);
                }

                break;

            case "shift":
                Position += PixelOffset.FromWhole(
                    int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture),
                    int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                break;

            case "play":
                Play(parts[1], restart: true);
                break;

            case "turn":
                Play(FacesRight ? parts[1] : parts[2], restart: true);
                break;

            case "land_in":
                _landing = parts[1];
                break;

            case "landed":
                // A faceless one and a golden one each walk on their own way from a landing; the others carry on.
                if (_breakage == Breakage.GutAndFaceBlownUp)
                    Play(parts[1], restart: true);
                else if (Golden)
                    Play(parts[2], restart: true);
                break;

            case "attack":
                bool first = samus.Inventory.Amount(Amounts.Missiles) < FirstAttackMissiles ||
                    ((Room.Session.Clock.Updates + (samus.Position.X.Whole + 1) / 2) & 8) != 0;
                Play(first ? parts[1] : parts[2], restart: true);
                break;

            case "branch":
                Branch(parts[1], samus);
                break;

            case "interrupt":
                Interrupt(parts[1], parts[2], samus);
                break;

            case "look":
                _look = parts[1];
                break;

            case "fade":
                (_fadeFrom, _fadeTo, _fadeStep) = (_tint, Tint(parts[1]), 0);
                break;

            case "advance_fade":
                // Each step is also one of the count every gradual fade shares, which his waking leaves at one.
                Room.Session.Service<SharedFadeCount>().TakeStep(SharedFadeLastStep);
                if (_fadeStep < FadeSteps)
                    _tint = _fadeFrom.Lerp(_fadeTo, (float)++_fadeStep / FadeSteps);
                break;

            case "show":
                Visual?.SetShown(true);
                break;

            case "hide":
                Visual?.SetShown(false);
                break;

            case "lock":
                _locked = true;
                break;

            case "unlock":
                _locked = false;
                break;

            case "box":
                if (parts[1] == "off")
                {
                    SetBox(null, sitting: false);
                    break;
                }

                SetBox(
                    new PixelRect(
                        int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture),
                        int.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture), int.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture)),
                    sitting: parts[5] == "1");
                break;

            case "resume_walk":
                ResumeTimeline(_interruptedWalk);
                break;

            case "resume_interrupted":
                ResumeTimeline(_interrupted);
                break;

            case "swipe":
                int across = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), down = int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                Fire("swipe", Position + PixelOffset.FromWhole(FacesRight ? -across : across, down), PixelVelocity.Zero);
                break;

            case "explosions":
                PixelOffset site = parts[1] switch
                {
                    "gut" => PixelOffset.FromWhole(FacesRight ? 12 : -12, -8),
                    "face" => PixelOffset.FromWhole(FacesRight ? 16 : -16, -20),
                    _ => throw new InvalidOperationException($"A Torizo has nothing called \"{parts[1]}\" to blow up."),
                };
                for (int explosion = 0; explosion < 6; explosion++)
                    Fire("low_health_explosion", Position + site, PixelVelocity.Zero);
                FlashFor(40);
                break;

            case "blow_up_gut":
                if (_breakage == Breakage.Whole)
                    _breakage = Breakage.GutBlownUp;
                for (int piece = 0; piece < 6; piece++)
                    SpewDrool();
                break;

            case "face_blown_up":
                _breakage = Breakage.GutAndFaceBlownUp;
                break;

            case "death_explosion":
                Fire("death_explosion", Position, PixelVelocity.Zero);
                FlashFor(1);
                break;

            case "finish_death":
                FinishDeath();
                break;

            case "landing_dust":
                Fire("landing_dust_right", Position + PixelOffset.FromWhole(24, 48), PixelVelocity.Zero);
                Fire("landing_dust_left", Position + PixelOffset.FromWhole(-24, 48), PixelVelocity.Zero);
                break;

            case "drool":
                if (Health.Current < GutHealth)
                    Drool();
                break;

            case "turning_around":
                _turning = true;
                break;

            case "rising":
                // Once it is past the top of its leap it shows itself falling.
                if (!Velocity.Y.IsNegative)
                    Play(_landing, restart: true);
                break;

            case "dropping":
                if (Position.Y.Whole == FloorY)
                    Play("golden_torizo_settling", restart: true);
                break;

            case "start_fight":
                _fighting = true;
                if (_fightBody is { } fightBody && Body is { } body)
                    body.Rect = fightBody;
                if (FightMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(FightMusic);
                break;

            case "orbs":
                for (int orb = 0; orb < OrbsPerBurst; orb++)
                    SpewOrb("orb", PixelOffset.FromWhole(27, -40), Pixels.FromNumber(OrbSpeedAcross), Pixels.FromNumber(OrbSpeedUp));
                break;

            case "sonic_boom":
                PixelOffset from = Random.Draw().Part(0, 1) == 1 ? PixelOffset.FromWhole(32, -12) : PixelOffset.FromWhole(32, 20);
                Fire("sonic_boom", Position + Ahead(from), new PixelVelocity(Facing.Toward(Shot("sonic_boom").Speed), Pixels.Zero));
                break;

            case "egg":
                var egg = (TorizoEgg)SpewOrb("egg", PixelOffset.FromWhole(16, -1), Pixels.FromNumber(0.5), Pixels.FromNumber(1.5));
                egg.Laid(Facing);
                _eggs.Add(egg);
                break;

            case "chest_held_open":
                _eggs.RemoveAll(laid => !IsInstanceValid(laid) || laid.HasEnded);
                if (_eggs.Count == 0)
                    Play("golden_chest_closing", restart: true);
                break;

            case "super_missile":
                ((TorizoSuperMissile)Fire("super_missile", Position + Ahead(PixelOffset.FromWhole(30, -52)), PixelVelocity.Zero)).HeldBy(this);
                break;

            case "clear_caught":
                _caught = false;
                break;

            case "eye_beam":
                // Two draws go by unused before the one that spreads the beam.
                Random.Draw();
                Random.Draw();
                int heading = (FacesRight ? 96 : 160) + Random.Draw().Part(1, 4) - 8;
                var beam = (TorizoEyeBeam)Fire("eye_beam", Position + Ahead(PixelOffset.FromWhole(20, -30)), AlongHeading(Shot("eye_beam").Speed, heading));
                beam.FiredBy(this);
                break;

            case "eye_beam_flames":
                EyeBeamFlames = parts[1] == "on";
                break;

            case "unstun":
                _stunned = false;
                break;

            default:
                throw new InvalidOperationException($"A Torizo's timeline \"{Playing}\" gives it the cue \"{cue}\", which it does not know.");
        }
    }

    /// <summary>Takes one of the ways out of its walk that a timeline offers, if Samus is where that way calls for.</summary>
    private void Branch(string which, Player samus)
    {
        int distance = Math.Abs(samus.Position.X.Whole - Position.X.Whole);
        string foot = _leadingFoot == Facing.Right ? "right" : "left";
        switch (which)
        {
            case "jump_back" when !Golden:
                if (distance >= CloseInFront || !FacesSamus())
                    return;
                break;

            case "jump_back":
                if (_decisions < RepeatedJumpSteps && (distance >= CloseInFront || !FacesSamus()))
                    return;
                _decisions = 0;
                break;

            case "sit":
                // It sits on a Samus rolled up on the ground close behind it.
                bool rolledOnGround = samus.State is CrawlingState && samus.IsOnGround;
                if (FacesSamus() || distance < 4 || distance >= 40 || !rolledOnGround)
                    return;
                _decisions = 0;
                Play($"golden_facing_{Side}_sitting_down_attack", restart: true);
                return;

            case "leap":
                if (distance < 112 || !FacesSamus())
                    return;
                // It leaps at a Samus who has been space jumping a long while; otherwise only, as often as not, at one
                // who is steering.
                if (_spaceJumping <= SpaceJumpUpdates)
                {
                    if (!samus.Input.IsHeld(GameAction.Left) && !samus.Input.IsHeld(GameAction.Right))
                        return;
                    RandomDraw chance = Random.Draw();
                    if (chance.Part(0, 1) == 0 && chance.Part(8, 1) == 0)
                        return;
                }

                _decisions = 0;
                Velocity = new PixelVelocity(Facing.Toward(Pixels.FromWhole(2)), Velocity.Y);
                _groundReach = FarthestGroundReach;
                SetFall(Pixels.FromNumber(-5.75));
                _jumpGravity = LeapGravity;
                Play($"facing_{Side}_jumping_forward", restart: true);
                return;

            default:
                throw new InvalidOperationException($"A Torizo has no way out of its walk called \"{which}\".");
        }

        // It jumps back, to land on the foot it leads with.
        Velocity = new PixelVelocity(Facing.Opposite().Toward(Pixels.FromWhole(3)), Velocity.Y);
        _groundReach = FarthestGroundReach;
        SetFall(Pixels.FromNumber(-4.5));
        _jumpGravity = LeapGravity;
        Play($"facing_{Side}_jumping_backward_to_{foot}_foot", restart: true);
    }

    /// <summary>Breaks off its walk for something else, if that is called for; it picks the walk up again afterwards.</summary>
    private void Interrupt(string which, string timeline, Player samus)
    {
        int distance = Math.Abs(samus.Position.X.Whole - Position.X.Whole);
        switch (which)
        {
            case "swipe":
                if (distance >= CloseBehind || FacesAwayFromSamus())
                    return;
                break;

            case "stagger":
                if (Health.Current <= StaggerHealth || !_stunned)
                    return;
                break;

            case "eye_beam":
                if (!FacesSamus() || distance < 32 || distance >= 96)
                    return;
                RandomDraw chance = Random.Draw();
                if (chance.Part(4, 1) != 0 || chance.Part(8, 1) != 0)
                    return;
                break;

            case "eggs":
                if (Health.Current > EggsHealth)
                    return;
                RandomDraw luck = Random.Draw();
                if (luck.Part(1, 1) != 0 || luck.Part(8, 1) != 0)
                    return;
                _decisions = 0;
                break;

            default:
                throw new InvalidOperationException($"A Torizo has nothing called \"{which}\" to break off its walk for.");
        }

        _interruptedWalk = MarkTimeline();
        Play(timeline, restart: true);
    }

    /// <summary>
    /// Gives it the part of it that can be touched and shot, which changes with its pose; none for a pose that has
    /// none. Its sitting and standing-up poses take shots another way from the rest.
    /// </summary>
    private void SetBox(PixelRect? box, bool sitting)
    {
        _sitting = sitting;
        foreach (Collider part in EntityNode.CollidersOf(this).Where(collider => collider.Role is ColliderRole.Hitbox or ColliderRole.Hurtbox))
        {
            part.Enabled = box is not null;
            if (box is { } rect)
                part.Rect = rect;
        }
    }

    /// <summary>Starts its death: it blows apart where it stands.</summary>
    private void BeginDeath()
    {
        if (_dying)
            return;
        _dying = true;
        _stage = Stage.Idle;
        _breakage = Breakage.GutAndFaceBlownUp;
        Play("dying", restart: true);
    }

    /// <summary>Ends its death: it scatters what it drops and is gone.</summary>
    private void FinishDeath()
    {
        for (int drop = 0; drop < DeathDrops; drop++)
        {
            RandomDraw draw = Random.Draw();
            Drop(_scatteredDrops, PixelPosition.FromWhole(
                _dropCorner.X + draw.Part(_dropAcross.From, _dropAcross.Digits), _dropCorner.Y + draw.Part(_dropDown.From, _dropDown.Digits)));
        }

        if (VictoryMusic.Length > 0)
            Room.Session.Sounds.RequestMusic(VictoryMusic);
        Die();
    }

    /// <summary>Now and then, when its health is low, a drop falls from it.</summary>
    private void MaybeDrool()
    {
        if (Health.Current == 0 || Health.Current >= GutHealth)
            return;
        RandomDraw chance = Random.Last;
        if (chance.Part(1, 1) == 0 && chance.Part(6, 1) == 0 && chance.Part(8, 1) == 0 && chance.Part(15, 1) == 0)
            Drool();
    }

    /// <summary>Lets a drop fall from about its middle.</summary>
    private void Drool()
    {
        RandomDraw draw = Random.Draw();
        int y = Position.Y.Whole + draw.Part(0, 2) - 5;
        Pixels fall = Pixels.FromNumber((49 + draw.Part(0, 5)) / 256.0);
        int x = Position.X.Whole + Random.Draw().Part(0, 2);
        if (!_turning)
            x += FacesRight ? 8 : -8;
        Fire("initial_drool", PixelPosition.FromWhole(x, y), new PixelVelocity(Pixels.Zero, fall));
    }

    /// <summary>Throws a gob out of its broken gut.</summary>
    private void SpewDrool()
    {
        int form = Random.Draw().Part(2, 3) % 3;
        RandomDraw draw = Random.Draw();
        int heading = _turning ? draw.Part(1, 8) : (FacesRight ? 32 : 224) + draw.Part(0, 4) - 8;
        Fire($"drool_{form}", Position + Ahead(PixelOffset.FromWhole(8, -5)), AlongHeading(Pixels.FromWhole(1), heading));
    }

    /// <summary>
    /// A speed split along a heading that points up at nothing and turns clockwise, as the cartridge splits it: each way
    /// is the speed in 256ths of a pixel times the coarse sine, cut off toward nothing.
    /// </summary>
    private static PixelVelocity AlongHeading(Pixels speed, int heading)
    {
        int speedIn256ths = speed.Whole * 256 + speed.Fraction / 256;
        return new PixelVelocity(
            From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, heading)),
            From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, heading - Turns.PerTurn / 4)));
    }

    /// <summary>A distance counted in 256ths of a pixel.</summary>
    private static Pixels From256ths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 255) * 256);

    /// <summary>Spews something ahead of it, up and forward, with chance nudging its speed each way.</summary>
    private Projectile SpewOrb(string shot, PixelOffset from, Pixels across, Pixels up)
    {
        Pixels nudgeAcross = Pixels.FromNumber((Random.Draw().Part(0, 8) - 128) / 256.0);
        Pixels nudgeDown = Pixels.FromNumber((Random.Draw().Part(0, 8) - 128) / 256.0);
        return Fire(shot, Position + Ahead(from), new PixelVelocity(Facing.Toward(across) + nudgeAcross, -up + nudgeDown));
    }

    /// <summary>A place given for when it faces right, turned to the way it faces.</summary>
    public PixelOffset Ahead(PixelOffset facingRight) => FacesRight ? facingRight : new PixelOffset(-facingRight.X, facingRight.Y);

    /// <summary>Whether it faces Samus; one facing left faces her when she is level with it too.</summary>
    private bool FacesSamus() => FacesRight == Position.X.Whole < Room.Player.Position.X.Whole;

    /// <summary>Whether Samus is behind it; behind one facing right is anywhere left of it, behind one facing left anywhere not left of it.</summary>
    private bool FacesAwayFromSamus() => FacesRight == Room.Player.Position.X.Whole < Position.X.Whole;

    /// <summary>The timeline that turns it about, for the kind of Torizo it is now.</summary>
    private string TurnAbout()
    {
        string kind = _breakage == Breakage.GutAndFaceBlownUp ? "faceless_" : Golden ? "golden_" : "";
        return $"{kind}turning_{(FacesRight ? "left" : "right")}";
    }

    /// <summary>The way it faces, as its timelines name it.</summary>
    private string Side => FacesRight ? "right" : "left";

    /// <summary>The way a cue names.</summary>
    private static Facing SideNamed(string side) => side switch
    {
        "left" => Facing.Left,
        "right" => Facing.Right,
        _ => throw new InvalidOperationException($"A Torizo's cue names the side \"{side}\", which is neither left nor right."),
    };

    /// <summary>The tint of a name from its data.</summary>
    private Color Tint(string name) => _tints.TryGetValue(name, out Color tint)
        ? tint
        : throw new InvalidOperationException($"The Torizo \"{Name}\" has no tint named \"{name}\".");

    /// <summary>
    /// Holding jump, run, fire and item cancel and nothing else as the golden one's room begins fills Samus's energy and
    /// ammunition to set amounts and gives her most of her equipment, as the cartridge does.
    /// </summary>
    private void GrantSecretCode()
    {
        Player samus = Room.Player;
        if (!samus.Input.HoldsSameAs(new GameInput([GameAction.Jump, Actions.Run, GameAction.Attack, Actions.ItemCancel], [])))
            return;
        Inventory inventory = samus.Inventory;
        foreach ((string amount, int quantity) in new[] { (Amounts.Energy, 700), (Amounts.ReserveEnergy, 300), (Amounts.Missiles, 100), (Amounts.SuperMissiles, 20), (Amounts.PowerBombs, 20) })
        {
            inventory.SetCapacity(amount, quantity);
            inventory.Fill(amount);
        }

        foreach (string equipment in new[]
                 {
                     Equipment.VariaSuit, Equipment.SpringBall, Equipment.MorphBall, Equipment.GravitySuit, Equipment.HiJumpBoots, Equipment.SpaceJump,
                     Equipment.Bombs, Equipment.SpeedBooster, Equipment.GrappleBeam, Equipment.XrayScope, Equipment.WaveBeam, Equipment.IceBeam,
                     Equipment.Spazer, Equipment.PlasmaBeam, Equipment.ChargeBeam,
                 })
        {
            inventory.Grant(equipment);
            inventory.Switch(equipment, true);
        }
    }
}
