using SuperMetroid.Core.Enemies.Deaths;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Enemies.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Reproduces enemy death-explosion animation during a reserve-refill
/// gameplay freeze, when projectile instructions still advance.</summary>
internal static class FrozenTimeEnemyProjectilesVerification
{
    /// <summary>
    /// Enemy_Projectile_Handler ($86:8104) checks only its enable bit; state eight calls it while
    /// time is frozen for a reserve refill, so enemy projectiles keep animating. In the 13% movie
    /// a Powamp's death explosion advances through Samus's reserve refill.
    /// </summary>
    internal static void VerifyFrozenTimeEnemyProjectiles()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MtEverest);
        runtime.Player.IntroducedSamus.Control.LockHandlers();
        RoomEnemySlot victim = runtime.Enemies.SlotTable.Slots.First(slot => slot.DefinitionId == EnemyDefinitionId.Sciser);
        runtime.Enemies.DeathsAndDrops.StartGenericEnemyDeath(victim, 2);
        RoomEnemyProjectileSlot explosion = runtime.Enemies.Projectiles.EnemyProjectiles
            .Single(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        runtime.StepFrame(0);
        var program = (EnemyDeathProjectileAnimation)(explosion.Animation ?? throw new InvalidOperationException("The explosion plays a program."));
        var cursor = program.Cursor;
        ushort timer = explosion.InstructionTimer.RawValue;

        runtime.Freeze.GameplayTimeFrozen = true;
        runtime.StepFrame(0);

        AssertTrue(program.Cursor != cursor || explosion.InstructionTimer.RawValue != timer,
            "the explosion's instructions advance while time is frozen");
        Console.WriteLine("  Frozen-time enemy projectiles: instructions keep running under a reserve-refill freeze.");
    }

    /// <summary>Runs the focused frozen-time enemy-projectile check from the CLI.</summary>
    [VerificationCommand("--frozen-time-enemy-projectiles")]
    internal static int FrozenTimeEnemyProjectilesCommand()
    {
        Suite(nameof(VerifyFrozenTimeEnemyProjectiles), () => VerifyFrozenTimeEnemyProjectiles());
        return 0;
    }
}
