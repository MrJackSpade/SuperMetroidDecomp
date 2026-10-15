using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that Dead Torizo's real initializer starts its stationary pose and the pose holds.</summary>
internal static class DeadTorizoBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the stationary pose; it shows once and holds (the update that reaches the native
    /// sleep still reports the one-update pose as new).
    /// </summary>
    internal static void VerifyDeadTorizoBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.BindFixtureBus(rom);
        RoomEnemySlot corpse = enemies.SlotTable.Slots[0];
        corpse.EnemyDefinitionPointer = EnemyDefinitionId.CorpseTorizo;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = SuperMetroid.Core.Enemies.Corpses.Artwork.DeadTorizoFrameDefinitions.Bank };
        enemies.DeadTorizoBehavior.InitializeDeadTorizo(corpse);
        AssertEqual(AnimationProgramCursor<DeadTorizoProgram>.Start(DeadTorizoProgram.Stationary),
            enemies.DeadTorizoBehavior._deadTorizoProgram, "real Dead Torizo initializer starts the stationary pose");
        ushort frame = DeadTorizoAnimations.Spritemaps(DeadTorizoProgram.Stationary).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame, frame], [1, 1]),
            EnemyInstructionRunner.ShownFrames(enemies, corpse, 3), "Dead Torizo stationary pose");
        Console.WriteLine("  Dead Torizo behavior: the initializer starts the stationary pose and it holds.");
    }

    /// <summary>Runs the Dead Torizo behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--dead-torizo-instruction-mechanics")]
    internal static int DeadTorizoBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDeadTorizoBehavior), () => VerifyDeadTorizoBehavior(rom));
        Suite(nameof(DeadTorizoAnimationsVerification.VerifyDeadTorizoAnimations), () => DeadTorizoAnimationsVerification.VerifyDeadTorizoAnimations(rom));
        return 0;
    }
}
