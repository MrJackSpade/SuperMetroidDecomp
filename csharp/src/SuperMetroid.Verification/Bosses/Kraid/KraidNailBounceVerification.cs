using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks that Kraid's contour and a wall reverse a flying nail's whole-pixel speed (#1620).</summary>
internal static class KraidNailBounceVerification
{
    /// <summary>A plain solid block showing block zero: the wall the nail hits.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>A flying nail moves in whole pixels, so each reflection is the plain opposite speed; the nail's positions after each.</summary>
    internal static void VerifyKraidNailBounce()
    {
        // An open room holding Kraid where his room's population places him.
        var terrain = new RoomTerrainBlock[64 * 64];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(64, 64), new RoomLevelPlanes(terrain, new RoomBlockBehavior[terrain.Length], new RoomBlockAppearance[terrain.Length]), RoomLevelData.NothingBeyondGrid(), []);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        _ = KraidPartsFixtures.PlaceKraid(enemies);
        Action<RoomEnemyPlace, RoomLevelData> tick = (place, room) => enemies.KraidBehavior.Projectiles.TickKraidNailFlight(place, ((KraidNailState)place.RequireSpeciesState<KraidPartState>()), room);
        var nail = enemies.Places.InUpdateOrder[6];
        nail.Radius = new(4, 4);
        // His body's contour stands where his body stands: the nail flies beside his lowest ledge, one move short of it.
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        KraidNailLedge lowest = KraidNailContourDefinitions.Ledges[0];
        int contourX = body.Position.X.Whole + lowest.Left;
        ushort nailY = (ushort)(body.Position.Y.Whole + lowest.Top + 1);
        for (int speed = 1; speed <= 4; speed++)
        {
            ushort start = (ushort)(contourX - nail.Radius.X - speed);
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(start, nailY));
            ((KraidNailState)nail.RequireSpeciesState<KraidPartState>()).NailVelocity = FixedVector<RoomPixels>.FromWholePixels(new(speed, 0));
            tick(nail, level);
            AssertEqual(SignedSixteenSixteen.FromWholePixels(-speed), ((KraidNailState)nail.RequireSpeciesState<KraidPartState>()).NailVelocity.X, "the contour reverses the nail's speed");
            AssertEqual((ushort)(start + speed), nail.Position.X.Whole, "the bounce follows the current horizontal move");
            AssertEqual((ushort)0, nail.Position.X.Fraction, "a whole-pixel move keeps the nail on a whole pixel");
        }
        var wallTerrain = new RoomTerrainBlock[32 * 32];
        for (int row = 0; row < 32; row++) wallTerrain[row * 32 + 9] = Solid;
        var wallRoom = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(wallTerrain, new RoomBlockBehavior[wallTerrain.Length], new RoomBlockAppearance[wallTerrain.Length]), RoomLevelData.NothingBeyondGrid(), []);
        // A wall hit must skip the contour lookup entirely, not bounce twice.
        for (int speed = 1; speed <= 4; speed++)
        {
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(140, 128));
            ((KraidNailState)nail.RequireSpeciesState<KraidPartState>()).NailVelocity = FixedVector<RoomPixels>.FromWholePixels(new(speed, 0));
            tick(nail, wallRoom);
            AssertEqual(SignedSixteenSixteen.FromWholePixels(-speed), ((KraidNailState)nail.RequireSpeciesState<KraidPartState>()).NailVelocity.X, "the wall reverses the nail's speed");
            AssertEqual((ushort)140, nail.Position.X.Whole, "wall collision keeps the nail against the wall");
        }
        Console.WriteLine("Kraid nail bounce: the contour and a wall reverse whole-pixel nail speeds.");
    }
}
