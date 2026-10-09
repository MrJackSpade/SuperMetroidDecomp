using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>One native checkpoint row describing the input timing and expected Samus/Mochtroid states for a pipe-clip replay.</summary>
    /// <param name="Case">Label distinguishing the successful passage from the one-pixel failure case.</param>
    /// <param name="EnemyNativeIndex">Retail enemy-slot index occupied by the frozen Mochtroid used as the landing surface.</param>
    /// <param name="SamusX">Initial room-pixel X coordinate for Samus.</param>
    /// <param name="SamusY">Initial room-pixel Y coordinate for Samus.</param>
    /// <param name="EnemyX">Room-pixel X coordinate assigned to the Mochtroid.</param>
    /// <param name="EnemyY">Room-pixel Y coordinate assigned to the Mochtroid; the two cases differ by one pixel.</param>
    /// <param name="FirstDownFrame">Zero-based replay frame on which Down is pressed to enter the first crouch.</param>
    /// <param name="StandFrame">Zero-based frame on which Up is pressed and the standing transition is checked.</param>
    /// <param name="FinalJumpFrame">Zero-based frame beginning the second twelve-frame jump input.</param>
    /// <param name="LandingFrame">Frame whose native solid-enemy collision and landing coordinate are asserted.</param>
    /// <param name="LandingY">Expected Samus center Y on the recorded landing frame.</param>
    /// <param name="Frame30Pose">Expected pose at the stand-input checkpoint.</param>
    /// <param name="Frame30Radius">Expected live collision radius at the stand-input checkpoint.</param>
    /// <param name="Frame30Y">Expected Samus center Y at the stand-input checkpoint.</param>
    /// <param name="Frame31Radius">Expected Y radius on the update after the stand-input checkpoint commits.</param>
    /// <param name="Frame40Pose">Expected pose at the final-jump checkpoint.</param>
    /// <param name="MinimumY">Lowest Samus center Y observed during the 120-update replay.</param>
    /// <param name="FinalY">Expected Samus center Y after all replay updates.</param>
    /// <param name="FinalPose">Expected pose after all replay updates.</param>
    private readonly record struct MochtroidClipNativeRecord(
        string Case,
        ushort EnemyNativeIndex,
        ushort SamusX,
        ushort SamusY,
        ushort EnemyX,
        ushort EnemyY,
        int FirstDownFrame,
        int StandFrame,
        int FinalJumpFrame,
        int LandingFrame,
        ushort LandingY,
        byte Frame30Pose,
        ushort Frame30Radius,
        ushort Frame30Y,
        ushort Frame31Radius,
        byte Frame40Pose,
        ushort MinimumY,
        ushort FinalY,
        byte FinalPose);

    /// <summary>
    /// Replays the smallest retail-room sequence for the frozen-Mochtroid Botwoon pipe
    /// clip and compares it with checkpoints captured from the original CPU routines.
    /// </summary>
    private static void VerifyMochtroidBotwoonPipeClip()
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

        AssertTrue(records[0].EnemyY + 1 == records[1].EnemyY,
            "Mochtroid failure differs from success by exactly one Y pixel");
        Console.WriteLine(
            "PASS Mochtroid Botwoon pipe clip: retail room, frozen-enemy overlap, " +
            "one-frame posture radius, successful ceiling passage, and one-pixel failure match the original CPU.");
    }

    /// <summary>Replays one native checkpoint row in the retail Botwoon Hallway and checks its landing, posture, and final-state measurements.</summary>
    /// <param name="expected">Recorded input-frame markers and native values for the selected success or failure case.</param>
    private static void VerifyMochtroidClipRecord(MochtroidClipNativeRecord expected)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.BotwoonHallway);

        RoomLevelData level = runtime.LevelData ??
            throw new InvalidDataException("Botwoon Hallway level data was not loaded.");
        AssertEqual(64, level.WidthInBlocks, "Botwoon Hallway retail width");
        AssertEqual(16, level.HeightInBlocks, "Botwoon Hallway retail height");

        SamusState samus = runtime.Samus ??
            throw new InvalidDataException("Botwoon Hallway did not initialize Samus.");
        samus.XPosition = expected.SamusX;
        samus.YPosition = expected.SamusY;
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YSubposition = 0;
        samus.Kinematics.ExtraXDisplacement = 0;
        samus.Kinematics.ExtraXSubdisplacement = 0;
        samus.Kinematics.ExtraYDisplacement = 0;
        samus.Kinematics.ExtraYSubdisplacement = 0;
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.HorizontalSpeed.ExtraRunSpeed = 0;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.HorizontalSpeed.HasRunningMomentum = false;
        samus.HorizontalSpeed.AccelerationMode = 0;
        samus.Pose = SamusPoseIds.CrouchingAimUpRightPose;
        samus.EquippedItems = samus.CollectedItems = (ushort)(
            SamusEquipmentFlags.MorphBall |
            SamusEquipmentFlags.HiJumpBoots);
        samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Ice;
        samus.Health = samus.MaxHealth = 1499;
        samus.InvincibilityTimer = 120;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);

        RoomEnemySlot mochtroid = runtime.Enemies.Slots.Single(
            slot => slot.NativeIndex == expected.EnemyNativeIndex);
        AssertEqual(EnemyDefinitionPointers.Mochtroid, mochtroid.EnemyDefinitionPointer,
            $"{expected.Case} fixture selects retail Mochtroid slot");
        foreach (RoomEnemySlot slot in runtime.Enemies.Slots.Where(
                     slot => slot.NativeIndex != expected.EnemyNativeIndex))
        {
            slot.Clear();
        }
        mochtroid.XPosition = expected.EnemyX;
        mochtroid.YPosition = expected.EnemyY;
        mochtroid.XSubposition = 0;
        mochtroid.YSubposition = 0;
        mochtroid.FrozenTimer = 1000;
        mochtroid.AiHandlerBits = 4;

        ushort minimumY = samus.YPosition;
        for (int frame = 0; frame < 120; frame++)
        {
            ushort input = 0;
            if (frame < 12 ||
                frame >= expected.FinalJumpFrame && frame < expected.FinalJumpFrame + 12)
            {
                input |= (ushort)SnesButton.A;
            }
            if (frame == expected.FirstDownFrame)
                input |= (ushort)SnesButton.Down;
            if (frame == expected.StandFrame)
                input |= (ushort)SnesButton.Up;

            runtime.StepFrame(input);
            minimumY = Math.Min(minimumY, samus.YPosition);

            if (frame == expected.LandingFrame)
            {
                AssertEqual(expected.LandingY, samus.YPosition,
                    $"{expected.Case} frozen-Mochtroid landing Y");
                AssertEqual(expected.EnemyNativeIndex,
                    samus.Kinematics.SolidEnemyCollisionIndexes[(int)SamusCollisionDirection.Down],
                    $"{expected.Case} landing collision owner");
            }
            if (frame == expected.StandFrame)
            {
                AssertEqual(expected.Frame30Pose, samus.Pose,
                    $"{expected.Case} stand-input pose");
                AssertEqual(expected.Frame30Radius, samus.Kinematics.YRadius,
                    $"{expected.Case} stand commit retains native live radius");
                AssertEqual(expected.Frame30Y, samus.YPosition,
                    $"{expected.Case} stand-input center Y");
            }
            if (frame == expected.StandFrame + 1)
            {
                AssertEqual(expected.Frame31Radius, samus.Kinematics.YRadius,
                    $"{expected.Case} next alpha radius");
            }
            if (frame == expected.FinalJumpFrame)
            {
                AssertEqual(expected.Frame40Pose, samus.Pose,
                    $"{expected.Case} final-jump pose");
            }
        }

        AssertEqual(expected.MinimumY, minimumY, $"{expected.Case} minimum Y");
        AssertEqual(expected.FinalY, samus.YPosition, $"{expected.Case} final Y");
        AssertEqual(expected.FinalPose, samus.Pose, $"{expected.Case} final pose");
    }

    /// <summary>Parses one fixed-width CSV checkpoint row using invariant integer syntax.</summary>
    /// <param name="line">Comma-separated record containing the case label followed by eighteen numeric values.</param>
    /// <returns>The replay inputs and expected measurements represented by the row.</returns>
    /// <exception cref="InvalidDataException">The row does not contain exactly nineteen fields.</exception>
    /// <exception cref="FormatException">A numeric field is not an unsigned decimal integer.</exception>
    /// <exception cref="OverflowException">A numeric field does not fit its declared integer type.</exception>
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
            ParseU16(fields[1]), ParseU16(fields[2]), ParseU16(fields[3]),
            ParseU16(fields[4]), ParseU16(fields[5]),
            ParseInt(fields[6]), ParseInt(fields[7]), ParseInt(fields[8]), ParseInt(fields[9]),
            ParseU16(fields[10]), ParseByte(fields[11]), ParseU16(fields[12]), ParseU16(fields[13]),
            ParseU16(fields[14]), ParseByte(fields[15]), ParseU16(fields[16]), ParseU16(fields[17]),
            ParseByte(fields[18]));

        static ushort ParseU16(string value) =>
            ushort.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
        static byte ParseByte(string value) =>
            byte.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
        static int ParseInt(string value) =>
            int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
    }
}
