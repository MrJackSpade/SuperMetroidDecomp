using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyFullBodyPaletteColorData(ISnesAddressSpace rom, byte[] extracted)
    {
        var native = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(extracted));
        var originalPointers = new SortedSet<ushort>();
        var originalBases = new Dictionary<ushort, ushort>();
        var stored = (Dictionary<int, ushort>)typeof(SamusFullBodyCycleColorCatalog)
            .GetField("colors", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(native)!;
        AssertEqual(202, stored.Count, "Full-body sharing, interpolation and base tints remove566 stored words");
        foreach (var (header, phases) in new[] { (0x91daa9, 4), (0x91da4a, 6), (0x91db10, 6), (0x91db75, 4) })
        for (int suit = 0; suit < 3; suit++)
        {
            ushort list = ReadVerificationWord(rom, header + 2 * suit);
            ushort speedList = ReadVerificationWord(rom, 0x91daa9 + 2 * suit);
            originalBases.Add(ReadVerificationWord(rom, 0x910000 | list), ReadVerificationWord(rom, 0x910000 | speedList));
            for (int phase = 0; phase < phases; phase++)
                originalPointers.Add(ReadVerificationWord(rom, 0x910000 | (list + 2 * phase)));
        }
        AssertEqual(48, originalPointers.Count, "Original dispatcher lists select48 distinct color rows");
        var speedTintWords = new HashSet<int>();
        foreach (int pointer in new[] { 0x9b40, 0x9b60, 0x9b80 })
        foreach (int color in new[] { 3, 4, 5, 6, 7, 8, 13, 14, 15 }) speedTintWords.Add(pointer + 2 * color);
        foreach (int pointer in new[] { 0x9d40, 0x9d60, 0x9d80 })
        foreach (int color in new[] { 1, 2 }) speedTintWords.Add(pointer + 2 * color);
        foreach (int pointer in new[] { 0x9d40, 0x9d60, 0x9f40, 0x9f60, 0x9f80 })
        foreach (int color in new[] { 10, 11 }) speedTintWords.Add(pointer + 2 * color);
        AssertEqual(43, speedTintWords.Count, "Original Speed Booster tint word domain");
        int ordinal = 0;
        foreach (ushort pointer in originalPointers)
        {
            AssertEqual(ordinal++, SamusFullBodyCycleColorFormat.PaletteIndex(pointer), "Original palette allocation order equals calculated index");
            var cgram = new SnesCgram();
            native.Apply(cgram, pointer);
            for (int color = 0; color < 16; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
                AssertEqual(expected, native.Resolve(pointer, color), "Every original full-body palette word");
                ushort sourcePointer = color != 0 && originalBases.TryGetValue(pointer, out ushort basePointer) ? basePointer : pointer;
                if (color == 0)
                    sourcePointer = originalPointers.First(candidate => ReadVerificationWord(rom, 0x9b0000 | candidate) == expected);
                else
                {
                    ushort powerPointer = originalPointers.ElementAt(originalPointers.ToList().IndexOf(sourcePointer) % 16);
                    if (ReadVerificationWord(rom, 0x9b0000 | (powerPointer + 2 * color)) == expected)
                        sourcePointer = powerPointer;
                }
                AssertEqual(expected, ReadVerificationWord(rom, 0x9b0000 | (sourcePointer + 2 * color)), "Native base-row equality is independent of the alias formula");
                int paletteIndex = ordinal - 1;
                int sourceIndex = originalPointers.ToList().IndexOf(sourcePointer) * 16 + color;
                AssertEqual(sourceIndex, SamusFullBodyCycleColorFormat.CanonicalColorIndex(paletteIndex, color), "Every native color alias index");
                bool storedShine = (pointer - 0x9b20) % 0x200 is >= 0xa0 and <= 0xe0 && color != 0;
                AssertEqual(storedShine, SamusFullBodyCycleColorFormat.IsStoredShineShade(paletteIndex, color), "Original stored-shine derived-row domain");
                if (storedShine)
                {
                    int shade = ((pointer - 0x9ba0) % 0x200) / 32;
                    ushort basis = ReadVerificationWord(rom, 0x9b0000 | (pointer - 32 * shade + 2 * color));
                    AssertEqual(expected, SamusFullBodyCycleColorFormat.StoredShineColor(basis, shade), "Every native stored-shine interpolation word");
                }
                bool speedTint = speedTintWords.Contains(pointer + 2 * color);
                ushort speedBasis = ReadVerificationWord(rom, 0x9b0000 | (0x9b20 + paletteIndex / 16 * 512 + 2 * color));
                AssertEqual(speedTint, SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(paletteIndex, color, speedBasis, out ushort tint), "Every Speed Booster tint domain member");
                if (speedTint) AssertEqual(expected, tint, "Every original Speed Booster base tint word");
                AssertEqual(pointer == sourcePointer && !storedShine && !speedTint, stored.ContainsKey(paletteIndex * 16 + color), "Only source inputs remain in stock storage");
                AssertEqual((ushort)(expected & 0x7fff), cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color], "Every full-body palette row reaches CGRAM");
            }
            foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => native.Resolve(pointer, invalid), "Full-body color bounds");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!originalPointers.Contains((ushort)pointer))
                AssertThrows<ArgumentOutOfRangeException>(() => native.Resolve((ushort)pointer, 0), "Complete original palette identity domain rejects gaps and outside addresses");
        foreach (int invalid in new[] { -1, 48, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusFullBodyCycleColorFormat.CanonicalColorIndex(invalid, 0), "Alias palette-index bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(invalid, 3, 0, out ushort rejected) && rejected == 0, "Speed tint palette bounds");
        }
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusFullBodyCycleColorFormat.CanonicalColorIndex(0, invalid), "Alias color-index bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(1, invalid, 0, out ushort rejected) && rejected == 0, "Speed tint color bounds");
        }
        for (int basis = 0; basis < 32768; basis++)
        for (int shade = 0; shade < 4; shade++)
        {
            int expected = 0;
            for (int shift = 0; shift <= 10; shift += 5)
            {
                int component = basis >> shift & 31;
                int blended = (int)Math.Floor(component + (31 - component) * (shade / 4.0));
                expected |= blended << shift;
            }
            AssertEqual((ushort)expected, SamusFullBodyCycleColorFormat.StoredShineColor((ushort)basis, shade), "Complete RGB5 quarter-white blend domain");
        }
        foreach (int invalid in new[] { -1, 4, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusFullBodyCycleColorFormat.StoredShineColor(0, invalid), "Stored-shine shade bounds");
        for (int invalid = 32768; invalid <= ushort.MaxValue; invalid++)
        {
            ushort value = (ushort)invalid;
            AssertThrows<ArgumentOutOfRangeException>(() => SamusFullBodyCycleColorFormat.StoredShineColor(value, 0), "Stored-shine RGB5 bounds");
        }
        var document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        ordinal = 0;
        foreach (var family in new[] { document.SpeedBooster, document.ScrewAttack, document.StoredShine, document.ActiveShinespark })
        foreach (var suit in family)
        foreach (var row in suit)
        for (int color = 0; color < 16; color++, ordinal++)
            row[color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
        var edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        ordinal = 0;
        foreach (int first in new[] { 0x9b20, 0x9ca0, 0x9ba0, 0x9c20 })
        for (int suit = 0; suit < 3; suit++)
        for (int shade = 0; shade < 4; shade++)
        for (int color = 0; color < 16; color++, ordinal++)
            AssertEqual((ushort)ordinal, edited.Resolve((ushort)(first + suit * 0x200 + shade * 32), color), "Every independent full-body color edit survives calculated placement");
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        for (int suit = 0; suit < 3; suit++)
        for (int color = 0; color < 16; color++)
        {
            int word = 2000 + suit * 16 + color;
            document.SpeedBooster[suit][0][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
        {
            int suit = (pointer - 0x9b20) / 0x200;
            ushort expected = pointer == 0x9b20 + suit * 0x200 ? (ushort)(2000 + suit * 16 + color) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
            AssertEqual(expected, edited.Resolve(pointer, color), "Changing only a shared source row preserves all supplied family colors");
        }
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        ordinal = 3000;
        foreach (var family in new[] { document.SpeedBooster, document.StoredShine, document.ActiveShinespark, document.ScrewAttack })
        foreach (var row in family[0])
        for (int color = 0; color < 16; color++, ordinal++)
            row[color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
        {
            ushort expected = pointer < 0x9d20 ? (ushort)(3000 + (pointer - 0x9b20) / 2 + color) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
            AssertEqual(expected, edited.Resolve(pointer, color), "Power-only edits preserve supplied Varia and Gravity colors");
        }
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        for (int suit = 0; suit < 3; suit++)
        for (int color = 0; color < 16; color++)
        {
            int word = 4000 + suit * 16 + color;
            document.StoredShine[suit][0][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
        {
            int suit = (pointer - 0x9b20) / 0x200;
            ushort expected = pointer == 0x9ba0 + suit * 0x200 ? (ushort)(4000 + suit * 16 + color) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
            AssertEqual(expected, edited.Resolve(pointer, color), "Stored-shine basis-only edits preserve supplied derived shades");
        }
    }
    private static void VerifySamusFullBodyCycleColorOverride(
        string stockDirectory, string overrideDirectory,
        AreaMapPresentationCatalog original, ISnesAddressSpace rom,
        GameplayBasePaletteCatalog initialPalettes,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.SamusFullBodyCycleColorExtractor.Extract(rom);
        VerifyFullBodyPaletteColorData(rom, extracted);

        SamusFullBodyCycleColorDocument document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, SamusFullBodyCycleColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock full-body cycle color JSON is null.");
        Paint(document.SpeedBooster[0][0]);
        Paint(document.ScrewAttack[1][2]);
        Paint(document.StoredShine[2][3]);
        Paint(document.ActiveShinespark[0][1]);
        string replacement = Path.Combine(overrideDirectory, SamusFullBodyCycleColorFormat.FileName);
        File.WriteAllBytes(replacement, SamusFullBodyCycleColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "full-body cycle edit changes installed-content identity");

        var guard = new FullBodyColorReadGuard(rom);
        ushort[] equipment = [0, (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)SamusEquipmentFlags.GravitySuit];
        foreach (ushort items in equipment)
        {
            int suit = items.HasAny(SamusEquipmentFlags.GravitySuit) ? 2 :
                items.HasAny(SamusEquipmentFlags.VariaSuit) ? 1 : 0;
            for (int phase = 0; phase < 4; phase++)
            {
                var speed = new SamusHorizontalSpeedState
                {
                    SpeedBoostCounter = 0x0400,
                    SpecialPaletteTimer = 1,
                    SpecialPaletteFrame = (ushort)(phase * 2),
                };
                var cgram = new SnesCgram();
                AssertTrue(speed.UpdateSpeedBoosterPalette(guard, cgram,
                        SamusMovementType.Running, 0, items,
                        cycleColors: edited.SamusFullBodyCycleColors),
                    "Speed Booster reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.SpeedBooster, suit, phase);
            }

            for (int phase = 0; phase < 6; phase++)
            {
                var screw = new SamusHorizontalSpeedState { SpecialPaletteFrame = (ushort)(phase * 2) };
                var cgram = new SnesCgram();
                AssertTrue(screw.UpdateSpeedBoosterPalette(guard, cgram,
                        SamusMovementType.SpinJumping, 0x1b,
                        (ushort)(items | (ushort)SamusEquipmentFlags.ScrewAttack),
                        cycleColors: edited.SamusFullBodyCycleColors),
                    "Screw Attack reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.ScrewAttack, suit,
                    Math.Min(phase, 6 - phase));
            }

            var stored = new SamusState { EquippedItems = items };
            AssertTrue(stored.Shinespark.TryStoreFromSpeedBooster(0x0400), "seed stored shine");
            for (int phase = 0; phase < 6; phase++)
            {
                var cgram = new SnesCgram();
                AssertTrue(stored.Shinespark.UpdatePalette(guard, cgram, items,
                        cycleColors: edited.SamusFullBodyCycleColors),
                    "stored shine reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.StoredShine, suit,
                    Math.Min(phase, 6 - phase));
            }

            var active = new SamusState { EquippedItems = items };
            AssertTrue(active.Shinespark.TryStoreFromSpeedBooster(0x0400), "seed active shinespark");
            active.Shinespark.BeginWindup(active);
            for (int phase = 0; phase < 4; phase++)
            {
                var cgram = new SnesCgram();
                AssertTrue(active.Shinespark.UpdatePalette(guard, cgram, items,
                        cycleColors: edited.SamusFullBodyCycleColors),
                    "active shinespark reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.ActiveShinespark, suit, phase);
            }

            var attached = new SamusState
            {
                EquippedItems = items,
                SpecialSuperPaletteFlags = 1,
                FullBodyCycleColors = edited.SamusFullBodyCycleColors,
            };
            var attachmentColors = new SnesCgram();
            AssertTrue(SamusSpecialSuperPalette.Update(attachmentColors, attached),
                "Metroid attachment reaches installed Speed Booster color source");
            AssertColors(attachmentColors, SamusFullBodyCycleFamily.SpeedBooster, suit, 3);
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed full-body cycles do not reread any of the 768 source color words");

        var runtime = new SuperMetroidRuntime(guard,
            initialPaletteArt: initialPalettes) { MapPresentation = edited };
        fixtureAssets.Bind(runtime);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        SamusState liveSamus = runtime.Samus ??
            throw new InvalidOperationException("Ceres entry did not initialize Samus.");
        AssertTrue(ReferenceEquals(edited.SamusFullBodyCycleColors, liveSamus.FullBodyCycleColors),
            "runtime binds edited full-body colors to live Samus");
        runtime.MapPresentation = original;
        AssertTrue(ReferenceEquals(original.SamusFullBodyCycleColors, liveSamus.FullBodyCycleColors),
            "runtime rebind replaces edited colors without changing game state");

        PaletteRgb5 originalColor = document.SpeedBooster[0][0][1];
        document.SpeedBooster[0][0][1] = originalColor with { Blue = 32 };
        byte[] malformed = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        AssertThrows<InvalidDataException>(() => SamusFullBodyCycleColorCatalog.Load(
            new MemoryStream(malformed, writable: false)),
            "full-body cycle colors reject out-of-range RGB5 values");

        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "cycle-color-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog afterRepair = AreaMapPresentationCatalog.Load(
            replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, afterRepair.ContentIdentity,
            "stock re-extraction preserves selected full-body cycle override");
        AssertTrue(overrideBeforeRepair.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)),
            "stock re-extraction never rewrites the user's cycle colors");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing cycle-color override restores stock identity");
        Console.WriteLine("Samus full-body cycles: 768 native colors, four live families/three suits, attachment flash, ROM guard, rebind, strict values and override removal pass.");

        void AssertColors(SnesCgram cgram, SamusFullBodyCycleFamily family, int suit, int shade)
        {
            ushort pointer = SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
            for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
                AssertEqual(edited.SamusFullBodyCycleColors.Resolve(pointer, color),
                    cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                    $"{family} suit {suit}, shade {shade}, color {color} reaches live CGRAM");
        }

        static void Paint(PaletteRgb5[] colors)
        {
            PaletteRgb5 originalColor = colors[1];
            colors[1] = originalColor with
            {
                Blue = originalColor.Blue == 31 ? 30 : originalColor.Blue + 1,
            };
        }
    }

    private sealed class FullBodyColorReadGuard :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        private readonly ISnesAddressSpace source;
        private readonly HashSet<int> forbidden = new();
        public int ForbiddenReadAttempts { get; private set; }

        public FullBodyColorReadGuard(ISnesAddressSpace source)
        {
            this.source = source;
            foreach (SamusFullBodyCycleFamily family in Enum.GetValues<SamusFullBodyCycleFamily>())
            for (int suit = 0; suit < SamusFullBodyCycleColorFormat.SuitCount; suit++)
            for (int shade = 0; shade < SamusFullBodyCycleColorFormat.ShadesPerSuit; shade++)
            {
                int address = SamusPaletteRomData.Banks.Palette |
                    SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
                for (int offset = 0;
                     offset < SamusFullBodyCycleColorFormat.ColorsPerPalette * sizeof(ushort);
                     offset++)
                    forbidden.Add(address + offset);
            }
        }

        private void RejectColorSource(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Production reread full-body color ${address:X6}.");
            }
        }

        public byte ReadByte(int address)
        {
            RejectColorSource(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectColorSource(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Full-body color guard source does not expose WRAM.")).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Full-body color guard source does not expose SRAM.")).ReadSaveRamByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
