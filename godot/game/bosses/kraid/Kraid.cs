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
/// <para>Its hurtbox "Mouth" is the only place it can be hurt; its other hurtboxes are its body, and its hitboxes
/// "Arm" and "Foot" hurt to touch. Its data gives "mouth_rects", the mouth's rectangle for each way it is open;
/// "body_edge", the left edge of its body from its place for each height at or above "from"; "ceiling_rocks" and
/// "sink_crumbles", each with a terrain "patch" and where a rock falls; "walks", for each place it may stand the
/// places it may walk to next, each with a "weight", a "to" and how many "updates" it then thinks; and the names of
/// its other terrain patches. Its shots are "spit_rock", "rising_rock" and "ceiling_rock". All its places are in the
/// room's own pixels.</para>
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

    /// <summary>How many updates its eye takes to glow, and again to dim.</summary>
    [Export, DataValue("eye_glow_updates")]
    public int EyeGlowUpdates { get; set; } = 16;

    /// <summary>How many steps the room's backdrop takes to fade.</summary>
    [Export, DataValue("backdrop_fade_steps")]
    public int BackdropFadeSteps { get; set; } = 12;

    /// <summary>How many things it drops when it has sunk.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>The effect of a shot that does it no harm, by the data file of its timeline.</summary>
    [Export, DataValue("dud_effect")]
    public string DudEffect { get; set; } = "";

    /// <summary>The sound of a rock flying, and of its rumbling as it sinks, by name.</summary>
    [Export, DataValue("rock_sound")]
    public string RockSound { get; set; } = "";

    /// <summary>The terrain patch of the spikes crumbling as it dies.</summary>
    [Export, DataValue("death_spikes_patch")]
    public string DeathSpikesPatch { get; set; } = "";

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

    private sealed record Crumble(int At, int RockX, string Patch);

    private sealed record Walk(int Weight, int To, int Updates);

    private readonly Dictionary<string, PixelRect> _mouthRects = new(StringComparer.Ordinal);
    private readonly List<(int From, int Left)> _bodyEdge = [];
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
    private int _glow;
    private int _fade;
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
        _mouthRects.Clear();
        foreach ((string name, DataNode rect) in data.Get("mouth_rects").Entries())
            _mouthRects[name] = new PixelRect(rect.Int("left"), rect.Int("top"), rect.Int("right") - rect.Int("left"), rect.Int("bottom") - rect.Int("top"));
        _bodyEdge.Clear();
        _bodyEdge.AddRange(data.List("body_edge").Select(row => (row.Int("from"), row.Int("left"))));
        if (_bodyEdge.Count == 0)
            throw data.Problem("must give at least one row of its body's edge");
        _ceilingRocks.Clear();
        _ceilingRocks.AddRange(data.List("ceiling_rocks").Select(rock => new Crumble(0, rock.Int("x"), rock.Text("patch"))));
        _sinkCrumbles.Clear();
        _sinkCrumbles.AddRange(data.List("sink_crumbles").Select(step => new Crumble(step.Int("y"), step.Int("x"), step.Text("patch"))));
        _walks.Clear();
        foreach (DataNode row in data.List("walks"))
            _walks[row.Int("from")] = [.. row.List("to").Select(walk => new Walk(walk.Int("weight"), walk.Int("to"), walk.Int("updates")))];
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) =>
        (_stage is Stage.Fighting or Stage.Growing || (_stage == Stage.Dying && _death != Death.Sink)) && base.IsHitBy(side);

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        // A beam that is not charged goes through its open mouth as if the mouth were not there.
        if (part.Name == "Mouth" && !hit.Carries(new HitKind(HitKinds.Charged)) &&
            hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Ice), new HitKind(HitKinds.Wave), new HitKind(HitKinds.Spazer), new HitKind(HitKinds.Plasma)]))
            return HitResult.Ignored;
        HitResult result = base.TakeHit(hit, part);
        if (part.Name == "Mouth" && result is HitResult.Hurt or HitResult.Destroyed && _mouthReopens > 0)
            _mouthReopenMarked = true;
        return result;
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
            return;
        // A shot at its body does no harm, but makes its eye glow and its mouth open; a charged one, three times over.
        if (DudEffect.Length > 0 && hit.Source is Node2D shot)
            LeaveEffect(DudEffect, PixelPosition.FromWhole((int)shot.Position.X, (int)shot.Position.Y));
        if (_stage != Stage.Fighting || _fight != Fight.FirstThinking)
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
        }

        return false;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (IsFinished || (part.Name == "Arm" && _armHarmless))
            return;
        if (part.Name != "Arm")
        {
            base.Touch(part);
            return;
        }

        // Its arm knocks Samus back, and sets off the lowest spike.
        ShiftPlayer(PixelOffset.FromWhole(4, -8));
        ArmTouched?.Invoke();
        base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        _initialHealth = Health.Current;
        Position = PixelPosition.FromWhole(RiseStart.X, RiseStart.Y);
        _stage = Stage.Rising;
        _rise = Rise.Wait;
        _wait = RiseWaitUpdates;
        Visual?.SetShown(false);
        Play("body");
        PlayPart("Arm", "arm_rising_or_sinking");
        PlayPart("Foot", "foot_initial");
        PlayPart("Head", "head_roar");
        _headOver = true;
        _foot = FootDoing.Resting;
        ShowMouth("closed");
        if (GetNodeOrNull<ActorVisual>("Arm") is { } arm)
            arm.Position = new Vector2(0, -44);
        if (GetNodeOrNull<ActorVisual>("Foot") is { } foot)
            foot.Position = new Vector2(0, 100);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
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

                // While its mouth is wide open a rock comes out every sixteenth update.
                if (_mouth == "open" && _openLeft > 0 && _openLeft-- % 16 == 0)
                {
                    RandomDraw draw = Random.Draw();
                    Pixels[] speeds = [Pixels.FromWhole(-4), Pixels.FromNumber(-3.75), Pixels.FromNumber(-4.75), Pixels.FromNumber(-4.5), Pixels.FromNumber(-4.75), Pixels.FromWhole(-4), Pixels.FromNumber(-4.5), Pixels.FromNumber(-3.75)];
                    Fire("spit_rock", Position + PixelOffset.FromWhole(16, -96), new PixelVelocity(speeds[draw.Part(0, 3)], Pixels.FromWhole(-4)));
                }

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
                _glow = 0;
                PlayPart("Head", "head_eye_glow", restart: true);
                StepHead();
                ShowEyeGlow(++_glow);
                break;

            case Fight.GlowEye:
                StepHead();
                ShowEyeGlow(++_glow);
                if (_glow >= EyeGlowUpdates)
                    _fight = Fight.UnglowEye;
                break;

            case Fight.UnglowEye:
                ShowEyeGlow(--_glow);
                if (_glow > 0)
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
                    Room.Terrain.ApplyPatch(rock.Patch);
                }

                if (Position.Y.Whole < GrownY)
                    _growth = Growth.BringInFront;
                break;

            case Growth.BringInFront:
                ZIndex = 1;
                _armHarmless = false;
                _growth = Growth.SettleParts;
                break;

            case Growth.SettleParts:
                PlayPart("Foot", "foot_neutral");
                _growth = Growth.ShowBackdrop;
                break;

            case Growth.ShowBackdrop:
                _fade = 0;
                _growth = Growth.FadeBackdrop;
                break;

            case Growth.FadeBackdrop:
                if (++_fade <= BackdropFadeSteps)
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
                _fade = 0;
                if (DeathSpikesPatch.Length > 0)
                    Room.Terrain.ApplyPatch(DeathSpikesPatch);
                _death = Death.FadeOut;
                break;

            case Death.FadeOut:
                StepHead();
                if (++_fade > BackdropFadeSteps)
                    _death = Death.ShowBody;
                break;

            case Death.ShowBody:
                StepHead();
                ZIndex = 0;
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
                    Room.Terrain.ApplyPatch(crumble.Patch);
                }

                Position += PixelOffset.FromWhole(0, 1);
                if (Position.Y.Whole < SunkY)
                    break;
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
                    (_sinking, _fade) = (Sinking.FadeIn, 0);
                break;

            case Sinking.FadeIn:
                if (++_fade <= BackdropFadeSteps)
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
        int roll = Random.Draw().Part(0, 3) % choices.Sum(choice => choice.Weight);
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
        RandomDraw draw = Random.Draw();
        int across = draw.Part(0, 6);
        if (draw.Part(0, 1) == 0)
            across = -(across + 1);
        Pixels side = Pixels.FromNumber(draw.Part(8, 4) / 16.0);
        Fire("rising_rock", PixelPosition.FromWhole(Position.X.Whole + across, 432), new PixelVelocity(draw.Part(12, 1) == 1 ? side : -side, Pixels.FromWhole(-5)));
        Sound(RockSound);
    }

    /// <summary>Lets a rock fall from the ceiling.</summary>
    private void DropCeilingRock(int x)
    {
        Pixels fall = Pixels.FromNumber(0.25 + Random.Draw().Part(0, 6) / 256.0);
        Fire("ceiling_rock", PixelPosition.FromWhole(x, 312), new PixelVelocity(Pixels.Zero, fall));
    }

    /// <summary>Moves it to a place across the room.</summary>
    private void SetX(int x) => Position = new PixelPosition(Pixels.FromWhole(x), Position.Y);

    /// <summary>Shows how far its eye glows, out of the updates its glowing takes.</summary>
    private void ShowEyeGlow(int glow)
    {
        if (GetNodeOrNull<ActorVisual>("Head") is { } head)
            head.Tint(Colors.White.Lerp(new Color(1f, 0.6f, 0.6f), Math.Clamp((float)glow / Math.Max(1, EyeGlowUpdates), 0f, 1f)));
    }

    /// <summary>Sets how far its mouth is open: where it can be hurt goes with it.</summary>
    private void ShowMouth(string mouth)
    {
        _mouth = mouth;
        if (mouth == "open")
            _openLeft = 64;
        if (GetNodeOrNull<Collider>("Mouth") is not { } collider)
            return;
        collider.Enabled = _mouthRects.TryGetValue(mouth, out PixelRect rect);
        if (collider.Enabled)
            collider.Rect = rect;
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
        GetNodeOrNull<ActorVisual>(part)?.Show(_parts[part].Current.Picture);
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
                    ShowMouth(parts[1]);
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
                    if (GetNodeOrNull<ActorVisual>(part) is { } placed)
                        placed.Position = new Vector2(int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), 100 + int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                    break;
                default:
                    throw new InvalidOperationException($"Kraid's timeline \"{_partPlaying[part]}\" gives the cue \"{cue}\", which it does not know.");
            }
        }

        GetNodeOrNull<ActorVisual>(part)?.Show(player.Current.Picture);
    }
}
