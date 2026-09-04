using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Retail room identification and collision reproduction for issue #259.</summary>
internal static class NorfairBarrierCollisionAudit
{
    private static readonly RoomIdentity TargetRoom = new(AreaId.Norfair, 0x0b);

    public static int Run(string romPath, string? recordingPath = null)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = FindRoom(bus, TargetRoom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room.Pointer);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        RoomLevelData level = runtime.LevelData ??
            throw new InvalidDataException($"Room {TargetRoom} loaded no level data.");

        VerifyCapturedCrampedLanding(bus, level);

        Console.WriteLine(
            $"Room {TargetRoom}: header=$8F:{room.Pointer:X4}, state=$8F:{room.State.Pointer:X4}, " +
            $"screens={room.WidthInScreens}x{room.HeightInScreens}, " +
            $"blocks={level.WidthInBlocks}x{level.HeightInBlocks}, " +
            $"map=({room.MapX},{room.MapY}).");
        if (recordingPath is not null)
            TraceRecordedRoom(bus, romPath, recordingPath);
        return 0;
    }

    /// <summary>
    /// Replays the exact pre-landing state captured at recording frame 39,440. The two
    /// shootable blocks at column eight are the ceiling and floor that constrain Samus;
    /// her five-pixel horizontal radius overlaps that column even though her center is in
    /// column nine. A full spin-landing body cannot fit in the resulting 32-pixel opening.
    /// </summary>
    private static void VerifyCapturedCrampedLanding(
        ISnesAddressSpace bus,
        RoomLevelData level)
    {
        RoomCollisionBlock ceiling = level.GetCollisionBlock(8, 2);
        RoomCollisionBlock floor = level.GetCollisionBlock(8, 4);
        Require(
            ceiling.CollisionType == RoomCollisionType.ShootableBlock,
            "Captured barrier ceiling is no longer the retail shootable block at (8,2).");
        Require(
            floor.CollisionType == RoomCollisionType.ShootableBlock,
            "Captured barrier floor is no longer the retail shootable block at (8,4).");

        var samus = new SamusState
        {
            Pose = SamusPoseIds.SpinJumpRightPose,
            XPosition = 0x0094,
            YPosition = 0x0034,
        };
        samus.RefreshCollisionRadii(bus);
        Require(samus.Kinematics.YRadius == 12, "Captured spin pose must have radius 12.");

        bool installedLanding = samus.TryApplyAerialLanding(
            bus,
            level,
            wasSpinning: true,
            controllerInput: (ushort)(SnesButton.Right | SnesButton.A),
            nmiFrameCounter: 39440);

        Require(!installedLanding, "Full-height spin landing unexpectedly fit below the barrier.");
        Require(
            samus.Pose == SamusPoseIds.CrouchingRightPose,
            $"Native two-sided collision must choose crouch $27, got ${samus.Pose:X2}.");
        Require(
            samus.YPosition == 0x0030,
            $"Crouch fallback must preserve the floor boundary at Y=$0040, got center ${samus.YPosition:X4}.");
        Require(
            samus.Kinematics.YRadius == 16,
            $"Crouch fallback must use radius 16, got {samus.Kinematics.YRadius}.");
        Require(
            samus.YPosition - samus.Kinematics.YRadius == 0x0020 &&
            samus.YPosition + samus.Kinematics.YRadius == 0x0040,
            "Crouched collision body must fit exactly between the retail ceiling and floor.");
        Console.WriteLine(
            "Exact frame-39440 landing reproduction passed: native pose collision selected " +
            "crouch $27 at Y=$0030 instead of embedding landing $A6 in the ceiling.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException(message);
    }

    private static void TraceRecordedRoom(
        SuperMetroidAddressSpace bus,
        string romPath,
        string recordingPath)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        using FileStream rom = File.OpenRead(romPath);
        byte[] digest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(digest, recording.RomSha256))
            throw new InvalidDataException("Barrier replay ROM SHA-256 does not match.");

        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions);
        var apuPorts = new byte[4];
        bool wasInTarget = false;
        for (int frame = 0; frame < recording.ControllerInputs.Length; frame++)
        {
            ushort input = recording.ControllerInputs[frame];
            SuperMetroidRuntime? before = game.RuntimeForVerification;
            SamusState? samusBefore = before?.Samus;
            bool inTargetBefore = before?.ActiveRoom?.Identity == TargetRoom;
            ushort yDirectionBefore = samusBefore?.Kinematics.YDirection ?? 0;
            ushort ySpeedBefore = samusBefore?.Kinematics.YSpeed ?? 0;
            byte poseBefore = samusBefore?.Pose ?? 0;
            ushort xBefore = samusBefore?.XPosition ?? 0;
            ushort yBefore = samusBefore?.YPosition ?? 0;
            ushort xRadiusBefore = samusBefore?.Kinematics.XRadius ?? 0;
            ushort yRadiusBefore = samusBefore?.Kinematics.YRadius ?? 0;

            FrontendFrame result = game.Step(input);
            foreach (SuperMetroid.Core.Audio.CartridgeAudioCommand command in result.AudioCommands)
            {
                if (command.Kind == SuperMetroid.Core.Audio.CartridgeAudioCommandKind.WritePort)
                    apuPorts[command.Port] = command.Value;
            }
            game.SetAudioAcknowledgements(new SuperMetroid.Core.Audio.CartridgeAudioAcknowledgements(
                apuPorts[0], apuPorts[1], apuPorts[2], apuPorts[3]));

            SuperMetroidRuntime? after = game.RuntimeForVerification;
            SamusState? samusAfter = after?.Samus;
            bool inTargetAfter = after?.ActiveRoom?.Identity == TargetRoom;
            if (inTargetAfter && !wasInTarget)
                Console.WriteLine($"Entered {TargetRoom} on recording frame {frame}.");
            if (inTargetBefore && samusBefore is not null && samusAfter is not null)
            {
                bool jumpEdge = (after!.Controller1.NewlyPressed & (ushort)SnesButton.A) != 0;
                bool hitCeiling = yDirectionBefore == 1 &&
                    samusAfter.Kinematics.YDirection == 2 && ySpeedBefore != 0;
                bool poseChanged = poseBefore != samusAfter.Pose;
                if (jumpEdge || hitCeiling || poseChanged)
                {
                    Console.WriteLine(
                        $"rec={frame} input=${input:X4}/${after.Controller1.NewlyPressed:X4} " +
                        $"pose=${poseBefore:X2}->${samusAfter.Pose:X2} " +
                        $"xy=(${xBefore:X4},${yBefore:X4})->" +
                        $"(${samusAfter.XPosition:X4},${samusAfter.YPosition:X4}) " +
                        $"radii=({xRadiusBefore},{yRadiusBefore})->" +
                        $"({samusAfter.Kinematics.XRadius},{samusAfter.Kinematics.YRadius}) " +
                        $"ydir/speed={yDirectionBefore}/${ySpeedBefore:X4}->" +
                        $"{samusAfter.Kinematics.YDirection}/{samusAfter.Kinematics.YSpeed:X4}" +
                        (hitCeiling ? " CEILING" : string.Empty));
                }
            }
            if (wasInTarget && !inTargetAfter)
            {
                Console.WriteLine($"Left {TargetRoom} on recording frame {frame}.");
                break;
            }
            wasInTarget = inTargetAfter;
        }
    }

    private static CartridgeRoomHeader FindRoom(
        SuperMetroidAddressSpace bus,
        RoomIdentity identity)
    {
        string namesPath = Path.GetFullPath(Path.Combine("upstream-sm", "assets", "names.txt"));
        foreach (string line in File.ReadLines(namesPath))
        {
            string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 2 ||
                !fields[0].StartsWith("0x8f", StringComparison.Ordinal) ||
                !fields[1].StartsWith("kRoom_", StringComparison.Ordinal) ||
                fields[1].Contains("_DoorOuts", StringComparison.Ordinal) ||
                !ushort.TryParse(
                    fields[1]["kRoom_".Length..],
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out ushort pointer) ||
                pointer == 0xe82c)
            {
                continue;
            }

            CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, pointer);
            if (room.Identity == identity)
                return room;
        }

        throw new InvalidDataException($"Could not find cartridge room {identity}.");
    }
}
