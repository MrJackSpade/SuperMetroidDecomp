using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

internal static class MotherBrainDroolTimingAudit
{
    internal static int Run(string installationRoot)
    {
        var installation = new GameInstallation(installationRoot);
        var bus = installation.OpenRuntimeAddressSpace();
        var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
        InstalledInputReplay.Bind(game, installation);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, [false]);
        var runtime = game.RuntimeForVerification!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var enemies = runtime.Enemies;
        var state = enemies.MotherBrain!;
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        var apply = typeof(RoomEnemySystem).GetMethod("ApplyLiveMotherBrainRainbowState", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnMotherBrainDrool", BindingFlags.Instance | BindingFlags.NonPublic)!;
        void SetPhase(MotherBrainRainbowBeamAttackPhase phase) => typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("Phase")!.SetValue(sequence, phase);
        void Publish() => apply.Invoke(enemies, [state, sequence, null, runtime.Samus, runtime.BombProjectiles]);
        void Step() { sequence.Step(bus, runtime.Samus!, 0, 0); Publish(); }
        void CheckSpawn(bool expected, string context)
        {
            ushort before = state.DroolProjectileParameter;
            spawn.Invoke(enemies, [state]);
            if ((state.DroolProjectileParameter != before) != expected || state.DroolGenerationEnabled != expected)
                throw new InvalidDataException($"{context}: expected drool enabled={expected}, live={state.DroolGenerationEnabled}, sequence={sequence.DroolGenerationEnabled}.");
        }
        SetPhase(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam);
        typeof(MotherBrainEnemyState).GetProperty("DroolGenerationEnabled")!.SetValue(state, true);
        Publish();
        CheckSpawn(true, "Before draining");
        SetPhase(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode);
        sequence.Body.Pose = 0;
        Step();
        CheckSpawn(false, "Low power");
        // BF60 clears drool before the pose check, even while the body is moving.
        var moving = new MotherBrainRainbowBeamAttackSequence();
        typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("Phase")!.SetValue(moving, MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode);
        moving.Body.Pose = 1;
        moving.Step(bus, runtime.Samus!, 0, 0);
        if (moving.DroolGenerationEnabled) throw new InvalidDataException("Drool remained enabled while waiting for standing pose.");
        SetPhase(MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey);
        Step();
        CheckSpawn(false, "Grey timer setup");
        for (int frame = 1; frame <= 768; frame++)
        {
            Step();
            CheckSpawn(false, $"Grey recovery frame {frame}");
        }
        Step();
        CheckSpawn(true, "Grey recovery frame 769");
        if (!enemies.EnemyProjectiles.Any(p => p.Kind is RoomEnemyProjectileKind.MotherBrainDrool or RoomEnemyProjectileKind.MotherBrainDyingDrool))
            throw new InvalidDataException("Re-enabled drool did not allocate a visible projectile.");
        Console.WriteLine("PASS: live drool stops at low power (including non-standing pose), stays disabled for 768 recovery calls, and spawns again on call 769.");
        return 0;
    }
}
