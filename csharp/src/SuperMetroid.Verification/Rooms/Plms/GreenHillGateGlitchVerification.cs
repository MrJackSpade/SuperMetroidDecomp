using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Globalization;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Compares Green Hill Grapple and Speed Booster gate triggers with native success and control records.</summary>
internal static class GreenHillGateGlitchVerification
{
    /// <summary>Captures one setup's gate opening frame, Samus position and pose, and Grapple state.</summary>
    internal readonly record struct GreenHillGateRecord(
        int StartX,
        int BaseSpeed,
        bool Opened,
        int OpenFrame,
        Position16<RoomPixels> Samus,
        int SamusSubX,
        int Pose,
        Position16<RoomPixels> Grapple,
        ushort GrappleFunction);

    /// <summary>Checks two native success windows and three adjacent failures against the real room.</summary>
    internal static void VerifyGreenHillGrappleSpeedGateGlitch()
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

    /// <summary>Seeds grounded Samus at a recorded fixed-point setup and observes the actual gate PLM and actor.</summary>
    private static GreenHillGateRecord RunGreenHillGrappleSpeedGateGlitch(
        int startX,
        int baseSpeed)
    {
        ISnesAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.GreenHillZone);
        RoomPlmSlotSnapshot gate = runtime.Plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        AssertEqual(0x1be4, gate.BlockIndex, "authored Green Hill gate block");
        RoomPlmSlotSnapshot trigger = runtime.Plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGateShotBlock);
        AssertEqual((ushort)0, trigger.RoomArgument,
            "authored Green Hill trigger selects the blue-left gate table");
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        gate = runtime.Plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == PlmHeaderId.DownwardGate);
        RoomEnemyProjectileSlot gateActor = runtime.Enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.IsActive &&
            projectile.Kind == RoomEnemyProjectileKind.DownwardGateClosed);
        ushort closedGateY = gateActor.Position.Y.Whole;

        runtime.InitializeDebugGroundedSamus(
            checked((ushort)startX),
            desiredScreenY: 171,
            minimumFloorBlockY: 60);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.MovingLeftGunExtendedPose;
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        samus.EquippedItems = (SamusEquipmentFlags)((ushort)(
            SamusEquipmentFlags.GrappleBeam |
            SamusEquipmentFlags.SpeedBooster));
        samus.SelectedHudItem = HudItem.Grapple;
        samus.HorizontalSpeed.BaseSpeed = new SnesUnsignedSixteenSixteen(checked((ushort)baseSpeed), 0);
        samus.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(4, 0);
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(
            SamusSpeedBoostCounterFixtures.ActiveStageWord);
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.SpeedBooster;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);

        int openFrame = -1;
        for (int frame = 0; frame < 21; frame++)
        {
            SnesButton input = SnesButton.Left | SnesButton.A;
            if (frame >= 1)
                input |= SnesButton.X;
            runtime.StepFrame(input);
            gate = runtime.Plms.PopulationSlots.Single(slot =>
                slot.HeaderPointer == PlmHeaderId.DownwardGate);
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
            samus.Position.Whole,
            samus.Kinematics.Position.X.Fraction,
            (int)samus.Pose,
            samus.Grapple.Anchor,
            samus.Grapple.Phase == GrapplePhase.Firing ? (ushort)0xc703 : (ushort)0xc4f0);

        if (openFrame >= 0)
        {
            for (int frame = 0; frame < 8; frame++)
                runtime.StepFrame((SnesButton.Left | SnesButton.A | SnesButton.X));
            gateActor = runtime.Enemies.Projectiles.EnemyProjectiles.Single(projectile =>
                projectile.IsActive &&
                projectile.Kind is RoomEnemyProjectileKind.DownwardGateClosed or
                    RoomEnemyProjectileKind.DownwardGateMoving);
            AssertTrue(gateActor.Position.Y.Whole < closedGateY,
                "successful Grapple trigger moves the visible gate actor upward");
        }

        return actual;
    }

    /// <summary>Decodes one captured native CSV row, including hexadecimal pose and Grapple function words.</summary>
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
            new Position16<RoomPixels>(
                ushort.Parse(fields[4], CultureInfo.InvariantCulture),
                ushort.Parse(fields[6], CultureInfo.InvariantCulture)),
            int.Parse(fields[5], CultureInfo.InvariantCulture),
            int.Parse(fields[7], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            new Position16<RoomPixels>(
                ushort.Parse(fields[8], CultureInfo.InvariantCulture),
                ushort.Parse(fields[9], CultureInfo.InvariantCulture)),
            ushort.Parse(fields[10], NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }

    /// <summary>Runs the Green Hill gate comparison from the command dispatcher.</summary>
    [VerificationCommand("--green-hill-gate-glitch")]
    internal static int GreenHillGateGlitchCommand()
    {
        Suite(nameof(VerifyGreenHillGrappleSpeedGateGlitch), () => VerifyGreenHillGrappleSpeedGateGlitch());
        return 0;
    }
}
