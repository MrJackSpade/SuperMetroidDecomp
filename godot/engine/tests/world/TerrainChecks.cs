using Engine.Collision;
using Engine.Shared;
using Engine.Tests.Collision;
using Engine.World;

namespace Engine.Tests.World;

/// <summary>Checks of terrain: its solid pixels, its slopes, its blocks' rules and its patches.</summary>
public sealed class TerrainChecks
{
    private static readonly Hit Shot = new(1, HitSide.Player, new HitKind("shot"));
    private static readonly Hit Bomb = new(1, HitSide.Player, new HitKind("bomb"));

    [Check]
    public void SolidAndEmptyTilesAreSolidAndEmpty()
    {
        using var room = new StandInRoom("#.", "..");
        Expect.True(room.Terrain.IsSolidIn(new PixelRect(15, 15, 1, 1), SlopeTreatment.Climb));
        Expect.False(room.Terrain.IsSolidIn(new PixelRect(16, 0, 16, 32), SlopeTreatment.Climb));
        Expect.Equal(32, room.Terrain.WidthInPixels);
    }

    [Check]
    public void ASlopeIsSolidOnlyBelowItsSurface()
    {
        using var room = new StandInRoom("/.", "##");
        // The slope rises to the right: its first column is one pixel high, its last sixteen.
        Expect.True(room.Terrain.IsSolidIn(new PixelRect(0, 15, 1, 1), SlopeTreatment.Climb));
        Expect.False(room.Terrain.IsSolidIn(new PixelRect(0, 14, 1, 1), SlopeTreatment.Climb));
        Expect.True(room.Terrain.IsSolidIn(new PixelRect(15, 0, 1, 1), SlopeTreatment.Climb));
        Expect.True(room.Terrain.HasSlopeIn(new PixelRect(0, 0, 16, 16)));
        Expect.False(room.Terrain.HasSlopeIn(new PixelRect(0, 0, 1, 14)));
        Expect.False(room.Terrain.IsSolidIn(new PixelRect(0, 0, 16, 16), SlopeTreatment.Ignore));
    }

    [Check]
    public void AHitOfTheRightKindBreaksABlockAndAnotherKindDoesNot()
    {
        using var room = new StandInRoom("S.", "##");
        var area = new PixelRect(0, 0, 16, 16);
        Expect.True(room.Terrain.IsSolidIn(area, SlopeTreatment.Climb));

        Expect.Equal(0, room.Terrain.TakeHit(Bomb, area).Count);
        Expect.True(room.Terrain.IsSolidIn(area, SlopeTreatment.Climb));

        Expect.Equal(1, room.Terrain.TakeHit(Shot, area).Count);
        Expect.True(room.Terrain.Blocks[0].Broken);

        // Broken, it stands in the way until the terrain's own step.
        Expect.True(room.Terrain.IsSolidIn(area, SlopeTreatment.Climb));
        room.Terrain.Step(_ => false);
        Expect.False(room.Terrain.IsSolidIn(area, SlopeTreatment.Climb));
    }

    [Check]
    public void AHitThatOpensTheWayAtOnceLeavesNothingStanding()
    {
        using var room = new StandInRoom("S.", "##");
        var area = new PixelRect(0, 0, 16, 16);

        Expect.Equal(1, room.Terrain.TakeHit(Shot, area, atOnce: true).Count);
        Expect.False(room.Terrain.IsSolidIn(area, SlopeTreatment.Climb));
    }

    [Check]
    public void ABlockThatRegrowsComesBackOnceItsSpaceIsClear()
    {
        using var room = new StandInRoom("R.", "##");
        var area = new PixelRect(0, 0, 16, 16);
        room.Terrain.TakeHit(Shot, area);

        for (int update = 0; update < 3; update++)
            room.Terrain.Step(_ => false);
        Expect.True(room.Terrain.Blocks[0].Broken);

        room.Terrain.Step(_ => true);
        Expect.True(room.Terrain.Blocks[0].Broken);

        room.Terrain.Step(_ => false);
        Expect.False(room.Terrain.Blocks[0].Broken);
        Expect.True(room.Terrain.IsSolidIn(area, SlopeTreatment.Climb));
    }

    [Check]
    public void ABlockThatDoesNotRegrowStaysBroken()
    {
        using var room = new StandInRoom("S.", "##");
        room.Terrain.TakeHit(Shot, new PixelRect(0, 0, 16, 16));
        for (int update = 0; update < 100; update++)
            room.Terrain.Step(_ => false);
        Expect.True(room.Terrain.Blocks[0].Broken);
    }

    [Check]
    public void ABlockGivesWayAShortWhileAfterBeingStoodOn()
    {
        using var room = new StandInRoom("..", "C#");
        var feet = new PixelRect(4, 0, 8, 16);
        room.Terrain.StandOn(feet);

        // It lasts two updates, the one it is stood on and the next.
        room.Terrain.Step(_ => false);
        Expect.False(room.Terrain.Blocks[0].Broken);
        room.Terrain.Step(_ => false);
        Expect.True(room.Terrain.Blocks[0].Broken);
    }

    [Check]
    public void AHiddenBlockIsRevealedByItsKindAndStillBlocksWhileHidden()
    {
        using var room = new StandInRoom("H.", "##");
        var area = new PixelRect(0, 0, 16, 16);
        Block block = room.Terrain.Blocks[0];
        Expect.True(block.Hidden);
        Expect.True(room.Terrain.IsSolidIn(area, SlopeTreatment.Climb));

        room.Terrain.TakeHit(Bomb, area);
        Expect.False(block.Hidden);
        Expect.False(block.Broken);
    }

    [Check]
    public void APatchReplacesTilesAndCanBeTakenBack()
    {
        using var room = new StandInRoom("#S", "##");
        var corner = new PixelRect(0, 0, 16, 16);
        room.Terrain.TakeHit(Shot, new PixelRect(16, 0, 16, 16));

        room.Terrain.ApplyPatch("top left opened");
        Expect.True(room.Terrain.IsPatchApplied("top left opened"));
        Expect.False(room.Terrain.IsSolidIn(corner, SlopeTreatment.Climb));
        // A block the patch did not touch keeps its state.
        Expect.True(room.Terrain.Blocks[0].Broken);

        room.Terrain.RevertPatch("top left opened");
        Expect.True(room.Terrain.IsSolidIn(corner, SlopeTreatment.Climb));
        Expect.Throws<ArgumentException>(() => room.Terrain.ApplyPatch("no such patch"));
    }
}
