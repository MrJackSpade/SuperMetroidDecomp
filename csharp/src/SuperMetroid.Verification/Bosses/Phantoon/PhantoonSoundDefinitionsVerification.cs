using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Phantoon's three materialization sounds and their production callback cycle.</summary>
internal static class PhantoonSoundDefinitionsVerification
{
    /// <summary>Compares native sound words and confirms two callback cycles advance the next sound without rereading the table.</summary>
    internal static void VerifyPhantoonSoundDefinitions(CartridgeImportAddressSpace rom)
    {
        const int sourceAddress = 0xa7cded;
        AssertEqual(3, PhantoonSoundDefinitions.MaterializationSounds.Count, "Phantoon materialization cycle length");
        for (int index = 0; index < 3; index++)
        {
            SoundEffectId expected = SoundEffectCommands.Decode(SoundEffectLibrary.Enemies,
                CartridgeImportSource.Require(rom).ReadCartridgeWord(sourceAddress + index * 2));
            AssertEqual(expected, PhantoonSoundDefinitions.MaterializationSounds[index],
                $"Phantoon materialization sound {index}");
        }

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.PhantoonBody, body.Definition);
        var state = new PhantoonEnemyState(body.HeldOccupant(), body, enemies.Places.InUpdateOrder[1], enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3]);
        enemies.BeginPhantoonAsSetUp(state);
        Action<PhantoonEnemyState, PhantoonPartCue> process = enemies.PhantoonBehavior.ApplyPhantoonPartCue;

        AssertEqual(PhantoonSoundDefinitions.MaterializationSounds[0], state.NextMaterializationSound.Entry, "Phantoon's first materialization queues the cycle's first sound");
        for (int call = 0; call < 6; call++)
        {
            int index = call % 3;
            process(state, PhantoonPartCue.PlayMaterializationSound);
            AssertEqual(PhantoonSoundDefinitions.MaterializationSounds[index],
                (enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a Phantoon materialization sound request.")),
                $"Phantoon materialization callback {call} sound");
            AssertEqual(PhantoonSoundDefinitions.MaterializationSounds[(index + 1) % 3], state.NextMaterializationSound.Entry,
                $"Phantoon materialization callback {call} next sound");
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
