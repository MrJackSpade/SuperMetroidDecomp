using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
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

    /// <summary>Checks stage-four reveal construction from the shown BG1 plane with the installed art and overlays (#1351).</summary>
    internal static void VerifyXraySetupBuffers()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.XrayScope);
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
                level.SetObjectTerrain(block, level.GetObjectCollisionBlockByIndex(block).Terrain, RoomBlockAppearance.Of(Markers[stage]));
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
        // The reveal the room gives now: the foreground it shows, its terrain reveals and its overlays (#1514).
        RevealedBlock?[] Expected()
        {
            var visuals = runtime.Xray.XrayRevealVisuals
                ?? throw new InvalidOperationException("Verification requires the runtime's installed X-ray reveals.");
            RevealedBlock?[] built = XrayRevealMap.Build(level, scroll.ForegroundPosition, room.AreaIndex, visuals);
            XrayRevealOverlays.Apply(built, runtime.RoomObjects.CollectibleObjects.Collectibles, runtime.System,
                room.State.XrayOverlay, scroll.ForegroundPosition, visuals);
            return built;
        }
        XrayRevealPlane Shown() => runtime.Display.Latched.XrayReveal ?? throw new InvalidOperationException("X-ray setup's reveal is not displayed.");
        RevealedBlock?[] expected = Expected();
        AssertTrue(expected.SequenceEqual(Shown().Blocks.ToArray()), "the shown reveal is built from the foreground the room shows and its overlays");
        AssertTrue(expected.Contains(Marker(2)), "the reveal copies the level data the frame shows");
        AssertTrue(!expected.Any(block => block == Marker(0) || block == Marker(1)), "no earlier frame's foreground reaches the reveal");
        // An uncollected item that shows its own picture overlays that picture's block (#1538), which lies past the common
        // room elements like the markers do.
        HashSet<int> itemPictures = [.. runtime.RoomObjects.CollectibleObjects.Collectibles
            .Where(item => item.Location is { } location && !runtime.System.HasCollectedItem(location) && CollectibleObjects.ShowsOwnPicture(item.Kind))
            .Select(item => CollectibleCellPictures.DefinitionOf(new CollectiblePicture(item.Kind, 0)))];
        RevealedBlock? stray = expected.FirstOrDefault(block => block is not null && block != Marker(2) &&
            block.Value.Definition >= CommonRoomElementBlocks && !itemPictures.Contains(block.Value.Definition));
        AssertTrue(stray is null,
            $"every revealed block is the shown foreground block as the level shows it, a reveal or overlay metatile, or an uncollected item's own picture (first other: {stray}, at {Array.IndexOf(expected, stray)})");
        AssertEqual(XrayRevealMap.OriginOf(scroll.ForegroundPosition), Shown().Origin,
            "the reveal lies over the room's own blocks");
        ShowEverywhere(3);
        for (int frame = 0; frame < 90; frame++) runtime.StepFrame(held);
        expected = Expected();
        AssertTrue(expected.Contains(Marker(3)) && expected.SequenceEqual(Shown().Blocks.ToArray()),
            "through the full beam the reveal shows the room as each frame shows it");
        // Each revealed block draws from the picture the room's own foreground draws for that block (#1392), laid out as the
        // reveal placed it.
        XrayRevealPlane shown = Shown();
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException("X-ray setup displays no room.");
        AssertTrue(ReferenceEquals(rooms.Foreground.Pictures, shown.Pictures), "the reveal draws its blocks from the pictures the room's foreground draws");
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        AssertTrue(!samus.Xray.IsActive, "setup fixture tears down normally");
        Console.WriteLine("  X-ray setup: the reveal resolves from the level data each frame shows, through the full beam.");
    }

    /// <summary>Runs the staged X-ray setup buffer check.</summary>
    [VerificationCommand("--xray-setup")]
    internal static int XraySetupCommand()
    {
        Suite(nameof(VerifyXraySetupBuffers), () => VerifyXraySetupBuffers());
        return 0;
    }
}
