using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyElevatubeScrolling()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool fromNorth in new[] { true, false })
        {
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            var door = CartridgeDoorHeader.Load(bus, fromNorth
                ? DoorPointers.MaridiaElevatubeFromNorth : DoorPointers.MaridiaElevatubeFromSouth);
            // Clear the constructor's Ceres arrival fixture before applying the real
            // incoming tube callback; no Ceres carrier belongs in this room.
            runtime.LoadCartridgeRoomForDebug(door.DestinationRoomPointer);
            runtime.LoadCartridgeRoomThroughDoorForVerification(door,
                cameraY: fromNorth ? (ushort)0 : (ushort)2304);
            var samus = runtime.Samus!;
            var camera = runtime.Camera!;
            samus.XPosition = MaridiaElevatubeRomData.SamusCenterX;
            samus.YPosition = fromNorth ? MaridiaElevatubeRomData.NorthStartingPosition
                : MaridiaElevatubeRomData.SouthStartingPosition;
            uint? previousScrollY = null;
            for (int frame = 0; frame < 180; frame++)
            {
                uint beforeY = samus.Kinematics.YFixed;
                uint expectedY = unchecked(beforeY + (uint)((short)runtime.MaridiaElevatube.Velocity << 8));
                AssertTrue(samus.StationaryScriptControlLocked,
                    "both tube entry callbacks install native command-zero ownership");
                runtime.StepFrame(0);
                AssertEqual(expectedY, samus.Kinematics.YFixed,
                    "tube displacement contains only room-main velocity, not ordinary falling");
                var checkpoint = camera.PreviousSamusPoint!.Value;
                uint savedY = ((uint)checkpoint.YPosition << 16) | checkpoint.YSubposition;
                AssertEqual(beforeY, savedY, "scroll checkpoint precedes elevatube room-main movement");
                AssertTrue(samus.Kinematics.YFixed != savedY,
                    "room-main displacement remains pending for next scrolling pass");
                if (previousScrollY is { } previous)
                {
                    uint distance = (beforeY >= previous ? beforeY - previous : previous - beforeY) + 65536;
                    AssertEqual(distance, ((uint)camera.CameraYSpeed << 16) | camera.CameraYSubspeed,
                        "native $90:96FF counts previous room-main displacement plus one");
                }
                if (frame >= 30)
                    AssertTrue(unchecked((short)(samus.YPosition - camera.YPosition)) is >= 32 and < 224,
                        "tube carriage stays inside gameplay viewport in both directions");
                previousScrollY = savedY;
            }
            AssertTrue(fromNorth ? camera.YPosition > 1600 : camera.YPosition < 700,
                "camera traverses tube with Samus instead of remaining at entry");
            Console.WriteLine($"Elevatube {(fromNorth ? "down" : "up")}: camera={camera.YPosition}, Samus={samus.YPosition}; 180 exact checkpoint/distance checks passed.");
        }
    }
}
