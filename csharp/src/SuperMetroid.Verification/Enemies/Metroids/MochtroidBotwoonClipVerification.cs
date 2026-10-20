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
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Enemies.Metroids;

using SuperMetroid.Verification.Harness;

/// <summary>Checks the frozen Mochtroid pipe clip and its one-pixel failure case against native Botwoon Hallway checkpoints.</summary>
internal static class MochtroidBotwoonClipVerification
{
    /// <summary>Captured native setup, input-frame milestones, collision radius, and final outcome for one clip trajectory.</summary>
    internal readonly record struct MochtroidClipNativeRecord(
        string Case,
        ushort EnemyNativeIndex,
        Position16<RoomPixels> Samus,
        Position16<RoomPixels> Enemy,
        int FirstDownFrame,
        int StandFrame,
        int FinalJumpFrame,
        int LandingFrame,
        ushort LandingY,
        SamusPoseId Frame30Pose,
        ushort Frame30Radius,
        ushort Frame30Y,
        ushort Frame31Radius,
        SamusPoseId Frame40Pose,
        ushort MinimumY,
        ushort FinalY,
        SamusPoseId FinalPose);

    /// <summary>
    /// Replays the smallest retail-room sequence for the frozen-Mochtroid Botwoon pipe
    /// clip and compares it with checkpoints captured from the original CPU routines.
    /// </summary>
    internal static void VerifyMochtroidBotwoonPipeClip()
    {
        MochtroidClipNativeRecord[] records = File.ReadLines(
                "csharp/test-fixtures/movement-release/mochtroid-botwoon-440-native.csv")
            .Skip(1)
            .Select(ParseMochtroidClipRecord)
            .ToArray();
        AssertEqual(2, records.Length,
            "Mochtroid clip fixture contains success and one-pixel failure");

        foreach (MochtroidClipNativeRecord expected in records)
            VerifyMochtroidClipRecord(expected);

        AssertTrue(records[0].Enemy.Y + 1 == records[1].Enemy.Y,
            "Mochtroid failure differs from success by exactly one Y pixel");
        Console.WriteLine(
            "PASS Mochtroid Botwoon pipe clip: retail room, frozen-enemy overlap, " +
            "one-frame posture radius, successful ceiling passage, and one-pixel failure match the original CPU.");
    }

    /// <summary>Replays one 120-frame retail hallway sequence with a frozen Mochtroid and compares pose, collision, and height checkpoints.</summary>
    private static void VerifyMochtroidClipRecord(MochtroidClipNativeRecord expected)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.BotwoonHallway);

        RoomLevelData level = runtime.Room.LevelData ??
            throw new InvalidDataException("Botwoon Hallway level data was not loaded.");
        AssertEqual(64, level.Size.X, "Botwoon Hallway retail width");
        AssertEqual(16, level.Size.Y, "Botwoon Hallway retail height");

        SamusState samus = runtime.Player.Samus ??
            throw new InvalidDataException("Botwoon Hallway did not initialize Samus.");
        samus.Position = samus.Position.WithWhole(expected.Samus);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWords(0, 0, 0, 0);
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.HasRunningMomentum = false;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.Pose = SamusPoseId.CrouchingAimUpRightPose;
        samus.EquippedItems = samus.CollectedItems = (SamusEquipmentFlags)((ushort)(
            SamusEquipmentFlags.MorphBall |
            SamusEquipmentFlags.HiJumpBoots));
        samus.EquippedBeams = samus.CollectedBeams = SamusBeamFlags.Ice;
        samus.Health = samus.MaxHealth = 1499;
        samus.InvincibilityTimer = NativeCountdown.Start(120);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.CommitPoseHistory();

        RoomEnemySlot mochtroid = runtime.Enemies.SlotTable.Slots.Single(
            slot => slot.NativeIndex == expected.EnemyNativeIndex);
        AssertEqual(EnemyDefinitionId.Mochtroid, mochtroid.EnemyDefinitionPointer,
            $"{expected.Case} fixture selects retail Mochtroid slot");
        foreach (RoomEnemySlot slot in runtime.Enemies.SlotTable.Slots.Where(
                     slot => slot.NativeIndex != expected.EnemyNativeIndex))
        {
            slot.Clear();
        }
        mochtroid.Position = FixedPosition<RoomPixels>.FromWhole(expected.Enemy);
        mochtroid.FrozenTimer = NativeCountdown.Start(1000);
        mochtroid.AiStates = EnemyAiStates.Frozen;

        ushort minimumY = samus.Position.Y.Whole;
        for (int frame = 0; frame < 120; frame++)
        {
            SnesButton input = 0;
            if (frame < 12 ||
                frame >= expected.FinalJumpFrame && frame < expected.FinalJumpFrame + 12)
            {
                input |= SnesButton.A;
            }
            if (frame == expected.FirstDownFrame)
                input |= SnesButton.Down;
            if (frame == expected.StandFrame)
                input |= SnesButton.Up;

            runtime.StepFrame(input);
            minimumY = Math.Min(minimumY, samus.Position.Y.Whole);

            if (frame == expected.LandingFrame)
            {
                AssertEqual(expected.LandingY, samus.Position.Y.Whole,
                    $"{expected.Case} frozen-Mochtroid landing Y");
                AssertEqual(expected.EnemyNativeIndex,
                    samus.Kinematics.SolidEnemyCollisionIndexes[(int)SamusCollisionDirection.Down],
                    $"{expected.Case} landing collision owner");
            }
            if (frame == expected.StandFrame)
            {
                AssertEqual(expected.Frame30Pose, samus.Pose,
                    $"{expected.Case} stand-input pose");
                AssertEqual(expected.Frame30Radius, samus.Kinematics.Radius.Y,
                    $"{expected.Case} stand commit retains native live radius");
                AssertEqual(expected.Frame30Y, samus.Position.Y.Whole,
                    $"{expected.Case} stand-input center Y");
            }
            if (frame == expected.StandFrame + 1)
            {
                AssertEqual(expected.Frame31Radius, samus.Kinematics.Radius.Y,
                    $"{expected.Case} next alpha radius");
            }
            if (frame == expected.FinalJumpFrame)
            {
                AssertEqual(expected.Frame40Pose, samus.Pose,
                    $"{expected.Case} final-jump pose");
            }
        }

        AssertEqual(expected.MinimumY, minimumY, $"{expected.Case} minimum Y");
        AssertEqual(expected.FinalY, samus.Position.Y.Whole, $"{expected.Case} final Y");
        AssertEqual(expected.FinalPose, samus.Pose, $"{expected.Case} final pose");
    }

    /// <summary>Parses one invariant-culture CSV row of captured native positions, input timing, and pose milestones.</summary>
    private static MochtroidClipNativeRecord ParseMochtroidClipRecord(string line)
    {
        string[] fields = line.Split(',');
        if (fields.Length != 19)
        {
            throw new InvalidDataException(
                $"Mochtroid clip fixture row has {fields.Length} fields instead of 19: {line}");
        }

        return new MochtroidClipNativeRecord(
            fields[0],
            ParseU16(fields[1]), new Position16<RoomPixels>(ParseU16(fields[2]), ParseU16(fields[3])),
            new Position16<RoomPixels>(ParseU16(fields[4]), ParseU16(fields[5])),
            ParseInt(fields[6]), ParseInt(fields[7]), ParseInt(fields[8]), ParseInt(fields[9]),
            ParseU16(fields[10]), (SamusPoseId)ParseByte(fields[11]), ParseU16(fields[12]), ParseU16(fields[13]),
            ParseU16(fields[14]), (SamusPoseId)ParseByte(fields[15]), ParseU16(fields[16]), ParseU16(fields[17]),
            (SamusPoseId)ParseByte(fields[18]));

        static ushort ParseU16(string value) =>
            ushort.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
        static byte ParseByte(string value) =>
            byte.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
        static int ParseInt(string value) =>
            int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    /// <summary>Runs the Botwoon Hallway Mochtroid clip verification command.</summary>
    [VerificationCommand("--mochtroid-botwoon-clip")]
    internal static int MochtroidBotwoonClipCommand()
    {
        Suite(nameof(VerifyMochtroidBotwoonPipeClip), () => VerifyMochtroidBotwoonPipeClip());
        return 0;
    }
}
