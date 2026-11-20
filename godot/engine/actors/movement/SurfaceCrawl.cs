using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Following the surface it is on, around corners: along floors, up walls, across ceilings and back down, always in
/// the same sense. Stepping off the end of a surface it turns round the outside corner; meeting a wall it turns up the
/// inside one.
/// </summary>
public sealed class SurfaceCrawl
{
    /// <summary>The side of the mover the surface it clings to is on.</summary>
    public enum Clings
    {
        /// <summary>The surface is beneath it: a floor.</summary>
        Below,

        /// <summary>The surface is to its right: a wall.</summary>
        Right,

        /// <summary>The surface is above it: a ceiling.</summary>
        Above,

        /// <summary>The surface is to its left: a wall.</summary>
        Left,
    }

    /// <summary>How far it travels along its surface each update.</summary>
    public Pixels Speed { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 2);

    /// <summary>Whether it goes round clockwise: rightward along a floor, down a wall on its left.</summary>
    public bool Clockwise { get; set; } = true;

    /// <summary>The side the surface it clings to is on.</summary>
    public Clings Side { get; set; } = Clings.Below;

    /// <summary>The part of a pixel travelled toward the next whole one.</summary>
    private Pixels _carried;

    /// <summary>Moves <paramref name="mover"/> for one update along its surface.</summary>
    public MoveReport Step(IMoves mover)
    {
        _carried += Speed;
        bool turned = false;
        while (_carried >= Pixels.FromWhole(1))
        {
            _carried -= Pixels.FromWhole(1);
            turned |= StepOnePixel(mover);
        }

        return new MoveReport(false, false, false, turned, false);
    }

    /// <summary>Moves one pixel along the surface, turning a corner if there is one. Returns whether it turned.</summary>
    private bool StepOnePixel(IMoves mover)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        ICollisionOwner owner = body.CollisionOwner;
        MoveOptions crawl = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        (int alongX, int alongY) = Along(Side);
        (int intoX, int intoY) = Into(Side);
        PixelRect here = body.RoomRect;

        // An inside corner: something is in the way ahead, so it climbs onto it.
        if (!room.IsFree(here.MovedBy(alongX, alongY), owner, crawl))
        {
            Side = Clockwise ? Previous(Side) : Next(Side);
            return true;
        }

        mover.Position += PixelOffset.FromWhole(alongX, alongY);
        here = body.RoomRect;

        // An outside corner: the surface has ended beneath it, so it goes round and down onto the next face.
        if (room.IsFree(here.MovedBy(intoX, intoY), owner, crawl))
        {
            mover.Position += PixelOffset.FromWhole(intoX, intoY);
            Side = Clockwise ? Next(Side) : Previous(Side);
            return true;
        }

        return false;
    }

    /// <summary>The step along a surface on the given side, in the mover's sense of travel.</summary>
    private (int X, int Y) Along(Clings side)
    {
        (int x, int y) = side switch
        {
            Clings.Below => (1, 0),
            Clings.Right => (0, -1),
            Clings.Above => (-1, 0),
            Clings.Left => (0, 1),
            _ => throw new InvalidOperationException($"Undefined side {side}."),
        };
        return Clockwise ? (x, y) : (-x, -y);
    }

    /// <summary>The step into the surface on the given side.</summary>
    private static (int X, int Y) Into(Clings side) => side switch
    {
        Clings.Below => (0, 1),
        Clings.Right => (1, 0),
        Clings.Above => (0, -1),
        Clings.Left => (-1, 0),
        _ => throw new InvalidOperationException($"Undefined side {side}."),
    };

    /// <summary>The side after <paramref name="side"/>, going round by way of the floor, the left wall, the ceiling and the right wall.</summary>
    private static Clings Next(Clings side) => side switch
    {
        Clings.Below => Clings.Left,
        Clings.Left => Clings.Above,
        Clings.Above => Clings.Right,
        Clings.Right => Clings.Below,
        _ => throw new InvalidOperationException($"Undefined side {side}."),
    };

    /// <summary>The side before <paramref name="side"/>, in the same order.</summary>
    private static Clings Previous(Clings side) => side switch
    {
        Clings.Below => Clings.Right,
        Clings.Right => Clings.Above,
        Clings.Above => Clings.Left,
        Clings.Left => Clings.Below,
        _ => throw new InvalidOperationException($"Undefined side {side}."),
    };
}
