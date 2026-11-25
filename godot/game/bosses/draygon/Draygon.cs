using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Draygon: the great crustacean of Maridia. After its brood has danced it swoops down across its chamber from
/// either side, or swims slowly through spitting goop that sticks to Samus and slows her. A slowed Samus it seizes,
/// carries to the middle, and spirals upward with, lashing her with its tail, until she struggles free. Its wall
/// cannons fire all the while. Each hit makes its swoops steeper. If Samus grapples a broken cannon while it holds
/// her, the current runs through them both and it takes far more than she does. Dead, it drifts to the floor, its
/// brood gathers over it, and it sinks away.
/// </summary>
/// <remarks>
/// Its body plays "body_left", "body_right", "fire_goop_left", "fire_goop_right", "roar_left", "roar_right",
/// "dying_left", "dying_right" and "rest"; a step with no picture leaves it as it was shown. The cue "goop" of a
/// fire-goop timeline lets one go; of a dying timeline "dying_effect:" with an effect's data file leaves that effect
/// somewhere about it, "intangible" ends its touch and "paralyse" stops its tail and arms where they are. The visual
/// "Tail" plays "tail_left", "tail_right", "tail_whip_left", "tail_whip_right", "tail_final_whips_left" and
/// "tail_final_whips_right", whose cue "whip_hit" lands a blow, "release" lets Samus go, "shift:" with how far across
/// and down moves the pictures of its body and eye that far for the lash, and "sound:" with a sound's name asks for
/// it; "Arms" plays "arms_left", "arms_right", "arms_reach_left", "arms_reach_right", "arms_grab_left" and
/// "arms_grab_right"; "Eye" plays "eye_left_looking_up" and the like for each side and each of the four ways, and
/// "eye_dying_left", "eye_dying_right", "eye_dead_left" and "eye_dead_right". The names of the pictures of its body
/// and eye may hold "{health}", which stands for "_health_" and which of its "health_bands" its health is in, from
/// nothing up: the least health of each, highest first. "part_colours" gives the colours its "Tail" and "Arms" show
/// changed, each as its pictures have it and as it is shown: "opening" from the start, and "fought" from its first
/// hurt on. Its hurtbox "Belly" is where it can be hurt; the rest of its
/// colliders are its shell, which stops shots and hurts to touch. "facing_colliders" gives every collider its place
/// when it faces "left" and when it faces "right". Its shot is "goop". Its data lists "intro_dance", the steps each
/// of its brood takes one after another, "intro_dancer_delays", how many updates into the dance each sets off, and
/// "burial_brood", where each of those that bury it starts ("x", "y"), the way it drifts ("heading", in turns of 256
/// from right through down) and the "effect" that shows it. "cannon_turns" lists, for each number its
/// "cannon_choice" draws ("from" which binary digit of a number drawn, and how many "digits"), the "cannon" that
/// fires, or none, and where its shot appears ("x", "y"). "scattered_drops" is the table of what it scatters when it
/// is gone, and "drop_scatter" where: a corner of the room ("x", "y") and, across and down, how many binary digits of
/// a number drawn are added and from which digit up. "dying_scatter" places the effects of its dying the same way,
/// from its own place. All its places are in the room's own pixels.
/// </remarks>
public partial class Draygon : Enemy
{
    /// <summary>The music that plays once it lies still, by name.</summary>
    [Export]
    public string VictoryMusic { get; set; } = "";

    /// <summary>How far down the room the line it swims along is, in pixels.</summary>
    [Export, DataValue("swim_line_y")]
    public int SwimLineY { get; set; } = 384;

    /// <summary>How far across the room it turns back on the right, in pixels; a goop pass leaves the room there.</summary>
    [Export, DataValue("right_side_distance")]
    public int RightSideDistance { get; set; } = 672;

    /// <summary>How many updates pass before its brood begins to dance.</summary>
    [Export, DataValue("intro_updates")]
    public int IntroUpdates { get; set; } = 256;

    /// <summary>How many updates its brood dances for before it first comes at Samus.</summary>
    [Export, DataValue("intro_dance_updates")]
    public int IntroDanceUpdates { get; set; } = 1232;

    /// <summary>The speed its swoop gains each update to start with, as a number of pixels.</summary>
    [Export, DataValue("swoop_quickening")]
    public double SwoopQuickening { get; set; } = 0.09375;

    /// <summary>How much more its swoop gains each update for every hit it takes.</summary>
    [Export, DataValue("swoop_quickening_per_hit")]
    public double SwoopQuickeningPerHit { get; set; } = 0.03125;

    /// <summary>What its swoop's gain each update must stay under.</summary>
    [Export, DataValue("swoop_quickening_limit")]
    public double SwoopQuickeningLimit { get; set; } = 0.625;

    /// <summary>How near across Samus must be, in pixels, for it to start spitting goop.</summary>
    [Export, DataValue("goop_range")]
    public int GoopRange { get; set; } = 208;

    /// <summary>How many goops it spits in one pass at most.</summary>
    [Export, DataValue("goops_per_pass")]
    public int GoopsPerPass { get; set; } = 16;

    /// <summary>How fast it chases a slowed Samus, and carries her, in pixels an update.</summary>
    [Export, DataValue("chase_speed")]
    public int ChaseSpeed { get; set; } = 2;

    /// <summary>How many times Samus must press a new way on the pad to struggle free.</summary>
    [Export, DataValue("escape_presses")]
    public int EscapePresses { get; set; } = 60;

    /// <summary>The harm the current does it each time it runs through, while Samus holds a broken cannon.</summary>
    [Export, DataValue("grapple_shock_damage")]
    public int GrappleShockDamage { get; set; } = 256;

    /// <summary>How hard the room shakes when its tail lands.</summary>
    [Export, DataValue("whip_shake_strength")]
    public int WhipShakeStrength { get; set; } = 3;

    /// <summary>How many things it drops when it has sunk away.</summary>
    [Export, DataValue("death_drops")]
    public int DeathDrops { get; set; } = 16;

    /// <summary>The effect that shows one of its brood dancing, by the data file of its timeline.</summary>
    [Export, DataValue("dancer_effect")]
    public string DancerEffect { get; set; } = "";

    /// <summary>The effect of the foam at its mouth.</summary>
    [Export, DataValue("foam_effect")]
    public string FoamEffect { get; set; } = "";

    /// <summary>The effect of its breath bubbling up.</summary>
    [Export, DataValue("bubble_effect")]
    public string BubbleEffect { get; set; } = "";

    /// <summary>The effect of the bubbles that rise from by its eye.</summary>
    [Export, DataValue("eye_bubble_effect")]
    public string EyeBubbleEffect { get; set; } = "";

    /// <summary>The effect of dust where its tail strikes.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of the smoke over where it lies.</summary>
    [Export, DataValue("smoke_effect")]
    public string SmokeEffect { get; set; } = "";

    private enum Phase
    {
        IntroDelay,
        IntroDance,
        SwoopSetupRight,
        SwoopSetupLeft,
        SwoopDown,
        SwoopApex,
        SwoopUp,
        GoopSetup,
        GoopApproach,
        GoopSpit,
        GoopRecovery,
        TryGrab,
        RepelGrapple,
        Carry,
        Flail,
        BeginTailWhip,
        TailWhip,
        FinalWhips,
        FinalWhipsWait,
        Release,
        FlyUp,
        Dying,
        DyingWait,
        DyingSink,
    }

    /// <summary>One of its brood that comes to bury it.</summary>
    private sealed record Burier(int X, int Y, int Heading, string Effect);

    private readonly List<PixelOffset> _dance = [];
    private readonly List<int> _dancerDelays = [];
    private readonly List<Burier> _buriers = [];
    private readonly List<(Effect Sprite, int Delay, int Step)> _dancers = [];
    private readonly List<(Effect Sprite, PixelOffset Drift)> _burying = [];
    private readonly List<int> _swoopHeights = [];
    private readonly Dictionary<string, TimelinePlayer> _parts = new(StringComparer.Ordinal);
    private readonly Dictionary<Facing, Dictionary<string, PixelRect>> _boxes = [];
    private readonly List<int> _healthBands = [];
    private readonly Dictionary<string, ColourSwap[]> _openingColours = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ColourSwap[]> _foughtColours = new(StringComparer.Ordinal);
    private bool _fought;
    private readonly List<(string Cannon, PixelPosition From)> _cannonTurns = [];
    private (int From, int Digits) _cannonChoice;
    private DropTable _scatteredDrops = DropTable.Nothing;
    private (Vector2I Corner, (int From, int Digits) Across, (int From, int Digits) Down) _dropScatter, _dyingScatter;
    private string _shown = "";
    private Vector2I _bodyShift;
    private bool _intangible;
    private Phase _phase;
    private Phase _afterGoop;
    private Facing _pass = Facing.Right;
    private PixelPosition _leftStart;
    private int _wait;
    private int _danced;
    private int _swoopLeft;
    private int _swoopStartY;
    private Pixels _acrossSpeed;
    private Pixels _quickening;
    private int _goopsLeft;
    private int _goopAngle;
    private bool _holds;
    private bool _passesThroughSamus;
    private Pixels _spiralRadius;
    private int _spiralAngle;
    private int _spiralTurn;
    private Pixels _spiralY;
    private bool _dying;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _dance.Clear();
        _dance.AddRange(data.List("intro_dance").Select(step => PixelOffset.FromWhole(step.Int("x"), step.Int("y"))));
        _dancerDelays.Clear();
        _dancerDelays.AddRange(data.List("intro_dancer_delays").Select(delay => delay.AsInt()));
        _buriers.Clear();
        _buriers.AddRange(data.List("burial_brood").Select(one => new Burier(one.Int("x"), one.Int("y"), one.Int("heading"), one.Text("effect"))));
        static (int From, int Digits) Field(DataNode field) => (field.Int("from"), field.Int("digits"));
        static (Vector2I Corner, (int From, int Digits) Across, (int From, int Digits) Down) Scatter(DataNode scatter) =>
            (new Vector2I(scatter.Int("x"), scatter.Int("y")), Field(scatter.Get("across")), Field(scatter.Get("down")));
        _boxes.Clear();
        foreach ((Facing way, string name) in new[] { (Facing.Left, "left"), (Facing.Right, "right") })
        {
            _boxes[way] = data.Get("facing_colliders").Get(name).Entries().ToDictionary(
                box => box.Key,
                box => new PixelRect(box.Value.Int("left"), box.Value.Int("top"), box.Value.Int("width"), box.Value.Int("height")),
                StringComparer.Ordinal);
        }

        _healthBands.Clear();
        _healthBands.AddRange(data.List("health_bands").Select(band => band.AsInt()));
        if (_healthBands.Count == 0)
            throw data.Problem("must give at least one band of its health");
        ReadPartColours(data.Get("part_colours").Get("opening"), _openingColours);
        ReadPartColours(data.Get("part_colours").Get("fought"), _foughtColours);
        _cannonChoice = Field(data.Get("cannon_choice"));
        _cannonTurns.Clear();
        _cannonTurns.AddRange(data.List("cannon_turns").Select(turn => (turn.Text("cannon"), PixelPosition.FromWhole(turn.Int("x"), turn.Int("y")))));
        if (_cannonTurns.Count != 1 << _cannonChoice.Digits)
            throw data.Problem("must give a cannon's turn for every number its choice of cannon can draw");
        _scatteredDrops = DropTable.Read(data.Get("scattered_drops"));
        _dropScatter = Scatter(data.Get("drop_scatter"));
        _dyingScatter = Scatter(data.Get("dying_scatter"));
    }

    /// <summary>Reads the colours each of its parts shows changed, by the part's name.</summary>
    private static void ReadPartColours(DataNode parts, Dictionary<string, ColourSwap[]> into)
    {
        into.Clear();
        foreach ((string part, DataNode colours) in parts.Entries())
            into[part] = ColourChange.ReadSwaps(colours);
    }

    /// <summary>Shows its tail and arms with the colours given changed.</summary>
    private void ShowPartColours(Dictionary<string, ColourSwap[]> colours)
    {
        foreach ((string part, ColourSwap[] swaps) in colours)
        {
            (GetNodeOrNull<ActorVisual>(part) ?? throw new InvalidOperationException($"Draygon's data colours its part \"{part}\", which it does not have."))
                .Recolour(nameof(Draygon), swaps);
        }
    }

    /// <summary>Which band of its health it is in, from nothing up; its belly reddens from each to the next.</summary>
    private int HealthBand
    {
        get
        {
            for (int band = 0; band < _healthBands.Count; band++)
            {
                if (Health.Current >= _healthBands[band])
                    return band;
            }

            return _healthBands.Count - 1;
        }
    }

    /// <summary>A picture of its body or eye, in the colours of the health it has left.</summary>
    private string Coloured(string picture) =>
        picture.Replace("{health}", "_health_" + HealthBand.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step)
    {
        // A step with no picture is a wait: it leaves the body as it was shown.
        if (step.Picture.Length > 0)
            _shown = step.Picture;
        return Coloured(_shown);
    }

    /// <summary>A place taken by chance: a corner and, added to it each way, some binary digits of a number drawn.</summary>
    private PixelOffset Scattered((Vector2I Corner, (int From, int Digits) Across, (int From, int Digits) Down) scatter)
    {
        RandomDraw draw = Random.Draw();
        return PixelOffset.FromWhole(
            scatter.Corner.X + draw.Part(scatter.Across.From, scatter.Across.Digits), scatter.Corner.Y + draw.Part(scatter.Down.From, scatter.Down.Digits));
    }

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !_dying && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        _wait = IntroUpdates;
        _leftStart = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _quickening = Pixels.FromNumber(SwoopQuickening);
        Face(Facing.Right);
        // Its tail and arms show its opening's colours until it is first hurt.
        _fought = false;
        ShowPartColours(_openingColours);
    }

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part)
    {
        Reaction reaction = base.ReactionTo(hit, part);
        return part.Name == "Belly" ? reaction : reaction with { DamageTimes = 0, Status = "", StatusWhen = StatusWhen.Never };
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part)
    {
        // Every hit makes its swoops steeper, up to a limit.
        Pixels steeper = _quickening + Pixels.FromNumber(SwoopQuickeningPerHit);
        if (steeper < Pixels.FromNumber(SwoopQuickeningLimit))
            _quickening = steeper;
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        BeginDeath();
        return false;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!_intangible && !_passesThroughSamus)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        const string dyingEffect = "dying_effect:";
        if (cue.StartsWith(dyingEffect, StringComparison.Ordinal))
        {
            LeaveEffect(cue[dyingEffect.Length..], Position + Scattered(_dyingScatter));
            return;
        }

        switch (cue)
        {
            case "goop":
                break;
            case "intangible":
                _intangible = true;
                return;
            case "paralyse":
                // Its tail and arms stop where they are, and stay as they were shown.
                _parts.Remove("Tail");
                _parts.Remove("Arms");
                return;
            default:
                throw new InvalidOperationException($"Draygon's body gives the cue \"{cue}\", which it does not know.");
        }

        // With no place free for a goop, none leaves and no number is drawn to spread it.
        if (!Room.HasFreePlace)
            return;

        // A goop leaves from by its tail, somewhere between straight down and straight ahead.
        PixelPosition from = PixelPosition.FromWhole(Position.X.Whole + (_pass == Facing.Right ? 24 : -28), Position.Y.Whole - 16);
        // As the cartridge counts an angle: none right, a quarter turn up.
        int scatter = Random.Draw().Part(0, 6);
        int angle = (_pass == Facing.Right ? 3 * Turns.PerTurn / 4 : Turns.PerTurn / 2) + scatter;
        Fire("goop", from, CartridgeAngles.VelocityAlong(angle, WholePixels(Shot("goop").Speed)));
    }

    /// <summary>Whether its wall cannons fire during what it is doing now.</summary>
    private bool CannonsFire => _phase switch
    {
        Phase.IntroDelay or Phase.IntroDance or Phase.SwoopSetupRight or Phase.SwoopSetupLeft or Phase.SwoopDown or
            Phase.SwoopApex or Phase.SwoopUp or Phase.GoopApproach or Phase.TryGrab or Phase.FlyUp => true,
        Phase.GoopSetup or Phase.GoopSpit => _pass == Facing.Right,
        _ => false,
    };

    /// <summary>Whether its breath bubbles up during what it is doing now.</summary>
    private bool Breathes => _phase switch
    {
        Phase.SwoopDown or Phase.GoopApproach or Phase.GoopRecovery => true,
        Phase.SwoopUp => _pass == Facing.Left,
        _ => false,
    };

    /// <inheritdoc/>
    protected override void Behave()
    {
        long clock = Room.Session.Clock.Updates;
        Player samus = Room.Player;
        if (CannonsFire && clock % 64 == 0)
        {
            // One of its cannons, or none, picked by chance, fires at Samus if it is still whole.
            (string turn, PixelPosition from) = _cannonTurns[Random.Draw().Part(_cannonChoice.From, _cannonChoice.Digits)];
            if (turn.Length > 0)
                Room.Enemies.OfType<DraygonCannon>().FirstOrDefault(cannon => cannon.Which == turn)?.FireAtSamusFrom(from);
        }

        if (Breathes && Updates % 128 == 0 && BubbleEffect.Length > 0)
            LeaveEffect(BubbleEffect, Position + PixelOffset.FromWhole(-16, -16));

        switch (_phase)
        {
            case Phase.IntroDelay:
                if (_wait == IntroUpdates)
                {
                    // Its brood waits at the left of the swim line until each sets off.
                    foreach (int delay in _dancerDelays)
                        _dancers.Add((LeaveEffect(DancerEffect, PixelPosition.FromWhole(16, SwimLineY)), delay, 0));
                }

                if (--_wait > 0)
                    break;
                _phase = Phase.IntroDance;
                _danced = 0;
                break;

            case Phase.IntroDance:
                if (_danced >= IntroDanceUpdates)
                {
                    _phase = Phase.SwoopSetupRight;
                    _pass = Facing.Right;
                    break;
                }

                StepDancers();
                _danced++;
                break;

            case Phase.SwoopSetupRight:
                BuildSwoop();
                SetSwoopSpeed(Math.Abs(samus.Position.X.Whole - _leftStart.X.Whole), Facing.Right);
                _swoopLeft = _swoopHeights.Count;
                _phase = Phase.SwoopDown;
                Face(Facing.Right);
                break;

            case Phase.SwoopSetupLeft:
                SetSwoopSpeed(Math.Abs(_leftStart.X.Whole + RightSideDistance - samus.Position.X.Whole), Facing.Left);
                _swoopLeft = _swoopHeights.Count;
                _phase = Phase.SwoopDown;
                Face(Facing.Left);
                break;

            case Phase.SwoopDown:
                if (_swoopLeft == 26)
                    PlayPart("Arms", _pass == Facing.Right ? "arms_reach_right" : "arms_reach_left");
                Position = new PixelPosition(Position.X, Pixels.FromWhole(HeightAt(_swoopLeft)));
                if (--_swoopLeft == 0)
                    _phase = Phase.SwoopApex;
                else
                    Position += new PixelOffset(_acrossSpeed, Pixels.Zero);
                break;

            case Phase.SwoopApex:
                SetSwoopSpeed(_pass == Facing.Right ? RightSideDistance - Position.X.Whole : Math.Abs(Position.X.Whole - _leftStart.X.Whole), _pass);
                _swoopLeft = 0;
                _phase = Phase.SwoopUp;
                break;

            case Phase.SwoopUp:
                if (_swoopLeft == 26)
                    PlayPart("Arms", _pass == Facing.Right ? "arms_right" : "arms_left");
                Position = new PixelPosition(Position.X, Pixels.FromWhole(HeightAt(_swoopLeft)));
                if (++_swoopLeft < _swoopHeights.Count)
                {
                    Position += new PixelOffset(_acrossSpeed, Pixels.Zero);
                    break;
                }

                // At the end of a pass it swoops back, or as likely swims through spitting goop.
                bool swoops = Random.Last.Part(0, 1) == 1;
                if (_pass == Facing.Right)
                {
                    _pass = Facing.Left;
                    _phase = swoops ? Phase.SwoopSetupLeft : Phase.GoopSetup;
                    _afterGoop = Phase.SwoopSetupRight;
                }
                else
                {
                    Position = swoops ? _leftStart : new PixelPosition(_leftStart.X, Position.Y);
                    _pass = Facing.Right;
                    _phase = swoops ? Phase.SwoopSetupRight : Phase.GoopSetup;
                    _afterGoop = Phase.SwoopSetupLeft;
                }

                break;

            case Phase.GoopSetup:
                Position = PixelPosition.FromWhole(_pass == Facing.Right ? -80 : _leftStart.X.Whole + RightSideDistance, SwimLineY);
                _acrossSpeed = _pass.Toward(Pixels.FromWhole(1));
                _goopAngle = 0;
                _phase = Phase.GoopApproach;
                Face(_pass);
                break;

            case Phase.GoopApproach:
                if (Math.Abs(Position.X.Whole - samus.Position.X.Whole) < GoopRange)
                {
                    _phase = Phase.GoopSpit;
                    _goopsLeft = GoopsPerPass;
                    break;
                }

                SwimAlong();
                break;

            case Phase.GoopSpit:
                if (samus.SpeedHalvings != 0)
                {
                    _phase = Phase.TryGrab;
                    break;
                }

                if (Random.Last.Part(0, 4) == 0)
                {
                    if (--_goopsLeft <= 0)
                    {
                        EndGoopSpitting();
                        break;
                    }

                    Play(_pass == Facing.Right ? "fire_goop_right" : "fire_goop_left", restart: true);
                }

                SwimAlong();
                if (HasLeftArena())
                    EndGoopSpitting();
                break;

            case Phase.GoopRecovery:
                if (_pass == Facing.Left && samus.SpeedHalvings != 0)
                {
                    _phase = Phase.TryGrab;
                    break;
                }

                SwimAlong();
                if (!HasLeftArena())
                    break;
                _phase = _afterGoop;
                Position = PixelPosition.FromWhole(_pass == Facing.Right ? 592 : -80, -80);
                _pass = _pass.Opposite();
                break;

            case Phase.TryGrab:
                if (samus.SpeedHalvings == 0)
                {
                    _phase = Phase.FlyUp;
                    break;
                }

                _passesThroughSamus = true;
                int claw = Position.X.Whole + (Facing == Facing.Right ? 8 : -8);
                if (Math.Abs(claw - samus.Position.X.Whole) >= 8 || Math.Abs(Position.Y.Whole - samus.Position.Y.Whole) >= 8)
                {
                    MoveToward(samus.Position, ChaseSpeed);
                    break;
                }

                PlayPart("Arms", Facing == Facing.Right ? "arms_grab_right" : "arms_grab_left");
                if (samus is Samus { Grapple.IsOut: true })
                {
                    // Her grapple beam, out as its claws close, wards it off.
                    _phase = Phase.RepelGrapple;
                    break;
                }

                samus.Facing = Facing;
                samus.ChangeState(new GrabbedState(EscapePresses));
                _holds = true;
                // A grab sets the whole pixels of the height its spiral winds about and leaves the part of a pixel the
                // fight's last spiral lifted it to, so that part decides on which updates this one rises a pixel.
                (_spiralRadius, _spiralAngle, _spiralTurn, _spiralY) = (Pixels.Zero, 3 * Turns.PerTurn / 4, 8 * 256, _spiralY.WithWhole(384));
                _phase = Phase.Carry;
                break;

            case Phase.RepelGrapple:
                (samus as Samus)?.Grapple?.Release();
                _phase = Phase.Release;
                break;

            case Phase.Carry:
                if (TakesCurrent(samus))
                    break;
                if (Math.Abs(Position.X.Whole - 256) < 2 && Math.Abs(Position.Y.Whole - 384) < 2)
                {
                    _phase = Phase.Flail;
                    Play(Facing == Facing.Right ? "roar_right" : "roar_left", restart: true);
                    break;
                }

                MoveToward(PixelPosition.FromWhole(256, 384), ChaseSpeed);
                CarrySamus(samus);
                break;

            case Phase.Flail:
                if (TakesCurrent(samus))
                    break;
                if (Random.Last.Part(0, 8) == 0)
                {
                    _phase = Phase.BeginTailWhip;
                    break;
                }

                // It winds upward in a widening spiral, four times as wide as it is tall.
                Position = PixelPosition.FromWhole(
                    256 + CartridgeAngles.CircleCosine(_spiralAngle, _spiralRadius.Whole).Whole,
                    _spiralY.Whole + CartridgeAngles.CircleSine(_spiralAngle + Turns.PerTurn / 2, _spiralRadius.Whole / 4).Whole);
                if (Updates % 8 == 0 && FoamEffect.Length > 0)
                    LeaveEffect(FoamEffect, Mouth);
                _spiralRadius += Pixels.FromNumber(0.125);
                if (_spiralRadius.Whole >= 160)
                {
                    _phase = Phase.FinalWhips;
                    break;
                }

                // Its turning slows by a little each update; only whole steps of it count.
                _spiralTurn--;
                _spiralAngle = Turns.Wrap(_spiralAngle + (_spiralTurn >> 8));
                _spiralY -= Pixels.FromNumber(0.25);
                if (_spiralY.Whole < 64)
                {
                    _phase = Phase.FinalWhips;
                    break;
                }

                CarrySamus(samus);
                break;

            case Phase.BeginTailWhip:
                _wait = 64;
                _phase = Phase.TailWhip;
                StepTailWhip(samus);
                PlayPart("Tail", Facing == Facing.Right ? "tail_whip_right" : "tail_whip_left", restart: true);
                break;

            case Phase.TailWhip:
                StepTailWhip(samus);
                break;

            case Phase.FinalWhips:
                PlayPart("Tail", Facing == Facing.Right ? "tail_final_whips_right" : "tail_final_whips_left", restart: true);
                _phase = Phase.FinalWhipsWait;
                CarrySamus(samus);
                break;

            case Phase.FinalWhipsWait:
                CarrySamus(samus);
                break;

            case Phase.Release:
                LetSamusGo();
                _phase = Phase.FlyUp;
                _passesThroughSamus = false;
                PlayPart("Tail", Facing == Facing.Right ? "tail_right" : "tail_left");
                break;

            case Phase.FlyUp:
                Position -= PixelOffset.FromWhole(0, 4);
                if (Position.Y.Whole >= 0)
                    break;
                _passesThroughSamus = false;
                _phase = Phase.SwoopSetupRight;
                _pass = Facing.Right;
                Position = _leftStart;
                break;

            case Phase.Dying:
                if (Updates % 16 == 0 && FoamEffect.Length > 0)
                    LeaveEffect(FoamEffect, Mouth);
                // It drifts toward where it will lie, aiming by quarters of a block.
                Drift(CartridgeAngles.AimAngle(256 / 4 - Position.X.Whole / 4, 480 / 4 - Position.Y.Whole / 4), 1);
                if (Math.Abs(Position.X.Whole - 256) >= 4 || Math.Abs(Position.Y.Whole - 480) >= 4)
                    break;
                foreach (Burier one in _buriers)
                {
                    // Its heading is the way it drifts, from the right through down; the cartridge drifts it back along
                    // the opposite heading, counted from the right through up.
                    PixelOffset drift = -CartridgeAngles.VelocityAlong(Turns.PerTurn / 2 - one.Heading, 1).OverOneUpdate;
                    _burying.Add((LeaveEffect(one.Effect, PixelPosition.FromWhole(one.X, one.Y)), drift));
                }

                _phase = Phase.DyingWait;
                if (VictoryMusic.Length > 0)
                    Room.Session.Sounds.RequestMusic(VictoryMusic);
                _wait = 416;
                Play("rest", restart: true);
                foreach (string part in new[] { "Tail", "Arms" })
                {
                    _parts.Remove(part);
                    GetNodeOrNull<ActorVisual>(part)?.SetShown(false);
                }

                PlayPart("Eye", Facing == Facing.Right ? "eye_dead_right" : "eye_dead_left");
                break;

            case Phase.DyingWait:
                Smoke(clock);
                if (--_wait <= 0)
                    _phase = Phase.DyingSink;
                else
                    MoveBurialBrood();
                break;

            case Phase.DyingSink:
                Smoke(clock);
                MoveBurialBrood();
                Position += PixelOffset.FromWhole(0, 1);
                if (Position.Y.Whole < 576)
                    break;
                for (int drop = 0; drop < DeathDrops; drop++)
                    Drop(_scatteredDrops, PixelPosition.Origin + Scattered(_dropScatter));

                foreach ((Effect sprite, _) in _burying)
                    Room.Remove(sprite);
                _burying.Clear();
                Die();
                return;

            default:
                throw new InvalidOperationException($"Draygon has no phase {_phase}.");
        }

        if (!_dying)
            LookAt(samus);
        StepParts();
    }

    /// <summary>Moves each of its brood that has set off one step along the dance; one that has finished is gone.</summary>
    private void StepDancers()
    {
        for (int index = _dancers.Count - 1; index >= 0; index--)
        {
            (Effect sprite, int delay, int step) = _dancers[index];
            if (_danced < delay)
                continue;
            if (step >= _dance.Count)
            {
                Room.Remove(sprite);
                _dancers.RemoveAt(index);
                continue;
            }

            sprite.Position += _dance[step];
            _dancers[index] = (sprite, delay, step + 1);
        }
    }

    /// <summary>Moves the brood that buries it one update on their way.</summary>
    private void MoveBurialBrood()
    {
        foreach ((Effect sprite, PixelOffset drift) in _burying)
            sprite.Position += drift;
    }

    /// <summary>
    /// Works out the heights of a swoop: up from the swim line faster and faster, as far as the height it started the
    /// fight at. A swoop goes down through them and then back up.
    /// </summary>
    private void BuildSwoop()
    {
        _swoopHeights.Clear();
        _swoopStartY = Position.Y.Whole;
        int y = SwimLineY;
        Pixels speed = Pixels.Zero;
        while (true)
        {
            speed += _quickening;
            int next = y - speed.Whole;
            if (next < _leftStart.Y.Whole)
                break;
            y = next;
            _swoopHeights.Add(next);
        }

        if (_swoopHeights.Count == 0)
            throw new InvalidOperationException("Draygon starts below its swim line and has no swoop to make.");
    }

    /// <summary>The height of a swoop at one of its places; the place past the last is the height the swoop began at.</summary>
    private int HeightAt(int place) => place == _swoopHeights.Count ? _swoopStartY : _swoopHeights[place];

    /// <summary>Sets its speed across to cover a distance in the time half a swoop takes.</summary>
    private void SetSwoopSpeed(int distance, Facing way)
    {
        _acrossSpeed = way.Toward(Pixels.FromWhole(distance).Share(1, _swoopHeights.Count));
        _pass = way;
    }

    /// <summary>Swims one update along its goop pass, bobbing about the swim line.</summary>
    private void SwimAlong()
    {
        Position = new PixelPosition(
            Position.X + _acrossSpeed,
            Pixels.FromWhole(SwimLineY + CartridgeAngles.CircleCosine(_goopAngle, 32).Whole));
        _goopAngle = Turns.Wrap(_goopAngle + 1);
    }

    /// <summary>Stops its spitting for this pass: it swims on out of the chamber.</summary>
    private void EndGoopSpitting()
    {
        PlayPart("Arms", _pass == Facing.Right ? "arms_right" : "arms_left");
        _phase = Phase.GoopRecovery;
    }

    /// <summary>Whether its pass has carried it out of the chamber.</summary>
    private bool HasLeftArena() => _pass == Facing.Right ? Position.X.Whole >= RightSideDistance : Position.X.Whole < -80;

    /// <summary>Moves it straight toward a place at a speed.</summary>
    private void MoveToward(PixelPosition target, int speed)
    {
        Drift(CartridgeAngles.AimAngle(target.X.Whole - Position.X.Whole, target.Y.Whole - Position.Y.Whole), speed);
    }

    /// <summary>Moves it along a bearing, as the cartridge counts an angle, at a whole number of pixels an update.</summary>
    private void Drift(int bearing, int speed) => Position += CartridgeAngles.VelocityAlong(bearing, speed).OverOneUpdate;

    /// <summary>A speed that is a whole number of pixels, as the cartridge splits one along an angle.</summary>
    private static int WholePixels(Pixels speed) => speed.Fraction == 0
        ? speed.Whole
        : throw new InvalidOperationException($"Draygon's shot speed {speed} is not a whole number of pixels.");

    /// <summary>Where its mouth is.</summary>
    private PixelPosition Mouth => Position + PixelOffset.FromWhole(Facing == Facing.Right ? 32 : -32, -16);

    /// <summary>Keeps Samus in its claws. Once she has struggled free it lets her be and flies off.</summary>
    private void CarrySamus(Player samus)
    {
        if (!_holds)
            return;
        if (samus.State is not GrabbedState)
        {
            _holds = false;
            _phase = Phase.FlyUp;
            return;
        }

        samus.Position = PixelPosition.FromWhole(Position.X.Whole + (Facing == Facing.Right ? 8 : -8), Position.Y.Whole + 40);
    }

    /// <summary>Holds Samus through one update of a lash of its tail, and goes back to its spiral when the lash is over.</summary>
    private void StepTailWhip(Player samus)
    {
        CarrySamus(samus);
        if (_phase == Phase.TailWhip && --_wait <= 0)
            _phase = Phase.Flail;
    }

    /// <summary>Lets Samus go, if it holds her.</summary>
    private void LetSamusGo()
    {
        if (!_holds)
            return;
        _holds = false;
        if (Room.Player.State is GrabbedState)
            Room.Player.Release();
    }

    /// <summary>
    /// While it carries Samus or winds upward with her and she has her grapple beam on a live wire, the current runs
    /// through it: it is made hurt, flashing as long as a shot makes it (its own flash and eight updates more, on the
    /// cartridge as here), and does nothing else on that update. The current's harm is done while it is hurt (<see cref="WhileHurt"/>). Returns whether that is so.
    /// </summary>
    private bool TakesCurrent(Player samus)
    {
        if (samus is not Samus { Grapple.IsLatched: true })
            return false;
        BeHurtFor(HurtFlashUpdates);
        return true;
    }

    /// <summary>
    /// Hurt, by the current or by anything else, it takes the current's harm on every eighth update of its own while
    /// Samus's grapple beam is on a live wire, as the cartridge deals the shock in its hurt's own behaviour. Its
    /// first turn hurt shows its tail and arms in its fight's colours, which they keep.
    /// </summary>
    protected override void WhileHurt()
    {
        if (!_fought)
        {
            _fought = true;
            ShowPartColours(_foughtColours);
        }

        if (Room.Player is not Samus { Grapple.IsLatched: true } || Updates % 8 != 0)
            return;
        Health.Take(Math.Min(GrappleShockDamage, Health.Current));
        if (Health.IsEmpty)
            BeginDeath();
    }

    /// <summary>Starts its death: it lets Samus go and drifts down to where it will lie.</summary>
    private void BeginDeath()
    {
        if (_dying)
            return;
        _dying = true;
        LetSamusGo();
        (Room.Player as Samus)?.Grapple?.Release();
        Play(Facing == Facing.Right ? "dying_right" : "dying_left", restart: true);
        PlayPart("Eye", Facing == Facing.Right ? "eye_dying_right" : "eye_dying_left");
        _phase = Phase.Dying;
    }

    /// <summary>Leaves smoke over where it lies, at random places, every eighth update.</summary>
    private void Smoke(long clock)
    {
        if (clock % 8 != 0 || SmokeEffect.Length == 0)
            return;
        RandomDraw draw = Random.Draw();
        LeaveEffect(SmokeEffect, PixelPosition.FromWhole(192 + draw.Part(0, 7), 400 + draw.Part(8, 6)));
    }

    /// <summary>Turns its eye toward Samus: up, down, left or right, whichever is nearest.</summary>
    private void LookAt(Player samus)
    {
        PixelPosition gaze = Position + PixelOffset.FromWhole(Facing == Facing.Right ? 24 : -24, -32);
        // Bubbles rise from by its eye all the while it watches her.
        if (Updates % 128 == 0 && EyeBubbleEffect.Length > 0)
            LeaveEffect(EyeBubbleEffect, gaze);
        PixelOffset to = samus.Position - gaze;
        string way = Math.Abs(to.X.Whole) >= Math.Abs(to.Y.Whole) ? (to.X.IsNegative ? "left" : "right") : (to.Y.IsNegative ? "up" : "down");
        PlayPart("Eye", $"eye_{(Facing == Facing.Right ? "right" : "left")}_looking_{way}");
    }

    /// <summary>Turns it and its tail and arms to face a way.</summary>
    private void Face(Facing way)
    {
        Facing = way;
        // Its belly and its shell lie where the way it faces puts them.
        foreach (Collider collider in EntityNode.CollidersOf(this))
        {
            collider.Rect = _boxes[way].TryGetValue(collider.Name.ToString(), out PixelRect box)
                ? box
                : throw new InvalidOperationException($"Draygon's data gives its collider \"{collider.Name}\" no place when it faces {way}.");
        }

        string side = way == Facing.Right ? "right" : "left";
        Play($"body_{side}", restart: true);
        PlayPart("Tail", $"tail_{side}");
        PlayPart("Arms", $"arms_{side}");
    }

    /// <summary>Starts one of its parts on a timeline, unless it is already playing it.</summary>
    private void PlayPart(string part, string timeline, bool restart = false)
    {
        FrameTimeline wanted = Timelines.Get(timeline);
        if (!restart && _parts.TryGetValue(part, out TimelinePlayer? player) && player.Timeline == wanted)
            return;
        _parts[part] = new TimelinePlayer(wanted);
        // A lash cut short leaves its body where it belongs.
        if (part == "Tail")
            _bodyShift = Vector2I.Zero;
    }

    /// <summary>Moves its parts' timelines on, and acts on their cues.</summary>
    private void StepParts()
    {
        foreach ((string part, TimelinePlayer player) in _parts.ToList())
        {
            foreach (string cue in player.Step())
            {
                if (cue == "whip_hit")
                {
                    // Its tail lands: Samus is hurt where she hangs, the room shakes, and its swoops lose their edge.
                    _quickening = Pixels.FromNumber(SwoopQuickening);
                    Room.Player.Sap(new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Source = this });
                    Room.Shake(WhipShakeStrength, 32, ShakeWay.Down);
                    if (DustEffect.Length > 0)
                        LeaveEffect(DustEffect, Room.Player.Position + PixelOffset.FromWhole(0, 16));
                }
                else if (cue == "release")
                {
                    _phase = Phase.Release;
                }
                else if (cue.StartsWith("sound:", StringComparison.Ordinal))
                {
                    Sound(cue["sound:".Length..]);
                }
                else if (cue.StartsWith("shift:", StringComparison.Ordinal))
                {
                    string[] by = cue.Split(':');
                    _bodyShift = new Vector2I(
                        int.Parse(by[1], System.Globalization.CultureInfo.InvariantCulture), int.Parse(by[2], System.Globalization.CultureInfo.InvariantCulture));
                }
                else
                {
                    throw new InvalidOperationException($"Draygon's part \"{part}\" gives the cue \"{cue}\", which it does not know.");
                }
            }
        }

        // Its parts are shown after all their cues, so that a lash's shift moves the body and eye in the same update.
        foreach ((string part, TimelinePlayer player) in _parts)
        {
            if (GetNodeOrNull<ActorVisual>(part) is not { } visual)
                continue;
            TimelineStep step = player.Current;
            visual.Show(step with { Picture = Coloured(step.Picture) });
            // Every part of it flashes with its body.
            if (Visual is { } body)
                visual.Tint(body.Modulate);
        }

        foreach (string layer in new[] { "Visual", "Eye" })
        {
            if (GetNodeOrNull<ActorVisual>(layer) is { } shifted)
                shifted.Position = _bodyShift;
        }
    }
}
