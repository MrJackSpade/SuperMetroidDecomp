using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Exploratory input search only; candidates require independent cartridge replay.</summary>
internal static class BombTraversalSearch
{
    public static int Run(string rom, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        int candidates = 0;
        foreach (bool ceiling in new[] { false, true })
        for (int interval = 24; interval <= 30; interval++)
        for (int offset = 1; offset <= 8; offset++)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            var level = runtime.LevelData!;
            for (int x = 0; x < level.WidthInBlocks; x++)
                level.SetForegroundEntry((ceiling ? 12 : 0) * level.WidthInBlocks + x, 0x8000);
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.XPosition = 128; samus.YPosition = 249;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.SetAnimationFrameFromSpecialHandler(0, 1);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(((byte)SamusMovementType.MorphBallGround << 8) | 8);
            samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
            runtime.Controller1.Latch(0);
            int targetX = 128 + offset, launches = 0, floorReturns = 0, ceilingHits = 0;
            var inputs = new List<ushort>();
            for (int frame = 0; frame < 600; frame++)
            {
                bool shoot = frame == 0 || frame >= 52 && (frame - 52) % interval == 0;
                if (shoot) targetX = samus.XPosition + (frame >= 170 ? offset : 0);
                // Aim beside the NEXT blast, not the newest bomb (which may still
                // have another half-cycle before it catches Samus). The dead band
                // avoids alternating left/right inertia overshoot around one pixel.
                // These reads choose controller input; they never alter game state.
                var nextBomb = runtime.BombProjectiles.Slots.Where(bomb => bomb.IsActive && bomb.BombTimer >= 9).OrderBy(bomb => bomb.BombTimer).FirstOrDefault();
                if (nextBomb is not null) targetX = nextBomb.XPosition + (frame >= 170 ? offset : 0);
                ushort input = shoot ? runtime.ControllerBindings.Shoot : (ushort)0;
                if (samus.XPosition < targetX - 3) input |= (ushort)SnesButton.Right;
                if (samus.XPosition > targetX + 3) input |= (ushort)SnesButton.Left;
                inputs.Add(input);
                runtime.StepFrame(input);
                if (runtime.LastBombJumpMovement is { Started: true }) launches++;
                if (frame > 170 && samus.YPosition >= 249) floorReturns++;
                if (frame > 170 && (runtime.LastMorphBallMovement is { HitCeiling: true } || runtime.LastBombJumpMovement is { Vertical.Collided: true } && samus.YPosition == 215)) ceilingHits++;
            }
            if (launches >= 8 && floorReturns == 0 && samus.XPosition > 160)
            {
                candidates++;
                string path = Path.Combine(outputDirectory, $"bomb-traversal-ceiling-{ceiling}-interval-{interval}-offset-{offset}.csv");
                using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                using var writer = new StreamWriter(stream);
                writer.WriteLine("frame,input");
                for (int frame = 0; frame < inputs.Count; frame++) writer.WriteLine($"{frame},{inputs[frame]:X4}");
                Console.WriteLine($"Candidate ceiling={ceiling} interval={interval} offset={offset}: x={samus.XPosition}, y={samus.YPosition}, launches={launches}, floor={floorReturns}, ceilingHits={ceilingHits}; {path}");
            }
        }
        Console.WriteLine($"Found {candidates} candidate sequences. These are not cartridge parity evidence.");
        return candidates > 0 ? 0 : 1;
    }
}
