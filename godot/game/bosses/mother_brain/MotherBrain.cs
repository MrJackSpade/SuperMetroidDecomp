using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
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
/// her body and the data file of the effect left there; "fire_hand_beam"; "hand_beam_done"; and "shape:" with the
/// name of one of her data's "shapes", which puts the parts of her body that stop shots where one picture of it has
/// them (<see cref="ColliderPoses"/>).</para>
/// <para>Her visual "Head" is her brain, which plays the timelines her fight names as each update ends
/// (<see cref="IClosingStep"/>), after everything in the room has had its turn; their cues are "shake", "drool",
/// "big_breath", "neck:on", "neck:off", "aim_rings", "ring", "laser", "bomb:" with how many flames it leaves,
/// "charge", "charge_particle", "maybe_hold:" with a timeline and "maybe_drool:" with a timeline, and those the rest
/// of her fight adds. Her visuals "Neck0" to "Neck4" are the joints of her neck, each showing her timeline "neck".
/// Her hurtbox "Brain" follows her head. Her data gives "contacts": the rectangles of her "body", her "brain" and a joint of her "neck" that push
/// Samus away, each with "left", "top", "right" and "bottom" from that part's place; the places of the dust and
/// blasts of her fall and rise; "tube_drops", the pipes that fall in turn; the names of her terrain patches; and
/// "hurt_colours", the colours her brain shows on the updates her hurt flash shows it, which the rest of her does
/// not show (<see cref="Engine.Ui.ColourChange"/>). All her places are in the room's own pixels.</para>
/// </remarks>
public partial class MotherBrain : Enemy, IClosingStep
{
    /// <summary>A plasma beam stops on it, as the cartridge's does: it is one of the few a beam does not pass through.</summary>
    public override bool Stops(Hit hit) => hit.Carries(new HitKind(Game.Data.HitKinds.Beam));

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

    /// <summary>The effect of the bigger dust her rising throws up as often as not.</summary>
    [Export, DataValue("big_dust_effect")]
    public string BigDustEffect { get; set; } = "";

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
    private ColliderPoses? _shapes;
    private PixelRect _brainRect;
    private Form _form;
    private Stage _stage;
    private Pose _pose;
    private int _headHealth;
    private PixelPosition _head;
    private TimelinePlayer? _headTimeline;
    private string _headPlaying = "";

    /// <summary>Whether her brain is playing its part of the update's end, where a timeline it starts goes straight on.</summary>
    private bool _brainPlaying;
    private bool _headShown = true;
    private bool _onNeck;
    private bool _bodyTouches;
    private bool _brainTouches = true;
    private bool _neckTouches;
    private int _timer;
    private int _brainShake;

    /// <summary>For how many more updates her head is safe from hits, counted down as each ends, the one it was hit on among them.</summary>
    private int _headSafeUpdates;

    /// <summary>
    /// Held still with the other enemies, her head's time of being safe counts on all the same, as the cartridge's
    /// head, an enemy of its own there, counts its time while time stands still.
    /// </summary>
    protected override void WhileHeldStill()
    {
        if (_headSafeUpdates > 0)
            _headSafeUpdates--;
    }
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

    /// <summary>The colours her brain shows in her hurt flash.</summary>
    private Engine.Ui.ColourChange _headFlash = Engine.Ui.ColourChange.None;

    /// <summary>The name her brain's flash colours are shown under.</summary>
    private const string HeadFlashColours = "HurtFlash";

    /// <summary>She shows her hurt on her brain alone, by her own cadence, not by the flash every enemy shares.</summary>
    protected override bool ShowsHurtFlash => false;

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
        _shapes = ColliderPoses.Read(data.Get("shapes"), this);
        _headFlash = Engine.Ui.ColourChange.Read(data.Get("hurt_colours"));
        // Her brain's hurtbox goes where her head goes, keeping where it lies from it.
        _brainRect = GetNode<Collider>("Brain").Rect;
        ReadFakeDeathValues(data);
        DataNode lights = data.Get("room_lights");
        _lightsOut = Engine.Ui.ColourChange.Read(lights.Get("out"));
        _lightsBack.Clear();
        _lightsBack.AddRange(lights.List("back").Select(Engine.Ui.ColourChange.Read));
        static (int, double, int) Motion(DataNode motion) => (motion.Int("height"), motion.Distance("speed").ToNumber(), motion.Int("delay"));
        (_liquidRise, _liquidDrain) = (Motion(data.Get("liquid").Get("rise")), Motion(data.Get("liquid").Get("drain")));
    }

    /// <summary>
    /// Where her room's liquid heads as she is set up, and as her false death starts the room's flash ("liquid":
    /// "rise" and "drain"): the height, in pixels down from the room's top, how far it moves an update, and how many
    /// updates it waits first.
    /// </summary>
    private (int Height, double Speed, int Delay) _liquidRise, _liquidDrain;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        _form = Form.Glass;
        _stage = Stage.Glass;
        _headHealth = FirstPhaseHealth;
        _head = Position;
        Visual?.SetShown(false);
        // Until her body rises nothing of it is there to stop a shot.
        Shapes.TakeNone();
        TimelineStep joint = Timelines.Get("neck").Steps[0];
        for (int each = 0; each < _neck.Length; each++)
            GetNode<ActorVisual>($"Neck{each}").Show(joint);
        PlayHead("whole_head_mouth_closed");
        Room.Liquid?.HeadFor(_liquidRise.Height, _liquidRise.Speed, _liquidRise.Delay);
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (part.Name != "Brain")
        {
            // Her body shrugs everything off.
            return HitResult.Absorbed;
        }

        // While her head is safe from hits nothing is tried against it: a shot goes on as if it were not there.
        if (_headSafeUpdates > 0)
            return HitResult.Ignored;

        bool missile =hit.CarriesAny([new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]);
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

        Reaction reaction = ReactionTo(hit, part);
        int damage = reaction.DamageFrom(hit.Damage);
        if (damage == 0)
            return HitResult.Absorbed;
        Sound(HurtSound);
        FlashFor(13);
        _headHealth = Math.Max(0, _headHealth - damage);

        // A hit whose reaction says so (the plasma beam's) leaves her head alone safe from hits for that many of her
        // turns after this one, as the cartridge's head, an enemy of its own there, is; her body still meets shots.
        if (reaction.UnhittableUpdates > 0)
            _headSafeUpdates = reaction.UnhittableUpdates + 1;
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
                Room.Shake(1, 4, ShakeWay.Down, ShakeReach.Tiles);
                if (_form == Form.Drained)
                    Sound(FootstepSound);
                Position += PixelOffset.FromWhole(Number(parts[1]), Number(parts[2]));
                break;

            case "move":
                Position += PixelOffset.FromWhole(Number(parts[1]), Number(parts[2]));
                break;

            case "hand_dust":
                Leave(parts[3], Position + PixelOffset.FromWhole(Number(parts[1]), Number(parts[2])));
                break;

            case "shape":
                // Once she is dying nothing of her stops a shot any more.
                if (_stage != Stage.LateFight || _late != Late.Dying)
                    Shapes.Take(parts[1]);
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
        PlaceHead(samus);
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
        Room.Session.Sounds.RequestMusic(Music.SilentSong);
        BeginDescent();
    }

    /// <summary>
    /// Moves her head with her neck, when she has one, and puts her brain's shape where her head is. What shakes her
    /// brain moves only its picture.
    /// </summary>
    private void PlaceHead(Player samus)
    {
        if (_onNeck)
        {
            if (_neckMoves)
                StepNeckAngles(samus);
            PlaceNeck();
            _head = _neck[4] + PixelOffset.FromWhole(0, -21);
        }

        if (GetNodeOrNull<Collider>("Brain") is { } brain)
        {
            PixelOffset fromBody = _head - Position;
            brain.Rect = new PixelRect(fromBody.X.Whole + _brainRect.Left, fromBody.Y.Whole + _brainRect.Top, _brainRect.Width, _brainRect.Height);
        }
    }

    /// <summary>
    /// Her brain's part of the update's end: it breathes, plays its timeline and is shown, once everything in the room
    /// has had its turn. So what a cue of her brain's fires first takes a turn on the next update, and a cue that
    /// reads the random number reads the update's last.
    /// </summary>
    public void StepClosing(Room room)
    {
        if (IsDead)
            return;
        Player samus = room.Player;
        if (_headSafeUpdates > 0)
            _headSafeUpdates--;

        // She breathes out a little, as often as not, whenever her last breath has gone; a breath the room has no
        // place for is not breathed, and she tries again.
        bool breathOut = _smallBreath is { } last && IsInstanceValid(last) && !Room.IsLeaving(last);
        if (_breathes && !breathOut && SmallBreathEffect.Length > 0 && Random.Last.Part(15, 1) == 0)
            _smallBreath = LeaveHoldingPlace(SmallBreathEffect, _head + PixelOffset.FromWhole(6, 16));

        TimelineStep? shown = PlayBrain(samus);

        // Her brain shakes when she is hurt, and for a while when her fight says so. A hurt's shake, of her brain and
        // of her neck, goes by what is left of her flash, two updates to a place; and her brain shows the flash's
        // colours alone on the odd counts of what is left.
        int flashLeft = HurtFlashLeft;
        PixelOffset hurtShake = IsFlashing ? ShakeOffsets[flashLeft / 2 % 4] : PixelOffset.FromWhole(0, 0);
        PixelOffset shake = PixelOffset.FromWhole(0, 0);
        if (_brainShake > 0)
            shake = ShakeOffsets[_brainShake-- / 2 % 4];
        else if (IsFlashing)
            shake = hurtShake;
        PixelOffset fromBody = _head + shake - Position;
        if (GetNodeOrNull<ActorVisual>("Head") is { } head)
        {
            head.Position = new Vector2(fromBody.X.Whole, fromBody.Y.Whole);
            head.SetShown(_headShown);
            bool flashes = IsFlashing && flashLeft % 2 == 1;
            head.Tint(flashes ? Colors.White : _brainDetached ? _headTint : _tint);
            if (shown is not null)
                head.Show(shown);
            // Shown once the picture is, since the colours are worked out by the pictures it shows.
            (flashes ? _headFlash : Engine.Ui.ColourChange.None).ShowOn(head, HeadFlashColours);
        }

        for (int joint = 0; joint < _neck.Length; joint++)
        {
            if (GetNodeOrNull<ActorVisual>($"Neck{joint}") is not { } visual)
                continue;
            PixelOffset from = _neck[joint] + hurtShake - Position;
            visual.Position = new Vector2(from.X.Whole, from.Y.Whole);
            visual.SetShown(_onNeck && Visual is { Visible: true });
            visual.Tint(_tint);
        }
    }

    /// <summary>
    /// Starts her brain on a timeline. Started in her own turn, one that opens with a picture counts the start as that
    /// picture's first update, and one that opens with a cue waits for her brain's part of the update's end. Started
    /// by a cue of her brain's, it is gone straight on into there.
    /// </summary>
    private void PlayHead(string timeline)
    {
        _headPlaying = timeline;
        _headTimeline = new TimelinePlayer(Timelines.Get(timeline));
        if (!_brainPlaying && _headTimeline.Current.Cues.Count == 0)
            _headTimeline.Step();
    }

    /// <summary>How many times in one update her brain's timelines may start one another before that is taken for a mistake in them.</summary>
    private const int MostBrainSwitches = 16;

    /// <summary>
    /// Plays one update of her brain's timeline and acts on the cues it reaches, going on into a timeline a cue
    /// starts. Gives the step that update belongs to, which is the picture her brain shows; null with no timeline.
    /// </summary>
    private TimelineStep? PlayBrain(Player samus)
    {
        TimelineStep? shown = null;
        _brainPlaying = true;
        for (int switches = 0; _headTimeline is { } playing; switches++)
        {
            if (switches > MostBrainSwitches)
                throw new InvalidOperationException($"Mother Brain's brain timelines start one another without end, the last of them \"{_headPlaying}\".");
            IReadOnlyList<string> cues = playing.Step();
            shown = playing.Shown;
            foreach (string cue in cues)
            {
                OnHeadCue(cue, samus);
                if (_headTimeline != playing)
                    break;
            }

            if (_headTimeline == playing)
                break;
        }

        _brainPlaying = false;
        return shown;
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

        // Each axis is the length along the coarse sine, rounded down, as the cartridge lays the joints.
        static PixelOffset Along(int angle, int length) => PixelOffset.FromWhole(
            CartridgeAngles.ReachRoundedDown(angle, length), CartridgeAngles.ReachRoundedDown(angle + Turns.PerTurn / 4, length));
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

    /// <summary>The parts of her body that stop shots, as her data places them for each picture of it.</summary>
    private ColliderPoses Shapes => _shapes ?? throw new InvalidOperationException("Mother Brain's data has not been read.");

    /// <summary>
    /// Leaves dust, smoke or a blast, if it is named: each is one of the enemies' shots on the cartridge, which holds
    /// one of the room's places while it plays and is not made with none free.
    /// </summary>
    private void Leave(string effect, PixelPosition at)
    {
        if (effect.Length > 0)
            LeaveHoldingPlace(effect, at);
    }

    /// <summary>
    /// Leaves an effect that is one of the enemies' shots on the cartridge: it holds one of the room's places while it
    /// plays, and with none free it is not made. Returns it, or null when it was not made.
    /// </summary>
    private Effect? LeaveHoldingPlace(string effect, PixelPosition at, Standing? standing = null)
    {
        Effect made = Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), at, holdsPlace: true, standing: standing);
        return Room.Spawn(made) ? made : null;
    }

    /// <summary>
    /// The speed a blast that follows something leaves in the place it holds among the room's: how far from that
    /// thing it lies, each pixel of it a 256th of a pixel an update. The blast never moves by it; whatever next takes
    /// the place and sets no speed of its own starts with it, as on the cartridge.
    /// </summary>
    private static PixelVelocity SpeedLeftByFollowing(PixelOffset from) =>
        new(Pixels.FromNumber(from.X.Whole / 256.0), Pixels.FromNumber(from.Y.Whole / 256.0));

    private static int Number(string text) => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
