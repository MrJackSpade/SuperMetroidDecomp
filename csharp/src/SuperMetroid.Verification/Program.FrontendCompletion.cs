using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
static void VerifyGameOptionsRomDataCatalog()
{
    GameOptionsPageResource[] pages =
    [
        GameOptionsRomData.Pages.Primary,
        GameOptionsRomData.Pages.ControllerEnglish,
        GameOptionsRomData.Pages.ControllerJapanese,
        GameOptionsRomData.Pages.SpecialEnglish,
        GameOptionsRomData.Pages.SpecialJapanese,
    ];
    AssertEqual(5, pages.Length, "options compressed page count");
    AssertEqual(pages.Length, pages.Select(page => page.Address).Distinct().Count(),
        "options compressed page addresses are unique");
    foreach (GameOptionsPageResource page in pages)
    {
        AssertTrue(page.Address is >= 0x808000 and <= 0xffffff,
            $"{page.Description} options resource is mapped ROM");
        AssertTrue(!string.IsNullOrWhiteSpace(page.Description),
            $"options resource ${page.Address:X6} has a diagnostic name");
    }

    AssertEqual(GameOptionsRomData.Rows.PrimaryCount,
        GameOptionsRomData.Cursors.PrimaryY.Length,
        "primary cursor rows match navigation rows");
    AssertEqual(GameOptionsRomData.Rows.ControllerCount,
        GameOptionsRomData.Cursors.ControllerY.Length,
        "controller cursor rows match navigation rows");
    AssertEqual(GameOptionsRomData.Rows.SpecialCount,
        GameOptionsRomData.Cursors.SpecialY.Length,
        "special cursor rows match navigation rows");
    AssertEqual(GameOptionsRomData.Rows.ControllerActionCount,
        GameOptionsRomData.ControllerLabels.Sources.Length,
        "controller label source count matches assignable actions");
    AssertEqual(GameOptionsRomData.Rows.ControllerActionCount,
        GameOptionsRomData.ControllerLabels.Destinations.Length,
        "controller label destination count matches assignable actions");

    const ushort packedTile = 0xe155;
    SnesBgTilemapWord selected = new SnesBgTilemapWord(packedTile)
        .WithPaletteIndex(GameOptionsRomData.TilePalettes.Selected);
    SnesBgTilemapWord unselected = selected
        .WithPaletteIndex(GameOptionsRomData.TilePalettes.Unselected);
    AssertEqual(GameOptionsRomData.TilePalettes.Selected, selected.PaletteIndex,
        "selected option uses typed tile palette");
    AssertEqual(GameOptionsRomData.TilePalettes.Unselected, unselected.PaletteIndex,
        "unselected option uses typed tile palette");
    AssertEqual(new SnesBgTilemapWord(packedTile).CharacterIndex, unselected.CharacterIndex,
        "options palette replacement preserves character index");
    AssertEqual(new SnesBgTilemapWord(packedTile).FlipFlags, unselected.FlipFlags,
        "options palette replacement preserves tile flips");
    AssertEqual(new SnesBgTilemapWord(packedTile).HasPriority, unselected.HasPriority,
        "options palette replacement preserves priority");

    Console.WriteLine(
        "  Options ROM data: pages, cursors, labels, toggles, spritemaps, and typed " +
        "palette replacement agree.");
}

static void VerifyControllerBindingsAndOptionsSubmenus()
{
    ControllerBindings swapped = ControllerBindings.Default.AssignAndSwap(
        action: 0,
        physicalButton: (ushort)SnesButton.R);
    AssertEqual((ushort)SnesButton.R, swapped.Shoot, "controller swap assigns requested button");
    AssertEqual((ushort)SnesButton.X, swapped.AimUp, "controller swap preserves permutation");
    ushort normalized = swapped.Normalize(
        (ushort)(SnesButton.R | SnesButton.X | SnesButton.Left | SnesButton.Start));
    AssertTrue((normalized & (ushort)SnesButton.X) != 0,
        "physical remapped Shoot becomes canonical Shoot");
    AssertTrue((normalized & (ushort)SnesButton.R) != 0,
        "displaced physical button becomes canonical Aim Up");
    AssertTrue((normalized & (ushort)(SnesButton.Left | SnesButton.Start)) ==
               (ushort)(SnesButton.Left | SnesButton.Start),
        "fixed directions and Start survive binding normalization");

    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
    // Five independent all-blank pages are enough to exercise the state machine. The
    // decompressor and page addresses remain real; visual asset fidelity is covered by the
    // production-ROM capture audits rather than embedding copyrighted menu data here.
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsRomData.Pages.Primary.Address,
        GameOptionsRomData.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsRomData.Pages.ControllerEnglish.Address,
        GameOptionsRomData.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsRomData.Pages.ControllerJapanese.Address,
        GameOptionsRomData.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsRomData.Pages.SpecialEnglish.Address,
        GameOptionsRomData.TilemapByteCount,
        0);
    WriteRepeatedCompressedStream(
        rom,
        GameOptionsRomData.Pages.SpecialJapanese.Address,
        GameOptionsRomData.TilemapByteCount,
        0);
    var bus = new SuperMetroidAddressSpace(rom);
    var options = new GameOptionsMenuState(bus);
    StepOptionsUntil(options, GameOptionsPhase.Main);

    for (int row = 0; row < 3; row++)
        PressOptions(options, SnesButton.Down);
    AssertEqual(3, options.SelectedItem, "primary options selects controller settings");
    PressOptions(options, SnesButton.A);
    StepOptionsUntil(options, GameOptionsPhase.ControllerSettings);

    PressOptions(options, SnesButton.R);
    AssertEqual((ushort)SnesButton.R, options.ControllerBindings.Shoot,
        "controller page assigns physical R to Shoot");
    AssertEqual((ushort)SnesButton.X, options.ControllerBindings.AimUp,
        "controller page swaps displaced Shoot button into Aim Up");

    for (int row = 0; row < 7; row++)
    {
        PressOptions(options, SnesButton.Down);
        if (options.Phase == GameOptionsPhase.ScrollControllerDown)
            StepOptionsUntil(options, GameOptionsPhase.ControllerSettings);
    }
    AssertEqual(7, options.SelectedItem, "controller page reaches Exit after native scroll");
    PressOptions(options, SnesButton.A);
    StepOptionsUntil(options, GameOptionsPhase.Main);

    for (int row = 0; row < 4; row++)
        PressOptions(options, SnesButton.Down);
    PressOptions(options, SnesButton.A);
    StepOptionsUntil(options, GameOptionsPhase.SpecialSettings);
    PressOptions(options, SnesButton.A);
    AssertTrue(options.IconCancelEnabled, "special page toggles Icon Cancel");
    PressOptions(options, SnesButton.Down);
    PressOptions(options, SnesButton.Right);
    AssertTrue(options.MoonwalkEnabled, "special page toggles Moonwalk with Right");

    var saveRam = new SuperMetroidSaveRam(bus);
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

static void VerifyReserveAutoRecovery()
{
    var samus = new SamusState
    {
        Health = 0,
        MaxHealth = 99,
        ReserveEnergy = 2,
        MaxReserveEnergy = 99,
        ReserveTankMode = 1,
    };
    var reserve = new SamusReserveAutoRecoveryState();
    reserve.Begin(samus);
    AssertTrue(samus.InputLocked, "reserve command $1B locks Samus");

    SamusReserveAutoRecoveryStep first = reserve.StepAfterNmi(samus, 8);
    AssertEqual(1, samus.Health, "first reserve frame restores one energy");
    AssertEqual(1, samus.ReserveEnergy, "first reserve frame consumes one reserve");
    AssertTrue(first.RefillSoundRequested, "eighth NMI requests reserve refill sound");
    AssertTrue(!first.Completed, "nonempty reserve remains in state $1B");

    SamusReserveAutoRecoveryStep second = reserve.StepAfterNmi(samus, 9);
    AssertEqual(2, samus.Health, "second reserve frame restores one energy");
    AssertEqual(0, samus.ReserveEnergy, "second reserve frame exhausts reserve");
    AssertTrue(second.Completed && !samus.InputLocked,
        "empty reserve publishes state eight and command $10 unlocks Samus");

    samus.Health = 0;
    samus.MaxHealth = 1;
    samus.ReserveEnergy = 5;
    samus.InputLocked = false;
    reserve.Begin(samus);
    SamusReserveAutoRecoveryStep clamped = reserve.StepAfterNmi(samus, 16);
    AssertEqual(1, samus.Health, "reserve recovery clamps at maximum energy");
    AssertEqual(0, samus.ReserveEnergy, "maximum-energy branch clears remaining reserve");
    AssertTrue(clamped.Completed, "maximum-energy branch completes state $1B");

    Console.WriteLine("  Reserve tanks: lock, one-point transfer, sound cadence, exhaustion, and clamp agree.");
}

static void VerifyDoorOpeningTrajectories()
{
    // Door destination screens are multiplied by $100. Up is the only direction whose
    // IRQ endpoint is destination+$20; the other three finish on the header coordinate.
    const ushort destinationX = 0x0300;
    const ushort destinationY = 0x0200;
    const uint sourceX = 0x00a5_4000;
    const uint sourceY = 0x0073_8000;
    const uint postNudgeX = 0x0318_4000;
    const uint postNudgeY = 0x0218_8000;

    for (int direction = 0; direction < 4; direction++)
    {
        AssertEqual(
            direction >= 2,
            SuperMetroidRuntime.DoorTransitionAlignsX((byte)direction),
            $"door direction {direction} alignment axis");
        var door = new CartridgeDoorHeader(
            Pointer: 0x8000,
            DestinationRoomPointer: 0x9000,
            BitFlags: 0,
            Orientation: (byte)direction,
            PlmX: 0,
            PlmY: 0,
            DestinationScreenX: 3,
            DestinationScreenY: 2,
            SamusDistance: 0x0100,
            SetupCodePointer: 0);
        ushort finalCameraY = direction == 3
            ? (ushort)(destinationY + 0x20)
            : destinationY;
        var trajectory = DoorOpeningScrollState.Create(
            door,
            sourceX,
            sourceY,
            destinationX,
            finalCameraY,
            finalLayer2X: 0x0180,
            finalLayer2Y: 0x0140,
            finalSamusXFixed: postNudgeX,
            finalSamusYFixed: postNudgeY);

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
        }, trajectory.CameraX, $"door direction {direction} initial camera X");
        AssertEqual(direction switch
        {
            2 => (ushort)(destinationY - 224),
            3 => (ushort)(destinationY + 251),
            _ => destinationY,
        }, trajectory.CameraY, $"door direction {direction} initial camera Y");

        ushort previousCamera = direction < 2 ? trajectory.CameraX : trajectory.CameraY;
        for (int frame = 0; frame < expectedFrames; frame++)
        {
            bool completed = trajectory.Advance();
            if (direction == 3)
            {
                AssertEqual(frame >= 3, trajectory.ShouldStreamAfterAdvance,
                    $"up-door counter {frame + 2} streaming gate");
            }
            AssertEqual(frame == expectedFrames - 1, completed,
                $"door direction {direction} completion call");
            ushort currentCamera = direction < 2 ? trajectory.CameraX : trajectory.CameraY;
            if (frame != expectedFrames - 1)
            {
                short delta = unchecked((short)(currentCamera - previousCamera));
                AssertEqual(direction is 0 or 2 ? (short)4 : (short)-4, delta,
                    $"door direction {direction} per-IRQ camera delta");
            }
            previousCamera = currentCamera;
        }

        AssertEqual(destinationX, trajectory.CameraX,
            $"door direction {direction} final camera X");
        AssertEqual(finalCameraY, trajectory.CameraY,
            $"door direction {direction} final camera Y");
        AssertEqual(0, trajectory.RemainingFrames,
            $"door direction {direction} exhausts IRQ frame count");
        AssertTrue(
            trajectory.SamusXFixed != trajectory.FinalSamusXFixed ||
            trajectory.SamusYFixed != trajectory.FinalSamusYFixed,
            $"door direction {direction} leaves $82:E6A2 nudge for its later phase");
    }

    Console.WriteLine(
        "  Doors: all four IRQ trajectories, frame counts, endpoints, and delayed nudge agree.");
}

static void VerifyCreditsObjectInterpreter()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
    WriteRepeatedCompressedStream(
        rom,
        EndingCreditsRomData.Assets.CreditsTilemap,
        EndingCreditsRomData.Rendering.CreditsSourceBytes,
        0);

    // A compact stream exercises all production control-flow opcodes: timer assignment,
    // a looping row, timer exhaustion/fallthrough, another row, and the end-credits seam.
    WriteRomWord(
        rom,
        (int)EndingCreditsRomData.Instructions.Bank.AddWithinBank(
            EndingCreditsRomData.Instructions.CreditsInitial),
        EndingCreditsRomData.Instructions.CreditsSetTimer);
    WriteRomWord(rom, 0x8cd91d, 0x0002);
    WriteRomWord(rom, 0x8cd91f, 0x0000);
    WriteRomWord(rom, 0x8cd921, 0x0000);
    WriteRomWord(rom, 0x8cd923, 0x9a0d);
    WriteRomWord(rom, 0x8cd925, 0xd91f);
    WriteRomWord(rom, 0x8cd927, 0x0000);
    WriteRomWord(rom, 0x8cd929, 0x0040);
    WriteRomWord(rom, 0x8cd92b, 0xf6fe);

    var credits = new CreditsObjectState(new SuperMetroidAddressSpace(rom));
    for (int frame = 0; frame < 15; frame++)
        AssertTrue(!credits.Step().CopiedRow, "credits waits sixteen half-pixel frames");
    CreditsObjectStepResult first = credits.Step();
    AssertTrue(first.CopiedRow, "credits copies first row at eight-pixel boundary");
    AssertEqual(1, credits.DestinationRow, "credits advances circular destination row");
    AssertEqual((ushort)2, credits.InstructionTimer, "credits timer is assigned before first row");

    for (int frame = 0; frame < 16; frame++)
        credits.Step();
    AssertEqual(2, credits.DestinationRow, "credits loop copies the repeated source row");
    AssertEqual((ushort)1, credits.InstructionTimer, "first decrement retains loop");

    for (int frame = 0; frame < 16; frame++)
        credits.Step();
    AssertEqual(3, credits.DestinationRow, "expired timer falls through to next row");
    AssertEqual((ushort)0, credits.InstructionTimer, "credits loop timer exhausts exactly");

    CreditsObjectStepResult ending = default;
    for (int frame = 0; frame < 16; frame++)
        ending = credits.Step();
    AssertTrue(ending.Finished && !credits.Enabled,
        "end-credits opcode disables the row object at its next boundary");

    Console.WriteLine(
        "  Credits: half-pixel scroll, circular rows, timer loop, fallthrough, and end opcode agree.");
}

static void VerifyEndingCreditsState()
{
    string romPath = Path.GetFullPath("Super Metroid.smc");
    if (!File.Exists(romPath))
    {
        Console.WriteLine("  Ending: retail-ROM state-$27 smoke test skipped (ROM not present).");
        return;
    }

    byte[] rom = File.ReadAllBytes(romPath);
    if (rom.Length != SuperMetroidAddressSpace.RetailRomByteCount)
    {
        throw new InvalidDataException(
            $"Ending smoke-test ROM is ${rom.Length:X} bytes; expected headerless retail size " +
            $"${SuperMetroidAddressSpace.RetailRomByteCount:X}.");
    }

    var bus = new SuperMetroidAddressSpace(rom);
    var audio = new SuperMetroid.Core.Audio.CartridgeAudioState();
    var ending = new EndingCreditsState(bus, audio, gameTimeHours: 2, gameTimeMinutes: 59);
    EndingCreditsPhase previous = ending.Phase;
    var reached = new HashSet<EndingCreditsPhase> { previous };
    int renderedTransitions = 0;

    for (int frame = 0; frame < 60_000 && ending.Phase != EndingCreditsPhase.SeeYouNextMission; frame++)
    {
        ending.Step();
        audio.AdvanceFrame(bus, default);
        if (ending.Phase != previous)
        {
            previous = ending.Phase;
            reached.Add(previous);
            Rgba32[] pixels = ending.Render();
            AssertEqual(FrontendFrame.Width * FrontendFrame.Height, pixels.Length,
                $"ending {previous} frame size");
            renderedTransitions++;
        }
    }

    AssertEqual(EndingCreditsPhase.SeeYouNextMission, ending.Phase,
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

    var middle = new EndingCreditsState(bus, audio, gameTimeHours: 3, gameTimeMinutes: 0);
    var slow = new EndingCreditsState(bus, audio, gameTimeHours: 10, gameTimeMinutes: 0);
    AssertEqual(EndingReward.Suitless, ending.EndingReward, "under-three-hour ending branch");
    AssertEqual(EndingReward.Helmetless, middle.EndingReward, "three-to-ten-hour ending branch");
    AssertEqual(EndingReward.Armored, slow.EndingReward, "ten-hour ending branch");

    var percentageTilemap = new ushort[0x400];
    Array.Fill(percentageTilemap, (ushort)0x007f);
    var percentage = new EndingBackgroundTextState(
        bus,
        percentageTilemap,
        instructionPointer: 0xdfdb,
        new EndingInventorySnapshot(
            MaxHealth: 1499,
            MaxReserveEnergy: 400,
            MaxMissiles: 230,
            MaxSuperMissiles: 50,
            MaxPowerBombs: 50,
            CollectedItems: 0xf32f,
            CollectedBeams: 0x100f),
        japaneseText: false);
    var percentageVram = new SnesVram();
    for (int frame = 0; frame < 4_000 && !percentage.Completed; frame++)
        percentage.Step(percentageVram);
    AssertTrue(percentage.Completed && percentage.RequestedItemPercentageScroll,
        "item-percentage object reaches its native scroll handoff");
    AssertEqual((ushort)0x3861, percentageTilemap[462], "100% hundreds top tile");
    AssertEqual((ushort)0x3860, percentageTilemap[463], "100% tens top tile");
    AssertEqual((ushort)0x3860, percentageTilemap[464], "100% units top tile");
    AssertEqual((ushort)0x386a, percentageTilemap[465], "100% percent-sign top tile");

    Console.WriteLine(
        "  Ending: escape, explosion, clear time, credits, rewards, 100% count, and final hold agree.");
}

private static void PressOptions(GameOptionsMenuState options, SnesButton button)
{
    options.Step((ushort)button);
    options.Step(0);
}

private static void StepOptionsUntil(
    GameOptionsMenuState options,
    GameOptionsPhase expected,
    int maximumFrames = 80)
{
    for (int frame = 0; frame < maximumFrames && options.Phase != expected; frame++)
        options.Step(0);
    AssertEqual(expected, options.Phase, $"options reaches {expected}");
}
}
