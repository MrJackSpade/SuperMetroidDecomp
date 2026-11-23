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
    public MoveResult MoveAcross(Collider body, Pixels distance, MoveOptions options)
    {
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

    /// <summary>Whether <paramref name="body"/> rests on terrain or a solid: the pixel row below it is not free.</summary>
    public bool IsOnGround(Collider body, MoveOptions options) =>
        FindObstacle(body.RoomRect.MovedBy(0, 1), body.CollisionOwner, options).Found;

    /// <summary>Whether <paramref name="body"/> is against terrain or a solid on the given side.</summary>
    public bool IsAgainstWall(Collider body, Facing side, MoveOptions options) =>
        FindObstacle(body.RoomRect.MovedBy(side == Facing.Left ? -1 : 1, 0), body.CollisionOwner, options).Found;

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
        if (options.StoppedBySolids)
        {
            IReadOnlyList<Collider> solids = _world.Overlapping(rect, ColliderRole.Solid, except: owner);
            if (solids.Count > 0)
                return new Obstacle(true, solids[0]);
        }

        return new Obstacle(false, null);
    }

    /// <summary>
    /// How many pixels a mover stopped by sloped ground climbs to carry on across, or zero when it cannot: the least
    /// rise, up to what the options allow, that leaves it free.
    /// </summary>
    private int ClimbFor(PixelRect here, PixelRect ahead, int step, ICollisionOwner owner, MoveOptions options, Obstacle obstacle)
    {
        if (options.Slopes != SlopeTreatment.Climb || obstacle.Solid is not null || !_world.Terrain.HasSlopeIn(ahead))
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
