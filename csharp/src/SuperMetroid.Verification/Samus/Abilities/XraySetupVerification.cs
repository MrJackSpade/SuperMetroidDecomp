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
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Progression;

/// <summary>Checks X-ray setup's staged BG1 capture and frozen reveal-map lifetime.</summary>
internal static class XraySetupVerification
{
    /// <summary>The common room elements' blocks, which precede the tileset's own.</summary>
    private const int CommonRoomElementBlocks = 0x100;

    /// <summary>The four tileset blocks the fixture shows across BG1, one per setup call, by block index.</summary>
    private static readonly int[] Markers = [CommonRoomElementBlocks, CommonRoomElementBlocks + 1, CommonRoomElementBlocks + 2,
        CommonRoomElementBlocks + 3];

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
            XrayRevealDefinition? native = XrayRevealTable.Find(type, new RoomBlockBehavior(bts));
            if (native is not { } definition ||
                !InstalledXrayReveals.IsDrawable(definition.Command)) continue;
            visualEntries.Add(new(type, new RoomBlockBehavior(bts), new XrayRevealVisualBlocks(
                type == RoomCollisionType.Air ? 0x00fe : definition.TopLeft,
                definition.TopRight, definition.BottomLeft, definition.BottomRight)));
        }
        runtime.Xray.XrayRevealVisuals = new InstalledXrayReveals(visualEntries, (runtime.Xray.XrayRevealVisuals ?? throw new InvalidOperationException("Verification requires runtime.Xray.XrayRevealVisuals.")).Overlays);
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
        // Markers are tileset blocks, which no reveal or overlay metatile names, so a revealed marker can only have been
        // copied from BG1 showing it (#1431); the room draws them from their installed pictures (#1392).
        RevealedBlock Marker(int stage) => new(Markers[stage], RevealedBlockArrangement.AsDefined);
        // BG1 shows level data (#1346): show one unmirrored block everywhere, keeping collision.
        void ShowEverywhere(int stage)
        {
            for (int block = 0; block < level.Size.Area; block++)
                level.SetPlmTerrain(block, level.GetPlmCollisionBlockByIndex(block).Terrain, RoomBlockAppearance.Of(Markers[stage]));
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
        RevealedBlock?[] original = XrayRevealMap.Build(level, scroll.Layer1Position, scroll.Bg1Offset, room.AreaIndex);
        RevealedBlock?[] expected = XrayRevealMap.Build(level, scroll.Layer1Position, scroll.Bg1Offset, room.AreaIndex,
            runtime.Xray.XrayRevealVisuals);
        AssertTrue(!original.SequenceEqual(expected),
            "installed X-ray visual selection changes the actual room reveal map");
        XrayRevealOverlays.Apply(level, expected, runtime.Plms.CollectiblePlms.Collectibles, runtime.System,
            room.State.XrayOverlay, scroll.Layer1Position, runtime.Xray.XrayRevealVisuals);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(runtime.Xray)), "stage four builds from the shown BG1 plane and overlays");
        AssertTrue(expected.Contains(Marker(2)), "the reveal copies the level data shown at stage four");
        AssertTrue(!expected.Any(block => block == Marker(0) || block == Marker(1)), "no earlier setup call's BG1 reaches the reveal");
        AssertTrue(expected.All(block => block is null || block == Marker(2) || block.Value.Definition < CommonRoomElementBlocks),
            "every revealed block is the stage-four BG1 block as the level shows it, or a reveal or overlay metatile");
        ShowEverywhere(3);
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(held);
        AssertTrue(expected.SequenceEqual(XraySetupMemory.ReadReveal(runtime.Xray)), "widening/full beam never rebuilds the frozen reveal map");
        // The displayed reveal is the frozen map's blocks (#1431), each drawn from the picture the room's own BG1 draws for
        // that block (#1392), laid out as the reveal placed it.
        XrayRevealPlane shown = runtime.Display.Latched.XrayReveal ?? throw new InvalidOperationException("X-ray setup's reveal is not displayed.");
        AssertTrue(expected.SequenceEqual(shown.Blocks.ToArray()), "the displayed reveal shows the frozen map's blocks and arrangements");
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException("X-ray setup displays no room.");
        AssertTrue(ReferenceEquals(rooms.Bg1.Pictures, shown.Pictures), "the reveal draws its blocks from the pictures the room's BG1 draws");
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
