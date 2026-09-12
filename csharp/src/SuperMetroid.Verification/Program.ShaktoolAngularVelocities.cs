using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledShaktoolAngularVelocities(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        var states = (ShaktoolSegmentState?[])typeof(RoomEnemySystem)
            .GetField("_shaktoolSegments", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(enemies)!;
        for (int index = 0; index < 7; index++)
        {
            ushort speed = Word(0xaadee9 + index * 2);
            AssertEqual((ushort)0, Word(0xaadef7 + index * 2), "Shaktool native initialization subtrahend");
            AssertEqual(speed, ShaktoolAngularVelocityDefinitions.ForSegment(index), "Shaktool native angular speed");
            var slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ShaktoolDefinition;
            states[index] = new ShaktoolSegmentState(slot) { OwnerNativeIndex = 0 };
        }
        var synchronize = typeof(RoomEnemySystem).GetMethod("SynchronizeShaktoolOrbitTargets", BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        // Every possible target, rotating the caller across all seven physical segments.
        // No cartridge is attached: synchronization must only touch live group state.
        for (int target = 0; target <= ushort.MaxValue; target++)
        {
            synchronize(enemies.Slots[target % 7], (ushort)target);
            for (int index = 0; index < 7; index++)
            {
                AssertEqual((ushort)target, states[index]!.OrbitAngle, "Shaktool real synchronized angle without bus");
                AssertEqual(Word(0xaadee9 + index * 2), states[index]!.AngularVelocity, "Shaktool real synchronized velocity without bus");
            }
        }
        AssertThrows<InvalidDataException>(() => ShaktoolAngularVelocityDefinitions.ForSegment(-1), "Shaktool negative segment");
        AssertThrows<InvalidDataException>(() => ShaktoolAngularVelocityDefinitions.ForSegment(7), "Shaktool excessive segment");
        Console.WriteLine("Shaktool angular definitions: all 14 native words and 65536 real seven-segment target synchronizations match without a bus.");
    }
}
