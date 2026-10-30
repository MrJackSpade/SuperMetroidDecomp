using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid nail contour and wall reflections across every fractional velocity word.</summary>
internal static class KraidNailBounceVerification
{
    /// <summary>A plain solid block showing block zero: the wall the nail hits.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>Checks independent whole/fraction velocity negation and the distinct post-collision positions.</summary>
    internal static void VerifyKraidNailBounce()
    {
        var terrain = new RoomTerrainBlock[32 * 32];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(terrain, new RoomBlockBehavior[terrain.Length], new RoomBlockAppearance[terrain.Length]));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Access.Kraid = state;
        Action<RoomEnemySlot, RoomLevelData> tick = enemies.KraidBehavior.Projectiles.TickKraidNailFlight;
        var nail = enemies.SlotTable.Slots[6];
        nail.Radius = new(4, 4);
        for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
        {
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            state.Parts[nail.SlotIndex].NailVelocity = FixedVector<RoomPixels>.FromWords(1, (ushort)fraction, 0, 0);
            tick(nail, level);
            AssertEqual(unchecked((ushort)-fraction), state.Parts[nail.SlotIndex].NailVelocity.X.Fraction, "Native contour bounce negates fractional word");
            AssertEqual(ushort.MaxValue, state.Parts[nail.SlotIndex].NailVelocity.X.WholeWord, "Native contour bounce independently negates whole word without fractional borrow");
            AssertEqual((ushort)129, nail.Position.X.Whole, "Bounce follows the current horizontal move");
            AssertEqual((ushort)fraction, nail.Position.X.Fraction, "Bounce preserves integrated fractional position");
        }
        var wallTerrain = new RoomTerrainBlock[32 * 32];
        for (int row = 0; row < 32; row++) wallTerrain[row * 32 + 9] = Solid;
        var wallRoom = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(wallTerrain, new RoomBlockBehavior[wallTerrain.Length], new RoomBlockAppearance[wallTerrain.Length]));
        // A wall hit must skip the contour lookup entirely, not bounce twice.
        for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
        {
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(140, 128));
            state.Parts[nail.SlotIndex].NailVelocity = FixedVector<RoomPixels>.FromWords(1, (ushort)fraction, 0, 0);
            tick(nail, wallRoom);
            AssertEqual(unchecked((ushort)-fraction), state.Parts[nail.SlotIndex].NailVelocity.X.Fraction, "Native wall bounce negates fractional word");
            AssertEqual(ushort.MaxValue, state.Parts[nail.SlotIndex].NailVelocity.X.WholeWord, "Native wall bounce independently negates whole word");
            AssertEqual((ushort)140, nail.Position.X.Whole, "Wall collision keeps aligned center");
            AssertEqual(ushort.MaxValue, nail.Position.X.Fraction, "Wall collision retains native subpixel alignment");
        }
        Console.WriteLine("Kraid nail bounce: 131072 actual contour/wall reflections preserve native independent word negation.");
    }
}
