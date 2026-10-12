using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static SharedSlopeHeightsVerification;

/// <summary>Checks Kraid nail contour and wall reflections across every fractional velocity word.</summary>
internal static class KraidNailBounceVerification
{
    /// <summary>Checks independent whole/fraction velocity negation and the distinct post-collision positions.</summary>
    internal static void VerifyKraidNailBounce(SuperMetroidAddressSpace rom)
    {
        var words = new ushort[32 * 32];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.BindFixtureBus(rom);
        Action<RoomEnemySlot, RoomLevelData> tick = enemies.KraidBehavior.TickKraidNailFlight;
        var nail = enemies.SlotTable.Slots[6];
        nail.Radius = new(4, 4);
        for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
        {
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            nail.VariableB = (ushort)fraction;
            nail.VariableC = 1;
            nail.VariableD = nail.VariableE = 0;
            tick(nail, level);
            AssertEqual(unchecked((ushort)-fraction), nail.VariableB, "Native contour bounce negates fractional word");
            AssertEqual(ushort.MaxValue, nail.VariableC, "Native contour bounce independently negates whole word without fractional borrow");
            AssertEqual((ushort)129, nail.Position.X.Whole, "Bounce follows the current horizontal move");
            AssertEqual((ushort)fraction, nail.Position.X.Fraction, "Bounce preserves integrated fractional position");
        }
        var wallWords = new ushort[32 * 32];
        for (int row = 0; row < 32; row++) wallWords[row * 32 + 9] = 0x8000;
        var wallRoom = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(wallWords, new byte[wallWords.Length], new ushort[wallWords.Length]), new byte[8]);
        // A wall hit must skip the contour lookup entirely, not bounce twice.
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
        {
            nail.Position = FixedPosition<RoomPixels>.FromWhole(new(140, 128));
            nail.VariableB = (ushort)fraction;
            nail.VariableC = 1;
            nail.VariableD = nail.VariableE = 0;
            tick(nail, wallRoom);
            AssertEqual(unchecked((ushort)-fraction), nail.VariableB, "Native wall bounce negates fractional word");
            AssertEqual(ushort.MaxValue, nail.VariableC, "Native wall bounce independently negates whole word");
            AssertEqual((ushort)140, nail.Position.X.Whole, "Wall collision keeps aligned center");
            AssertEqual(ushort.MaxValue, nail.Position.X.Fraction, "Wall collision retains native subpixel alignment");
        }
        Console.WriteLine("Kraid nail bounce: 131072 actual contour/wall reflections preserve native independent word negation.");
    }
}
