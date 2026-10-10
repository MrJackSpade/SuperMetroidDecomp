using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1255: $84:8968 grants energy before its synchronous bank-$85 message,
    // but the following PLM empty draw and main-loop HUD update wait for return.
    /// <summary>Verifies that a retail energy-tank pickup grants and refills energy before its synchronous message, while deferring the pickup-tile and HUD updates until dismissal and the following NMI.</summary>
    private static void VerifyCollectibleMessageTiming()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x965b, cameraX: 1280);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.XPosition = 1296;
        samus.YPosition = 110;
        samus.Health = 49;
        samus.MaxHealth = 99;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        var item = runtime.Plms.Collectibles.Single(i => i.Kind == InWorldCollectibleKind.EnergyTank);
        ushort visible = runtime.LevelData!.ForegroundEntries.Span[item.BlockIndex];
        var update = runtime.BackgroundStreamer!.BuildPlmLevelBlockUpdate(item.BlockIndex,
            bg1XOffset: runtime.BackgroundScroll.Bg1XOffset);
        ushort[] TileWords() => new[] { runtime.Vram.ReadWord(update.TopRowDestination),
            runtime.Vram.ReadWord((ushort)(update.TopRowDestination + 1)),
            runtime.Vram.ReadWord((ushort)(update.TopRowDestination + 32)),
            runtime.Vram.ReadWord((ushort)(update.TopRowDestination + 33)) };
        ushort[] HudWords() => runtime.Hud.Tiles.ToArray().Where((_, index) => index % 32 < 26).ToArray();
        ushort[] VisibleHudWords() => Enumerable.Range(0, runtime.Hud.Tiles.Length)
            .Where(index => index % 32 < 26)
            .Select(index => runtime.Vram.ReadWord((ushort)(0x5820 + index))).ToArray();
        ushort[] beforeVisibleHud = VisibleHudWords();
        ushort[] beforeTile = TileWords();
        ushort[] beforeHud = HudWords();
        AssertTrue(runtime.Plms.TryNotifyCollectibleTouch(item.BlockIndex), "reported pickup accepts contact");
        runtime.StepFrame(0);
        AssertTrue(runtime.MessageBox.IsActive, "pickup begins synchronous message");
        AssertEqual((ushort)199, samus.MaxHealth, "native grant precedes message return");
        AssertEqual((ushort)199, samus.Health, "native energy refill precedes message return");
        Console.WriteLine($"Pickup frame: tile retained={beforeTile.SequenceEqual(TileWords())}, HUD retained={beforeHud.SequenceEqual(HudWords())}");
        void AssertHeld()
        {
            AssertEqual(visible, runtime.LevelData.ForegroundEntries.Span[item.BlockIndex], "pickup level tile remains during message");
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
        AssertTrue(runtime.LevelData.ForegroundEntries.Span[item.BlockIndex] != visible, "return executes pickup empty draw");
        AssertTrue(!beforeHud.SequenceEqual(HudWords()), "return rebuilds HUD for acquired energy");
        AssertEqual((ushort)199, samus.MaxHealth, "return grants no duplicate energy");
        runtime.RunNmi(0, true);
        AssertTrue(!beforeTile.SequenceEqual(TileWords()), "post-message NMI displays empty pickup tile");
        for (int i = 0; i < runtime.Hud.Tiles.Length; i++)
            if (i % 32 < 26)
                AssertEqual(runtime.Hud.Tiles[i], runtime.Vram.ReadWord((ushort)(0x5820 + i)), "post-message NMI publishes HUD");
        Console.WriteLine("  Pickup message: native early grant, retained pickup/HUD during fanfare, and post-message visual update agree.");
    }
}
