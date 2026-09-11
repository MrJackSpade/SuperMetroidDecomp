using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyReserveAutoFrontend()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader);
        var samus = runtime.Samus!;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.Health = 0; samus.MaxHealth = 99;
        samus.ReserveEnergy = 2; samus.MaxReserveEnergy = 100; samus.ReserveTankMode = 1;
        var game = new SuperMetroidGame(bus);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        var recovery = (SamusReserveAutoRecoveryState)typeof(SuperMetroidGame)
            .GetField("reserveRecovery", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
        recovery.Begin(samus);
        runtime.GameplayTimeFrozen = true;
        runtime.BombProjectiles.SetSharedCooldown(10);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
            .SetValue(game, SuperMetroidGameState.ReserveTanksAuto);
        game.StepCaptured(0, 1, 1);
        AssertEqual(1, samus.Health, "automatic refill first frame");
        AssertTrue(runtime.TimeIsFrozen && samus.InputLocked, "nonfinal refill keeps native freeze and input lock");
        AssertTrue(runtime.LastGroundedSamusMovement is null, "nonfinal refill does not execute grounded movement");
        AssertEqual(10, runtime.BombProjectiles.CooldownTimer, "nonfinal refill freezes shared projectile clock");
        game.StepCaptured((ushort)SnesButton.Right, 2, 1);
        AssertEqual(2, samus.Health, "automatic refill completion frame");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "completion restores outer gameplay state");
        AssertTrue(!runtime.TimeIsFrozen && !samus.InputLocked, "completion clears native freeze and input lock");
        AssertEqual(9, runtime.BombProjectiles.CooldownTimer,
            "completion unfreezes the gameplay projectile pass on the same frame");
        AssertTrue(runtime.LastGroundedSamusMovement is not null,
            "native $82:DC18 unfreezes BEFORE the completion frame gameplay/movement pass");
        Console.WriteLine("Automatic reserve frontend: nonfinal freeze and same-frame completion movement pass.");
    }
}
