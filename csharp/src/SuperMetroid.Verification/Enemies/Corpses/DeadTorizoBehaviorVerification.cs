using SuperMetroid.Core.Programs;
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
    internal static void VerifyDeadTorizoBehavior()
    {
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot corpse = enemies.SlotTable.Slots[0];
        corpse.Bind(EnemyDefinitionId.CorpseTorizo, corpse.Definition);
        corpse.Bind(corpse.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = SuperMetroid.Core.Enemies.Corpses.Artwork.DeadTorizoFrameToolingDefinitions.Bank });
        enemies.DeadTorizoBehavior.InitializeDeadTorizo(corpse);
        AssertEqual(AnimationProgramCursor<DeadTorizoProgram>.Start(DeadTorizoProgram.Stationary),
            enemies.DeadTorizoBehavior.DeadTorizoProgram, "real Dead Torizo initializer starts the stationary pose");
        ushort frame = DeadTorizoAnimationsConstants.Spritemaps(DeadTorizoProgram.Stationary).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame, frame], [1, 1]),
            EnemyInstructionRunner.ShownFrames(corpse, 3), "Dead Torizo stationary pose");
        Console.WriteLine("  Dead Torizo behavior: the initializer starts the stationary pose and it holds.");
    }

    /// <summary>Runs the Dead Torizo behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--dead-torizo-instruction-mechanics")]
    internal static int DeadTorizoBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDeadTorizoBehavior), () => VerifyDeadTorizoBehavior());
        Suite(nameof(DeadTorizoAnimationsVerification.VerifyDeadTorizoAnimations), () => DeadTorizoAnimationsVerification.VerifyDeadTorizoAnimations(rom));
        return 0;
    }
}
