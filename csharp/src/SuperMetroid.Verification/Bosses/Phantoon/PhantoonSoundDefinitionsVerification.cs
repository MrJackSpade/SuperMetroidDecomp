using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Phantoon's three materialization sounds and their production callback cycle.</summary>
internal static class PhantoonSoundDefinitionsVerification
{
    /// <summary>Compares native sound words and confirms two callback cycles advance the index without rereading the table.</summary>
    internal static void VerifyPhantoonSoundDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa7cded;
        AssertEqual(3, PhantoonSoundDefinitions.MaterializationSoundCount, "Phantoon materialization cycle length");
        for (int index = 0; index < 3; index++)
        {
            SoundEffectId expected = SoundEffectCommands.Decode(SoundEffectLibrary.Library2,
                CartridgeImportSource.Require(rom).ReadCartridgeWord(sourceAddress + index * 2));
            AssertEqual(expected, PhantoonSounds.MaterializationSound(index),
                $"Phantoon materialization sound {index}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => PhantoonSounds.MaterializationSound(3),
            "Phantoon materialization sound past definitions");

        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.PhantoonBody, body.Definition);
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.SlotTable.Slots[1],
            Tentacles = enemies.SlotTable.Slots[2],
            Mouth = enemies.SlotTable.Slots[3],
        };
        enemies.PhantoonBehavior.Combat.Phantoon = state;
        Action<RoomEnemySlot, PhantoonPartCue> process = enemies.PhantoonBehavior.ApplyPhantoonPartCue;

        for (int call = 0; call < 6; call++)
        {
            int index = call % 3;
            process(body, PhantoonPartCue.PlayMaterializationSound);
            AssertEqual(PhantoonSounds.MaterializationSound(index),
                (enemies.Audio.PublishedSoundFrom(EnemySoundSource.PhantoonMaterialization) ?? throw new InvalidOperationException("Verification requires a Phantoon materialization sound request.")),
                $"Phantoon materialization callback {call} sound");
            AssertEqual((index + 1) % 3, state.MaterializationSoundIndex,
                $"Phantoon materialization callback {call} next index");
        }

        Console.WriteLine(
            "Phantoon sound definitions: all three native selections and two complete production callback cycles pass with the source table forbidden.");
    }


    /// <summary>Runs the Phantoon sound definition check with a retail cartridge oracle.</summary>
    [VerificationCommand("--phantoon-sound-definitions")]
    internal static int PhantoonSoundDefinitionsCommand()
    {
        Suite(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
