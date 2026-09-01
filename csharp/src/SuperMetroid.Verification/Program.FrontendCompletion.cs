using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
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
    WriteRepeatedCompressedStream(rom, 0x978df4, 0x0800, 0);
    WriteRepeatedCompressedStream(rom, 0x978fcd, 0x0800, 0);
    WriteRepeatedCompressedStream(rom, 0x9791c4, 0x0800, 0);
    WriteRepeatedCompressedStream(rom, 0x97938d, 0x0800, 0);
    WriteRepeatedCompressedStream(rom, 0x97953a, 0x0800, 0);
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
