using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private readonly record struct GreenHillGateRecord(
        int StartX,
        int BaseSpeed,
        bool Opened,
        int OpenFrame,
        int SamusX,
        int SamusSubX,
        int SamusY,
        int Pose,
        int GrappleX,
        int GrappleY,
        ushort GrappleFunction);

    private static void VerifyGreenHillGrappleSpeedGateGlitch()
    {
        GreenHillGateRecord[] expected = File.ReadLines(
                "csharp/test-fixtures/movement-release/green-gate-405-native.csv")
            .Skip(1)
            .Select(ParseGreenHillGateRecord)
            .ToArray();
        AssertEqual(5, expected.Length, "Green Hill native success/control matrix size");
        AssertEqual(2, expected.Count(record => record.Opened),
            "Green Hill native successful fixed-point setup count");

        GreenHillGateRecord[] actual = expected
            .Select(record => RunGreenHillGrappleSpeedGateGlitch(
                record.StartX,
                record.BaseSpeed))
            .ToArray();
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual[index],
                $"Green Hill Grapple/Speed Booster native record {index}");

        AssertTrue(actual[1].Opened && !actual[0].Opened && !actual[2].Opened,
            "one-pixel/one-speed controls bound the first Green Hill success");
        AssertTrue(actual[4].Opened && !actual[3].Opened,
            "one-speed control bounds the adjacent Green Hill success");
        Console.WriteLine(
            "PASS Green Hill actual-room Grapple/Speed Booster gate glitch: both " +
            "cartridge success windows and all three adjacent failures match the " +
            "original CPU, including the opening gate actor.");
    }

    private static GreenHillGateRecord RunGreenHillGrappleSpeedGateGlitch(
        int startX,
        int baseSpeed)
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GreenHillZone);
        RoomPlmSlotSnapshot gate = runtime.Plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        AssertEqual(0x1be4, gate.BlockIndex, "authored Green Hill gate block");
        RoomPlmSlotSnapshot trigger = runtime.Plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGateShotBlock);
        AssertEqual((ushort)0, trigger.RoomArgument,
            "authored Green Hill trigger selects the blue-left gate table");
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        gate = runtime.Plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        RoomEnemyProjectileSlot gateActor = runtime.Enemies.EnemyProjectiles.Single(projectile =>
            projectile.IsActive &&
            projectile.Kind == RoomEnemyProjectileKind.DownwardGateClosed);
        ushort closedGateY = gateActor.YPosition;

        runtime.InitializeDebugGroundedSamus(
            checked((ushort)startX),
            desiredScreenY: 171,
            minimumFloorBlockY: 60);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MovingLeftGunExtendedPose;
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YSubposition = 0;
        samus.EquippedItems = (ushort)(
            SamusEquipmentFlags.GrappleBeam |
            SamusEquipmentFlags.SpeedBooster);
        samus.SelectedHudItem = 4;
        samus.HorizontalSpeed.BaseSpeed = checked((ushort)baseSpeed);
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.HorizontalSpeed.ExtraRunSpeed = 4;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.HorizontalSpeed.SpeedBoostCounter =
            SamusMovementRomData.HorizontalMotion.ActiveSpeedBoostStage;
        samus.HorizontalSpeed.ContactDamageIndex = 1;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        int openFrame = -1;
        for (int frame = 0; frame < 21; frame++)
        {
            SnesButton input = SnesButton.Left | SnesButton.A;
            if (frame >= 1)
                input |= SnesButton.X;
            runtime.StepFrame((ushort)input);
            gate = runtime.Plms.PopulationSlots.Single(slot =>
                slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
            if (gate.LoopTimer == 0)
                continue;
            openFrame = frame;
            break;
        }

        var actual = new GreenHillGateRecord(
            startX,
            baseSpeed,
            openFrame >= 0,
            openFrame,
            samus.XPosition,
            samus.Kinematics.XSubposition,
            samus.YPosition,
            samus.Pose,
            samus.Grapple.AnchorX,
            samus.Grapple.AnchorY,
            samus.Grapple.Phase == GrapplePhase.Firing ? (ushort)0xc703 : (ushort)0xc4f0);

        if (openFrame >= 0)
        {
            for (int frame = 0; frame < 8; frame++)
                runtime.StepFrame((ushort)(SnesButton.Left | SnesButton.A | SnesButton.X));
            gateActor = runtime.Enemies.EnemyProjectiles.Single(projectile =>
                projectile.IsActive &&
                projectile.Kind is RoomEnemyProjectileKind.DownwardGateClosed or
                    RoomEnemyProjectileKind.DownwardGateMoving);
            AssertTrue(gateActor.YPosition < closedGateY,
                "successful Grapple trigger moves the visible gate actor upward");
        }

        return actual;
    }

    private static GreenHillGateRecord ParseGreenHillGateRecord(string line)
    {
        string[] fields = line.Split(',');
        if (fields.Length != 11)
            throw new InvalidDataException($"Malformed Green Hill gate record: {line}");
        return new GreenHillGateRecord(
            int.Parse(fields[0], CultureInfo.InvariantCulture),
            int.Parse(fields[1], CultureInfo.InvariantCulture),
            fields[2] != "0",
            int.Parse(fields[3], CultureInfo.InvariantCulture),
            int.Parse(fields[4], CultureInfo.InvariantCulture),
            int.Parse(fields[5], CultureInfo.InvariantCulture),
            int.Parse(fields[6], CultureInfo.InvariantCulture),
            int.Parse(fields[7], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            int.Parse(fields[8], CultureInfo.InvariantCulture),
            int.Parse(fields[9], CultureInfo.InvariantCulture),
            ushort.Parse(fields[10], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }
}
