using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBotwoonInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        var guarded = new BotwoonInstructionReadGuard(rom);
        var animateHead = typeof(RoomEnemySystem).GetMethod(
            "AnimateBotwoonHeadFromMovement", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>();
        var aimHead = typeof(RoomEnemySystem).GetMethod("AimBotwoonAtSamus", flags)!
            .CreateDelegate<Action<RoomEnemySystem, RoomEnemySlot, BotwoonEnemyState, SamusState>>();
        var animateBody = typeof(RoomEnemySystem).GetMethod(
            "AnimateBotwoonBodySegment", flags)!
            .CreateDelegate<Action<RoomEnemySystem, RoomEnemyProjectileSlot, byte>>();
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;

        (short X, short Y)[] vectors =
        [
            (0, -100),
            (100, -100),
            (100, 0),
            (100, 100),
            (0, 100),
            (-100, 100),
            (-100, 0),
            (-100, -100),
        ];

        for (int octant = 0; octant < vectors.Length; octant++)
        {
            BotwoonHeadInstructionDefinition definition =
                BotwoonInstructionDefinitions.HeadForOctant(octant);
            AssertEqual(Word(0xb3946b + octant * 2), definition.MovementInstruction,
                $"Botwoon head movement octant {octant}");
            AssertEqual(Word(0xb3948b + octant * 2), definition.SpitInstruction,
                $"Botwoon head spit octant {octant}");
            AssertEqual(Word(0xb3947b + octant * 2),
                BotwoonInstructionDefinitions.HiddenHeadInstruction,
                $"Botwoon hidden-head duplicate {octant}");

            (short dx, short dy) = vectors[octant];
            var movementEnemies = new RoomEnemySystem();
            busField.SetValue(movementEnemies, guarded);
            RoomEnemySlot movementHead = movementEnemies.Slots[0];
            var movementState = new BotwoonEnemyState(movementHead);
            movementHead.XPosition = unchecked((ushort)(1000 + dx));
            movementHead.YPosition = unchecked((ushort)(1000 + dy));
            movementState.HeadHistoryX[3] = 1000;
            movementState.HeadHistoryY[3] = 1000;
            animateHead(movementHead, movementState);
            AssertEqual(definition.MovementInstruction, movementHead.CurrentInstruction,
                $"production Botwoon moving-head octant {octant}");

            var spitEnemies = new RoomEnemySystem();
            busField.SetValue(spitEnemies, guarded);
            RoomEnemySlot spitHead = spitEnemies.Slots[0];
            var spitState = new BotwoonEnemyState(spitHead)
            {
                Function = BotwoonEnemyFunction.SpitWhileHidden,
            };
            spitHead.XPosition = 1000;
            spitHead.YPosition = 1000;
            var samus = new SamusState
            {
                XPosition = unchecked((ushort)(1000 + dx)),
                YPosition = unchecked((ushort)(1000 + dy)),
            };
            aimHead(spitEnemies, spitHead, spitState, samus);
            AssertEqual(definition.SpitInstruction, spitHead.CurrentInstruction,
                $"production Botwoon spit-head octant {octant}");
        }

        for (ushort byteOffset = 0; byteOffset < 64; byteOffset += 2)
        {
            ushort expected = Word(0x86e9f1 + byteOffset);
            AssertEqual(expected, BotwoonInstructionDefinitions.BodyInstruction(byteOffset),
                $"Botwoon body selector ${byteOffset:X2}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var segment = new RoomEnemyProjectileSlot(0)
            {
                DirectionParameter = byteOffset,
                Variable0 = ushort.MaxValue,
            };
            animateBody(enemies, segment, 0);
            AssertEqual(expected, segment.InstructionPointer,
                $"production Botwoon body selector ${byteOffset:X2}");
            AssertEqual(expected, segment.Variable0,
                $"production Botwoon body installed selector ${byteOffset:X2}");
            AssertEqual((ushort)1, segment.InstructionTimer,
                $"production Botwoon body timer ${byteOffset:X2}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => BotwoonInstructionDefinitions.HeadForOctant(8),
            "Botwoon head octant past definitions");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BotwoonInstructionDefinitions.BodyInstruction(1),
            "Botwoon odd body byte offset");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BotwoonInstructionDefinitions.BodyInstruction(64),
            "Botwoon body byte offset past definitions");

        Console.WriteLine(
            "Botwoon instruction definitions: all 56 native selector words and 48 real head/body selections pass with all source tables forbidden.");
    }

    private sealed class BotwoonInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xb3946b and < 0xb3949b or >= 0x86e9f1 and < 0x86ea31
                ? throw new InvalidOperationException(
                    $"Botwoon attempted migrated instruction-table read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
