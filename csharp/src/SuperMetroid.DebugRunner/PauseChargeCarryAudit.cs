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
        foreach (bool toggleBombs in pauseAt == 142 ? new[] { false, true } : new[] { false })
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
            void ToggleBombsInMenu(bool shouldBeEquipped)
            {
                // Navigate the real equipment page. With this inventory, Right from
                // Charge selects Morph Ball and Down selects Bombs. Do not write the
                // equipped bits or selector directly: those are what this path tests.
                for (int tick = 0; tick < 40; tick++) Step(SnesButton.X | SnesButton.R);
                if (game.PauseScreenMode != 1)
                    throw new InvalidDataException("R did not open the equipment page.");
                Step(SnesButton.X);
                Step(SnesButton.X | SnesButton.Right);
                Step(SnesButton.X);
                Step(SnesButton.X | SnesButton.Down);
                Step(SnesButton.X);
                Step(SnesButton.X | SnesButton.A);
                // Leave A held: the resumed Jump hold must not create a second
                // equipment-toggle edge before Start leaves this menu.
                bool equipped = (samus.EquippedItems & (ushort)SamusEquipmentFlags.Bombs) != 0;
                ushort expectedItems = shouldBeEquipped ? seed.EquippedItems : (ushort)SamusEquipmentFlags.MorphBall;
                if (equipped != shouldBeEquipped || samus.EquippedItems != expectedItems ||
                    samus.CollectedItems != seed.CollectedItems || samus.EquippedBeams != seed.EquippedBeams)
                    throw new InvalidDataException("Equipment menu did not toggle only the equipped Bombs state.");
            }
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
            if (toggleBombs) ToggleBombsInMenu(false);
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
                Step(SnesButton.X | SnesButton.A | SnesButton.Right | (toggleBombs ? 0 : SnesButton.Down));
                bounced |= samus.MorphBallBounceState != 0;
            }
            bool carried = morphed && !bounced && runtime.Projectiles.FlareCounter >= 60 && SamusState.IsStableBallPose(samus.Pose);
            // Preserve the adjacent early/late controls as well as the successful
            // window. These frame numbers describe this fixture, not ROM identities
            // or a claim that the window has been measured on the cartridge yet.
            if (morphed != (pauseAt <= 146) || bounced != (pauseAt < 140) ||
                carried != (pauseAt >= 140 && pauseAt <= 146))
                throw new InvalidDataException($"Pause carry timing baseline changed at frame {pauseAt}: toggle={toggleBombs}, morph={morphed}, bounce={bounced}, carry={carried}, pose={samus.Pose:X2}, charge={runtime.Projectiles.FlareCounter:X4}, equipped={samus.EquippedItems:X4}.");
            if (carried) successful++;
            Console.WriteLine($"PAUSE CARRY start={pauseAt} toggleBombs={toggleBombs} frozen={frozenPose:X2}/{frozenY:X8}/{frozenCharge:X4} morph={morphed} bounce={bounced} carry={carried} charge={runtime.Projectiles.FlareCounter:X4}");
            if (carried)
            {
                if (runtime.BombProjectiles.BombCounter != 0)
                    throw new InvalidDataException("Holding Down released the spread prematurely.");
                if (toggleBombs)
                {
                    ushort chargeBeforeMenu = runtime.Projectiles.FlareCounter;
                    Step(SnesButton.X | SnesButton.Start);
                    for (int tick = 0; tick < 100 && game.GameState != SuperMetroidGameState.PausedB; tick++)
                        Step(SnesButton.X);
                    if (game.GameState != SuperMetroidGameState.PausedB)
                        throw new InvalidDataException("Second pause failed while carrying charge with Bombs disabled.");
                    ToggleBombsInMenu(true);
                    for (int tick = 0; tick < 8 && game.GameState == SuperMetroidGameState.PausedB; tick++)
                        Step(SnesButton.X | SnesButton.Start);
                    for (int tick = 0; tick < 100 && game.GameState != SuperMetroidGameState.Unpausing; tick++)
                        Step(SnesButton.X);
                    if (game.GameState != SuperMetroidGameState.Unpausing || runtime.Projectiles.FlareCounter != chargeBeforeMenu)
                        throw new InvalidDataException("Re-enabling Bombs lost the carried charge.");
                    for (int tick = 0; tick < 20; tick++) Step(SnesButton.X | SnesButton.Down);
                    if (runtime.BombProjectiles.BombCounter != 0)
                        throw new InvalidDataException("Re-enabled Bombs ignored the held-Down delay.");
                }
                Step(SnesButton.X | SnesButton.A | SnesButton.Right);
                if (runtime.BombProjectiles.BombCounter != 5 || runtime.Projectiles.FlareCounter != 0)
                    throw new InvalidDataException("Releasing Down failed to consume charge and produce five bombs.");
            }
        }
        Console.WriteLine($"Pause carry integration baseline: 31 timings plus equipment-toggle case, {successful} no-bounce carries with delayed five-bomb release.");
        return successful == 8 ? 0 : 1;
    }
}
