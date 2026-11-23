using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Shaktool: a digging machine, a chain of seven parts with a saw at each end and its head in the middle. One saw
/// stays put while the rest of the chain curls round it and straightens out ahead; when the far saw meets a wall the
/// chain pauses, and either turns end for end or curls back the other way. Its saws cut through what they are made
/// to cut, and so it digs its way along.
/// </summary>
/// <remarks>
/// Its head plays "head_up", "head_up_right" and so on round the eight ways it can point. The visuals named "Part1"
/// through "Part7" are its parts in order; the saws play "saw" and, while it pauses at a wall, "saw_bob", whose cue
/// "resume" sets it moving again; the arms play "arm". Its hitboxes "Saw1" and "Saw2" are its saws.
/// </remarks>
public partial class Shaktool : Enemy
{
    /// <summary>Whether its chain curls clockwise to start with.</summary>
    [Export]
    public bool StartsClockwise { get; set; } = true;

    /// <summary>How far each part sits from the one before it, in pixels.</summary>
    [Export, DataValue("link_length")]
    public int LinkLength { get; set; } = 12;

    /// <summary>Half the size of a saw, in pixels, for meeting walls and for cutting.</summary>
    [Export, DataValue("saw_half_size")]
    public int SawHalfSize { get; set; } = 8;

    /// <summary>How far round, in angle steps, the chain has curled when it starts to straighten.</summary>
    [Export, DataValue("straighten_at")]
    public int StraightenAt { get; set; } = 240;

    /// <summary>The count its curl starts again from once every part points the same way.</summary>
    [Export, DataValue("straighten_restart")]
    public int StraightenRestart { get; set; } = 120;

    private const int Parts = 7;
    private const int Head = 3;
    private const int StepParts = 256;
    private const int TurnParts = Turns.PerTurn * StepParts;

    // Each part's starting angle about the part before it, in angle steps, and how fast it turns, in 256ths of a step.
    private static readonly int[] StartAngles = [0, 248, 232, 208, 176, 152, 136];
    private static readonly int[] TurnSpeeds = [0, 32, 96, 192, 320, 416, 480];
    private static readonly string[] Ways = ["up", "up_right", "right", "down_right", "down", "down_left", "left", "up_left"];

    private readonly int[] _angle = new int[Parts];
    private readonly int[] _turn = new int[Parts];
    private readonly PixelPosition[] _at = new PixelPosition[Parts];
    private bool _paused;
    private bool _straightening;
    private bool _turnsEndForEndNext = true;
    private bool _clockwise;
    private bool _headReversed;
    private int _target;
    private int _curled;
    private TimelinePlayer? _saw;
    private TimelinePlayer? _arm;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _clockwise = StartsClockwise;
        _at[0] = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        for (int part = 0; part < Parts; part++)
        {
            _angle[part] = StartAngles[part] * StepParts;
            _turn[part] = TurnSpeeds[part];
            if (part > 0)
                _at[part] = _at[part - 1] + Reach(_angle[part]);
        }

        _saw = new TimelinePlayer(Timelines.Get("saw"));
        _arm = new TimelinePlayer(Timelines.Get("arm"));
        AlwaysActive = true;
        Play("head_down");
        ShowParts();
    }

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
        Room.Terrain.TakeHit(cut, SawRect(_at[0]));
        Room.Terrain.TakeHit(cut, SawRect(_at[Parts - 1]));
        ShowParts();
    }

    /// <summary>An angle in 256ths of a step brought into one turn.</summary>
    private static int Wrapped(int angle) => ((angle % TurnParts) + TurnParts) % TurnParts;

    /// <summary>How far a part sits from the one before it at an angle: straight up at nothing, turning clockwise.</summary>
    private PixelOffset Reach(int angle)
    {
        int steps = angle / StepParts;
        Pixels length = Pixels.FromWhole(LinkLength);
        return new PixelOffset(Turns.Sine(steps, length), Turns.Sine(steps - Turns.PerTurn / 4, length));
    }

    /// <summary>The room a saw takes up when it is at <paramref name="at"/>.</summary>
    private PixelRect SawRect(PixelPosition at) =>
        new(at.X.Whole - SawHalfSize, at.Y.Whole - SawHalfSize, SawHalfSize * 2, SawHalfSize * 2);

    /// <summary>Puts a part where its angle has it about the part before, and turns its angle on.</summary>
    private void Orbit(int part)
    {
        _at[part] = _at[part - 1] + Reach(_angle[part]);
        int by = _straightening ? StepParts : _turn[part];
        _angle[part] = Wrapped(_angle[part] + (_clockwise ? by : -by));
    }

    /// <summary>Moves the far saw on; a wall in its way pauses the chain and turns it about.</summary>
    private void DriveTail()
    {
        const int tail = Parts - 1;
        PixelPosition before = PixelPosition.FromWhole(_at[tail].X.Whole, _at[tail].Y.Whole);
        Orbit(tail);
        PixelPosition wanted = PixelPosition.FromWhole(_at[tail].X.Whole, _at[tail].Y.Whole);
        var across = PixelPosition.FromWhole(wanted.X.Whole, before.Y.Whole);
        if (Room.Terrain.IsSolidIn(SawRect(across), SlopeTreatment.Ignore) || Room.Terrain.IsSolidIn(SawRect(wanted), SlopeTreatment.Wall))
        {
            _at[tail] = before;
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
                _turn[part] = TurnSpeeds[part];
            }

            _curled = StraightenRestart * StepParts;
            _turnsEndForEndNext = true;
        }

        _curled += _turn[tail];
        if (_curled >= StraightenAt * StepParts)
            _straightening = true;
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
        // The chain now aims a quarter turn on from where its far saw lies from the near one.
        PixelOffset apart = _at[Parts - 1] - _at[0];
        int bearing = (Turns.AngleOf(apart.X.Whole, apart.Y.Whole) + Turns.PerTurn / 4) * StepParts;
        _target = Wrapped(bearing + (_clockwise ? TurnParts / 4 : -TurnParts / 4));
        for (int part = Parts - 1; part >= 0; part--)
        {
            int left = Wrapped(_clockwise ? _target - _angle[part] : _angle[part] - _target);
            _turn[part] = left / StepParts * StepParts / 64;
        }

        _paused = true;
        _saw = new TimelinePlayer(Timelines.Get("saw_bob"));
    }

    /// <summary>Turns the chain end for end: the saws change places, and each part takes the mirrored angle of its opposite number.</summary>
    private void TurnEndForEnd()
    {
        _headReversed = !_headReversed;
        (int Left, int Right)[] pairs = [(6, 1), (5, 2), (4, 3)];
        foreach ((int left, int right) in pairs)
        {
            int saved = _angle[left];
            _angle[left] = Opposite(_angle[right]);
            _angle[right] = Opposite(saved);
        }

        (_at[0], _at[Parts - 1]) = (_at[Parts - 1], _at[0]);
    }

    /// <summary>The angle straight opposite, in whole steps.</summary>
    private static int Opposite(int angle) => Wrapped(angle + TurnParts / 2) / StepParts * StepParts;

    /// <summary>Moves the parts' pictures on, and puts them, the saws' hitboxes and the enemy itself where they are.</summary>
    private void ShowParts()
    {
        if (_saw is not null)
        {
            foreach (string cue in _saw.Step())
            {
                if (cue != "resume")
                    continue;
                _paused = false;
                _saw = new TimelinePlayer(Timelines.Get("saw"));
            }
        }

        _arm?.Step();
        Position = _at[Head];
        for (int part = 0; part < Parts; part++)
        {
            var offset = new Vector2(_at[part].X.Whole - Position.X.Whole, _at[part].Y.Whole - Position.Y.Whole);
            bool saw = part is 0 or Parts - 1;
            TimelinePlayer? player = saw ? _saw : _arm;
            if (part != Head && player is not null && GetNodeOrNull<ActorVisual>($"Part{part + 1}") is { } visual)
            {
                visual.Position = offset;
                visual.Show(player.Current.Picture);
            }

            if (saw && GetNodeOrNull<Collider>(part == 0 ? "Saw1" : "Saw2") is { } hitbox)
                hitbox.Rect = new PixelRect((int)offset.X - SawHalfSize, (int)offset.Y - SawHalfSize, SawHalfSize * 2, SawHalfSize * 2);
        }
    }
}
