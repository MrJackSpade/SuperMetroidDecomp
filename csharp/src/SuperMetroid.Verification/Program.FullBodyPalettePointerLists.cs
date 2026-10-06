using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyFullBodyPalettePointerLists()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guarded = new FullBodyPalettePointerReadGuard(rom);
        SamusFullBodyCycleColorCatalog cycleColors = SamusFullBodyCycleColorCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.SamusFullBodyCycleColorExtractor.Extract(rom)));
        ushort[] equipment =
        [
            0,
            (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)SamusEquipmentFlags.GravitySuit,
            (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        (string Name, int Top, ushort FirstList, ushort FirstPalette, int Phases, bool PingPong)[] families =
        [
            ("Screw Attack", SamusPaletteRomData.FullBodyCycles.ScrewAttackLists, 0xda50, 0x9ca0, 6, true),
            ("Speed Booster", SamusPaletteRomData.FullBodyCycles.SpeedBoosterLists, 0xdaaf, 0x9b20, 4, false),
            ("stored shine", SamusPaletteRomData.FullBodyCycles.StoredShineLists, 0xdb16, 0x9ba0, 6, true),
            ("active shinespark", SamusPaletteRomData.FullBodyCycles.ActiveShinesparkLists, 0xdb7b, 0x9c20, 4, false),
        ];

        foreach (var family in families)
        {
            for (int suit = 0; suit < 3; suit++)
            {
                ushort suitOffset = (ushort)(suit * 2);
                ushort expectedList = (ushort)(family.FirstList + suit * family.Phases * 2);
                AssertEqual(expectedList, ReadPalettePointerWord(rom, family.Top + suitOffset),
                    $"{family.Name} top-level list for suit {suit}");
                for (int phase = 0; phase < family.Phases; phase++)
                {
                    int shade = family.PingPong ? Math.Min(phase, 6 - phase) : phase;
                    ushort expectedPalette = (ushort)(family.FirstPalette + suit * 0x200 + shade * 0x20);
                    AssertEqual(expectedPalette,
                        ReadPalettePointerWord(rom, SamusPaletteRomData.Banks.Movement | (expectedList + phase * 2)),
                        $"{family.Name} nested pointer for suit {suit}, phase {phase}");
                }
            }
        }

        foreach (ushort items in equipment)
        {
            int suit = items.HasAny(SamusEquipmentFlags.GravitySuit) ? 2 :
                items.HasAny(SamusEquipmentFlags.VariaSuit) ? 1 : 0;
            for (int phase = 0; phase < 6; phase++)
            {
                var speed = new SamusHorizontalSpeedState { SpecialPaletteFrame = (ushort)(phase * 2) };
                var actual = new SnesCgram();
                AssertTrue(speed.UpdateSpeedBoosterPalette(
                    guarded, actual, SamusMovementType.SpinJumping, 1,
                    (ushort)(items | (ushort)SamusEquipmentFlags.ScrewAttack),
                    cycleColors: cycleColors),
                    "Screw Attack copies a palette without reading either pointer table");
                int shade = Math.Min(phase, 6 - phase);
                AssertPaletteColors(rom, actual, 0x9ca0 + suit * 0x200 + shade * 0x20,
                    $"Screw Attack suit {suit}, phase {phase}");
                AssertEqual((ushort)(phase == 5 ? 0 : (phase + 1) * 2), speed.SpecialPaletteFrame,
                    "Screw Attack palette phase wraps after its sixth frame");
            }

            for (int phase = 0; phase < 4; phase++)
            {
                var speed = new SamusHorizontalSpeedState
                {
                    SpeedBoostCounter = 0x0400,
                    SpecialPaletteTimer = 1,
                    SpecialPaletteFrame = (ushort)(phase * 2),
                };
                var actual = new SnesCgram();
                AssertTrue(speed.UpdateSpeedBoosterPalette(
                    guarded, actual, SamusMovementType.Running, 0, items,
                    cycleColors: cycleColors),
                    "active Speed Booster copies a palette without reading either pointer table");
                AssertPaletteColors(rom, actual, 0x9b20 + suit * 0x200 + phase * 0x20,
                    $"active Speed Booster suit {suit}, phase {phase}");
                AssertEqual((ushort)(phase == 3 ? 6 : (phase + 1) * 2), speed.SpecialPaletteFrame,
                    "active Speed Booster palette pins its fourth frame");
            }

            var stored = new SamusState { EquippedItems = items };
            AssertTrue(stored.Shinespark.TryStoreFromSpeedBooster(0x0400), "seed stored shine");
            for (int phase = 0; phase < 6; phase++)
            {
                var actual = new SnesCgram();
                AssertTrue(stored.Shinespark.UpdatePalette(guarded, actual, items,
                    cycleColors: cycleColors),
                    "stored shine copies a palette without reading either pointer table");
                int shade = Math.Min(phase, 6 - phase);
                AssertPaletteColors(rom, actual, 0x9ba0 + suit * 0x200 + shade * 0x20,
                    $"stored shine suit {suit}, phase {phase}");
                AssertEqual((ushort)(phase == 5 ? 0 : (phase + 1) * 2),
                    stored.Shinespark.PaletteFrameOffset, "stored shine cycles six phases");
            }

            var active = new SamusState { EquippedItems = items };
            AssertTrue(active.Shinespark.TryStoreFromSpeedBooster(0x0400), "seed active shine");
            active.Shinespark.BeginWindup(active);
            for (int phase = 0; phase < 4; phase++)
            {
                var actual = new SnesCgram();
                AssertTrue(active.Shinespark.UpdatePalette(guarded, actual, items,
                    cycleColors: cycleColors),
                    "active shinespark copies a palette without reading either pointer table");
                AssertPaletteColors(rom, actual, 0x9c20 + suit * 0x200 + phase * 0x20,
                    $"active shinespark suit {suit}, phase {phase}");
                AssertEqual((ushort)(phase == 3 ? 0 : (phase + 1) * 2),
                    active.Shinespark.PaletteFrameOffset, "active shinespark cycles four phases");
            }
        }

        AssertTrue(!SamusPaletteRomData.FullBodyCycles.TryScrewAttackPalettePointer(0, 12, out _),
            "Screw Attack does not catalogue a seventh phase");
        AssertTrue(!SamusPaletteRomData.FullBodyCycles.TryActiveSpeedBoosterPalettePointer(0, 8, out _),
            "active Speed Booster does not catalogue a fifth phase");
        AssertTrue(!SamusPaletteRomData.FullBodyCycles.TryStoredShinePalettePointer(1, 0, out _),
            "stored shine does not catalogue an unaligned suit offset");
        AssertTrue(!SamusPaletteRomData.FullBodyCycles.TryActiveShinesparkPalettePointer(0, 1, out _),
            "active shinespark does not catalogue an unaligned phase offset");

        var adjacentScrew = new SamusHorizontalSpeedState { SpecialPaletteFrame = 12 };
        var adjacentScrewColors = new SnesCgram();
        AssertTrue(adjacentScrew.UpdateSpeedBoosterPalette(
            rom, adjacentScrewColors, SamusMovementType.SpinJumping, 1,
            (ushort)SamusEquipmentFlags.ScrewAttack, cycleColors: cycleColors),
            "non-catalog Screw Attack phase retains native adjacent-data read");
        AssertPaletteColors(rom, adjacentScrewColors, 0x9ea0,
            "Screw Attack phase twelve reads the adjacent Varia list");

        var adjacentBoost = new SamusHorizontalSpeedState
        {
            SpeedBoostCounter = 0x0400,
            SpecialPaletteTimer = 1,
            SpecialPaletteFrame = 8,
        };
        var adjacentBoostColors = new SnesCgram();
        AssertTrue(adjacentBoost.UpdateSpeedBoosterPalette(
            rom, adjacentBoostColors, SamusMovementType.Running, 0, 0,
            cycleColors: cycleColors),
            "non-catalog Speed Booster phase retains native adjacent-data read");
        AssertPaletteColors(rom, adjacentBoostColors, 0x9d20,
            "Speed Booster phase eight reads the adjacent Varia list");

        VerifyCompiledWindow(0xda50, 6,
            SamusPaletteRomData.FullBodyCycles.ReadScrewAttackPalettePointer,
            "Screw Attack");
        VerifyCompiledWindow(0xdaaf, 4,
            SamusPaletteRomData.FullBodyCycles.ReadActiveSpeedBoosterPalettePointer,
            "active Speed Booster");
        AssertThrows<InvalidDataException>(() =>
            SamusPaletteRomData.FullBodyCycles.ReadScrewAttackPalettePointer(4, 12),
            "a Screw Attack read beyond the compiled three-suit window fails explicitly");
        AssertThrows<InvalidDataException>(() =>
            SamusPaletteRomData.FullBodyCycles.ReadActiveSpeedBoosterPalettePointer(4, 12),
            "a Speed Booster phase beyond both proven Screw Attack overruns fails explicitly");

        Console.WriteLine("Full-body palettes: all four suit-indexed list families match ROM and live CGRAM without pointer-table reads.");

        void VerifyCompiledWindow(int firstList, int phases,
            Func<ushort, ushort, ushort> readCompiled, string family)
        {
            int totalBytes = 3 * phases * sizeof(ushort);
            for (ushort suitOffset = 0; suitOffset <= 4; suitOffset += 2)
            {
                int listStart = firstList + suitOffset / 2 * phases * sizeof(ushort);
                for (int phaseOffset = 0; listStart + phaseOffset + 1 < firstList + totalBytes;
                     phaseOffset++)
                    AssertEqual(ReadPalettePointerWord(rom,
                            SamusPaletteRomData.Banks.Movement | (listStart + phaseOffset)),
                        readCompiled(suitOffset, (ushort)phaseOffset),
                        $"{family} suit offset {suitOffset}, byte offset {phaseOffset}");
            }
        }
    }

    private static ushort ReadPalettePointerWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static void AssertPaletteColors(
        ISnesAddressSpace rom, SnesCgram actual, int pointer, string context)
    {
        for (int color = 0; color < SamusPaletteRomData.Common.ColorsPerObjPalette; color++)
            AssertEqual((ushort)(RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(rom),
                    SamusPaletteRomData.Banks.Palette | (pointer + color * sizeof(ushort))) & 0x7fff),
                actual.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                $"{context} CGRAM color {color} matches ROM");
    }

    private sealed class FullBodyPalettePointerReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if ((uint)(address - SamusPaletteRomData.FullBodyCycles.ScrewAttackLists) < 0x2a ||
                (uint)(address - SamusPaletteRomData.FullBodyCycles.SpeedBoosterLists) < 0x1e ||
                (uint)(address - SamusPaletteRomData.FullBodyCycles.StoredShineLists) < 0x2a ||
                (uint)(address - SamusPaletteRomData.FullBodyCycles.ActiveShinesparkLists) < 0x1e)
                throw new InvalidOperationException($"Runtime read of compiled full-body palette pointer ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
