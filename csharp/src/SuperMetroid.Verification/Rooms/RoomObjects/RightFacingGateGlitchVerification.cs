using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks two reported right-facing gate activations against captured native success and control records.</summary>
internal static class RightFacingGateGlitchVerification
{
    /// <summary>One East Tunnel trace containing frozen-enemy placement, shot timing, gate result, and final actor coordinates.</summary>
    internal readonly record struct EastTunnelGateRecord(
        int EnemyX,
        int ShootFrame,
        bool Opened,
        int OpenFrame,
        Position16<RoomPixels> Samus,
        int SamusSubX,
        int Pose,
        Position16<RoomPixels> Shot);

    /// <summary>Runs the Pink Brinstar high-speed shot and East Tunnel frozen-enemy gate reproductions.</summary>
    internal static void VerifyRightFacingGateGlitches()
    {
        Suite(nameof(VerifyPinkBrinstarRightFacingGateGlitch), () => VerifyPinkBrinstarRightFacingGateGlitch());
        Suite(nameof(VerifyEastTunnelFrozenEnemyGateGlitch), () => VerifyEastTunnelFrozenEnemyGateGlitch());
    }

    /// <summary>Compares every actual-room launch origin with ten native right-gate positives and their negative controls.</summary>
    private static void VerifyPinkBrinstarRightFacingGateGlitch()
    {
        string[] expected = File.ReadLines(
            "csharp/test-fixtures/movement-release/right-gate-404-native.csv").
            Skip(1).
            ToArray();
        AssertEqual(10, expected.Length, "Complete native Pink Brinstar right-gate positive set");

        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.PinkBrinstarHoppers);

        RoomLevelData original = runtime.Room.LoadedLevelData;
        var residentGate = runtime.RoomObjects.PopulationSlots.Single(
            slot => slot.Kind == RoomObjectKind.DownwardGate);
        var shotBlock = runtime.RoomObjects.PopulationSlots.Single(
            slot => slot.Kind == RoomObjectKind.DownwardGateShotBlock);
        AssertEqual(32, original.Size.X, "Pink Brinstar Hopper room width");
        AssertEqual(32, original.Size.Y, "Pink Brinstar Hopper room height");
        AssertEqual(new Position16<RoomBlocks>(17, 4), residentGate.Block, "Authored Pink Brinstar gate block");
        AssertEqual((ushort)0x0002, shotBlock.RoomArgument,
            "Authored Pink Brinstar blue-right trigger table row");

        int gateX = residentGate.Block.X * 16;
        int gateY = residentGate.Block.Y * 16;
        int cameraX = gateX - 128;
        int cameraY = 0;
        // The authored population is an immutable import record, read from the cartridge once for every position.
        RoomObjectPopulation population = RoomObjectPopulationImporter.Read(bus, runtime.Room.LoadedRoom.State.RoomObjectPointer);
        RoomObjectSystem unboundObjects = RoomObjectFixtures.Unbound();
        var areaIndex = runtime.Room.LoadedRoom.AreaIndex;
        // The gate and its activations read only terrain and behaviour, so every position's level shows a blank background.
        var background = new RoomBlockAppearance[original.Size.X * original.Size.Y];
        int firstX = gateX - 192;
        var activationsByColumn = new List<string>[gateX - firstX];
        // The shared installed fixtures every position's Samus, shots and bombs are built from load here, on the suite's
        // own thread, as timed shared fixture loads; the workers then only read them.
        SuperMetroid.Core.Compatibility.LaidOutProgramContinuation<SamusPoseId, SamusAnimationProgramStep> animationContinuation = InstalledFixtures.AnimationContinuation;
        _ = CreateProjectileFixture();
        _ = CreateBombFixture();
        // Each launch column runs on its own worker with its own levels and VRAM (#1532): a position reads only the
        // immutable population, the installed definitions every fixture shares unchanged, and its own Samus, PLM, shot
        // and bomb systems, so the columns are independent.
        ParallelSweep.For(firstX, gateX, x =>
        {
            var columnActivations = new List<string>();
            // PLM setup only uploads into VRAM; nothing below reads it, so every position of a column shares one image.
            var populationVram = new SnesVram();
            // One pristine level and one working level restored from it per position. The
            // field-by-field check proves no state from the previous position survives.
            RoomLevelData NewLevel() => new RoomLevelData(original.Size, new RoomLevelPlanes(original.Terrain, original.Behaviors, background), RoomLevelData.NothingBeyondGrid());
            RoomLevelData pristine = NewLevel();
            RoomLevelData level = NewLevel();
            for (int y = gateY - 32; y <= gateY + 96; y++)
            {
                level.RestoreFrom(pristine);
                AssertTrue(InstanceStateEquals(level, pristine), "Restored gate-glitch level matches a freshly built level");
                var samus = new SamusState(animationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
                {
                    Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)x), unchecked((ushort)y))),
                    PoseId = SamusPoseId.FacingRightNormalPose,
                    SelectedHudItem = HudItem.SuperMissile,
                    SuperMissiles = 10,
                    MaxSuperMissiles = 10,
                };
                // Loading binds a new PLM system to this position's level, so every position starts from the same unbound one.
                RoomObjectSystem roomObjects = unboundObjects;
                RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(populationVram), new SharedGameState(new SharedRandom()), areaIndex), population, new RoomObjectServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
                for (int warm = 0; warm < 2; warm++)
                    roomObjects.Step(level);
                roomObjects.Gates.TakeDownwardGateProjectileRequests();

                var shots = CreateProjectileFixture();
                var bombs = CreateBombFixture();
                for (int frame = 0; frame < 30; frame++)
                {
                    ControllerButton input = frame == 0 ? ControllerButton.X : 0;
                    bombs.StepFrame(new SamusTerrain { Level = level }, samus, input, input);
                    shots.StepFrame(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new((ushort)cameraX, (ushort)cameraY), bombs);
                    var gate = roomObjects.SinglePopulationSlot(RoomObjectKind.DownwardGate);
                    if (gate.Behaviour is DownwardGateObject { Triggered: true })
                    {
                        SamusProjectile shot = shots.Slots[0];
                        columnActivations.Add($"{x},{y},{frame},{shot.Position.X.Whole},{shot.Position.Y.Whole}");
                        break;
                    }
                    roomObjects.Step(level);
                }
            }
            activationsByColumn[x - firstX] = columnActivations;
        });
        // Column by column and row by row within a column, the order the native matrix lists its positives in.
        List<string> actual = [.. activationsByColumn.SelectMany(column => column)];

        AssertEqual(expected.Length, actual.Count,
            "Pink Brinstar right-gate native positive count; omitted matrix rows are negative controls");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual[index],
                $"Pink Brinstar right-gate native launch record {index}");
        Console.WriteLine(
            "PASS 24768 Pink Brinstar actual-room origins: all ten high-speed Super " +
            "right-gate activations and every negative match the original CPU.");
    }

    /// <summary>Compares nine native frozen-Boyon shot timings, including the successful resumed frame, with production gameplay.</summary>
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

    /// <summary>Loads East Tunnel, positions a frozen Boyon, and records the gate switch after the specified resumed shot frame.</summary>
    private static EastTunnelGateRecord RunEastTunnelFrozenEnemyGateGlitch(
        int frozenEnemyX,
        int shootFrame)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.EastTunnel);

        RoomLevelData level = runtime.Room.LoadedLevelData;
        var gate = runtime.RoomObjects.PopulationSlots.Single(
            slot => slot.Kind == RoomObjectKind.DownwardGate);
        var shotBlock = runtime.RoomObjects.PopulationSlots.Single(
            slot => slot.Kind == RoomObjectKind.DownwardGateShotBlock);
        AssertEqual(64, level.Size.X, "East Tunnel room width");
        AssertEqual(32, level.Size.Y, "East Tunnel room height");
        AssertEqual(new Position16<RoomBlocks>(22, 5), gate.Block, "Authored East Tunnel gate block");
        AssertEqual((ushort)0x000a, shotBlock.RoomArgument,
            "Authored East Tunnel green-right trigger table row");

        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWholeX(238);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(8191);
        samus.Position = samus.Position.WithWholeY(139);
        samus.EquippedItems = SamusEquipmentFlags.HiJumpBoots;
        samus.EquippedBeams = SamusBeamFlags.Ice | SamusBeamFlags.Wave;
        samus.SelectedHudItem = HudItem.SuperMissile;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(128, 0));

        RoomEnemySlot frozen = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.DefinitionId == EnemyDefinitionId.Boyon);
        foreach (RoomEnemySlot enemy in runtime.Enemies.SlotTable.Slots)
        {
            if (!ReferenceEquals(enemy, frozen))
                enemy.Bind(0, enemy.Definition);
        }
        frozen.Position = frozen.Position.WithWhole(new(checked((ushort)frozenEnemyX), 139));
        frozen.FrozenTimer = 1000;
        frozen.AiStates |= EnemyAiStates.Frozen;

        // The cartridge continues normal gameplay during pause darkening. The published
        // setup holds right+dash for all 31 accepted samples, introduces jump+dash+angle
        // on the first resumed sample, then fires at the candidate resumed frame.
        for (int frame = 0; frame < 31; frame++)
            runtime.StepFrame((ControllerButton.Right | ControllerButton.B));

        int openFrame = -1;
        Position16<RoomPixels> shotPosition = Position16<RoomPixels>.Zero;
        for (int frame = 0; frame < 40; frame++)
        {
            ControllerButton input = ControllerButton.A | ControllerButton.B | ControllerButton.R;
            if (frame == shootFrame)
                input |= ControllerButton.X;
            runtime.StepFrame(input);
            gate = runtime.RoomObjects.PopulationSlots.Single(
                slot => slot.Kind == RoomObjectKind.DownwardGate);
            if (gate.Behaviour is not DownwardGateObject { Triggered: true })
                continue;

            openFrame = frame;
            // The switch consumes the projectile type in the same frame but, as on the
            // cartridge, slot zero retains the impact coordinates used by this trace.
            SamusProjectile shot = runtime.Projectiles.Slots[0];
            shotPosition = shot.Position.Whole;
            break;
        }

        return new EastTunnelGateRecord(
            frozenEnemyX,
            shootFrame,
            openFrame >= 0,
            openFrame,
            samus.Position.Whole,
            samus.Kinematics.Position.X.Fraction,
            SamusPoseNumbers.Number(samus.Pose),
            shotPosition);
    }

    /// <summary>Parses one ten-field native East Tunnel outcome from the recorded CSV fixture.</summary>
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
            new Position16<RoomPixels>(checked((ushort)fields[4]), checked((ushort)fields[6])),
            fields[5],
            fields[7],
            new Position16<RoomPixels>(checked((ushort)fields[8]), checked((ushort)fields[9])));
    }

    /// <summary>Runs both reported gate-glitch comparisons against the local retail cartridge.</summary>
    [VerificationCommand("--right-facing-gate-glitch")]
    internal static int RightFacingGateGlitchCommand()
    {
        Suite(nameof(VerifyRightFacingGateGlitches), () => VerifyRightFacingGateGlitches());
        return 0;
    }
}
