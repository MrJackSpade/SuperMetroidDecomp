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

    /// <summary>How far the surface moves each update while rising or falling, as a number of pixels.</summary>
    [Export]
    public double MoveSpeed { get; set; } = 1;

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

    /// <summary>Where the surface lies with the tide left out of it.</summary>
    private Pixels _level;

    /// <summary>The updates left before it starts toward where it is heading.</summary>
    private int _waiting;

    /// <summary>How far round the tide has turned, in 256ths of a turn, and how far that puts the surface from where it lies.</summary>
    private double _tidePhase;
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
        _level = Pixels.FromWhole(StartsRaised ? HighSurface : LowSurface);
        _target = _level;
        (_waiting, _tidePhase, _tide) = (0, 0, Pixels.Zero);
        Surface = _level;
        _content = content;
        _surface = _type.Surface is { } surface ? new TimelinePlayer(surface) : null;
        _body = _type.Body is { } body ? new TimelinePlayer(body) : null;
        _drifted = Pixels.Zero;
        Material = new CanvasItemMaterial { BlendMode = _type.Blend };
        ZIndex = RoomDepth.Liquid;
    }

    /// <summary>Puts where the surface lies at a height at once, in pixels down from the room's top; it goes on from there to where it was heading.</summary>
    public void PutAt(int height)
    {
        _level = Pixels.FromWhole(height);
        Surface = _level + _tide;
        QueueRedraw();
    }

    /// <summary>Ends whatever wait is left before it starts toward where it is heading.</summary>
    public void EndWait() => _waiting = 0;

    /// <summary>Whether <paramref name="point"/> lies under the surface.</summary>
    public bool IsSubmerged(PixelPosition point) => point.Y >= Surface;

    /// <summary>Plays its pictures on one update, to be drawn across <paramref name="view"/>.</summary>
    internal void Step(PixelRect view)
    {
        _view = view;
        _surface?.Step();
        _body?.Step();
        if (Drifts)
            _drifted += Type.Drift;
        QueueRedraw();
    }

    /// <summary>
    /// Moves the surface one update toward where it is heading, once its wait is over, and turns its tide. The room
    /// does this as an update opens, so that everything in the update finds the surface where the update shows it.
    /// </summary>
    internal void Move()
    {
        if (_waiting > 0)
        {
            _waiting--;
        }
        else if (_level != _target)
        {
            Pixels step = Pixels.FromNumber(MoveSpeed);
            _level = _level < _target
                ? (_level + step > _target ? _target : _level + step)
                : (_level - step < _target ? _target : _level - step);
        }

        if (TideReach > 0)
        {
            // The tide lifts the surface and lets it fall, turning more slowly while it is lifted.
            _tide = Pixels.FromNumber(-Math.Cos(_tidePhase * Math.Tau / 256) * TideReach);
            _tidePhase += _tide.IsNegative ? TideTurnAbove : TideTurnBelow;
        }

        Surface = _level + _tide;
    }

    /// <summary>Draws the strip along the surface and the body under it, across the view.</summary>
    public override void _Draw()
    {
        if (_surface is null || _body is null || _content is null)
            return;
        Texture2D strip = _content.LoadTexture(_surface.Shown.Picture);
        Texture2D body = _content.LoadTexture(_body.Shown.Picture);
        // The strip's timeline says how far up from the surface its picture starts.
        int top = Surface.Whole + _surface.Shown.Offset.Y;
        int bottom = _view.Top + _view.Height;
        int right = _view.Left + _view.Width;
        if (top >= bottom)
            return;

        // Each is tiled from the room's left edge, moved by its drift, starting with the tile that reaches into the view.
        int stripWidth = strip.GetWidth(), stripHeight = strip.GetHeight();
        for (int x = FirstTile(_view.Left, stripWidth); x < right; x += stripWidth)
            DrawTexture(strip, ToLocal(new Vector2(x, top)));
        int bodyWidth = body.GetWidth(), bodyHeight = body.GetHeight();
        int under = top + stripHeight;
        int firstRow = under >= _view.Top ? under : under + (_view.Top - under) / bodyHeight * bodyHeight;
        for (int y = firstRow; y < bottom; y += bodyHeight)
        {
            for (int x = FirstTile(_view.Left, bodyWidth); x < right; x += bodyWidth)
                DrawTexture(body, ToLocal(new Vector2(x, y)));
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
    }
}
