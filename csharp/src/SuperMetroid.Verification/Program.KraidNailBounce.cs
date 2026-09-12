using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyKraidNailBounce(SuperMetroidAddressSpace rom)
    {
        var words = new ushort[32 * 32];
        var level = new RoomLevelData(32, 32, words, new byte[words.Length], new ushort[words.Length], new byte[8]);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, rom);
        var tick = typeof(RoomEnemySystem).GetMethod("TickKraidNailFlight", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RoomLevelData>>(enemies);
        var nail = enemies.Slots[6];
        nail.XRadius = nail.YRadius = 4;
        for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
        {
            nail.XPosition = nail.YPosition = 128;
            nail.XSubposition = nail.YSubposition = 0;
            nail.VariableB = (ushort)fraction;
            nail.VariableC = 1;
            nail.VariableD = nail.VariableE = 0;
            tick(nail, level);
            AssertEqual(unchecked((ushort)-fraction), nail.VariableB, "Native contour bounce negates fractional word");
            AssertEqual(ushort.MaxValue, nail.VariableC, "Native contour bounce independently negates whole word without fractional borrow");
            AssertEqual((ushort)129, nail.XPosition, "Bounce follows the current horizontal move");
            AssertEqual((ushort)fraction, nail.XSubposition, "Bounce preserves integrated fractional position");
        }
        var wallWords = new ushort[32 * 32];
        for (int row = 0; row < 32; row++) wallWords[row * 32 + 9] = 0x8000;
        var wallRoom = new RoomLevelData(32, 32, wallWords, new byte[wallWords.Length], new ushort[wallWords.Length], new byte[8]);
        // A wall hit must skip the contour lookup entirely, not bounce twice.
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
        {
            nail.XPosition = 140;
            nail.YPosition = 128;
            nail.XSubposition = nail.YSubposition = 0;
            nail.VariableB = (ushort)fraction;
            nail.VariableC = 1;
            nail.VariableD = nail.VariableE = 0;
            tick(nail, wallRoom);
            AssertEqual(unchecked((ushort)-fraction), nail.VariableB, "Native wall bounce negates fractional word");
            AssertEqual(ushort.MaxValue, nail.VariableC, "Native wall bounce independently negates whole word");
            AssertEqual((ushort)140, nail.XPosition, "Wall collision keeps aligned center");
            AssertEqual(ushort.MaxValue, nail.XSubposition, "Wall collision retains native subpixel alignment");
        }
        Console.WriteLine("Kraid nail bounce: 131072 actual contour/wall reflections preserve native independent word negation.");
    }
}
