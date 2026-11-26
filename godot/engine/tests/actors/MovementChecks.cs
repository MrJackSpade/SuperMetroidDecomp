using Engine.Actors.Movement;
using Engine.Shared;
using Engine.Tests.World;
using Engine.World;

namespace Engine.Tests.Actors;

/// <summary>Checks of the shared ways of moving, each as its mover would be seen to move.</summary>
public sealed class MovementChecks
{
    private static readonly string[] Boxed =
    [
        "################", "#..............#", "#..............#", "#..............#", "#..............#", "################",
    ];

    [Check]
    public void APatrolTurnsAroundAtAWall()
    {
        using var world = new StandInWorld(Boxed).StartAt(40, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Walker", StartsFacing = Facing.Right }, 200, 72);
        world.Setup();
        var patrol = new Patrol { Speed = Pixels.FromWhole(2) };

        bool hitWall = false;
        for (int update = 0; update < 40 && !hitWall; update++)
            hitWall = patrol.Step(enemy).HitWall;

        Expect.True(hitWall);
        Expect.Equal(Facing.Left, enemy.Facing);
        // The right wall's face is at pixel 240 and the enemy reaches 8 to the right of its middle.
        Expect.Equal(Pixels.FromWhole(232), enemy.Position.X);
    }

    [Check]
    public void AWalkingPatrolTurnsAtTheEdgeOfItsGround()
    {
        using var world = new StandInWorld(
            "................", "................", "................", "....######......", "................", "################")
            .StartAt(20, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Walker", StartsFacing = Facing.Right }, 100, 40);
        world.Setup();
        var patrol = new Patrol { Speed = Pixels.FromWhole(1) };

        bool turned = false;
        for (int update = 0; update < 120 && !turned; update++)
            turned = patrol.Step(enemy).ReachedEdge;

        Expect.True(turned);
        Expect.Equal(Facing.Left, enemy.Facing);
        Expect.Equal(Pixels.FromWhole(40), enemy.Position.Y);
        Expect.True(enemy.Position.X < Pixels.FromWhole(160));
    }

    [Check]
    public void AHopLeapsLandsAndWaits()
    {
        using var world = new StandInWorld(Boxed).StartAt(40, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Hopper", StartsFacing = Facing.Right }, 100, 72);
        world.Setup();
        var hop = new Hop { WaitUpdates = 5, AcrossSpeed = Pixels.FromWhole(1), UpSpeed = Pixels.FromWhole(3) };

        Pixels highest = enemy.Position.Y;
        bool landed = false;
        for (int update = 0; update < 200 && !landed; update++)
        {
            landed = hop.Step(enemy).Finished;
            if (enemy.Position.Y < highest)
                highest = enemy.Position.Y;
        }

        Expect.True(landed);
        Expect.True(highest < Pixels.FromWhole(60));

        // It lands on the pixel it left, at that pixel's far end: a hop's moves are tried where they end.
        Expect.Equal(72, enemy.Position.Y.Whole);
        Expect.True(enemy.Position.X > Pixels.FromWhole(100));
        Expect.False(hop.IsInAir);
    }

    [Check]
    public void ADiveWaitsForItsTargetThenDropsToTheFloor()
    {
        using var world = new StandInWorld(Boxed).StartAt(40, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Diver" }, 200, 30);
        world.Setup();
        var dive = new Dive { Reach = 20, DiveSpeed = Pixels.FromWhole(3) };

        for (int update = 0; update < 10; update++)
            dive.Step(enemy, PixelPosition.FromWhole(40, 80));
        Expect.Equal(Pixels.FromWhole(30), enemy.Position.Y);

        bool landed = false;
        for (int update = 0; update < 60 && !landed; update++)
            landed = dive.Step(enemy, PixelPosition.FromWhole(195, 80)).HitFloor;
        Expect.True(landed);
        Expect.Equal(Pixels.FromWhole(72), enemy.Position.Y);
    }

    [Check]
    public void AnOrbitStaysAtItsDistanceFromItsCentre()
    {
        using var world = new StandInWorld(Boxed).StartAt(40, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Circler" }, 100, 48);
        world.Setup();
        var orbit = new Orbit { Radius = Pixels.FromWhole(20), AngleStep = 64 };
        PixelPosition centre = PixelPosition.FromWhole(120, 48);

        orbit.Step(enemy, centre);
        Expect.Equal(PixelPosition.FromWhole(120, 68), enemy.Position);
        orbit.Step(enemy, centre);
        Expect.Equal(PixelPosition.FromWhole(100, 48), enemy.Position);
        orbit.Step(enemy, centre);
        Expect.Equal(PixelPosition.FromWhole(120, 28), enemy.Position);
        orbit.Step(enemy, centre);
        Expect.Equal(PixelPosition.FromWhole(140, 48), enemy.Position);
    }

    [Check]
    public void ABounceReflectsOffWallsAndFloors()
    {
        using var world = new StandInWorld(Boxed).StartAt(40, 80);
        var enemy = world.Place(new StandInEnemy { Name = "Bouncer" }, 220, 60);
        world.Setup();
        enemy.Velocity = new PixelVelocity(Pixels.FromWhole(3), Pixels.FromWhole(3));
        var bounce = new Bounce();

        bool hitWall = false, hitFloor = false;
        for (int update = 0; update < 20; update++)
        {
            MoveReport report = bounce.Step(enemy);
            hitWall |= report.HitWall;
            hitFloor |= report.HitFloor;
        }

        Expect.True(hitWall);
        Expect.True(hitFloor);
        Expect.True(enemy.Velocity.X.IsNegative);
        Expect.True(enemy.Velocity.Y.IsNegative);
    }


    [Check]
    public void ASurfaceCrawlGoesRoundTheOutsideOfABlock()
    {
        // A single solid tile in open space, at pixels 64 through 79 across and 32 through 47 down.
        using var world = new StandInWorld(
            "................", "................", "....#...........", "................", "................", "################")
            .StartAt(20, 80);
        var enemy = new StandInEnemy { Name = "Crawler" };
        world.Place(enemy, 72, 28);
        world.Setup();
        // An 8 by 8 body sitting on top of the tile.
        foreach (Engine.Collision.Collider collider in EntityNode.CollidersOf(enemy))
            collider.Rect = new PixelRect(-4, -4, 8, 8);
        var crawl = new SurfaceCrawl { Speed = Pixels.FromWhole(1) };
        crawl.Start(SurfaceCrawl.Clings.Below, clockwise: true);

        var sides = new List<SurfaceCrawl.Clings> { crawl.Side };
        for (int update = 0; update < 120; update++)
        {
            crawl.Step(enemy);
            if (sides[^1] != crawl.Side)
                sides.Add(crawl.Side);
            // It never leaves the tile's surface by more than its own size.
            Expect.True(enemy.Position.X >= Pixels.FromWhole(56) && enemy.Position.X <= Pixels.FromWhole(88));
            Expect.True(enemy.Position.Y >= Pixels.FromWhole(24) && enemy.Position.Y <= Pixels.FromWhole(56));
        }

        // Going round clockwise from the top it clings in turn to the tile's right face, underside, left face and top.
        Expect.True(sides.Count >= 5);
        Expect.Equal(SurfaceCrawl.Clings.Left, sides[1]);
        Expect.Equal(SurfaceCrawl.Clings.Above, sides[2]);
        Expect.Equal(SurfaceCrawl.Clings.Right, sides[3]);
        Expect.Equal(SurfaceCrawl.Clings.Below, sides[4]);
    }
}
