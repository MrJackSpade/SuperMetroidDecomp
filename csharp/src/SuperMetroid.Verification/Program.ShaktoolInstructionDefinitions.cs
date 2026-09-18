using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            AssertEqual(Word(0xaadd15 + (bucket >> 5) * 2),
                ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                $"Shaktool orientation list {bucket:X2}");
        }
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(Word(0xaadf13 + index * 2),
                ShaktoolInstructionDefinitions.CollisionForSegment(index),
                $"Shaktool collision list {index}");
            AssertEqual(Word(0xaadf21 + index * 2),
                ShaktoolInstructionDefinitions.AttackForSegment(index),
                $"Shaktool attack list {index}");
        }

        RoomEnemySystem enemies = CreateCompiledShaktoolGroup(rom, flags);
        RoomEnemySlot center = enemies.Slots[3];
        ShaktoolSegmentState centerState = enemies.ShaktoolSegments[3]!;
        ShaktoolSegmentState nextState = enemies.ShaktoolSegments[4]!;
        var orient = typeof(RoomEnemySystem).GetMethod(
                "AdvanceAndOrientShaktoolCenter", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState>>(enemies);
        for (ushort bucket = 0; bucket <= 0x00e0; bucket += 0x0020)
        {
            ushort midpoint = unchecked((ushort)(bucket << 8));
            center.Parameter1 = 0;
            centerState.OrbitAngle = unchecked((ushort)(midpoint ^ 0x8000));
            centerState.AngularVelocity = 0;
            centerState.OrientationAndAcceleration = 0;
            nextState.OrbitAngle = midpoint;
            orient(center, centerState);
            AssertEqual(bucket, unchecked((ushort)(
                    centerState.OrientationAndAcceleration & 0x00ff)),
                $"Shaktool production orientation bucket {bucket:X2}");
            AssertEqual(ShaktoolInstructionDefinitions.ForOrientationBucket(bucket),
                center.CurrentInstruction,
                $"Shaktool production orientation list {bucket:X2}");
        }

        RoomEnemySlot tail = enemies.Slots[6];
        ShaktoolSegmentState tailState = enemies.ShaktoolSegments[6]!;
        var reverse = typeof(RoomEnemySystem).GetMethod(
                "ReverseShaktoolAfterCollision", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ShaktoolSegmentState, ushort, ushort>>(
                enemies);
        reverse(tail, tailState, tail.XPosition, tail.YPosition);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(ShaktoolInstructionDefinitions.CollisionForSegment(index),
                enemies.Slots[index].CurrentInstruction,
                $"Shaktool production collision list {index}");
        }

        enemies.StartUnusedShaktoolAttack(enemies.Slots[0]);
        for (int index = 0; index < 7; index++)
        {
            AssertEqual(ShaktoolInstructionDefinitions.AttackForSegment(index),
                enemies.Slots[index].CurrentInstruction,
                $"Shaktool production dormant attack list {index}");
        }

        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.ForOrientationBucket(1),
            "unaligned Shaktool orientation bucket");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.ForOrientationBucket(0x0100),
            "Shaktool orientation bucket past table");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.CollisionForSegment(-1),
            "negative Shaktool collision segment");
        AssertThrows<InvalidDataException>(
            () => ShaktoolInstructionDefinitions.AttackForSegment(7),
            "Shaktool attack segment past table");
        Console.WriteLine(
            "Shaktool instruction definitions: 22 native selectors and all orientation, collision-reversal and dormant-attack production handoffs pass with source tables forbidden.");
    }

    private static RoomEnemySystem CreateCompiledShaktoolGroup(
        SuperMetroidAddressSpace rom,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new ShaktoolInstructionReadGuard(rom));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeShaktool", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        for (int index = 0; index < 7; index++)
        {
            RoomEnemySlot slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = RoomEnemySystem.ShaktoolDefinition;
            slot.Parameter2 = unchecked((ushort)(index * 2));
            slot.XPosition = unchecked((ushort)(0x0100 + index * 16));
            slot.YPosition = 0x0200;
            initialize(slot);
        }
        return enemies;
    }

    private sealed class ShaktoolInstructionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xaadd15 and < 0xaadd25 or >= 0xaadf13 and < 0xaadf2f
                ? throw new InvalidOperationException(
                    $"Shaktool attempted migrated instruction-selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
