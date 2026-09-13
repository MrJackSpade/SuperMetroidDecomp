using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyCompiledPauseEquipmentRules(ISnesAddressSpace bus, AreaMapPresentationCatalog catalog)
    {
        var guard = new PauseRulesReadGuard(bus);
        for (int category = 1; category <= 3; category++)
        {
            var definition = PauseEquipmentCategories.Definitions[category];
            ushort[] masks = Enumerable.Range(0, definition.ItemCount)
                .Select(item => RomDataReader.ReadWordFixedBank(bus, definition.BitmaskTableAddress + item * 2)).ToArray();
            for (int item = 0; item < masks.Length; item++)
            {
                AssertEqual(masks[item], PauseEquipmentRules.Mask(category, item), "compiled pause mask matches native ordered table");
                for (int bits = 0; bits <= ushort.MaxValue; bits++)
                    AssertEqual(bits & masks[item], bits & PauseEquipmentRules.Mask(category, item), "all inventory words retain native equipment eligibility");
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
            AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentRules.Mask(category, -1), "negative item index rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentRules.Mask(category, definition.ItemCount), "out-of-category index rejected");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentRules.Mask(0, 0), "reserve subdispatcher is not an equipment bit table");
        ushort[] wireframeMasks = Enumerable.Range(0, 4).Select(index => RomDataReader.ReadWordFixedBank(bus,
            PauseMenuRomData.EquipmentSetTable + index * 2)).ToArray();
        for (int word = 0; word <= ushort.MaxValue; word++)
            AssertEqual(Array.IndexOf(wireframeMasks, (ushort)(word & 0x0101)), PauseEquipmentRules.WireframeIndex((ushort)word),
                "all equipped words preserve native wireframe selection, including ignored Gravity bit");
        foreach (ushort items in wireframeMasks.SelectMany(mask => new[] { mask, (ushort)(mask | (ushort)SamusEquipmentFlags.GravitySuit) }))
        {
            var pause = Create(new SamusState { EquippedItems = items, CollectedItems = items }); EnterEquipment(pause);
            int variant = Array.IndexOf(wireframeMasks, (ushort)(items & 0x0101));
            int pointer = RomDataReader.ReadWordFixedBank(bus, PauseMenuRomData.EquipmentTilemapPatchPointerTable + variant * 2);
            var memory = pause.CaptureRenderSnapshot().Memory;
            for (int row = 0; row < 17; row++)
            for (int column = 0; column < 8; column++)
                AssertEqual(RomDataReader.ReadWordFixedBank(bus, 0x820000 | (pointer + (row * 8 + column) * 2)),
                    BinaryPrimitives.ReadUInt16LittleEndian(memory.Vram.Slice(PauseMenuLayout.Bg1TilemapWord * 2 + 472 + row * 64 + column * 2)),
                    "selected native wireframe artwork reaches the actual equipment tilemap");
        }
        ushort nativeAmount = RomDataReader.ReadWordFixedBank(bus, PauseReserveTransferRomData.TransferAmount);
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
        Console.WriteLine("Compiled pause rules: 14 native masks/all inventory words, 104 subsets, actual toggles, 65,536 wireframe selectors/rendered patches and manual transfer frames pass with rule ROM reads blocked.");
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

    private sealed class PauseRulesReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if ((uint)(address - PauseMenuRomData.EquipmentSetTable) < 8 ||
                (uint)(address - PauseReserveTransferRomData.TransferAmount) < 2 ||
                PauseEquipmentCategories.Definitions.Any(category => category.ItemCount != 0 &&
                    (uint)(address - category.BitmaskTableAddress) < category.ItemCount * 2))
                throw new InvalidOperationException($"Pause read compiled inventory/transfer rule at {address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
