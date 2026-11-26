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
/// Shaktool: a digging machine, a chain of seven parts with a saw at each end and its head in the middle. One saw
/// stays put while the rest of the chain curls round it and straightens out ahead; when the far saw meets a wall the
/// chain pauses, each part nodding in its turn, and either turns end for end or curls back the other way. Its saws
/// cut through what they are made to cut, and so it digs its way along. Every part can be shot and has health of its
/// own; the first part destroyed takes the whole machine with it.
/// </summary>
/// <remarks>
/// <code>
/// "parts": [ { "angle": 0, "turn": 0, "look": "saw", "bob": "saw_bob" }, ... ],
/// "orbit": [ 0, 0.29, ... ]
/// </code>
/// "parts" lists its seven parts in order: the angle each starts at about the part before it and how fast it turns,
/// both in 256ths of an angle step, the timeline it shows, and the timeline it plays when the chain stops at a wall.
/// A part's timeline may hold steps with no picture, which leave the part as it was shown; its cues are "lower" and
/// "raise", which nudge the part a pixel away from the way the chain will go and back, and "resume", which sets the
/// chain moving again. "orbit" is how far across a part sits from the one before it at each of the 256 angle steps,
/// straight up at nothing and turning clockwise; how far down is the same a quarter turn behind. Its head plays
/// "head_up", "head_up_right" and so on round the eight ways it can point. The visuals named "Part1" through "Part7"
/// are its parts in order, the hurtboxes "Hurtbox1" through "Hurtbox7" what a shot lands on, and the hitboxes "Saw1"
/// and "Saw2" its saws.
/// </remarks>
public partial class Shaktool : Enemy
{
    /// <summary>Whether its chain curls clockwise to start with.</summary>
    [Export]
    public bool StartsClockwise { get; set; } = true;

    /// <summary>Half the size of a part, in pixels, for being shot, for meeting walls and for cutting.</summary>
    [Export, DataValue("part_half_size")]
    public int PartHalfSize { get; set; } = 6;

    /// <summary>How far round, in angle steps, the chain has curled when it starts to straighten.</summary>
    [Export, DataValue("straighten_at")]
    public int StraightenAt { get; set; } = 240;

    /// <summary>The count its curl starts again from once every part points the same way.</summary>
    [Export, DataValue("straighten_restart")]
    public int StraightenRestart { get; set; } = 120;

    /// <summary>How fast every part turns while the chain straightens, in 256ths of an angle step an update.</summary>
    [Export, DataValue("straightening_turn")]
    public int StraighteningTurn { get; set; } = 256;

    /// <summary>After a wall, each part turns each update by its angle short of the chain's new aim divided by this.</summary>
    [Export, DataValue("convergence_shares")]
    public int ConvergenceShares { get; set; } = 64;

    private const int Parts = 7;
    private const int Head = Parts / 2;
    private const int StepParts = 256;
    private const int TurnParts = Turns.PerTurn * StepParts;
    private static readonly string[] Ways = ["up", "up_right", "right", "down_right", "down", "down_left", "left", "up_left"];

    private readonly int[] _startAngle = new int[Parts];
    private readonly int[] _startTurn = new int[Parts];
    private readonly string[] _look = new string[Parts];
    private readonly string[] _bob = new string[Parts];
    private readonly Pixels[] _orbit = new Pixels[Turns.PerTurn];
    private readonly int[] _angle = new int[Parts];
    private readonly int[] _turn = new int[Parts];
    private readonly PixelPosition[] _at = new PixelPosition[Parts];
    private readonly TimelinePlayer?[] _players = new TimelinePlayer?[Parts];
    private readonly Health[] _health = new Health[Parts];
    private bool _paused;
    private bool _straightening;
    private bool _turnsEndForEndNext = true;
    private bool _clockwise;
    private bool _headReversed;
    private int _target;
    private int _curled;
    private string _headPicture = "";

    /// <summary>The part the latest hit landed on.</summary>
    private int _struck;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        IReadOnlyList<DataNode> parts = data.List("parts");
        if (parts.Count != Parts)
            throw data.Problem($"lists {parts.Count} parts where Shaktool has {Parts}");
        for (int part = 0; part < Parts; part++)
        {
            _startAngle[part] = parts[part].Int("angle");
            _startTurn[part] = parts[part].Int("turn");
            _look[part] = parts[part].Text("look");
            _bob[part] = parts[part].Text("bob");
        }

        IReadOnlyList<DataNode> orbit = data.List("orbit");
        if (orbit.Count != Turns.PerTurn)
            throw data.Problem($"gives its orbit at {orbit.Count} angle steps where a turn has {Turns.PerTurn}");
        for (int step = 0; step < Turns.PerTurn; step++)
            _orbit[step] = orbit[step].AsPixels();
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _clockwise = StartsClockwise;
        _at[0] = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        for (int part = 0; part < Parts; part++)
        {
            _angle[part] = _startAngle[part];
            _turn[part] = _startTurn[part];
            _health[part] = new Health(MaxHealth);
            if (part > 0)
                _at[part] = _at[part - 1] + Reach(_angle[part]);
            if (part != Head)
                _players[part] = new TimelinePlayer(Timelines.Get(_look[part]));
        }

        AlwaysActive = true;
        // Its saws and head stand in front of Samus, its arms just behind her.
        Standing = Standing.InFront;
        for (int part = 1; part < Parts - 1; part++)
        {
            if (part != Head && GetNodeOrNull<ActorVisual>($"Part{part + 1}") is { } arm)
                RoomDepth.Stand(arm, Standing.JustBehind);
        }

        Play(_look[Head]);
        ShowParts();
    }

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        _struck = PartOf(part);
        return base.TakeHit(hit, part);
    }

    /// <inheritdoc/>
    protected override Health HealthOf(Collider part) => _health[PartOf(part)];

    /// <summary>
    /// The machine dies where the part that was struck lies, as the cartridge's struck part died where it was and
    /// the rest were taken away: its death and what it leaves are there, not at its head.
    /// </summary>
    protected override bool DiesAtOnce(Hit hit)
    {
        Position = _at[_struck];
        return true;
    }

    /// <summary>Which of its parts, counted from nought, <paramref name="hurtbox"/> is.</summary>
    private int PartOf(Collider hurtbox)
    {
        for (int index = 0; index < Parts; index++)
        {
            if (hurtbox.Name == $"Hurtbox{index + 1}")
                return index;
        }

        throw new InvalidOperationException($"A hit landed on \"{hurtbox.Name}\" of Shaktool \"{Name}\", which is none of its parts.");
    }

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step)
    {
        // A step with no picture leaves the head as it was shown.
        if (step.Picture.Length > 0)
            _headPicture = step.Picture;
        return _headPicture;
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue) => OnPartCue(Head, cue);

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!_paused)
        {
            for (int part = 1; part < Parts - 1; part++)
                Orbit(part);
            // The head points midway between the way it hangs from the chain and the way the chain goes on from it.
            int back = Wrapped(_angle[Head] + TurnParts / 2);
            int midway = Wrapped(back + Wrapped(_angle[Head + 1] - back) / 2 + (_headReversed ? TurnParts / 2 : 0));
            Play($"head_{Ways[Turns.Wrap(midway / StepParts + 8) * Ways.Length / Turns.PerTurn]}");
            DriveTail();
        }

        Hit cut = new(1, HitSide.Enemies, new HitKind(HitKinds.Saw)) { Source = this };
        Room.Terrain.TakeHit(cut, PartRect(_at[0]));
        Room.Terrain.TakeHit(cut, PartRect(_at[Parts - 1]));
        ShowParts();
    }

    /// <summary>An angle in 256ths of a step brought into one turn.</summary>
    private static int Wrapped(int angle) => ((angle % TurnParts) + TurnParts) % TurnParts;

    /// <summary>How far a part sits from the one before it at an angle: straight up at nothing, turning clockwise.</summary>
    private PixelOffset Reach(int angle)
    {
        int steps = angle / StepParts;
        return new PixelOffset(_orbit[Turns.Wrap(steps)], _orbit[Turns.Wrap(steps - Turns.PerTurn / 4)]);
    }

    /// <summary>The room a part takes up when it is at <paramref name="at"/>.</summary>
    private PixelRect PartRect(PixelPosition at) =>
        new(at.X.Whole - PartHalfSize, at.Y.Whole - PartHalfSize, PartHalfSize * 2, PartHalfSize * 2);

    /// <summary>Puts a part where its angle has it about the part before, and turns its angle on.</summary>
    private void Orbit(int part)
    {
        _at[part] = _at[part - 1] + Reach(_angle[part]);
        int by = _straightening ? StraighteningTurn : _turn[part];
        _angle[part] = Wrapped(_angle[part] + (_clockwise ? by : -by));
    }

    /// <summary>Moves the far saw on; a wall in its way pauses the chain and turns it about.</summary>
    private void DriveTail()
    {
        const int tail = Parts - 1;
        PixelPosition before = PixelPosition.FromWhole(_at[tail].X.Whole, _at[tail].Y.Whole);
        Orbit(tail);
        PixelPosition wanted = PixelPosition.FromWhole(_at[tail].X.Whole, _at[tail].Y.Whole);
        // As the old game does, it moves across and then down through the terrain, each stopping against a wall.
        PixelPosition stopped = before;
        if (MovePart(ref stopped, wanted.X.Whole - before.X.Whole, 0, SlopeTreatment.Ignore)
            || MovePart(ref stopped, 0, wanted.Y.Whole - before.Y.Whole, SlopeTreatment.Wall))
        {
            // Turning end for end puts it back where it was; turning its orbit about leaves it where the wall stopped it.
            _at[tail] = _turnsEndForEndNext ? before : stopped;
            MeetWall();
            return;
        }

        _at[tail] = wanted;
        if (_straightening)
            return;
        if (_target / StepParts == _angle[tail] / StepParts)
        {
            // Every part now points the way the chain was aiming: it turns as one, and the count of its curl starts over.
            for (int part = 0; part < Parts; part++)
            {
                _angle[part] = _target;
                _turn[part] = _startTurn[part];
            }

            _curled = StraightenRestart * StepParts;
            _turnsEndForEndNext = true;
        }

        _curled += _turn[tail];
        if (_curled >= StraightenAt * StepParts)
            _straightening = true;
    }

    /// <summary>
    /// Moves a part at <paramref name="at"/> a pixel at a time, stopping short of terrain. Returns whether terrain
    /// stopped it.
    /// </summary>
    private bool MovePart(ref PixelPosition at, int across, int down, SlopeTreatment slopes)
    {
        for (int moved = 0; moved < Math.Abs(across) + Math.Abs(down); moved++)
        {
            PixelPosition next = at + PixelOffset.FromWhole(Math.Sign(across), Math.Sign(down));
            if (Room.Terrain.IsSolidIn(PartRect(next), slopes))
                return true;
            at = next;
        }

        return false;
    }

    /// <summary>Answers the far saw meeting a wall: the chain turns end for end, or the next time curls back the other way.</summary>
    private void MeetWall()
    {
        if (_turnsEndForEndNext)
        {
            TurnEndForEnd();
            _turnsEndForEndNext = false;
        }
        else
        {
            _clockwise = !_clockwise;
            _turnsEndForEndNext = true;
        }

        _straightening = false;
        // The chain now aims a quarter turn on from where its far saw lies from the near one: the cartridge's aim
        // between their whole pixels, which counts up from right, turned to count clockwise from up.
        int aim = CartridgeAngles.AimAngle(_at[Parts - 1].X.Whole - _at[0].X.Whole, _at[Parts - 1].Y.Whole - _at[0].Y.Whole);
        int bearing = Turns.Wrap(Turns.PerTurn / 4 - aim) * StepParts;
        _target = Wrapped(bearing + (_clockwise ? TurnParts / 4 : -TurnParts / 4));
        for (int part = Parts - 1; part >= 0; part--)
        {
            int left = Wrapped(_clockwise ? _target - _angle[part] : _angle[part] - _target);
            _turn[part] = left / StepParts * StepParts / ConvergenceShares;
            // Each part stops and nods.
            if (part == Head)
                Play(_bob[part], restart: true);
            else
                _players[part] = new TimelinePlayer(Timelines.Get(_bob[part]));
        }

        _paused = true;
    }

    /// <summary>Turns the chain end for end: the saws change places, and each part takes the mirrored angle of its opposite number.</summary>
    private void TurnEndForEnd()
    {
        _headReversed = !_headReversed;
        for (int right = 1; right <= Head; right++)
        {
            int left = Parts - right;
            int saved = _angle[left];
            _angle[left] = Opposite(_angle[right]);
            _angle[right] = Opposite(saved);
        }

        (_at[0], _at[Parts - 1]) = (_at[Parts - 1], _at[0]);
    }

    /// <summary>The angle straight opposite, in whole steps.</summary>
    private static int Opposite(int angle) => Wrapped(angle + TurnParts / 2) / StepParts * StepParts;

    /// <summary>Acts on a cue of one part's timeline.</summary>
    private void OnPartCue(int part, string cue)
    {
        switch (cue)
        {
            case "lower":
                Nudge(part, _target / StepParts + Turns.PerTurn / 2);
                break;
            case "raise":
                Nudge(part, _target / StepParts);
                break;
            case "resume":
                _paused = false;
                break;
            default:
                throw new InvalidOperationException($"Shaktool \"{Name}\" does not know the cue \"{cue}\".");
        }
    }

    /// <summary>
    /// Moves a part a pixel the way an angle points: straight up at nothing, turning clockwise. As on the cartridge, a
    /// pixel along the sine across and along the sine a quarter turn behind down, in 256ths of a pixel.
    /// </summary>
    private void Nudge(int part, int angle)
    {
        _at[part] += new PixelOffset(
            FromTwoFiftySixths(CartridgeAngles.SpeedAlong(256, angle)),
            FromTwoFiftySixths(CartridgeAngles.SpeedAlong(256, angle - Turns.PerTurn / 4)));
    }

    /// <summary>A distance given in 256ths of a pixel.</summary>
    private static Pixels FromTwoFiftySixths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 0xFF) << 8);

    /// <summary>Moves the parts' pictures on, and puts them, their colliders and the enemy itself where they are.</summary>
    private void ShowParts()
    {
        for (int part = 0; part < Parts; part++)
        {
            if (_players[part] is not { } player)
                continue;
            foreach (string cue in player.Step())
                OnPartCue(part, cue);
        }

        Position = _at[Head];
        for (int part = 0; part < Parts; part++)
        {
            var offset = new Vector2I(_at[part].X.Whole - Position.X.Whole, _at[part].Y.Whole - Position.Y.Whole);
            // A step with no picture leaves the part as it was shown.
            if (_players[part] is { } player && GetNodeOrNull<ActorVisual>($"Part{part + 1}") is { } visual)
            {
                visual.Position = offset;
                if (player.Current.Picture.Length > 0)
                    visual.Show(player.Current);
            }

            var rect = new PixelRect(offset.X - PartHalfSize, offset.Y - PartHalfSize, PartHalfSize * 2, PartHalfSize * 2);
            if (GetNodeOrNull<Collider>($"Hurtbox{part + 1}") is { } hurtbox)
                hurtbox.Rect = rect;
            if (part is 0 or Parts - 1 && GetNodeOrNull<Collider>(part == 0 ? "Saw1" : "Saw2") is { } hitbox)
                hitbox.Rect = rect;
        }
    }
}
