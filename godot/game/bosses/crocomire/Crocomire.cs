using System.Globalization;
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
/// Crocomire: a great beast that cannot be hurt, only driven back. It lumbers toward Samus, spitting fire, and she
/// can never get past it. A shot in its open mouth knocks it back a step or more; driven far enough it steps onto a
/// bridge that gives way and drops it into acid, where it thrashes and melts. Its skeleton then comes down the
/// river, bursts through the spike wall behind Samus, and falls apart.
/// </summary>
/// <remarks>
/// <para>Its body's timelines are "idle_sway", "awaiting_hit", "step_forward", "step_forward_after_delay",
/// "step_back", "stepping_back", "projectile_attack", "near_spike_wall_charge", "back_off_from_spike_wall",
/// "power_bomb_reaction_mouth_fully_open", "power_bomb_reaction_mouth_partially_open",
/// "power_bomb_reaction_mouth_not_open", "bridge_collapsed", the melting ones its data's "melt_looks" name,
/// "skeleton_flowing_down_river", "skeleton_falling", "skeleton_falls_apart" and "dead". Their cues are
/// "step:&lt;pixels&gt;:&lt;always|unless_hit|on_screen&gt;" with ":dust" added for a step that raises dust;
/// "dust:&lt;across&gt;"; "fight", at which it decides what to do next; "maybe_projectile"; "shake";
/// "toward_spike_wall"; "skeleton_collapsed"; and "pose:" and a name, which puts its parts where its data's "poses"
/// have them (<see cref="ColliderPoses"/>). The visual "Tongue" plays "tongue" and, once it has begun to melt,
/// "tongue_melting". Its shots are "fire", "bridge_fragment" and "spike_piece". All its places are in the room's own
/// pixels.</para>
/// <para>Its pieces are "Part1" through "Part10", each a hurtbox of that name for a shot and a hitbox named for it with
/// "Touch" after for Samus, a pixel apart at their edges as the cartridge meets the two; a shot or Samus meets the
/// first of them, in that order, that it reaches. A pose says what each does: under "shots", "mouth" for the one a shot moves it by, "dust" for one a shot
/// bursts on, and "absorbed" or "plain" for one that swallows a shot; under "touches", "claw" for one that hurts and
/// knocks back, "plain" for one that only hurts, and "none"; and under "mouth" how far its mouth is open,
/// "fully_open", "partially_open" or "not_open", which picks how it answers a power bomb.</para>
/// <para>Its data also gives "volley", how far down each ball of fire of a volley is aimed over the "reach" to the
/// left; "spit", where its fire leaves it and the "first_step" it moves straight across before it takes its aim; "rumbles", the shakes of the hidden wall, each between a "return" and an
/// "extreme" height so many "repeats" times at a "speed"; "acid_script", what it does in the acid in order: "sink",
/// "pause", "rise", "wait:&lt;updates&gt;", "melt:&lt;shake&gt;:&lt;half period&gt;" with the "melt_updates" a melt
/// lasts, "sound:&lt;name&gt;" and "body:&lt;whole or a name in melt_looks&gt;"; "melt_looks", for each melting body
/// the timeline it shows at or below each height, lowest first; "bridge_fragments", where the pieces of the bridge
/// fall from, one an update; "spike_pieces", the pieces the spike wall breaks into; and "screens", the screens of its
/// room it shuts and opens: "behind_wall" and "arena", lists of them, "hidden_wall", "bridge_left" and "bridge", each
/// a column "x" and a row "y", and "bridge_left_shut_from_x", how far across Samus shuts the screen left of the bridge
/// while it fights.</para>
/// </remarks>
public partial class Crocomire : Enemy, ILatchable
{
    /// <summary>Whether this is the skeleton a room where it has been beaten shows: dead and still.</summary>
    [Export]
    public bool Corpse { get; set; }

    /// <summary>The terrain patches that crumble the first blocks of the bridge as it steps onto it, in order.</summary>
    [Export]
    public Godot.Collections.Array<string> BridgeCrumblePatches { get; set; } = [];

    /// <summary>The terrain patch that takes the bridge away.</summary>
    [Export]
    public string BridgeCollapsePatch { get; set; } = "";

    /// <summary>The terrain patch the room is left with once its skeleton has broken through the spike wall.</summary>
    [Export]
    public string SpikeWallPatch { get; set; } = "";

    /// <summary>The music of the fight, by name, which starts as Samus nears the wall its skeleton breaks through.</summary>
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
    [Export, DataValue("melt_updates")]
    public int MeltUpdates { get; set; } = 54;

    /// <summary>How many updates apart the acid is heard burning it.</summary>
    [Export, DataValue("acid_sound_updates")]
    public int AcidSoundUpdates { get; set; } = 32;

    /// <summary>How many updates apart the acid smokes about it while it sinks, rises and melts.</summary>
    [Export, DataValue("acid_smoke_updates")]
    public int AcidSmokeUpdates { get; set; } = 6;

    /// <summary>How far below the acid's surface its smoke starts before its rise, in pixels.</summary>
    [Export, DataValue("acid_smoke_depth")]
    public int AcidSmokeDepth { get; set; } = 16;

    /// <summary>How far across the room Samus must have come back, in pixels, for its skeleton to break out.</summary>
    [Export, DataValue("wall_approach_x")]
    public int WallApproachX { get; set; } = 640;

    /// <summary>How far above and below its middle it is solid to Samus once the bridge has given way under it, in pixels.</summary>
    [Export, DataValue("sinking_half_height")]
    public int SinkingHalfHeight { get; set; } = 16;

    /// <summary>How many updates it flashes as Samus's grapple takes hold of it.</summary>
    [Export, DataValue("latched_flash_updates")]
    public int LatchedFlashUpdates { get; set; } = 4;

    /// <summary>Its solid body as its data sizes it, before the bridge gives way.</summary>
    private PixelRect _solidWhole;

    /// <summary>
    /// Whether it stands in Samus's way: solid to her, and something her grapple's line takes hold of. It does from
    /// its room's setup until she has come back to the wall its skeleton waits behind; never as a corpse.
    /// </summary>
    private bool StandsInHerWay => _dying is Dying.Fighting or Dying.InAcid or Dying.FlowingDownRiver or Dying.WaitingBehindWall;

    /// <inheritdoc/>
    public bool CanBeLatched => StandsInHerWay && !IsDead;

    /// <inheritdoc/>
    public PixelPosition LatchPoint => Position;

    /// <summary>Her grapple's line taking hold of it makes it flash, as a hit that hurt it does, and no more.</summary>
    public void Latched(PixelPosition at) => FlashFor(LatchedFlashUpdates);

    /// <inheritdoc/>
    public void Unlatched()
    {
    }

    /// <summary>
    /// Puts its solid body as the fight has it: its whole size while it fights, as high as the acid leaves of it once
    /// the bridge has given way, and nothing once it no longer stands in her way.
    /// </summary>
    private void ShapeSolid()
    {
        Collider solid = Solid ?? throw new InvalidOperationException($"Crocomire \"{Name}\" has no solid collider.");
        solid.Enabled = StandsInHerWay;
        solid.Rect = _dying == Dying.Fighting
            ? _solidWhole
            : new PixelRect(_solidWhole.Left, -SinkingHalfHeight, _solidWhole.Width, SinkingHalfHeight * 2);
    }

    /// <summary>How many updates the wall shows its spikes after it has rumbled, before the skeleton breaks through.</summary>
    [Export, DataValue("spiked_wall_updates")]
    public int SpikedWallUpdates { get; set; } = 129;

    /// <summary>How many updates its skeleton waits, broken out, before it falls.</summary>
    [Export, DataValue("fall_delay")]
    public int FallDelay { get; set; } = 80;

    /// <summary>How many things it drops when its skeleton falls apart.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>How many pixels from the view's left edge its fight keeps Samus while she faces right.</summary>
    [Export, DataValue("samus_in_view_facing_right")]
    public int SamusInViewFacingRight { get; set; } = 64;

    /// <summary>How many pixels from the view's left edge its fight keeps Samus while she faces left.</summary>
    [Export, DataValue("samus_in_view_facing_left")]
    public int SamusInViewFacingLeft { get; set; } = 80;

    /// <summary>How many pixels from the view's left edge its skeleton's break-out keeps Samus, whichever way she faces.</summary>
    [Export, DataValue("samus_in_view_at_break_out")]
    public int SamusInViewAtBreakOut { get; set; } = 224;

    /// <summary>How far past the left of the view its near edge may be for a step back to move it, in pixels.</summary>
    [Export, DataValue("step_back_screen_reach")]
    public int StepBackScreenReach { get; set; } = 260;

    /// <summary>How far past the left of the view its near edge may be for a shot in the mouth to count for what it is, in pixels.</summary>
    [Export, DataValue("mouth_shot_screen_reach")]
    public int MouthShotScreenReach { get; set; } = 256;

    /// <summary>How many updates a shot in the mouth holds it as it is.</summary>
    [Export, DataValue("mouth_shot_hold")]
    public int MouthShotHold { get; set; } = 8;

    /// <summary>How many updates a shot in the mouth makes it flash.</summary>
    [Export, DataValue("mouth_shot_flash")]
    public int MouthShotFlash { get; set; } = 14;

    /// <summary>How many updates a power bomb makes it flash.</summary>
    [Export, DataValue("power_bomb_flash")]
    public int PowerBombFlash { get; set; } = 4;

    /// <summary>How many steps back a charged beam in the mouth drives it.</summary>
    [Export, DataValue("charged_beam_steps")]
    public int ChargedBeamSteps { get; set; } = 2;

    /// <summary>How many steps back a missile in the mouth drives it.</summary>
    [Export, DataValue("missile_steps")]
    public int MissileSteps { get; set; } = 1;

    /// <summary>How many steps back a super missile in the mouth drives it.</summary>
    [Export, DataValue("super_missile_steps")]
    public int SuperMissileSteps { get; set; } = 3;

    /// <summary>How many steps a power bomb sets it charging.</summary>
    [Export, DataValue("power_bomb_steps")]
    public int PowerBombSteps { get; set; } = 3;

    /// <summary>How far a claw's touch knocks Samus back, in pixels; left, when less than nothing.</summary>
    [Export, DataValue("claw_push")]
    public int ClawPush { get; set; } = -4;

    /// <summary>How far right of its place its tongue is, in pixels.</summary>
    [Export, DataValue("tongue_across")]
    public int TongueAcross { get; set; } = 23;

    /// <summary>The effect of a cloud of dust, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of the smoke the acid raises about it.</summary>
    [Export, DataValue("acid_smoke_effect")]
    public string AcidSmokeEffect { get; set; } = "";

    /// <summary>The effect of a shot bursting harmlessly on it.</summary>
    [Export, DataValue("dud_effect")]
    public string DudEffect { get; set; } = "";

    /// <summary>The effect of a super missile bursting harmlessly on it.</summary>
    [Export, DataValue("big_dud_effect")]
    public string BigDudEffect { get; set; } = "";

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

    /// <summary>The sound of the wall rumbling before its skeleton breaks out.</summary>
    [Export, DataValue("rumble_sound")]
    public string RumbleSound { get; set; } = "";

    /// <summary>The sound of a big explosion.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    /// <summary>The sound of the wall crumbling as its skeleton breaks through.</summary>
    [Export, DataValue("crumble_sound")]
    public string CrumbleSound { get; set; } = "";

    /// <summary>The sound of the spike wall breaking apart.</summary>
    [Export, DataValue("wall_break_sound")]
    public string WallBreakSound { get; set; } = "";

    /// <summary>The terrain patch that walls Samus in behind the fallen bridge until its skeleton is carried off.</summary>
    [Export, DataValue("bridge_wall_patch")]
    public string BridgeWallPatch { get; set; } = "";

    /// <summary>The terrain patch that walls Samus in before the hidden wall until its skeleton lies dead.</summary>
    [Export, DataValue("skeleton_wall_patch")]
    public string SkeletonWallPatch { get; set; } = "";

    /// <summary>The terrain patch of the spike wall as the skeleton breaks through it: the way it came by opened, the way on still shut.</summary>
    [Export, DataValue("spike_wall_breaking_patch")]
    public string SpikeWallBreakingPatch { get; set; } = "";

    /// <summary>The terrain patch of the blocks beyond the bridge that the skeleton's breaking through clears.</summary>
    [Export, DataValue("break_through_patch")]
    public string BreakThroughPatch { get; set; } = "";

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
        Settling,
        Corpse,
    }

    // What it does in the acid, a step at a time.
    private enum Acid
    {
        Sink,
        Pause,
        Rise,
        Wait,
        Melt,
        Sound,
        Body,
    }

    private sealed record AcidStep(Acid Does, int Count, int Shake, int ShakeHalfPeriod, string Name);

    private sealed record PiecePart(string Shot, string Touch);

    private sealed record SpikePiece(int Depth, Pixels Growth, Pixels Most, int Fastest, string Sound);

    private readonly Dictionary<string, Dictionary<string, PiecePart>> _partsIn = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _mouthIn = new(StringComparer.Ordinal);
    private readonly List<int> _volley = [];
    private readonly List<(int Return, int Extreme, int Repeats, int Speed)> _rumbles = [];
    private readonly List<AcidStep> _acidScript = [];
    private readonly Dictionary<string, List<(int FromY, string Timeline)>> _meltLooks = new(StringComparer.Ordinal);
    private readonly List<int> _fragmentXs = [];
    private readonly List<SpikePiece> _spikePieces = [];
    private readonly List<Vector2I> _behindWallScreens = [];
    private readonly List<Vector2I> _arenaScreens = [];
    private Vector2I _hiddenWallScreen;
    private Vector2I _bridgeLeftScreen;
    private Vector2I _bridgeScreen;
    private int _bridgeLeftShutFromX;
    private ColliderPoses? _poses;
    private int _volleyReach = 64;
    private PixelOffset _spitFrom;
    private int _spitFirstStep;

    /// <summary>The fire spat in the last update, with the speed each is aimed at from its second update on.</summary>
    private readonly List<(CrocomireFire Fire, PixelVelocity Aimed)> _justSpat = [];
    private int _fragmentY;
    private Pixels _fragmentSlowestFall;
    private PixelPosition _spikeWallTop;
    private Pixels _pieceLaunchSpeed;
    private Pixels _pieceGravity;
    private int _pieceLandingY;

    private Phase _phase;
    private Dying _dying;
    private int _steps;
    private bool _hitPending;

    /// <summary>Whether the hit being taken met a part that takes a shot as its own, so that nothing of the shot is left.</summary>
    private bool _shotTaken;
    private int _shotsSpat;
    private int _approachStages;
    private int _acidStep;
    private int _wait;
    private int _hold;
    private Pixels _downSpeed;
    private Pixels _downQuickening;
    private int _acidSoundIn = 1;
    private int _acidSmokeIn = 1;
    private int _rumble;
    private int _rumbleRepeats;
    private int _rumbleOffset;
    private bool _rumbleToExtreme;
    private bool _skeletonCollapsed;
    private string _mouth = "not_open";
    private string _acidBody = "whole";
    private int _meltBaseX;
    private int _fragmentsFallen;
    private TimelinePlayer? _tongue;

    /// <summary>How far the wall it hides behind is rattled up or down this update, in pixels; zero when it is not.</summary>
    public int WallRumble => _dying == Dying.Rumbling ? _rumbleOffset : 0;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _poses = ColliderPoses.Read(data.Get("poses"), this);
        _partsIn.Clear();
        _mouthIn.Clear();
        foreach ((string name, DataNode pose) in data.Get("poses").Entries())
        {
            var parts = new Dictionary<string, PiecePart>(StringComparer.Ordinal);
            foreach ((string part, DataNode shot) in pose.Get("shots").Entries())
                parts[part] = new PiecePart(shot.AsText(), pose.Get("touches").Text(part));
            _partsIn[name] = parts;
            _mouthIn[name] = pose.Text("mouth");
        }

        DataNode volley = data.Get("volley");
        _volleyReach = volley.Int("reach");
        _volley.Clear();
        _volley.AddRange(volley.List("gradients").Select(gradient => gradient.AsInt()));
        _spitFrom = PixelOffset.FromWhole(data.Get("spit").Int("x"), data.Get("spit").Int("y"));
        _spitFirstStep = data.Get("spit").Int("first_step");
        _rumbles.Clear();
        _rumbles.AddRange(data.List("rumbles").Select(rumble => (rumble.Int("return"), rumble.Int("extreme"), rumble.Int("repeats"), rumble.Int("speed"))));
        _acidScript.Clear();
        foreach (DataNode step in data.List("acid_script"))
        {
            string[] parts = step.AsText().Split(':');
            int Number(int index) => int.Parse(parts[index], CultureInfo.InvariantCulture);
            _acidScript.Add(parts[0] switch
            {
                "sink" => new AcidStep(Acid.Sink, 0, 0, 0, ""),
                "pause" => new AcidStep(Acid.Pause, 0, 0, 0, ""),
                "rise" => new AcidStep(Acid.Rise, 0, 0, 0, ""),
                "wait" => new AcidStep(Acid.Wait, Number(1), 0, 0, ""),
                "melt" => new AcidStep(Acid.Melt, MeltUpdates, Number(1), Number(2), ""),
                "sound" => new AcidStep(Acid.Sound, 0, 0, 0, parts[1]),
                "body" => new AcidStep(Acid.Body, 0, 0, 0, parts[1]),
                _ => throw step.Problem("is nothing Crocomire does in the acid"),
            });
        }

        if (_acidScript.Count == 0 || _acidScript[0].Does != Acid.Sink)
            throw data.Problem("must begin what Crocomire does in the acid with a sink");
        _meltLooks.Clear();
        foreach ((string body, DataNode looks) in data.Get("melt_looks").Entries())
            _meltLooks[body] = [.. looks.Items().Select(look => (look.Int("from_y"), look.Text("timeline")))];
        DataNode fragments = data.Get("bridge_fragments");
        _fragmentY = fragments.Int("y");
        _fragmentSlowestFall = fragments.Distance("slowest_fall");
        _fragmentXs.Clear();
        _fragmentXs.AddRange(fragments.List("xs").Select(x => x.AsInt()));
        DataNode pieces = data.Get("spike_pieces");
        _spikeWallTop = PixelPosition.FromWhole(pieces.Int("x"), pieces.Int("y"));
        _pieceLaunchSpeed = pieces.Distance("launch_speed");
        _pieceGravity = pieces.Distance("gravity");
        _pieceLandingY = pieces.Int("landing_y");
        _spikePieces.Clear();
        _spikePieces.AddRange(pieces.List("pieces").Select(piece =>
            new SpikePiece(piece.Int("depth"), piece.Distance("growth"), piece.Distance("most"), piece.Int("fastest"), piece.Text("sound"))));
        DataNode screens = data.Get("screens");
        static Vector2I Screen(DataNode at) => new(at.Int("x"), at.Int("y"));
        _behindWallScreens.Clear();
        _behindWallScreens.AddRange(screens.List("behind_wall").Select(Screen));
        _arenaScreens.Clear();
        _arenaScreens.AddRange(screens.List("arena").Select(Screen));
        _hiddenWallScreen = Screen(screens.Get("hidden_wall"));
        _bridgeLeftScreen = Screen(screens.Get("bridge_left"));
        _bridgeScreen = Screen(screens.Get("bridge"));
        _bridgeLeftShutFromX = screens.Int("bridge_left_shut_from_x");
    }

    /// <summary>Sets how one screen of its room lets the view in.</summary>
    private void WriteScreen(Vector2I screen, ScreenScroll state) => Room.Session.ScrollCells.Write(screen.X, screen.Y, state);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        _poses?.TakeNone();
        _solidWhole = (Solid ?? throw new InvalidOperationException($"Crocomire \"{Name}\" has no solid collider.")).Rect;
        if (Corpse)
        {
            _dying = Dying.Corpse;
            ShapeSolid();
            Play("dead");
            // Beaten, its room is open from the left to the screen before the bridge.
            foreach (Vector2I screen in _arenaScreens)
                WriteScreen(screen, ScreenScroll.OpenAlignedToTop);
            return;
        }

        // Its fight keeps Samus where a boss's does in the view, from its setup until its skeleton is carried off.
        Room.KeepPlayerInViewAt(SamusInViewFacingRight, SamusInViewFacingLeft);
        Play("idle_sway");
        _tongue = new TimelinePlayer(Timelines.Get("tongue"));
        // The screens behind its wall are shut while it lives.
        foreach (Vector2I screen in _behindWallScreens)
            WriteScreen(screen, ScreenScroll.Closed);
        ShapeSolid();
    }

    /// <summary>Gives the room back its own place for Samus in the view.</summary>
    private void ReleaseSamusInView() => Room.KeepPlayerInViewAt(Room.PlayerInView.X, Room.PlayerInView.Y);

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part) =>
        base.ReactionTo(hit, part) with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never };

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        _shotTaken = false;
        HitResult result = base.TakeHit(hit, part);

        // A shot one of its own parts takes is gone with nothing of its own left: only what the part leaves shows.
        return result == HitResult.Absorbed && _shotTaken ? HitResult.Dud : result;
    }

    /// <summary>What the piece of it a shot or Samus has met does, in the pose it is in.</summary>
    private PiecePart PartMet(Collider part)
    {
        // A piece is a hurtbox, which a shot meets, and a hitbox named for it with "Touch" after, which Samus meets.
        string pose = _poses?.Current ?? "", piece = part.Name.ToString();
        if (piece.EndsWith("Touch", StringComparison.Ordinal))
            piece = piece[..^"Touch".Length];
        return _partsIn.TryGetValue(pose, out Dictionary<string, PiecePart>? parts) && parts.TryGetValue(piece, out PiecePart? met)
            ? met
            : throw new InvalidOperationException($"Crocomire's pose \"{pose}\" does not say what its part \"{part.Name}\" does.");
    }

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (hit.Carries(new HitKind(HitKinds.PowerBomb)))
        {
            if (_dying != Dying.Fighting)
                return;
            // A power bomb sets it charging so many steps wherever it lands.
            _steps = PowerBombSteps;
            if (_phase == Phase.PowerBombCharge)
                return;
            FlashFor(PowerBombFlash);
            _phase = Phase.PowerBombCharge;
            Play("power_bomb_reaction_mouth_" + _mouth, restart: true);
            return;
        }

        PixelPosition at = hit.At ?? Position;
        string shot = PartMet(part).Shot;
        _shotTaken = hit.At is not null && shot != "plain";
        switch (shot)
        {
            case "absorbed" or "plain":
                return;
            case "dust":
                BurstShot(hit, at);
                return;
            case "mouth":
                break;
            default:
                throw new InvalidOperationException($"Crocomire's part \"{part.Name}\" meets a shot in a way it does not know.");
        }

        if (_dying != Dying.Fighting)
            return;
        // A shot in the mouth counts for what it is only while its near edge is far enough into view; out of view it
        // is a hit all the same, and drives it back no steps.
        int steps = 0;
        if (NearEdgeIsWithin(MouthShotScreenReach))
        {
            bool beam = hit.Carries(new HitKind(HitKinds.Beam));
            if (beam && !hit.Carries(new HitKind(HitKinds.Charged)))
            {
                // A beam that is not charged only holds it a moment and bursts.
                _hold = MouthShotHold;
                BurstShot(hit, at);
                return;
            }

            steps = hit.Carries(new HitKind(HitKinds.SuperMissile)) ? SuperMissileSteps
                : hit.Carries(new HitKind(HitKinds.Missile)) ? MissileSteps
                : beam ? ChargedBeamSteps
                : -1;
        }

        if (steps >= 0)
        {
            _steps += steps;
            if (!_hitPending)
                _hold += MouthShotHold;
            _hitPending = true;
        }

        FlashFor(MouthShotFlash);
    }

    /// <summary>Leaves the burst of a shot that does it no harm: the big one for a super missile.</summary>
    private void BurstShot(Hit hit, PixelPosition at)
    {
        string burst = hit.Carries(new HitKind(HitKinds.SuperMissile)) ? BigDudEffect : DudEffect;
        if (burst.Length > 0)
            LeaveHoldingPlace(burst, at);
    }

    /// <summary>
    /// Leaves dust or a burst: on the cartridge each is one of the enemies' shots, which holds one of the room's places
    /// while it plays and is not made with none free.
    /// </summary>
    private void LeaveHoldingPlace(string effect, PixelPosition at) =>
        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), at, holdsPlace: true));

    /// <summary>Whether its near edge lies less than <paramref name="reach"/> pixels right of the view's left edge.</summary>
    private bool NearEdgeIsWithin(int reach) =>
        Position.X.Whole - (Body?.Size.X ?? 0) / 2 - reach < Room.Session.Camera.Left;

    /// <summary>Whether the number last drawn lies in the half of all numbers that begins at <paramref name="from"/>.</summary>
    private bool DrawnInHalfFrom(int from)
    {
        long above = (long)Random.Last.Number - from;
        return above is >= 0 and < 32768;
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "pose":
                (_poses ?? throw new InvalidOperationException("Crocomire's data has not been read.")).Take(parts[1]);
                _mouth = _mouthIn[parts[1]];
                break;
            case "step":
                // Its foot raises dust up to 31 pixels to one side of it or the other, by the number last drawn.
                if (parts.Length > 3)
                    RaiseDust((DrawnInHalfFrom(4096) ? -1 : 1) * Random.Last.Part(0, 5));
                bool moves = parts[2] switch
                {
                    "always" => true,
                    "unless_hit" => !_hitPending,
                    "on_screen" => NearEdgeIsWithin(StepBackScreenReach),
                    _ => throw new InvalidOperationException($"Crocomire takes a step \"{parts[2]}\", which it does not know."),
                };
                if (moves)
                    Stride(int.Parse(parts[1], CultureInfo.InvariantCulture));
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
                Room.Shake(2, 5, ShakeWay.Down, ShakeReach.Tiles);
                Sound(ShakeSound);
                break;
            case "toward_spike_wall":
                if (Stride(-4))
                {
                    _phase = Phase.BackingOffSpikeWall;
                    Play("back_off_from_spike_wall", restart: true);
                }
                else
                {
                    RaiseDust((DrawnInHalfFrom(2048) ? -32 : 32) + Random.Last.Part(0, 4));
                }

                break;
            case "skeleton_collapsed":
                _skeletonCollapsed = true;
                break;
            default:
                throw new InvalidOperationException($"Crocomire's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (_dying is not Dying.Fighting)
            return;
        string touch = PartMet(part).Touch;
        if (touch == "none")
            return;
        base.Touch(part);
        // A claw knocks her back as well.
        if (touch == "claw")
            ShiftPlayer(PixelOffset.FromWhole(ClawPush, 0));
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        ShapeSolid();
        AimJustSpat();
        // A shot in the mouth holds its timeline where it is for a moment.
        if (_hold > 0)
            _hold--;
        HoldsTimeline = _hold > 0;

        if (_dying == Dying.Fighting)
        {
            WatchBridge();
            // While it fights, and on the update the bridge gives way too, the screen left of the bridge is shut once
            // Samus is far enough across and open to its top before, and the bridge's screen is open to its top.
            WriteScreen(_bridgeLeftScreen, Room.Player.Position.X.Whole >= _bridgeLeftShutFromX ? ScreenScroll.Closed : ScreenScroll.OpenAlignedToTop);
            WriteScreen(_bridgeScreen, ScreenScroll.OpenAlignedToTop);
        }
        else
        {
            StepDeath();
        }

        // It keeps Samus from getting past only while it still fights, after the bridge has been watched.
        if (_dying == Dying.Fighting)
            BlockSamus();

        if (_tongue is not null && GetNodeOrNull<ActorVisual>("Tongue") is { } tongue)
        {
            _tongue.Step();
            tongue.Position = new Vector2(TongueAcross, 0);
            // Its tongue shows while it fights, and again in the acid once it has begun to melt.
            tongue.SetShown(_dying == Dying.Fighting || (_dying == Dying.InAcid && _acidBody != "whole"));
            tongue.Show(_tongue.Shown);
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
                // It wakes with no change of music: the old game asks for none here.
                _phase = Phase.WaitingForFirstDamage;
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
                if (_shotsSpat < _volley.Count)
                {
                    Spit(_volley[_shotsSpat++]);
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
        // As the cartridge counts an angle: none right, a quarter turn up.
        int bearing = CartridgeAngles.AimAngle(-_volleyReach, gradient);
        Pixels speed = Shot("fire").Speed;
        int speedIn256ths = speed.Whole * 256 + speed.Fraction / 256;
        // The old fire first moves straight across once, and takes its aimed speed only from its next update.
        CrocomireFire fire = Fire("fire", Position + _spitFrom, new PixelVelocity(Pixels.FromWhole(_spitFirstStep), Pixels.Zero)) as CrocomireFire
            ?? throw new InvalidOperationException("Crocomire's shot \"fire\" is not a ball of its fire.");
        _justSpat.Add((fire, new PixelVelocity(
            From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, bearing + Turns.PerTurn / 4)),
            From256ths(CartridgeAngles.SpeedAlong(speedIn256ths, bearing + Turns.PerTurn / 2)))));
        Sound(SpitSound);
    }

    /// <summary>A quickening as a speed takes it: its whole 256ths of a pixel, with what is finer left out.</summary>
    private static Pixels Whole256ths(Pixels quickening) => Pixels.FromParts(quickening.Whole, quickening.Fraction / 256 * 256);

    /// <summary>A distance counted in 256ths of a pixel.</summary>
    private static Pixels From256ths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 255) * 256);

    /// <summary>Gives the fire spat in the last update the speed it is aimed at.</summary>
    private void AimJustSpat()
    {
        foreach ((CrocomireFire fire, PixelVelocity aimed) in _justSpat)
        {
            if (!fire.HasEnded)
                fire.Aim(aimed);
        }

        _justSpat.Clear();
    }

    /// <summary>Raises a puff of dust at its feet, a way across from its middle and up to 7 pixels further right by the number last drawn.</summary>
    private void RaiseDust(int across)
    {
        if (DustEffect.Length == 0)
            return;
        int below = (Body?.Size.Y ?? 0) / 2 - 16;
        LeaveHoldingPlace(DustEffect, PixelPosition.FromWhole(Position.X.Whole + across + Random.Last.Part(0, 3), Position.Y.Whole + below));
    }

    /// <summary>Keeps Samus from ever getting past it: at its near edge she is hurt and thrown back.</summary>
    private void BlockSamus()
    {
        Player samus = Room.Player;
        int halfWidth = (Body?.Size.X ?? 0) / 2;
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
                    LeaveHoldingPlace(DustEffect, PixelPosition.FromWhole(BridgeApproachX + 16 * (stage + (stage > 0 ? 1 : 0)), 176));
            }
        }

        if (x < BridgeCollapseX)
            return;
        _dying = Dying.InAcid;
        _acidStep = 0;
        _acidBody = "whole";
        _fragmentsFallen = 0;
        // The acid smokes and burns on its first update in the acid.
        (_acidSmokeIn, _acidSoundIn) = (1, 1);
        _hold = 0;
        Play("bridge_collapsed", restart: true);
        _downSpeed = Pixels.Zero;
        _downQuickening = Pixels.FromNumber(1.0 / 32);
        Sound(BridgeSound);
        if (BridgeCollapsePatch.Length > 0)
            Room.Terrain.ApplyPatch(BridgeCollapsePatch);
        // Samus is walled in on the bridge's side until its skeleton is carried off.
        if (BridgeWallPatch.Length > 0)
            Room.Terrain.ApplyPatch(BridgeWallPatch);
        for (int puff = 0; puff < 7 && DustEffect.Length > 0; puff++)
            LeaveHoldingPlace(DustEffect, PixelPosition.FromWhole(BridgeApproachX + 16 * puff, puff is 0 or 2 ? 176 : 192));
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
                WriteScreen(_hiddenWallScreen, ScreenScroll.Closed);
                WriteScreen(_bridgeLeftScreen, ScreenScroll.OpenAlignedToTop);
                if (SkeletonWallPatch.Length > 0)
                    Room.Terrain.ApplyPatch(SkeletonWallPatch);
                // Until the fight is won Samus is kept toward the view's right edge, whichever way she faces.
                Room.KeepPlayerInViewAt(SamusInViewAtBreakOut, SamusInViewAtBreakOut);
                (_rumble, _rumbleOffset, _rumbleToExtreme, _rumbleRepeats) = (0, 0, false, _rumbles.Count > 0 ? _rumbles[0].Repeats : 0);
                _hitPending = false;
                _dying = Dying.Rumbling;
                break;

            case Dying.Rumbling:
                Rumble();
                break;

            case Dying.BaringSkeleton:
                if (--_wait > 0)
                    break;
                Position = PixelPosition.FromWhole(480, 54);
                _wait = FallDelay;
                Velocity = PixelVelocity.Zero;
                if (SpikeWallBreakingPatch.Length > 0)
                    Room.Terrain.ApplyPatch(SpikeWallBreakingPatch);
                if (BreakThroughPatch.Length > 0)
                    Room.Terrain.ApplyPatch(BreakThroughPatch);
                Sound(CrumbleSound);
                Play("skeleton_falls_apart", restart: true);
                // Everything that holds one of the room's places goes first, the enemies' shots and what is dropped or
                // left alike, as the cartridge clears all its enemy projectiles: so every piece of the wall is made.
                foreach (IRoomEntity held in Room.PlaceHolders.ToList())
                    Room.Remove(held);
                // The spike wall flies apart, its lowest piece first.
                foreach (SpikePiece piece in _spikePieces.AsEnumerable().Reverse())
                {
                    Projectile fired = Fire("spike_piece", _spikeWallTop + PixelOffset.FromWhole(0, piece.Depth), PixelVelocity.Zero);
                    (fired as CrocomireSpikePiece ?? throw new InvalidOperationException("Crocomire's shot \"spike_piece\" is not a piece of its spike wall."))
                        .Begin(piece.Growth, piece.Most, piece.Fastest, _pieceLaunchSpeed, _pieceGravity, _pieceLandingY, DustEffect, piece.Sound, ExplosionSound);
                }

                Sound(WallBreakSound);
                _dying = Dying.WaitingToFall;
                break;

            case Dying.WaitingToFall:
                if (--_wait > 0)
                    break;
                Velocity = Velocity.WithX(Pixels.FromWhole(Velocity.X.Whole));
                Play("skeleton_falling", restart: true);
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
                if (SkeletonWallPatch.Length > 0)
                    Room.Terrain.RevertPatch(SkeletonWallPatch);
                for (int drop = 0; drop < DeathDrops; drop++)
                {
                    RandomDraw draw = Random.Draw();
                    Drop(Drops, PixelPosition.FromWhole(576 + draw.Part(0, 7), 96 + draw.Part(8, 6)));
                }

                // The way on opens on the next update, and its defeat is settled three updates after that.
                (_dying, _wait) = (Dying.Settling, 4);
                break;

            case Dying.Settling:
                if (_wait == 4)
                {
                    // The screens from the left to the one before the bridge open with the way on.
                    foreach (Vector2I screen in _arenaScreens)
                        WriteScreen(screen, ScreenScroll.OpenAlignedToTop);
                    if (SpikeWallPatch.Length > 0)
                        Room.Terrain.ApplyPatch(SpikeWallPatch);
                }

                if (--_wait > 0)
                    break;
                if (TensionMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(TensionMusic);
                ReleaseSamusInView();
                RaiseDust(-16);
                RaiseDust(16);
                // The fight is won; what is left of it lies where it fell.
                Room.MarkGone(Name);
                _dying = Dying.Corpse;
                break;
        }
    }

    /// <summary>Moves one update of what it does in the acid.</summary>
    private void StepInAcid()
    {
        // The pieces of the bridge fall after it, one an update, each a little faster or slower by the number last drawn.
        if (_fragmentsFallen < _fragmentXs.Count)
        {
            Fire("bridge_fragment", PixelPosition.FromWhole(_fragmentXs[_fragmentsFallen++], _fragmentY),
                new PixelVelocity(Pixels.Zero, _fragmentSlowestFall + Pixels.FromNumber(Random.Last.Part(0, 6) / 256.0)));
        }

        // A sound, or a change of the body it shows, takes no update of its own.
        while (_acidScript[_acidStep].Does is Acid.Sound or Acid.Body)
        {
            AcidStep told = _acidScript[_acidStep];
            if (told.Does == Acid.Sound)
            {
                Sound(told.Name);
            }
            else
            {
                if (_acidBody == "whole" && told.Name != "whole" && _tongue is not null)
                    _tongue = new TimelinePlayer(Timelines.Get("tongue_melting"));
                _acidBody = told.Name;
            }

            if (!NextAcidStep())
                return;
        }

        AcidStep step = _acidScript[_acidStep];
        // While it is whole the acid is heard burning it as it sinks, rests under it and rises, and not as it waits.
        if (_acidBody == "whole" && step.Does is Acid.Sink or Acid.Pause or Acid.Rise && --_acidSoundIn <= 0)
        {
            _acidSoundIn = AcidSoundUpdates;
            Sound(AcidSound);
        }

        // The acid smokes about it as it sinks, rises and melts, by where it is before it moves.
        if (step.Does is Acid.Sink or Acid.Rise or Acid.Melt)
            SmokeAcid();
        switch (step.Does)
        {
            case Acid.Sink:
                _hitPending = false;
                if (Position.Y.Whole >= SunkenY)
                {
                    NextAcidStep();
                    _downQuickening = Pixels.Zero;
                    break;
                }

                Pixels quicker = _downQuickening + Pixels.FromNumber(7.0 / 512), most = Pixels.FromNumber(3.0 / 16);
                _downQuickening = quicker < most ? quicker : most;
                // Its speed takes only the whole 256ths of what quickens it, and past its fastest keeps the fastest's
                // whole pixels with its own part of a pixel, so at its fastest it climbs through that pixel and starts over.
                Pixels speed = _downSpeed + Whole256ths(_downQuickening);
                _downSpeed = speed.Whole > 3 ? speed.WithWhole(3) : speed;
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
                    break;
                }

                Pixels slower = _downQuickening + Pixels.FromNumber(1.0 / 256), least = Pixels.FromNumber(31.0 / 256);
                _downQuickening = slower < least ? slower : least;
                Pixels rise = _downSpeed - Whole256ths(_downQuickening);
                _downSpeed = rise.IsNegative ? Pixels.FromNumber(255.0 / 256) : rise;
                Position -= new PixelOffset(Pixels.Zero, _downSpeed);
                break;

            case Acid.Wait:
                if (--_wait <= 0)
                    NextAcidStep();
                break;

            case Acid.Melt:
                // It shakes as it melts: a little to the right on every other pair of updates.
                Position = new PixelPosition(Pixels.FromWhole(_meltBaseX + (_wait / Math.Max(1, step.ShakeHalfPeriod) % 2 == 1 ? step.Shake : 0)), Position.Y);
                if (--_wait > 0)
                    break;
                Position = new PixelPosition(Pixels.FromWhole(_meltBaseX), Position.Y);
                NextAcidStep();
                break;
        }

        ShowMelting();
    }

    /// <summary>
    /// Each time its wait runs out the acid raises a cloud of smoke about it, by the number last drawn: up to 63 pixels
    /// to one side of it, and up to 31 above a line a little under the acid's surface.
    /// </summary>
    private void SmokeAcid()
    {
        if (--_acidSmokeIn > 0)
            return;
        _acidSmokeIn = AcidSmokeUpdates;
        if (AcidSmokeEffect.Length == 0)
            return;
        Liquid acid = Room.Liquid ?? throw new InvalidOperationException($"Crocomire \"{Name}\" is in the acid of a room with no liquid.");
        RandomDraw last = Random.Last;
        int across = last.Part(0, 6);
        // Smoke on the left rises a pixel further from it than its mirror on the right.
        if (last.Part(1, 1) == 0)
            across = -(across + 1);
        LeaveEffect(AcidSmokeEffect, PixelPosition.FromWhole(Position.X.Whole + across, acid.Surface.Whole + AcidSmokeDepth - last.Part(8, 5)));
    }

    /// <summary>Shows as much of its melting body as stands above the acid at the height it is at.</summary>
    private void ShowMelting()
    {
        if (_dying != Dying.InAcid || !_meltLooks.TryGetValue(_acidBody, out List<(int FromY, string Timeline)>? looks))
            return;
        foreach ((int fromY, string timeline) in looks)
        {
            if (Position.Y.Whole >= fromY)
            {
                Play(timeline);
                return;
            }
        }
    }

    /// <summary>
    /// Goes on to the next thing it does in the acid, and returns whether there is one; after the last, its skeleton
    /// is carried off down the river.
    /// </summary>
    private bool NextAcidStep()
    {
        if (++_acidStep < _acidScript.Count)
        {
            AcidStep next = _acidScript[_acidStep];
            // A rest under the acid, a wait and a melt each last their own time.
            if (next.Does == Acid.Pause)
                _wait = SubmergedPause;
            else if (next.Does is Acid.Wait or Acid.Melt)
                _wait = next.Count;
            if (next.Does == Acid.Melt)
                _meltBaseX = Position.X.Whole;
            return true;
        }

        _dying = Dying.FlowingDownRiver;
        if (TensionMusic.Length > 0)
            Room.Session.Sounds.RequestMusic(TensionMusic);
        WriteScreen(_bridgeLeftScreen, ScreenScroll.OpenAlignedToTop);
        WriteScreen(_bridgeScreen, ScreenScroll.OpenAlignedToTop);
        if (BridgeWallPatch.Length > 0)
            Room.Terrain.RevertPatch(BridgeWallPatch);
        ReleaseSamusInView();
        Play("skeleton_flowing_down_river", restart: true);
        return false;
    }

    /// <summary>Shakes the hidden wall up and down, harder and then easing, until its skeleton is ready to break out.</summary>
    private void Rumble()
    {
        if (_rumble >= _rumbles.Count)
        {
            _wait = SpikedWallUpdates;
            _dying = Dying.BaringSkeleton;
            return;
        }

        (int back, int extreme, _, int speed) = _rumbles[_rumble];
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

        if (++_rumble < _rumbles.Count)
            _rumbleRepeats = _rumbles[_rumble].Repeats;
    }

    /// <summary>
    /// How a step across is tried, as the cartridge moves an enemy across: only where it would end, and sloped ground
    /// that holds things up by their middle no part of it.
    /// </summary>
    private static readonly MoveOptions StridesAcross =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>Steps him <paramref name="pixels"/> whole pixels across, rightward when more than nothing; returns whether a wall stopped him.</summary>
    private bool Stride(int pixels) =>
        Room.Mover.MoveAcross(
            Body ?? throw new InvalidOperationException("Crocomire has no body collider."), Pixels.FromWhole(pixels), StridesAcross).Blocked;
}
