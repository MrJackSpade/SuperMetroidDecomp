using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Air spikes are sampled by <c>$94:9B60</c> after alpha's SetSamusRadius, so a pose whose
    /// radius grew in the previous beta reaches them with its new body. The 13% movie's
    /// diagonal jump ($1A to $6A, radius 12 to 19) takes air-spike damage this way.
    /// </summary>
    private static void VerifyAirSpikeAlphaRadius()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        RoomLevelData level = runtime.LevelData!;
        const int SpikeColumn = 32, SpikeRow = 29;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 64; x++)
        {
            int index = y * level.WidthInBlocks + x;
            RoomCollisionType type = y >= 33 ? RoomCollisionType.SolidBlock
                : x == SpikeColumn && y == SpikeRow ? RoomCollisionType.SpikeAir
                : RoomCollisionType.Air;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0, type).Raw);
            level.SetBehavior(index, type == RoomCollisionType.SpikeAir
                ? SamusTerrainHazardRomData.DamagingSpikeAirBehavior : (byte)0);
        }

        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = SpikeColumn * 16 + 8;
        // Radius 21 puts the top at Y 474, inside the spike row (464-479). The live radius
        // still holds the previous pose's 12, whose top (483) is in the air row below.
        samus.YPosition = 495;
        samus.InitializeAnimation(bus);
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 12;
        samus.InvincibilityTimer = 0;
        runtime.Camera!.SetPosition(400, 350);
        ushort health = samus.Health;

        runtime.StepFrame(0);

        AssertEqual((ushort)21, samus.Kinematics.YRadius, "alpha publishes the standing radius");
        AssertTrue(samus.InvincibilityTimer != 0, "the spike sample uses the radius alpha just published");
        AssertTrue(samus.KnockbackTimer != 0, "the air spike starts its knockback window");
        AssertTrue(samus.Health < health, "the air spike deals its damage");
        Console.WriteLine("  Air spike alpha radius: the refreshed standing body reaches the spike.");
    }
}
