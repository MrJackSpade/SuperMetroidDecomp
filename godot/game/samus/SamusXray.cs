using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game;

/// <summary>
/// Samus's X-ray scope: a cone of sight from her visor that opens as she holds it, swings up and down, and shows the
/// hidden blocks it passes over for what they are. It is a child of Samus and only looks; it changes nothing but
/// which blocks are hidden.
/// </summary>
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
    private bool _fullyOpen;
    private int _swing;

    /// <summary>How far the cone's middle has swung from level, in 256ths of a turn: up when negative.</summary>
    public int Swing => _swing;
    private int _halfWidth;
    private Vector2 _eye;
    private bool _facingLeft;

    /// <summary>Whether the scope is being looked through.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>Opens the scope, level, with its cone closed.</summary>
    public void Open(Samus samus)
    {
        IsOpen = true;
        _widening = Pixels.Zero;
        _opened = Pixels.Zero;
        _fullyOpen = false;
        _swing = 0;
        _halfWidth = 0;
        QueueRedraw();
    }

    /// <summary>Closes the scope.</summary>
    public void Close()
    {
        IsOpen = false;
        QueueRedraw();
    }

    /// <summary>
    /// Plays one update of looking: the cone opens a little more or, once fully open, swings by
    /// <paramref name="swing"/> (up when negative), and every hidden block inside it that sight reveals is shown.
    /// </summary>
    public void Look(Samus samus, int swing)
    {
        if (_fullyOpen)
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

    /// <summary>Whether a point lies inside the cone, given how far ahead of and below the eye it is.</summary>
    private bool IsInCone(long ahead, long below)
    {
        if (ahead <= 0)
            return false;
        // The cone lies between its upper and lower edges; a point is inside when it is below the upper and above the lower.
        (long upperAhead, long upperBelow) = Direction(_swing - _halfWidth);
        (long lowerAhead, long lowerBelow) = Direction(_swing + _halfWidth);
        return upperAhead * below - upperBelow * ahead >= 0 && lowerAhead * below - lowerBelow * ahead <= 0;
    }

    /// <summary>A direction at <paramref name="angle"/> below level, as distances ahead and below, scaled by 4096.</summary>
    private static (long Ahead, long Below) Direction(int angle)
    {
        Pixels size = Pixels.FromWhole(4096);
        return (Turns.Cosine(angle, size).Whole, Turns.Sine(angle, size).Whole);
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!IsOpen)
            return;
        float forward = _facingLeft ? -1 : 1;
        (long upperAhead, long upperBelow) = Direction(_swing - _halfWidth);
        (long lowerAhead, long lowerBelow) = Direction(_swing + _halfWidth);
        const float length = 400f / 4096f;
        Vector2[] cone =
        [
            _eye,
            _eye + new Vector2(upperAhead * forward, upperBelow) * length,
            _eye + new Vector2(lowerAhead * forward, lowerBelow) * length,
        ];
        DrawColoredPolygon(cone, new Color(1, 1, 1, 0.35f));
    }
}
