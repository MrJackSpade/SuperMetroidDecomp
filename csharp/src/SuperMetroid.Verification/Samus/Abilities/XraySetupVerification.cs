using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.Xray;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks X-ray setup's staged BG1 capture and frozen reveal-map lifetime.</summary>
internal static class XraySetupVerification
{
    /// <summary>Checks NMI sampling of each BG1 page and stage-four reveal construction with edited art and overlays.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyXraySetupBuffers()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        // Change only the installed art for air reveals; the runtime still executes
        // the cartridge's compiled collision lookup and X-ray setup stages.
        var visualEntries = new List<(RoomCollisionType Type, byte Bts,
            XrayRevealVisualWords Visual)>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, unchecked((byte)bts));
            if (native is not { } definition ||
                !XrayRevealVisualCatalog.IsDrawable(definition.Command)) continue;
            visualEntries.Add((type, unchecked((byte)bts), new XrayRevealVisualWords(
                type == RoomCollisionType.Air ? (ushort)0x00fe : definition.TopLeft,
                definition.TopRight, definition.BottomLeft, definition.BottomRight)));
        }
        runtime.Xray.XrayRevealVisuals = new XrayRevealVisualCatalog(visualEntries, (runtime.Xray.XrayRevealVisuals ?? throw new InvalidOperationException("Verification requires runtime.Xray.XrayRevealVisuals.")).Overlays);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.BrinstarShutterRoom);
        var samus = runtime.Player.IntroducedSamus;
        var support = runtime.Enemies.SlotTable.Slots[1];
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.Position = samus.Position.WithWhole(new(support.Position.X.Whole, (ushort)(support.Position.Y.Whole - support.Radius.Y - samus.Kinematics.Radius.Y)));
        runtime.StepFrame(0);
        AssertTrue(samus.Xray.TryBegin(bus, samus, samus.PoseMetadata.ReadMovementType(bus), new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }), "setup fixture activates X-ray");
        ushort held = runtime.Options.ControllerBindings.Dash;
        runtime.StepFrame(held); // Stage one, before either BG1 page is captured.
        var first = Enumerable.Repeat((ushort)0x0123, XrayTilemapLayout.ScreenWords).ToArray();
        var second = Enumerable.Repeat((ushort)0x0456, XrayTilemapLayout.ScreenWords).ToArray();
        runtime.Vram.ExecuteWordTransfer(second,
            GameplayVramLayout.Bg1TilemapWord + XrayTilemapLayout.ScreenWords, 1);
        runtime.StepFrame(held);
        Suite(nameof(VerifySaved), () => VerifySaved(XrayTilemapLayout.ScreenWords, new ushort[second.Length], "stage two only queues the second page read"));
        // The source is sampled at the NEXT NMI, not when the request is issued.
        Array.Fill(second, (ushort)0x0789);
        runtime.Vram.ExecuteWordTransfer(second,
            GameplayVramLayout.Bg1TilemapWord + XrayTilemapLayout.ScreenWords, 1);
        runtime.Vram.ExecuteWordTransfer(first, GameplayVramLayout.Bg1TilemapWord, 1);
        runtime.StepFrame(held);
        Suite(nameof(VerifySaved), () => VerifySaved(XrayTilemapLayout.ScreenWords, second, "stage-two read completes at the next NMI"));

        // Deliberately destroy the live source before stage four. A render-time rebuild
        // or a single late VRAM snapshot cannot pass this page-ownership assertion.
        runtime.Vram.ExecuteWordTransfer(new ushort[XrayTilemapLayout.ScreenWords],
            GameplayVramLayout.Bg1TilemapWord + XrayTilemapLayout.ScreenWords, 1);
        // Queue a mutable-memory transfer from the already captured second page.
        int source = XraySetupMemory.SavedBg1SecondScreen;
        first[0] = second[0];
        runtime.VramWrites.Enqueue(sizeInBytes: 2, sourceAddress: source,
            destination: VramDmaDestination.FromNative(GameplayVramLayout.Bg1TilemapWord));
        var captured = new SnesVram();
        captured.ExecuteWordTransfer(first.Concat(second).ToArray(), GameplayVramLayout.Bg1TilemapWord, 1);
        var scroll = runtime.BackgroundScroll;
        var room = runtime.Room.LoadedRoom;
        var original = XrayRevealTilemap.Build(runtime.Room.LoadedLevelData, captured,
            scroll.Layer1Position + scroll.Bg1Offset,
            scroll.Layer1Position, (byte)room.AreaIndex);
        var expected = XrayRevealTilemap.Build(runtime.Room.LoadedLevelData, captured,
            scroll.Layer1Position + scroll.Bg1Offset,
            scroll.Layer1Position, (byte)room.AreaIndex,
            runtime.Xray.XrayRevealVisuals);
        AssertTrue(!original.SequenceEqual(expected),
            "installed X-ray visual selection changes the actual room reveal map");
        XrayRevealOverlays.Apply(runtime.Room.LoadedLevelData, expected, runtime.Plms.CollectiblePlms.Collectibles, runtime.System,
            room.State.XrayPointer, scroll.Layer1Position, runtime.Xray.XrayRevealVisuals);
        runtime.StepFrame(held);
        Suite(nameof(VerifySaved), () => VerifySaved(0, first, "stage-three read completes before stage-four build"));
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(bus)), "stage four builds from the separately captured pages and overlays");
        AssertTrue(expected.Any(word => word == first[1] || word == second[0]), "fixture observes captured BG1 content, not only reveal replacements");
        runtime.Vram.ExecuteWordTransfer(new ushort[XrayTilemapLayout.BufferWords], GameplayVramLayout.Bg1TilemapWord, 1);
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(held);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(bus)), "widening/full beam never rebuilds the frozen reveal map");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive, "setup fixture tears down normally");
        Console.WriteLine("  X-ray setup: separate BG1 capture calls, frozen WRAM reveal/overlays and full-beam lifetime agree.");

        void VerifySaved(int firstWord, ushort[] words, string context)
        {
            WorkRamWordTable saved = new XrayMemory(bus).SavedBg1;
            for (int i = 0; i < words.Length; i++)
                AssertEqual(words[i], saved[firstWord + i], $"{context}, word {i}");
        }
    }

    /// <summary>Runs the staged X-ray setup buffer check.</summary>
    [VerificationCommand("--xray-setup")]
    internal static int XraySetupCommand()
    {
        Suite(nameof(VerifyXraySetupBuffers), () => VerifyXraySetupBuffers());
        return 0;
    }
}
