using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private readonly record struct EastTunnelGateRecord(
        int EnemyX,
        int ShootFrame,
        bool Opened,
        int OpenFrame,
        int SamusX,
        int SamusSubX,
        int SamusY,
        int Pose,
        int ShotX,
        int ShotY);

    private static void VerifyRightFacingGateGlitches()
    {
        VerifyPinkBrinstarRightFacingGateGlitch();
        VerifyEastTunnelFrozenEnemyGateGlitch();
    }

    private static void VerifyPinkBrinstarRightFacingGateGlitch()
    {
        string[] expected = File.ReadLines(
            "csharp/test-fixtures/movement-release/right-gate-404-native.csv").
            Skip(1).
            ToArray();
        AssertEqual(10, expected.Length, "Complete native Pink Brinstar right-gate positive set");

        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.PinkBrinstarHopper);

        RoomLevelData original = runtime.LevelData!;
        var residentGate = runtime.Plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        var shotBlock = runtime.Plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGateShotBlock);
        AssertEqual(32, original.WidthInBlocks, "Pink Brinstar Hopper room width");
        AssertEqual(32, original.HeightInBlocks, "Pink Brinstar Hopper room height");
        AssertEqual(0x0091, residentGate.BlockIndex, "Authored Pink Brinstar gate block");
        AssertEqual((ushort)0x0002, shotBlock.RoomArgument,
            "Authored Pink Brinstar blue-right trigger table row");

        int gateX = residentGate.BlockIndex % original.WidthInBlocks * 16;
        int gateY = residentGate.BlockIndex / original.WidthInBlocks * 16;
        int cameraX = gateX - 128;
        int cameraY = 0;
        var actual = new List<string>();
        for (int x = gateX - 192; x < gateX; x++)
        for (int y = gateY - 32; y <= gateY + 96; y++)
        {
            var level = new RoomLevelData(
                original.WidthInBlocks,
                original.HeightInBlocks,
                original.ForegroundEntries.Span,
                original.BehaviorBytes.Span,
                original.BackgroundEntries.Span,
                original.BlockDefinitions.Span);
            var samus = new SamusState
            {
                XPosition = unchecked((ushort)x),
                YPosition = unchecked((ushort)y),
                PoseId = SamusPoseId.FacingRightNormalPose,
                SelectedHudItem = 2,
                SuperMissiles = 10,
                MaxSuperMissiles = 10,
            };
            var plms = new RoomPlmSystem();
            var streamer = level.CreateBackgroundStreamer(0);
            plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                runtime.ActiveRoom!.State.PlmPointer,
                new Bank80SystemState(),
                runtime.ActiveRoom.AreaIndex,
                () => samus,
                () => false);
            for (int warm = 0; warm < 2; warm++)
                plms.Step(bus, level, streamer, (ushort)cameraX, (ushort)cameraY, 0);
            plms.TakeDownwardGateProjectileRequests();

            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 30; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                bombs.StepFrame(bus, level, samus, input, input);
                shots.StepFrame(
                    bus,
                    level,
                    samus,
                    input,
                    input,
                    (ushort)cameraX,
                    (ushort)cameraY,
                    bombs,
                    roomPlms: plms);
                var gate = plms.PopulationSlots.Single(
                    slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
                if (gate.LoopTimer != 0)
                {
                    SamusProjectileSlot shot = shots.Slots[0];
                    actual.Add($"{x},{y},{frame},{shot.XPosition},{shot.YPosition}");
                    break;
                }
                plms.Step(bus, level, streamer, (ushort)cameraX, (ushort)cameraY, 0);
            }
        }

        AssertEqual(expected.Length, actual.Count,
            "Pink Brinstar right-gate native positive count; omitted matrix rows are negative controls");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual[index],
                $"Pink Brinstar right-gate native launch record {index}");
        Console.WriteLine(
            "PASS 24768 Pink Brinstar actual-room origins: all ten high-speed Super " +
            "right-gate activations and every negative match the original CPU.");
    }

    private static void VerifyEastTunnelFrozenEnemyGateGlitch()
    {
        EastTunnelGateRecord[] expected = File.ReadLines(
                "csharp/test-fixtures/movement-release/east-gate-404-native.csv")
            .Skip(1)
            .Select(ParseEastTunnelRecord)
            .ToArray();
        AssertEqual(9, expected.Length, "East Tunnel native success/control matrix size");
        AssertEqual(1, expected.Count(record => record.Opened),
            "East Tunnel native gate-glitch positive count");

        var actual = new List<EastTunnelGateRecord>();
        foreach (EastTunnelGateRecord native in expected)
            actual.Add(RunEastTunnelFrozenEnemyGateGlitch(native.EnemyX, native.ShootFrame));

        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual[index],
                $"East Tunnel native frozen-enemy launch record {index}");

        EastTunnelGateRecord success = actual.Single(record => record.Opened);
        AssertEqual(364, success.EnemyX, "East Tunnel successful frozen Boyon X");
        AssertEqual(5, success.ShootFrame, "East Tunnel successful resumed shot frame");
        AssertEqual(7, success.OpenFrame, "East Tunnel actual gate-switch activation frame");
        Console.WriteLine(
            "PASS East Tunnel actual-room pause handoff: the frozen Boyon X=364/frame-5 " +
            "Super activates the real switch, while all eight adjacent controls fail " +
            "exactly as on the original CPU.");
    }

    private static EastTunnelGateRecord RunEastTunnelFrozenEnemyGateGlitch(
        int frozenEnemyX,
        int shootFrame)
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.EastTunnel);

        RoomLevelData level = runtime.LevelData!;
        var gate = runtime.Plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        var shotBlock = runtime.Plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGateShotBlock);
        AssertEqual(64, level.WidthInBlocks, "East Tunnel room width");
        AssertEqual(32, level.HeightInBlocks, "East Tunnel room height");
        AssertEqual(0x0156, gate.BlockIndex, "Authored East Tunnel gate block");
        AssertEqual((ushort)0x000a, shotBlock.RoomArgument,
            "Authored East Tunnel green-right trigger table row");

        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = 238;
        samus.Kinematics.XSubposition = 8191;
        samus.YPosition = 139;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots;
        samus.EquippedBeams = (ushort)(SamusBeamFlags.Ice | SamusBeamFlags.Wave);
        samus.SelectedHudItem = 2;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(128, 0);

        RoomEnemySlot frozen = runtime.Enemies.Slots.First(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.BoyonDefinition);
        foreach (RoomEnemySlot enemy in runtime.Enemies.Slots)
        {
            if (!ReferenceEquals(enemy, frozen))
                enemy.EnemyDefinitionPointer = 0;
        }
        frozen.XPosition = checked((ushort)frozenEnemyX);
        frozen.YPosition = 139;
        frozen.FrozenTimer = 1000;
        frozen.AiHandlerBits |= 4;

        // The cartridge continues normal gameplay during pause darkening. The published
        // setup holds right+dash for all 31 accepted samples, introduces jump+dash+angle
        // on the first resumed sample, then fires at the candidate resumed frame.
        for (int frame = 0; frame < 31; frame++)
            runtime.StepFrame((ushort)(SnesButton.Right | SnesButton.B));

        int openFrame = -1;
        int shotX = 0;
        int shotY = 0;
        for (int frame = 0; frame < 40; frame++)
        {
            SnesButton input = SnesButton.A | SnesButton.B | SnesButton.R;
            if (frame == shootFrame)
                input |= SnesButton.X;
            runtime.StepFrame((ushort)input);
            gate = runtime.Plms.PopulationSlots.Single(
                slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
            if (gate.LoopTimer == 0)
                continue;

            openFrame = frame;
            // The switch consumes the projectile type in the same frame but, as on the
            // cartridge, slot zero retains the impact coordinates used by this trace.
            SamusProjectileSlot shot = runtime.Projectiles.Slots[0];
            shotX = shot.XPosition;
            shotY = shot.YPosition;
            break;
        }

        return new EastTunnelGateRecord(
            frozenEnemyX,
            shootFrame,
            openFrame >= 0,
            openFrame,
            samus.XPosition,
            samus.Kinematics.XSubposition,
            samus.YPosition,
            samus.Pose,
            shotX,
            shotY);
    }

    private static EastTunnelGateRecord ParseEastTunnelRecord(string line)
    {
        int[] fields = line.Split(',').Select(int.Parse).ToArray();
        if (fields.Length != 10)
            throw new InvalidDataException($"Malformed East Tunnel native record: {line}");
        return new EastTunnelGateRecord(
            fields[0],
            fields[1],
            fields[2] != 0,
            fields[3],
            fields[4],
            fields[5],
            fields[6],
            fields[7],
            fields[8],
            fields[9]);
    }
}
