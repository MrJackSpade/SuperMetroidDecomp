using SuperMetroid.Core.Samus.Drawing;
using System.Globalization;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks native door-camera alignment steps and the complete retail transition handoff.</summary>
internal static class DoorAlignmentVerification
{
    /// <summary>Compares recorded camera steps with the alignment state and checks subpixels across the visible scroll.</summary>
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
                Name: "fixture door",
                DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
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

    /// <summary>Drives a retail door transition to verify alignment calls, input continuity, game time, and running momentum.</summary>
    private static void VerifyCompleteDoorAlignmentHandoff()
    {
        RoomHeaderDefinition sourceRoom = RoomHeaderDefinitions.MainHall;
        RoomHeaderDefinition destinationRoom = RoomHeaderDefinitions.AcidStatue;
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(sourceRoom);

        RoomLevelData level = runtime.Room.LoadedLevelData;
        int doorX = -1;
        int doorY = -1;
        int behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            CartridgeDoorHeader? candidate = level.ResolveDoorCollision(
                block.Bts.Value,
                SamusPoseId.MovingRightNormalPose,
                publishDoorSideEffects: false).Door;
            if (candidate?.DestinationRoom != destinationRoom)
                continue;
            doorX = x;
            doorY = y;
            behavior = block.Bts.Value;
            break;
        }
        AssertTrue(doorX >= 0, "door-alignment route contains its retail destination door");

        runtime.LoadCartridgeRoomForDebug(
            sourceRoom,
            camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.HorizontalSpeed.BaseSpeed = new SnesUnsignedSixteenSixteen(1, 0x8000);
        samus.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(2, 0x4000);
        samus.HorizontalSpeed.HasRunningMomentum = true;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.Room.LoadedLevelData.ResolveDoorCollision(behavior, samus.Pose, true);
        runtime.Room.LoadedCamera.SetPosition(
            new Position16<RoomPixels>(runtime.Room.LoadedCamera.PixelPosition.X, unchecked((ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 2))));
        runtime.GameTime.Load(frames: 58, seconds: 56, minutes: 34, hours: 12);

        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        const SnesButton heldLeftRight = (SnesButton.Left | SnesButton.Right);
        int alignmentCalls = 0;
        bool observedDestinationBuild = false;
        for (int frame = 0; transition.IsActive && frame < 500; frame++)
        {
            DoorTransitionPhase phaseBeforeStep = transition.Phase;
            if (transition.Phase == DoorTransitionPhase.AlignSourceCamera)
                alignmentCalls++;
            transition.Step(runtime, audio, heldLeftRight, SuperMetroid.Core.Rooms.Doors.LagFreeDoorLoaderProgress.Instance);
            if (phaseBeforeStep == DoorTransitionPhase.BuildDestinationOam &&
                transition.Phase == DoorTransitionPhase.FadeInDestinationPalette)
            {
                observedDestinationBuild = true;
                AssertEqual(heldLeftRight, runtime.Controller1.Current,
                    "destination OAM handoff retains the live Left+Right sample");
                AssertEqual(SnesButton.None, runtime.Controller1.NewlyPressed,
                    "destination OAM handoff does not synthesize a release or fresh edge");
            }
        }

        AssertTrue(!transition.IsActive, "offset retail door transition completes");
        AssertEqual(3, alignmentCalls,
            "two-pixel offset takes two movement calls and one completion call");
        AssertTrue(observedDestinationBuild,
            "door transition exercised the destination OAM handoff");
        AssertEqual(destinationRoom, runtime.Room.LoadedRoom.Definition,
            "offset retail transition reaches its destination");
        AssertEqual(58, runtime.GameTime.Frames, "door transition excludes game-time frames");
        AssertEqual(56, runtime.GameTime.Seconds, "door transition excludes game-time seconds");
        AssertEqual(34, runtime.GameTime.Minutes, "door transition excludes game-time minutes");
        AssertEqual(12, runtime.GameTime.Hours, "door transition excludes game-time hours");
        AssertEqual(1, samus.HorizontalSpeed.BaseSpeed.Whole,
            "door transition retains base running speed");
        AssertEqual(0x8000, samus.HorizontalSpeed.BaseSpeed.Fraction,
            "door transition retains base running subspeed");
        AssertEqual(2, samus.HorizontalSpeed.ExtraRunSpeed.Whole,
            "door transition retains extra running speed");
        AssertEqual(0x4000, samus.HorizontalSpeed.ExtraRunSpeed.Fraction,
            "door transition retains extra running subspeed");
        AssertTrue(samus.HorizontalSpeed.HasRunningMomentum,
            "door transition retains the running-momentum flag");
    }

    /// <summary>Parses a decimal field from the culture-independent native alignment fixture.</summary>
    private static int ParseDecimal(string value) =>
        int.Parse(value, CultureInfo.InvariantCulture);

    /// <summary>Parses a hexadecimal word from the native alignment fixture.</summary>
    private static ushort ParseHex(string value) =>
        ushort.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
}
