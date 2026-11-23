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
/// Mother Brain. At first she is a brain in a glass tank, behind the Zebetites, with her turrets and Rinkas firing;
/// only missiles hurt her, and they must break the tank too. Beaten there, she seems to die: the room goes grey, her
/// tank's pipes fall away, and she drops to the floor, where a body rises under her. Then she walks, and attacks
/// with rings from her mouth, bombs, a beam from her eye and a beam from her hand, until what happens next is told
/// in the rest of her fight.
/// </summary>
/// <remarks>
/// <para>Her body is the enemy itself and plays the timelines named after its poses and walks: "crouched",
/// "crouch_slow", "crouch_fast", "stand_after_crouch_slow", "stand_after_crouch_fast", "stand_after_leaning",
/// "lean_down", "hand_beam", and "walk_forward_" and "walk_backward_" with "really_fast", "fast", "medium", "slow"
/// and "really_slow". Their cues are "pose:" and "standing", "walking", "crouching", "crouched", "death_beam" or
/// "leaning"; "move:" with pixels across and down; "step:" the same, with a footfall; "hand_dust:" with a place from
/// her body; "fire_hand_beam"; and "hand_beam_done".</para>
/// <para>Her visual "Head" is her brain, which plays the timelines her fight names; their cues are "shake", "drool",
/// "big_breath", "neck:on", "neck:off", "aim_rings", "ring", "laser", "bomb:" with how many flames it leaves,
/// "charge", "charge_particle", "maybe_hold:" with a timeline and "maybe_drool:" with a timeline, and those the rest
/// of her fight adds. Her visuals "Neck0" to "Neck4" are the joints of her neck. Her hurtbox "Brain" follows her
/// head. Her data gives "contacts": the rectangles of her "body", her "brain" and a joint of her "neck" that push
/// Samus away, each with "left", "top", "right" and "bottom" from that part's place; the places of the dust and
/// blasts of her fall and rise; "tube_drops", the pipes that fall in turn; and the names of her terrain patches. All
/// her places are in the room's own pixels.</para>
/// </remarks>
public partial class MotherBrain : Enemy
{
    /// <summary>The music of her fight, by name.</summary>
    [Export]
    public string FightMusic { get; set; } = "";

    /// <summary>Her health in the tank.</summary>
    [Export, DataValue("first_phase_health")]
    public int FirstPhaseHealth { get; set; } = 3000;

    /// <summary>Her health once she has her body.</summary>
    [Export, DataValue("second_phase_health")]
    public int SecondPhaseHealth { get; set; } = 18000;

    /// <summary>Her health once she has come back to life.</summary>
    [Export, DataValue("third_phase_health")]
    public int ThirdPhaseHealth { get; set; } = 36000;

    /// <summary>At or above this health, with her body, she mostly walks and only now and then attacks.</summary>
    [Export, DataValue("full_strength_health")]
    public int FullStrengthHealth { get; set; } = 4500;

    /// <summary>Where her body stands once it has risen, in the room's pixels, before it rises.</summary>
    [Export]
    public Vector2I StandingPlace { get; set; } = new(59, 279);

    /// <summary>How far down the room her body has risen to.</summary>
    [Export]
    public int RisenY { get; set; } = 188;

    /// <summary>The sound of a shot that hurts her, by name.</summary>
    [Export, DataValue("hurt_sound")]
    public string HurtSound { get; set; } = "";

    /// <summary>The sound of one of her blasts, by name.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    /// <summary>The effects she leaves, by the data files of their timelines: small dust, a small blast, smoke, a big blast.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of a small blast.</summary>
    [Export, DataValue("explosion_effect")]
    public string ExplosionEffect { get; set; } = "";

    /// <summary>The effect of smoke.</summary>
    [Export, DataValue("smoke_effect")]
    public string SmokeEffect { get; set; } = "";

    /// <summary>The effect of a big blast.</summary>
    [Export, DataValue("big_explosion_effect")]
    public string BigExplosionEffect { get; set; } = "";

    /// <summary>The forms she takes in turn.</summary>
    private enum Form
    {
        Glass,
        FakeDeath,
        SecondPhase,
        Drained,
        ThirdPhase,
    }

    /// <summary>The stages of her fight up to its last part.</summary>
    private enum Stage
    {
        Glass,
        Descent,
        Ascent,
        SecondPhase,
        LateFight,
    }

    /// <summary>How her body stands.</summary>
    private enum Pose
    {
        Standing,
        Walking,
        Crouching,
        Crouched,
        DeathBeam,
        Leaning,
    }

    /// <summary>What a joint of her neck is doing.</summary>
    private enum NeckMove
    {
        None,
        BobDown,
        BobUp,
        Lower,
        Raise,
    }

    private sealed record Contact(int Left, int Top, int Right, int Bottom);

    private static readonly int[] NeckLowerLengths = [2, 10, 20];
    private static readonly int[] NeckUpperLengths = [10, 20];
    private static readonly PixelOffset[] ShakeOffsets = [PixelOffset.FromWhole(0, 0), PixelOffset.FromWhole(-1, 1), PixelOffset.FromWhole(0, -1), PixelOffset.FromWhole(1, 1)];

    /// <summary>How fast her neck turns, in 256ths of a step of angle an update.</summary>
    private const int NeckSlow = 64, NeckRising = 80, NeckSteady = 256, NeckQuick = 512, NeckFast = 1280, NeckTakenAback = 1792, NeckRecoil = 2304, NeckDyingDrool = 128;

    private readonly Dictionary<string, List<Contact>> _contacts = new(StringComparer.Ordinal);
    private readonly PixelPosition[] _neck = new PixelPosition[5];
    private Form _form;
    private Stage _stage;
    private Pose _pose;
    private int _headHealth;
    private PixelPosition _head;
    private TimelinePlayer? _headTimeline;
    private string _headPlaying = "";
    private bool _headShown = true;
    private bool _onNeck;
    private bool _bodyTouches;
    private bool _brainTouches = true;
    private bool _neckTouches;
    private int _timer;
    private int _brainShake;
    private int _lowerNeck;
    private int _upperNeck;
    private int _neckTurn = NeckSteady;
    private bool _neckMoves;
    private NeckMove _lowerMove;
    private NeckMove _upperMove;
    private int _walkCredit;
    private bool _drools;
    private bool _breathes;
    private Color _tint = Colors.White;

    /// <summary>Where her head is.</summary>
    public PixelPosition HeadPosition => _head;

    /// <inheritdoc/>
    protected override Color RestingTint => _tint;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _contacts.Clear();
        foreach ((string part, DataNode rects) in data.Get("contacts").Entries())
            _contacts[part] = [.. rects.Items().Select(rect => new Contact(rect.Int("left"), rect.Int("top"), rect.Int("right"), rect.Int("bottom")))];
        ReadFakeDeathValues(data);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        _form = Form.Glass;
        _stage = Stage.Glass;
        _headHealth = FirstPhaseHealth;
        _head = Position;
        Visual?.SetShown(false);
        PlayHead("whole_head_mouth_closed");
        Room.Liquid?.Triggered("rise");
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (part.Name != "Brain")
        {
            // Her body shrugs everything off.
            return HitResult.Absorbed;
        }

        bool missile = hit.CarriesAny([new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]);
        if (_form == Form.Glass)
        {
            // In the tank only missiles tell, and each one that reaches her counts against the glass too.
            if (!missile)
                return HitResult.Absorbed;
            Room.Enemies.OfType<MotherBrainGlass>().FirstOrDefault(glass => !glass.IsDead)?.CountHeadShot();
        }
        else
        {
            OnLaterHeadHit(hit, missile);
            if (_form == Form.FakeDeath)
                return HitResult.Absorbed;
        }

        int damage = ReactionTo(hit, part).DamageFrom(hit.Damage);
        if (damage == 0)
            return HitResult.Absorbed;
        Sound(HurtSound);
        FlashFor(13);
        _headHealth = Math.Max(0, _headHealth - damage);
        return HitResult.Hurt;
    }

    /// <summary>What a hit on her head does to her walking, once she is out of the tank.</summary>
    private void OnLaterHeadHit(Hit hit, bool missile)
    {
        if (_stage == Stage.LateFight)
        {
            OnLateFightHeadHit(hit, missile);
            return;
        }

        // A missile stops her advance dead; anything else takes a step's worth off it.
        _walkCredit = missile ? 0 : Math.Max(0, _walkCredit - 256);
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // She pushes Samus away by her own rule.
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "pose":
                _pose = parts[1] switch
                {
                    "standing" => Pose.Standing,
                    "walking" => Pose.Walking,
                    "crouching" => Pose.Crouching,
                    "crouched" => Pose.Crouched,
                    "death_beam" => Pose.DeathBeam,
                    "leaning" => Pose.Leaning,
                    _ => throw new InvalidOperationException($"Mother Brain's body has no pose \"{parts[1]}\"."),
                };
                break;

            case "step":
                Room.Shake(1, 4);
                if (_form == Form.Drained)
                    Sound(FootstepSound);
                Position += PixelOffset.FromWhole(Number(parts[1]), Number(parts[2]));
                break;

            case "move":
                Position += PixelOffset.FromWhole(Number(parts[1]), Number(parts[2]));
                break;

            case "hand_dust":
                Leave(DustEffect, Position + PixelOffset.FromWhole(Number(parts[1]), Number(parts[2])));
                break;

            case "fire_hand_beam":
                Sound(HandBeamSound);
                BeginHandBeam();
                break;

            case "hand_beam_done":
                _handBeam = HandBeam.Finish;
                break;

            default:
                throw new InvalidOperationException($"Mother Brain's body timeline \"{Playing}\" gives the cue \"{cue}\", which she does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        switch (_stage)
        {
            case Stage.Glass:
                StepGlass(samus);
                break;
            case Stage.Descent:
                StepDescent(samus);
                break;
            case Stage.Ascent:
                StepAscent();
                break;
            case Stage.SecondPhase:
                StepSecondPhase(samus);
                break;
            case Stage.LateFight:
                StepLateFight(samus);
                break;
            default:
                throw new InvalidOperationException($"Mother Brain has no stage {_stage}.");
        }

        if (IsDead)
            return;
        PushSamusAway(samus);
        StepHead(samus);
    }

    /// <summary>In her tank. Once the glass is gone and her health with it, and Samus has come close, she seems to die.</summary>
    private void StepGlass(Player samus)
    {
        bool glassGone = !Room.Enemies.OfType<MotherBrainGlass>().Any(glass => !glass.IsDead);
        if (!glassGone)
            return;
        if (Room.ShakeStrength > 0 && _brainShake == 0)
            _brainShake = 1;
        if (samus.Position.X.Whole >= 236 || _headHealth != 0)
            return;
        foreach (MotherBrainTurret turret in Room.Enemies.OfType<MotherBrainTurret>().ToList())
            turret.PutOut();
        foreach (RinkaSpawner source in Room.GetChildren().OfType<RinkaSpawner>())
            source.Triggered("stop");
        _form = Form.FakeDeath;
        Room.Session.Sounds.RequestMusic("");
        BeginDescent();
    }

    /// <summary>Moves her head: with her neck when she has one, and with whatever shakes it.</summary>
    private void StepHead(Player samus)
    {
        if (_onNeck)
        {
            if (_neckMoves)
                StepNeckAngles(samus);
            PlaceNeck();
            _head = _neck[4] + PixelOffset.FromWhole(0, -21);
        }

        // She breathes out a little, as often as not, whenever her last breath has gone.
        bool breathOut = _smallBreath is { } last && IsInstanceValid(last) && !Room.IsLeaving(last);
        if (_breathes && !breathOut && SmallBreathEffect.Length > 0 && Random.Last.Part(15, 1) == 0)
            _smallBreath = LeaveEffect(SmallBreathEffect, _head + PixelOffset.FromWhole(6, 16));

        if (_headTimeline is { } timeline)
        {
            foreach (string cue in timeline.Step())
            {
                OnHeadCue(cue, samus);
                if (_headTimeline != timeline)
                    break;
            }
        }

        // Her brain shakes when she is hurt, and for a while when her fight says so.
        PixelOffset shake = PixelOffset.FromWhole(0, 0);
        if (_brainShake > 0)
            shake = ShakeOffsets[_brainShake-- / 2 % 4];
        else if (IsFlashing)
            shake = ShakeOffsets[Updates / 2 % 4];
        PixelOffset fromBody = _head + shake - Position;
        if (GetNodeOrNull<ActorVisual>("Head") is { } head)
        {
            head.Position = new Vector2(fromBody.X.Whole, fromBody.Y.Whole);
            head.SetShown(_headShown);
            head.Tint(IsFlashing && Updates % 2 == 0 ? new Color(2f, 2f, 2f) : _brainDetached ? _headTint : _tint);
            if (_headTimeline is not null)
                head.Show(_headTimeline.Current.Picture);
        }

        if (GetNodeOrNull<Collider>("Brain") is { } brain)
        {
            PixelRect rect = brain.Rect;
            brain.Rect = new PixelRect(fromBody.X.Whole - rect.Width / 2, fromBody.Y.Whole - rect.Height / 2, rect.Width, rect.Height);
        }

        for (int joint = 0; joint < _neck.Length; joint++)
        {
            if (GetNodeOrNull<ActorVisual>($"Neck{joint}") is not { } visual)
                continue;
            PixelOffset from = _neck[joint] - Position;
            visual.Position = new Vector2(from.X.Whole, from.Y.Whole);
            visual.SetShown(_onNeck && Visual is { Visible: true });
            visual.Tint(_tint);
        }
    }

    /// <summary>Starts her brain on a timeline.</summary>
    private void PlayHead(string timeline)
    {
        _headPlaying = timeline;
        _headTimeline = new TimelinePlayer(Timelines.Get(timeline));
    }

    /// <summary>Turns the two parts of her neck, each as it has been told.</summary>
    private void StepNeckAngles(Player samus)
    {
        const int Step = 256;
        int turn = _neckTurn;
        switch (_lowerMove)
        {
            case NeckMove.BobDown:
                _lowerNeck -= turn;
                if (_lowerNeck < 40 * Step)
                    (_lowerNeck, _lowerMove) = (40 * Step, NeckMove.BobUp);
                break;
            case NeckMove.BobUp:
                if (_head.Y.Whole < 60)
                {
                    _lowerMove = NeckMove.BobDown;
                    break;
                }

                _lowerNeck += turn;
                if (_lowerNeck >= 144 * Step)
                    (_lowerNeck, _lowerMove) = (144 * Step, NeckMove.BobDown);
                break;
            case NeckMove.Lower:
                _lowerNeck -= turn;
                if (_lowerNeck < 48 * Step)
                    (_lowerNeck, _lowerMove) = (48 * Step, NeckMove.None);
                break;
            case NeckMove.Raise:
                _lowerNeck += turn;
                if (_lowerNeck >= 144 * Step)
                    (_lowerNeck, _lowerMove) = (144 * Step, NeckMove.None);
                break;
            case NeckMove.None:
                break;
            default:
                throw new InvalidOperationException($"Mother Brain's neck has no movement {_lowerMove}.");
        }

        // The upper part never turns past a little beyond the lower.
        int lead = _lowerNeck + 8 * Step;
        switch (_upperMove)
        {
            case NeckMove.BobDown:
                if (_head.Y.Whole + 4 >= samus.Position.Y.Whole)
                {
                    (_lowerMove, _upperMove) = (NeckMove.BobUp, NeckMove.BobUp);
                    break;
                }

                _upperNeck -= turn;
                if (_upperNeck < 32 * Step)
                    (_upperNeck, _upperMove) = (32 * Step, NeckMove.BobUp);
                break;
            case NeckMove.BobUp:
                _upperNeck += turn;
                if (_upperNeck >= lead)
                    (_upperNeck, _upperMove) = (lead, NeckMove.BobDown);
                break;
            case NeckMove.Lower:
                _upperNeck -= turn;
                if (_upperNeck < 32 * Step)
                    (_upperNeck, _upperMove) = (32 * Step, NeckMove.None);
                break;
            case NeckMove.Raise:
                _upperNeck += turn;
                if (_upperNeck >= lead)
                    (_upperNeck, _upperMove) = (lead, NeckMove.None);
                break;
            case NeckMove.None:
                break;
            default:
                throw new InvalidOperationException($"Mother Brain's neck has no movement {_upperMove}.");
        }
    }

    /// <summary>Lays the joints of her neck out from her shoulder; angles are from straight down toward the right.</summary>
    private void PlaceNeck()
    {
        PixelPosition root = Position + PixelOffset.FromWhole(32, -50);
        int lower = Turns.Wrap(_lowerNeck >> 8), upper = Turns.Wrap(_upperNeck >> 8);
        for (int joint = 0; joint < 3; joint++)
            _neck[joint] = root + Along(lower, NeckLowerLengths[joint]);
        for (int joint = 0; joint < 2; joint++)
            _neck[3 + joint] = _neck[2] + Along(upper, NeckUpperLengths[joint]);

        static PixelOffset Along(int angle, int length) =>
            PixelOffset.FromWhole(Turns.Sine(angle, Pixels.FromWhole(length)).Whole, Turns.Cosine(angle, Pixels.FromWhole(length)).Whole);
    }

    /// <summary>Pushes Samus away from whichever part of her she has walked into, and hurts her if she is past the body's front.</summary>
    private void PushSamusAway(Player samus)
    {
        if (_bodyTouches && Pushes(samus, "body", Position))
            return;
        if (_brainTouches && Pushes(samus, "brain", _head))
            return;
        if (!_neckTouches)
            return;
        foreach (int joint in new[] { 1, 2, 3 })
        {
            if (Pushes(samus, "neck", _neck[joint]))
                return;
        }
    }

    private bool Pushes(Player samus, string part, PixelPosition origin)
    {
        if (!_contacts.TryGetValue(part, out List<Contact>? rects))
            throw new InvalidOperationException($"Mother Brain's data gives no contact for her {part}.");
        PixelOffset from = samus.Position - origin;
        PixelRect body = samus.BodyRect;
        foreach (Contact rect in rects)
        {
            int downOverlap = body.Height / 2 + Math.Abs(from.Y.Whole >= 0 ? rect.Bottom : rect.Top) - Math.Abs(from.Y.Whole);
            int acrossOverlap = body.Width / 2 + Math.Abs(from.X.Whole >= 0 ? rect.Right : rect.Left) - Math.Abs(from.X.Whole);
            if (downOverlap < 0 || acrossOverlap < 0)
                continue;
            ShiftPlayer(PixelOffset.FromWhole(Math.Max(acrossOverlap, 4), 4));
            if (samus.Position.Y.Whole < 192 && samus.Velocity.Y.IsNegative)
                samus.Velocity = samus.Velocity.WithY(-samus.Velocity.Y);
            // Past the front of her body the push hurts as well.
            int damage = Position.X.Whole + 24 < samus.Position.X.Whole ? ContactDamage : 0;
            samus.TakeHit(new Hit(damage, HitSide.Enemies, HitKind.Contact) { Direction = PixelOffset.FromWhole(1, 0), Source = this }, samus.Hurtbox);
            return true;
        }

        return false;
    }

    /// <summary>Leaves an effect, if it is named.</summary>
    private void Leave(string effect, PixelPosition at)
    {
        if (effect.Length > 0)
            LeaveEffect(effect, at);
    }

    private static int Number(string text) => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
