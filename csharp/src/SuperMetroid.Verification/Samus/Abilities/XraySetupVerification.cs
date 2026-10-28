using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Samus.Drawing;
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
namespace SuperMetroid.Verification.Samus.Abilities;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Progression;

/// <summary>Checks X-ray setup's staged BG1 capture and frozen reveal-map lifetime.</summary>
internal static class XraySetupVerification
{
    /// <summary>The common room elements' block definitions, which precede the tileset's own.</summary>
    private const int CommonRoomElementBlocks = 0x100;

    /// <summary>Checks stage-four reveal construction from the shown BG1 plane with edited art and overlays (#1351).</summary>
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
                type == RoomCollisionType.Air ? 0x00fe : definition.TopLeft,
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
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Position = samus.Position.WithWhole(new(support.Position.X.Whole, (ushort)(support.Position.Y.Whole - support.Radius.Y - samus.Kinematics.Radius.Y)));
        runtime.StepFrame(0);
        AssertTrue(samus.Xray.TryBegin(samus, SamusPoseMetadata.ReadMovementType(samus), new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "setup fixture activates X-ray");
        ControllerButton held = runtime.Options.ControllerBindings.Dash;
        runtime.StepFrame(held); // Stage one, before either BG1 page is captured.
        RoomLevelData level = runtime.Room.LoadedLevelData;
        // Markers are tileset blocks whose four tile words no other definition holds, so a word of one in the reveal
        // can only have been copied from a block showing it; the room draws them from their installed pictures (#1392).
        ushort[] words = BackgroundTileWordBytes.Words(level.BlockDefinitions.Span);
        Dictionary<ushort, int> uses = words.GroupBy(word => word).ToDictionary(group => group.Key, group => group.Count());
        int[] markers = [.. Enumerable.Range(CommonRoomElementBlocks, words.Length / 4 - CommonRoomElementBlocks)
            .Where(definition => Enumerable.Range(0, 4).All(corner => uses[words[definition * 4 + corner]] == 1)).Take(4)];
        AssertEqual(4, markers.Length, "the room's tileset has four blocks of unique tile words");
        BackgroundTile Marker(int stage) => BackgroundTileWordBytes.Tile(words[markers[stage] * 4]);
        // BG1 shows level data (#1346): show one block everywhere, keeping collision.
        void ShowEverywhere(int stage)
        {
            ReadOnlySpan<ushort> blocks = level.ForegroundEntries.Span;
            for (int block = 0; block < blocks.Length; block++)
                level.SetPlmForegroundEntry(block, blocks[block], checked((ushort)markers[stage]));
        }
        // Stage four builds the reveal from the BG1 plane the room shows then (#1351); nothing is captured earlier.
        ShowEverywhere(0);
        runtime.StepFrame(held);
        ShowEverywhere(1);
        runtime.StepFrame(held);
        ShowEverywhere(2);
        runtime.StepFrame(held);
        var scroll = runtime.BackgroundScroll;
        var room = runtime.Room.LoadedRoom;
        IBgTileSource bg1 = level.ForegroundPlane.Tiles(scroll.Bg1Offset.AsBgPlaneOffset());
        var original = XrayRevealTilemap.Build(level, bg1,
            scroll.Layer1Position + scroll.Bg1Offset,
            scroll.Layer1Position, room.AreaIndex);
        var expected = XrayRevealTilemap.Build(level, bg1,
            scroll.Layer1Position + scroll.Bg1Offset,
            scroll.Layer1Position, room.AreaIndex,
            runtime.Xray.XrayRevealVisuals);
        AssertTrue(!original.SequenceEqual(expected),
            "installed X-ray visual selection changes the actual room reveal map");
        XrayRevealOverlays.Apply(level, expected, runtime.Plms.CollectiblePlms.Collectibles, runtime.System,
            room.State.XrayOverlay, scroll.Layer1Position, runtime.Xray.XrayRevealVisuals);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(runtime.Xray)), "stage four builds from the shown BG1 plane and overlays");
        AssertTrue(expected.Contains(Marker(2)), "the reveal copies the level data shown at stage four");
        AssertTrue(!expected.Any(tile => tile == Marker(0) || tile == Marker(1)), "no earlier setup call's BG1 reaches the reveal");
        ShowEverywhere(3);
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(held);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(runtime.Xray)), "widening/full beam never rebuilds the frozen reveal map");
        // The displayed reveal, latched from the frozen map, draws each block from the room's block pictures as its words
        // lay it out (#1392).
        Rooms.Artwork.RoomBlockPicturesVerification.AssertRevealPaintsWords(runtime, expected, "X-ray setup's reveal");
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
