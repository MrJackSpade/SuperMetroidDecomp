using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Rooms.Doors.DoorAlignmentVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresDestructionVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Frontend;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks options, door opening, reserve recovery, and ending handoffs in the playable frontend.</summary>
internal static class FrontendCompletionVerification
{
/// <summary>Checks typed option-tile palette replacement leaves character, flip, and priority bits intact.</summary>
internal static void VerifyGameOptionsRomDataCatalog()
{
    const ushort packedTile = 0xe155;
    SnesBgTilemapWord selected = new SnesBgTilemapWord(packedTile)
        .WithPaletteIndex(GameOptionsPageDefinitions.TilePalettes.Selected);
    SnesBgTilemapWord unselected = selected
        .WithPaletteIndex(GameOptionsPageDefinitions.TilePalettes.Unselected);
    AssertEqual(GameOptionsPageDefinitions.TilePalettes.Selected, selected.PaletteIndex,
        "selected option uses typed tile palette");
    AssertEqual(GameOptionsPageDefinitions.TilePalettes.Unselected, unselected.PaletteIndex,
        "unselected option uses typed tile palette");
    AssertEqual(new SnesBgTilemapWord(packedTile).CharacterIndex, unselected.CharacterIndex,
        "options palette replacement preserves character index");
    AssertEqual(new SnesBgTilemapWord(packedTile).FlipFlags, unselected.FlipFlags,
        "options palette replacement preserves tile flips");
    AssertEqual(new SnesBgTilemapWord(packedTile).HasPriority, unselected.HasPriority,
        "options palette replacement preserves priority");

    Console.WriteLine(
        "  Options ROM data: typed " +
        "palette replacement agree.");
}

/// <summary>Exercises controller reassignment, options submenus, normalized inputs, and their native SRAM persistence.</summary>
internal static void VerifyControllerBindingsAndOptionsSubmenus()
{
    ControllerBindings swapped = ControllerBindings.Default.AssignAndSwap(
        action: 0,
        physicalButton: ControllerButton.R);
    AssertEqual(ControllerButton.R, swapped.Shoot, "controller swap assigns requested button");
    AssertEqual(ControllerButton.X, swapped.AimUp, "controller swap preserves permutation");
    ControllerButton normalized = swapped.Normalize(
        (ControllerButton.R | ControllerButton.X | ControllerButton.Left | ControllerButton.Right |
                 ControllerButton.Start));
    AssertTrue((normalized & ControllerButton.X) != 0,
        "physical remapped Shoot becomes canonical Shoot");
    AssertTrue((normalized & ControllerButton.R) != 0,
        "displaced physical button becomes canonical Aim Up");
    AssertTrue((normalized & (ControllerButton.Left | ControllerButton.Right | ControllerButton.Start)) ==
               (ControllerButton.Left | ControllerButton.Right | ControllerButton.Start),
        "opposed fixed directions and Start survive binding normalization");

    var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];
    // Five independent all-blank pages are enough to exercise the state machine. The
    // decompressor and page addresses remain real; visual asset fidelity is covered by the
    // production-ROM capture audits rather than embedding copyrighted menu data here.
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsSourceDefinitions.Pages[GameOptionsTilemap.Primary].Address,
        GameOptionsDefinitions.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsSourceDefinitions.Pages[GameOptionsTilemap.ControllerEnglish].Address,
        GameOptionsDefinitions.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsSourceDefinitions.Pages[GameOptionsTilemap.ControllerJapanese].Address,
        GameOptionsDefinitions.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsSourceDefinitions.Pages[GameOptionsTilemap.SpecialEnglish].Address,
        GameOptionsDefinitions.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsSourceDefinitions.Pages[GameOptionsTilemap.SpecialJapanese].Address,
        GameOptionsDefinitions.TilemapByteCount,
        0);
    var options = new GameOptionsMenuState(mapPresentation: RetailPresentationFixture(), spriteSheets: MenuSheetFixtures.For(RetailPresentationFixture()));
    StepOptionsUntil(options, GameOptionsPhase.Main);

    for (int row = 0; row < 3; row++)
        PressOptions(options, ControllerButton.Down);
    AssertEqual(3, options.SelectedItem, "primary options selects controller settings");
    PressOptions(options, ControllerButton.A);
    StepOptionsUntil(options, GameOptionsPhase.ControllerSettings);

    PressOptions(options, ControllerButton.R);
    AssertEqual(ControllerButton.R, options.ControllerBindings.Shoot,
        "controller page assigns physical R to Shoot");
    AssertEqual(ControllerButton.X, options.ControllerBindings.AimUp,
        "controller page swaps displaced Shoot button into Aim Up");

    for (int row = 0; row < 7; row++)
    {
        PressOptions(options, ControllerButton.Down);
        if (options.Phase == GameOptionsPhase.ScrollControllerDown)
            StepOptionsUntil(options, GameOptionsPhase.ControllerSettings);
    }
    AssertEqual(7, options.SelectedItem, "controller page reaches Exit after native scroll");
    PressOptions(options, ControllerButton.A);
    StepOptionsUntil(options, GameOptionsPhase.Main);

    for (int row = 0; row < 4; row++)
        PressOptions(options, ControllerButton.Down);
    PressOptions(options, ControllerButton.A);
    StepOptionsUntil(options, GameOptionsPhase.SpecialSettings);
    PressOptions(options, ControllerButton.A);
    AssertTrue(options.IconCancelEnabled, "special page toggles Icon Cancel");
    PressOptions(options, ControllerButton.Down);
    PressOptions(options, ControllerButton.Right);
    AssertTrue(options.MoonwalkEnabled, "special page toggles Moonwalk with Right");

    var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
    saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot
    {
        ControllerBindings = options.ControllerBindings,
        IconCancelEnabled = options.IconCancelEnabled,
        MoonwalkEnabled = options.MoonwalkEnabled,
    });
    SuperMetroidSaveSlot slot = saveRam.ReadSlot(0)
        ?? throw new InvalidOperationException("Options SRAM fixture failed its checksum.");
    AssertEqual(options.ControllerBindings, slot.ControllerBindings,
        "controller permutation round-trips through native SRAM words");
    AssertTrue(slot.IconCancelEnabled, "Icon Cancel round-trips through SRAM $09EA mirror");
    AssertTrue(slot.MoonwalkEnabled, "Moonwalk round-trips through SRAM $09E4 mirror");

    Console.WriteLine("  Options: controller swap, both submenus, normalization, and SRAM persistence agree.");
}

/// <summary>Checks reserve-energy input lock, one-point transfers, sound cadence, exhaustion, and maximum-health clamp.</summary>
internal static void VerifyReserveAutoRecovery()
{
    var samus = new SamusState
    {
        Health = 0,
        MaxHealth = 99,
        ReserveEnergy = 2,
        MaxReserveEnergy = 99,
        ReserveTankMode = ReserveTankMode.Auto,
    };
    var reserve = new SamusReserveAutoRecoveryState();
    reserve.Begin(samus);
    AssertTrue(samus.Control.AlphaLocked, "reserve command $1B locks Samus");

    SamusReserveAutoRecoveryStep first = reserve.StepAfterFrame(samus, 8);
    AssertEqual(1, samus.Health, "first reserve frame restores one energy");
    AssertEqual(1, samus.ReserveEnergy, "first reserve frame consumes one reserve");
    AssertTrue(first.RefillSoundRequested, "eighth NMI requests reserve refill sound");
    AssertTrue(!first.Completed, "nonempty reserve remains in state $1B");

    SamusReserveAutoRecoveryStep second = reserve.StepAfterFrame(samus, 9);
    AssertEqual(2, samus.Health, "second reserve frame restores one energy");
    AssertEqual(0, samus.ReserveEnergy, "second reserve frame exhausts reserve");
    AssertTrue(second.Completed && !samus.Control.AlphaLocked,
        "empty reserve publishes state eight and command $10 unlocks Samus");

    samus.Health = 0;
    samus.MaxHealth = 1;
    samus.ReserveEnergy = 5;
    samus.Control.Unlock();
    reserve.Begin(samus);
    SamusReserveAutoRecoveryStep clamped = reserve.StepAfterFrame(samus, 16);
    AssertEqual(1, samus.Health, "reserve recovery clamps at maximum energy");
    AssertEqual(0, samus.ReserveEnergy, "maximum-energy branch clears remaining reserve");
    AssertTrue(clamped.Completed, "maximum-energy branch completes state $1B");

    Console.WriteLine("  Reserve tanks: lock, one-point transfer, sound cadence, exhaustion, and clamp agree.");
}

/// <summary>Checks all four door IRQ paths for frame counts, destination alignment, layer positions, and delayed Samus nudge.</summary>
internal static void VerifyDoorOpeningTrajectories()
{
    // Door destination screens are multiplied by $100. Up is the only direction whose
    // IRQ endpoint is destination+$20; the other three finish on the header coordinate.
    const ushort destinationX = 0x0300;
    const ushort destinationY = 0x0200;
    var source = FixedPosition<RoomPixels>.FromWords(0x00a5, 0x4000, 0x0073, 0x8000);
    var postNudge = FixedPosition<RoomPixels>.FromWords(0x0318, 0x4000, 0x0218, 0x8000);
    var finalLayer2 = new Position16<BackgroundPixels>(0x0180, 0x0140);

    for (int direction = 0; direction < 4; direction++)
    {
        AssertEqual(
            direction >= 2,
            RoomLoader.DoorTransitionAlignsX(DoorRecordCodec.DecodeOrientation(checked((byte)direction)).Direction),
            $"door direction {direction} alignment axis");
        var door = new DoorDefinition(
            Name: "fixture door",
            DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
            Orientation: DoorRecordCodec.DecodeOrientation(checked((byte)direction)),
            PlmBlock: new(0, 0),
            DestinationScreen: new(3, 2),
            SamusSpeed: DoorSamusSpeed.SubpixelsPerStep(256),
            SetupCode: DoorSetupCode.None);
        ushort finalCameraY = direction == 3
            ? (ushort)(destinationY + 0x20)
            : destinationY;
        var trajectory = DoorOpeningScrollState.Create(
            door,
            source,
            new Position16<RoomPixels>(destinationX, finalCameraY),
            finalLayer2,
            postNudge);

        // $82:E3C0 rebases both axes before the later E6A2 nudge. The
        // perpendicular axis must not start at the supplied post-nudge endpoint.
        AssertEqual(direction switch
        {
            0 => 0x02aa4000u,
            1 => 0x04a04000u,
            _ => 0x03a54000u,
        }, trajectory.SamusPosition.X.RawValue, $"door direction {direction} rebased initial Samus X");
        AssertEqual(direction switch
        {
            2 => 0x01938000u,
            3 => 0x036d8000u,
            _ => 0x02738000u,
        }, trajectory.SamusPosition.Y.RawValue, $"door direction {direction} rebased initial Samus Y");

        int expectedFrames = direction switch
        {
            0 or 1 => 63,
            2 => 56,
            3 => 55,
            _ => throw new InvalidOperationException(),
        };
        AssertEqual(expectedFrames, trajectory.RemainingFrames,
            $"door direction {direction} remaining IRQ calls after setup");
        AssertEqual(direction switch
        {
            0 => (ushort)(destinationX - 252),
            1 => (ushort)(destinationX + 252),
            _ => destinationX,
        }, trajectory.CameraPosition.X, $"door direction {direction} initial camera X");
        AssertEqual(direction switch
        {
            2 => (ushort)(destinationY - 224),
            3 => (ushort)(destinationY + 251),
            _ => destinationY,
        }, trajectory.CameraPosition.Y, $"door direction {direction} initial camera Y");

        // Each IRQ call is one whole-value step along the travel axis: camera and layer two
        // move four pixels, Samus moves her 16.16 step, and the other axis never changes.
        var cameraStep = direction switch
        {
            0 => new Offset16<RoomPixels>(4, 0),
            1 => new Offset16<RoomPixels>(-4, 0),
            2 => new Offset16<RoomPixels>(0, 4),
            _ => new Offset16<RoomPixels>(0, -4),
        };
        var layer2Step = new Offset16<BackgroundPixels>(cameraStep.X, cameraStep.Y);
        int samusStepRaw = direction is 0 or 2 ? 0x0001_0000 : -0x0001_0000;
        Position16<RoomPixels> previousCamera = trajectory.CameraPosition;
        Position16<BackgroundPixels> previousLayer2 = trajectory.Layer2Position;
        FixedPosition<RoomPixels> previousSamus = trajectory.SamusPosition;
        for (int frame = 0; frame < expectedFrames; frame++)
        {
            bool completed = trajectory.Advance();
            if (direction == 3)
            {
                AssertEqual(frame >= 3, trajectory.RecalculatesScrollAfterAdvance,
                    $"up-door counter {frame + 2} streaming gate");
            }
            AssertEqual(frame == expectedFrames - 1, completed,
                $"door direction {direction} completion call");
            AssertEqual(cameraStep, trajectory.CameraPosition - previousCamera,
                $"door direction {direction} per-IRQ camera step");
            AssertEqual(layer2Step, trajectory.Layer2Position - previousLayer2,
                $"door direction {direction} per-IRQ layer-two step");
            uint expectedTravelRaw = unchecked((direction < 2 ? previousSamus.X : previousSamus.Y).RawValue + (uint)samusStepRaw);
            AssertEqual(expectedTravelRaw, (direction < 2 ? trajectory.SamusPosition.X : trajectory.SamusPosition.Y).RawValue,
                $"door direction {direction} per-IRQ Samus 16.16 step with 32-bit wrap");
            AssertEqual(direction < 2 ? previousSamus.Y : previousSamus.X,
                direction < 2 ? trajectory.SamusPosition.Y : trajectory.SamusPosition.X,
                $"door direction {direction} leaves Samus's perpendicular axis untouched");
            previousCamera = trajectory.CameraPosition;
            previousLayer2 = trajectory.Layer2Position;
            previousSamus = trajectory.SamusPosition;
        }
        AssertEqual(finalLayer2, trajectory.Layer2Position,
            $"door direction {direction} layer two reaches its endpoint on the final step");

        ushort preSnapCameraX = trajectory.CameraPosition.X;
        ushort preSnapCameraY = trajectory.CameraPosition.Y;
        AssertEqual(direction switch
        {
            0 => (ushort)(destinationX + 0x0000),
            1 => (ushort)(destinationX + 0x0000),
            _ => destinationX,
        }, preSnapCameraX, $"door direction {direction} final streamed camera X");
        AssertEqual(direction switch
        {
            2 => destinationY,
            3 => (ushort)(finalCameraY - 1),
            _ => finalCameraY,
        }, preSnapCameraY, $"door direction {direction} final streamed camera Y");

        trajectory.SnapLayerOneToDestination();
        AssertEqual(new Position16<RoomPixels>(destinationX, finalCameraY), trajectory.CameraPosition,
            $"door direction {direction} final camera snap");
        AssertEqual(finalLayer2, trajectory.Layer2Position,
            $"door direction {direction} snap leaves layer two at its streamed endpoint");
        AssertEqual(0, trajectory.RemainingFrames,
            $"door direction {direction} exhausts IRQ frame count");
        AssertTrue(trajectory.SamusPosition != trajectory.FinalSamusPosition,
            $"door direction {direction} leaves $82:E6A2 nudge for its later phase");
    }

    Console.WriteLine(
        "  Doors: all four IRQ trajectories, frame counts, endpoints, and delayed nudge agree.");
}

/// <summary>
/// Checks the credits roll takes its rows on half-pixel scroll boundaries and holds the last thirty-two, each one screen
/// below its row's place (#1467).
/// </summary>
internal static void VerifyCreditsObjectInterpreter()
{
    const int rows = 33;
    var credits = new CreditsObjectState(rows);
    for (int frame = 0; frame < 15; frame++)
        AssertTrue(!credits.Step().CopiedRow, "credits waits sixteen half-pixel frames");
    CreditsObjectStepResult first = credits.Step();
    AssertTrue(first.CopiedRow, "credits takes its first row at the eight-pixel boundary");
    AssertEqual(1, credits.NextSourceRow, "credits advances to its next row");
    AssertEqual(8, (int)credits.ScrollPosition.Y, "the first row is taken as the roll reaches eight pixels");

    for (int row = 1; row < rows; row++)
        for (int frame = 0; frame < 16; frame++)
            credits.Step();
    AssertEqual(rows, credits.NextSourceRow, "credits takes one row per eight pixels");

    CreditsObjectStepResult ending = default;
    for (int frame = 0; frame < 16; frame++)
        ending = credits.Step();
    AssertTrue(ending.Finished && !credits.Enabled,
        "compiled credits disable the row object one boundary after the final row");

    Console.WriteLine(
        "  Credits: half-pixel scroll, installed rows, circular staging, and completion agree.");
}

/// <summary>Runs the retail ending state through escape, rewards, percentage, and final hold when its ROM oracle is available.</summary>
internal static void VerifyEndingCreditsState()
{
    string romPath = Path.GetFullPath("Super Metroid.smc");
    if (!File.Exists(romPath))
    {
        Console.WriteLine("  Ending: retail-ROM state-$27 smoke test skipped (ROM not present).");
        return;
    }

    byte[] rom = File.ReadAllBytes(romPath);
    if (rom.Length != SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount)
    {
        throw new InvalidDataException(
            $"Ending smoke-test ROM is ${rom.Length:X} bytes; expected headerless retail size " +
            $"${SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount:X}.");
    }

    var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
    var audio = new SuperMetroid.Core.Audio.AudioQueues();
    var ending = CreateRetailEndingFixture(audio, gameTimeHours: 2, gameTimeMinutes: 59);
    EndingCreditsPhase previous = ending.Stage.Phase;
    var reached = new HashSet<EndingCreditsPhase> { previous };
    int renderedTransitions = 0;

    for (int frame = 0; frame < 60_000 && ending.Stage.Phase != EndingCreditsPhase.SeeYouNextMission; frame++)
    {
        ending.Step();
        audio.AdvanceFrame(default);
        if (ending.Stage.Phase != previous)
        {
            previous = ending.Stage.Phase;
            reached.Add(previous);
            RgbaPixel[] pixels = ending.Render();
            AssertEqual(FrontendFrame.Width * FrontendFrame.Height, pixels.Length,
                $"ending {previous} frame size");
            renderedTransitions++;
        }
    }

    AssertEqual(EndingCreditsPhase.SeeYouNextMission, ending.Stage.Phase,
        "ending reaches the retail final hold state");
    AssertTrue(reached.Contains(EndingCreditsPhase.ZebesExplosionAnimation),
        "ending executes the sprite-opcode-driven Zebes explosion");
    AssertTrue(reached.Contains(EndingCreditsPhase.OperationSuccessfulText),
        "ending executes the cartridge clear-time text chain");
    AssertTrue(reached.Contains(EndingCreditsPhase.Credits),
        "ending executes the ROM credits row stream");
    AssertTrue(reached.Contains(EndingCreditsPhase.PostCreditsReward),
        "ending executes the time-selected reward screen");
    AssertTrue(renderedTransitions >= 20,
        "ending renders each materially different cartridge phase");

    var middle = CreateRetailEndingFixture(audio, gameTimeHours: 3, gameTimeMinutes: 0);
    var slow = CreateRetailEndingFixture(audio, gameTimeHours: 10, gameTimeMinutes: 0);
    AssertEqual(EndingReward.Suitless, ending.Stage.EndingReward, "under-three-hour ending branch");
    AssertEqual(EndingReward.Helmetless, middle.Stage.EndingReward, "three-to-ten-hour ending branch");
    AssertEqual(EndingReward.Armored, slow.Stage.EndingReward, "ten-hour ending branch");

    var percentagePage = new EndingTextPage();
    var percentage = new EndingBackgroundTextState(new EndingBackgroundTextSetup { Page = percentagePage, Inventory = new EndingInventorySnapshot(
            MaxHealth: 1499,
            MaxReserveEnergy: 400,
            MaxMissiles: 230,
            MaxSuperMissiles: 50,
            MaxPowerBombs: 50,
            CollectedItems: (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.SpringBall | SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster | SamusEquipmentFlags.GrappleBeam | SamusEquipmentFlags.XrayScope),
            CollectedBeams: (SamusBeamFlags)0x100f), JapaneseText = false, Presentation = RetailPresentationFixture().EndingText, InstalledSequence = EndingTextSequence.ItemPercentage });
    var percentageScreen = new EndingScreen();
    for (int frame = 0; frame < 4_000 && !percentage.Completed; frame++)
        percentage.Step(percentageScreen);
    AssertTrue(percentage.Completed && percentage.RequestedItemPercentageScroll,
        "item-percentage object reaches its native scroll handoff");
    // The digits and the percent sign are pictures written left to right from the hundreds digit's cell (#1467).
    ShownEndingText[] digits = [.. percentagePage.Written.Where(text => text.Cell.Y == EndingResultTextLayout.PercentageHundredsCell.Y)];
    string[] hundredPercent = [EndingPictureNames.PercentDigit(1), EndingPictureNames.PercentDigit(0), EndingPictureNames.PercentDigit(0),
        EndingPictureNames.PercentSign];
    AssertTrue(digits.Select(text => text.Picture).SequenceEqual(hundredPercent), "100% shows one, zero, zero and the percent sign");
    AssertTrue(digits.Select(text => (int)text.Cell.X).SequenceEqual(Enumerable.Range(EndingResultTextLayout.PercentageHundredsCell.X, hundredPercent.Length)),
        "100% places its glyphs in consecutive cells from the hundreds digit");

    Console.WriteLine(
        "  Ending: escape, explosion, clear time, credits, rewards, 100% count, and final hold agree.");
}

/// <summary>Presses and releases an options button so successive menu inputs have distinct edges.</summary>
private static void PressOptions(GameOptionsMenuState options, ControllerButton button)
{
    options.Step(button);
    options.Step(0);
}

/// <summary>Advances neutral options frames until the expected phase or fails at the bounded frame limit.</summary>
private static void StepOptionsUntil(
    GameOptionsMenuState options,
    GameOptionsPhase expected,
    int maximumFrames = 80)
{
    for (int frame = 0; frame < maximumFrames && options.Phase != expected; frame++)
        options.Step(0);
    AssertEqual(expected, options.Phase, $"options reaches {expected}");
}

    /// <summary>Runs door opening trajectory and alignment parity suites from the verification command line.</summary>
    [VerificationCommand("--door-alignment")]
    internal static int DoorAlignmentCommand()
    {
        Suite(nameof(VerifyDoorOpeningTrajectories), () => VerifyDoorOpeningTrajectories());
        Suite(nameof(VerifyDoorAlignmentParity), () => VerifyDoorAlignmentParity());
        return 0;
    }
}
