using System.Globalization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyDoorAlignmentParity(
        string fixture = "csharp/test-fixtures/movement-release/door-alignment-448-v1.csv")
    {
        string[] rows = File.ReadAllLines(Path.GetFullPath(fixture));
        AssertEqual(1_073, rows.Length, "native door-alignment matrix dimensions");

        int completedCases = 0;
        foreach (string row in rows.Skip(1))
        {
            string[] fields = row.Split(',');
            int direction = ParseDecimal(fields[0]);
            ushort startX = ParseHex(fields[1]);
            ushort startY = ParseHex(fields[2]);
            int call = ParseDecimal(fields[3]);
            ushort beforeX = ParseHex(fields[4]);
            ushort beforeY = ParseHex(fields[5]);
            ushort expectedX = ParseHex(fields[6]);
            ushort expectedY = ParseHex(fields[7]);
            ushort nextFunction = ParseHex(fields[8]);

            DoorCameraAlignmentState actual = DoorCameraAlignmentState.Step(
                (byte)direction,
                beforeX,
                beforeY);
            string context =
                $"door alignment direction {direction}, start ${startX:X4}/${startY:X4}, call {call}";
            AssertEqual(expectedX, actual.CameraX, $"{context} camera X");
            AssertEqual(expectedY, actual.CameraY, $"{context} camera Y");
            AssertEqual(nextFunction != 0xe310, actual.Completed, $"{context} completion seam");
            if (actual.Completed)
                completedCases++;
        }

        AssertEqual(28, completedCases, "all native door-alignment cases complete");

        VerifyCompleteDoorAlignmentHandoff();

        // The scrolling owner replaces only whole position words. Fractional movement
        // survives the complete transition and is therefore available to the first frame
        // in the destination room, which is the basis of alignment-sensitive strategies.
        const uint sourceX = 0x04d2_abcd;
        const uint sourceY = 0x0187_2468;
        const uint finalX = 0x0318_abcd;
        const uint finalY = 0x0218_2468;
        for (int direction = 0; direction < 4; direction++)
        {
            var door = new CartridgeDoorHeader(
                Pointer: 0x8000,
                DestinationRoomPointer: 0x9000,
                BitFlags: 0,
                Orientation: (byte)direction,
                PlmX: 0,
                PlmY: 0,
                DestinationScreenX: 3,
                DestinationScreenY: 2,
                SamusDistance: 0x0100,
                SetupCodePointer: 0);
            DoorOpeningScrollState transition = DoorOpeningScrollState.Create(
                door,
                sourceX,
                sourceY,
                finalCameraX: 0x0300,
                finalCameraY: direction == 3 ? (ushort)0x0220 : (ushort)0x0200,
                finalLayer2X: 0x0180,
                finalLayer2Y: 0x0140,
                finalSamusXFixed: finalX,
                finalSamusYFixed: finalY);
            while (!transition.Advance())
            {
            }

            AssertEqual(unchecked((ushort)sourceX), unchecked((ushort)transition.SamusXFixed),
                $"door direction {direction} retains X subposition during visible scroll");
            AssertEqual(unchecked((ushort)sourceY), unchecked((ushort)transition.SamusYFixed),
                $"door direction {direction} retains Y subposition during visible scroll");
            AssertEqual(unchecked((ushort)sourceX), unchecked((ushort)transition.FinalSamusXFixed),
                $"door direction {direction} retains X subposition at handoff");
            AssertEqual(unchecked((ushort)sourceY), unchecked((ushort)transition.FinalSamusYFixed),
                $"door direction {direction} retains Y subposition at handoff");
        }

        Console.WriteLine(
            "  Door alignment: 1,072 native camera steps, completion calls, and retained subpixels agree.");
    }

    private static void VerifyCompleteDoorAlignmentHandoff()
    {
        const ushort sourceRoom = 0xb236;
        const ushort destinationRoom = 0xb1e5;
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(sourceRoom);

        RoomLevelData level = runtime.LevelData!;
        int doorX = -1;
        int doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.HeightInBlocks && doorX < 0; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            CartridgeDoorHeader candidate = level.ResolveDoorCollision(
                bus,
                block.Behavior,
                SamusPoseIds.MovingRightNormalPose,
                publishDoorSideEffects: false);
            if (candidate.DestinationRoomPointer != destinationRoom)
                continue;
            doorX = x;
            doorY = y;
            behavior = block.Behavior;
            break;
        }
        AssertTrue(doorX >= 0, "door-alignment route contains its retail destination door");

        runtime.LoadCartridgeRoomForDebug(
            sourceRoom,
            cameraX: (ushort)(doorX / 16 * 256),
            cameraY: (ushort)(doorY / 16 * 256));
        SamusState samus = runtime.Samus!;
        samus.XPosition = (ushort)(doorX * 16 + 8);
        samus.YPosition = (ushort)(doorY * 16 + 8);
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x8000;
        samus.HorizontalSpeed.ExtraRunSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);
        runtime.Camera!.SetPosition(
            runtime.Camera.XPosition,
            unchecked((ushort)(runtime.Camera.YPosition + 2)));
        runtime.GameTime.Load(frames: 58, seconds: 56, minutes: 34, hours: 12);

        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        int alignmentCalls = 0;
        for (int frame = 0; transition.IsActive && frame < 500; frame++)
        {
            if (transition.Phase == DoorTransitionPhase.AlignSourceCamera)
                alignmentCalls++;
            transition.Step(runtime, audio, 0);
        }

        AssertTrue(!transition.IsActive, "offset retail door transition completes");
        AssertEqual(3, alignmentCalls,
            "two-pixel offset takes two movement calls and one completion call");
        AssertEqual(destinationRoom, runtime.ActiveRoom!.Pointer,
            "offset retail transition reaches its destination");
        AssertEqual((ushort)58, runtime.GameTime.Frames, "door transition excludes game-time frames");
        AssertEqual((ushort)56, runtime.GameTime.Seconds, "door transition excludes game-time seconds");
        AssertEqual((ushort)34, runtime.GameTime.Minutes, "door transition excludes game-time minutes");
        AssertEqual((ushort)12, runtime.GameTime.Hours, "door transition excludes game-time hours");
        AssertEqual((ushort)1, samus.HorizontalSpeed.BaseSpeed,
            "door transition retains base running speed");
        AssertEqual((ushort)0x8000, samus.HorizontalSpeed.BaseSubspeed,
            "door transition retains base running subspeed");
        AssertEqual((ushort)2, samus.HorizontalSpeed.ExtraRunSpeed,
            "door transition retains extra running speed");
        AssertEqual((ushort)0x4000, samus.HorizontalSpeed.ExtraRunSubspeed,
            "door transition retains extra running subspeed");
        AssertTrue(samus.HorizontalSpeed.HasRunningMomentum,
            "door transition retains the running-momentum flag");
    }

    private static int ParseDecimal(string value) =>
        int.Parse(value, CultureInfo.InvariantCulture);

    private static ushort ParseHex(string value) =>
        ushort.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
}
