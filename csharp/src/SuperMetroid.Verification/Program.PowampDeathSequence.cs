using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// A Powamp's death countdown ends in EnemyDeath ($A8:C5B5): the body slot clears and the
    /// death explosion spawns in that frame, after the eight spikes. In the 13% movie the
    /// explosion appears the frame the countdown ends; the port only marked the body deleted.
    /// </summary>
    private static void VerifyPowampDeathSequence()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xd0b9);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo requireState = typeof(RoomEnemySystem).GetMethod("RequirePowampState", flags)!;
        RoomEnemySlot body = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == 0xe8bf &&
            !((PowampEnemyState)requireState.Invoke(runtime.Enemies, [slot])!).IsBalloon);
        var state = (PowampEnemyState)requireState.Invoke(runtime.Enemies, [body])!;

        body.Health = 0;
        state.Function = PowampEnemyFunction.DeathSequence;
        state.FunctionTimer = 1;
        typeof(RoomEnemySystem).GetMethod("RunPowampMain", flags)!
            .Invoke(runtime.Enemies, [body, state, runtime.LevelData]);

        var projectiles = runtime.Enemies.EnemyProjectiles;
        AssertEqual(8, projectiles.Count(p => p.Kind == RoomEnemyProjectileKind.PowampSpike), "the eight spikes fire");
        AssertEqual(1, projectiles.Count(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion),
            "EnemyDeath spawns the death explosion in the same frame");
        AssertEqual((ushort)0, body.EnemyDefinitionPointer, "EnemyDeath clears the body slot at once");
        Console.WriteLine("  Powamp death sequence: the countdown ends in EnemyDeath with spikes and an explosion.");
    }
}
