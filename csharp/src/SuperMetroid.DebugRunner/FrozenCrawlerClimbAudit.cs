using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Controller exploration of Super detachment followed by an actual Ice shot.</summary>
internal static class FrozenCrawlerClimbAudit
{
    public static int Run(string rom, string? directory = null)
    {
        if (directory is not null) Directory.CreateDirectory(directory);
        int frozenInAir = 0;
        foreach (int shoot in directory is null ? Enumerable.Range(20, 26) : new[] { 26, 27, 38, 39 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            var level = runtime.LevelData!;
            for (int y = 0; y < 16; y++)
                level.SetForegroundEntry(level.GetBlockIndex(18, y), RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw);
            for (int x = 2; x < 11; x++)
                level.SetForegroundEntry(level.GetBlockIndex(x, 6), RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw);
            var samus = runtime.Samus!;
            samus.EquippedBeams = (ushort)SamusBeamFlags.Ice;
            var population = new PopulationSelectionAddressSpace(bus,
                [new RoomEnemyPopulationRecord(RoomEnemySystem.ZoomerDefinition, 128, 120,
                    0, (ushort)(EnemyProperties.ProcessInstructions | EnemyProperties.ProcessOffScreen), 0, 0, 0)]);
            runtime.Enemies.Load(population, PopulationSelectionAddressSpace.PopulationPointer,
                PopulationSelectionAddressSpace.TilesetPointer, runtime.Vram, runtime.Cgram, () => 1, samus: samus);
            // Let the retail instruction stream publish its first real spritemap.
            // The detachment-only fixture could leave the initial invisible map;
            // that map deliberately rejects projectile collision and cannot test Ice.
            runtime.Enemies.StepFrame(0, 0, false, samus: samus, level: level);
            var enemy = runtime.Enemies.Slots[0];
            var state = runtime.Enemies.CrawlerStates[0]!;
            state.Function = CrawlerEnemyFunction.CrawlingHorizontally;
            state.XVelocity = 0;
            state.YVelocity = unchecked((ushort)-128);
            enemy.YPosition = (ushort)(112 + enemy.YRadius);
            enemy.InstructionTimer = ushort.MaxValue;
            samus.SelectedHudItem = 2;
            samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            string? prefix = directory is null ? null : Path.Combine(directory, $"crawler-{shoot}");
            using var trace = prefix is null ? null : new StreamWriter(prefix + ".jsonl");
            if (prefix is not null)
            {
                RoomMovementSeedExporter.Write(runtime, prefix + ".movement-seed");
                File.WriteAllText(prefix + ".metadata.json", System.Text.Json.JsonSerializer.Serialize(new
                {
                    Format = "frozen-crawler-climb-v1", NativeParityEstablished = false,
                    samus.Health, samus.EquippedBeams, samus.SelectedHudItem, samus.SuperMissiles,
                    Enemy = enemy, Crawler = state,
                    Scrolls = runtime.Camera!.Scrolls.Storage.ToArray(),
                }));
            }
            int fallFrame = -1, freezeFrame = -1;
            ushort freezeY = 0, freezeTimer = 0;
            uint frozenPosition = 0;
            int supportFrames = 0, thawFrame = -1;
            int freezingBeamSlot = -1;
            for (int frame = 0; frame < 500; frame++)
            {
                SnesButton input = frame == 0 ? SnesButton.X : 0;
                if (frame == 13) input |= SnesButton.Y;
                if (frame == 16) input |= SnesButton.Left;
                if (frame >= shoot && frame < shoot + 4) input |= SnesButton.X;
                if (frame >= 60 && frame < 110) input |= SnesButton.Left | SnesButton.A;
                runtime.StepFrame((ushort)input);
                if (freezingBeamSlot >= 0 && frame == freezeFrame + 1 &&
                    runtime.Projectiles.Slots[freezingBeamSlot].IsActive)
                    throw new InvalidDataException("Enemy-hit Ice beam survived its next projectile pass.");
                trace?.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
                {
                    Frame = frame, Input = (ushort)input,
                    samus.Kinematics.XFixed, samus.Kinematics.YFixed, samus.Pose,
                    samus.AnimationFrame, samus.AnimationFrameTimer, samus.Health, samus.SuperMissiles,
                    CameraX = runtime.Camera!.XPosition, CameraY = runtime.Camera.YPosition,
                    Scrolls = runtime.Camera.Scrolls.Storage.ToArray(),
                    Enemy = new { enemy.XPosition, enemy.XSubposition, enemy.YPosition, enemy.YSubposition,
                        enemy.Health, enemy.FrozenTimer, enemy.FlashTimer, enemy.AiHandlerBits,
                        enemy.CurrentInstruction, enemy.InstructionTimer, enemy.SpritemapPointer },
                    Crawler = state,
                    Support = samus.Kinematics.SolidEnemyCollisionIndexes,
                    Shots = runtime.Projectiles.Slots.Select(p => new { p.Type, p.XPosition, p.YPosition }),
                }));
                if (fallFrame < 0 && state.Function == CrawlerEnemyFunction.Falling) fallFrame = frame;
                if (freezeFrame < 0 && enemy.FrozenTimer != 0)
                {
                    freezeFrame = frame; freezeY = enemy.YPosition; freezeTimer = enemy.FrozenTimer;
                    frozenPosition = ((uint)enemy.YPosition << 16) | enemy.YSubposition;
                    // Original-CPU comparison: enemy collision marks the live beam;
                    // it does not convert it into a terrain-style explosion this frame.
                    freezingBeamSlot = runtime.Projectiles.Slots.ToList().FindIndex(p =>
                        p.PackedType.Family == SamusProjectileFamily.Beam &&
                        p.PackedDirection.HasLowByteLifecycleState);
                    if (freezingBeamSlot < 0)
                        throw new InvalidDataException("Freezing hit lost the beam's native collision marker.");
                }
                if (freezeFrame >= 0 && enemy.FrozenTimer != 0 &&
                    (((uint)enemy.YPosition << 16) | enemy.YSubposition) != frozenPosition)
                    throw new InvalidDataException("Ice-frozen falling crawler changed its vertical position.");
                if (freezeFrame >= 0 && thawFrame < 0 && enemy.FrozenTimer == 0) thawFrame = frame;
                if (thawFrame >= 0 && frame > thawFrame &&
                    samus.Kinematics.SolidEnemyCollisionIndexes[(int)SamusCollisionDirection.Down] == enemy.NativeIndex)
                    throw new InvalidDataException("Thawed crawler retained frozen-platform support.");
                if (enemy.FrozenTimer != 0 &&
                    samus.Kinematics.SolidEnemyCollisionIndexes[(int)SamusCollisionDirection.Down] == enemy.NativeIndex)
                {
                    if (samus.Kinematics.BottomBoundary != enemy.YPosition - enemy.YRadius)
                        throw new InvalidDataException("Crawler support did not align Samus's feet with the real enemy top.");
                    supportFrames++;
                }
            }
            bool midair = freezeFrame >= 0 && freezeY + enemy.YRadius < 256;
            if (midair) frozenInAir++;
            if (directory is not null)
            {
                bool expectedFrozen = shoot is 27 or 38;
                if (fallFrame != 11 || midair != expectedFrozen ||
                    (expectedFrozen ? supportFrames == 0 || freezeTimer != 400 || thawFrame < 0 || enemy.YPosition <= freezeY : supportFrames != 0))
                    throw new InvalidDataException($"Managed crawler candidate changed at shoot frame {shoot}; inspect the exported sequence.");
            }
            Console.WriteLine($"CLIMB shoot={shoot} fall={fallFrame} freeze={freezeFrame} y={freezeY} timer={freezeTimer} midair={midair} support={supportFrames} thaw={thawFrame}");
        }
        Console.WriteLine($"CLIMB midair candidates={frozenInAir}; original-CPU confirmation of the connected sequence remains required.");
        return 0;
    }
}
