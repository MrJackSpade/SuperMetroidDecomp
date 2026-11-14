using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks the pickup grant and delayed visual update around the synchronous item message.</summary>
internal static class CollectibleMessageTimingVerification
{
    /// <summary>Confirms $84:8968 grants energy before the bank-$85 message, while empty PLM draw and HUD update wait for return (#1255).</summary>
    internal static void VerifyCollectibleMessageTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.GauntletETank, camera: new Position16<RoomPixels>(1280, 0));
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(1296, 110));
        samus.Health = 49;
        samus.MaxHealth = 99;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        var item = runtime.RoomObjects.CollectibleObjects.Collectibles.Single(i => i.Kind == InWorldCollectibleKind.EnergyTank);
        int itemIndex = runtime.Room.LoadedLevelData.GetBlockIndex(item.Block);
        RoomTerrainBlock visible = runtime.Room.LoadedLevelData.GetCollisionBlockByIndex(itemIndex).Terrain;
        // The block the displayed frame shows for the pickup, with its mirroring, and the picture it draws: its latched room
        // BG1 (#1346, #1431).
        (RoomBlockCell Cell, LayeredPicture? Picture) ShownBlock()
        {
            RoomBackgroundLayer bg1 = (runtime.Display.Latched.Rooms ?? throw new InvalidOperationException(
                "The displayed frame has no room backgrounds.")).Foreground;
            RoomBlockCell cell = bg1.Blocks.Cells[itemIndex];
            return (cell, bg1.Pictures?.Picture(cell.Picture));
        }
        ushort[] HudWords() => runtime.Hud.TileWords(runtime.InstalledHudArtwork).WhereToArray((_, index) => index % 32 < 26);
        // The displayed HUD is the one the last accepted NMI latched, laid out as tilemap words below its static top row;
        // the HUD's images are latched rather than written into VRAM (#1466).
        ushort[] VisibleHudWords()
        {
            ushort[] shown = runtime.AcceptedHudWords;
            return Enumerable.Range(0, runtime.Hud.TileWords(runtime.InstalledHudArtwork).Length)
                .Where(index => index % 32 < 26)
                .Select(index => shown[FxOverlayPlaneConstants.Columns + index]).ToArray();
        }
        ushort[] beforeVisibleHud = VisibleHudWords();
        (RoomBlockCell Cell, LayeredPicture? Picture) beforeTile = ShownBlock();
        ushort[] beforeHud = HudWords();
        AssertTrue(runtime.RoomObjects.CollectibleObjects.TryNotifyCollectibleTouch(item.Block), "reported pickup accepts contact");
        runtime.StepFrame(0);
        AssertTrue(runtime.MessageBox.IsActive, "pickup begins synchronous message");
        AssertEqual(199, samus.MaxHealth, "native grant precedes message return");
        AssertEqual(199, samus.Health, "native energy refill precedes message return");
        Console.WriteLine($"Pickup frame: tile retained={beforeTile == ShownBlock()}, HUD retained={beforeHud.SequenceEqual(HudWords())}");
        void AssertHeld()
        {
            AssertEqual(visible, runtime.Room.LoadedLevelData.GetCollisionBlockByIndex(itemIndex).Terrain, "pickup level tile remains during message");
            AssertTrue(beforeTile == ShownBlock(), "displayed pickup block and picture remain during message");
            AssertTrue(beforeHud.SequenceEqual(HudWords()), "HUD counters remain during message");
            AssertTrue(beforeVisibleHud.SequenceEqual(VisibleHudWords()), "displayed HUD VRAM remains during message");
            AssertEqual(199, samus.MaxHealth, "message cannot repeat acquisition");
        }
        AssertHeld();
        int frames = 0;
        while (runtime.MessageBox.Phase != GameplayMessageBoxPhase.AwaitingInput && frames++ < 400)
        {
            runtime.StepFrame(0);
            AssertHeld();
        }
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, runtime.MessageBox.Phase, "fanfare reaches acknowledgement");
        runtime.StepFrame(ControllerButton.A);
        while (runtime.MessageBox.IsActive && frames++ < 450)
        {
            AssertHeld();
            runtime.StepFrame(0);
        }
        AssertTrue(!runtime.MessageBox.IsActive, "message returns");
        AssertTrue(runtime.Room.LoadedLevelData.GetCollisionBlockByIndex(itemIndex).Terrain != visible, "return executes pickup empty draw");
        AssertTrue(!beforeHud.SequenceEqual(HudWords()), "return rebuilds HUD for acquired energy");
        AssertEqual(199, samus.MaxHealth, "return grants no duplicate energy");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(beforeTile != ShownBlock(), "post-message NMI displays the empty pickup block");
        ushort[] drawn = runtime.Hud.TileWords(runtime.InstalledHudArtwork);
        ushort[] published = runtime.AcceptedHudWords;
        for (int i = 0; i < drawn.Length; i++)
            if (i % 32 < 26)
                AssertEqual(drawn[i], published[FxOverlayPlaneConstants.Columns + i], "post-message NMI publishes HUD");
        Console.WriteLine("  Pickup message: native early grant, retained pickup/HUD during fanfare, and post-message visual update agree.");
    }

    /// <summary>Runs the early-grant and post-message visual update reproduction.</summary>
    [VerificationCommand("--collectible-message-timing")]
    internal static int CollectibleMessageTimingCommand()
    {
     VerifyCollectibleMessageTiming(); return 0; 
    }
}
