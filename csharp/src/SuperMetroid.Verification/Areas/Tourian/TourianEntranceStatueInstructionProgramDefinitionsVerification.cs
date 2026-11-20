using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Tourian entrance statue scene's three placements against their native lists (#2014).</summary>
internal static class TourianEntranceStatueInstructionProgramDefinitionsVerification
{
    /// <summary>The bank of the statues' native lists.</summary>
    private const byte ListBank = 0xaa;

    /// <summary>Loads the retail cartridge for the entrance statue checks.</summary>
    internal static void VerifyTourianEntranceStatueInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// For each placement's native parameter: the list it selects deletes the statue at its first step, and the production
    /// placement spawns the decorations only from the first placement and leaves the room at its first animation step.
    /// </summary>
    internal static void VerifyTourianEntranceStatueInstructionProgramDefinitions(
        CartridgeImportAddressSpace rom)
    {
        var cartridge = CartridgeImportSource.Require(rom);
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            ushort entry = cartridge.ReadCartridgeWord(EnemyRomTablePointers.TourianStatue.AnimationScriptWords + parameter);
            AssertEqual(NativeEnemyListDefinitions.WordKind.Stop,
                NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord((int)new SnesAddress(ListBank, entry))),
                $"Tourian entrance-statue parameter {parameter} selects a native list that deletes the statue at once");

            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork, statueRoom: new SuperMetroid.Verification.Areas.Tourian.FixedTourianStatueRoom(WaterSurface: 216, StatueOffset: 0));
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.TourianStatue, place.Definition);
            place.Bind(place.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
            place.SetSpawnParameter1(EnemyDefinitionId.TourianStatue, parameter);
            EnemySystemFixtures.InitializeStatuePlacement(place);
            AssertEqual(parameter == 0 ? 3 : 0, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive),
                $"Tourian entrance-statue parameter {parameter} spawns the decorations only from the first placement");
            AssertTrue(!place.Presence.HasAny(EnemyPresence.Deleted),
                $"Tourian entrance-statue parameter {parameter} stays in the room until its first animation step");

            EnemyAnimationPlayer.AdvanceAnimation(place, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, 0);
            AssertTrue(place.Presence.HasAny(EnemyPresence.Deleted),
                $"Tourian entrance-statue parameter {parameter} leaves the room at its first animation step");
        }

        Console.WriteLine(
            "Tourian entrance statue: all three placements' native lists delete at once, and the production placements spawn and leave as they do.");
    }

    /// <summary>Runs the entrance statue instruction-definition command.</summary>
    [VerificationCommand("--tourian-entrance-statue-instruction-program-definitions")]
    internal static int TourianEntranceStatueInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyTourianEntranceStatueInstructionProgramDefinitions), () => VerifyTourianEntranceStatueInstructionProgramDefinitions());
        return 0;
    }
}
