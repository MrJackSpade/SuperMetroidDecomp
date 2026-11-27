using System.Globalization;
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

namespace Game.Enemies;

/// <summary>
/// Spore Spawn: a great pod hanging on a stalk from the ceiling of its chamber. It lowers itself, then swings in a
/// figure of eight, shut tight, while the ceiling rains spores; every so often it stops and opens, and only then can
/// its core be hurt. Each hurt sends it swinging the other way, faster once it is weak, and shows how hurt it is in
/// its colours. Killed, it drifts back to the middle, hardens into a husk Samus can climb, and the ceiling above it
/// crumbles.
/// </summary>
/// <remarks>
/// Its timelines are "husk", "awaiting_descent", "fight_started", "open_and_stop", "close_and_move" and "death".
/// Their cues are "movement:&lt;idle|descending|moving|setup_death|dying&gt;"; "sway:&lt;radius&gt;:&lt;steps&gt;",
/// which sets how wide it swings and how many angle steps a swing turns each update, backwards when below nothing;
/// "spores:&lt;on|off&gt;"; "pose:&lt;name&gt;", which puts its parts where its data's "poses" have them under that
/// name (<see cref="ColliderPoses"/>); "shade:&lt;name&gt;", which shows it, its stalk and the room's own pictures in
/// the colours "hardening_colours" gives that step of its hardening; "widen", "reopen", "harden", "drops",
/// "hardening_dust" and "dying_explosion". "ceiling_crumble" is the change to the terrain that brings the ceiling above
/// it down as it dies (<see cref="PatchSequence"/>).
/// <code>
/// "poses": { "spore_spawn_ajar": { "core_takes_shots": true,
///            "colliders": { "UpperShell": { "left": -45, "top": -38, "width": 89, "height": 29 }, "Core": { ... } } } },
/// "health_shades": [ { "health": 770, "body": { ... }, "stalk": { ... } }, { "health": 0, "body": { ... }, "stalk": { ... } } ],
/// "hardening_colours": [ { "shade": "hardening_0", "body": { ... }, "stalk": { ... }, "room": { ... } } ],
/// "emitters": [ { "x": -96, "y": -104, "drift": "right" } ]
/// </code>
/// Its hurtboxes are "UpperShell", "LowerShell", "Core" and "Crown", and a shot that reaches several meets the first of
/// them in that order. Each has a hitbox named for it with "Touch" after, which Samus's touch meets, a pixel down
/// and right of the hurtbox as the cartridge meets the two. Only "Core" can be hurt, and only in a pose that says so; every other part turns shots away. A
/// pose that does not name a part leaves it switched off. "health_shades" lists, most health first, the changes of the
/// colours of its body ("body") and its stalk ("stalk") it shows from the first hurt of an opening on, by the least
/// health each is shown at (<see cref="ColourChange"/>); a step of its hardening ("hardening_colours") changes them in
/// place of that, and the room's own pictures ("room") with them. A room where it has been beaten is shown in the
/// colours of the last step of its hardening from the moment it is set up. The visuals "Stalk1" through "Stalk4" are
/// its stalk, ceiling end first, shown by the timeline "stalk" in its colours, and "Emitter1" through "Emitter4" the
/// places in the ceiling its spores come from, playing "emitter_closed" and "emitter_release", whose cue "spore" lets
/// the spore go; an emitter keeps the colours the fight began in. "emitters" gives each one's place from where Spore
/// Spawn hangs and the way its spores first drift. Its stalk and emitters go on while it flashes hurt. Its shot is
/// "spore".
/// </remarks>
public partial class SporeSpawn : Enemy
{
    /// <summary>Whether this is the husk left after the fight: solid, still and harmless.</summary>
    [Export]
    public bool Husk { get; set; }

    /// <summary>The floor the view's top is kept at or below until it dies, so that the view does not rise above its chamber; empty for none.</summary>
    [Export]
    public NodePath ArenaView { get; set; } = new();

    /// <summary>How far above where it hangs for the fight it waits to begin, in pixels.</summary>
    [Export, DataValue("start_rise")]
    public int StartRise { get; set; } = 128;

    /// <summary>How far it lowers itself each update, in pixels.</summary>
    [Export, DataValue("descent_speed")]
    public int DescentSpeed { get; set; } = 1;

    /// <summary>How far above where it hangs the ceiling end of its stalk is, in pixels.</summary>
    [Export, DataValue("ceiling_above")]
    public int CeilingAbove { get; set; } = 64;

    /// <summary>How far above where it hangs its stalk's bend starts from, in pixels.</summary>
    [Export, DataValue("stalk_anchor_above")]
    public int StalkAnchorAbove { get; set; } = 72;

    /// <summary>How far above its middle its stalk joins it, in pixels.</summary>
    [Export, DataValue("stalk_body_gap")]
    public int StalkBodyGap { get; set; } = 40;

    /// <summary>How much narrower its swing is up and down than across, in pixels.</summary>
    [Export, DataValue("vertical_sway_narrowing")]
    public int VerticalSwayNarrowing { get; set; } = 16;

    /// <summary>The widest its swing grows to, in pixels.</summary>
    [Export, DataValue("widened_sway_limit")]
    public int WidenedSwayLimit { get; set; } = 40;

    /// <summary>How much wider each closing makes its swing, in pixels.</summary>
    [Export, DataValue("sway_widening")]
    public int SwayWidening { get; set; } = 8;

    /// <summary>The health below which it swings fast.</summary>
    [Export, DataValue("fast_swing_below")]
    public int FastSwingBelow { get; set; } = 400;

    /// <summary>How many angle steps a fast swing turns each update.</summary>
    [Export, DataValue("fast_sway_step")]
    public int FastSwayStep { get; set; } = 2;

    /// <summary>How many things it drops when it dies, scattered below it.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>The effect of the dust that bursts about it as it hardens, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of the dust that falls from the ceiling while it drifts back to the middle.</summary>
    [Export, DataValue("ceiling_dust_effect")]
    public string CeilingDustEffect { get; set; } = "";

    /// <summary>The effect of the explosions on it as it dies.</summary>
    [Export, DataValue("explosion_effect")]
    public string ExplosionEffect { get; set; } = "";

    /// <summary>The sound of dust falling as it hardens, by name.</summary>
    [Export, DataValue("hardening_sound")]
    public string HardeningSound { get; set; } = "";

    /// <summary>The sound of an explosion on it as it dies, by name.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    private const int Emitters = 4;
    private const int StalkLinks = 4;

    /// <summary>Its hurtboxes, in the order a shot meets them.</summary>
    private static readonly string[] Parts = ["UpperShell", "LowerShell", "Core", "Crown"];

    private enum Moving
    {
        Idle,
        Descending,
        Swinging,
        SetupDeath,
        Dying,
    }

    private readonly int[] _emitterWait = new int[Emitters];
    private readonly TimelinePlayer?[] _emitterPlayers = new TimelinePlayer?[Emitters];
    private readonly List<(PixelOffset Place, bool DriftsLeft)> _emitters = [];
    private readonly Dictionary<string, bool> _coreTakesShotsIn = new(StringComparer.Ordinal);
    private ColliderPoses? _poses;
    private PatchSequence? _ceilingCrumble;

    /// <summary>The colours of its body and its stalk as hurt as it may be, most health first, by the least health each is shown at.</summary>
    private readonly List<(int Health, ColourChange Body, ColourChange Stalk)> _healthShades = [];

    /// <summary>Each step of its hardening, in turn: the step's name, and its body, its stalk and the room's own pictures in its colours.</summary>
    private readonly List<(string Shade, ColourChange Body, ColourChange Stalk, ColourChange Room)> _hardening = [];

    private Moving _moving;
    private PixelPosition _centre;
    private int _angle;
    private int _radius;
    private int _turn;
    private bool _sporesHeldBack;
    private bool _hurtThisOpening;
    private bool _dying;
    private bool _hardened;
    private bool _coreTakesShots;
    private TimelinePlayer? _stalk;

    /// <summary>
    /// What holds each link of its stalk's place among the room's places for what enemies launch, for good: on the
    /// cartridge each is a shot of its own. Null for a link the room had no place for, which is not there.
    /// </summary>
    private readonly Effect?[] _stalkHolds = new Effect?[StalkLinks];

    /// <summary>
    /// What holds each emitter's place among the room's places for what enemies launch, until it dies: on the cartridge
    /// each is a shot of its own. Null for an emitter the room had no place for, which is not there and lets out nothing.
    /// </summary>
    private readonly Effect?[] _emitterHolds = new Effect?[Emitters];

    /// <inheritdoc/>
    /// <remarks>Shots pass it from its death until it hardens; the husk turns them away.</remarks>
    public override bool IsHitBy(HitSide side) => (Husk || _hardened || !_dying) && base.IsHitBy(side);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _emitters.Clear();
        _emitters.AddRange(data.List("emitters").Select(emitter =>
            (PixelOffset.FromWhole(emitter.Int("x"), emitter.Int("y")), emitter.Text("drift") switch
            {
                "left" => true,
                "right" => false,
                _ => throw emitter.Problem("drifts neither \"left\" nor \"right\""),
            })));
        if (_emitters.Count != Emitters)
            throw data.Problem($"must list {Emitters} emitters");

        _poses = ColliderPoses.Read(data.Get("poses"), this);
        _coreTakesShotsIn.Clear();
        foreach ((string name, DataNode pose) in data.Get("poses").Entries())
            _coreTakesShotsIn[name] = pose.Flag("core_takes_shots");
        _ceilingCrumble = PatchSequence.Read(data.Get("ceiling_crumble"));

        _healthShades.Clear();
        _healthShades.AddRange(data.List("health_shades").Select(shade =>
            (shade.Int("health"), ColourChange.Read(shade.Get("body")), ColourChange.Read(shade.Get("stalk")))));
        _hardening.Clear();
        _hardening.AddRange(data.List("hardening_colours").Select(step => (step.Text("shade"),
            ColourChange.Read(step.Get("body")), ColourChange.Read(step.Get("stalk")), ColourChange.Read(step.Get("room")))));
        if (_hardening.Count == 0)
            throw data.Problem("must list the steps of its hardening");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _centre = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _stalk = new TimelinePlayer(Timelines.Get("stalk"));
        AlwaysActive = true;
        // Its parts are nowhere until its first pose puts them.
        foreach (string part in Parts)
        {
            GetNode<Collider>(part).Enabled = false;
            GetNode<Collider>(part + "Touch").Enabled = false;
        }

        // Its stalk's links take their places as it is set up, the husk's as well, the lowest first and the one at the
        // ceiling last.
        for (int link = StalkLinks - 1; link >= 0; link--)
            _stalkHolds[link] = HoldPlace();

        // Its stalk and the places its spores come from are things it launched and keeps by it, which the cartridge
        // draws behind Samus; the pod itself stands in front of her.
        foreach (string launched in new[] { "Stalk1", "Stalk2", "Stalk3", "Stalk4", "Emitter1", "Emitter2", "Emitter3", "Emitter4" })
        {
            if (GetNodeOrNull<ActorVisual>(launched) is { } visual)
                RoomDepth.Stand(visual, Standing.Hindmost);
        }

        if (Husk)
        {
            Play("husk");
            if (Solid is { } shell)
                shell.Enabled = true;
            ShowParts();
            // A room where it has been beaten, and the husk in it, are in the colours of its last step of hardening
            // from the moment it is set up, so through the door's passage too.
            ShowHardening(_hardening[^1].Shade);
            return;
        }

        if (Solid is { } inert)
            inert.Enabled = false;
        Play("awaiting_descent");
        Position -= PixelOffset.FromWhole(0, StartRise);
        // Then its emitters take theirs, in order.
        for (int emitter = 0; emitter < Emitters; emitter++)
        {
            _emitterHolds[emitter] = HoldPlace();
            if (_emitterHolds[emitter] is not null)
                _emitterPlayers[emitter] = new TimelinePlayer(Timelines.Get("emitter_closed"));
        }

        ShowParts();
    }

    /// <summary>Takes one of the room's places for what enemies launch, for a part of it; null when none is free.</summary>
    private Effect? HoldPlace()
    {
        Effect place = Effect.HoldingPlace(Position);
        return Room.Spawn(place) ? place : null;
    }

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        Reaction reaction = base.ReactionTo(hit, part);
        return part.Name == "Core" && _coreTakesShots ? reaction : reaction with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never };
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part) => ReactWhileFlashing();

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        // A bomb on the open core, which does it no harm, still shuts it while an earlier hit's flash shows.
        if (part.Name == "Core" && _coreTakesShots && IsFlashing && hit.Carries(new HitKind(HitKinds.Bomb)))
            ReactWhileFlashing();
    }

    /// <summary>What a hit on its core does while it flashes hurt: it swings, faster once weak, and the first of an opening shuts it.</summary>
    private void ReactWhileFlashing()
    {
        _moving = Moving.Swinging;
        if (Health.Current < FastSwingBelow)
            _turn = _turn < 0 ? -FastSwayStep : FastSwayStep;
        if (_hurtThisOpening)
            return;
        // The first hurt of each opening shuts it, sends it swinging the other way and shows how hurt it now is.
        _turn = -_turn;
        _hurtThisOpening = true;
        Play("close_and_move", restart: true);
        foreach ((int health, ColourChange body, ColourChange stalk) in _healthShades)
        {
            if (Health.Current >= health)
            {
                ShowColours(body, stalk);
                break;
            }
        }
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        _dying = true;
        _sporesHeldBack = false;
        // Its death begins at once, not when the flash of the hit that killed it is over, and ends any time the hit left
        // it unhittable for: that time would stand while it cannot be reached, and meet its hardened husk.
        EndHurt();
        BeUnhittableFor(0);
        // Everything the enemies have launched or left goes with its death, whatever it is and whoever made it:
        // spores, dust, blasts, drops. Only its stalk stays, for its death to show, and its emitters, which go next.
        foreach (IRoomEntity held in Room.PlaceHolders.Where(held => !_stalkHolds.Contains(held as Effect) && !_emitterHolds.Contains(held as Effect)).ToList())
            Room.Remove(held);
        // The places its spores came from go with them, and give back the room's places they held.
        for (int emitter = 0; emitter < Emitters; emitter++)
        {
            _emitterPlayers[emitter] = null;
            GetNode<ActorVisual>($"Emitter{emitter + 1}").Show("");
            if (_emitterHolds[emitter] is { } held)
                Room.Remove(held);
            _emitterHolds[emitter] = null;
        }

        Play("death", restart: true);
        // The fight is won from this moment, though what is left of it stays where it is.
        Room.MarkGone(Name);
        if (!ArenaView.IsEmpty)
            GetNode<ViewFloor>(ArenaView).Triggered("release");
        if (_ceilingCrumble is { } crumble)
            Room.Play(crumble.FromStart());
        return false;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!Husk && !_dying)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "movement":
                _moving = parts[1] switch
                {
                    "idle" => Moving.Idle,
                    "descending" => Moving.Descending,
                    "moving" => Moving.Swinging,
                    "setup_death" => Moving.SetupDeath,
                    "dying" => Moving.Dying,
                    _ => throw new InvalidOperationException($"Spore Spawn has no movement named \"{parts[1]}\"."),
                };
                break;
            case "sway":
                _radius = int.Parse(parts[1], CultureInfo.InvariantCulture);
                _turn = int.Parse(parts[2], CultureInfo.InvariantCulture);
                break;
            case "spores":
                _sporesHeldBack = parts[1] == "off";
                break;
            case "pose":
                TakePose(parts[1]);
                break;
            case "shade":
                ShowHardening(parts[1]);
                break;
            case "widen":
                if (_radius < WidenedSwayLimit)
                    _radius += SwayWidening;
                break;
            case "reopen":
                _hurtThisOpening = false;
                break;
            case "harden":
                Position = _centre;
                _hardened = true;
                if (Solid is { } shell)
                    shell.Enabled = true;
                break;
            case "drops":
                for (int drop = 0; drop < DeathDrops; drop++)
                {
                    RandomDraw draw = Random.Draw();
                    Drop(Drops, _centre + PixelOffset.FromWhole(draw.Part(0, 7) - 64, draw.Part(8, 6) - 96));
                }

                break;
            case "hardening_dust":
                // Its dust is one of the enemies' shots on the cartridge, which holds one of the room's places; its
                // explosions are not.
                Scatter(DustEffect, 7, 7, 64, 64, holdsPlace: true);
                Sound(HardeningSound);
                break;
            case "dying_explosion":
                Scatter(ExplosionEffect, 7, 6, 64, 32);
                Sound(ExplosionSound);
                break;
            default:
                throw new InvalidOperationException($"Spore Spawn's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        }
    }

    /// <summary>
    /// Shows it, its stalk and the room's own pictures, its level and its background, in the colours of a step of its
    /// hardening, at once.
    /// </summary>
    private void ShowHardening(string shade)
    {
        foreach ((string name, ColourChange body, ColourChange stalk, ColourChange room) in _hardening)
        {
            if (name == shade)
            {
                ShowColours(body, stalk);
                Room.Recolour(nameof(SporeSpawn), room);
                return;
            }
        }

        throw new InvalidOperationException($"Spore Spawn has no step of its hardening named \"{shade}\".");
    }

    /// <summary>Shows its body and its stalk in other colours at once, in place of those it showed them in before.</summary>
    private void ShowColours(ColourChange body, ColourChange stalk)
    {
        if (Visual is { } shown)
            body.ShowOn(shown, nameof(SporeSpawn));
        for (int link = 0; link < StalkLinks; link++)
        {
            if (GetNodeOrNull<ActorVisual>($"Stalk{link + 1}") is { } visual)
                stalk.ShowOn(visual, nameof(SporeSpawn));
        }
    }

    /// <summary>Puts its parts where a pose has them, and switches off those the pose does not name.</summary>
    private void TakePose(string name)
    {
        (_poses ?? throw new InvalidOperationException("Spore Spawn's data has not been read.")).Take(name);
        _coreTakesShots = _coreTakesShotsIn[name];
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_moving)
        {
            case Moving.Descending:
                Position += PixelOffset.FromWhole(0, DescentSpeed);
                if (Position.Y.Whole >= _centre.Y.Whole)
                    Play("fight_started", restart: true);
                _angle = 3 * Turns.PerTurn / 4;
                break;

            case Moving.Swinging:
                // Across it swings once for every twice it swings up and down: a figure of eight.
                int behind = Turns.Wrap(_angle - Turns.PerTurn / 4);
                Position = PixelPosition.FromWhole(
                    _centre.X.Whole + CartridgeAngles.CircleCosine(_angle, _radius).Whole,
                    _centre.Y.Whole + CartridgeAngles.CircleSine(behind * 2 + Turns.PerTurn / 2, _radius - VerticalSwayNarrowing).Whole);
                _angle = Turns.Wrap(_angle + _turn);
                break;

            case Moving.SetupDeath:
                int bearing = CartridgeAngles.AimAngle(_centre.X.Whole - Position.X.Whole, _centre.Y.Whole - Position.Y.Whole);
                Velocity = CartridgeAngles.VelocityAlong(bearing, 1);
                break;

            case Moving.Dying:
                Position += new PixelOffset(Velocity.X, Velocity.Y);
                if (Math.Abs(Position.X.Whole - _centre.X.Whole) < 8 && Math.Abs(Position.Y.Whole - _centre.Y.Whole) < 8)
                    _moving = Moving.Idle;
                if (Room.Session.Clock.Updates % 16 == 0 && CeilingDustEffect.Length > 0)
                {
                    RandomDraw draw = Random.Draw();
                    LeaveEffect(CeilingDustEffect, _centre + PixelOffset.FromWhole(draw.Part(0, 6) - 32, draw.Part(8, 4) - 144));
                }

                break;
        }

        if (!Husk && !_dying)
            ReleaseSpores();
        ShowParts();
    }

    /// <summary>
    /// Its emitters and stalk are things of their own on the cartridge, which a hurt does not hold: while it flashes,
    /// still where it is, the emitters count down to their releases, drawing their numbers, and they and the stalk play
    /// on.
    /// </summary>
    protected override void WhileHurt()
    {
        if (!Husk && !_dying)
            ReleaseSpores();
        ShowParts();
    }

    /// <summary>
    /// Leaves an effect at a random place about itself: a fresh number gives how far across and down, within the given
    /// bit widths, less the centring. One that holds a place is not made when the room has none free.
    /// </summary>
    private void Scatter(string effect, int acrossBits, int downBits, int centreAcross, int centreDown, bool holdsPlace = false)
    {
        RandomDraw draw = Random.Draw();
        if (effect.Length == 0)
            return;
        PixelPosition at = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole) + PixelOffset.FromWhole(draw.Part(0, acrossBits) - centreAcross, draw.Part(8, downBits) - centreDown);
        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(effect)), at, holdsPlace));
    }

    /// <summary>Counts each emitter's wait down, and opens any whose wait has run out.</summary>
    private void ReleaseSpores()
    {
        if (_sporesHeldBack)
            return;
        for (int emitter = 0; emitter < Emitters; emitter++)
        {
            // An emitter that is not there draws no number.
            if (_emitterHolds[emitter] is null)
                continue;
            if (_emitterWait[emitter] == 0)
            {
                _emitterPlayers[emitter] = new TimelinePlayer(Timelines.Get("emitter_release"));
                _emitterWait[emitter] = Random.Draw().Part(0, 9);
            }

            // The wait counts down past nothing and round again.
            _emitterWait[emitter] = (_emitterWait[emitter] - 1) & 0xFFFF;
        }
    }

    /// <summary>
    /// Puts its stalk and its emitters where they are, and lets go the spore of any emitter that has opened. The links
    /// of its stalk run from the ceiling to where it is now: the first at the ceiling, the others a quarter, a half and
    /// three quarters of the way from the stalk's bend.
    /// </summary>
    private void ShowParts()
    {
        _stalk?.Step();
        (int X, int Y) anchor = (_centre.X.Whole, _centre.Y.Whole - StalkAnchorAbove);
        int across = Position.X.Whole - anchor.X, down = Position.Y.Whole - StalkBodyGap - anchor.Y;
        for (int link = 0; link < StalkLinks; link++)
        {
            if (_stalk is null || GetNodeOrNull<ActorVisual>($"Stalk{link + 1}") is not { } visual)
                continue;
            (int x, int y) = link switch
            {
                0 => (_centre.X.Whole, _centre.Y.Whole - CeilingAbove),
                1 => (anchor.X + across / 4, anchor.Y + down / 4),
                2 => (anchor.X + across / 2, anchor.Y + down / 2),
                _ => (anchor.X + across / 4 + across / 2, anchor.Y + down / 4 + down / 2),
            };
            visual.Position = new Vector2(x - Position.X.Whole, y - Position.Y.Whole);
            visual.Show(_stalk.Shown);
            visual.SetShown(_stalkHolds[link] is not null);
        }

        for (int emitter = 0; emitter < Emitters; emitter++)
        {
            if (_emitterPlayers[emitter] is not { } player || GetNodeOrNull<ActorVisual>($"Emitter{emitter + 1}") is not { } visual)
                continue;
            (PixelOffset place, bool driftsLeft) = _emitters[emitter];
            PixelPosition at = _centre + place;
            foreach (string cue in player.Step())
            {
                if (cue != "spore")
                    throw new InvalidOperationException($"An emitter of Spore Spawn's gives the cue \"{cue}\", which it does not know.");
                // The speed it is launched with only says which way it drifts.
                if (!Husk && !_dying)
                    Fire("spore", at, new PixelVelocity(Pixels.FromWhole(driftsLeft ? -1 : 1), Pixels.Zero));
            }

            visual.Position = new Vector2(at.X.Whole - Position.X.Whole, at.Y.Whole - Position.Y.Whole);
            // An emitter keeps the colours the fight began in.
            visual.Show(player.Shown);
        }
    }
}
