using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPhantoonSoundDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa7cded;
        for (ushort index = 0; index < 3; index++)
        {
            ushort expected = (ushort)(
                rom.ReadByte(sourceAddress + index * 2) |
                rom.ReadByte(sourceAddress + index * 2 + 1) << 8);
            AssertEqual(expected, PhantoonSoundDefinitions.MaterializationSound(index),
                $"Phantoon materialization sound {index}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => PhantoonSoundDefinitions.MaterializationSound(3),
            "Phantoon materialization sound past definitions");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new PhantoonSoundReadGuard(rom));
        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = RoomEnemySystem.PhantoonBodyDefinition;
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.Slots[1],
            Tentacles = enemies.Slots[2],
            Mouth = enemies.Slots[3],
        };
        typeof(RoomEnemySystem).GetField("_phantoonState", flags)!.SetValue(enemies, state);
        var process = typeof(RoomEnemySystem).GetMethod(
            "ProcessPhantoonInstructionFunction", flags)!
            .CreateDelegate<Func<RoomEnemySlot, ushort, byte, bool>>(enemies);

        for (ushort call = 0; call < 6; call++)
        {
            ushort index = (ushort)(call % 3);
            AssertTrue(!process(
                    body,
                    PhantoonInstructionCodes.PlayPhantoonMaterializationSFX,
                    0),
                $"Phantoon materialization callback {call} resumes instruction stream");
            AssertEqual(PhantoonSoundDefinitions.MaterializationSound(index),
                state.LastMaterializationSound!.Value,
                $"Phantoon materialization callback {call} sound");
            AssertEqual((ushort)((index + 1) % 3), state.MaterializationSoundIndex,
                $"Phantoon materialization callback {call} next index");
        }

        Console.WriteLine(
            "Phantoon sound definitions: all three native selections and two complete production callback cycles pass with the source table forbidden.");
    }

    private sealed class PhantoonSoundReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa7cded and < 0xa7cdf3
            ? throw new InvalidOperationException(
                $"Phantoon attempted migrated materialization sound read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
