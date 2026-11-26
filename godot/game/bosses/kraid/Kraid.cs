using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Kraid. It rises out of the floor in a hail of rocks, and at first shows only its upper half: it thinks, roars
/// and spits rocks, lunges forward and backs off, and swings its arm. Shots at its body do nothing but make its eye
/// glow and its mouth open; only what goes into the open mouth hurts. With an eighth of its health gone it rises to
/// its full height through the ceiling, and from then on walks to and fro, fires the spikes from its belly, which
/// can be ridden, and flicks its nails across the room. Dead, it sinks back into the floor, the platforms crumbling
/// after it.
/// </summary>
/// <remarks>
/// <para>Its body plays "body". Its visual "Head" plays "head_roar", "head_eye_glow" and "head_death", whose cues
/// "mouth:" and "closed", "opening", "wider" or "open" say how far its mouth is open. "Arm" plays "arm_normal",
/// "arm_slow", "arm_rising_or_sinking" and "arm_dying_or_preparing_to_lunge", with the cues "swing_begins",
/// "reaches_pause" and "slow_check". "Foot" plays "foot_initial", "foot_neutral", "foot_walking_forward",
/// "foot_lunge_forward" and "foot_walking_backward", with the cues "step_left", "step_right", "raise_body",
/// "lower_body", "walk_end", "walk_on" and "place:" with where the foot is drawn from its usual place. Any of them may
/// give "sound:" and a sound.</para>
/// <para>Its hurtbox "Mouth" is the only place it can be hurt; "HeadShell" and "Body1" through "Body6" are the rest of
/// it, which turn shots away; its hitboxes "Arm1" through "Arm10" hurt to touch and knock Samus back, and its hurtboxes "ArmShield1" through
/// "ArmShield10", in the same places, stop shots, which leave their explosion there; its hurtboxes "FootShield1" and
/// "FootShield2" stop shots and leave nothing. Its data gives
/// "head_poses", where "Mouth", "HeadShell" and the body's pieces are for each way its mouth is open, "arm_poses",
/// where the arm's pieces are for each of the arm's pictures, taken on the arm timelines' cue "pose:" and a name
/// (<see cref="ColliderPoses"/>), and "foot_poses", where the foot's pieces are for each of the foot's pictures, taken
/// on the foot timelines' cue "pose:" and moved as the "place:" before it moves the foot; "body_edge", the left edge of its body from its place for each height at or above
/// "from"; "ceiling_rocks" and "sink_crumbles", each with where a rock falls and the "crumble" of the terrain there,
/// and "death_spikes", the crumbling of the spikes as it dies (<see cref="PatchSequence"/>); "walks", for each place
/// it may stand the places it may walk to next, each with a "weight", a "to" and how many "updates" it then thinks;
/// "spit", where its rocks leave it from, how fast they rise and the eight speeds across that chance picks from;
/// "health_shades", most health first, the changes of the colours of its body and head ("body") and of its parts
/// ("parts") it shows at each least "health" (<see cref="ColourChange"/>), and the colours of its "eyes" there;
/// "hurt_colours", the same two changes for the flash of a hit in the mouth; "death_colours", the change of its body's
/// and head's colours as it dies; "eye_colours", the colours each of those changes but the last shows its eyes in
/// ("pictured"), which its eyes' colours are shown in place of, its eyes' colours in the flash of a hit ("hurt"), and
/// the "steps" a channel of a colour moves through as its eyes glow and dim, darkest first; and "hurt_flash", the
/// "wait" and then the steps, each so many "updates" and "hurt" or not, of the flash a hit in the mouth sets off. Each
/// colour is six hexadecimal digits, red, green and blue. It has one set of pictures, its room's own colours, and its
/// parts' change is shown on its arm, its foot, and the spikes and nails of its room. Its shots are "spit_rock",
/// "rising_rock", "rising_rock_small" and "ceiling_rock". All its places are in the room's own pixels.</para>
/// </remarks>
public partial class Kraid : Enemy
{
    /// <summary>The music of the fight, by name.</summary>
    [Export]
    public string FightMusic { get; set; } = "";

    /// <summary>The music once it has sunk, by name.</summary>
    [Export]
    public string VictoryMusic { get; set; } = "";

    /// <summary>The things told when it grows to its full height, so that the view may follow it up.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenGrown { get; set; } = [];

    /// <summary>What they are told.</summary>
    [Export]
    public string GrownMessage { get; set; } = "open";

    /// <summary>Where it waits under the floor, in the room's pixels.</summary>
    [Export]
    public Vector2I RiseStart { get; set; } = new(176, 592);

    /// <summary>How far down the room it has risen to at first.</summary>
    [Export]
    public int RisenY { get; set; } = 457;

    /// <summary>How far down the room it stands at its full height.</summary>
    [Export]
    public int GrownY { get; set; } = 296;

    /// <summary>How far down the room it has sunk away.</summary>
    [Export]
    public int SunkY { get; set; } = 608;

    /// <summary>How far across the room its lunge carries it.</summary>
    [Export]
    public int LungeReachX { get; set; } = 92;

    /// <summary>How far across the room it starts its second walk from.</summary>
    [Export]
    public int SecondPhaseStartX { get; set; } = 288;

    /// <summary>How many updates it waits under the floor.</summary>
    [Export, DataValue("rise_wait_updates")]
    public int RiseWaitUpdates { get; set; } = 300;

    /// <summary>How many updates pass between its lunges.</summary>
    [Export, DataValue("lunge_delay_updates")]
    public int LungeDelayUpdates { get; set; } = 300;

    /// <summary>How many updates each step of its thinking lasts; chance says how many steps.</summary>
    [Export, DataValue("thinking_step_updates")]
    public int ThinkingStepUpdates { get; set; } = 64;

    /// <summary>How many updates apart the rocks leave its wide-open mouth.</summary>
    [Export, DataValue("spit_cadence_updates")]
    public int SpitCadenceUpdates { get; set; } = 16;

    /// <summary>How many steps the room's backdrop takes to fade.</summary>
    [Export, DataValue("backdrop_fade_steps")]
    public int BackdropFadeSteps { get; set; } = 12;

    /// <summary>How many things it drops when it has sunk.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>How many pixels from the view's left edge its fight keeps Samus while she faces right.</summary>
    [Export, DataValue("samus_in_view_facing_right")]
    public int SamusInViewFacingRight { get; set; } = 64;

    /// <summary>How many pixels from the view's left edge its fight keeps Samus while she faces left.</summary>
    [Export, DataValue("samus_in_view_facing_left")]
    public int SamusInViewFacingLeft { get; set; } = 80;

    /// <summary>The effect of a shot that does it no harm, by the data file of its timeline.</summary>
    [Export, DataValue("dud_effect")]
    public string DudEffect { get; set; } = "";

    /// <summary>The effect of a super missile or a power bomb that does it no harm.</summary>
    [Export, DataValue("big_dud_effect")]
    public string BigDudEffect { get; set; } = "";

    /// <summary>The effect of a shot in its mouth that does it no harm, by the data file of its timeline.</summary>
    [Export, DataValue("mouth_dud_effect")]
    public string MouthDudEffect { get; set; } = "";

    /// <summary>The sound of a shot that does it no harm, by name.</summary>
    [Export, DataValue("dud_sound")]
    public string DudSound { get; set; } = "";

    /// <summary>The sound of a rock flying, and of its rumbling as it sinks, by name.</summary>
    [Export, DataValue("rock_sound")]
    public string RockSound { get; set; } = "";

    private enum Stage
    {
        Rising,
        Fighting,
        Growing,
        Dying,
        Sunk,
    }

    private enum Rise
    {
        Wait,
        ShowBody,
        StartQuake,
        SlowRocks,
        FastRocks,
        RaiseBody,
    }

    private enum Fight
    {
        FirstThinking,
        SecondThinking,
        MouthOpenAttack,
        MouthOpenReaction,
        WaitBeforeEyeGlow,
        BeginEyeGlow,
        GlowEye,
        UnglowEye,
        HeadBeforeGrowth,
    }

    private enum Growth
    {
        ReleaseCamera,
        BreakCeiling,
        BringInFront,
        SettleParts,
        ShowBackdrop,
        FadeBackdrop,
    }

    private enum Death
    {
        Begin,
        FadeOut,
        ShowBody,
        StartSinking,
        Sink,
    }

    private enum Sinking
    {
        HideBody,
        Wait,
        FadeIn,
        Finished,
    }

    private enum FootDoing
    {
        Resting,
        FirstThinking,
        PrepareLunge,
        Lunge,
        WaitBeforeRetreat,
        Retreat,
        WalkToStart,
        WaitBeforeSecond,
        SecondBegin,
        SecondThinking,
        WalkingRight,
        WalkingLeft,
    }

    /// <summary>How the swing of its arm stands: not a plain swing, a plain swing not yet at its pause, or one past it.</summary>
    private enum Swing
    {
        Away,
        BeforePause,
        PastPause,
    }

    private sealed record Crumble(int At, int RockX, PatchSequence Change);

    private sealed record Walk(int Weight, int To, int Updates);

    /// <summary>The colour of one of its eyes, as the steps of each channel.</summary>
    private readonly record struct EyeColour(int Red, int Green, int Blue);

    /// <summary>The name its eyes' colours are shown under.</summary>
    private const string EyeColours = "KraidEyes";

    /// <summary>The name the colours of its body, head and parts are shown under.</summary>
    private const string OwnColours = "KraidColours";

    private readonly List<(int From, int Left)> _bodyEdge = [];

    /// <summary>Most health first, the colours of its body and head and of its parts at each least health, and of its eyes there.</summary>
    private readonly List<(int Health, ColourChange Body, ColourChange Parts, EyeColour[] Eyes)> _healthShades = [];

    /// <summary>The colours of its body and head, and of its parts, in the flash of a hit; and of its body and head as it dies.</summary>
    private ColourChange _hurtBody = ColourChange.None, _hurtParts = ColourChange.None, _deathColours = ColourChange.None;

    /// <summary>The colours its body and head, and its parts, show while it lives; none, its room's own.</summary>
    private ColourChange _bodyColours = ColourChange.None, _partColours = ColourChange.None;

    /// <summary>For each of its visuals, the colours last shown on it.</summary>
    private readonly Dictionary<ActorVisual, ColourChange> _shownColours = [];
    private readonly List<int> _channelSteps = [];
    private EyeColour[] _picturedEyes = [];
    private EyeColour[] _hurtEyes = [];
    private EyeColour[] _eyes = [];
    private readonly List<(bool Hurt, int Updates)> _hurtFlash = [];
    private readonly List<Pixels> _spitSpeeds = [];
    private ColliderPoses? _headPoses;
    private ColliderPoses? _armPoses;
    private ColliderPoses? _footPoses;
    private Vector2I _footPlace;
    private PatchSequence? _deathSpikes;
    private PixelOffset _spitFrom;
    private Pixels _spitRise;
    private int _hurtFlashWait;
    private int _flashStep = -1;
    private int _flashLeft;

    /// <summary>Whether it still shows its room's own colours, as it does until its flash first shows a frame or its health is gone.</summary>
    private bool _inRoomColours;
    private readonly List<Crumble> _ceilingRocks = [];
    private readonly List<Crumble> _sinkCrumbles = [];
    private readonly Dictionary<int, List<Walk>> _walks = [];
    private readonly Dictionary<string, TimelinePlayer> _parts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _partPlaying = new(StringComparer.Ordinal);
    private Stage _stage;
    private Rise _rise;
    private Fight _fight;
    private Growth _growth;
    private Death _death;
    private Sinking _sinking;
    private FootDoing _foot;
    private Swing _swing;
    private string _mouth = "closed";
    private int _wait;
    private int _thinking;
    private int _rocksWait;
    private int _footWait;
    private int _footThink;
    private int _openLeft;
    /// <summary>The count every gradual fade shares, by which his backdrop's fades are over.</summary>
    private Compatibility.SharedFadeCount FadeCount => Room.Session.Service<Compatibility.SharedFadeCount>();
    private int _nextCeilingRock;
    private int _mouthReopens;
    private bool _mouthReopenMarked;
    private bool _walkEnd;
    private bool _headOver;
    private int _initialHealth;
    private int _walkTarget;
    private int _sinkSound;
    private bool _armHarmless;

    /// <summary>Whether it stands at its full height and its spikes and nails are in play.</summary>
    public bool HasGrown { get; private set; }

    /// <summary>Whether it is dying or gone: its spikes and nails go with it.</summary>
    public bool IsFinished => _stage is Stage.Dying or Stage.Sunk;

    /// <summary>How high Samus can be pushed up out of its body: no higher than this, in the room's pixels.</summary>
    public int EjectionLimitY { get; private set; } = 324;

    /// <summary>The count that moves on each time its spikes are set back at their heights; they follow it.</summary>
    public int LintsPlaced { get; private set; }

    /// <summary>Whether it has begun to rise to its full height: its spikes are out of sight from then until each next comes out.</summary>
    public bool HasBegunGrowing { get; private set; }

    /// <summary>The left edge of its body, in the room's pixels, at a height in the room.</summary>
    public int LeftEdgeAt(int y)
    {
        int relative = y - Position.Y.Whole;
        foreach ((int from, int left) in _bodyEdge)
        {
            if (relative >= from)
                return Position.X.Whole + left;
        }

        return Position.X.Whole + _bodyEdge[^1].Left;
    }

    /// <summary>Where its arm is.</summary>
    public PixelPosition ArmPosition => Position + PixelOffset.FromWhole(0, -44);

    /// <summary>Has the lowest of its spikes fire at once, as a touch of its arm does.</summary>
    public event Action? ArmTouched;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _headPoses = ColliderPoses.Read(data.Get("head_poses"), this);
        _armPoses = ColliderPoses.Read(data.Get("arm_poses"), this);
        _footPoses = ColliderPoses.Read(data.Get("foot_poses"), this);
        DataNode eyeColours = data.Get("eye_colours");
        _channelSteps.Clear();
        _channelSteps.AddRange(eyeColours.List("steps").Select(step => step.AsInt()));
        if (_channelSteps.Count < 2)
            throw eyeColours.Problem("must give the steps of a channel of colour, at least the darkest and the brightest");
        _picturedEyes = ReadEyes(eyeColours, "pictured");
        _hurtEyes = ReadEyes(eyeColours, "hurt");
        _healthShades.Clear();
        _healthShades.AddRange(data.List("health_shades").Select(band =>
            (band.Int("health"), ColourChange.Read(band.Get("body")), ColourChange.Read(band.Get("parts")), ReadEyes(band, "eyes"))));
        if (_healthShades.Count == 0)
            throw data.Problem("must give at least one of its health shades");
        DataNode hurt = data.Get("hurt_colours");
        (_hurtBody, _hurtParts) = (ColourChange.Read(hurt.Get("body")), ColourChange.Read(hurt.Get("parts")));
        _deathColours = ColourChange.Read(data.Get("death_colours"));
        if (_hurtEyes.Length != _picturedEyes.Length || _healthShades.Any(band => band.Eyes.Length != _picturedEyes.Length))
            throw data.Problem("must give the colours of each of its eyes, as many as its pictures have, wherever it gives its eyes' colours");
        DataNode flash = data.Get("hurt_flash");
        _hurtFlashWait = flash.Int("wait");
        _hurtFlash.Clear();
        _hurtFlash.AddRange(flash.List("steps").Select(step => (step.Flag("hurt"), step.Int("updates"))));
        DataNode spit = data.Get("spit");
        _spitFrom = PixelOffset.FromWhole(spit.Int("x"), spit.Int("y"));
        _spitRise = spit.Distance("rise");
        _spitSpeeds.Clear();
        _spitSpeeds.AddRange(spit.List("speeds").Select(speed => speed.AsPixels()));
        if (_spitSpeeds.Count != 8)
            throw spit.Problem("must give the eight speeds its roll of eight picks from");
        _deathSpikes = PatchSequence.Read(data.Get("death_spikes"));
        _bodyEdge.Clear();
        _bodyEdge.AddRange(data.List("body_edge").Select(row => (row.Int("from"), row.Int("left"))));
        if (_bodyEdge.Count == 0)
            throw data.Problem("must give at least one row of its body's edge");
        _ceilingRocks.Clear();
        _ceilingRocks.AddRange(data.List("ceiling_rocks").Select(rock => new Crumble(0, rock.Int("x"), PatchSequence.Read(rock.Get("crumble")))));
        _sinkCrumbles.Clear();
        _sinkCrumbles.AddRange(data.List("sink_crumbles").Select(step => new Crumble(step.Int("y"), step.Int("x"), PatchSequence.Read(step.Get("crumble")))));
        _walks.Clear();
        foreach (DataNode row in data.List("walks"))
            _walks[row.Int("from")] = [.. row.List("to").Select(walk => new Walk(walk.Int("weight"), walk.Int("to"), walk.Int("updates")))];
    }

    /// <inheritdoc/>
    /// <remarks>Its body is out of reach of shots while it rises and once it sinks; its arm while it is harmless or finished.</remarks>
    public override bool IsHitBy(HitSide side) =>
        _stage != Stage.Sunk && !(_stage == Stage.Dying && _death == Death.Sink) && base.IsHitBy(side);

    /// <summary>Whether the pieces of its arm stop shots at the moment.</summary>
    private bool ArmTakesShots => !IsFinished && !_armHarmless;

    /// <summary>Whether a part is one of the pieces of its arm that shots meet.</summary>
    private static bool IsArmShield(Collider part) => part.Name.ToString().StartsWith(ArmShield, StringComparison.Ordinal);

    /// <summary>The name of the pieces of its arm that shots meet, before their number.</summary>
    private const string ArmShield = "ArmShield";

    /// <summary>Whether a part is one of the pieces of its foot, which stop shots.</summary>
    private static bool IsFootShield(Collider part) => part.Name.ToString().StartsWith(FootShield, StringComparison.Ordinal);

    /// <summary>The name of the pieces of its foot, before their number.</summary>
    private const string FootShield = "FootShield";

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        // A power bomb is not met by its arm or its foot.
        if ((IsArmShield(part) || IsFootShield(part)) && hit.Carries(new HitKind(HitKinds.PowerBomb)))
            return HitResult.Ignored;
        // Its foot stops every other shot, and leaves nothing.
        if (IsFootShield(part))
            return HitResult.Absorbed;
        // A beam that is not charged goes through its open mouth as if the mouth were not there.
        if (part.Name == "Mouth" && !hit.Carries(new HitKind(HitKinds.Charged)) &&
            hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Ice), new HitKind(HitKinds.Wave), new HitKind(HitKinds.Spazer), new HitKind(HitKinds.Plasma)]))
            return HitResult.Ignored;
        HitResult result = base.TakeHit(hit, part);
        if (part.Name == "Mouth" && result is HitResult.Hurt or HitResult.Destroyed)
            MarkMouthHit();
        return result;
    }

    /// <summary>
    /// A hit in the mouth, harmful or not, sets its colours flashing from the start, keeping those it shows until the
    /// flash's first frame, and marks an opening yet to come.
    /// </summary>
    private void MarkMouthHit()
    {
        (_flashStep, _flashLeft) = (0, _hurtFlashWait);
        if (_mouthReopens > 0)
            _mouthReopenMarked = true;
    }

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        Reaction reaction = base.ReactionTo(hit, part);
        return part.Name == "Mouth" ? reaction : reaction with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never };
    }

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (part.Name == "Mouth")
        {
            // A shot in its mouth that does no harm leaves the dud's own effect, not its body's explosion.
            if (MouthDudEffect.Length > 0 && hit.At is { } dudAt)
                LeaveEffect(MouthDudEffect, dudAt);
            Sound(DudSound, SoundPriority.Low);
            MarkMouthHit();
            return;
        }

        // A shot at its arm or its body does no harm and leaves its explosion; one at its body also makes its eye glow
        // and its mouth open, and a charged one, three times over.
        string dud = hit.CarriesAny([new HitKind(HitKinds.SuperMissile), new HitKind(HitKinds.PowerBomb)]) ? BigDudEffect : DudEffect;
        if (dud.Length > 0 && hit.At is { } shotAt)
            LeaveEffect(dud, shotAt);
        Sound(DudSound);
        if (IsArmShield(part) || _stage != Stage.Fighting || _fight != Fight.FirstThinking)
            return;
        _fight = Fight.BeginEyeGlow;
        if (hit.Carries(new HitKind(HitKinds.Charged)))
            _mouthReopens = 3;
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        if (_stage is Stage.Fighting or Stage.Growing)
        {
            _stage = Stage.Dying;
            _death = Death.Begin;
            (_mouthReopens, _mouthReopenMarked) = (0, false);
            ShowShields();
        }

        return false;
    }

    /// <summary>He shows his hurt by his own colours alone, not by the flash every enemy shares.</summary>
    protected override bool ShowsHurtFlash => false;

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // Only the pieces of its arm touch; its body pushes Samus out of itself by its own rule, and its foot does nothing.
        if (IsFinished || _armHarmless || IsFootShield(part))
            return;

        // Its arm knocks Samus back, and sets off the lowest spike.
        ShiftPlayer(PixelOffset.FromWhole(4, -8));
        ArmTouched?.Invoke();
        base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;

        // Each of its parts takes its own depth as the fight goes (its body among the room's things, its head, arm
        // and foot in front of the room's tiles), so it is not put in front of the tiles as a whole.
        InFrontOfTiles = false;
        // Its fight keeps Samus where a boss's does in the view, from its setup until it has sunk.
        Room.KeepPlayerInViewAt(SamusInViewFacingRight, SamusInViewFacingLeft);
        _initialHealth = Health.Current;
        Position = PixelPosition.FromWhole(RiseStart.X, RiseStart.Y);
        _stage = Stage.Rising;
        _rise = Rise.Wait;
        _wait = RiseWaitUpdates;
        Visual?.SetShown(false);
        if (Visual is { } body)
            body.ZIndex = RoomDepth.Things;
        // Its head is drawn with its body until it first shows a pose.
        SetPartDepth("Head", RoomDepth.Things);
        Play("body");
        PlayPart("Arm", "arm_rising_or_sinking");
        PlayPart("Foot", "foot_initial");
        PlayPart("Head", "head_roar");
        _headOver = true;
        _foot = FootDoing.Resting;
        _armPoses?.TakeNone();
        _footPoses?.TakeNone();
        ShowMouth("closed", 0);
        // Its mouth and body let every shot through while it rises.
        _headPoses?.TakeNone();
        // It shows its room's own colours, with none of its eyes' own, until its flash first shows a frame or its
        // health is gone; its eyes start in the colours of its band all the same.
        _inRoomColours = true;
        (_bodyColours, _partColours) = (ColourChange.None, ColourChange.None);
        _eyes = [.. BandShade().Eyes];
        if (GetNodeOrNull<ActorVisual>("Arm") is { } arm)
            arm.Position = new Vector2(0, -44);
        if (GetNodeOrNull<ActorVisual>("Foot") is { } foot)
            foot.Position = new Vector2(0, 100);
        ShowColours();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        StepColours();
        if (_stage is Stage.Fighting or Stage.Growing)
            PushSamusOut(samus);

        switch (_stage)
        {
            case Stage.Rising:
                StepRise(samus);
                break;
            case Stage.Fighting:
                StepFight();
                break;
            case Stage.Growing:
                StepGrowth();
                break;
            case Stage.Dying:
                StepDeath();
                break;
            case Stage.Sunk:
                StepSunk();
                return;
            default:
                throw new InvalidOperationException($"Kraid has no stage {_stage}.");
        }

        if (IsDead)
            return;
        StepFoot();
        // Its arm holds still while its mouth has openings yet to come.
        if (_mouthReopens == 0)
            StepPart("Arm");
        StepPart("Foot");
    }

    /// <summary>Its rising out of the floor: Samus is kept to the first screen while rocks fly and it comes up.</summary>
    private void StepRise(Player samus)
    {
        if (samus.Position.X.Whole >= 256)
            samus.Position = new PixelPosition(Pixels.FromWhole(256), samus.Position.Y);
        switch (_rise)
        {
            case Rise.Wait:
                if (--_wait <= 0)
                    _rise = Rise.ShowBody;
                break;

            case Rise.ShowBody:
                Visual?.SetShown(true);
                _rise = Rise.StartQuake;
                break;

            case Rise.StartQuake:
                _rise = Rise.SlowRocks;
                _rocksWait = 120;
                _wait = 496;
                Room.Shake(2, 496);
                if (FightMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(FightMusic);
                break;

            case Rise.SlowRocks:
                _wait--;
                if (--_rocksWait <= 0)
                    (_rise, _rocksWait) = (Rise.FastRocks, 96);
                else if (_rocksWait % 16 == 0)
                    ThrowRisingRock();
                break;

            case Rise.FastRocks:
                _wait--;
                if (--_rocksWait <= 0)
                    _rise = Rise.RaiseBody;
                else if (_rocksWait % 8 == 0)
                    ThrowRisingRock();
                break;

            case Rise.RaiseBody:
                // Two rocks fly in every eight updates of the shaking that is left.
                _wait = Math.Max(0, _wait - 1);
                if (_wait % 8 is 0 or 2)
                    ThrowRisingRock();
                Position = new PixelPosition(Position.X + Pixels.FromWhole(Position.Y.Whole % 4 >= 2 ? 1 : -1), Position.Y - Pixels.FromNumber(0.5));
                if (Position.Y.Whole >= RisenY)
                    break;
                Position = new PixelPosition(Pixels.FromWhole(RiseStart.X), Position.Y);
                _stage = Stage.Fighting;
                ShowMouth(_mouth, _openLeft);
                BeginFirstThinking();
                _foot = FootDoing.FirstThinking;
                _footWait = LungeDelayUpdates;
                PlayPart("Arm", "arm_normal");
                break;

            default:
                throw new InvalidOperationException($"Kraid's rise has no step {_rise}.");
        }
    }

    /// <summary>Its fight: thinking, then its mouth opening, by itself to spit rocks or after its eye has glowed.</summary>
    private void StepFight()
    {
        switch (_fight)
        {
            case Fight.FirstThinking:
                if (_thinking > 0)
                    _thinking--;
                if (_thinking == 0)
                {
                    _fight = Fight.MouthOpenAttack;
                    PlayPart("Head", "head_roar", restart: true);
                }

                break;

            case Fight.SecondThinking:
                if (_thinking > 0)
                    _thinking--;
                if (_thinking == 0)
                {
                    _fight = Fight.MouthOpenReaction;
                    PlayPart("Head", "head_roar", restart: true);
                }

                break;

            case Fight.MouthOpenAttack:
                StepHead();
                if (_headOver)
                {
                    BeginFirstThinking();
                    CloseMouth();
                    break;
                }

                // While its mouth is wide open a rock comes out every so many updates, from the update it opens. How
                // fast it flies across is read from the second through fourth digits of the number last drawn.
                if (_mouth == "open" && _openLeft > 0 && _openLeft-- % SpitCadenceUpdates == 0)
                    Fire("spit_rock", Position + _spitFrom, new PixelVelocity(_spitSpeeds[Random.Last.Part(1, 3)], _spitRise));

                break;

            case Fight.MouthOpenReaction:
                StepHead();
                if (!_headOver)
                    break;
                _fight = Fight.FirstThinking;
                CloseMouth();
                break;

            case Fight.WaitBeforeEyeGlow:
                if (--_wait <= 0)
                    _fight = Fight.BeginEyeGlow;
                break;

            case Fight.BeginEyeGlow:
                _fight = Fight.GlowEye;
                PlayPart("Head", "head_eye_glow", restart: true);
                GlowEyes();
                break;

            case Fight.GlowEye:
                GlowEyes();
                break;

            case Fight.UnglowEye:
                // Its eyes dim a step an update toward its band's colours, and it roars on the update it finds them there.
                bool dimmed = DimEyes(BandShade().Eyes);
                ShowColours();
                if (!dimmed)
                    break;
                _fight = Fight.MouthOpenReaction;
                PlayPart("Head", "head_roar", restart: true);
                break;

            case Fight.HeadBeforeGrowth:
                StepHead();
                if (--_wait <= 0)
                    (_stage, _growth) = (Stage.Growing, Growth.ReleaseCamera);
                break;

            default:
                throw new InvalidOperationException($"Kraid's fight has no step {_fight}.");
        }
    }

    /// <summary>Its rising to its full height through the ceiling.</summary>
    private void StepGrowth()
    {
        switch (_growth)
        {
            case Growth.ReleaseCamera:
                foreach (NodePath path in ToldWhenGrown)
                    (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"Kraid is to tell \"{path}\" that it has grown, but that is nothing that can be told.")).Triggered(GrownMessage);
                EjectionLimitY = 164;
                _growth = Growth.BreakCeiling;
                break;

            case Growth.BreakCeiling:
                if (Updates % 8 == 0)
                    ThrowRisingRock();
                Position = PixelPosition.FromWhole(Position.X.Whole + (Position.Y.Whole % 4 >= 2 ? -1 : 1), Position.Y.Whole - 1);
                if (Position.Y.Whole % 4 == 0 && _nextCeilingRock < _ceilingRocks.Count)
                {
                    Crumble rock = _ceilingRocks[_nextCeilingRock++];
                    DropCeilingRock(rock.RockX);
                    Room.Play(rock.Change.FromStart());
                }

                if (Position.Y.Whole < GrownY)
                    _growth = Growth.BringInFront;
                break;

            case Growth.BringInFront:
                // Its grown body comes in front of what stands in the room, still behind the room's front tiles.
                if (Visual is { } body)
                    body.ZIndex = 1;
                _armHarmless = false;
                // Its arm meets shots again where its pose has it.
                if (ArmPoses.Current.Length > 0)
                    ArmPoses.Take(ArmPoses.Current);
                _growth = Growth.SettleParts;
                break;

            case Growth.SettleParts:
                PlayPart("Foot", "foot_neutral");
                _growth = Growth.ShowBackdrop;
                break;

            case Growth.ShowBackdrop:
                // His room is drawn anew before its backdrop fades, which starts the fades' shared count over.
                FadeCount.Restart();
                _growth = Growth.FadeBackdrop;
                break;

            case Growth.FadeBackdrop:
                if (!FadeCount.TakeStep(BackdropFadeSteps - 1))
                    break;
                _stage = Stage.Fighting;
                _fight = Fight.SecondThinking;
                HasGrown = true;
                LintsPlaced++;
                _thinking = ThinkingUpdates();
                PlayPart("Head", "head_roar", restart: true);
                _headOver = true;
                _walkTarget = SecondPhaseStartX;
                _foot = FootDoing.WalkToStart;
                PlayPart("Foot", "foot_walking_backward", restart: true);
                break;

            default:
                throw new InvalidOperationException($"Kraid's growth has no step {_growth}.");
        }
    }

    /// <summary>Its death: a last cry in the dark, and then it sinks, the platforms crumbling after it.</summary>
    private void StepDeath()
    {
        switch (_death)
        {
            case Death.Begin:
                PlayPart("Arm", "arm_dying_or_preparing_to_lunge", restart: true);
                PlayPart("Head", "head_death", restart: true);
                if (_deathSpikes is { } spikes)
                    Room.Play(spikes.FromStart());
                _death = Death.FadeOut;
                break;

            case Death.FadeOut:
                StepHead();
                if (!FadeCount.TakeStep(BackdropFadeSteps - 1))
                    break;
                // With the room dark its body is drawn behind what stands in the room again, grown or not.
                _death = Death.ShowBody;
                if (Visual is { } body)
                    body.ZIndex = RoomDepth.Things;
                break;

            case Death.ShowBody:
                StepHead();
                _death = Death.StartSinking;
                break;

            case Death.StartSinking:
                StepHead();
                _death = Death.Sink;
                _sinkSound = 43;
                Room.Shake(1, 256);
                PlayPart("Arm", "arm_rising_or_sinking", restart: true);
                PlayPart("Foot", "foot_initial", restart: true);
                _foot = FootDoing.Resting;
                break;

            case Death.Sink:
                StepHead();
                if (--_sinkSound <= 0)
                {
                    Sound(RockSound);
                    _sinkSound = 30;
                }

                if (_sinkCrumbles.FirstOrDefault(step => step.At == Position.Y.Whole) is { } crumble)
                {
                    DropCeilingRock(crumble.RockX);
                    Room.Play(crumble.Change.FromStart());
                }

                Position += PixelOffset.FromWhole(0, 1);
                if (Position.Y.Whole < SunkY)
                    break;
                // Sunk, it gives the room back its own place for Samus in the view, before its drops.
                Room.KeepPlayerInViewAt(Room.PlayerInView.X, Room.PlayerInView.Y);
                for (int drop = 0; drop < DeathDrops; drop++)
                {
                    RandomDraw draw = Random.Draw();
                    Drop(Drops, PixelPosition.FromWhole(128 + draw.Part(0, 8), 352 + draw.Part(8, 6)));
                }

                foreach (string part in new[] { "Arm", "Foot", "Head" })
                    GetNodeOrNull<ActorVisual>(part)?.SetShown(false);
                _stage = Stage.Sunk;
                _sinking = Sinking.HideBody;
                break;

            default:
                throw new InvalidOperationException($"Kraid's death has no step {_death}.");
        }
    }

    /// <summary>Once it has sunk the room comes back out of the dark, and it is gone.</summary>
    private void StepSunk()
    {
        switch (_sinking)
        {
            case Sinking.HideBody:
                Visual?.SetShown(false);
                _wait = 5;
                _sinking = Sinking.Wait;
                break;

            case Sinking.Wait:
                if (--_wait <= 0)
                {
                    // His room is drawn anew before this fade too, and the shared count starts over.
                    FadeCount.Restart();
                    _sinking = Sinking.FadeIn;
                }

                break;

            case Sinking.FadeIn:
                if (!FadeCount.TakeStep(BackdropFadeSteps - 1))
                    break;
                if (VictoryMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(VictoryMusic);
                _sinking = Sinking.Finished;
                Die();
                break;

            case Sinking.Finished:
                break;

            default:
                throw new InvalidOperationException($"Kraid's sinking has no step {_sinking}.");
        }
    }

    /// <summary>What its legs are doing: lunging and backing off at first, and walking to and fro once it has grown.</summary>
    private void StepFoot()
    {
        int x = Position.X.Whole;
        switch (_foot)
        {
            case FootDoing.Resting:
                break;

            case FootDoing.FirstThinking:
                if (!TryBeginGrowth() && --_footWait <= 0)
                    _foot = FootDoing.PrepareLunge;
                break;

            case FootDoing.PrepareLunge:
                // It lunges only once the swing of its arm has reached its pause.
                if (TryBeginGrowth() || _swing == Swing.BeforePause)
                    break;
                PlayPart("Arm", "arm_dying_or_preparing_to_lunge", restart: true);
                PlayPart("Foot", "foot_lunge_forward", restart: true);
                _foot = FootDoing.Lunge;
                break;

            case FootDoing.Lunge:
                if (TryBeginGrowth())
                    break;
                if (x < LungeReachX)
                    SetX(x = LungeReachX);
                if (!_walkEnd)
                    break;
                if (x == LungeReachX)
                {
                    (_foot, _footWait) = (FootDoing.WaitBeforeRetreat, 1);
                    PlayPart("Foot", "foot_neutral", restart: true);
                }
                else
                {
                    PlayPart("Foot", "foot_lunge_forward", restart: true);
                }

                break;

            case FootDoing.WaitBeforeRetreat:
                if (--_footWait > 0)
                    break;
                _foot = FootDoing.Retreat;
                PlayPart("Foot", "foot_walking_backward", restart: true);
                break;

            case FootDoing.Retreat:
                if (TryBeginGrowth())
                    break;
                if (x >= RiseStart.X)
                    SetX(x = RiseStart.X);
                if (!_walkEnd)
                    break;
                if (x == RiseStart.X)
                {
                    PlayPart("Arm", "arm_normal", restart: true);
                    PlayPart("Foot", "foot_neutral", restart: true);
                    (_foot, _footWait) = (FootDoing.FirstThinking, LungeDelayUpdates);
                }
                else
                {
                    PlayPart("Foot", "foot_walking_backward", restart: true);
                }

                break;

            case FootDoing.WalkToStart:
                if (_walkTarget != x)
                {
                    if (_walkTarget >= x)
                        break;
                    SetX(_walkTarget);
                }

                if (!_walkEnd)
                    break;
                (_foot, _footWait) = (FootDoing.WaitBeforeSecond, 180);
                PlayPart("Foot", "foot_neutral", restart: true);
                break;

            case FootDoing.WaitBeforeSecond:
                if (--_footWait <= 0)
                    _foot = FootDoing.SecondBegin;
                break;

            case FootDoing.SecondBegin:
                WalkTo(352, 180, x);
                break;

            case FootDoing.SecondThinking:
                if (_footThink > 0 && --_footThink > 0)
                    break;
                Walk choice = ChooseWalk(x);
                WalkTo(choice.To, choice.Updates, x);
                break;

            case FootDoing.WalkingRight:
                if (_walkTarget != x)
                {
                    if (_walkTarget >= x)
                        break;
                    SetX(_walkTarget);
                }

                if (_walkEnd)
                {
                    _foot = FootDoing.SecondThinking;
                    PlayPart("Foot", "foot_neutral", restart: true);
                }

                break;

            case FootDoing.WalkingLeft:
                if (_walkTarget < x)
                {
                    if (_walkEnd)
                        PlayPart("Foot", "foot_walking_forward", restart: true);
                    break;
                }

                SetX(_walkTarget);
                if (_walkEnd)
                {
                    _foot = FootDoing.SecondThinking;
                    PlayPart("Foot", "foot_neutral", restart: true);
                }

                break;

            default:
                throw new InvalidOperationException($"Kraid's legs have nothing to do called {_foot}.");
        }
    }

    /// <summary>Sets it walking toward a place, to think for a while once it is there.</summary>
    private void WalkTo(int target, int thinkUpdates, int from)
    {
        _walkTarget = target;
        _footThink = thinkUpdates;
        bool right = target >= from;
        _foot = right ? FootDoing.WalkingRight : FootDoing.WalkingLeft;
        PlayPart("Foot", right ? "foot_walking_backward" : "foot_walking_forward", restart: true);
    }

    /// <summary>Picks where it walks next from where it stands, by chance and by the weights its data gives.</summary>
    private Walk ChooseWalk(int from)
    {
        if (!_walks.TryGetValue(from, out List<Walk>? choices) && !_walks.TryGetValue(352, out choices))
            throw new InvalidOperationException($"Kraid's data gives it nowhere to walk to from {from}.");
        // The roll of eight is the third through fifth digits of the number last drawn.
        int roll = Random.Last.Part(2, 3) % choices.Sum(choice => choice.Weight);
        foreach (Walk choice in choices)
        {
            if (roll < choice.Weight)
                return choice;
            roll -= choice.Weight;
        }

        throw new InvalidOperationException("Kraid's walks have weights that do not add up.");
    }

    /// <summary>With an eighth of its health gone it stops what it is doing and rises to its full height.</summary>
    private bool TryBeginGrowth()
    {
        if (Health.Current >= _initialHealth / 8 * 7 || _stage != Stage.Fighting)
            return false;
        _fight = Fight.HeadBeforeGrowth;
        _wait = 180;
        Room.Shake(2, 340);
        PlayPart("Foot", "foot_initial", restart: true);
        _foot = FootDoing.Resting;
        PlayPart("Arm", "arm_normal", restart: true);
        _armHarmless = true;
        ShowShields();
        HasBegunGrowing = true;
        return true;
    }

    /// <summary>Begins a spell of thinking, and sets its spikes back at their heights.</summary>
    private void BeginFirstThinking()
    {
        _fight = Fight.FirstThinking;
        LintsPlaced++;
        _thinking = ThinkingUpdates();
    }

    /// <summary>How long it thinks: a chance number of steps from one to seven, or two when chance gives none.</summary>
    private int ThinkingUpdates()
    {
        int steps = Random.Last.Part(0, 3);
        return (steps == 0 ? 2 : steps) * ThinkingStepUpdates;
    }

    /// <summary>Closes its mouth. If it has openings yet to come and was hit in the mouth, its eye soon glows again.</summary>
    private void CloseMouth()
    {
        if (_mouthReopenMarked && _mouthReopens > 1)
        {
            _mouthReopens--;
            _fight = Fight.WaitBeforeEyeGlow;
            _wait = 64;
            return;
        }

        (_mouthReopens, _mouthReopenMarked) = (0, false);
    }

    /// <summary>Pushes Samus out of its body, up and to the left, and hurts her.</summary>
    private void PushSamusOut(Player samus)
    {
        if (LeftEdgeAt(samus.Position.Y.Whole) >= samus.BodyRect.Right)
            return;
        int x = samus.Position.X.Whole >= 40 ? samus.Position.X.Whole - 8 : samus.Position.X.Whole;
        samus.Position = PixelPosition.FromWhole(x, Math.Max(samus.Position.Y.Whole - 8, EjectionLimitY));
        ShiftPlayer(PixelOffset.FromWhole(4, -8));
        if (!samus.IsInvincible)
            samus.TakeHit(new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Direction = PixelOffset.FromWhole(-1, 0), Source = this }, samus.Hurtbox);
    }

    /// <summary>Throws a rock up out of the floor near it.</summary>
    private void ThrowRisingRock()
    {
        // Everything about it is read from the number last drawn: how far from Kraid it starts and on which side, how
        // fast it sets off to the right, in sixteenths of a pixel, and which of the two rocks it looks like.
        RandomDraw draw = Random.Last;
        int across = draw.Part(0, 6);
        if (draw.Part(0, 1) == 0)
            across = -(across + 1);
        Pixels side = Pixels.FromNumber(draw.Part(4, 6) / 16.0);
        Fire(draw.Part(4, 1) == 1 ? "rising_rock_small" : "rising_rock", PixelPosition.FromWhole(Position.X.Whole + across, 432), new PixelVelocity(side, Pixels.FromWhole(-5)));
        Sound(RockSound);
    }

    /// <summary>Lets a rock fall from the ceiling.</summary>
    private void DropCeilingRock(int x)
    {
        Pixels fall = Pixels.FromNumber(0.25 + Random.Last.Part(0, 6) / 256.0);
        Fire("ceiling_rock", PixelPosition.FromWhole(x, 312), new PixelVelocity(Pixels.Zero, fall));
    }

    /// <summary>Its arm's poses.</summary>
    private ColliderPoses ArmPoses => _armPoses ?? throw new InvalidOperationException("Kraid's data has not been read.");

    /// <summary>Its foot's poses.</summary>
    private ColliderPoses FootPoses => _footPoses ?? throw new InvalidOperationException("Kraid's data has not been read.");

    /// <summary>
    /// Switches off the pieces of its arm that shots meet while its arm lets them through, and the pieces of its foot
    /// once it is dying.
    /// </summary>
    private void ShowShields()
    {
        foreach (Collider shield in EntityNode.CollidersOf(this))
        {
            if ((IsArmShield(shield) && !ArmTakesShots) || (IsFootShield(shield) && IsFinished))
                shield.Enabled = false;
        }
    }

    /// <summary>Moves it to a place across the room.</summary>
    private void SetX(int x) => Position = new PixelPosition(Pixels.FromWhole(x), Position.Y);

    /// <summary>
    /// Its eye's glow: its head plays on and each eye's red and green brighten a step, and once every one is at its
    /// brightest its eyes begin to dim.
    /// </summary>
    private void GlowEyes()
    {
        StepHead();
        int brightest = _channelSteps.Count - 1;
        for (int eye = 0; eye < _eyes.Length; eye++)
            _eyes[eye] = _eyes[eye] with { Red = Math.Min(_eyes[eye].Red + 1, brightest), Green = Math.Min(_eyes[eye].Green + 1, brightest) };
        if (_eyes.All(eye => eye.Red == brightest && eye.Green == brightest))
            _fight = Fight.UnglowEye;
        ShowColours();
    }

    /// <summary>
    /// Dims each eye's red and green a step toward <paramref name="band"/>'s where they differ, one at none going round
    /// to the brightest. Returns whether its eyes' red and green were already the band's.
    /// </summary>
    private bool DimEyes(EyeColour[] band)
    {
        int brightest = _channelSteps.Count - 1;
        int Dimmed(int step, int to) => step == to ? step : step == 0 ? brightest : step - 1;
        bool wereBand = true;
        for (int eye = 0; eye < _eyes.Length; eye++)
        {
            EyeColour shown = _eyes[eye];
            wereBand &= shown.Red == band[eye].Red && shown.Green == band[eye].Green;
            _eyes[eye] = shown with { Red = Dimmed(shown.Red, band[eye].Red), Green = Dimmed(shown.Green, band[eye].Green) };
        }

        return wereBand;
    }

    /// <summary>
    /// Shows the colours it is in on its body, head, arm and foot at once, and its parts' on the spikes and nails of
    /// its room; over its body's and head's, its eyes' colours, save in its room's own colours and as it dies. A change
    /// is worked out for a visual by the pictures it shows, so one that shows none yet is shown it once it does.
    /// </summary>
    private void ShowColours()
    {
        ColourChange body = IsFinished ? _deathColours : _bodyColours;
        bool eyesShown = !IsFinished && !_inRoomColours;
        foreach (ActorVisual visual in new[] { Visual, GetNodeOrNull<ActorVisual>("Head") }.OfType<ActorVisual>())
        {
            if (visual.Picture.Length == 0)
                continue;
            ShowOn(visual, body);
            // Its eyes' colours are asked for after its body's, and so are shown over them.
            visual.Recolour(EyeColours, eyesShown ? EyeSwaps(body.On(visual)) : []);
        }

        foreach (string part in new[] { "Arm", "Foot" })
        {
            if (GetNodeOrNull<ActorVisual>(part) is { } visual && visual.Picture.Length > 0)
                ShowOn(visual, _partColours);
        }

        foreach (KraidLint lint in Room.Enemies.OfType<KraidLint>())
            lint.ShowColours(_partColours);
        foreach (KraidNail nail in Room.Enemies.OfType<KraidNail>())
            nail.ShowColours(_partColours);
    }

    /// <summary>Shows its parts' colours on a nail of its room made since its turn, so that it never shows its own.</summary>
    public void ShowPartColoursOn(KraidNail nail)
    {
        ArgumentNullException.ThrowIfNull(nail);
        nail.ShowColours(_partColours);
    }

    /// <summary>Shows a change of colours on one of its visuals, in place of the one shown there before.</summary>
    private void ShowOn(ActorVisual visual, ColourChange colours)
    {
        if (_shownColours.TryGetValue(visual, out ColourChange? shown) && ReferenceEquals(shown, colours))
            return;
        colours.ShowOn(visual, OwnColours);
        _shownColours[visual] = colours;
    }

    /// <summary>
    /// Its eyes' colours as swaps over <paramref name="shown"/>, the colours its body's change shows on a visual: every
    /// colour that change shows as one its eyes are pictured in, and that colour itself where the change leaves it, is
    /// shown as that eye is now.
    /// </summary>
    private ColourSwap[] EyeSwaps(IReadOnlyList<ColourSwap> shown)
    {
        static bool Same(Color one, Color other) => one.R8 == other.R8 && one.G8 == other.G8 && one.B8 == other.B8;
        var swaps = new List<ColourSwap>();
        for (int eye = 0; eye < _eyes.Length; eye++)
        {
            Color pictured = ColourOf(_picturedEyes[eye]), now = ColourOf(_eyes[eye]);
            foreach (ColourSwap swap in shown)
            {
                if (Same(swap.To, pictured))
                    swaps.Add(new ColourSwap(swap.From, now, swap.Region));
            }

            if (!shown.Any(swap => Same(swap.From, pictured)))
                swaps.Add(new ColourSwap(pictured, now));
        }

        return [.. swaps];
    }

    /// <summary>A colour of its eyes as it is shown.</summary>
    private Color ColourOf(EyeColour eye) =>
        Color.Color8((byte)_channelSteps[eye.Red], (byte)_channelSteps[eye.Green], (byte)_channelSteps[eye.Blue]);

    /// <summary>Reads the colours of its eyes a group names, one for each eye, as the steps of each channel.</summary>
    private EyeColour[] ReadEyes(DataNode group, string name)
    {
        EyeColour[] eyes = [.. group.List(name).Select(ReadEye)];
        return eyes.Length > 0 ? eyes : throw group.Problem($"must give the colours of its eyes in \"{name}\"");
    }

    /// <summary>One colour of its eyes, written as six hexadecimal digits, each channel one of the steps a channel moves through.</summary>
    private EyeColour ReadEye(DataNode colour)
    {
        string text = colour.AsText();
        if (text.Length != 6 || !text.All(Uri.IsHexDigit))
            throw colour.Problem($"names the colour \"{text}\", which is not six hexadecimal digits");
        int Step(string channel)
        {
            int value = Convert.ToInt32(channel, 16);
            int step = _channelSteps.IndexOf(value);
            return step >= 0 ? step : throw colour.Problem($"names the colour \"{text}\", one of whose channels, {value}, is none of the steps of a channel");
        }

        return new EyeColour(Step(text[..2]), Step(text[2..4]), Step(text[4..]));
    }

    /// <summary>
    /// Sets how far its mouth is open: where it can be hurt, and where its head and body turn shots away, go with it.
    /// Wide open, it stays so for <paramref name="updates"/> updates.
    /// </summary>
    private void ShowMouth(string mouth, int updates)
    {
        _mouth = mouth;
        if (mouth == "open")
            _openLeft = updates;
        (_headPoses ?? throw new InvalidOperationException("Kraid's data has not been read.")).Take(mouth);
    }

    /// <summary>The colours of its body and head and of its parts as hurt as it is, and the colours of its eyes there.</summary>
    private (int Health, ColourChange Body, ColourChange Parts, EyeColour[] Eyes) BandShade()
    {
        foreach ((int Health, ColourChange Body, ColourChange Parts, EyeColour[] Eyes) band in _healthShades)
        {
            if (Health.Current >= band.Health)
                return band;
        }

        return _healthShades[^1];
    }

    /// <summary>
    /// Moves the flash of a hit in the mouth on. Only as the flash shows a frame does it take the colours of that
    /// frame, or of its band as hurt as it is then, its eyes with them, as it does on every update once its health is
    /// gone; between, it keeps the colours it last took, and until the first such time it shows its room's own.
    /// </summary>
    private void StepColours()
    {
        if (Health.IsEmpty)
        {
            // Dying, it flashes no more.
            _flashStep = -1;
            ShowFlashColours(hurt: false);
        }
        else if (_flashStep >= 0 && --_flashLeft <= 0)
        {
            bool hurt = false;
            if (_flashStep < _hurtFlash.Count)
                (hurt, _flashLeft) = _hurtFlash[_flashStep++];
            else
                _flashStep = -1;
            ShowFlashColours(hurt);
        }

        ShowColours();
    }

    /// <summary>Takes the colours of the flash of a hit, or of its band as hurt as it is, and its eyes those colours' own.</summary>
    private void ShowFlashColours(bool hurt)
    {
        _inRoomColours = false;
        (_bodyColours, _partColours) = hurt ? (_hurtBody, _hurtParts) : (BandShade().Body, BandShade().Parts);
        _eyes = [.. hurt ? _hurtEyes : BandShade().Eyes];
    }

    /// <summary>Shows one of its parts as its timeline has it.</summary>
    private void ShowPart(string part)
    {
        TimelineStep step = _parts[part].Current;
        GetNodeOrNull<ActorVisual>(part)?.Show(step);
    }

    /// <summary>Sets the depth in the room one of its parts is drawn at.</summary>
    private void SetPartDepth(string part, int depth)
    {
        if (GetNodeOrNull<ActorVisual>(part) is { } visual)
            visual.ZIndex = depth;
    }

    /// <summary>Moves its head's timeline on by one update.</summary>
    private void StepHead()
    {
        if (_headOver)
            return;
        StepPart("Head");
        _headOver = _parts["Head"].IsOver;
    }

    /// <summary>Starts one of its parts on a timeline.</summary>
    private void PlayPart(string part, string timeline, bool restart = false)
    {
        if (!restart && _partPlaying.GetValueOrDefault(part) == timeline)
            return;
        _partPlaying[part] = timeline;
        _parts[part] = new TimelinePlayer(Timelines.Get(timeline));
        // Its arm and foot are in front of every tile of the room, but behind the room's front tiles while it rises or
        // sinks; its head, from the first pose its fight shows on, is in front of what stands in the room for good.
        if (part == "Arm")
            SetPartDepth(part, timeline == "arm_rising_or_sinking" ? RoomDepth.Things : RoomDepth.InFrontOfRoom);
        else if (part == "Foot")
            SetPartDepth(part, timeline == "foot_initial" ? RoomDepth.Things : RoomDepth.InFrontOfRoom);
        else if (part == "Head" && _stage != Stage.Rising)
            SetPartDepth(part, RoomDepth.InFrontOfRoom);
        ShowPart(part);
        if (part == "Head")
            _headOver = false;
        if (part == "Foot")
            _walkEnd = false;
        if (part == "Arm")
            _swing = timeline == "arm_normal" ? Swing.BeforePause : Swing.Away;
    }

    /// <summary>Moves one of its parts' timelines on, and acts on its cues.</summary>
    private void StepPart(string part)
    {
        TimelinePlayer player = _parts[part];
        foreach (string cue in player.Step())
        {
            string[] parts = cue.Split(':');
            switch (parts[0])
            {
                case "sound":
                    Sound(parts[1]);
                    break;
                case "mouth":
                    ShowMouth(parts[1], player.Current.Updates);
                    break;
                case "pose":
                    // The pieces of its foot move with where the foot is drawn.
                    if (part == "Foot")
                        FootPoses.Take(parts[1], _footPlace);
                    else
                        ArmPoses.Take(parts[1]);
                    ShowShields();
                    break;
                case "swing_begins":
                    _swing = Swing.BeforePause;
                    break;
                case "reaches_pause":
                    _swing = Swing.PastPause;
                    break;
                case "slow_check":
                    // Below half its health the swing of its arm slows.
                    if (Health.Current < _initialHealth / 8 * 4 && _swing != Swing.Away)
                    {
                        _partPlaying[part] = "arm_slow";
                        _parts[part] = new TimelinePlayer(Timelines.Get("arm_slow"));
                        _swing = Swing.Away;
                        return;
                    }

                    break;
                case "step_left":
                    Position -= PixelOffset.FromWhole(3, 0);
                    break;
                case "step_right":
                    Position += PixelOffset.FromWhole(3, 0);
                    break;
                case "raise_body":
                    Position -= PixelOffset.FromWhole(0, 1);
                    break;
                case "lower_body":
                    Position += PixelOffset.FromWhole(0, 1);
                    Room.Shake(1, 10);
                    break;
                case "walk_end":
                    _walkEnd = true;
                    break;
                case "walk_on":
                    _walkEnd = false;
                    break;
                case "place":
                    _footPlace = new Vector2I(int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                    if (GetNodeOrNull<ActorVisual>(part) is { } placed)
                        placed.Position = new Vector2(_footPlace.X, 100 + _footPlace.Y);
                    break;
                default:
                    throw new InvalidOperationException($"Kraid's timeline \"{_partPlaying[part]}\" gives the cue \"{cue}\", which it does not know.");
            }
        }

        ShowPart(part);
    }
}
