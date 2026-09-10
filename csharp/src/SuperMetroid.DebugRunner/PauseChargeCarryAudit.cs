using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Checks the production frontend's pause-separated Down edges and charged soft morphs.
/// This is a C# integration baseline, not a native-CPU timing comparison.
/// </summary>
internal static class PauseChargeCarryAudit
{
    public static int Run(string rom)
    {
        int successful = 0;
        for (int pauseAt = 120; pauseAt <= 150; pauseAt++)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var system = new Bank80SystemState();
            system.SetBossBits(0, BossBits.AreaTorizo);
            system.MarkSaveStationUsed(AreaId.Crateria, 0);
            var seed = new SamusState
            {
                Health = 99, MaxHealth = 99,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                EquippedBeams = (ushort)SamusBeamFlags.Charge,
                CollectedBeams = (ushort)SamusBeamFlags.Charge,
            };
            new SuperMetroidSaveRam(bus).SaveSlot(0, SuperMetroidSaveSnapshot.Capture(seed, system, 0, 0));
            var game = new SuperMetroidGame(bus, new SuperMetroidGameOptions { SkipOpeningCinematic = true });
            var frame = FrontendAuditDriver.EnterSelectedSlot(game);
            FrontendAuditDriver.StepUntil(game, frame, _ => game.GameplayMovementEnabled, 420, "Load never enabled gameplay.");
            var runtime = game.RuntimeForVerification!;
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 0, 0);
            var level = runtime.LevelData!;
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks + x, y == 16 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(y * level.WidthInBlocks + x, (byte)0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            runtime.InitializeDebugGroundedSamus(1000, 235, 16);
            var samus = runtime.Samus!;
            samus.EquippedItems = seed.EquippedItems; samus.CollectedItems = seed.CollectedItems;
            samus.EquippedBeams = seed.EquippedBeams; samus.CollectedBeams = seed.CollectedBeams;
            samus.InputLocked = false;
            long hostFrame = 0;
            void Step(SnesButton input) => game.StepCaptured((ushort)input, ++hostFrame, 1);
            // Default cartridge bindings: X shoots, B runs and A jumps. Charge is
            // earned by controller input, never injected into the projectile state.
            for (int tick = 0; tick <= pauseAt; tick++)
            {
                SnesButton input = SnesButton.X;
                if (tick >= 30 && tick < pauseAt) input |= SnesButton.Right | SnesButton.B;
                if (tick >= 70) input |= SnesButton.A;
                if (tick == pauseAt) input |= SnesButton.Start | SnesButton.Down;
                Step(input);
            }
            if (game.GameState != SuperMetroidGameState.PausingDarkening)
                throw new InvalidDataException("Start did not enter normal pause darkening.");
            int darkeningFrames = 0;
            while (game.GameState == SuperMetroidGameState.PausingDarkening && darkeningFrames++ < 100)
                Step(SnesButton.X | SnesButton.A | SnesButton.Down);
            if (game.GameState != SuperMetroidGameState.Pausing)
                throw new InvalidDataException("Gameplay pause darkening did not reach the frozen setup state.");
            byte frozenPose = samus.Pose;
            uint frozenY = samus.Kinematics.YFixed;
            ushort frozenCharge = runtime.Projectiles.FlareCounter;
            int guard = 0;
            while (game.GameState != SuperMetroidGameState.PausedB && guard++ < 100)
                Step(SnesButton.X | SnesButton.A);
            if (game.GameState != SuperMetroidGameState.PausedB) throw new InvalidDataException("Pause menu never became interactive.");
            Step(SnesButton.X | SnesButton.A);
            // Start uses the menu's delayed-held filter, unlike the Down edge used
            // by movement. A one-frame Start pulse cannot request this unpause.
            for (int tick = 0; tick < 8 && game.GameState == SuperMetroidGameState.PausedB; tick++)
                Step(SnesButton.X | SnesButton.A | SnesButton.Start);
            guard = 0;
            while (game.GameState != SuperMetroidGameState.Unpausing && guard++ < 100)
                Step(SnesButton.X | SnesButton.A);
            if (game.GameState != SuperMetroidGameState.Unpausing) throw new InvalidDataException("Unpause teardown never completed.");
            if (samus.Pose != frozenPose || samus.Kinematics.YFixed != frozenY || runtime.Projectiles.FlareCounter != frozenCharge)
                throw new InvalidDataException("Frozen menu frames changed Samus movement or charge.");
            Step(SnesButton.X | SnesButton.A | SnesButton.Down);
            bool morphed = samus.Pose is SamusPoseIds.MorphingTransitionRightPose or SamusPoseIds.MorphingTransitionLeftPose;
            if ((runtime.Controller1.NewlyPressed & (ushort)SnesButton.Down) == 0)
                throw new InvalidDataException("Pause failed to separate the two Down edges.");
            bool bounced = false;
            for (int tick = 0; tick < 50; tick++)
            {
                Step(SnesButton.X | SnesButton.A | SnesButton.Down | SnesButton.Right);
                bounced |= samus.MorphBallBounceState != 0;
            }
            bool carried = morphed && !bounced && runtime.Projectiles.FlareCounter >= 60 && SamusState.IsStableBallPose(samus.Pose);
            // Preserve the adjacent early/late controls as well as the successful
            // window. These frame numbers describe this fixture, not ROM identities
            // or a claim that the window has been measured on the cartridge yet.
            if (morphed != (pauseAt <= 146) || bounced != (pauseAt < 140) ||
                carried != (pauseAt >= 140 && pauseAt <= 146))
                throw new InvalidDataException($"Pause carry timing baseline changed at frame {pauseAt}.");
            if (carried) successful++;
            Console.WriteLine($"PAUSE CARRY start={pauseAt} frozen={frozenPose:X2}/{frozenY:X8}/{frozenCharge:X4} morph={morphed} bounce={bounced} carry={carried} charge={runtime.Projectiles.FlareCounter:X4}");
            if (carried)
            {
                if (runtime.BombProjectiles.BombCounter != 0)
                    throw new InvalidDataException("Holding Down released the spread prematurely.");
                Step(SnesButton.X | SnesButton.A | SnesButton.Right);
                if (runtime.BombProjectiles.BombCounter != 5 || runtime.Projectiles.FlareCounter != 0)
                    throw new InvalidDataException("Releasing Down failed to consume charge and produce five bombs.");
            }
        }
        Console.WriteLine($"Pause carry integration baseline: 31 timings, {successful} no-bounce carries with delayed five-bomb release.");
        return successful == 7 ? 0 : 1;
    }
}
