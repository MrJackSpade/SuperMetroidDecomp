using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Effects;
using Godot;

namespace Game;

/// <summary>
/// Samus's X-ray scope: a cone of sight from her visor that opens as she holds it, swings up and down, and shows the
/// hidden blocks it passes over for what they are. It is a child of Samus and only looks; it changes nothing but
/// which blocks are hidden.
/// </summary>
/// <remarks>
/// What it shows is the room outside its cone washed with the scope's grey, and the room inside it as it is: the
/// cartridge's colour maths outside a window. The grey is added to every colour of the room and of what is in it,
/// or taken from them in a room whose own liquid or overlay darkens what it lies over, and lies under the room's
/// liquid and overlays (<see cref="RoomDepth.OverRoom"/>). A room that shows none of its blocks to the scope
/// (<see cref="Room.ShowsBlocksToSight"/>) is not washed. In a Fireflea room the room's own dark, never less than the
/// scope's grey, is what is taken from everything outside the cone, and the dark is lifted from the room meanwhile,
/// so that nothing is dark inside the cone. Where the room's liquid lies, outside the cone, the room and the liquid
/// are halved together and take no grey (<see cref="RoomDepth.OverLiquid"/>), in a room that shows none of its blocks
/// too.
/// </remarks>
public partial class SamusXray : Node2D
{
    /// <summary>How far above her middle the scope looks from while she stands, in pixels.</summary>
    [Export]
    public int StandingEyeHeight { get; set; } = 16;

    /// <summary>How far above her middle the scope looks from while she crouches, in pixels.</summary>
    [Export]
    public int CrouchingEyeHeight { get; set; } = 12;

    /// <summary>How far ahead of her middle, the way she faces, the scope looks from, in pixels.</summary>
    [Export]
    public int EyeAhead { get; set; } = 3;

    private Pixels _widening;
    private Pixels _opened;
    private bool _lookedThrough;
    private bool _fullyOpen;
    private int _swing;

    /// <summary>How far the cone's middle has swung from level, in 256ths of a turn: up when negative.</summary>
    public int Swing => _swing;
    private int _halfWidth;
    private Vector2 _eye;
    private bool _facingLeft;

    /// <summary>Whether the scope is being looked through.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>The scope's grey, out of the cartridge's thirty-one levels.</summary>
    private const int Grey = 7;

    /// <summary>What the room outside the cone is washed with, as a part of each colour.</summary>
    private float _wash = Grey / 31f;

    /// <summary>The dark of the Fireflea room it was opened in, which is lifted while it is open; null in any other room.</summary>
    private FirefleaDarkness? _darkness;

    /// <summary>
    /// What halves the room where its liquid lies, outside the cone: the cartridge combines the room with a liquid
    /// lying behind it and halves the two there, where above the liquid it combines the room with the grey unhalved.
    /// Drawn over the liquid, each a piece of the view outside the cone and below the liquid's surface.
    /// </summary>
    private readonly List<Polygon2D> _halved = [];

    /// <summary>How many pieces the view outside the cone and below a surface can fall into.</summary>
    private const int MostHalvedPieces = 4;

    /// <summary>How far down the room Samus's own place is, as the scope was last looked through: what a room's row is counted from here.</summary>
    private int _ownRow;

    /// <summary>Whether the room it was opened in is washed outside its cone.</summary>
    private bool _washes;

    /// <summary>The room it was opened in, which is told when it closes; null while it is closed.</summary>
    private Room? _room;

    /// <summary>Opens the scope, level, with its cone closed.</summary>
    public void Open(Samus samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        Room room = samus.Room;
        // A Fireflea room's dark goes before all else: the scope takes it, never less than its own grey, from
        // everything outside its cone, and the room's own darkening is lifted while it does.
        _darkness = room.GetChildren().OfType<FirefleaDarkness>().FirstOrDefault(darkness => darkness.IsActive);
        _darkness?.BeLifted(true);
        _washes = _darkness is not null || room.ShowsBlocksToSight;
        _wash = Math.Max(Grey, _darkness?.Dark ?? 0) / 31f;

        // The grey is taken from the room where the room's own liquid or overlay darkens what it lies over, whether
        // or not any of it is in view, and added to it everywhere else.
        bool darkens = _darkness is not null || room.Liquid?.Type.Blend == CanvasItemMaterial.BlendModeEnum.Sub
            || room.GetChildren().OfType<OverlayEffect>().Any(overlay => overlay.Material is CanvasItemMaterial { BlendMode: CanvasItemMaterial.BlendModeEnum.Sub });
        Material = new CanvasItemMaterial { BlendMode = darkens ? CanvasItemMaterial.BlendModeEnum.Sub : CanvasItemMaterial.BlendModeEnum.Add };
        ZAsRelative = false;
        ZIndex = RoomDepth.OverRoom;

        // The pieces that halve the room over its liquid are made once and shown as they are wanted.
        while (_halved.Count < MostHalvedPieces)
        {
            var piece = new Polygon2D { Name = $"Halved{_halved.Count}", Color = new Color(0, 0, 0, 0.5f), ZAsRelative = false, ZIndex = RoomDepth.OverLiquid, Visible = false };
            AddChild(piece);
            _halved.Add(piece);
        }

        // A room that hides what lies behind it from the scope, the glass tunnel, does so from here until it closes.
        _room = room;
        room.BeLookedThrough(true);
        IsOpen = true;
        _widening = Pixels.Zero;
        _opened = Pixels.Zero;
        _lookedThrough = false;
        _fullyOpen = false;
        _swing = 0;
        _halfWidth = 0;
        QueueRedraw();
    }

    /// <summary>Closes the scope.</summary>
    public void Close()
    {
        _room?.BeLookedThrough(false);
        _room = null;
        _darkness?.BeLifted(false);
        _darkness = null;
        IsOpen = false;
        QueueRedraw();
    }

    /// <summary>
    /// Plays one update of looking: the cone opens a little more or, once fully open, swings by
    /// <paramref name="swing"/> (up when negative), and every hidden block inside it that sight reveals is shown. The
    /// first update it is looked through it is a ray no wider than a pixel, and it opens from the next.
    /// </summary>
    public void Look(Samus samus, int swing)
    {
        if (!_lookedThrough)
            _lookedThrough = true;
        else if (_fullyOpen)
        {
            // It swings no further than puts its edge straight up or straight down.
            int furthest = Turns.PerTurn / 4 - _halfWidth;
            _swing = Math.Clamp(_swing + swing, -furthest, furthest);
        }
        else
        {
            // The cone opens faster each update; the update it passes its widest it is held there, and it swings
            // from the next.
            _widening += Pixels.FromNumber(samus.XrayWideningStep);
            _opened += _widening;
            _fullyOpen = _opened.Whole > samus.XrayWidestHalfWidth;
            _halfWidth = Math.Min(samus.XrayWidestHalfWidth, _opened.Whole);
        }
        _facingLeft = samus.Facing == Facing.Left;
        int ahead = _facingLeft ? -EyeAhead : EyeAhead;
        int height = samus.Form == Player.Crouching ? CrouchingEyeHeight : StandingEyeHeight;
        var eye = new PixelPosition(samus.Position.X + Pixels.FromWhole(ahead), samus.Position.Y - Pixels.FromWhole(height));
        _eye = new Vector2(ahead, -height);
        _ownRow = samus.Position.Y.Whole;

        int reach = samus.XrayReach;
        int forward = _facingLeft ? -1 : 1;
        var sweep = new PixelRect(_facingLeft ? eye.X.Whole - reach : eye.X.Whole, eye.Y.Whole - reach, reach, reach * 2);
        // In some rooms the scope shows nothing of the room's blocks: the cone still opens, and that is all.
        if (!samus.Room.ShowsBlocksToSight)
        {
            QueueRedraw();
            return;
        }

        Terrain terrain = samus.Room.Terrain;
        foreach (Block block in terrain.BlocksIn(sweep))
        {
            if (!block.Hidden || !block.Type.RevealedBySight)
                continue;
            long across = (block.Rect.Left + block.Rect.Width / 2 - eye.X.Whole) * forward;
            long down = block.Rect.Top + block.Rect.Height / 2 - eye.Y.Whole;
            if (IsInCone(across, down))
                terrain.RevealBySight(block.Rect);
        }

        // The plain tiles the scope looks through too: those whose middles its cone covers.
        int size = terrain.TileSize;
        for (int y = Math.Max(0, sweep.Top / size * size); y < sweep.Bottom; y += size)
        {
            for (int x = Math.Max(0, sweep.Left / size * size); x < sweep.Right; x += size)
            {
                if (IsInCone((long)(x + size / 2 - eye.X.Whole) * forward, y + size / 2 - eye.Y.Whole))
                    terrain.SeeTileAt(x, y);
            }
        }

        QueueRedraw();
    }

    /// <summary>
    /// Whether a point lies inside the cone, given how far ahead of and below the eye it is: no more than half a pixel
    /// outside either edge and not behind the eye, so a cone with no width is a ray a pixel wide.
    /// </summary>
    private bool IsInCone(long ahead, long below)
    {
        (long upperAhead, long upperBelow) = Direction(_swing - _halfWidth);
        (long lowerAhead, long lowerBelow) = Direction(_swing + _halfWidth);
        return upperAhead * below - upperBelow * ahead >= -HalfPixel
            && ahead * lowerBelow - below * lowerAhead >= -HalfPixel
            && ahead * (upperAhead + lowerAhead) + below * (upperBelow + lowerBelow) >= 0;
    }

    /// <summary>The length of a direction in its own numbers: the peak of the cartridge's fine sine.</summary>
    private const int DirectionLength = 32767;

    /// <summary>Half a pixel in a direction's own numbers, against which a point is measured beside an edge.</summary>
    private const int HalfPixel = (DirectionLength + 1) / 2;

    /// <summary>A direction at <paramref name="angle"/> below level, as distances ahead and below by the cartridge's fine sine.</summary>
    private static (long Ahead, long Below) Direction(int angle) =>
        (CartridgeAngles.FineSignedSine(angle + Turns.PerTurn / 4), CartridgeAngles.FineSignedSine(angle));

    /// <inheritdoc/>
    public override void _Draw()
    {
        // Nothing is seen until the scope is first looked through.
        foreach (Polygon2D piece in _halved)
            piece.Visible = false;
        if (!IsOpen || !_lookedThrough)
            return;
        float forward = _facingLeft ? -1 : 1;

        // Far enough from the eye that the corners between two points of the round reach past the view's corners.
        const float length = 480f / DirectionLength;
        const int step = Turns.PerTurn / 8;
        Vector2 Around(int angle)
        {
            (long ahead, long below) = Direction(angle);
            return _eye + new Vector2(ahead * forward, below) * length;
        }

        // What is washed is everything outside the cone: round the eye from the cone's lower edge, behind her, to
        // its upper edge. A cone with no width is a ray a pixel wide, and all the view round the eye is washed.
        int from = _swing + _halfWidth, to = _swing - _halfWidth + Turns.PerTurn;
        var outside = new List<Vector2>();
        if (_halfWidth > 0)
            outside.Add(_eye);
        for (int angle = from; angle < to; angle += step)
            outside.Add(Around(angle));
        if (_halfWidth > 0)
            outside.Add(Around(to));
        IEnumerable<Vector2[]> washed = [[.. outside]];

        // Where the room's liquid lies, from the row below its surface down, the room and the liquid are halved and
        // take no grey; the grey is for what is above. A Fireflea room's dark is whole wherever it falls.
        if (_darkness is null && _room?.Liquid is { } liquid)
        {
            const float far = 600;
            float surface = liquid.Surface.Whole + 1 - _ownRow;
            washed = Geometry2D.IntersectPolygons([.. outside], [new(-far, -far), new(far, -far), new(far, surface), new(-far, surface)]);
            int shown = 0;
            foreach (Vector2[] under in Geometry2D.IntersectPolygons([.. outside], [new(-far, surface), new(far, surface), new(far, far), new(-far, far)]))
            {
                if (shown == _halved.Count || under.Length < 3)
                    continue;
                (_halved[shown].Polygon, _halved[shown].Visible) = (under, true);
                shown++;
            }
        }

        if (!_washes)
            return;
        foreach (Vector2[] piece in washed)
        {
            if (piece.Length >= 3)
                DrawColoredPolygon(piece, new Color(_wash, _wash, _wash));
        }
    }
}
