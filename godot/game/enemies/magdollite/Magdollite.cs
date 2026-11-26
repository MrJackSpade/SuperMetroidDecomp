using System.Globalization;
using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Magdollite: a creature of lava. Its head watches Samus from the surface; when she comes within range it sinks,
/// and a pillar of lava with a hand on top rises as high as she stands and flings blobs of lava at her, then sinks
/// back, and the head comes up again.
/// </summary>
/// <remarks>
/// The head plays "idle_left", "idle_right", "submerge_left", "submerge_right", "emerge_left" and "emerge_right",
/// whose cues are "move:&lt;down&gt;", which moves the head, "pillar:&lt;down&gt;", which moves head and pillar,
/// "sink_and_show", "submerged", "restore", "surface_and_hide" and "surfaced". The visual named "Pillar" plays
/// "pillar_1" through "pillar_8". The visual named "Hand" plays "cap", "throw_left" and "throw_right", whose cues are
/// "throw_sound", "cooldown", "shift:&lt;across&gt;:&lt;down&gt;", "lava" and "thrown". Its shot is "lava". Its one
/// hurtbox grows to cover the whole of it, head to hand. It glows: "glow" lists the changes of its colours its glow
/// goes round through (<see cref="ColourChange"/>), each shown for "glow_updates", and "lava_glow" the same steps for
/// the lava it has thrown, which shows its thrower's glow while it flies. The list "grows_at_full_height" says, for
/// each place in a room's list of enemies, whether a pillar listed there grows one more length on reaching its full
/// height.
/// </remarks>
public partial class Magdollite : Enemy
{
    /// <summary>How fast its pillar rises and falls, as a number of pixels an update.</summary>
    [Export]
    public double Speed { get; set; } = 1;

    /// <summary>How near across Samus must be, in pixels, for it to attack.</summary>
    [Export]
    public int Range { get; set; } = 96;

    /// <summary>
    /// Where its pillar came in the original's list of the room's enemies, counting from nought. The original decides
    /// by this, and nothing else, whether a pillar that reaches its full height grows one last length there.
    /// </summary>
    [Export]
    public int ListedAt { get; set; }

    /// <summary>How far below the surface its pillar and hand wait, in pixels.</summary>
    [Export, DataValue("hidden_depth")]
    public int HiddenDepth { get; set; } = 32;

    /// <summary>How far below the surface its head sinks, in pixels.</summary>
    [Export, DataValue("sunk_depth")]
    public int SunkDepth { get; set; } = 24;

    /// <summary>How far below the surface its head first shows again, in pixels.</summary>
    [Export, DataValue("surfaced_depth")]
    public int SurfacedDepth { get; set; } = 4;

    /// <summary>The most its pillar rises, in pixels.</summary>
    [Export, DataValue("maximum_rise")]
    public int MaximumRise { get; set; } = 108;

    /// <summary>How tall one length of the pillar is, in pixels: it grows a length for every two of these it rises.</summary>
    [Export, DataValue("pillar_length")]
    public int PillarLength { get; set; } = 8;

    /// <summary>The most lengths its pillar grows to.</summary>
    [Export, DataValue("most_lengths")]
    public int MostLengths { get; set; } = 8;

    /// <summary>How many updates pass after a throw before it can attack again.</summary>
    [Export, DataValue("attack_cooldown")]
    public int AttackCooldown { get; set; } = 257;

    /// <summary>The least its hurtbox reaches above and below its head, in pixels.</summary>
    [Export, DataValue("smallest_reach")]
    public int SmallestReach { get; set; } = 8;

    /// <summary>How far past its hand its hurtbox reaches, in pixels.</summary>
    [Export, DataValue("reach_past_hand")]
    public int ReachPastHand { get; set; } = 2;

    /// <summary>How far below its hand the lava it throws starts, in pixels.</summary>
    [Export, DataValue("lava_drop")]
    public int LavaDrop { get; set; } = 2;

    /// <summary>How many updates pass between its looks for Samus while it waits.</summary>
    [Export, DataValue("attack_poll_updates")]
    public int AttackPollUpdates { get; set; } = 1;

    /// <summary>How many updates each step of its glow lasts.</summary>
    [Export, DataValue("glow_updates")]
    public int GlowUpdates { get; set; } = 8;

    /// <summary>The sound of it throwing, by name, heard only while its hand is in view.</summary>
    [Export, DataValue("throw_sound")]
    public string ThrowSound { get; set; } = "";

    private enum HeadDoing
    {
        Waiting,
        Submerging,
        Submerged,
        WaitingForArm,
        ArmLanded,
        Emerging,
        Surfaced,
    }

    private enum PillarDoing
    {
        Dormant,
        Released,
        Rising,
        AtApex,
        Falling,
    }

    private enum HandDoing
    {
        WaitingForAttack,
        Throwing,
        Thrown,
        RisingWithPillar,
        FallingWithPillar,
    }

    private HeadDoing _head;
    private PillarDoing _pillar;
    private HandDoing _hand;
    private int _originY;
    private Pixels _pillarY;
    private int _risen;
    private int _lengths;
    private (int X, int Y) _handAt;
    private (int X, int Y) _throwFrom;
    private Facing _throws;
    private Countdown _cooldown;
    private bool _partsShown;
    private TimelinePlayer? _pillarPlayer;
    private TimelinePlayer? _handPlayer;
    private bool _growsAtFullHeight;
    private readonly List<ColourChange> _glow = [];
    private readonly List<ColourChange> _lavaGlow = [];
    private readonly List<Projectile> _lava = [];
    private int _glowShown;

    /// <summary>The name its glow's colours are shown under.</summary>
    private const string GlowColours = "Glow";

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        IReadOnlyList<DataNode> byPlace = data.List("grows_at_full_height");
        if (ListedAt < 0 || ListedAt >= byPlace.Count)
            throw data.Problem($"says nothing of a pillar listed at {ListedAt}");
        _growsAtFullHeight = byPlace[ListedAt].AsFlag();
        _glow.Clear();
        _glow.AddRange(data.List("glow").Select(ColourChange.Read));
        _lavaGlow.Clear();
        _lavaGlow.AddRange(data.List("lava_glow").Select(ColourChange.Read));
        if (_glow.Count == 0 || _lavaGlow.Count != _glow.Count)
            throw data.Problem($"gives its glow {_glow.Count} steps and its lava's {_lavaGlow.Count}, where both go round through the same steps");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _originY = Position.Y.Whole;
        _pillarY = Pixels.FromWhole(_originY + HiddenDepth);
        _handAt = (Position.X.Whole, _originY + HiddenDepth);
        _cooldown = Countdown.Of(AttackPollUpdates);
        Play(TowardPlayer == Facing.Left ? "idle_left" : "idle_right");
        _pillarPlayer = new TimelinePlayer(Timelines.Get("pillar_1"));
        _handPlayer = new TimelinePlayer(Timelines.Get("cap"));
        ShowParts();
        // Its glow goes round from the moment its room is set up, and shows its first step at once.
        _lava.Clear();
        ShowGlow(0);
    }

    /// <summary>
    /// Moves its glow on: as the cartridge's glow steps once the enemies have had their update, each step shows from
    /// the update its time comes, counted from the room's setup.
    /// </summary>
    private void StepGlow()
    {
        int step = (Updates + 1) / GlowUpdates % _glow.Count;
        if (step != _glowShown)
            ShowGlow(step);
    }

    /// <summary>Shows a step of its glow on all of it and on the lava it has thrown that still flies.</summary>
    private void ShowGlow(int step)
    {
        _glowShown = step;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            _glow[step].ShowOn(visual, GlowColours);
        _lava.RemoveAll(lava => !IsInstanceValid(lava) || lava.HasEnded);
        foreach (Projectile lava in _lava)
            _lavaGlow[step].ShowOn(lava, GlowColours);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "move":
                Position += PixelOffset.FromWhole(0, Number(parts, 1));
                break;
            case "pillar":
                Position += PixelOffset.FromWhole(0, Number(parts, 1));
                _pillarY += Pixels.FromWhole(Number(parts, 1));
                break;
            case "sink_and_show":
                SetHeadAndPillar(_originY + SunkDepth);
                _partsShown = true;
                break;
            case "submerged":
                _head = HeadDoing.Submerged;
                break;
            case "restore":
                SetHeadAndPillar(_originY);
                break;
            case "surface_and_hide":
                SetHeadAndPillar(_originY + SurfacedDepth);
                _partsShown = false;
                break;
            case "surfaced":
                _head = HeadDoing.Surfaced;
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        string side = TowardPlayer == Facing.Left ? "left" : "right";
        switch (_head)
        {
            case HeadDoing.Waiting:
                Play($"idle_{side}");
                if (_cooldown.IsRunning)
                    break;
                _cooldown = Countdown.Of(AttackPollUpdates);
                if (Math.Abs(Position.X.Whole - Target.X.Whole) >= Range)
                    break;
                Play($"submerge_{side}", restart: true);
                _head = HeadDoing.Submerging;
                break;
            case HeadDoing.Submerged:
                _pillar = PillarDoing.Released;
                _head = HeadDoing.WaitingForArm;
                break;
            case HeadDoing.ArmLanded:
                Play($"emerge_{side}", restart: true);
                _head = HeadDoing.Emerging;
                break;
            case HeadDoing.Surfaced:
                Play($"idle_{side}");
                _head = HeadDoing.Waiting;
                break;
        }

        StepPillar();
        StepHand();
        ShowParts();
        StepGlow();
    }

    /// <summary>While a hurt holds its behaviour off, its glow still goes round.</summary>
    protected override void WhileHurt() => StepGlow();

    /// <summary>The whole number a cue carries after its name.</summary>
    private static int Number(string[] parts, int at) => int.Parse(parts[at], CultureInfo.InvariantCulture);

    /// <summary>How far above the pillar's foot the hand sits when the pillar is <paramref name="lengths"/> long.</summary>
    private int HandAbove(int lengths) => PillarLength / 2 + PillarLength * Math.Max(1, lengths);

    /// <summary>Puts the head and the pillar's foot at one height.</summary>
    private void SetHeadAndPillar(int y)
    {
        Position = new PixelPosition(Position.X, Position.Y.WithWhole(y));
        _pillarY = _pillarY.WithWhole(y);
    }

    /// <summary>Moves the pillar on by one update: it rises to Samus's height, growing as it goes, and falls back shrinking.</summary>
    private void StepPillar()
    {
        Pixels speed = Pixels.FromNumber(Speed);
        switch (_pillar)
        {
            case PillarDoing.Released:
                _pillar = PillarDoing.Rising;
                _risen = 0;
                _lengths = 1;
                _pillarY = _pillarY.WithWhole(_originY);
                break;

            case PillarDoing.Rising:
                int before = _pillarY.Whole;
                _pillarY -= speed;
                _risen += before - _pillarY.Whole;
                bool atMost = _risen >= MaximumRise;
                bool pastSamus = _pillarY.Whole - HandAbove(_lengths) < Target.Y.Whole;
                // At its full height the original asks something else: an answer that goes by where the pillar is listed.
                bool grows = _lengths < MostLengths && (atMost ? _growsAtFullHeight : _risen >= _lengths * PillarLength * 2);
                if (atMost || pastSamus)
                    _pillar = PillarDoing.AtApex;
                if (!grows)
                    break;
                _lengths++;
                _pillarY += Pixels.FromWhole(PillarLength);
                break;

            case PillarDoing.Falling:
                int from = _pillarY.Whole;
                _pillarY += speed;
                _risen -= _pillarY.Whole - from;
                if (_risen <= 0)
                {
                    _pillar = PillarDoing.Dormant;
                    _head = HeadDoing.ArmLanded;
                    break;
                }

                if (_lengths <= 1 || _risen >= (_lengths - 1) * PillarLength * 2)
                    break;
                _lengths--;
                _pillarY -= Pixels.FromWhole(PillarLength);
                break;
        }
    }

    /// <summary>Moves the hand on by one update: it rides the top of the pillar, and throws when the pillar is up.</summary>
    private void StepHand()
    {
        _cooldown = _cooldown.Stepped();
        switch (_hand)
        {
            case HandDoing.WaitingForAttack:
                if (_head is HeadDoing.Submerging or HeadDoing.Submerged)
                    _hand = HandDoing.RisingWithPillar;
                return;

            case HandDoing.RisingWithPillar:
                if (_pillar == PillarDoing.AtApex)
                {
                    _throws = TowardPlayer;
                    _handPlayer = new TimelinePlayer(Timelines.Get(_throws == Facing.Right ? "throw_right" : "throw_left"));
                    _hand = HandDoing.Throwing;
                    _throwFrom = _handAt;
                }
                else
                {
                    _handAt = (_handAt.X, _pillarY.Whole - HandAbove(_lengths));
                }

                return;

            case HandDoing.Thrown:
                _handPlayer = new TimelinePlayer(Timelines.Get("cap"));
                _pillar = PillarDoing.Falling;
                _hand = HandDoing.FallingWithPillar;
                _handAt = _throwFrom;
                return;

            case HandDoing.FallingWithPillar:
                if (_head == HeadDoing.Waiting)
                    _hand = HandDoing.WaitingForAttack;
                else
                    _handAt = (_handAt.X, _pillarY.Whole - HandAbove(_lengths));
                return;
        }
    }

    /// <summary>Acts on one cue of the hand's timeline.</summary>
    private void OnHandCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "throw_sound":
                // Heard by the old game's enemies' view: a square as wide as the screen from its corner, far edges
                // included, so reaching further below the screen than the camera's view does.
                PixelRect view = Room.Session.Camera.View;
                int across = _handAt.X - view.Left, down = _handAt.Y - view.Top;
                if (across >= 0 && across <= view.Width && down >= 0 && down <= view.Width)
                    Sound(ThrowSound);
                break;
            case "cooldown":
                _cooldown = Countdown.Of(AttackCooldown);
                break;
            case "shift":
                _handAt = (_throwFrom.X + Number(parts, 1), _throwFrom.Y + Number(parts, 2));
                break;
            case "lava":
                Projectile lava = Fire("lava", PixelPosition.FromWhole(_handAt.X, _handAt.Y + LavaDrop), new PixelVelocity(_throws.Toward(Shot("lava").Speed), Pixels.Zero));
                // Lava with no place free in the room is not thrown. What is thrown is in its thrower's glow from the
                // start, and goes round with it while it flies.
                if (lava.GetParent() != Room)
                    break;
                _lavaGlow[_glowShown].ShowOn(lava, GlowColours);
                _lava.Add(lava);
                break;
            case "thrown":
                _hand = HandDoing.Thrown;
                break;
        }
    }

    /// <summary>Moves the pillar's and hand's pictures on, puts them where they are, and sizes the hurtbox to cover head to hand.</summary>
    private void ShowParts()
    {
        if (_pillarPlayer is not null && GetNodeOrNull<ActorVisual>("Pillar") is { } pillar)
        {
            FrameTimeline lengths = Timelines.Get($"pillar_{Math.Max(1, _lengths)}");
            if (_pillarPlayer.Timeline != lengths)
                _pillarPlayer = new TimelinePlayer(lengths);
            else
                _pillarPlayer.Step();
            pillar.SetShown(_partsShown);
            pillar.Position = new Vector2(0, _pillarY.Whole - Position.Y.Whole);
            pillar.Show(_pillarPlayer.Shown.Picture);
        }

        if (_handPlayer is not null)
        {
            foreach (string cue in _handPlayer.Step())
                OnHandCue(cue);
            if (GetNodeOrNull<ActorVisual>("Hand") is { } hand)
            {
                hand.SetShown(_partsShown);
                hand.Position = new Vector2(_handAt.X - Position.X.Whole, _handAt.Y - Position.Y.Whole);
                hand.Show(_handPlayer.Shown.Picture);
            }
        }

        if (EntityNode.ColliderOf(this, ColliderRole.Hurtbox) is not { } hurtbox)
            return;
        int reach = Math.Max(Position.Y.Whole - _handAt.Y + ReachPastHand, SmallestReach);
        hurtbox.Rect = new PixelRect(-hurtbox.Size.X / 2, -reach, hurtbox.Size.X, reach * 2);
    }
}
