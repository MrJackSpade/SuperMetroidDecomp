using Engine.Actors;
using Engine.Actors.Components;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Yapping Maw: a mouth on a long neck, rooted in the floor or the ceiling. When Samus comes within reach it darts
/// out at her; if it catches her it holds her, helpless, while it draws back and chews, and then lets her go. Frozen
/// or killed, it lets go at once.
/// </summary>
/// <remarks>
/// Its mouth plays "attack_up", "attack_up_right" and so on round the eight ways it can strike, each with the cue
/// "bite" for its sound, and once it has drawn back "cooldown_straight", "cooldown_right" or "cooldown_left", whose
/// cue "hold:&lt;way&gt;" moves where it holds Samus. The visual named "Root" plays "root" and those named "Link1"
/// through "Link4" play "link". Its data gives where it holds Samus for each way under "held_offsets". How fast it
/// darts follows the speed curve its data names.
/// </remarks>
public partial class YappingMaw : Enemy
{
    /// <summary>How near Samus must come, in pixels, for it to strike.</summary>
    [Export]
    public int ActivationDistance { get; set; } = 64;

    /// <summary>Whether it hangs from the ceiling and strikes downward.</summary>
    [Export]
    public bool StrikesDown { get; set; }

    /// <summary>How near Samus must be, in pixels, for it to hold back rather than strike.</summary>
    [Export, DataValue("safety_distance")]
    public int SafetyDistance { get; set; } = 32;

    /// <summary>The furthest it reaches, in pixels.</summary>
    [Export, DataValue("longest_reach")]
    public int LongestReach { get; set; } = 64;

    /// <summary>How many updates after letting go, or drawing back, before it can catch Samus again.</summary>
    [Export, DataValue("grab_cooldown")]
    public int GrabCooldown { get; set; } = 48;

    /// <summary>How many updates it stays drawn back before it lets go and watches again.</summary>
    [Export, DataValue("retracted_hold")]
    public int RetractedHold { get; set; } = 65;

    /// <summary>How far from its mouth's resting place its root sits, in pixels.</summary>
    [Export, DataValue("root_distance")]
    public int RootDistance { get; set; } = 8;

    /// <summary>The sound of it biting, by name, heard only while its mouth is in view.</summary>
    [Export, DataValue("bite_sound")]
    public string BiteSound { get; set; } = "";

    /// <summary>The data file of the speed curve its darting follows.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private const int FullStretch = 128;
    private const int Links = 4;

    private static readonly string[] Ways = ["up", "up_right", "right", "down_right", "down", "down_left", "left", "up_left"];

    private enum Doing
    {
        Watching,
        BeginStrike,
        Striking,
        DrawnBack,
    }

    private readonly Dictionary<string, PixelOffset> _heldOffsets = new(StringComparer.Ordinal);
    private SpeedCurve? _curve;
    private Doing _doing;
    private PixelPosition _origin;
    private int _reach;
    private int _bearing;
    private string _way = "up";
    private PixelOffset _heldAt;
    private Pixels _stretch;
    private int _place;
    private bool _drawingBack;
    private bool _holds;
    private int _cooldown;
    private Countdown _drawnBack;
    private TimelinePlayer? _link;
    private TimelinePlayer? _root;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _heldOffsets.Clear();
        foreach ((string way, DataNode offset) in data.Get("held_offsets").Entries())
            _heldOffsets[way] = PixelOffset.FromWhole(offset.Int("x"), offset.Int("y"));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _origin = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _drawnBack = Countdown.Of(RetractedHold);
        _link = new TimelinePlayer(Timelines.Get("link"));
        _root = new TimelinePlayer(Timelines.Get("root"));
        Play(StrikesDown ? "attack_down" : "attack_up");
        ShowNeck();
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "bite")
        {
            if (IsInView(0))
                Sound(BiteSound);
        }
        else if (cue.StartsWith("hold:", StringComparison.Ordinal))
        {
            _heldAt = HeldOffset(cue["hold:".Length..]);
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // It can catch her only once its cooldown has run out; having caught her, not again until the strike is over.
        if (_cooldown != 0 || _holds || IsDead)
            return;
        _cooldown = -1;
        _holds = true;
        Room.Player.Hold();
    }

    /// <inheritdoc/>
    protected override void OnStatusBegan(string status)
    {
        if (status == Frozen)
            LetGo();
    }

    /// <inheritdoc/>
    protected override void OnDied() => LetGo();

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (_cooldown > 0)
            _cooldown--;
        switch (_doing)
        {
            case Doing.Watching:
                PixelOffset toSamus = Target - _origin;
                int across = toSamus.X.Whole, down = toSamus.Y.Whole;
                int distance = (int)Math.Sqrt((double)across * across + (double)down * down);
                if (distance < SafetyDistance)
                {
                    _cooldown = GrabCooldown;
                    break;
                }

                if (distance >= ActivationDistance)
                    break;
                _reach = Math.Min(distance, LongestReach);
                _bearing = Turns.AngleOf(across, down);
                _doing = Doing.BeginStrike;
                break;

            case Doing.BeginStrike:
                _stretch = Pixels.Zero;
                _place = 0;
                _drawingBack = false;
                // The way it strikes is the nearest of eight to its bearing, counted round from straight up.
                _way = Ways[Turns.Wrap(_bearing + Turns.PerTurn / 4 + Turns.PerTurn / 16) * Ways.Length / Turns.PerTurn];
                Play($"attack_{_way}", restart: true);
                _heldAt = HeldOffset(_way);
                _doing = Doing.Striking;
                break;

            case Doing.Striking:
                Strike();
                break;

            case Doing.DrawnBack:
                if (_holds)
                    HoldSamus();
                _drawnBack = _drawnBack.Stepped();
                if (_drawnBack.IsRunning || Room.Player.IsDead)
                    break;
                LetGo();
                _cooldown = GrabCooldown;
                _drawnBack = Countdown.Of(RetractedHold);
                _doing = Doing.Watching;
                break;

            default:
                throw new InvalidOperationException($"Undefined Yapping Maw stage {_doing}.");
        }

        ShowNeck();
    }

    /// <summary>Moves its strike on by one update: out to full stretch and back, faster all the while.</summary>
    private void Strike()
    {
        SpeedCurve curve = _curve ?? throw new InvalidOperationException($"The Yapping Maw \"{Name}\" is not in a room.");
        Position = Along(Links);
        Pixels speed = curve.SpeedAt(Math.Min(_place, (curve.Marks - 1) * SpeedCurve.PartsPerMark));
        _stretch += _drawingBack ? -speed : speed;
        _place += SpeedCurve.PartsPerMark;
        if (!_stretch.IsNegative)
        {
            if (_stretch.Whole >= FullStretch)
            {
                _stretch = Pixels.FromWhole(FullStretch);
                if (_drawingBack)
                    _place += SpeedCurve.PartsPerMark;
                _drawingBack = !_drawingBack;
            }

            if (_holds)
                HoldSamus();
            return;
        }

        _doing = Doing.DrawnBack;
        _cooldown = GrabCooldown;
        string side = _way == "right" ? "right" : _way == "left" ? "left" : "straight";
        Play($"cooldown_{side}", restart: true);
        // Drawn back from a strike straight across, or by one that strikes downward, Samus is brought along at once.
        if (_holds && (StrikesDown || side != "straight"))
            HoldSamus();
    }

    /// <summary>Where the <paramref name="link"/>th of its links is, the last being its mouth: evenly out along its strike.</summary>
    private PixelPosition Along(int link)
    {
        int stretch = Math.Clamp(_stretch.Whole, 0, FullStretch);
        Pixels length = Pixels.FromWhole(_reach).Share(stretch * link, FullStretch * Links);
        return PixelPosition.FromWhole(
            _origin.X.Whole + Turns.Cosine(_bearing, length).Whole,
            _origin.Y.Whole + Turns.Sine(_bearing, length).Whole);
    }

    /// <summary>Keeps Samus in its mouth.</summary>
    private void HoldSamus() => Room.Player.Position = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole) + _heldAt;

    /// <summary>Lets Samus go, if it holds her.</summary>
    private void LetGo()
    {
        if (!_holds)
            return;
        _holds = false;
        if (!Room.Player.IsDead)
            Room.Player.Release();
    }

    /// <summary>Where it holds Samus when it has struck the given way.</summary>
    private PixelOffset HeldOffset(string way) =>
        _heldOffsets.TryGetValue(way, out PixelOffset offset) ? offset : throw new InvalidOperationException($"The Yapping Maw \"{Name}\" has no held offset for \"{way}\".");

    /// <summary>Puts its root and the links of its neck where they are.</summary>
    private void ShowNeck()
    {
        _link?.Step();
        _root?.Step();
        if (_root is not null && GetNodeOrNull<ActorVisual>("Root") is { } root)
        {
            root.Position = new Vector2(_origin.X.Whole - Position.X.Whole, _origin.Y.Whole + (StrikesDown ? -RootDistance : RootDistance) - Position.Y.Whole);
            root.Show(_root.Current);
        }

        for (int link = 0; link < Links; link++)
        {
            if (_link is null || GetNodeOrNull<ActorVisual>($"Link{link + 1}") is not { } visual)
                continue;
            PixelPosition at = _doing == Doing.Striking ? Along(link) : _origin;
            visual.Position = new Vector2(at.X.Whole - Position.X.Whole, at.Y.Whole - Position.Y.Whole);
            visual.Show(_link.Current);
        }
    }
}
