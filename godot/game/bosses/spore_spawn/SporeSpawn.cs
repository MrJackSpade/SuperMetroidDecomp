using System.Globalization;
using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Spore Spawn: a great pod hanging on a stalk from the ceiling of its chamber. It lowers itself, then swings in a
/// figure of eight, shut tight, while the ceiling rains spores; every so often it stops and opens, and only then can
/// its core be hurt. Each hurt sends it swinging the other way, faster once it is weak. Killed, it drifts back to
/// the middle, hardens into a husk Samus can climb, and the ceiling above it crumbles.
/// </summary>
/// <remarks>
/// Its timelines are "husk", "awaiting_descent", "fight_started", "open_and_stop", "close_and_move" and "death".
/// Their cues are "movement:&lt;idle|descending|moving|setup_death|dying&gt;"; "sway:&lt;radius&gt;:&lt;steps&gt;",
/// which sets how wide it swings and how many angle steps a swing turns each update; "spores:&lt;on|off&gt;";
/// "core:&lt;open|shut&gt;", which says whether its core can be hit; "widen", "reopen", "harden", "drops",
/// "hardening_dust" and "dying_explosion". Its hurtbox named "Core" is its core; any other part turns shots away.
/// The visuals "Stalk1" through "Stalk4" are its stalk, ceiling end first, and "Emitter1" through "Emitter4" the
/// places in the ceiling its spores come from, playing "emitter_closed" and "emitter_release". Its shot is "spore".
/// </remarks>
public partial class SporeSpawn : Enemy
{
    /// <summary>Whether this is the husk left after the fight: solid, still and harmless.</summary>
    [Export]
    public bool Husk { get; set; }

    /// <summary>The terrain patch that opens the ceiling above it when it dies.</summary>
    [Export]
    public string CeilingPatch { get; set; } = "";

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

    /// <summary>The effect of the dust that falls as it dies and hardens, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

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
    private readonly List<PixelOffset> _emitterPlaces = [];
    private Moving _moving;
    private PixelPosition _centre;
    private int _angle;
    private int _radius;
    private int _turn;
    private bool _sporesHeldBack;
    private bool _hurtThisOpening;
    private bool _dying;
    private bool _coreOpen;
    private TimelinePlayer? _stalk;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !Husk && !_dying && base.IsHitBy(side);

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        // Where in the ceiling its spores come from, from where it hangs; those left of it drift right, and the others left.
        _emitterPlaces.Clear();
        _emitterPlaces.AddRange(data.List("emitters").Select(place => PixelOffset.FromWhole(place.Int("x"), place.Int("y"))));
        if (_emitterPlaces.Count != Emitters)
            throw data.Problem($"must list {Emitters} emitters");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _centre = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _stalk = new TimelinePlayer(Timelines.Get("stalk"));
        AlwaysActive = true;
        if (Husk)
        {
            Play("husk");
            if (Solid is { } shell)
                shell.Enabled = true;
            ShowParts();
            return;
        }

        if (Solid is { } inert)
            inert.Enabled = false;
        Play("awaiting_descent");
        Position -= PixelOffset.FromWhole(0, StartRise);
        for (int emitter = 0; emitter < Emitters; emitter++)
            _emitterPlayers[emitter] = new TimelinePlayer(Timelines.Get("emitter_closed"));
        ShowParts();
    }

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        Reaction reaction = base.ReactionTo(hit, part);
        return part.Name == "Core" && _coreOpen ? reaction : reaction with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never };
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part)
    {
        _moving = Moving.Swinging;
        if (Health.Current < FastSwingBelow)
            _turn = _turn < 0 ? -FastSwayStep : FastSwayStep;
        if (_hurtThisOpening)
            return;
        // The first hurt of each opening shuts it and sends it swinging the other way.
        _turn = -_turn;
        _hurtThisOpening = true;
        Play("close_and_move", restart: true);
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        _dying = true;
        _sporesHeldBack = false;
        foreach (Projectile spore in Room.Projectiles.Where(projectile => projectile.Side == HitSide.Enemies).ToList())
            Room.Remove(spore);
        Play("death", restart: true);
        // The fight is won from this moment, though what is left of it stays where it is.
        Room.MarkGone(Name);
        if (CeilingPatch.Length > 0)
            Room.Terrain.ApplyPatch(CeilingPatch);
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
            case "core":
                _coreOpen = parts[1] == "open";
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
                Scatter(DustEffect, 7, 7, 64, 64);
                Sound(HardeningSound);
                break;
            case "dying_explosion":
                Scatter(ExplosionEffect, 7, 6, 64, 32);
                Sound(ExplosionSound);
                break;
        }
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
                    _centre.X.Whole + Turns.Sine(_angle + Turns.PerTurn / 4, Pixels.FromWhole(_radius), Turns.Fineness.Coarse).Whole,
                    _centre.Y.Whole + Turns.Sine(behind * 2 + Turns.PerTurn / 2, Pixels.FromWhole(_radius - VerticalSwayNarrowing), Turns.Fineness.Coarse).Whole);
                _angle = Turns.Wrap(_angle - _turn);
                break;

            case Moving.SetupDeath:
                PixelOffset home = _centre - Position;
                int bearing = Turns.AngleOf(home.X.Whole, home.Y.Whole);
                Velocity = new PixelVelocity(Turns.Cosine(bearing, Pixels.FromWhole(1)), Turns.Sine(bearing, Pixels.FromWhole(1)));
                break;

            case Moving.Dying:
                Position += new PixelOffset(Velocity.X, Velocity.Y);
                if (Math.Abs(Position.X.Whole - _centre.X.Whole) < 8 && Math.Abs(Position.Y.Whole - _centre.Y.Whole) < 8)
                    _moving = Moving.Idle;
                if (Room.Session.Clock.Updates % 16 == 0 && DustEffect.Length > 0)
                {
                    RandomDraw draw = Random.Draw();
                    LeaveEffect(DustEffect, _centre + PixelOffset.FromWhole(draw.Part(0, 6) - 32, draw.Part(8, 4) - 144));
                }

                break;
        }

        if (!Husk && !_dying)
            ReleaseSpores();
        ShowParts();
    }

    /// <summary>Leaves an effect at a random place about itself: a fresh number gives how far across and down, within the given bit widths, less the centring.</summary>
    private void Scatter(string effect, int acrossBits, int downBits, int centreAcross, int centreDown)
    {
        RandomDraw draw = Random.Draw();
        if (effect.Length > 0)
            LeaveEffect(effect, PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole) + PixelOffset.FromWhole(draw.Part(0, acrossBits) - centreAcross, draw.Part(8, downBits) - centreDown));
    }

    /// <summary>Counts each emitter's wait down, and lets a spore go from any whose wait has run out.</summary>
    private void ReleaseSpores()
    {
        if (_sporesHeldBack)
            return;
        for (int emitter = 0; emitter < Emitters; emitter++)
        {
            if (_emitterWait[emitter] == 0)
            {
                _emitterPlayers[emitter] = new TimelinePlayer(Timelines.Get("emitter_release"));
                PixelPosition from = _centre + _emitterPlaces[emitter];
                Fire("spore", from, new PixelVelocity(Pixels.FromWhole(_emitterPlaces[emitter].X.IsNegative ? 1 : -1), Pixels.Zero));
                _emitterWait[emitter] = Random.Draw().Part(0, 9);
            }

            // The wait counts down past nothing and round again.
            _emitterWait[emitter] = (_emitterWait[emitter] - 1) & 0xFFFF;
        }
    }

    /// <summary>Puts its stalk and its emitters where they are.</summary>
    private void ShowParts()
    {
        _stalk?.Step();
        (int X, int Y) anchor = (_centre.X.Whole, _centre.Y.Whole - StalkAnchorAbove);
        int across = Position.X.Whole - anchor.X, down = Position.Y.Whole - StalkBodyGap - anchor.Y;
        for (int link = 0; link < 4; link++)
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
            visual.Show(_stalk.Current);
        }

        for (int emitter = 0; emitter < Emitters; emitter++)
        {
            if (_emitterPlayers[emitter] is not { } player || GetNodeOrNull<ActorVisual>($"Emitter{emitter + 1}") is not { } visual)
                continue;
            player.Step();
            PixelPosition at = _centre + _emitterPlaces[emitter];
            visual.Position = new Vector2(at.X.Whole - Position.X.Whole, at.Y.Whole - Position.Y.Whole);
            visual.Show(player.Current);
        }
    }
}
