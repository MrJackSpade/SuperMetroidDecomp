using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A room's body of liquid: its surface level, its rise and fall, and the answer to "is this point under it". It
/// applies no physics and no damage; the player and enemies ask it and act on the answer. It can be told to "rise"
/// and "fall" between its two levels. It draws itself as its type says: a strip along its surface and a picture tiled
/// under it, across the view.
/// </summary>
[Tool]
public partial class Liquid : Node2D, IPlaced, ITriggerTarget
{
    /// <summary>The data file of its liquid type.</summary>
    [Export(PropertyHint.File)]
    public string TypeFile { get; set; } = "";

    /// <summary>The surface's height when lowered, in pixels down from the room's top.</summary>
    [Export]
    public int LowSurface { get; set; }

    /// <summary>The surface's height when raised, in pixels down from the room's top.</summary>
    [Export]
    public int HighSurface { get; set; }

    /// <summary>
    /// How far the surface moves each update while rising or falling, as a number of pixels. A speed is spent as the
    /// surface arrives where it was heading: it goes nowhere more until it is given a speed again, or told to rise or
    /// fall.
    /// </summary>
    [Export]
    public double MoveSpeed
    {
        get => _moveSpeed;
        set => (_moveSpeed, _speedSpent) = (value, false);
    }

    private double _moveSpeed = 1;

    /// <summary>Whether the speed it has brought the surface where it was heading, and so moves it no more.</summary>
    private bool _speedSpent;

    /// <summary>
    /// Whether it sets off for its other level by itself as its room is set up, with nothing telling it to: one that
    /// starts lowered rises and one that starts raised falls, after its wait. Its moves begin with its room's, so one
    /// entered through a passage that moves the room's liquid is waiting, or on its way, by the time play begins.
    /// </summary>
    [Export]
    public bool SetsOffAtOnce { get; set; }

    /// <summary>Whether the liquid starts raised.</summary>
    [Export]
    public bool StartsRaised { get; set; }

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>
    /// Whether it is held back from the player as its room is set up: it is there, moves and is drawn, and the player
    /// moves as though it were not, as behind glass, until something of the room lets it in (<see cref="LetIn"/>).
    /// </summary>
    [Export]
    public bool HeldBack { get; set; }

    private bool _letIn;

    /// <summary>Whether the player meets it: it is not held back, or has been let in.</summary>
    public bool IsMet => !HeldBack || _letIn;

    /// <summary>Lets a liquid that was held back at the player, for as long as the room lasts.</summary>
    public void LetIn() => _letIn = true;

    /// <summary>
    /// Whether it lies below its room all through, with no surface in it: nothing is ever under it and it is drawn
    /// nowhere. It moves as any other does, so whatever watches a liquid's moves is told of them: a room may have a
    /// liquid's effect and none of the liquid.
    /// </summary>
    [Export]
    public bool LiesBelowRoom { get; set; }

    /// <summary>How many updates it waits, once told to rise or fall, before it starts to.</summary>
    [Export]
    public int MoveDelay { get; set; }

    /// <summary>How far its tide carries its surface above and below where it lies, in pixels; zero for no tide.</summary>
    [Export]
    public int TideReach { get; set; }

    /// <summary>How far the tide turns each update while the surface is above where it lies, in 256ths of a turn.</summary>
    [Export]
    public double TideTurnAbove { get; set; } = 0.75;

    /// <summary>How far the tide turns each update while the surface is at or below where it lies.</summary>
    [Export]
    public double TideTurnBelow { get; set; } = 1.125;

    /// <summary>Whether its pictures move across as its type says they may.</summary>
    [Export]
    public bool Drifts { get; set; }

    /// <summary>The liquid's type, once set up.</summary>
    private LiquidType? _type;

    /// <summary>
    /// Where the surface lies with the tide left out of it. It begins half a pixel down its line, so that a tide or a
    /// slow rise or fall crosses onto the next line half a pixel sooner one way and later the other; a rise or fall
    /// that arrives leaves it on its line exactly.
    /// </summary>
    private Pixels _level;

    /// <summary>Where a rise or fall has got to.</summary>
    private enum Motion
    {
        /// <summary>It is going nowhere. With somewhere to go and a speed to go at, an update only begins its wait.</summary>
        Dormant,

        /// <summary>It counts its wait down, an update at a time, and moves from the update after the count runs out.</summary>
        Waiting,

        /// <summary>It moves toward where it is heading, and is dormant again once it arrives.</summary>
        Moving,
    }

    private Motion _motion;

    /// <summary>
    /// The count left of its wait. It is counted down before it is asked whether it has run out, round the end of a
    /// count of 65536: a wait of nothing is the longest there is.
    /// </summary>
    private int _waiting;

    /// <summary>Whether it is heading down, to a lower surface, settled as it is given somewhere to go.</summary>
    private bool _goesDown;

    /// <summary>
    /// How far round the tide has turned, in 256ths of a 256th of a turn, and how far that puts the surface from where
    /// it lies.
    /// </summary>
    private int _tidePhase;
    private Pixels _tide;

    /// <summary>Where the surface is heading.</summary>
    private Pixels _target;

    /// <summary>What it is drawn from, the pictures it shows now, how far they have moved across, and the view they cover.</summary>
    private ContentLibrary? _content;
    private TimelinePlayer? _surface;
    private TimelinePlayer? _body;
    private Pixels _drifted;
    private PixelRect _view;

    /// <summary>The liquid's type.</summary>
    public LiquidType Type => _type ?? throw new InvalidOperationException($"The liquid \"{Name}\" has not been set up.");

    /// <summary>The surface's height right now, down from the room's top.</summary>
    public Pixels Surface { get; private set; }

    /// <summary>Loads the liquid's type and puts its surface at its starting level.</summary>
    internal void Setup(ContentLibrary content)
    {
        if (TypeFile.Length == 0)
            throw new InvalidOperationException($"The liquid \"{Name}\" names no liquid type.");
        _type = LiquidType.Read(content.ReadData(TypeFile));
        int begins = StartsRaised ? HighSurface : LowSurface;
        _level = Pixels.FromParts(begins, Pixels.FractionsPerPixel / 2);
        _target = Pixels.FromWhole(SetsOffAtOnce ? (StartsRaised ? LowSurface : HighSurface) : begins);
        (_motion, _speedSpent, _waiting, _tidePhase, _tide) = (Motion.Dormant, false, MoveDelay, 0, Pixels.Zero);
        _begun = false;
        IsAstir = false;
        Surface = _level;
        _content = content;
        _surface = _type.Surface is { } surface ? new TimelinePlayer(surface) : null;
        _body = _type.Body is { } body ? new TimelinePlayer(body) : null;
        _drifted = Pixels.Zero;
        Material = new CanvasItemMaterial { BlendMode = _type.Blend };
        ZIndex = RoomDepth.Liquid;
    }

    /// <summary>
    /// Puts where the surface lies at a height at once, in pixels down from the room's top, keeping the part of a
    /// pixel it had; it goes on from there to where it was heading.
    /// </summary>
    public void PutAt(int height)
    {
        _level = _level.WithWhole(height);
        Surface = _level + _tide;
        QueueRedraw();
    }

    /// <summary>
    /// Sets the surface heading for a height, in pixels down from the room's top, at a speed of its own after a wait
    /// of its own: for something of the room that moves the liquid by its own values, as a boss whose fight raises it
    /// and whose death drains it, each at another speed. The speed is the liquid's from then on.
    /// </summary>
    public void HeadFor(int height, double speed, int delay)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(speed);
        ArgumentOutOfRangeException.ThrowIfNegative(delay);
        (MoveSpeed, _waiting, _target) = (speed, delay, Pixels.FromWhole(height));
        _goesDown = _target.Whole > _level.Whole;
    }

    /// <summary>Cuts whatever wait is left to one update: a liquid already waiting starts to move on the update after its next.</summary>
    public void EndWait() => _waiting = 1;

    /// <summary>
    /// Whether its last update was one of a rise or fall under way: an update of its wait, or one on which it moved,
    /// the one it arrived on among them. The update that only begins a wait is not one.
    /// </summary>
    public bool IsAstir { get; private set; }

    /// <summary>Whether <paramref name="point"/> lies under the surface: the pixel row it is in is below the surface's line, not on it.</summary>
    public bool IsSubmerged(PixelPosition point) => !LiesBelowRoom && point.Y > Surface;

    /// <summary>
    /// Takes the view it is drawn across. The room tells it each time the room shows itself, so it is drawn across
    /// the view as shown, on an update the liquid itself is not stepped as on any other.
    /// </summary>
    internal void Follow(PixelRect view)
    {
        _view = view;
        QueueRedraw();
    }

    /// <summary>Plays its pictures on one update.</summary>
    internal void Step()
    {
        _surface?.Step();
        _body?.Step();
        if (Drifts)
            _drifted += Type.Drift;
        QueueRedraw();
    }

    /// <summary>Whether its motion has begun: its first move since it was set up, which does nothing else, is behind it.</summary>
    private bool _begun;

    /// <summary>
    /// Plays one update of the surface's rise or fall, and turns its tide, and returns whether it did: its first move
    /// after it is set up only begins its motion, with no stage of a rise or fall and no turn of the tide. A rise or
    /// fall goes by stages: dormant, an update on which it has somewhere to go and a speed only begins its wait;
    /// waiting, it counts the wait down; and from the update after the count runs out it moves, until it arrives. The
    /// room does this as an update opens, so that everything in the update finds the surface where the update shows
    /// it.
    /// </summary>
    internal bool Move()
    {
        if (!_begun)
        {
            _begun = true;
            return false;
        }

        IsAstir = _motion != Motion.Dormant;
        switch (_motion)
        {
            case Motion.Dormant:
                // With somewhere to go and a speed to go at it begins to wait; nothing moves on this update.
                if (!_speedSpent && _moveSpeed > 0 && _level.Whole != _target.Whole)
                {
                    _goesDown = _target.Whole > _level.Whole;
                    _motion = Motion.Waiting;
                }

                break;

            case Motion.Waiting:
                _waiting = (_waiting - 1) & 0xFFFF;
                if (_waiting == 0)
                    _motion = Motion.Moving;
                break;

            case Motion.Moving:
                if (Arrives())
                    (_speedSpent, _motion) = (true, Motion.Dormant);
                break;

            default:
                throw new InvalidOperationException($"Undefined stage {_motion} of a liquid's motion.");
        }

        if (TideReach > 0)
        {
            // The tide lifts the surface and lets it fall: the negative cosine of how far it has turned, by the game's
            // table of sines, taken at the whole 256th of a turn it has reached. It turns more slowly while lifted.
            const int perStep = 256;
            _tide = Pixels.FromWhole(TideReach * Turns.CoarseSine((_tidePhase / perStep) - (Turns.PerTurn / 4))).Share(1, perStep);
            int turn = (int)Math.Round((_tide.IsNegative ? TideTurnAbove : TideTurnBelow) * perStep);
            _tidePhase = (_tidePhase + turn) % (Turns.PerTurn * perStep);
        }

        Surface = _level + _tide;
        return true;
    }

    /// <summary>
    /// Moves where the surface lies one update toward where it is heading, and returns whether it has arrived.
    /// Heading down it has arrived once it is past the line it was heading for, and is put back on that line; heading
    /// up, once it is on that line or above it. Arrived, it lies on the line with no part of a pixel.
    /// </summary>
    private bool Arrives()
    {
        Pixels step = Pixels.FromNumber(_moveSpeed);
        Pixels moved = _goesDown ? _level + step : _level - step;
        bool arrived = _goesDown ? moved.Whole > _target.Whole : moved.Whole <= _target.Whole;
        _level = arrived ? Pixels.FromWhole(_target.Whole) : moved;
        return arrived;
    }

    /// <summary>Draws the strip along the surface and the body under it, across the view.</summary>
    public override void _Draw()
    {
        if (LiesBelowRoom || _surface is null || _body is null || _content is null)
            return;
        Texture2D strip = _content.LoadTexture(_surface.Shown.Picture);
        Texture2D body = _content.LoadTexture(_body.Shown.Picture);
        // The strip's timeline says how far up from the surface its picture starts.
        int top = Surface.Whole + _surface.Shown.Offset.Y;
        int bottom = _view.Top + _view.Height;
        int right = _view.Left + _view.Width;
        if (top >= bottom)
            return;

        // Each is tiled from the room's left edge, moved by its drift, starting with the tile that reaches into the
        // view. They are laid in the room's own pixels, which are the liquid's but for where it is put in the room:
        // the room itself is moved against the screen.
        int stripWidth = strip.GetWidth(), stripHeight = strip.GetHeight();
        for (int x = FirstTile(_view.Left, stripWidth); x < right; x += stripWidth)
            DrawTexture(strip, new Vector2(x, top) - Position);
        int bodyWidth = body.GetWidth(), bodyHeight = body.GetHeight();
        int under = top + stripHeight;
        int firstRow = under >= _view.Top ? under : under + (_view.Top - under) / bodyHeight * bodyHeight;
        for (int y = firstRow; y < bottom; y += bodyHeight)
        {
            for (int x = FirstTile(_view.Left, bodyWidth); x < right; x += bodyWidth)
                DrawTexture(body, new Vector2(x, y) - Position);
        }

        int FirstTile(int left, int width)
        {
            int moved = _drifted.Whole;
            return left - ((((left - moved) % width) + width) % width);
        }
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        _waiting = MoveDelay;
        _target = what switch
        {
            "rise" => Pixels.FromWhole(HighSurface),
            "fall" => Pixels.FromWhole(LowSurface),
            _ => throw new ArgumentException($"A liquid can be told to \"rise\" or \"fall\", not \"{what}\".", nameof(what)),
        };

        // Told to go, it has its speed to go at again.
        _speedSpent = false;
        _goesDown = _target.Whole > _level.Whole;
    }
}
