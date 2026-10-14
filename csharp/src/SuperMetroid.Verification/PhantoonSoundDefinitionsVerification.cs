using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Phantoon's three materialization sounds and their production callback cycle.</summary>
internal static class PhantoonSoundDefinitionsVerification
{
    /// <summary>Compares native sound words and confirms two callback cycles advance the index without rereading the table.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon sound definitions")]
    internal static void VerifyPhantoonSoundDefinitions(SuperMetroidAddressSpace rom)
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
        enemies.BindFixtureBus(NewPhantoonSoundReadGuard(rom));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.PhantoonBody;
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.SlotTable.Slots[1],
            Tentacles = enemies.SlotTable.Slots[2],
            Mouth = enemies.SlotTable.Slots[3],
        };
        enemies.PhantoonBehavior.Phantoon = state;
        Func<RoomEnemySlot, ushort, bool> process = enemies.PhantoonBehavior.ProcessPhantoonInstructionFunction;

        for (ushort call = 0; call < 6; call++)
        {
            ushort index = (ushort)(call % 3);
            AssertTrue(!process(
                    body,
                    (ushort)PhantoonInstruction.PlayPhantoonMaterializationSFX),
                $"Phantoon materialization callback {call} resumes instruction stream");
            AssertEqual(PhantoonSoundDefinitions.MaterializationSound(index),
                (state.LastMaterializationSound ?? throw new InvalidOperationException("Verification requires state.LastMaterializationSound.")),
                $"Phantoon materialization callback {call} sound");
            AssertEqual((ushort)((index + 1) % 3), state.MaterializationSoundIndex,
                $"Phantoon materialization callback {call} next index");
        }

        Console.WriteLine(
            "Phantoon sound definitions: all three native selections and two complete production callback cycles pass with the source table forbidden.");
    }

    /// <summary>Forbids production reads of the compiled materialization sound table.</summary>
    private static CartridgeReadGuard NewPhantoonSoundReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa7cded and < 0xa7cdf3,
            address => $"Phantoon attempted migrated materialization sound read ${address:X6}.");

    /// <summary>Runs the Phantoon sound definition check with a retail cartridge oracle.</summary>
    [VerificationCommand("--phantoon-sound-definitions")]
    internal static int PhantoonSoundDefinitionsCommand()
    {
        Suite(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
