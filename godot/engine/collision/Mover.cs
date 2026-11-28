using Engine.Shared;

namespace Engine.Collision;

/// <summary>
/// Moves bodies through a room. There is one mover for everything: it moves a body along one axis by an exact
/// distance, a pixel at a time, and reports how far it got and what stopped it. It does not know what it is moving.
/// The ways a move may differ are <see cref="MoveOptions"/>, never a second mover.
/// </summary>
public sealed class Mover
{
    /// <summary>The room's collision.</summary>
    private readonly CollisionWorld _world;

    /// <summary>A mover for <paramref name="world"/>'s room.</summary>
    public Mover(CollisionWorld world)
    {
        ArgumentNullException.ThrowIfNull(world);
        _world = world;
    }

    /// <summary>Moves <paramref name="body"/>'s owner across by <paramref name="distance"/>, rightward when positive.</summary>
    /// <param name="body">The body moved.</param>
    /// <param name="distance">How far it goes when nothing stops it.</param>
    /// <param name="options">The ways this move differs from the plain one.</param>
    /// <param name="triedAs">
    /// For a move tried only where it ends: the distance it is tried as, where that is more than it goes, as for a
    /// move the ground under it lets only a share of through. The same distance, when null.
    /// </param>
    /// <param name="stillToward">
    /// For a move tried only where it ends that is tried as no distance: the way it is tried all the same, for the
    /// solids that could lie in it. The cartridge feels for a solid enemy ahead of Samus on every move of hers
    /// across, a move of nothing among them. A move of no distance that names no way asks after no solid.
    /// </param>
    public MoveResult MoveAcross(Collider body, Pixels distance, MoveOptions options, Pixels? triedAs = null, Facing? stillToward = null)
    {
        if (options.Tries == WayTried.WhereItEnds)
            return MoveWhereItEnds(body, distance, triedAs ?? distance, options, across: true, stillToward);
        ICollisionOwner owner = body.CollisionOwner;
        bool startedOnGround = options.FollowGroundDown > 0 && IsOnGround(body, options);
        Pixels start = owner.Position.X;
        Pixels target = start + distance;
        int step = distance.IsNegative ? -1 : 1;
        int pixels = Math.Abs(target.Whole - start.Whole);
        MoveResult? blocked = null;

        for (int moved = 0; moved < pixels; moved++)
        {
            PixelRect here = body.RoomRect;
            PixelRect ahead = here.MovedBy(step, 0);
            Obstacle obstacle = FindObstacle(ahead, owner, options);
            if (!obstacle.Found)
            {
                owner.Position = owner.Position with { X = Pixels.FromWhole(owner.Position.X.Whole + step) };
                if (startedOnGround)
                    FollowGroundDown(body, options);
                continue;
            }

            int climb = ClimbFor(here, ahead, step, owner, options, obstacle);
            if (climb > 0)
            {
                owner.Position = new PixelPosition(
                    Pixels.FromWhole(owner.Position.X.Whole + step), owner.Position.Y.WithWhole(owner.Position.Y.Whole - climb));
                continue;
            }

            blocked = new MoveResult(owner.Position.X - start, true, obstacle.Solid is null, obstacle.Solid);
            break;
        }

        if (blocked is null && step > 0 && !distance.IsZero)
        {
            // A move that ends part of the way into a pixel must not carry that part into a wall: against something it
            // cannot climb, it stops flush.
            PixelRect here = body.RoomRect;
            Obstacle obstacle = FindObstacle(here.MovedBy(1, 0), owner, options);
            if (obstacle.Found && ClimbFor(here, here.MovedBy(1, 0), 1, owner, options, obstacle) == 0)
                blocked = new MoveResult(Pixels.FromWhole(owner.Position.X.Whole) - start, true, obstacle.Solid is null, obstacle.Solid);
        }

        if (blocked is null)
            owner.Position = owner.Position with { X = target };
        else
            owner.Position = owner.Position with { X = Pixels.FromWhole(owner.Position.X.Whole) };

        return blocked ?? MoveResult.Free(distance);
    }

    /// <summary>Moves <paramref name="body"/>'s owner down by <paramref name="distance"/>, upward when negative.</summary>
    public MoveResult MoveDown(Collider body, Pixels distance, MoveOptions options)
    {
        if (options.Tries == WayTried.WhereItEnds)
            return MoveWhereItEnds(body, distance, distance, options, across: false);
        ICollisionOwner owner = body.CollisionOwner;
        Pixels start = owner.Position.Y;
        Pixels target = start + distance;
        int step = distance.IsNegative ? -1 : 1;
        int pixels = Math.Abs(target.Whole - start.Whole);

        for (int moved = 0; moved < pixels; moved++)
        {
            Obstacle obstacle = FindObstacle(body.RoomRect.MovedBy(0, step), owner, options);
            if (obstacle.Found)
            {
                owner.Position = owner.Position with { Y = Pixels.FromWhole(owner.Position.Y.Whole) };
                return new MoveResult(owner.Position.Y - start, true, obstacle.Solid is null, obstacle.Solid);
            }

            owner.Position = owner.Position with { Y = Pixels.FromWhole(owner.Position.Y.Whole + step) };
        }

        if (step > 0 && !distance.IsZero)
        {
            // A fall that ends part of the way into a pixel must not carry that part into the floor: it lands flush.
            Obstacle below = FindObstacle(body.RoomRect.MovedBy(0, 1), owner, options);
            if (below.Found)
            {
                owner.Position = owner.Position with { Y = Pixels.FromWhole(owner.Position.Y.Whole) };
                return new MoveResult(owner.Position.Y - start, true, below.Solid is null, below.Solid);
            }
        }

        owner.Position = owner.Position with { Y = target };
        return MoveResult.Free(distance);
    }

    /// <summary>
    /// Moves a body along one axis, trying the move only where it would end (<see cref="WayTried.WhereItEnds"/>).
    /// </summary>
    /// <param name="body">The body moved.</param>
    /// <param name="distance">How far it goes when nothing stops it.</param>
    /// <param name="tried">The distance the move is tried as.</param>
    /// <param name="options">The ways this move differs from the plain one.</param>
    /// <param name="across">Whether the move is across; otherwise it is down.</param>
    /// <param name="stillToward">The way a move across tried as no distance is tried; none for one that asks after no solid.</param>
    private MoveResult MoveWhereItEnds(Collider body, Pixels distance, Pixels tried, MoveOptions options, bool across, Facing? stillToward = null)
    {
        ICollisionOwner owner = body.CollisionOwner;
        Pixels start = across ? owner.Position.X : owner.Position.Y;
        int step = tried.IsNegative || (tried.IsZero && stillToward == Facing.Left) ? -1 : 1;
        MoveResult result;

        // The stretch of the body's leading edge the terrain was asked about before anything stopped it, when it is asked.
        PixelRect? reached = null;

        // Solids come first, and one in the way settles the move by itself. A move up or down asks after them even
        // when it goes nowhere; a move across that goes nowhere asks after them the way it says it is tried, and not
        // at all when it says none.
        (Collider Solid, int Gap)? inTheWay = options.StoppedBySolids && (!across || !tried.IsZero || stillToward is not null)
            ? SolidInTheWay(body, tried, across, back: step < 0)
            : null;
        if (inTheWay is { } met)
        {
            // Lying exactly against the solid, the body loses its part of a pixel up and down, whichever way it moved.
            if (met.Gap == 0)
                owner.Position = owner.Position with { Y = Pixels.FromWhole(owner.Position.Y.Whole) };

            // The body goes the whole pixels between them, and keeps whatever part of a pixel it has.
            Pixels from = across ? owner.Position.X : owner.Position.Y;
            Place(owner, from + Pixels.FromWhole(step * met.Gap), across);
            result = new MoveResult(Pixels.FromWhole(step * met.Gap), true, false, met.Solid);
        }
        else if (!tried.IsZero && options.StoppedByTerrain &&
            StopByTerrain(body, start, tried, step, options, across, out PixelRect? tile, out bool putsOnSlope, out reached) is { } rest)
        {
            // A stopped move went whole pixels: where within its pixel the stop leaves the body is no part of it.
            Place(owner, rest, across);
            result = new MoveResult(Pixels.FromWhole(rest.Whole - start.Whole), true, true, null) { StoppedByTile = tile, PutOnSlope = putsOnSlope };
        }
        else
        {
            Place(owner, start + distance, across);
            result = MoveResult.Free(distance);
        }

        // Having moved across, or tried to, the body is put where sloped ground holds it under its middle.
        bool put = across && options.Slopes == SlopeTreatment.ClimbUnderMiddle && RestUnderMiddle(body);
        return result with { PutOnSlope = put || result.PutOnSlope, Reached = reached };
    }

    /// <summary>
    /// Where terrain leaves a body whose move is tried only where it ends, along the axis of the move; null when the
    /// terrain its leading edge would end in does not stop it.
    /// </summary>
    private Pixels? StopByTerrain(
        Collider body, Pixels start, Pixels tried, int step, MoveOptions options, bool across, out PixelRect? stoppedBy, out bool putsOnSlope, out PixelRect? reached)
    {
        int shift = (start + tried).Whole - start.Whole;
        PixelRect here = body.RoomRect;
        PixelRect edge = across
            ? new PixelRect((step < 0 ? here.Left : here.Right - 1) + shift, here.Top, 1, here.Height)
            : new PixelRect(here.Left, (step < 0 ? here.Top : here.Bottom - 1) + shift, here.Width, 1);
        EdgeStop stop = _world.Terrain.StopsEdge(edge, across ? step : 0, across ? 0 : step, options.Slopes, options.TriesFromRight);
        stoppedBy = stop.Tile;
        putsOnSlope = stop.PutsOnSlope;

        // The edge is asked about tile by tile, and what comes after the tile that stops it is not asked about at all.
        reached = stop.Tile is { } stopping ? Before(edge, stopping, across, options.TriesFromRight) : edge;

        // A solid that stands as part of the room stops the edge as a tile of terrain would, and is met with the
        // terrain: by a move the terrain stops, whether or not other solids stop it.
        if (!stop.Stopped)
        {
            foreach (Collider solid in _world.Overlapping(edge, ColliderRole.Solid, except: body.CollisionOwner))
            {
                if (!solid.StandsAsTerrain)
                    continue;
                PixelRect other = solid.RoomRect;
                int into = across
                    ? (step > 0 ? edge.Left - other.Left + 1 : other.Right - edge.Left)
                    : (step > 0 ? edge.Top - other.Top + 1 : other.Bottom - edge.Top);
                stop = new EdgeStop(true, into, false);
                reached = Before(edge, other, across, options.TriesFromRight);
                break;
            }
        }

        // A way out of the room stops the edge as a tile of terrain would, for a move that says it does.
        if (!stop.Stopped && options.StoppedByWaysOut && _world.WayOutOver(edge) is { } wayOut)
        {
            int into = across
                ? (step > 0 ? edge.Left - wayOut.Left + 1 : wayOut.Right - edge.Left)
                : (step > 0 ? edge.Top - wayOut.Top + 1 : wayOut.Bottom - edge.Top);
            stop = new EdgeStop(true, into, false);
            reached = Before(edge, wayOut, across, options.TriesFromRight);
        }

        if (!stop.Stopped)
            return null;

        if (stop.BySlopeUnderMiddle)
        {
            // A move that rests against slopes leaves the body on the slope's surface, wherever that lies from where
            // it began, at the end of its pixel that lies against the slope.
            if (options.RestsAgainstSlopes && stop.Back is { } surface)
            {
                int rested = step > 0 ? shift - surface : shift + surface;
                return Pixels.FromParts(start.Whole + rested, step > 0 && !options.RestsAtPixelStart ? Pixels.FractionsPerPixel - 1 : 0);
            }

            // Otherwise the body keeps its part of a pixel: going down it goes only the whole pixels of the move,
            // less what the stop takes back, and never backwards; going up it stays where it began.
            int kept = stop.Back is not { } taken ? 0 : step > 0 ? Math.Max(0, tried.Whole - taken) : 0;
            return start + Pixels.FromWhole(kept);
        }

        // Otherwise it rests against what stopped it, never further back than where it began: at the far end of its
        // pixel going right or down, unless the move says it rests at the pixel's start, and at the pixel's start going
        // left or up.
        int moved = stop.Back is not { } back ? 0 : step > 0 ? Math.Max(0, shift - back) : Math.Min(0, shift + back);
        return Pixels.FromParts(start.Whole + moved, step > 0 && !options.RestsAtPixelStart ? Pixels.FractionsPerPixel - 1 : 0);
    }

    /// <summary>
    /// The stretch of a leading edge that comes before what stopped it, in the order the edge is asked about: from its
    /// top for a move across; for a move down from its left, or from its right when it is tried from the right.
    /// </summary>
    private static PixelRect Before(PixelRect edge, PixelRect stopper, bool across, bool fromRight) =>
        across ? new PixelRect(edge.Left, edge.Top, edge.Width, Math.Max(0, stopper.Top - edge.Top))
        : fromRight ? new PixelRect(stopper.Right, edge.Top, Math.Max(0, edge.Right - stopper.Right), edge.Height)
        : new PixelRect(edge.Left, edge.Top, Math.Max(0, stopper.Left - edge.Left), edge.Height);

    /// <summary>
    /// The solid that stops a move tried only where it ends, and the whole pixels between it and the body: the first
    /// registered that the body would overlap where the move is tried and does not overlap already. The move is tried
    /// one pixel beyond the one it reaches, unless it ends exactly on a pixel without the part of a pixel it began
    /// with having carried it over an edge; and carried so onto the room's first row or column, it is tried there.
    /// A move of no distance is tried so too, the way <paramref name="back"/> says: a pixel on from a body with part
    /// of a pixel to its place, and where it lies from one with none.
    /// </summary>
    private (Collider Solid, int Gap)? SolidInTheWay(Collider body, Pixels tried, bool across, bool back)
    {
        ICollisionOwner owner = body.CollisionOwner;
        Pixels centre = across ? owner.Position.X : owner.Position.Y;
        Pixels reached = centre + tried;
        bool crossed = back
            ? centre.Fraction < tried.Abs().Fraction
            : centre.Fraction + tried.Fraction >= Pixels.FractionsPerPixel;
        int tested = crossed && reached.Whole == 0 ? 0
            : !crossed && reached.Fraction == 0 ? reached.Whole
            : reached.Whole + (back ? -1 : 1);
        int shift = tested - centre.Whole;
        PixelRect here = body.RoomRect;
        PixelRect there = across ? here.MovedBy(shift, 0) : here.MovedBy(0, shift);
        foreach (Collider solid in _world.Overlapping(there, ColliderRole.Solid, except: owner))
        {
            // One that stands as part of the room is met as terrain is.
            if (solid.StandsAsTerrain)
                continue;
            PixelRect other = solid.RoomRect;
            int gap = across
                ? (back ? here.Left - other.Right : other.Left - here.Right)
                : (back ? here.Top - other.Bottom : other.Top - here.Bottom);

            // A solid the body is already in does not stop it.
            if (gap >= 0)
                return (solid, gap);
        }

        return null;
    }

    /// <summary>
    /// Puts a body where sloped ground that holds things up by their middle has it under its middle column: up out of
    /// a floor its feet have sunk into, then down from a ceiling its head is in or lies against. Returns whether
    /// either held it.
    /// </summary>
    private bool RestUnderMiddle(Collider body)
    {
        ICollisionOwner owner = body.CollisionOwner;
        PixelRect here = body.RoomRect;
        int middle = here.Left + here.Width / 2;
        bool put = false;
        if (_world.Terrain.SlopeRestRow(middle, here.Bottom - 1, ceiling: false) is { } floor && here.Bottom - 1 > floor)
        {
            owner.Position = owner.Position with { Y = owner.Position.Y.WithWhole(owner.Position.Y.Whole - (here.Bottom - 1 - floor)) };
            put = true;
            here = body.RoomRect;
        }

        if (_world.Terrain.SlopeRestRow(middle, here.Top, ceiling: true) is { } under && here.Top <= under)
        {
            owner.Position = owner.Position with { Y = owner.Position.Y.WithWhole(owner.Position.Y.Whole + (under - here.Top)) };
            put = true;
        }

        return put;
    }

    /// <summary>Puts an owner at a place along one axis.</summary>
    private static void Place(ICollisionOwner owner, Pixels at, bool across) =>
        owner.Position = across ? owner.Position with { X = at } : owner.Position with { Y = at };

    /// <summary>
    /// Tries a move across without making it: reports what the move would report, and where it would leave the body,
    /// with the owner put back exactly where it was.
    /// </summary>
    /// <param name="body">The body the move is tried for.</param>
    /// <param name="distance">How far the move would go, rightward when positive.</param>
    /// <param name="options">The ways the move differs from the plain one.</param>
    /// <param name="rest">Where the move would have left the owner.</param>
    public MoveResult TryAcross(Collider body, Pixels distance, MoveOptions options, out PixelPosition rest)
    {
        ICollisionOwner owner = body.CollisionOwner;
        PixelPosition before = owner.Position;
        MoveResult result = MoveAcross(body, distance, options);
        rest = owner.Position;
        owner.Position = before;
        return result;
    }

    /// <summary>
    /// Tries a move down, upward when negative, without making it: reports what the move would report, and where it
    /// would leave the body, with the owner put back exactly where it was.
    /// </summary>
    /// <param name="body">The body the move is tried for.</param>
    /// <param name="distance">How far the move would go, downward when positive.</param>
    /// <param name="options">The ways the move differs from the plain one.</param>
    /// <param name="rest">Where the move would have left the owner.</param>
    /// <param name="shapedAs">The rectangle the body is taken to have for the try, about its owner's place; its own when null.</param>
    public MoveResult TryDown(Collider body, Pixels distance, MoveOptions options, out PixelPosition rest, PixelRect? shapedAs = null)
    {
        ICollisionOwner owner = body.CollisionOwner;
        PixelPosition before = owner.Position;
        PixelRect shape = body.Rect;
        if (shapedAs is { } other)
            body.Rect = other;
        MoveResult result = MoveDown(body, distance, options);
        rest = owner.Position;
        owner.Position = before;
        body.Rect = shape;
        return result;
    }

    /// <summary>Whether <paramref name="body"/> rests on terrain or a solid: the pixel row below it is not free.</summary>
    public bool IsOnGround(Collider body, MoveOptions options) =>
        FindObstacle(body.RoomRect.MovedBy(0, 1), body.CollisionOwner, options).Found;

    /// <summary>Whether <paramref name="rect"/> is free of terrain and of solids other than <paramref name="owner"/>'s own.</summary>
    public bool IsFree(PixelRect rect, ICollisionOwner owner, MoveOptions options) => !FindObstacle(rect, owner, options).Found;

    /// <summary>
    /// Moves a solid's owner by <paramref name="offset"/> and carries with it every body standing on the solid. Bodies
    /// the solid moves into are pushed ahead of it. The solid itself is not stopped by anything.
    /// </summary>
    public void MoveSolid(Collider solid, PixelOffset offset)
    {
        if (solid.Role != ColliderRole.Solid)
            throw new ArgumentException($"The collider \"{solid.Name}\" is not a solid.", nameof(solid));
        ICollisionOwner owner = solid.CollisionOwner;
        PixelRect before = solid.RoomRect;
        IReadOnlyList<Collider> riders = _world.Overlapping(before.MovedBy(0, -1), ColliderRole.Body, except: owner)
            .Where(rider => rider.RoomRect.Bottom == before.Top)
            .ToList();

        owner.Position += offset;
        PixelRect after = solid.RoomRect;
        int acrossPixels = after.Left - before.Left;
        int downPixels = after.Top - before.Top;

        // A carried body moves with the solid but is still stopped by terrain, and not by the solid carrying it.
        MoveOptions carried = MoveOptions.Plain with { StoppedBySolids = false, Slopes = SlopeTreatment.Wall };
        foreach (Collider rider in riders)
        {
            if (acrossPixels != 0)
                MoveAcross(rider, Pixels.FromWhole(acrossPixels), carried);
            if (downPixels != 0)
                MoveDown(rider, Pixels.FromWhole(downPixels), carried);
        }

        foreach (Collider pushed in _world.Overlapping(after, ColliderRole.Body, except: owner))
        {
            if (riders.Contains(pushed))
                continue;
            if (acrossPixels != 0)
                MoveAcross(pushed, Pixels.FromWhole(acrossPixels), carried);
            if (downPixels != 0)
                MoveDown(pushed, Pixels.FromWhole(downPixels), carried);
        }
    }

    /// <summary>What, if anything, lies in a rectangle that would stop a move.</summary>
    private Obstacle FindObstacle(PixelRect rect, ICollisionOwner owner, MoveOptions options)
    {
        if (options.StoppedByTerrain && _world.Terrain.IsSolidIn(rect, options.Slopes))
            return new Obstacle(true, null);

        // A way out of the room is a wall, as terrain is, to a move that says so.
        if (options.StoppedByTerrain && options.StoppedByWaysOut && _world.WayOutOver(rect) is not null)
            return new Obstacle(true, null);
        if (!options.StoppedByTerrain && !options.StoppedBySolids)
            return new Obstacle(false, null);

        // A solid that stands as part of the room is met with the terrain: by what the terrain stops, whether or not
        // other solids stop it, and not by what only other solids stop.
        foreach (Collider solid in _world.Overlapping(rect, ColliderRole.Solid, except: owner))
        {
            if (solid.StandsAsTerrain ? options.StoppedByTerrain : options.StoppedBySolids)
                return new Obstacle(true, solid);
        }

        return new Obstacle(false, null);
    }

    /// <summary>
    /// How many pixels a mover stopped by sloped ground climbs to carry on across, or zero when it cannot: the least
    /// rise, up to what the options allow, that leaves it free.
    /// </summary>
    private int ClimbFor(PixelRect here, PixelRect ahead, int step, ICollisionOwner owner, MoveOptions options, Obstacle obstacle)
    {
        bool climbs = options.Slopes is SlopeTreatment.Climb or SlopeTreatment.ClimbUnderMiddle;
        if (!climbs || obstacle.Solid is not null || !_world.Terrain.HasSlopeIn(ahead))
            return 0;

        // A mover held up by its middle climbs only the slopes that hold it so: a wall or a step in its way stops it,
        // though a slope lies beside it.
        if (options.Slopes == SlopeTreatment.ClimbUnderMiddle && _world.Terrain.IsSolidIn(ahead, SlopeTreatment.IgnoreMiddleHeld))
            return 0;
        for (int rise = 1; rise <= options.ClimbPerPixel; rise++)
        {
            if (!FindObstacle(here.MovedBy(step, -rise), owner, options).Found &&
                !FindObstacle(here.MovedBy(0, -rise), owner, options).Found)
                return rise;
        }

        return 0;
    }

    /// <summary>
    /// Brings a body that has just stepped across off its ground back down onto ground close beneath it: down by the
    /// least drop, up to what the options allow, that puts ground under its feet. With no ground that close it stays in
    /// the air.
    /// </summary>
    private void FollowGroundDown(Collider body, MoveOptions options)
    {
        ICollisionOwner owner = body.CollisionOwner;
        PixelRect here = body.RoomRect;
        if (FindObstacle(here.MovedBy(0, 1), owner, options).Found)
            return;
        for (int drop = 1; drop <= options.FollowGroundDown; drop++)
        {
            if (!FindObstacle(here.MovedBy(0, drop + 1), owner, options).Found)
                continue;
            owner.Position = owner.Position with { Y = Pixels.FromWhole(owner.Position.Y.Whole + drop) };
            return;
        }
    }

    /// <summary>What stopped a move: terrain when <see cref="Solid"/> is null, otherwise that solid.</summary>
    private readonly record struct Obstacle(bool Found, Collider? Solid);
}
