using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class AirSpikeAlphaRadiusVerification
{
    /// <summary>
    /// Air spikes are sampled by <c>$94:9B60</c> after alpha's SetSamusRadius, so a pose whose
    /// radius grew in the previous beta reaches them with its new body. The 13% movie's
    /// diagonal jump ($1A to $6A, radius 12 to 19) takes air-spike damage this way.
    /// </summary>
    internal static void VerifyAirSpikeAlphaRadius()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        RoomLevelData level = runtime.Room.LevelData!;
        const int SpikeColumn = 32, SpikeRow = 29;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 64; x++)
        {
            int index = y * level.Size.X + x;
            RoomCollisionType type = y >= 33 ? RoomCollisionType.SolidBlock
                : x == SpikeColumn && y == SpikeRow ? RoomCollisionType.SpikeAir
                : RoomCollisionType.Air;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, type).Raw);
            level.SetBehavior(index, type == RoomCollisionType.SpikeAir
                ? SamusTerrainHazardRomData.DamagingSpikeAirBehavior : (byte)0);
        }

        SamusState samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWholeX(SpikeColumn * 16 + 8);
        // Radius 21 puts the top at Y 474, inside the spike row (464-479). The live radius
        // still holds the previous pose's 12, whose top (483) is in the air row below.
        samus.Position = samus.Position.WithWholeY(495);
        samus.Presentation.InitializeAnimation(bus);
        samus.Kinematics.Radius = new(5, 12);
        samus.InvincibilityTimer = NativeCountdown.Start(0);
        runtime.Room.Camera!.SetPosition(new Position16<RoomPixels>(400, 350));
        ushort health = samus.Health;

        runtime.StepFrame(0);

        AssertEqual((ushort)21, samus.Kinematics.Radius.Y, "alpha publishes the standing radius");
        AssertTrue(!samus.InvincibilityTimer.IsZero, "the spike sample uses the radius alpha just published");
        AssertTrue(!samus.KnockbackTimer.IsZero, "the air spike starts its knockback window");
        AssertTrue(samus.Health < health, "the air spike deals its damage");
        Console.WriteLine("  Air spike alpha radius: the refreshed standing body reaches the spike.");
    }

    [VerificationCommand("--air-spike-alpha-radius")]
    internal static int AirSpikeAlphaRadiusCommand()
    {
        Suite(nameof(VerifyAirSpikeAlphaRadius), () => VerifyAirSpikeAlphaRadius());
        return 0;
    }
}
