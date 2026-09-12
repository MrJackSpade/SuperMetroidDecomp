using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledCrawlerSpeeds(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<T>();
        var reset = Method<Action<RoomEnemySlot>>("ResetCrawlerVelocitiesFromProperties");
        var setYard = Method<Action<RoomEnemySlot, YardEnemyState, ushort>>("SetYardCrawlingVelocities");
        var slot = new RoomEnemySystem().Slots[0];
        var yard = new YardEnemyState(slot);
        for (ushort parameter = 0; parameter < 32; parameter++)
        {
            ushort expected = Word(0xa3e5f0 + parameter * 2);
            AssertEqual(expected, Word(0xa3cca2 + parameter * 2), "Yard/crawler native speed mirror");
            AssertEqual(expected, CrawlerSpeedDefinitions.ForParameter(parameter), "Compiled crawling speed");
            slot.Parameter1 = parameter;
            for (ushort property = 0; property < 4; property++)
            {
                slot.Properties = (ushort)(0xa000 | property);
                slot.VariableA = 123;
                slot.VariableB = 456;
                reset(slot);
                AssertEqual(property == 0 ? unchecked((ushort)-expected) : expected, slot.VariableA, "Real crawler X reset without bus");
                AssertEqual(property == 2 ? unchecked((ushort)-expected) : expected, slot.VariableB, "Real crawler Y reset without bus");
            }
            for (ushort direction = 0; direction < 8; direction++)
            {
                setYard(slot, yard, direction);
                int address = 0xa3cd82 + direction * 8;
                AssertEqual(unchecked((ushort)((expected ^ Word(address)) + Word(address + 2))), yard.CrawlingXVelocity, "Real Yard X reset without bus");
                AssertEqual(unchecked((ushort)((expected ^ Word(address + 4)) + Word(address + 6))), yard.CrawlingYVelocity, "Real Yard Y reset without bus");
            }
        }
        // The preserve sentinel skips the table, not the subsequent native sign application.
        slot.Parameter1 = CrawlerSpeedDefinitions.PreserveVelocity;
        for (int magnitude = 0; magnitude <= ushort.MaxValue; magnitude++)
        {
            for (ushort property = 0; property < 4; property++)
            {
                slot.Properties = property;
                slot.VariableA = (ushort)magnitude;
                slot.VariableB = (ushort)~magnitude;
                reset(slot);
                AssertEqual(property == 0 ? unchecked((ushort)-magnitude) : (ushort)magnitude, slot.VariableA, "Crawler sentinel X sign");
                AssertEqual(property == 2 ? unchecked((ushort)-(ushort)~magnitude) : (ushort)~magnitude, slot.VariableB, "Crawler sentinel Y sign");
            }
            for (ushort direction = 0; direction < 8; direction++)
            {
                int address = 0xa3cd82 + direction * 8;
                var actual = YardVelocityDefinitions.Apply((ushort)magnitude, direction);
                AssertEqual(unchecked((ushort)((magnitude ^ Word(address)) + Word(address + 2))), actual.X, "Yard exhaustive native X sign");
                AssertEqual(unchecked((ushort)((magnitude ^ Word(address + 4)) + Word(address + 6))), actual.Y, "Yard exhaustive native Y sign");
            }
        }
        AssertThrows<InvalidDataException>(() => CrawlerSpeedDefinitions.ForParameter(32), "Crawler invalid speed");
        AssertThrows<InvalidDataException>(() => YardVelocityDefinitions.Apply(1, 8), "Yard invalid direction");
        Console.WriteLine("Compiled crawling speeds: both 32-word copies, 384 real resets, 262144 sentinel resets and 524288 Yard sign pairs match native values without a bus.");
    }
}
