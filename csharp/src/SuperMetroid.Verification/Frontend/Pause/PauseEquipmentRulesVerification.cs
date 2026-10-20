using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks equipment-mask, wireframe, and reserve-transfer rules through the live pause menu.</summary>
internal static class PauseEquipmentRulesVerification
{
    /// <summary>Native $82:C04C/$C056/$C062 beam, suit/misc and boot upgrade-mask tables.</summary>
    private static int PauseMaskTableAddress(PauseEquipmentCategory category) => category switch
    {
        PauseEquipmentCategory.Beams => 0x82c04c,
        PauseEquipmentCategory.Suits => 0x82c056,
        PauseEquipmentCategory.Boots => 0x82c062,
        PauseEquipmentCategory.Reserves => throw new ArgumentOutOfRangeException(nameof(category)),
        _ => throw new ArgumentOutOfRangeException(nameof(category)),
    };

    /// <summary>Compares five beam upgrade masks with bank-$82 and checks invalid selectors.</summary>
    internal static void VerifyPauseBeamMasks(ISnesAddressSpace rom) => VerifyPauseMaskCases(rom, PauseEquipmentCategory.Beams, PauseMaskTableAddress(PauseEquipmentCategory.Beams), 5);
    /// <summary>Compares six suit and miscellaneous upgrade masks with bank-$82.</summary>
    internal static void VerifyPauseSuitMasks(ISnesAddressSpace rom) => VerifyPauseMaskCases(rom, PauseEquipmentCategory.Suits, PauseMaskTableAddress(PauseEquipmentCategory.Suits), 6);
    /// <summary>Compares three boot upgrade masks with bank-$82.</summary>
    internal static void VerifyPauseBootMasks(ISnesAddressSpace rom) => VerifyPauseMaskCases(rom, PauseEquipmentCategory.Boots, PauseMaskTableAddress(PauseEquipmentCategory.Boots), 3);

    /// <summary>Matches a category's native mask table and verifies category-first bounds rejection.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pause mask cases")]
    private static void VerifyPauseMaskCases(ISnesAddressSpace rom, PauseEquipmentCategory category, int address, int count)
    {
        for (int item = 0; item < count; item++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + item * 2);
            AssertEqual(expected, PauseEquipmentRules.Mask(category, item).ToNativeWord(), "original native upgrade flag case");
        }
        foreach (int item in new[] { int.MinValue, -1, count, 256, int.MaxValue }) CheckRejected(category, item, "item");
        foreach (PauseEquipmentCategory invalid in new[] { PauseEquipmentCategory.Reserves, (PauseEquipmentCategory)4, (PauseEquipmentCategory)byte.MaxValue })
        {
            CheckRejected(invalid, 0, "category");
            CheckRejected(invalid, -1, "category");
        }
        // Check that an invalid mask lookup reports the offending selector parameter.
        static void CheckRejected(PauseEquipmentCategory category, int item, string parameter)
        {
            try { _ = PauseEquipmentRules.Mask(category, item); }
            catch (ArgumentOutOfRangeException error)
            {
                AssertEqual(parameter, error.ParamName, "mask domain preserves category-first rejection");
                return;
            }
            throw new InvalidOperationException("Unsupported pause mask selector was accepted.");
        }
    }

    /// <summary>Compares the compiled suit-wireframe selector with every sixteen-bit equipped-item word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pause wireframe selection")]
    internal static void VerifyPauseWireframeSelection(ISnesAddressSpace rom)
    {
        AssertEqual((byte)0x29, rom.ReadCartridgeByte(0x82b212), "native AND immediate opcode");
        AssertEqual((byte)0xdd, rom.ReadCartridgeByte(0x82b218), "native CMP absolute-X opcode");
        AssertEqual((ushort)0xb257, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x82b219), "native comparison table operand");
        ushort nativeMask = CartridgeImportSource.Require(rom).ReadCartridgeWord(0x82b213);
        ushort[] original = new ushort[4];
        for (int index = 0; index < original.Length; index++) original[index] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x82b257 + index * 2));
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            int expected = Array.IndexOf(original, (ushort)(word & nativeMask));
            AssertTrue(expected >= 0, "native masked input has a table match");
            AssertEqual(expected, PauseEquipmentRules.WireframeIndex((SamusEquipmentFlags)((ushort)word)), "complete native wireframe selection domain");
        }
    }

    /// <summary>Checks real menu toggles, first-owned selection, rendered wireframes, Start+A ordering, and reserve transfer cadence.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause equipment set, mask and reserve-transfer tables")]
    internal static void VerifyCompiledPauseEquipmentRules(ISnesAddressSpace bus, AreaMapPresentationCatalog catalog)
    {
        Suite(nameof(VerifyPauseBeamMasks), () => VerifyPauseBeamMasks(bus));
        Suite(nameof(VerifyPauseSuitMasks), () => VerifyPauseSuitMasks(bus));
        Suite(nameof(VerifyPauseBootMasks), () => VerifyPauseBootMasks(bus));
        var guard = NewPauseRulesReadGuard(bus);
        foreach (PauseEquipmentCategory category in PauseEquipmentCategories.InventoryCategories)
        {
            var definition = PauseEquipmentCategories.Get(category);
            PauseEquipmentBit[] masks = Enumerable.Range(0, definition.ItemCount)
                .Select(item => PauseEquipmentRules.Mask(category, item)).ToArray();
            for (int item = 0; item < masks.Length; item++)
            {
                var samus = Inventory(masks[item]);
                var pause = Create(samus);
                EnterPauseEquipment(pause);
                AssertEqual((category, item), (pause.SelectedCategory, pause.SelectedItem), "single owned upgrade selects its actual native menu entry");
                pause.Step(0, SnesButton.A);
                AssertEqual(default(PauseEquipmentBit), new PauseEquipmentBit(samus.EquippedBeams, samus.EquippedItems), "A toggles exactly the compiled upgrade bit off");
                pause.Step(0, 0); pause.Step(0, SnesButton.A);
                AssertEqual(masks[item], new PauseEquipmentBit(samus.EquippedBeams, samus.EquippedItems), "A toggles exactly the compiled upgrade bit on");
                AssertEqual(masks[item], new PauseEquipmentBit(samus.CollectedBeams, samus.CollectedItems), "menu toggle cannot change collection");
            }
            for (int subset = 0; subset < 1 << masks.Length; subset++)
            {
                PauseEquipmentBit owned = default;
                for (int item = 0; item < masks.Length; item++)
                    if ((subset & (1 << item)) != 0) owned = new(owned.Beam | masks[item].Beam, owned.Item | masks[item].Item);
                var pause = Create(Inventory(owned));
                int expected = Array.FindIndex(masks, mask => mask.IsSetIn(owned.Beam, owned.Item));
                AssertEqual(expected < 0 ? (PauseEquipmentCategory.Reserves, 0) : (category, expected), (pause.SelectedCategory, pause.SelectedItem), "all category subsets retain first-owned selection");
            }
        }
        // `$82:9142` runs EquipmentScreenMain before the Start handler, so an A press in the
        // frame that unpauses still toggles the item. The 13% movie unequips the Speed Booster
        // with Start+A this way.
        PauseEquipmentBit speedBooster = PauseEquipmentRules.Mask(PauseEquipmentCategory.Boots, 2);
        var unpausing = Inventory(speedBooster);
        var startAndA = Create(unpausing);
        EnterPauseEquipment(startAndA);
        AssertEqual((PauseEquipmentCategory.Boots, 2), (startAndA.SelectedCategory, startAndA.SelectedItem), "the Speed Booster is selected");
        AssertTrue(startAndA.Step(SnesButton.Start, SnesButton.A), "Start unpauses");
        AssertEqual(SamusEquipmentFlags.None, unpausing.EquippedItems, "the same frame's A still unequips the Speed Booster");
        ushort[] wireframeMasks = Enumerable.Range(0, 4).Select(index => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            PauseMenuRomData.EquipmentSetTable + index * 2)).ToArray();
        Suite(nameof(VerifyPauseWireframeSelection), () => VerifyPauseWireframeSelection(bus));
        foreach (ushort items in wireframeMasks.SelectMany(mask => new[] { mask, (ushort)(mask | SamusEquipmentFlags.GravitySuit.ToNativeWord()) }))
        {
            var pause = Create(new SamusState { EquippedItems = (SamusEquipmentFlags)items, CollectedItems = (SamusEquipmentFlags)items }); EnterPauseEquipment(pause);
            int variant = Array.IndexOf(wireframeMasks, (ushort)(items & 0x0101));
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseMenuRomData.EquipmentTilemapPatchPointerTable + variant * 2);
            var memory = pause.CaptureRenderSnapshot().Memory;
            for (int row = 0; row < 17; row++)
            for (int column = 0; column < 8; column++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x820000 | (pointer + (row * 8 + column) * 2)),
                    BinaryPrimitives.ReadUInt16LittleEndian(memory.Vram.Slice(PauseMenuLayout.Bg1TilemapWord * 2 + 472 + row * 64 + column * 2)),
                    "selected native wireframe artwork reaches the actual equipment tilemap");
        }
        ushort nativeAmount = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), PauseReserveTransferRomData.TransferAmount);
        AssertEqual(nativeAmount, PauseEquipmentRules.ReserveEnergyPerFrame, "manual reserve rate matches native word");
        foreach (var scenario in new[] { (Health: 20, Reserve: 10), (Health: 98, Reserve: 10), (Health: 99, Reserve: 1) })
        {
            var samus = new SamusState { Health = (ushort)scenario.Health, MaxHealth = 99, ReserveEnergy = (ushort)scenario.Reserve,
                MaxReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Manual, CollectedBeams = SamusBeamFlags.Charge };
            var pause = Create(samus); EnterPauseEquipment(pause);
            pause.Step(0, SnesButton.Up); pause.Step(0, SnesButton.Down);
            AssertEqual((PauseEquipmentCategory.Reserves, 1), (pause.SelectedCategory, pause.SelectedItem), "manual transfer fixture reaches reserve dispatcher");
            int health = scenario.Health, reserve = scenario.Reserve;
            for (int tick = 0; tick < scenario.Reserve && reserve != 0; tick++)
            {
                health += nativeAmount;
                if (health >= 99) { health = 99; reserve = 0; }
                else reserve -= nativeAmount;
                pause.Step(0, tick == 0 ? SnesButton.A : 0);
                AssertEqual((health, reserve), (samus.Health, samus.ReserveEnergy), "real manual-transfer frames retain native rate and full-health reserve discard");
            }
        }
        Console.WriteLine("Compiled pause rules: 14 native masks, 104 subsets, actual toggles, 65,536 wireframe selectors/rendered patches and manual transfer frames pass with rule ROM reads blocked.");
        // Create a pause menu backed by guarded ROM reads and the supplied Samus inventory.
        PauseMenuState Create(SamusState samus) => new(new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = catalog });
        // Equip exactly the collected bits in the beam or item field for a category fixture.
        static SamusState Inventory(PauseEquipmentBit owned) =>
            new() { CollectedBeams = owned.Beam, EquippedBeams = owned.Beam, CollectedItems = owned.Item, EquippedItems = owned.Item };
    }

    /// <summary>Forbids production reads of migrated equipment masks, wireframe masks, and reserve-transfer rate.</summary>
    private static CartridgeReadGuard NewPauseRulesReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => (uint)(address - PauseMenuRomData.EquipmentSetTable) < 8 ||
                (uint)(address - PauseReserveTransferRomData.TransferAmount) < 2 ||
                Enum.GetValues<PauseEquipmentCategory>().Select(PauseEquipmentCategories.Get).Any(category => category.ItemCount != 0 &&
                    (uint)(address - PauseMaskTableAddress(category.Category)) < category.ItemCount * 2),
                address => $"Pause read compiled inventory/transfer rule at {address:X6}.");

    /// <summary>
    /// Native page-change length since #1266: fifteen fade-out steps, one load step, then
    /// fifteen brightness writes each spending CounterReload counter-only updates first.
    /// </summary>
    internal static int PausePageTransitionNativeLength => 15 + 1 + 15 * (PauseFadeTiming.CounterReload + 1);

    /// <summary>
    /// Presses a page button, then steps until the page change completes, advancing through
    /// <paramref name="step"/> (default: the pause menu alone). Page input is accepted only once the
    /// transition completes, so fixtures wait for it rather than a fixed tick count.
    /// </summary>
    internal static void ChangePausePage(PauseMenuState pause, SnesButton page, Action<SnesButton>? step = null)
    {
        step ??= input => pause.Step(input, input);
        step(page);
        int ticks = 0;
        do
        {
            step(0);
            AssertTrue(++ticks <= PausePageTransitionNativeLength, "pause page transition completes within its native length");
        }
        while (pause.IsPageTransitionActive);
    }

    /// <summary>
    /// Moves the equipment selector from its entry selection ($82:AB47 selects the first beam on
    /// every page entry, #1266) to the Boots category with real input, for the simultaneous
    /// Boots Left+A beam-selection glitches.
    /// </summary>
    internal static void SelectPauseBoots(PauseMenuState pause)
    {
        pause.Step(0, SnesButton.Right);
        for (int step = 0; pause.SelectedCategory != PauseEquipmentCategory.Boots && step < 6; step++)
            pause.Step(0, SnesButton.Down);
        AssertEqual(PauseEquipmentCategory.Boots, pause.SelectedCategory, "fixture moves the equipment selector to Boots");
    }

    /// <summary>
    /// Moves the equipment selector down to the beam category with real input. With reserve
    /// capacity, $82:ABAD-$ABB5 selects the reserve mode control on entry (#1266).
    /// </summary>
    internal static void SelectPauseBeams(PauseMenuState pause)
    {
        for (int step = 0; pause.SelectedCategory != PauseEquipmentCategory.Beams && step < 6; step++)
            pause.Step(0, SnesButton.Down);
        AssertEqual(PauseEquipmentCategory.Beams, pause.SelectedCategory, "fixture moves the equipment selector to the beams");
    }

    /// <summary>Presses R and steps until the equipment page is ready for input (#1266 cadence).</summary>
    internal static void EnterPauseEquipment(PauseMenuState pause)
    {
        ChangePausePage(pause, SnesButton.R);
        AssertEqual(1, pause.ScreenMode, "fixture actually enters equipment screen");
    }
}
