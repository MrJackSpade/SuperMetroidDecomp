using SuperMetroid.Core.Bosses.Kraid;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid nail contour and wall reflections across every fractional velocity word.</summary>
internal static class KraidNailBounceVerification
{
    /// <summary>Checks independent whole/fraction velocity negation and the distinct post-collision positions.</summary>
    internal static void VerifyKraidNailBounce()
    {
        var words = new ushort[32 * 32];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
        var enemies = new RoomEnemySystem(new EnemySession());
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Kraid = state;
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, RoomLevelData> tick = enemies.KraidBehavior.TickKraidNailFlight;
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
        var wallWords = new ushort[32 * 32];
        for (int row = 0; row < 32; row++) wallWords[row * 32 + 9] = 0x8000;
        var wallRoom = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(wallWords, new byte[wallWords.Length], new ushort[wallWords.Length]), new byte[8]);
        // A wall hit must skip the contour lookup entirely, not bounce twice.
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
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
