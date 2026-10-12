using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class ElevatubeScrollingVerification
{
    internal static void VerifyElevatubeScrolling()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool fromNorth in new[] { true, false })
        {
            var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            var door = SuperMetroid.AssetExtraction.CartridgeDoorHeaderImporter.Load(bus, fromNorth
                ? DoorPointers.MaridiaElevatubeFromNorth : DoorPointers.MaridiaElevatubeFromSouth);
            // Clear the constructor's Ceres arrival fixture before applying the real
            // incoming tube callback; no Ceres carrier belongs in this room.
            runtime.LoadCartridgeRoomForDebug(door.DestinationRoomPointer);
            runtime.LoadCartridgeRoomThroughDoorForVerification(door,
                camera: new Position16<RoomPixels>(0, fromNorth ? (ushort)0 : (ushort)2304));
            var samus = runtime.Samus!;
            var camera = runtime.Camera!;
            samus.Position = samus.Position.WithWholeX(MaridiaElevatubeRomData.SamusCenterX);
            samus.Position = samus.Position.WithWholeY(fromNorth ? MaridiaElevatubeRomData.NorthStartingPosition
                : MaridiaElevatubeRomData.SouthStartingPosition);
            uint? previousScrollY = null;
            for (int frame = 0; frame < 180; frame++)
            {
                uint beforeY = samus.Kinematics.Position.Y.RawValue;
                uint expectedY = unchecked(beforeY + (uint)(SnesSixteenSixteen.FromEightEight((short)runtime.MaridiaElevatube.Velocity)));
                AssertTrue(samus.StationaryScriptControlLocked,
                    "both tube entry callbacks install native command-zero ownership");
                runtime.StepFrame(0);
                AssertEqual(expectedY, samus.Kinematics.Position.Y.RawValue,
                    "tube displacement contains only room-main velocity, not ordinary falling");
                var checkpoint = camera.PreviousSamusPoint!.Value;
                uint savedY = checkpoint.Y.RawValue;
                AssertEqual(beforeY, savedY, "scroll checkpoint precedes elevatube room-main movement");
                AssertTrue(samus.Kinematics.Position.Y.RawValue != savedY,
                    "room-main displacement remains pending for next scrolling pass");
                if (previousScrollY is { } previous)
                {
                    uint distance = (beforeY >= previous ? beforeY - previous : previous - beforeY) + 65536;
                    AssertEqual(distance, unchecked((uint)camera.Speed.Y.RawValue),
                        "native $90:96FF counts previous room-main displacement plus one");
                }
                if (frame >= 30)
                    AssertTrue(unchecked((short)(samus.Position.Y.Whole - camera.PixelPosition.Y)) is >= 32 and < 224,
                        "tube carriage stays inside gameplay viewport in both directions");
                previousScrollY = savedY;
            }
            AssertTrue(fromNorth ? camera.PixelPosition.Y > 1600 : camera.PixelPosition.Y < 700,
                "camera traverses tube with Samus instead of remaining at entry");
            Console.WriteLine($"Elevatube {(fromNorth ? "down" : "up")}: camera={camera.PixelPosition.Y}, Samus={samus.Position.Y.Whole}; 180 exact checkpoint/distance checks passed.");
        }
    }

    [VerificationCommand("--elevatube-scrolling")]
    internal static int ElevatubeScrollingCommand()
    {
        Suite(nameof(VerifyElevatubeScrolling), () => VerifyElevatubeScrolling());
        return 0;
    }
}
