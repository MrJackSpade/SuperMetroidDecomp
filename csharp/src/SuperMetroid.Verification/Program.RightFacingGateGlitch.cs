using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Captures the cartridge comparison fields for one East Tunnel frozen-enemy gate attempt.</summary>
    /// <param name="EnemyX">Horizontal spawn position assigned to the frozen Boyon before the pause sequence.</param>
    /// <param name="ShootFrame">Resumed gameplay frame on which the Super Missile is fired.</param>
    /// <param name="Opened">Whether the gate's activation timer changed, indicating the projectile triggered its switch.</param>
    /// <param name="OpenFrame">Resumed frame of switch activation, or the sentinel returned by the trace when no activation occurred.</param>
    /// <param name="SamusX">Samus's whole-pixel horizontal position when the trace is captured.</param>
    /// <param name="SamusSubX">Samus's horizontal subpixel position at capture time.</param>
    /// <param name="SamusY">Samus's whole-pixel vertical position when the trace is captured.</param>
    /// <param name="Pose">Numeric pose value used to compare the resulting Samus state with the native trace.</param>
    /// <param name="ShotX">Projectile slot zero's retained impact X coordinate, or zero when the gate never activates.</param>
    /// <param name="ShotY">Projectile slot zero's retained impact Y coordinate, or zero when the gate never activates.</param>
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

    /// <summary>Runs the Pink Brinstar right-gate projectile matrix and the East Tunnel frozen-enemy comparison.</summary>
    private static void VerifyRightFacingGateGlitches()
    {
        Suite(nameof(VerifyPinkBrinstarRightFacingGateGlitch), () => VerifyPinkBrinstarRightFacingGateGlitch());
        Suite(nameof(VerifyEastTunnelFrozenEnemyGateGlitch), () => VerifyEastTunnelFrozenEnemyGateGlitch());
    }

    /// <summary>Compares actual-room Pink Brinstar gate activations across the sampled Samus-position matrix with native records.</summary>
    private static void VerifyPinkBrinstarRightFacingGateGlitch()
    {
        string[] expected = File.ReadLines(
            "csharp/test-fixtures/movement-release/right-gate-404-native.csv").
            Skip(1).
            ToArray();
        AssertEqual(10, expected.Length, "Complete native Pink Brinstar right-gate positive set");

        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
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
        // PLM setup only uploads into VRAM; nothing below reads it, so every position shares one image.
        var populationVram = new SnesVram();
        // One pristine level and one working level restored from it per position. The
        // field-by-field check proves no state from the previous position survives.
        RoomLevelData NewLevel() => new(
            original.WidthInBlocks,
            original.HeightInBlocks,
            original.ForegroundEntries.Span,
            original.BehaviorBytes.Span,
            original.BackgroundEntries.Span,
            original.BlockDefinitions.Span);
        RoomLevelData pristine = NewLevel();
        RoomLevelData level = NewLevel();
        for (int x = gateX - 192; x < gateX; x++)
        for (int y = gateY - 32; y <= gateY + 96; y++)
        {
            level.RestoreFrom(pristine);
            AssertTrue(InstanceStateEquals(level, pristine), "Restored gate-glitch level matches a freshly built level");
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
                populationVram,
                RoomPlmPopulationImporter.Read(bus, runtime.ActiveRoom!.State.PlmPointer),
                new Bank80SystemState(),
                runtime.ActiveRoom.AreaIndex,
                () => samus,
                () => false);
            for (int warm = 0; warm < 2; warm++)
                plms.Step(bus, level, streamer, (ushort)cameraX, (ushort)cameraY, 0);
            plms.TakeDownwardGateProjectileRequests();

            var shots = CreateProjectileFixture();
            var bombs = CreateBombFixture();
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
                var gate = plms.SinglePopulationSlot(RoomPlmHeaders.DownwardGate);
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

    /// <summary>Checks the native East Tunnel success and control cases for firing after gameplay resumes from pause.</summary>
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

    /// <summary>Replays one frozen-Boyon position and resumed firing frame, returning the observed gate and actor state.</summary>
    /// <param name="frozenEnemyX">Whole-pixel X position assigned to the Boyon held frozen during pause.</param>
    /// <param name="shootFrame">Resumed update on which Samus fires the Super Missile.</param>
    /// <returns>The gate result and captured Samus/projectile coordinates for comparison with the native trace.</returns>
    private static EastTunnelGateRecord RunEastTunnelFrozenEnemyGateGlitch(
        int frozenEnemyX,
        int shootFrame)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
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

    /// <summary>Parses one comma-separated native trace row containing ten integer fields in record order.</summary>
    /// <param name="line">CSV row whose values describe a single East Tunnel gate attempt.</param>
    /// <returns>The typed record represented by the row.</returns>
    /// <exception cref="InvalidDataException">The row does not contain exactly ten fields.</exception>
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
