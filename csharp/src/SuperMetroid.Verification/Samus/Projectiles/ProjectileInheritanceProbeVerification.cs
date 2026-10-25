using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus projectile launch velocity against native overlapping movement records and live runtime shots.</summary>
internal static class ProjectileInheritanceProbeVerification
{
    /// <summary>
    /// Native block snapshots: CameraYSubSpeed followed by the four integer/fraction pairs at $0DAA..0DB9, deliberately
    /// in the cartridge's word order.
    /// </summary>
    private static readonly ushort[][] InheritanceSnapshots =
    [
        [0, 0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 3, 0, 0, 0, 0, 0],
        [0, 0xfffd, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, 0xfffc, 0, 0, 0],
        [0xabcd, 0, 0x1234, 0, 0x5678, 0, 0x9abc, 0, 0],
    ];

    /// <summary>Base launch speed of a diagonal power beam.</summary>
    private const int DiagonalBaseSpeed = 0x02ab;

    /// <summary>Base launch speed of a straight power beam.</summary>
    private const int StraightBaseSpeed = 0x0400;

    /// <summary>The two top bits an arithmetic shift right by two fills for a negative upward word.</summary>
    private const int UpwardShiftSignFill = 0xc000;

    /// <summary>Replays initialized WRAM snapshots through power-beam velocity setup and confirms live movement and weapon launches.</summary>
    internal static void ProbeProjectileVelocityInheritance()
    {
        var initialize = new Action<ProjectileInheritanceRecords, SamusProjectileSlot>(BeamFiring.InitializePowerBeamVelocity);
        var inheritance = new ProjectileInheritanceRecords();
        var records = new ProjectileInheritanceRecordsAccess(inheritance);
        int failures = 0;
        ushort[][] snapshots = InheritanceSnapshots;
        for (int sample = 0; sample < snapshots.Length; sample++)
        {
            var snapshot = new byte[snapshots[sample].Length * sizeof(ushort)];
            for (int i = 0; i < snapshots[sample].Length; i++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(snapshot.AsSpan(i * sizeof(ushort)), snapshots[sample][i]);
            records.Load(snapshot);
            for (ushort direction = 0; direction < 10; direction++)
            {
                int baseSpeed = direction is 1 or 3 or 6 or 8 ? DiagonalBaseSpeed : StraightBaseSpeed;
                var slot = new SamusProjectileSlot(0) { Direction = direction };
                initialize(inheritance, slot);
                // Direct transcription of loads/LSR/ORA/ADC at $90:B218..B2F5.
                // Do not replace these overlapping reads with idealized fixed-point math.
                ushort upWord = records.WordBefore(SamusCollisionDirection.Up);
                int up = (SnesLittleEndian.HighByteInPlace(upWord)) == 0 ? 0 : (upWord >> 2) | UpwardShiftSignFill;
                short expectedX = unchecked((short)(direction switch
                {
                    1 or 2 or 3 => baseSpeed + records.WordBefore(SamusCollisionDirection.Right),
                    6 or 7 or 8 => -baseSpeed + records.WordBefore(SamusCollisionDirection.Left),
                    _ => 0,
                }));
                short expectedY = unchecked((short)(direction switch
                {
                    0 or 1 or 8 or 9 => -baseSpeed + up,
                    3 or 4 or 5 or 6 => baseSpeed + records.WordBefore(SamusCollisionDirection.Down),
                    _ => 0,
                }));
                if (slot.Velocity.X.SignedRawValue == expectedX && slot.Velocity.Y.SignedRawValue == expectedY) continue;
                failures++;
                Console.WriteLine($"Snapshot {sample}, direction {direction}: native ({expectedX:X4},{expectedY:X4}), port ({slot.Velocity.X.SignedRawValue:X4},{slot.Velocity.Y.SignedRawValue:X4})");
            }
        }
        AssertEqual(0, failures, "Projectile initialization must inherit native movement/camera overlapping words");
        Suite(nameof(VerifyProjectileInheritanceRecordedWords), () => VerifyProjectileInheritanceRecordedWords());
        Suite(nameof(VerifyProjectileInheritanceMovement), () => VerifyProjectileInheritanceMovement());
        for (ushort weapon = 0; weapon < 3; weapon++) VerifyProjectileInheritanceRuntime(weapon);
        Console.WriteLine("Projectile inheritance: original 50 cases, live movement writes and preceding-frame runtime shot pass.");
    }

    /// <summary>Checks that recorded distances produce the overlapping native launch words for all fifty cases.</summary>
    private static void VerifyProjectileInheritanceRecordedWords()
    {
        ushort[][] snapshots = InheritanceSnapshots;
        foreach (ushort[] snapshot in snapshots)
        {
            var inheritance = new ProjectileInheritanceRecords { CameraYSubspeed = snapshot[0] };
            inheritance.Record(SamusCollisionDirection.Left, unchecked((int)SixteenSixteen.Compose(snapshot[1], snapshot[2])));
            inheritance.Record(SamusCollisionDirection.Right, unchecked((int)SixteenSixteen.Compose(snapshot[3], snapshot[4])));
            inheritance.Record(SamusCollisionDirection.Up, unchecked((int)SixteenSixteen.Compose(snapshot[5], snapshot[6])));
            inheritance.Record(SamusCollisionDirection.Down, unchecked((int)SixteenSixteen.Compose(snapshot[7], snapshot[8])));
            for (ushort direction = 0; direction < 10; direction++)
            {
                short speed = (short)(direction is 1 or 3 or 6 or 8 ? DiagonalBaseSpeed : StraightBaseSpeed);
                // These overlapping words are a native load across adjacent fields, not net displacement.
                int upward = SnesLittleEndian.Word(SnesLittleEndian.HighByte(snapshot[4]), unchecked((byte)snapshot[5]));
                int up = SnesLittleEndian.HighByte(unchecked((ushort)upward)) == 0 ? 0 : (unchecked((ushort)upward) >> 2) | UpwardShiftSignFill;
                short x = unchecked((short)(direction switch
                {
                    1 or 2 or 3 => speed + SnesLittleEndian.Word(SnesLittleEndian.HighByte(snapshot[2]), unchecked((byte)snapshot[3])),
                    6 or 7 or 8 => -speed + SnesLittleEndian.Word(SnesLittleEndian.HighByte(snapshot[0]), unchecked((byte)snapshot[1])),
                    _ => 0,
                }));
                short y = unchecked((short)(direction switch
                {
                    0 or 1 or 8 or 9 => -speed + up,
                    3 or 4 or 5 or 6 => speed + SnesLittleEndian.Word(SnesLittleEndian.HighByte(snapshot[6]), unchecked((byte)snapshot[7])),
                    _ => 0,
                }));
                AssertEqual(new EightEightVector<RoomPixels>(x, y), inheritance.ReadVelocity(direction, speed),
                    "projectile inheritance preserves each directional overlapping word");
            }
        }
    }

    /// <summary>Checks directional displacement writes, wall clipping, camera subspeed, and alpha reset behavior.</summary>
    private static void VerifyProjectileInheritanceMovement()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(64, 64)) };
        samus.Kinematics.Radius = new(5, 5);
        var level = CreateRoom(new(16, 16), new ushort[256], new byte[256]);
        samus.ProjectileInheritance.ClearMovement();
        SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = 0x00031234 });
        AssertEqual((short)0x0300, samus.ProjectileInheritance.ReadVelocity(2, 0).X.SignedRawValue,
            "Live movement publishes right integer displacement");
        SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = -0x00018000 });
        AssertEqual(unchecked((short)0xfe00), samus.ProjectileInheritance.ReadVelocity(7, 0).X.SignedRawValue,
            "Negative movement preserves two's-complement integer half");
        AssertEqual((short)0x0380, samus.ProjectileInheritance.ReadVelocity(2, 0).X.SignedRawValue,
            "Opposite direction fraction leaks into rightward velocity, not net displacement");
        SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = 0x00020000 });
        AssertEqual((short)0x0280, samus.ProjectileInheritance.ReadVelocity(2, 0).X.SignedRawValue,
            "Second move overwrites one directional record instead of accumulating");
        var records = new ProjectileInheritanceRecordsAccess(samus.ProjectileInheritance);
        byte[] beforeProbe = records.Capture();
        SamusBlockCollision.ProbeWallHorizontal(level, samus.Kinematics, samus, 1 << 16);
        AssertTrue(beforeProbe.SequenceEqual(records.Capture()),
            "Wall observation cannot replace live movement inheritance");
        level.SetForegroundEntry(4 * 16 + 5, RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw);
        var clipped = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = 20 << 16 });
        AssertTrue(clipped.Collided, "Inheritance fixture actually reaches its solid wall");
        AssertEqual(FixedCoordinate.WholeOf(clipped.AcceptedDisplacement),
            records.DistanceInteger(SamusCollisionDirection.Right),
            "Collision publishes clipped displacement, not the twenty-pixel request");
        samus.ProjectileInheritance.CameraYSubspeed = 0xabcd;
        samus.ProjectileInheritance.ClearMovement();
        AssertEqual(unchecked((short)0xfcab), samus.ProjectileInheritance.ReadVelocity(7, 0x400).X.SignedRawValue,
            "Alpha reset preserves camera fractional high byte");
        AssertTrue(records.CaptureDistanceRecords().All(value => value == 0),
            "Alpha reset clears exactly all eight directional words");
    }

    /// <summary>Fires a beam, missile, or Super Missile after controller-driven movement and verifies the prior-frame record survives save restoration.</summary>
    private static void VerifyProjectileInheritanceRuntime(ushort weapon)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        var level = runtime.Room.LoadedLevelData;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.Size.X + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.SelectedHudItem = HudItems.Decode(weapon);
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(512, 490));
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(400, 350));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        for (int frame = 0; frame < 20; frame++) runtime.StepFrame(ControllerButton.Right);
        var records = new ProjectileInheritanceRecordsAccess(samus.ProjectileInheritance);
        ushort inherited = records.WordBefore(SamusCollisionDirection.Right);
        AssertTrue((SnesLittleEndian.HighByteInPlace(inherited)) != 0, "Controller-driven beta publishes nonzero preceding-frame movement");
        short expected = unchecked((short)(inherited +
            (weapon == 0 ? ReadPowerBeamLaunchSpeedSum(bus) : 0x0100)));
        using var stateStream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stateStream, runtime);
        stateStream.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidRuntime>(stateStream);
        // The host rebinds nonserialized presentation assets after debugger restore.
        BindRetailRuntimeFixture(restored);
        runtime.StepFrame((ControllerButton.Right | runtime.Options.ControllerBindings.Shoot));
        restored.StepFrame((ControllerButton.Right | restored.Options.ControllerBindings.Shoot));
        var shot = runtime.Projectiles.Slots.First(slot => slot.IsActive);
        AssertEqual(expected, shot.Velocity.X.SignedRawValue,
            "Next alpha fires using prior beta displacement before reset and this-frame movement");
        AssertEqual(expected, restored.Projectiles.Slots.First(slot => slot.IsActive).Velocity.X.SignedRawValue,
            "Debugger graph round trip retains native WRAM inheritance for resumed launch");
        AssertEqual(runtime.Room.LoadedCamera.Speed.Y.Fraction, records.CameraYSubspeed,
            "Runtime publishes camera Y subspeed into the native shared record");
        byte[] beforeFreeze = records.CaptureDistanceRecords();
        runtime.Freeze.GameplayTimeFrozen = true;
        runtime.StepFrame(0);
        AssertTrue(beforeFreeze.SequenceEqual(records.CaptureDistanceRecords()),
            "Frozen gameplay skips the projectile alpha and preserves directional records");
        runtime.Freeze.GameplayTimeFrozen = false;
        samus.Control.LockHandlers();
        runtime.StepFrame(0);
        AssertTrue(records.CaptureDistanceRecords().All(value => value == 0),
            "Input-locked projectile alpha still clears directional movement records");
    }

    /// <summary>The two retail speed words the uncharged power beam adds to inherited launch velocity.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the power beam launch speed words")]
    private static int ReadPowerBeamLaunchSpeedSum(ISnesAddressSpace rom)
    {
        return CartridgeImportSource.Require(rom).ReadCartridgeWord(0x90c2d1) + CartridgeImportSource.Require(rom).ReadCartridgeWord(0x90c357);
    }

    /// <summary>Runs the projectile velocity inheritance verification command.</summary>
    [VerificationCommand("--projectile-inheritance-probe")]
    internal static int ProjectileInheritanceProbeCommand()
    {
        ProbeProjectileVelocityInheritance();
        return 0;
    }
}
