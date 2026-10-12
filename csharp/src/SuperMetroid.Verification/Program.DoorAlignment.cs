using System.Globalization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;

internal static class DoorAlignmentVerification
{
    internal static void VerifyDoorAlignmentParity(
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
                (DoorDirection)direction,
                new Position16<RoomPixels>(beforeX, beforeY));
            string context =
                $"door alignment direction {direction}, start ${startX:X4}/${startY:X4}, call {call}";
            AssertEqual(new Position16<RoomPixels>(expectedX, expectedY), actual.CameraPosition, $"{context} camera");
            AssertEqual(nextFunction != 0xe310, actual.Completed, $"{context} completion seam");
            if (actual.Completed)
                completedCases++;
        }

        AssertEqual(28, completedCases, "all native door-alignment cases complete");

        Suite(nameof(VerifyCompleteDoorAlignmentHandoff), () => VerifyCompleteDoorAlignmentHandoff());

        // The scrolling owner replaces only whole position words. Fractional movement
        // survives the complete transition and is therefore available to the first frame
        // in the destination room, which is the basis of alignment-sensitive strategies.
        var source = FixedPosition<RoomPixels>.FromWords(0x04d2, 0xabcd, 0x0187, 0x2468);
        var final = FixedPosition<RoomPixels>.FromWords(0x0318, 0xabcd, 0x0218, 0x2468);
        for (int direction = 0; direction < 4; direction++)
        {
            var door = new CartridgeDoorHeader(
                Pointer: 0x8000,
                DestinationRoomPointer: 0x9000,
                Orientation: new((DoorDirection)direction, DoorClosingBehavior.None),
                PlmBlock: new(0, 0),
                DestinationScreen: new(3, 2),
                SamusDistance: 0x0100,
                SetupCodePointer: DoorSetupCode.None);
            DoorOpeningScrollState transition = DoorOpeningScrollState.Create(
                door,
                source,
                finalCamera: new Position16<RoomPixels>(0x0300, direction == 3 ? (ushort)0x0220 : (ushort)0x0200),
                finalLayer2: new Position16<Bg2Pixels>(0x0180, 0x0140),
                finalSamus: final);
            while (!transition.Advance())
            {
            }

            AssertEqual(source.X.Fraction, transition.SamusPosition.X.Fraction,
                $"door direction {direction} retains X subposition during visible scroll");
            AssertEqual(source.Y.Fraction, transition.SamusPosition.Y.Fraction,
                $"door direction {direction} retains Y subposition during visible scroll");
            AssertEqual(source.X.Fraction, transition.FinalSamusPosition.X.Fraction,
                $"door direction {direction} retains X subposition at handoff");
            AssertEqual(source.Y.Fraction, transition.FinalSamusPosition.Y.Fraction,
                $"door direction {direction} retains Y subposition at handoff");
        }

        Console.WriteLine(
            "  Door alignment: 1,072 native camera steps, completion calls, and retained subpixels agree.");
    }

    private static void VerifyCompleteDoorAlignmentHandoff()
    {
        const ushort sourceRoom = 0xb236;
        const ushort destinationRoom = 0xb1e5;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(sourceRoom);

        RoomLevelData level = runtime.Room.LevelData!;
        int doorX = -1;
        int doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            CartridgeDoorHeader? candidate = level.ResolveDoorCollision(
                bus,
                block.Behavior,
                SamusPoseId.MovingRightNormalPose,
                publishDoorSideEffects: false).Door;
            if (candidate?.DestinationRoomPointer != destinationRoom)
                continue;
            doorX = x;
            doorY = y;
            behavior = block.Behavior;
            break;
        }
        AssertTrue(doorX >= 0, "door-alignment route contains its retail destination door");

        runtime.LoadCartridgeRoomForDebug(
            sourceRoom,
            camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        SamusState samus = runtime.Player.Samus!;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x8000;
        samus.HorizontalSpeed.ExtraRunSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        runtime.Room.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);
        runtime.Room.Camera!.SetPosition(
            new Position16<RoomPixels>(runtime.Room.Camera.PixelPosition.X, unchecked((ushort)(runtime.Room.Camera.PixelPosition.Y + 2))));
        runtime.GameTime.Load(frames: 58, seconds: 56, minutes: 34, hours: 12);

        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        const ushort heldLeftRight = (ushort)(SnesButton.Left | SnesButton.Right);
        int alignmentCalls = 0;
        bool observedDestinationBuild = false;
        for (int frame = 0; transition.IsActive && frame < 500; frame++)
        {
            DoorTransitionPhase phaseBeforeStep = transition.Phase;
            if (transition.Phase == DoorTransitionPhase.AlignSourceCamera)
                alignmentCalls++;
            transition.Step(runtime, audio, heldLeftRight, SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance);
            if (phaseBeforeStep == DoorTransitionPhase.BuildDestinationOam &&
                transition.Phase == DoorTransitionPhase.FadeInDestinationPalette)
            {
                observedDestinationBuild = true;
                AssertEqual(heldLeftRight, runtime.Controller1.Current,
                    "destination OAM handoff retains the live Left+Right sample");
                AssertEqual(0, runtime.Controller1.NewlyPressed,
                    "destination OAM handoff does not synthesize a release or fresh edge");
            }
        }

        AssertTrue(!transition.IsActive, "offset retail door transition completes");
        AssertEqual(3, alignmentCalls,
            "two-pixel offset takes two movement calls and one completion call");
        AssertTrue(observedDestinationBuild,
            "door transition exercised the destination OAM handoff");
        AssertEqual(destinationRoom, runtime.Room.ActiveRoom!.Pointer,
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
