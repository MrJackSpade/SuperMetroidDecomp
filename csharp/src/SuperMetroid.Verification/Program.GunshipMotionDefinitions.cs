using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGunshipMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var brakeMethod = typeof(RoomEnemySystem).GetMethod("BouncePostCeresGunship", flags)!;
        var bobMethod = typeof(RoomEnemySystem).GetMethod("StepGunshipBob", flags)!;

        for (ushort frame = 0; frame < 17; frame++)
        {
            short expected = unchecked((short)ReadGunshipMotionWord(
                rom,
                0xa2a622 + frame * 2));
            AssertEqual(expected, GunshipMotionDefinitions.LandingBrakeYDelta(frame),
                $"gunship landing-brake delta {frame}");

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new GunshipMotionDefinitionReadGuard(rom));
            var applyBrake = brakeMethod.CreateDelegate<Action<RoomEnemySlot, SamusState>>(enemies);
            RoomEnemySlot top = enemies.Slots[0];
            top.XPosition = 0x1234;
            top.YPosition = 0x2000;
            top.VariableE = frame;
            enemies.Slots[1].YPosition = 0x2100;
            enemies.Slots[2].YPosition = 0x2200;
            var samus = new SamusState { YPosition = 0x3000 };

            applyBrake(top, samus);

            AssertEqual(unchecked((ushort)(0x3000 + expected)), samus.YPosition,
                $"gunship production brake Samus Y {frame}");
            AssertEqual(unchecked((ushort)(0x2000 + expected)), top.YPosition,
                $"gunship production brake top Y {frame}");
            AssertEqual(unchecked((ushort)(0x2100 + expected)), enemies.Slots[1].YPosition,
                $"gunship production brake bottom Y {frame}");
            AssertEqual(unchecked((ushort)(0x2200 + expected)), enemies.Slots[2].YPosition,
                $"gunship production brake pad Y {frame}");
        }

        for (ushort phase = 0; phase < 4; phase++)
        {
            int address = 0xa2a7cf + phase * 2;
            byte expectedTimer = rom.ReadByte(address);
            sbyte expectedDelta = unchecked((sbyte)rom.ReadByte(address + 1));
            GunshipIdleBobDefinition definition = GunshipMotionDefinitions.IdleBob(phase);
            AssertEqual(expectedTimer, definition.Timer,
                $"gunship idle-bob timer {phase}");
            AssertEqual(expectedDelta, definition.YDelta,
                $"gunship idle-bob delta {phase}");

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new GunshipMotionDefinitionReadGuard(rom));
            var stepBob = bobMethod.CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot top = enemies.Slots[0];
            top.VariableC = phase;
            top.VariableD = 1;
            top.YPosition = 0x2000;
            enemies.Slots[1].YPosition = 0x2100;
            enemies.Slots[2].YPosition = 0x2200;

            stepBob(top);

            AssertEqual((ushort)expectedTimer, top.VariableD,
                $"gunship production bob timer {phase}");
            AssertEqual(unchecked((ushort)((phase + 1) & 3)), top.VariableC,
                $"gunship production bob phase {phase}");
            AssertEqual(unchecked((ushort)(0x2000 + expectedDelta)), top.YPosition,
                $"gunship production bob top Y {phase}");
            AssertEqual(unchecked((ushort)(0x2100 + expectedDelta)), enemies.Slots[1].YPosition,
                $"gunship production bob bottom Y {phase}");
            AssertEqual(unchecked((ushort)(0x2200 + expectedDelta)), enemies.Slots[2].YPosition,
                $"gunship production bob pad Y {phase}");
        }

        AssertThrows<InvalidDataException>(
            () => GunshipMotionDefinitions.LandingBrakeYDelta(17),
            "gunship landing-brake schedule rejects a post-table frame");
        AssertThrows<InvalidDataException>(
            () => GunshipMotionDefinitions.IdleBob(4),
            "gunship idle-bob schedule rejects a post-table phase");

        Console.WriteLine(
            "Gunship motion definitions: all seventeen brake deltas, four idle-bob records, and both production consumers pass with source reads forbidden.");
    }

    private static ushort ReadGunshipMotionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class GunshipMotionDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa2a622 and < 0xa2a644 or
                >= 0xa2a7cf and < 0xa2a7d7
                ? throw new InvalidOperationException(
                    $"Gunship attempted migrated motion-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
