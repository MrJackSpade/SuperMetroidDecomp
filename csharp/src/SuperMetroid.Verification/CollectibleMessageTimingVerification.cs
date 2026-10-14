using SuperMetroid.Core;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hardware;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks the pickup grant and delayed visual update around the synchronous item message.</summary>
internal static class CollectibleMessageTimingVerification
{
    /// <summary>Confirms $84:8968 grants energy before the bank-$85 message, while empty PLM draw and HUD update wait for return (#1255).</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyCollectibleMessageTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x965b, camera: new Position16<RoomPixels>(1280, 0));
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(1296, 110));
        samus.Health = 49;
        samus.MaxHealth = 99;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        var item = runtime.Plms.CollectiblePlms.Collectibles.Single(i => i.Kind == InWorldCollectibleKind.EnergyTank);
        ushort visible = runtime.Room.LoadedLevelData.ForegroundEntries.Span[item.BlockIndex];
        var update = (runtime.Room.BackgroundStreamer ?? throw new InvalidOperationException("Verification requires runtime.Room.BackgroundStreamer.")).BuildPlmLevelBlockUpdate(item.BlockIndex,
            bg1XOffset: unchecked((ushort)runtime.BackgroundScroll.Bg1Offset.X));
        ushort[] TileWords() => new[] { runtime.Vram.ReadWord(update.TopRowDestination),
            runtime.Vram.ReadWord((ushort)(update.TopRowDestination + 1)),
            runtime.Vram.ReadWord((ushort)(update.TopRowDestination + 32)),
            runtime.Vram.ReadWord((ushort)(update.TopRowDestination + 33)) };
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
        AssertEqual((ushort)199, samus.MaxHealth, "native grant precedes message return");
        AssertEqual((ushort)199, samus.Health, "native energy refill precedes message return");
        Console.WriteLine($"Pickup frame: tile retained={beforeTile.SequenceEqual(TileWords())}, HUD retained={beforeHud.SequenceEqual(HudWords())}");
        void AssertHeld()
        {
            AssertEqual(visible, runtime.Room.LoadedLevelData.ForegroundEntries.Span[item.BlockIndex], "pickup level tile remains during message");
            AssertTrue(beforeTile.SequenceEqual(TileWords()), "visible pickup VRAM remains during message");
            AssertTrue(beforeHud.SequenceEqual(HudWords()), "HUD counters remain during message");
            AssertTrue(beforeVisibleHud.SequenceEqual(VisibleHudWords()), "displayed HUD VRAM remains during message");
            AssertEqual((ushort)199, samus.MaxHealth, "message cannot repeat acquisition");
        }
        AssertHeld();
        int frames = 0;
        while (runtime.MessageBox.Phase != GameplayMessageBoxPhase.AwaitingInput && frames++ < 400)
        {
            runtime.StepFrame(0);
            AssertHeld();
        }
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, runtime.MessageBox.Phase, "fanfare reaches acknowledgement");
        runtime.StepFrame((ushort)SnesButton.A);
        while (runtime.MessageBox.IsActive && frames++ < 450)
        {
            AssertHeld();
            runtime.StepFrame(0);
        }
        AssertTrue(!runtime.MessageBox.IsActive, "message returns");
        AssertTrue(runtime.Room.LoadedLevelData.ForegroundEntries.Span[item.BlockIndex] != visible, "return executes pickup empty draw");
        AssertTrue(!beforeHud.SequenceEqual(HudWords()), "return rebuilds HUD for acquired energy");
        AssertEqual((ushort)199, samus.MaxHealth, "return grants no duplicate energy");
        runtime.NmiHandler.RunNmi(0, true);
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
