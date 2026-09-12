using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledBotwoonSpeeds(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (byte phase = 0; phase < 3; phase++)
        {
            var speed = BotwoonSpeedDefinitions.ForHealthPhase(phase);
            AssertEqual(Word(0xb394bb + phase * 4), speed.MovementSpeed, "Botwoon native movement speed");
            AssertEqual(Word(0xb394bd + phase * 4), speed.SegmentSpacingBytes, "Botwoon native body spacing");
            AssertEqual(Word(0xb39e77 + phase * 2), speed.SpitSpeed, "Botwoon native spit speed");
        }
        var update = typeof(RoomEnemySystem).GetMethod("UpdateBotwoonHealthPhase", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>();
        var head = new RoomEnemySystem().Slots[0];
        var state = new BotwoonEnemyState(head) { HalfHealth = 1500, QuarterHealth = 750 };
        for (int health = 0; health <= 3000; health++)
        {
            head.Health = (ushort)health;
            byte phase = health == 0 || health >= 1500 ? (byte)0 : health >= 750 ? (byte)1 : (byte)2;
            state.InsideHole = false;
            update(head, state);
            AssertEqual(phase, state.HealthPhase, "Botwoon real phase selection without bus");
            AssertEqual(Word(0xb394bb + phase * 4), state.Speed, "Botwoon real phase speed");
            AssertEqual(Word(0xb394bd + phase * 4), state.SegmentSpacingBytes, "Botwoon real phase spacing");
            state.InsideHole = true;
            head.Health = (ushort)(3000 - health);
            update(head, state);
            AssertEqual(phase, state.HealthPhase, "Botwoon preserves phase inside hole");
        }
        AssertThrows<InvalidDataException>(() => BotwoonSpeedDefinitions.ForHealthPhase(3), "Botwoon invalid phase");
        Console.WriteLine("Botwoon compiled speeds: nine native words and 3,001 real health transitions match; hidden phases remain held.");
    }
}
