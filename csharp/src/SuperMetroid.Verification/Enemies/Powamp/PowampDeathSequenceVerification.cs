using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Reproduces the 13% movie's final Powamp death-countdown frame.</summary>
internal static class PowampDeathSequenceVerification
{
    /// <summary>
    /// A Powamp's death countdown ends in EnemyDeath ($A8:C5B5): the body slot clears and the
    /// death explosion spawns in that frame, after the eight spikes. In the 13% movie the
    /// explosion appears the frame the countdown ends; the port only marked the body deleted.
    /// </summary>
    internal static void VerifyPowampDeathSequence()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MtEverest);
        RoomEnemySlot body = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Powamp &&
            !((PowampEnemyState)PowampBehavior.RequirePowampState(slot)).IsBalloon);
        var state = (PowampEnemyState)PowampBehavior.RequirePowampState(body);

        body.Health = 0;
        state.Function = PowampEnemyFunction.DeathSequence;
        state.FunctionTimer = NativeCountdown.Start(1);
        runtime.Enemies.PowampBehavior.RunPowampMain(body, state, runtime.Room.LevelData);

        var projectiles = runtime.Enemies.Projectiles.EnemyProjectiles;
        AssertEqual(8, projectiles.Count(p => p.Kind == RoomEnemyProjectileKind.PowampSpike), "the eight spikes fire");
        AssertEqual(1, projectiles.Count(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion),
            "EnemyDeath spawns the death explosion in the same frame");
        AssertEqual(EnemyDefinitionId.None, body.EnemyDefinitionPointer, "EnemyDeath clears the body slot at once");
        Console.WriteLine("  Powamp death sequence: the countdown ends in EnemyDeath with spikes and an explosion.");
    }

    /// <summary>Runs the focused spike, explosion, and body-slot death check.</summary>
    [VerificationCommand("--powamp-death-sequence")]
    internal static int PowampDeathSequenceCommand()
    {
        Suite(nameof(VerifyPowampDeathSequence), () => VerifyPowampDeathSequence());
        return 0;
    }
}
