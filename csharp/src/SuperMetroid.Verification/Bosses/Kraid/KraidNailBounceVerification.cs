using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks that Kraid's contour and a wall reverse a flying nail's whole-pixel speed (#1620).</summary>
internal static class KraidNailBounceVerification
{
    /// <summary>A plain solid block showing block zero: the wall the nail hits.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>A flying nail moves in whole pixels, so each reflection is the plain opposite speed; the nail's positions after each.</summary>
    internal static void VerifyKraidNailBounce()
    {
        var terrain = new RoomTerrainBlock[32 * 32];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(terrain, new RoomBlockBehavior[terrain.Length], new RoomBlockAppearance[terrain.Length]), RoomLevelData.NothingBeyondGrid());
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Access.Kraid = state;
        Action<RoomEnemySlot, RoomLevelData> tick = enemies.KraidBehavior.Projectiles.TickKraidNailFlight;
        var nail = enemies.SlotTable.Slots[6];
        nail.Radius = new(4, 4);
        for (int speed = 1; speed <= 4; speed++)
        {
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity = FixedVector<RoomPixels>.FromWholePixels(new(speed, 0));
            tick(nail, level);
            AssertEqual(SignedSixteenSixteen.FromWholePixels(-speed), enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.X, "the contour reverses the nail's speed");
            AssertEqual((ushort)(128 + speed), nail.Position.X.Whole, "the bounce follows the current horizontal move");
            AssertEqual((ushort)0, nail.Position.X.Fraction, "a whole-pixel move keeps the nail on a whole pixel");
        }
        var wallTerrain = new RoomTerrainBlock[32 * 32];
        for (int row = 0; row < 32; row++) wallTerrain[row * 32 + 9] = Solid;
        var wallRoom = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(wallTerrain, new RoomBlockBehavior[wallTerrain.Length], new RoomBlockAppearance[wallTerrain.Length]), RoomLevelData.NothingBeyondGrid());
        // A wall hit must skip the contour lookup entirely, not bounce twice.
        for (int speed = 1; speed <= 4; speed++)
        {
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(140, 128));
            enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity = FixedVector<RoomPixels>.FromWholePixels(new(speed, 0));
            tick(nail, wallRoom);
            AssertEqual(SignedSixteenSixteen.FromWholePixels(-speed), enemies.KraidBehavior.Parts.NailStateOf(state, nail).NailVelocity.X, "the wall reverses the nail's speed");
            AssertEqual((ushort)140, nail.Position.X.Whole, "wall collision keeps the nail against the wall");
        }
        Console.WriteLine("Kraid nail bounce: the contour and a wall reverse whole-pixel nail speeds.");
    }
}
