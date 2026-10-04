using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyPauseBeamMasks(ISnesAddressSpace rom) => VerifyPauseMaskCases(rom, 1, 0x82c04c, 5);
    private static void VerifyPauseSuitMasks(ISnesAddressSpace rom) => VerifyPauseMaskCases(rom, 2, 0x82c056, 6);
    private static void VerifyPauseBootMasks(ISnesAddressSpace rom) => VerifyPauseMaskCases(rom, 3, 0x82c062, 3);

    private static void VerifyPauseMaskCases(ISnesAddressSpace rom, int category, int address, int count)
    {
        for (int item = 0; item < count; item++)
        {
            ushort expected = (ushort)(rom.ReadByte(address + item * 2) | rom.ReadByte(address + item * 2 + 1) << 8);
            AssertEqual(expected, PauseEquipmentRules.Mask(category, item), "original native upgrade flag case");
        }
        foreach (int item in new[] { int.MinValue, -1, count, 256, int.MaxValue }) CheckRejected(category, item, "item");
        foreach (int invalid in new[] { int.MinValue, -1, 0, 4, 256, int.MaxValue })
        {
            CheckRejected(invalid, 0, "category");
            CheckRejected(invalid, -1, "category");
        }
        static void CheckRejected(int category, int item, string parameter)
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

    private static void VerifyPauseWireframeSelection(ISnesAddressSpace rom)
    {
        static ushort Word(ISnesAddressSpace source, int address) =>
            (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);
        AssertEqual((byte)0x29, rom.ReadByte(0x82b212), "native AND immediate opcode");
        AssertEqual((byte)0xdd, rom.ReadByte(0x82b218), "native CMP absolute-X opcode");
        AssertEqual((ushort)0xb257, Word(rom, 0x82b219), "native comparison table operand");
        ushort nativeMask = Word(rom, 0x82b213);
        ushort[] original = new ushort[4];
        for (int index = 0; index < original.Length; index++) original[index] = Word(rom, 0x82b257 + index * 2);
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            int expected = Array.IndexOf(original, (ushort)(word & nativeMask));
            AssertTrue(expected >= 0, "native masked input has a table match");
            AssertEqual(expected, PauseEquipmentRules.WireframeIndex((ushort)word), "complete native wireframe selection domain");
        }
    }

    private static void VerifyCompiledPauseEquipmentRules(ISnesAddressSpace bus, AreaMapPresentationCatalog catalog)
    {
        VerifyPauseBeamMasks(bus);
        VerifyPauseSuitMasks(bus);
        VerifyPauseBootMasks(bus);
        var guard = new PauseRulesReadGuard(bus);
        for (int category = 1; category <= 3; category++)
        {
            var definition = PauseEquipmentCategories.Get(category);
            ushort[] masks = Enumerable.Range(0, definition.ItemCount)
                .Select(item => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), definition.BitmaskTableAddress + item * 2)).ToArray();
            for (int item = 0; item < masks.Length; item++)
            {
                var samus = Inventory(category, masks[item]);
                var pause = Create(samus);
                EnterEquipment(pause);
                AssertEqual((category, item), (pause.SelectedCategory, pause.SelectedItem), "single owned upgrade selects its actual native menu entry");
                pause.Step(0, (ushort)SnesButton.A);
                AssertEqual((ushort)0, category == 1 ? samus.EquippedBeams : samus.EquippedItems, "A toggles exactly the compiled upgrade bit off");
                pause.Step(0, 0); pause.Step(0, (ushort)SnesButton.A);
                AssertEqual(masks[item], category == 1 ? samus.EquippedBeams : samus.EquippedItems, "A toggles exactly the compiled upgrade bit on");
                AssertEqual(masks[item], category == 1 ? samus.CollectedBeams : samus.CollectedItems, "menu toggle cannot change collection");
            }
            for (int subset = 0; subset < 1 << masks.Length; subset++)
            {
                ushort owned = 0;
                for (int item = 0; item < masks.Length; item++) if ((subset & (1 << item)) != 0) owned |= masks[item];
                var pause = Create(Inventory(category, owned));
                int expected = Array.FindIndex(masks, mask => (mask & owned) != 0);
                AssertEqual(expected < 0 ? (0, 0) : (category, expected), (pause.SelectedCategory, pause.SelectedItem), "all category subsets retain first-owned selection");
            }
        }
        ushort[] wireframeMasks = Enumerable.Range(0, 4).Select(index => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            PauseMenuRomData.EquipmentSetTable + index * 2)).ToArray();
        VerifyPauseWireframeSelection(bus);
        foreach (ushort items in wireframeMasks.SelectMany(mask => new[] { mask, (ushort)(mask | (ushort)SamusEquipmentFlags.GravitySuit) }))
        {
            var pause = Create(new SamusState { EquippedItems = items, CollectedItems = items }); EnterEquipment(pause);
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
                MaxReserveEnergy = 100, ReserveTankMode = 2, CollectedBeams = (ushort)SamusBeamFlags.Charge };
            var pause = Create(samus); EnterEquipment(pause);
            pause.Step(0, (ushort)SnesButton.Up); pause.Step(0, (ushort)SnesButton.Down);
            AssertEqual((0, 1), (pause.SelectedCategory, pause.SelectedItem), "manual transfer fixture reaches reserve dispatcher");
            int health = scenario.Health, reserve = scenario.Reserve;
            for (int tick = 0; tick < scenario.Reserve && reserve != 0; tick++)
            {
                health += nativeAmount;
                if (health >= 99) { health = 99; reserve = 0; }
                else reserve -= nativeAmount;
                pause.Step(0, tick == 0 ? (ushort)SnesButton.A : (ushort)0);
                AssertEqual((health, reserve), ((int)samus.Health, (int)samus.ReserveEnergy), "real manual-transfer frames retain native rate and full-health reserve discard");
            }
        }
        Console.WriteLine("Compiled pause rules: 14 native masks, 104 subsets, actual toggles, 65,536 wireframe selectors/rendered patches and manual transfer frames pass with rule ROM reads blocked.");
        PauseMenuState Create(SamusState samus) => new(guard, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: catalog);
        static SamusState Inventory(int category, ushort owned) => category == 1
            ? new SamusState { CollectedBeams = owned, EquippedBeams = owned }
            : new SamusState { CollectedItems = owned, EquippedItems = owned };
        static void EnterEquipment(PauseMenuState pause)
        {
            pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
            for (int tick = 0; tick < 32; tick++) pause.Step(0, 0);
            AssertEqual(1, pause.ScreenMode, "fixture actually enters equipment screen");
        }
    }

    private sealed class PauseRulesReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if ((uint)(address - PauseMenuRomData.EquipmentSetTable) < 8 ||
                (uint)(address - PauseReserveTransferRomData.TransferAmount) < 2 ||
                Enumerable.Range(0, 4).Select(PauseEquipmentCategories.Get).Any(category => category.ItemCount != 0 &&
                    (uint)(address - category.BitmaskTableAddress) < category.ItemCount * 2))
                throw new InvalidOperationException($"Pause read compiled inventory/transfer rule at {address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
