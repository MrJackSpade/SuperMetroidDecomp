using Engine.Collision;
using Engine.Shared;

namespace Engine.Tests.Collision;

/// <summary>Checks of moving a body through terrain and solids: walls, floors, slopes, fractions and being carried.</summary>
public sealed class MoverChecks
{
    [Check]
    public void ABodyWalksAcrossOpenFloorKeepingItsFraction()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (owner, body) = room.AddBody(20, 32);
        Pixels step = Pixels.FromParts(2, Pixels.FractionsPerPixel / 2);

        MoveResult result = room.Mover.MoveAcross(body, step, MoveOptions.Plain);

        Expect.False(result.Blocked);
        Expect.Equal(Pixels.FromWhole(20) + step, owner.Position.X);
        Expect.Equal(Pixels.FromWhole(32), owner.Position.Y);
    }

    [Check]
    public void AWallStopsABodyFlushAgainstIt()
    {
        using var room = new StandInRoom("....#...", "....#...", "########");
        var (owner, body) = room.AddBody(40, 32);

        MoveResult result = room.Mover.MoveAcross(body, Pixels.FromWhole(50), MoveOptions.Plain);

        Expect.True(result.Blocked);
        Expect.True(result.BlockedByTerrain);
        // The wall's left face is at pixel 64, and the body reaches 4 to the right of its origin.
        Expect.Equal(Pixels.FromWhole(60), owner.Position.X);
        Expect.Equal(Pixels.FromWhole(20), result.Moved);
    }

    [Check]
    public void ABodyMovingLeftIsStoppedTheSameWay()
    {
        using var room = new StandInRoom("#.......", "#.......", "########");
        var (owner, body) = room.AddBody(40, 32);
        MoveResult result = room.Mover.MoveAcross(body, Pixels.FromWhole(-50), MoveOptions.Plain);
        Expect.True(result.Blocked);
        Expect.Equal(Pixels.FromWhole(20), owner.Position.X);
    }

    [Check]
    public void AFallingBodyLandsOnTheFloor()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (owner, body) = room.AddBody(40, 20);
        Expect.False(room.Mover.IsOnGround(body, MoveOptions.Plain));

        MoveResult result = room.Mover.MoveDown(body, Pixels.FromWhole(100), MoveOptions.Plain);

        Expect.True(result.Blocked);
        Expect.Equal(Pixels.FromWhole(32), owner.Position.Y);
        Expect.True(room.Mover.IsOnGround(body, MoveOptions.Plain));
    }

    [Check]
    public void ARisingBodyIsStoppedByTheCeiling()
    {
        using var room = new StandInRoom("########", "........", "........", "########");
        var (owner, body) = room.AddBody(40, 48);
        MoveResult result = room.Mover.MoveDown(body, Pixels.FromWhole(-100), MoveOptions.Plain);
        Expect.True(result.Blocked);
        // The ceiling's underside is at pixel 16 and the body is 16 tall.
        Expect.Equal(Pixels.FromWhole(32), owner.Position.Y);
    }

    [Check]
    public void ABodyWalkingIntoASlopeClimbsIt()
    {
        using var room = new StandInRoom("........", "....../#", "########");
        var (owner, body) = room.AddBody(80, 32, halfWidth: 2);

        MoveResult result = room.Mover.MoveAcross(body, Pixels.FromWhole(20), MoveOptions.Plain);

        Expect.False(result.Blocked);
        Expect.Equal(Pixels.FromWhole(100), owner.Position.X);
        Expect.True(owner.Position.Y < Pixels.FromWhole(32));
        Expect.True(room.Mover.IsOnGround(body, MoveOptions.Plain));
    }

    [Check]
    public void ASlopeTreatedAsAWallStopsTheBody()
    {
        using var room = new StandInRoom("........", "....../#", "########");
        var (owner, body) = room.AddBody(80, 32, halfWidth: 2);
        MoveResult result = room.Mover.MoveAcross(body, Pixels.FromWhole(20), MoveOptions.Plain with { Slopes = SlopeTreatment.Wall });
        Expect.True(result.Blocked);
        Expect.Equal(Pixels.FromWhole(32), owner.Position.Y);
    }

    [Check]
    public void AnIgnoredSlopeIsNotThere()
    {
        using var room = new StandInRoom("........", "....../.", "########");
        var (owner, body) = room.AddBody(80, 32, halfWidth: 2);
        MoveResult result = room.Mover.MoveAcross(body, Pixels.FromWhole(30), MoveOptions.Plain with { Slopes = SlopeTreatment.Ignore });
        Expect.False(result.Blocked);
        Expect.Equal(Pixels.FromWhole(32), owner.Position.Y);
    }

    [Check]
    public void ABodyWalkingDownASlopeStaysOnTheGroundWhenAskedTo()
    {
        using var room = new StandInRoom("........", "..#\\....", "########");
        var (owner, body) = room.AddBody(46, 16, halfWidth: 2);
        Expect.True(room.Mover.IsOnGround(body, MoveOptions.Plain));

        room.Mover.MoveAcross(body, Pixels.FromWhole(10), MoveOptions.Plain with { FollowGroundDown = 2 });

        Expect.True(owner.Position.Y > Pixels.FromWhole(16));
        Expect.True(room.Mover.IsOnGround(body, MoveOptions.Plain));
    }

    [Check]
    public void WithoutFollowingTheGroundABodyWalksOffIntoTheAir()
    {
        using var room = new StandInRoom("........", "..#\\....", "########");
        var (owner, body) = room.AddBody(46, 16, halfWidth: 2);
        room.Mover.MoveAcross(body, Pixels.FromWhole(10), MoveOptions.Plain);
        Expect.Equal(Pixels.FromWhole(16), owner.Position.Y);
        Expect.False(room.Mover.IsOnGround(body, MoveOptions.Plain));
    }

    [Check]
    public void TheRoomsEdgesAreSolid()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (owner, body) = room.AddBody(100, 32);
        MoveResult result = room.Mover.MoveAcross(body, Pixels.FromWhole(500), MoveOptions.Plain);
        Expect.True(result.Blocked);
        Expect.Equal(Pixels.FromWhole(124), owner.Position.X);
    }

    [Check]
    public void ASolidStopsABodyAndIsReportedAsWhatStoppedIt()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (owner, body) = room.AddBody(20, 32);
        var (_, solid) = room.AddSolid(new PixelRect(60, 16, 16, 16));

        MoveResult result = room.Mover.MoveAcross(body, Pixels.FromWhole(100), MoveOptions.Plain);

        Expect.True(result.Blocked);
        Expect.False(result.BlockedByTerrain);
        Expect.True(ReferenceEquals(solid, result.BlockedBy));
        Expect.Equal(Pixels.FromWhole(56), owner.Position.X);
    }

    [Check]
    public void ADisabledSolidStopsNothing()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (owner, body) = room.AddBody(20, 32);
        var (_, solid) = room.AddSolid(new PixelRect(60, 16, 16, 16));
        solid.Enabled = false;
        Expect.False(room.Mover.MoveAcross(body, Pixels.FromWhole(60), MoveOptions.Plain).Blocked);
        Expect.Equal(Pixels.FromWhole(80), owner.Position.X);
    }

    [Check]
    public void ABodyCanStandOnASolid()
    {
        using var room = new StandInRoom("........", "........", "........", "########");
        var (owner, body) = room.AddBody(68, 20);
        room.AddSolid(new PixelRect(60, 30, 16, 4));
        room.Mover.MoveDown(body, Pixels.FromWhole(100), MoveOptions.Plain);
        Expect.Equal(Pixels.FromWhole(30), owner.Position.Y);
        Expect.True(room.Mover.IsOnGround(body, MoveOptions.Plain));
    }

    [Check]
    public void AMovingSolidCarriesWhatStandsOnIt()
    {
        using var room = new StandInRoom("........", "........", "........", "########");
        var (rider, _) = room.AddBody(68, 30);
        var (bystander, _) = room.AddBody(20, 48);
        var (platform, solid) = room.AddSolid(new PixelRect(60, 30, 16, 4));

        room.Mover.MoveSolid(solid, PixelOffset.FromWhole(10, -3));

        Expect.Equal(PixelPosition.FromWhole(70, 27), platform.Position);
        Expect.Equal(PixelPosition.FromWhole(78, 27), rider.Position);
        Expect.Equal(PixelPosition.FromWhole(20, 48), bystander.Position);
    }

    [Check]
    public void ACarriedBodyIsStillStoppedByAWall()
    {
        using var room = new StandInRoom(".....#..", ".....#..", "........", "########");
        var (rider, _) = room.AddBody(70, 40);
        var (_, solid) = room.AddSolid(new PixelRect(60, 40, 16, 4));

        room.Mover.MoveSolid(solid, PixelOffset.FromWhole(20, 0));

        // The wall's left face is at pixel 80 and the rider reaches 4 to the right of its origin.
        Expect.Equal(Pixels.FromWhole(76), rider.Position.X);
    }

    [Check]
    public void AMovingSolidPushesABodyInItsWay()
    {
        using var room = new StandInRoom("........", "........", "########");
        var (pushed, _) = room.AddBody(70, 32);
        var (_, solid) = room.AddSolid(new PixelRect(40, 16, 16, 16));

        room.Mover.MoveSolid(solid, PixelOffset.FromWhole(15, 0));

        // The solid's right face moved to pixel 71; the body it overlapped is carried the same 15 pixels on.
        Expect.Equal(Pixels.FromWhole(85), pushed.Position.X);
    }
}
