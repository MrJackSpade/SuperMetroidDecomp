using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon sound definitions")]
    private static void VerifyPhantoonSoundDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa7cded;
        for (ushort index = 0; index < 3; index++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(sourceAddress + index * 2);
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
            NewPhantoonSoundReadGuard(rom));
        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.PhantoonBody;
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.Slots[1],
            Tentacles = enemies.Slots[2],
            Mouth = enemies.Slots[3],
        };
        typeof(RoomEnemySystem).GetField("<Phantoon>k__BackingField", flags)!.SetValue(enemies, state);
        var process = typeof(RoomEnemySystem).GetMethod(
            "ProcessPhantoonInstructionFunction", flags)!
            .CreateDelegate<Func<RoomEnemySlot, ushort, bool>>(enemies);

        for (ushort call = 0; call < 6; call++)
        {
            ushort index = (ushort)(call % 3);
            AssertTrue(!process(
                    body,
                    (ushort)PhantoonInstruction.PlayPhantoonMaterializationSFX),
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

    private static CartridgeReadGuard NewPhantoonSoundReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa7cded and < 0xa7cdf3,
            address => $"Phantoon attempted migrated materialization sound read ${address:X6}.");
}
