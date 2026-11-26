using Engine.Actors;
using Engine.Actors.Components;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
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
/// through "Link4" play "link". One that strikes downward plays "cooldown_straight_down", "cooldown_right_down",
/// "cooldown_left_down", "root_down" and "link_down" instead. Its data gives where it holds Samus for each way under "held_offsets". How fast it
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

    /// <summary>How many updates after letting go, or drawing back, it refuses to catch Samus; it may catch her from the second update after them.</summary>
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
    private const int QuarterTurn = 64, HalfTurn = 128;

    /// <summary>The steps of curl for each step a link bends by.</summary>
    private const int CurlPerBendStep = 4;

    /// <summary>The distance on one axis, in pixels, from which it measures by that axis alone.</summary>
    private const int ShortProductReach = 255;

    private static readonly string[] Ways = ["up", "up_right", "right", "down_right", "down", "down_left", "left", "up_left"];

    private enum Doing
    {
        Watching,
        BeginStrike,
        Striking,
        DrawnBack,
    }

    private readonly Dictionary<string, PixelOffset> _heldOffsets = new(StringComparer.Ordinal);
    private readonly PixelOffset[] _linkAt = new PixelOffset[Links];

    /// <summary>
    /// What holds each link's place among the room's places for what enemies launch: on the cartridge each link of
    /// its neck is a shot of its own. Null for a link the room had no place for, which shows nothing.
    /// </summary>
    private readonly Effect?[] _linkPlaces = new Effect?[Links];
    private SpeedCurve? _curve;
    private Doing _doing;
    private PixelPosition _origin;
    private int _reach;

    // As the cartridge counts it: none right, a quarter turn up.
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
        _link = new TimelinePlayer(Timelines.Get(Oriented("link")));
        _root = new TimelinePlayer(Timelines.Get(Oriented("root")));

        // Its links take their places as it is set up, the one at its mouth first.
        for (int link = Links - 1; link >= 0; link--)
        {
            Effect place = Effect.HoldingPlace(Position);
            _linkPlaces[link] = Room.Spawn(place) ? place : null;
        }

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
        // As every enemy's touch on the cartridge, it meets a Samus still safe from her last hurt only while her body is
        // a weapon, and that meeting ends her safety.
        Engine.Actors.Player samus = Room.Player;
        bool bodyIsWeapon = samus.BodyPowers.Any(power => power.IsActive);
        if (samus.IsInvincible && !bodyIsWeapon)
            return;
        if (bodyIsWeapon)
            samus.BeInvincibleFor(0);

        // It can catch her only while its cooldown reads below nothing; having caught her, not again until the strike is over.
        if (_cooldown >= 0 || _holds || IsDead)
            return;
        _cooldown = 0;
        _holds = true;

        // The cartridge's Maw only stops her taking part: what she was doing, and the speed she had, are hers again
        // as it lets her go. Out of a spin or a wall jump it stands her up first, body and all.
        if (samus.State is SpinJumpingState or WallJumpingState)
        {
            samus.Hold();
            samus.TrySetForm(Engine.Actors.Player.Standing);
        }
        else
        {
            samus.HoldAsTheyAre();
        }
    }

    /// <inheritdoc/>
    protected override void OnStatusBegan(string status)
    {
        if (status == Frozen)
            LetGo();
    }

    /// <inheritdoc/>
    protected override void OnDied()
    {
        LetGo();

        // Its links go with it, and their places are free again.
        for (int link = 0; link < Links; link++)
        {
            if (_linkPlaces[link] is { } place)
                Room.Remove(place);
            _linkPlaces[link] = null;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // The cooldown counts down every update with no floor, within 65536: past nothing it lets it catch Samus for
        // 32768 updates, then reads as above nothing again and refuses for 32768, and so on until it starts again.
        _cooldown = unchecked((short)(_cooldown - 1));
        switch (_doing)
        {
            case Doing.Watching:
                (int distance, int bearing) = Measure();
                if (distance < SafetyDistance)
                {
                    _cooldown = GrabCooldown;
                    break;
                }

                if (distance >= ActivationDistance)
                    break;
                _reach = Math.Min(distance, LongestReach);
                _bearing = bearing;
                _doing = Doing.BeginStrike;
                break;

            case Doing.BeginStrike:
                _stretch = Pixels.Zero;
                _place = 0;
                _drawingBack = false;
                // The way it strikes is the nearest of eight to its bearing, counted round clockwise from straight up.
                _way = Ways[Turns.Wrap(QuarterTurn - _bearing + Turns.PerTurn / 16) * Ways.Length / Turns.PerTurn];
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
                if (_drawnBack.IsRunning || Room.Player.IsDying)
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
        Bend();
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
        Play(Oriented($"cooldown_{side}"), restart: true);
        // Only one that strikes downward and bit straight across brings Samus along on the update it draws back; any
        // other leaves her where it last held her until its next update.
        if (_holds && StrikesDown && side != "straight")
            HoldSamus();
    }

    /// <summary>
    /// How far Samus is, and the bearing it would strike along, as the cartridge measures them. From 255 pixels away on
    /// either axis it measures by that axis alone, and its bearing is straight up; nearer, the distance is the whole
    /// pixels of two short products along its aim at her offsets folded onto the lower right, each axis's offset the
    /// radius of its own, and the bearing its aim at her.
    /// </summary>
    private (int Distance, int Bearing) Measure()
    {
        int across = Target.X.Whole - _origin.X.Whole, down = Target.Y.Whole - _origin.Y.Whole;
        int farAcross = Math.Abs(across), farDown = Math.Abs(down);
        if (farAcross >= ShortProductReach)
            return (farAcross, QuarterTurn);
        if (farDown >= ShortProductReach)
            return (farDown, QuarterTurn);
        int folded = CartridgeAngles.AimAngle(farAcross, farDown);
        int distance = Math.Abs(CartridgeAngles.CircleSine(folded - QuarterTurn, farAcross).Whole)
            + Math.Abs(CartridgeAngles.CircleSine(folded, farDown).Whole);
        return (distance, CartridgeAngles.AimAngle(across, down));
    }

    /// <summary>
    /// Bends its four links for how far it has curled, as the cartridge does. Each link turns from a half turn by a
    /// quarter of the curl's whole steps for each link out, the mirrored way when its bearing lies in the turn's second
    /// half, on a circle whose radius across is half its reach and down a quarter, less where an uncurled link would lie.
    /// The bearing then turns a correction whose length is the link's offset across, for both of its parts. The eight
    /// offsets, link by link and across before down, take their corrections in one chain within 65536, each passing the
    /// end moving the next one a pixel further.
    /// </summary>
    private void Bend()
    {
        int curl = _stretch.Whole;
        if (curl < 0 || curl > FullStretch)
            throw new InvalidOperationException($"The Yapping Maw \"{Name}\" bends with a curl of {curl}, outside 0 through {FullStretch}.");
        int bendStep = curl / CurlPerBendStep;
        int radius = _reach / 2;
        int baselineAcross = AcrossOf(HalfTurn, radius), baselineDown = DownOf(HalfTurn, radius / 2);
        bool mirrored = _bearing >= HalfTurn;
        bool passedEnd = false;
        for (int link = 0; link < Links; link++)
        {
            int multiple = bendStep * (link + 1);
            int linkAngle = mirrored ? HalfTurn + multiple : HalfTurn - multiple;
            int across = Held(AcrossOf(linkAngle, radius) - baselineAcross);
            int down = Held(DownOf(linkAngle, radius / 2) - baselineDown);
            int correctionAcross = Held(AcrossOf(_bearing, across) - AcrossOf(0, across));
            int correctionDown = Held(DownOf(_bearing, across) - DownOf(0, across));
            across = Chained(across, correctionAcross, ref passedEnd);
            down = Chained(down, correctionDown, ref passedEnd);
            _linkAt[link] = PixelOffset.FromWhole(unchecked((short)across), unchecked((short)down));
        }
    }

    /// <summary>An offset as the cartridge holds it, within 65536: one backward as its complement.</summary>
    private static int Held(int offset) => unchecked((ushort)offset);

    /// <summary>An offset held within 65536 moved by a correction, and a pixel more if the one before passed the end; notes whether this one does.</summary>
    private static int Chained(int offset, int correction, ref bool passedEnd)
    {
        int moved = offset + correction + (passedEnd ? 1 : 0);
        passedEnd = moved >= 65536;
        return Held(moved);
    }

    /// <summary>A link's offset across for a turn and a length: its offset down a quarter turn behind.</summary>
    private static int AcrossOf(int angle, int length) => DownOf(angle - QuarterTurn, length);

    /// <summary>
    /// A link's offset down for a turn and a length, as the cartridge reckons it: the fine sine read coarsely, reversed,
    /// along the length counted within 256, in whole pairs of pixels rounded down.
    /// </summary>
    private static int DownOf(int angle, int length)
    {
        int wrapped = Turns.Wrap(angle);
        int share = CartridgeAngles.CoarseSizeOfFineSine(wrapped) * 2;
        int reversed = wrapped >= HalfTurn ? share : -share;
        return reversed * (length % 256) >> 9 << 1;
    }

    /// <summary>Where the <paramref name="link"/>th of its links is, the last being its mouth: the root, then each link's far end as last bent.</summary>
    private PixelPosition Along(int link) => link == 0 ? _origin : _origin + _linkAt[link - 1];

    /// <summary>Keeps Samus in its mouth: at the whole pixel its hold puts her, with the part of a pixel she had.</summary>
    private void HoldSamus()
    {
        PixelPosition at = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole) + _heldAt;
        Engine.Actors.Player samus = Room.Player;
        samus.Position = new PixelPosition(samus.Position.X.WithWhole(at.X.Whole), samus.Position.Y.WithWhole(at.Y.Whole));

        // Each placing drops a grapple she has out, and holds the view to following her by twelve pixels and one more.
        (samus as Samus)?.Grapple?.Release();
        Room.Session.Camera.CapFollowUpOrDown(samus.Position.Y, Samus.PlacedViewCap);
        Room.Session.Camera.CapFollowAcross(samus.Position.X, Samus.PlacedViewCap);
    }

    /// <summary>Lets Samus go, if it holds her.</summary>
    private void LetGo()
    {
        if (!_holds)
            return;
        _holds = false;
        if (!Room.Player.IsDying)
            Room.Player.Release();
    }

    /// <summary>Where it holds Samus when it has struck the given way.</summary>
    private PixelOffset HeldOffset(string way) =>
        _heldOffsets.TryGetValue(way, out PixelOffset offset) ? offset : throw new InvalidOperationException($"The Yapping Maw \"{Name}\" has no held offset for \"{way}\".");

    /// <summary>
    /// The name of a timeline that differs by which way it strikes: one that strikes downward has its own, named
    /// with "_down" after.
    /// </summary>
    private string Oriented(string timeline) => StrikesDown ? timeline + "_down" : timeline;

    /// <summary>Puts its root and the links of its neck where they are.</summary>
    private void ShowNeck()
    {
        _link?.Step();
        _root?.Step();
        if (_root is not null && GetNodeOrNull<ActorVisual>("Root") is { } root)
        {
            root.Position = new Vector2(_origin.X.Whole - Position.X.Whole, _origin.Y.Whole + (StrikesDown ? -RootDistance : RootDistance) - Position.Y.Whole);
            root.Show(_root.Shown);
        }

        for (int link = 0; link < Links; link++)
        {
            if (_link is null || GetNodeOrNull<ActorVisual>($"Link{link + 1}") is not { } visual)
                continue;
            // The links stay where they were last bent, as the mouth does.
            PixelPosition at = Along(link);
            visual.Position = new Vector2(at.X.Whole - Position.X.Whole, at.Y.Whole - Position.Y.Whole);
            visual.Show(_link.Shown);
            visual.SetShown(_linkPlaces[link] is not null);
        }
    }
}
