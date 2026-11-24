using System.Globalization;
using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Crocomire: a great beast that cannot be hurt, only driven back. It lumbers toward Samus, spitting fire, and she
/// can never get past it. A shot in its open mouth knocks it back a step or more; driven far enough it steps onto a
/// bridge that gives way and drops it into acid, where it thrashes and melts. Its skeleton then comes down the
/// river, bursts through the spike wall behind Samus, and falls apart.
/// </summary>
/// <remarks>
/// Its body's timelines are "idle_sway", "awaiting_hit", "step_forward", "step_forward_after_delay", "step_back",
/// "stepping_back", "projectile_attack", "near_spike_wall_charge", "back_off_from_spike_wall",
/// "power_bomb_reaction", "bridge_collapsed", "in_acid", "melting_flesh", "melting_skull", "skeleton_flowing",
/// "skeleton_falls_apart" and "dead". Their cues are "step:&lt;pixels&gt;:&lt;always|unless_hit|on_screen&gt;" with
/// ":dust" added for a step that raises dust; "dust:&lt;across&gt;"; "fight", at which it decides what to do next;
/// "maybe_projectile"; "shake"; "toward_spike_wall"; and "skeleton_collapsed". The visual "Tongue" plays "tongue".
/// Its hurtbox "Mouth" is the only part a shot can move it by. Its shots are "fire" and "spike_piece". All its places
/// are in the room's own pixels.
/// </remarks>
public partial class Crocomire : Enemy
{
    /// <summary>The terrain patches that crumble the first blocks of the bridge as it steps onto it, in order.</summary>
    [Export]
    public Godot.Collections.Array<string> BridgeCrumblePatches { get; set; } = [];

    /// <summary>The terrain patch that takes the bridge away.</summary>
    [Export]
    public string BridgeCollapsePatch { get; set; } = "";

    /// <summary>The terrain patch that opens the spike wall its skeleton breaks through.</summary>
    [Export]
    public string SpikeWallPatch { get; set; } = "";

    /// <summary>The music of the fight, by name.</summary>
    [Export]
    public string FightMusic { get; set; } = "";

    /// <summary>The music while its skeleton comes down the river, and once it is over, by name.</summary>
    [Export]
    public string TensionMusic { get; set; } = "";

    /// <summary>How near across Samus must come, in pixels, to wake it.</summary>
    [Export, DataValue("wake_distance")]
    public int WakeDistance { get; set; } = 224;

    /// <summary>How far across the room, in pixels, it charges the spike wall from.</summary>
    [Export, DataValue("spike_wall_charge_x")]
    public int SpikeWallChargeX { get; set; } = 768;

    /// <summary>How far across the room the bridge begins to crumble under it, in pixels.</summary>
    [Export, DataValue("bridge_approach_x")]
    public int BridgeApproachX { get; set; } = 1536;

    /// <summary>How far across the room the bridge gives way under it, in pixels.</summary>
    [Export, DataValue("bridge_collapse_x")]
    public int BridgeCollapseX { get; set; } = 1600;

    /// <summary>How far down the room it sinks to in the acid, and rises back to, in pixels.</summary>
    [Export, DataValue("sunken_y")]
    public int SunkenY { get; set; } = 280;

    /// <summary>How far down the room it rises to out of the acid, in pixels.</summary>
    [Export, DataValue("risen_y")]
    public int RisenY { get; set; } = 218;

    /// <summary>How many updates it rests under the acid between heaves.</summary>
    [Export, DataValue("submerged_pause")]
    public int SubmergedPause { get; set; } = 49;

    /// <summary>How many updates each of its two meltings takes.</summary>
    [Export, DataValue("dissolve_updates")]
    public int DissolveUpdates { get; set; } = 98;

    /// <summary>How far across the room Samus must have come back, in pixels, for its skeleton to break out.</summary>
    [Export, DataValue("wall_approach_x")]
    public int WallApproachX { get; set; } = 640;

    /// <summary>How many things it drops when its skeleton falls apart.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>The effect of a cloud of dust, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of a shot bursting harmlessly on it.</summary>
    [Export, DataValue("dud_effect")]
    public string DudEffect { get; set; } = "";

    /// <summary>The sound of it spitting fire, by name.</summary>
    [Export, DataValue("spit_sound")]
    public string SpitSound { get; set; } = "";

    /// <summary>The sound of its step shaking the room.</summary>
    [Export, DataValue("shake_sound")]
    public string ShakeSound { get; set; } = "";

    /// <summary>The sound of the bridge giving way.</summary>
    [Export, DataValue("bridge_sound")]
    public string BridgeSound { get; set; } = "";

    /// <summary>The sound of the acid burning it.</summary>
    [Export, DataValue("acid_sound")]
    public string AcidSound { get; set; } = "";

    /// <summary>Its cry as it melts.</summary>
    [Export, DataValue("melt_cry")]
    public string MeltCry { get; set; } = "";

    /// <summary>The sound of the wall rumbling before its skeleton breaks out.</summary>
    [Export, DataValue("rumble_sound")]
    public string RumbleSound { get; set; } = "";

    /// <summary>The sound of a big explosion.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    // The slants of the nine shots of a volley: how far down each is aimed over 64 pixels to the left.
    private static readonly int[] VolleyGradients = [0, 32, -16, 0, 32, -16, 0, 32, 0];

    // The rumbling of the hidden wall: it shakes between two heights, a number of times at a speed, harder and then easing.
    private static readonly (int Return, int Extreme, int Repeats, int Speed)[] Rumbles =
        [(0, -1, 10, 1), (1, -1, 8, 1), (1, -2, 12, 1), (2, -2, 16, 2), (2, -4, 16, 2), (1, -2, 8, 1), (1, -1, 3, 1)];

    private enum Phase
    {
        Dormant,
        WaitingForFirstDamage,
        WaitingForSecondDamage,
        SteppingForward,
        SteppingBack,
        ProjectileAttack,
        NearSpikeWallCharge,
        BackingOffSpikeWall,
        PowerBombCharge,
    }

    private enum Dying
    {
        Fighting,
        InAcid,
        FlowingDownRiver,
        WaitingBehindWall,
        Rumbling,
        BaringSkeleton,
        WaitingToFall,
        Arcing,
        Collapsing,
        Corpse,
    }

    // What it does in the acid, in order: sink, rest, rise, or melt.
    private enum Acid
    {
        Sink,
        Pause,
        Rise,
        MeltFlesh,
        MeltSkull,
    }

    private static readonly Acid[] AcidScript =
    [
        Acid.Sink, Acid.Pause, Acid.Rise, Acid.Sink, Acid.Pause, Acid.Rise, Acid.Sink,
        Acid.Rise, Acid.MeltFlesh, Acid.Sink, Acid.Pause, Acid.Rise, Acid.Sink, Acid.Pause, Acid.Rise, Acid.Sink,
        Acid.Rise, Acid.MeltSkull, Acid.Sink,
    ];

    private Phase _phase;
    private Dying _dying;
    private int _steps;
    private bool _hitPending;
    private int _shotsSpat;
    private int _approachStages;
    private int _acidStep;
    private int _wait;
    private Pixels _downSpeed;
    private Pixels _downQuickening;
    private int _acidSoundIn = 1;
    private int _rumble;
    private int _rumbleRepeats;
    private int _rumbleOffset;

    /// <summary>How far the wall it hides behind is rattled up or down this update, in pixels; zero when it is not.</summary>
    public int WallRumble => _dying == Dying.Rumbling ? _rumbleOffset : 0;
    private bool _rumbleToExtreme;
    private bool _skeletonCollapsed;
    private TimelinePlayer? _tongue;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        Play("idle_sway");
        _tongue = new TimelinePlayer(Timelines.Get("tongue"));
    }

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part) =>
        base.ReactionTo(hit, part) with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never };

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (_dying != Dying.Fighting)
            return;
        if (hit.Carries(new HitKind(HitKinds.PowerBomb)))
        {
            // A power bomb drives it back three steps wherever it lands.
            _steps = 3;
            if (_phase == Phase.PowerBombCharge)
                return;
            _phase = Phase.PowerBombCharge;
            Play("power_bomb_reaction", restart: true);
            return;
        }

        PixelPosition at = hit.Source is ICollisionOwner source ? source.Position : Position;
        if (part.Name != "Mouth")
        {
            if (DudEffect.Length > 0)
                LeaveEffect(DudEffect, at);
            return;
        }

        // Only a shot in the mouth moves it, and only while its near edge is in view.
        int steps = 0;
        if (IsInView(Engine.World.EntityNode.ColliderOf(this, ColliderRole.Body)?.Size.X / 2 ?? 0))
        {
            bool beam = hit.Carries(new HitKind(HitKinds.Beam));
            if (beam && !hit.Carries(new HitKind(HitKinds.Charged)))
            {
                if (DudEffect.Length > 0)
                    LeaveEffect(DudEffect, at);
                return;
            }

            steps = hit.Carries(new HitKind(HitKinds.SuperMissile)) ? 3 : hit.Carries(new HitKind(HitKinds.Missile)) ? 1 : beam ? 2 : -1;
        }

        if (steps < 0)
            return;
        _steps += steps;
        _hitPending = true;
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "step":
                if (parts.Length > 3)
                    RaiseDust((Random.Last.Part(8, 1) == 1 ? -1 : 1) * Random.Last.Part(0, 5));
                bool moves = parts[2] switch
                {
                    "always" => true,
                    "unless_hit" => !_hitPending,
                    _ => IsInView(4),
                };
                if (moves)
                    MoveAcrossBy(Pixels.FromWhole(int.Parse(parts[1], CultureInfo.InvariantCulture)));
                break;
            case "dust":
                RaiseDust(int.Parse(parts[1], CultureInfo.InvariantCulture));
                break;
            case "fight":
                if (Decide() is { } next)
                    Play(next, restart: true);
                break;
            case "maybe_projectile":
                // One time in four it stops to spit.
                if (Random.Last.Part(0, 12) < 1024)
                {
                    _phase = Phase.ProjectileAttack;
                    _shotsSpat = 0;
                    Play("projectile_attack", restart: true);
                }

                break;
            case "shake":
                Room.Shake(2, 5);
                Sound(ShakeSound);
                break;
            case "toward_spike_wall":
                if (MoveAcrossBy(Pixels.FromWhole(-4)))
                {
                    _phase = Phase.BackingOffSpikeWall;
                    Play("back_off_from_spike_wall", restart: true);
                }
                else
                {
                    RaiseDust((Random.Last.Part(8, 1) == 1 ? -32 : 32) + Random.Last.Part(0, 4));
                }

                break;
            case "skeleton_collapsed":
                _skeletonCollapsed = true;
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (_dying is Dying.Fighting)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_dying == Dying.Fighting)
        {
            BlockSamus();
            WatchBridge();
        }
        else
        {
            StepDeath();
        }

        if (_tongue is not null && GetNodeOrNull<ActorVisual>("Tongue") is { } tongue)
        {
            _tongue.Step();
            tongue.SetShown(_dying is Dying.Fighting or Dying.InAcid);
            tongue.Show(_tongue.Current);
        }
    }

    /// <summary>Decides, at a pause in its timeline, what it does next; returns the timeline to change to, if any.</summary>
    private string? Decide()
    {
        switch (_phase)
        {
            case Phase.Dormant:
                if (Math.Abs(Position.X.Whole - Target.X.Whole) >= WakeDistance)
                    return null;
                _phase = Phase.WaitingForFirstDamage;
                if (FightMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(FightMusic);
                return "awaiting_hit";

            case Phase.WaitingForFirstDamage:
                return StepBackIfHit(Phase.WaitingForSecondDamage);

            case Phase.WaitingForSecondDamage or Phase.NearSpikeWallCharge:
                return StepBackIfHit(Phase.SteppingBack);

            case Phase.SteppingForward:
                if (TakePendingHit() && _steps != 0)
                {
                    _phase = Phase.SteppingBack;
                    return "step_back";
                }

                if (Position.X.Whole >= SpikeWallChargeX)
                    return null;
                _phase = Phase.NearSpikeWallCharge;
                return "near_spike_wall_charge";

            case Phase.ProjectileAttack:
                if (StepBackIfHit(Phase.SteppingBack) is { } back)
                    return back;
                if (_shotsSpat < VolleyGradients.Length)
                {
                    Spit(VolleyGradients[_shotsSpat++]);
                    return null;
                }

                _phase = Phase.SteppingForward;
                return "step_forward_after_delay";

            case Phase.SteppingBack:
                if (_steps != 0)
                    _steps--;
                if (_steps != 0)
                    return "stepping_back";
                _phase = Phase.SteppingForward;
                return "step_forward";

            case Phase.BackingOffSpikeWall:
                if (Position.X.Whole < SpikeWallChargeX)
                    return null;
                _phase = Phase.SteppingForward;
                return "step_forward";

            case Phase.PowerBombCharge:
                if (_steps - 1 >= 2)
                {
                    _steps--;
                    return null;
                }

                _steps = 0;
                _phase = Phase.SteppingForward;
                return "step_forward";

            default:
                throw new InvalidOperationException($"Undefined Crocomire phase {_phase}.");
        }
    }

    /// <summary>Takes note of a waiting hit, and returns whether there was one.</summary>
    private bool TakePendingHit()
    {
        bool pending = _hitPending;
        _hitPending = false;
        return pending;
    }

    /// <summary>Steps back if a hit is waiting, going on to the given phase.</summary>
    private string? StepBackIfHit(Phase then)
    {
        if (!TakePendingHit())
            return null;
        _phase = then;
        return "step_back";
    }

    /// <summary>Spits one ball of fire, aimed on a slant.</summary>
    private void Spit(int gradient)
    {
        int bearing = Turns.AngleOf(-64, gradient);
        Pixels speed = Shot("fire").Speed;
        Fire("fire", PixelPosition.FromWhole(Position.X.Whole - 32, Position.Y.Whole - 16), new PixelVelocity(Turns.Cosine(bearing, speed), Turns.Sine(bearing, speed)));
        Sound(SpitSound);
    }

    /// <summary>Raises a puff of dust at its feet, a way across from its middle.</summary>
    private void RaiseDust(int across)
    {
        if (DustEffect.Length == 0)
            return;
        int below = (Engine.World.EntityNode.ColliderOf(this, ColliderRole.Body)?.Size.Y ?? 0) / 2 - 16;
        LeaveEffect(DustEffect, PixelPosition.FromWhole(Position.X.Whole + across, Position.Y.Whole + below + Random.Last.Part(0, 3)));
    }

    /// <summary>Keeps Samus from ever getting past it: at its near edge she is hurt and thrown back.</summary>
    private void BlockSamus()
    {
        Player samus = Room.Player;
        int halfWidth = (Engine.World.EntityNode.ColliderOf(this, ColliderRole.Body)?.Size.X ?? 0) / 2;
        int edge = Position.X.Whole - halfWidth - samus.BodyRect.Width / 2;
        if (edge >= samus.Position.X.Whole)
            return;
        samus.TakeHit(new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Direction = PixelOffset.FromWhole(-1, 0), Source = this }, samus.Hurtbox);
        samus.Position = new PixelPosition(samus.Position.X.WithWhole(edge), samus.Position.Y);
        ShiftPlayer(PixelOffset.FromWhole(-4, -1));
    }

    /// <summary>Crumbles the bridge as it steps onto it, and drops it into the acid once it is far enough out.</summary>
    private void WatchBridge()
    {
        int x = Position.X.Whole;
        if (x < BridgeApproachX)
        {
            _approachStages = 0;
        }
        else
        {
            int stage = (x - BridgeApproachX) / 16;
            if (stage >= _approachStages && stage <= BridgeCrumblePatches.Count)
            {
                _approachStages = stage + 1;
                if (stage > 0)
                    Room.Terrain.ApplyPatch(BridgeCrumblePatches[stage - 1]);
                if (DustEffect.Length > 0)
                    LeaveEffect(DustEffect, PixelPosition.FromWhole(BridgeApproachX + 16 * (stage + (stage > 0 ? 1 : 0)), 176));
            }
        }

        if (x < BridgeCollapseX)
            return;
        _dying = Dying.InAcid;
        _acidStep = 0;
        Play("bridge_collapsed", restart: true);
        _downSpeed = Pixels.Zero;
        _downQuickening = Pixels.FromNumber(1.0 / 32);
        Sound(BridgeSound);
        if (BridgeCollapsePatch.Length > 0)
            Room.Terrain.ApplyPatch(BridgeCollapsePatch);
        for (int puff = 0; puff < 7 && DustEffect.Length > 0; puff++)
            LeaveEffect(DustEffect, PixelPosition.FromWhole(BridgeApproachX + 16 * puff, puff is 0 or 2 ? 176 : 192));
    }

    /// <summary>Moves its long death on by one update.</summary>
    private void StepDeath()
    {
        switch (_dying)
        {
            case Dying.InAcid:
                StepInAcid();
                break;

            case Dying.FlowingDownRiver:
                // Its skeleton drifts back along the river to behind the wall where the fight began.
                Position = PixelPosition.FromWhole(Position.X.Whole - 2, 220);
                if (Position.X.Whole >= 480)
                    break;
                Position = PixelPosition.FromWhole(480, 54);
                _dying = Dying.WaitingBehindWall;
                break;

            case Dying.WaitingBehindWall:
                if (Target.X.Whole >= WallApproachX)
                    break;
                if (FightMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(FightMusic);
                (_rumble, _rumbleOffset, _rumbleToExtreme, _rumbleRepeats) = (0, 0, false, Rumbles[0].Repeats);
                _dying = Dying.Rumbling;
                break;

            case Dying.Rumbling:
                Rumble();
                break;

            case Dying.BaringSkeleton:
                if (--_wait > 0)
                    break;
                Position = PixelPosition.FromWhole(480, 54);
                _wait = 80;
                Velocity = PixelVelocity.Zero;
                if (SpikeWallPatch.Length > 0)
                    Room.Terrain.ApplyPatch(SpikeWallPatch);
                // The spike wall flies apart, top to bottom, each piece slower than the one above.
                int[] fastest = [6, 5, 4, 3, 2, 2, 1, 1];
                for (int piece = fastest.Length - 1; piece >= 0; piece--)
                    Fire("spike_piece", PixelPosition.FromWhole(528, 56 + 16 * piece), new PixelVelocity(Pixels.FromWhole(fastest[piece]).Share(1, 2), Pixels.FromNumber(-143.0 / 32)));
                Sound(ExplosionSound);
                _dying = Dying.WaitingToFall;
                break;

            case Dying.WaitingToFall:
                if (--_wait <= 0)
                    _dying = Dying.Arcing;
                break;

            case Dying.Arcing:
                Pixels faster = Velocity.X + Pixels.FromNumber(1.0 / 32);
                Velocity = Velocity.WithX(faster.Whole > 5 ? faster.WithWhole(5) : faster);
                Position += new PixelOffset(Velocity.X, Pixels.FromNumber(0.875));
                if (Position.X.Whole < 576)
                    break;
                Sound(ExplosionSound);
                Play("skeleton_falls_apart", restart: true);
                _dying = Dying.Collapsing;
                break;

            case Dying.Collapsing:
                if (!_skeletonCollapsed)
                    break;
                Play("dead", restart: true);
                Position += PixelOffset.FromWhole(64, 21);
                for (int drop = 0; drop < DeathDrops; drop++)
                {
                    RandomDraw draw = Random.Draw();
                    Drop(Drops, PixelPosition.FromWhole(576 + draw.Part(0, 7), 96 + draw.Part(8, 6)));
                }

                if (TensionMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(TensionMusic);
                RaiseDust(-16);
                RaiseDust(16);
                // The fight is won; what is left of it lies where it fell.
                Room.MarkGone(Name);
                _dying = Dying.Corpse;
                break;
        }
    }

    /// <summary>Moves one update of its thrashing in the acid: sinking, resting, heaving up, and twice melting.</summary>
    private void StepInAcid()
    {
        if (--_acidSoundIn <= 0)
        {
            _acidSoundIn = 32;
            Sound(AcidSound);
        }

        switch (AcidScript[_acidStep])
        {
            case Acid.Sink:
                _hitPending = false;
                if (Position.Y.Whole >= SunkenY)
                {
                    NextAcidStep();
                    _wait = SubmergedPause;
                    _downQuickening = Pixels.Zero;
                    break;
                }

                Pixels quicker = _downQuickening + Pixels.FromNumber(7.0 / 512), most = Pixels.FromNumber(3.0 / 16);
                _downQuickening = quicker < most ? quicker : most;
                Pixels speed = _downSpeed + _downQuickening;
                _downSpeed = speed.Whole >= 3 ? Pixels.FromWhole(3) : speed;
                Position += new PixelOffset(Pixels.Zero, _downSpeed);
                break;

            case Acid.Pause:
                if (--_wait > 0)
                    break;
                NextAcidStep();
                _downSpeed = Pixels.FromWhole(3);
                break;

            case Acid.Rise:
                if (Position.Y.Whole < RisenY)
                {
                    NextAcidStep();
                    _downSpeed = Pixels.Zero;
                    _downQuickening = Pixels.FromNumber(1.0 / 32);
                    _wait = DissolveUpdates;
                    break;
                }

                Pixels slower = _downQuickening + Pixels.FromNumber(1.0 / 256), least = Pixels.FromNumber(31.0 / 256);
                _downQuickening = slower < least ? slower : least;
                Pixels rise = _downSpeed - _downQuickening;
                _downSpeed = rise.IsNegative ? Pixels.FromNumber(255.0 / 256) : rise;
                Position -= new PixelOffset(Pixels.Zero, _downSpeed);
                break;

            case Acid.MeltFlesh or Acid.MeltSkull:
                if (_wait == DissolveUpdates)
                {
                    Play(AcidScript[_acidStep] == Acid.MeltFlesh ? "melting_flesh" : "melting_skull", restart: true);
                    Sound(MeltCry);
                }

                if (--_wait <= 0)
                    NextAcidStep();
                break;
        }
    }

    /// <summary>Goes on to the next thing it does in the acid; after the last, its skeleton is carried off down the river.</summary>
    private void NextAcidStep()
    {
        if (++_acidStep < AcidScript.Length)
            return;
        _dying = Dying.FlowingDownRiver;
        if (TensionMusic.Length > 0)
            Room.Session.Sounds.RequestMusic(TensionMusic);
        Play("skeleton_flowing", restart: true);
    }

    /// <summary>Shakes the hidden wall up and down, harder and then easing, until its skeleton is ready to break out.</summary>
    private void Rumble()
    {
        if (_rumble >= Rumbles.Length)
        {
            _wait = 129;
            _dying = Dying.BaringSkeleton;
            return;
        }

        (int back, int extreme, _, int speed) = Rumbles[_rumble];
        int target = _rumbleToExtreme ? extreme : back;
        if (_rumbleOffset != target)
        {
            _rumbleOffset += _rumbleOffset >= target ? -speed : speed;
            return;
        }

        if (!_rumbleToExtreme)
        {
            _rumbleToExtreme = true;
            return;
        }

        _rumbleToExtreme = false;
        if (_rumbleRepeats > 0)
        {
            _rumbleRepeats--;
            Sound(RumbleSound);
            return;
        }

        if (++_rumble < Rumbles.Length)
            _rumbleRepeats = Rumbles[_rumble].Repeats;
    }
}
