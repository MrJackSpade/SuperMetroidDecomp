using SuperMetroid.Core.Input;
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
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Checks X-ray setup's staged BG1 capture and frozen reveal-map lifetime.</summary>
internal static class XraySetupVerification
{
    /// <summary>Checks stage-four reveal construction from the shown BG1 plane with edited art and overlays (#1351).</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyXraySetupBuffers()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        // Change only the installed art for air reveals; the runtime still executes
        // the cartridge's compiled collision lookup and X-ray setup stages.
        var visualEntries = new List<XrayRevealVisualDefinition>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, unchecked((byte)bts));
            if (native is not { } definition ||
                !XrayRevealVisualCatalog.IsDrawable(definition.Command)) continue;
            visualEntries.Add(new(type, unchecked((byte)bts), new XrayRevealVisualWords(
                type == RoomCollisionType.Air ? (ushort)0x00fe : definition.TopLeft,
                definition.TopRight, definition.BottomLeft, definition.BottomRight)));
        }
        runtime.Xray.XrayRevealVisuals = new XrayRevealVisualCatalog(visualEntries, (runtime.Xray.XrayRevealVisuals ?? throw new InvalidOperationException("Verification requires runtime.Xray.XrayRevealVisuals.")).Overlays);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.XrayScope);
        var samus = runtime.Player.IntroducedSamus;
        var support = runtime.Enemies.SlotTable.Slots[1];
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.Position = samus.Position.WithWhole(new(support.Position.X.Whole, (ushort)(support.Position.Y.Whole - support.Radius.Y - samus.Kinematics.Radius.Y)));
        runtime.StepFrame(0);
        AssertTrue(samus.Xray.TryBegin(samus, samus.PoseMetadata.ReadMovementType(), new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "setup fixture activates X-ray");
        SnesButton held = runtime.Options.ControllerBindings.Dash;
        runtime.StepFrame(held); // Stage one, before either BG1 page is captured.
        RoomLevelData level = runtime.Room.LoadedLevelData;
        // BG1 shows level data (#1346): repaint every block's visual with one tile word, keeping collision.
        void ShowEverywhere(ushort tile)
        {
            const int visual = 1;
            for (int corner = 0; corner < 4; corner++)
                level.SetBlockDefinitionWord(visual * 4 + corner, tile);
            ReadOnlySpan<ushort> blocks = level.ForegroundEntries.Span;
            for (int block = 0; block < blocks.Length; block++)
                level.SetPlmForegroundEntry(block, blocks[block], visual);
        }
        // Stage four builds the reveal from the BG1 plane the room shows then (#1351); nothing is captured earlier.
        ShowEverywhere(0x0456);
        runtime.StepFrame(held);
        ShowEverywhere(0x0789);
        runtime.StepFrame(held);
        ShowEverywhere(0x0abc);
        runtime.StepFrame(held);
        var scroll = runtime.BackgroundScroll;
        var room = runtime.Room.LoadedRoom;
        var bg1 = new RoomBackgroundLayer(level.ForegroundPlane, scroll.Bg1Offset.AsBgPlaneOffset());
        var original = XrayRevealTilemap.Build(level, bg1,
            scroll.Layer1Position + scroll.Bg1Offset,
            scroll.Layer1Position, (byte)room.AreaIndex);
        var expected = XrayRevealTilemap.Build(level, bg1,
            scroll.Layer1Position + scroll.Bg1Offset,
            scroll.Layer1Position, (byte)room.AreaIndex,
            runtime.Xray.XrayRevealVisuals);
        AssertTrue(!original.SequenceEqual(expected),
            "installed X-ray visual selection changes the actual room reveal map");
        XrayRevealOverlays.Apply(level, expected, runtime.Plms.CollectiblePlms.Collectibles, runtime.System,
            room.State.XrayOverlay, scroll.Layer1Position, runtime.Xray.XrayRevealVisuals);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(runtime.Xray)), "stage four builds from the shown BG1 plane and overlays");
        AssertTrue(expected.Contains((ushort)0x0abc), "the reveal copies the level data shown at stage four");
        AssertTrue(!expected.Any(word => word is 0x0456 or 0x0789), "no earlier setup call's BG1 reaches the reveal");
        ShowEverywhere(0x0def);
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(held);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(runtime.Xray)), "widening/full beam never rebuilds the frozen reveal map");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive, "setup fixture tears down normally");
        Console.WriteLine("  X-ray setup: the stage-four reveal resolves from level data, then stays frozen through the full beam.");
    }

    /// <summary>Runs the staged X-ray setup buffer check.</summary>
    [VerificationCommand("--xray-setup")]
    internal static int XraySetupCommand()
    {
        Suite(nameof(VerifyXraySetupBuffers), () => VerifyXraySetupBuffers());
        return 0;
    }
}
