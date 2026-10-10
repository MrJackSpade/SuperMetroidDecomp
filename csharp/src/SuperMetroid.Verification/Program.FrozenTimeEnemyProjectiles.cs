using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Enemy_Projectile_Handler ($86:8104) checks only its enable bit; state eight calls it while
    /// time is frozen for a reserve refill, so enemy projectiles keep animating. In the 13% movie
    /// a Powamp's death explosion advances through Samus's reserve refill.
    /// </summary>
    private static void VerifyFrozenTimeEnemyProjectiles()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xd0b9);
        runtime.Samus!.InputLocked = true;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        RoomEnemySlot victim = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Sciser);
        typeof(RoomEnemySystem).GetMethod("StartGenericEnemyDeath", flags)!.Invoke(runtime.Enemies, [victim, (ushort)2]);
        RoomEnemyProjectileSlot explosion = runtime.Enemies.EnemyProjectiles
            .Single(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        runtime.StepFrame(0);
        ushort pointer = explosion.InstructionPointer;
        ushort timer = explosion.InstructionTimer;

        runtime.GameplayTimeFrozen = true;
        runtime.StepFrame(0);

        AssertTrue(explosion.InstructionPointer != pointer || explosion.InstructionTimer != timer,
            "the explosion's instructions advance while time is frozen");
        Console.WriteLine("  Frozen-time enemy projectiles: instructions keep running under a reserve-refill freeze.");
    }
}
