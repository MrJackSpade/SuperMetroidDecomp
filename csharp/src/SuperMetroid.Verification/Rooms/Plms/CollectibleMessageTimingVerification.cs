using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Plms;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Checks the pickup grant and delayed visual update around the synchronous item message.</summary>
internal static class CollectibleMessageTimingVerification
{
    /// <summary>Confirms $84:8968 grants energy before the bank-$85 message, while empty PLM draw and HUD update wait for return (#1255).</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyCollectibleMessageTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.GauntletETank, camera: new Position16<RoomPixels>(1280, 0));
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
        var item = runtime.Plms.CollectiblePlms.Collectibles.Single(i => i.Kind == InWorldCollectibleKind.EnergyTank);
        ushort visible = runtime.Room.LoadedLevelData.ForegroundEntries.Span[item.BlockIndex];
        // The tiles the displayed frame shows for the pickup block: its latched room BG1 (#1346).
        ushort[] TileWords()
        {
            ExpandedBlockTiles shown = (runtime.Display.Latched.Rooms ?? throw new InvalidOperationException(
                "The displayed frame has no room backgrounds.")).Bg1.Plane.BlockTiles(
                runtime.Room.LoadedLevelData.Size.CellAtRowMajorIndex(item.BlockIndex));
            return [shown.TopLeft.Raw, shown.TopRight.Raw, shown.BottomLeft.Raw, shown.BottomRight.Raw];
        }
        ushort[] HudWords() => runtime.Hud.Tiles.WhereToArray((_, index) => index % 32 < 26);
        ushort[] VisibleHudWords() => Enumerable.Range(0, runtime.Hud.Tiles.Length)
            .Where(index => index % 32 < 26)
            .Select(index => runtime.Vram.ReadWord((ushort)(0x5820 + index))).ToArray();
        ushort[] beforeVisibleHud = VisibleHudWords();
        ushort[] beforeTile = TileWords();
        ushort[] beforeHud = HudWords();
        AssertTrue(runtime.Plms.CollectiblePlms.TryNotifyCollectibleTouch(item.BlockIndex), "reported pickup accepts contact");
        runtime.StepFrame(0);
        AssertTrue(runtime.MessageBox.IsActive, "pickup begins synchronous message");
        AssertEqual(199, samus.MaxHealth, "native grant precedes message return");
        AssertEqual(199, samus.Health, "native energy refill precedes message return");
        Console.WriteLine($"Pickup frame: tile retained={beforeTile.SequenceEqual(TileWords())}, HUD retained={beforeHud.SequenceEqual(HudWords())}");
        void AssertHeld()
        {
            AssertEqual(visible, runtime.Room.LoadedLevelData.ForegroundEntries.Span[item.BlockIndex], "pickup level tile remains during message");
            AssertTrue(beforeTile.SequenceEqual(TileWords()), "displayed pickup tile remains during message");
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
        runtime.StepFrame(SnesButton.A);
        while (runtime.MessageBox.IsActive && frames++ < 450)
        {
            AssertHeld();
            runtime.StepFrame(0);
        }
        AssertTrue(!runtime.MessageBox.IsActive, "message returns");
        AssertTrue(runtime.Room.LoadedLevelData.ForegroundEntries.Span[item.BlockIndex] != visible, "return executes pickup empty draw");
        AssertTrue(!beforeHud.SequenceEqual(HudWords()), "return rebuilds HUD for acquired energy");
        AssertEqual(199, samus.MaxHealth, "return grants no duplicate energy");
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(!beforeTile.SequenceEqual(TileWords()), "post-message NMI displays empty pickup tile");
        for (int i = 0; i < runtime.Hud.Tiles.Length; i++)
            if (i % 32 < 26)
                AssertEqual(runtime.Hud.Tiles[i], runtime.Vram.ReadWord((ushort)(0x5820 + i)), "post-message NMI publishes HUD");
        Console.WriteLine("  Pickup message: native early grant, retained pickup/HUD during fanfare, and post-message visual update agree.");
    }

    /// <summary>Runs the early-grant and post-message visual update reproduction.</summary>
    [VerificationCommand("--collectible-message-timing")]
    internal static int CollectibleMessageTimingCommand()
    {
     VerifyCollectibleMessageTiming(); return 0; 
    }
}
