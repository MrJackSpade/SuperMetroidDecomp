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
        AssertEqual(25, stored.Count, "Full-body shades leave25 whole-word base/transparent inputs");
        int endpointComponents = 0;
        foreach (var field in typeof(SamusFullBodyCycleColorCatalog).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            if (field.FieldType == typeof(LoadingPaletteInputView.Channels))
                foreach (var channel in field.FieldType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                    if (channel.GetValue(field.GetValue(native)) is not null) endpointComponents++;
        AssertEqual(16, endpointComponents, "Full-body shade endpoints store sixteen independent channels");
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
        var speedBrightWords = new HashSet<int>();
        foreach (int pointer in new[] { 0x9b60, 0x9b80 })
        foreach (int color in new[] { 1, 2, 10, 11, 12 }) speedBrightWords.Add(pointer + 2 * color);
        foreach (int pointer in new[] { 0x9d60, 0x9d80 }) speedBrightWords.Add(pointer + 24);
        foreach (int pointer in new[] { 0x9f60, 0x9f80 }) speedBrightWords.Add(pointer + 4);
        AssertEqual(14, speedBrightWords.Count, "Original Speed Booster endpoint brightening domain");
        var activeTintWords = new HashSet<int>();
        foreach (int pointer in new[] { 0x9c40, 0x9c60, 0x9c80 })
        foreach (int color in new[] { 3, 4, 5, 6, 7, 8, 13, 14, 15 })
            if (pointer != 0x9c40 || color != 8) activeTintWords.Add(pointer + 2 * color);
        foreach (int pointer in new[] { 0x9e40, 0x9e60, 0x9e80 }) activeTintWords.Add(pointer + 18);
        foreach (int pointer in new[] { 0xa040, 0xa060, 0xa080 })
        foreach (int color in new[] { 1, 2, 9, 10, 11, 12 }) activeTintWords.Add(pointer + 2 * color);
        AssertEqual(47, activeTintWords.Count, "Original active-shinespark warm tint domain");
        var activeGoldWords = new HashSet<int>();
        foreach (int pointer in new[] { 0x9c40, 0x9c60, 0x9c80 })
        foreach (int color in new[] { 1, 2, 9, 10, 11, 12 }) activeGoldWords.Add(pointer + 2 * color);
        foreach (int pointer in new[] { 0x9e40, 0x9e60, 0x9e80 })
        foreach (int color in new[] { 1, 2, 10, 11, 12 }) activeGoldWords.Add(pointer + 2 * color);
        AssertEqual(33, activeGoldWords.Count, "Original active gold-ramp domain");
        var screwTintWords = new HashSet<int>();
        foreach (int pointer in new[] { 0x9cc0, 0x9ce0, 0x9d00 })
        foreach (int color in new[] { 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15 })
            screwTintWords.Add(pointer + 2 * color);
        foreach (int pointer in new[] { 0x9ec0, 0x9ee0, 0x9f00 })
        foreach (int color in new[] { 10, 11, 12 }) screwTintWords.Add(pointer + 2 * color);
        foreach (int pointer in new[] { 0xa0c0, 0xa0e0, 0xa100 })
        foreach (int color in new[] { 1, 2, 10, 11 }) screwTintWords.Add(pointer + 2 * color);
        AssertEqual(60, screwTintWords.Count, "Original Screw Attack tint domain");
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
                bool speedBright = speedBrightWords.Contains(pointer + 2 * color);
                ushort dim = ReadVerificationWord(rom, 0x9b0000 | (0x9b40 + paletteIndex / 16 * 512 + 2 * color));
                AssertEqual(speedBright, SamusFullBodyCycleColorFormat.TrySpeedBoosterBrightening(paletteIndex, color, dim, out ushort brightened), "Every Speed Booster brightening domain member");
                if (speedBright) AssertEqual(expected, brightened, "Every original Speed Booster endpoint brightening word");
                int sharedAddress = (pointer + 2 * color) switch
                {
                    0x9b52 => 0x9b32,
                    0x9b54 or 0x9b56 => 0x9b44,
                    0x9b72 => 0x9b92,
                    0x9d96 => 0x9d94,
                    _ => -1,
                };
                int sharedIndex = SamusFullBodyCycleColorFormat.SpeedSharedChannelSource(paletteIndex, color);
                AssertEqual(sharedAddress < 0 ? -1 : (sharedAddress - 0x9b20) / 2, sharedIndex, "Every original shared-channel source identity");
                if (sharedAddress >= 0)
                    AssertEqual(expected, SamusFullBodyCycleColorFormat.SpeedSharedChannelColor(paletteIndex, color, speedBasis,
                        ReadVerificationWord(rom, 0x9b0000 | sharedAddress)), "Every native shared-channel color");
                bool activeTint = activeTintWords.Contains(pointer + 2 * color);
                AssertEqual(activeTint, SamusFullBodyCycleColorFormat.IsActiveShineTint(paletteIndex, color), "Every active-shinespark tint domain member");
                if (activeTint)
                    AssertEqual(expected, SamusFullBodyCycleColorFormat.ActiveShineTint(
                        ReadVerificationWord(rom, 0x9b0000 | (0x9c20 + paletteIndex / 16 * 512 + 2 * color)), paletteIndex % 4), "Every original active-shinespark tint word");
                bool activeGold = activeGoldWords.Contains(pointer + 2 * color);
                ushort goldBase = ReadVerificationWord(rom, 0x9b0000 | (0x9c20 + paletteIndex / 16 * 512 + 2 * color));
                AssertEqual(activeGold, SamusFullBodyCycleColorFormat.TryActiveGoldRamp(paletteIndex, color, goldBase, out ushort gold), "Every active gold-ramp domain member");
                if (activeGold) AssertEqual(color == 2 ? (ushort)(expected & 1023) : expected, color == 2 ? (ushort)(gold & 1023) : gold, "Every original active gold-ramp channel");
                bool screwTint = screwTintWords.Contains(pointer + 2 * color);
                ushort screwBase = ReadVerificationWord(rom, 0x9b0000 | (0x9ca0 + paletteIndex / 16 * 512 + 2 * color));
                AssertEqual(screwTint, SamusFullBodyCycleColorFormat.TryScrewAttackTint(paletteIndex, color, screwBase, out ushort screw), "Every Screw Attack tint domain member");
                                if (screwTint)
                {
                    int mask = (pointer + 2 * color) switch { 0x9d14 => 0x7fe0, 0x9d1e => 0x7c1f, _ => 0x7fff };
                    AssertEqual(expected & mask, screw & mask, "Every original calculated Screw Attack channel");
                }
                bool variaScrew = pointer + 2 * color is 0x9ec4 or 0x9ee4 or 0x9f04;
                AssertEqual(variaScrew, SamusFullBodyCycleColorFormat.TryVariaScrewInk(paletteIndex, color,
                    ReadVerificationWord(rom, 0x9b9ea4), ReadVerificationWord(rom, 0x9b9f04), out ushort variaInk), "Every Varia Screw slot2 member");
                if (variaScrew) AssertEqual(expected, variaInk, "Original Varia Screw endpoint/midpoint colors");
                bool powerInk = pointer + 2 * color is 0x9cc4 or 0x9ce4 or 0x9d04 or 0x9cd8 or 0x9cf8 or 0x9d18;
                AssertEqual(powerInk, SamusFullBodyCycleColorFormat.TryScrewPowerInk(paletteIndex, color,
                    ReadVerificationWord(rom, 0x9b9ca0 + 2 * color), ReadVerificationWord(rom, 0x9b9b58), out ushort ink), "Every Power Screw ink domain member");
                if (powerInk) AssertEqual(expected, ink, "Every original Power Screw ink word");
                bool endpoint = pointer + 2 * color is 0x9b42 or 0x9b44 or 0x9b92 or 0x9b58 or 0x9d94 or 0x9d58 or 0x9f44 or 0x9c64 or 0x9c84 or 0x9e84 or 0x9c50 or 0x9d14 or 0x9d1e or 0x9f04;
                bool gravityShared = pointer + 2 * color is 0x9f22 or 0x9f38;
                if (gravityShared)
                    AssertEqual(expected, SamusFullBodyCycleColorFormat.GravitySharedBase(
                        ReadVerificationWord(rom, 0x9b9b20 + 2 * color), ReadVerificationWord(rom, 0x9b9b40 + 2 * color)), "Original Gravity base shared-channel pairs");
                AssertEqual(pointer == sourcePointer && !storedShine && !speedTint && !speedBright && sharedAddress < 0 && !activeTint && !activeGold && !screwTint && !powerInk && !endpoint && !gravityShared && !variaScrew,
                    stored.ContainsKey(paletteIndex * 16 + color), "Only source inputs remain in stock storage");
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
            AssertTrue(!SamusFullBodyCycleColorFormat.TrySpeedBoosterBrightening(invalid, 1, 0, out rejected) && rejected == 0, "Speed brightening palette bounds");
            AssertEqual(-1, SamusFullBodyCycleColorFormat.SpeedSharedChannelSource(invalid, 9), "Shared-channel palette bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.IsActiveShineTint(invalid, 3), "Active tint palette bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryActiveGoldRamp(invalid, 1, 0, out rejected) && rejected == 0, "Gold-ramp palette bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryScrewAttackTint(invalid, 1, 0, out rejected) && rejected == 0, "Screw tint palette bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryScrewPowerInk(invalid, 2, 0, 0, out rejected) && rejected == 0, "Power Screw ink palette bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryVariaScrewInk(invalid, 2, 0, 0, out rejected) && rejected == 0, "Varia Screw palette bounds");
        }
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusFullBodyCycleColorFormat.CanonicalColorIndex(0, invalid), "Alias color-index bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TrySpeedBoosterTint(1, invalid, 0, out ushort rejected) && rejected == 0, "Speed tint color bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TrySpeedBoosterBrightening(2, invalid, 0, out rejected) && rejected == 0, "Speed brightening color bounds");
            AssertEqual(-1, SamusFullBodyCycleColorFormat.SpeedSharedChannelSource(1, invalid), "Shared-channel color bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.IsActiveShineTint(9, invalid), "Active tint color bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryActiveGoldRamp(9, invalid, 0, out rejected) && rejected == 0, "Gold-ramp color bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryScrewAttackTint(13, invalid, 0, out rejected) && rejected == 0, "Screw tint color bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryScrewPowerInk(13, invalid, 0, 0, out rejected) && rejected == 0, "Power Screw ink color bounds");
            AssertTrue(!SamusFullBodyCycleColorFormat.TryVariaScrewInk(29, invalid, 0, 0, out rejected) && rejected == 0, "Varia Screw color bounds");
        }
        for (int basis = 0; basis < 32768; basis++)
        for (int shade = 0; shade < 4; shade++)
        {
            if (shade == 0)
                for (int baseBlue = 0; baseBlue < 32; baseBlue++)
                    AssertEqual((ushort)((basis % 1024) + 1024 * baseBlue), SamusFullBodyCycleColorFormat.GravitySharedBase(
                        (ushort)((baseBlue << 10) | (1023 - (basis & 1023))), (ushort)basis), "Complete independent Gravity base channel domain");
            int expected = 0;
            for (int shift = 0; shift <= 10; shift += 5)
            {
                int component = basis >> shift & 31;
                int blended = (int)Math.Floor(component + (31 - component) * (shade / 4.0));
                expected |= blended << shift;
            }
            AssertEqual((ushort)expected, SamusFullBodyCycleColorFormat.StoredShineColor((ushort)basis, shade), "Complete RGB5 quarter-white blend domain");
            int warm = new[] { 0, 10, 16, 26 }[shade], blue = new[] { 0, 5, 0, 10 }[shade];
            int activeExpected = Math.Clamp((basis & 31) + warm, 0, 31) +
                32 * Math.Clamp((basis >> 5 & 31) + warm, 0, 31) +
                1024 * Math.Clamp((basis >> 10) + blue, 0, 31);
            AssertEqual((ushort)activeExpected, SamusFullBodyCycleColorFormat.ActiveShineTint((ushort)basis, shade), "Complete RGB5 active warm tint domain");
            if (shade > 0)
            {
                for (int brightRed = 0; brightRed < 32; brightRed++)
                {
                    AssertTrue(SamusFullBodyCycleColorFormat.TryVariaScrewInk(28 + shade, 2, (ushort)basis, (ushort)brightRed, out ushort varia), "Varia Screw interpolation selected");
                    int red = shade == 1 ? basis & 31 : shade == 3 ? brightRed : (int)Math.Ceiling(((basis & 31) + brightRed) / 2.0);
                    int expectedVaria = red + 32 * Math.Clamp((basis >> 5 & 31) + 5, 0, 31) + (basis & 31744);
                    AssertEqual((ushort)expectedVaria, varia, "Complete Varia Screw basis/red endpoint domain");
                }
                AssertTrue(SamusFullBodyCycleColorFormat.TryScrewPowerInk(12 + shade, 2, (ushort)basis, 0, out ushort descending), "Descending-red ink selected");
                int descendExpected = Math.Clamp((basis & 31) - 5 * shade, 0, 31) +
                    32 * Math.Clamp((basis >> 5 & 31) + 5 * shade, 0, 31) + (basis & 31744);
                AssertEqual((ushort)descendExpected, descending, "Full RGB5 descending red/ascending green domain");
                for (int baseBlue = 0; baseBlue < 32; baseBlue++)
                {
                    AssertTrue(SamusFullBodyCycleColorFormat.TryScrewPowerInk(12 + shade, 12, (ushort)(baseBlue << 10), (ushort)basis, out ushort sharedInk), "Shared gold ink selected");
                    int sharedExpected = (basis & 31) + 32 * Math.Clamp((basis >> 5 & 31) + 10 * shade, 0, 31) +
                        1024 * Math.Clamp(baseBlue + (shade == 3 ? 10 : 0), 0, 31);
                    AssertEqual((ushort)sharedExpected, sharedInk, "Shared source RGB5 and blue saturation boundaries");
                }
                foreach (int slot in new[] { 1, 3, 4 })
                {
                    int green = Math.Clamp((basis >> 5 & 31) + (slot != 1 ? 10 : 5) * shade, 0, 31);
                    int blueChannel = Math.Clamp((basis >> 10) + (slot == 4 ? (int)Math.Floor(10 * Math.Pow(2, shade - 3)) : slot == 3 && shade == 3 ? 10 : 0), 0, 31);
                    AssertTrue(SamusFullBodyCycleColorFormat.TryScrewAttackTint(12 + shade, slot, (ushort)basis, out ushort screw), "Screw tint operation selected");
                    AssertEqual((ushort)((basis & 31) + 32 * green + 1024 * blueChannel), screw, "Complete RGB5 Screw tint/saturation domain");
                }
                AssertTrue(SamusFullBodyCycleColorFormat.TryActiveGoldRamp(8 + shade, 1, (ushort)basis, out ushort quarter), "Quarter gold-ramp selected");
                AssertEqual((ushort)((expected & 1023) | (basis & 31744)), quarter, "Gold quarter ramp preserves blue over RGB5 domain");
                foreach (var (palette, slot, step) in new[] { (8 + shade, 10, 3), (24 + shade, 1, 5), (24 + shade, 10, 5), (8 + shade, 2, 5), (24 + shade, 2, 5) })
                {
                    int linear = Math.Clamp((basis & 31) + (palette >= 24 && slot == 10 ? 0 : step * shade), 0, 31) +
                        32 * Math.Clamp((basis >> 5 & 31) + step * shade, 0, 31) + (basis & 31744);
                    AssertTrue(SamusFullBodyCycleColorFormat.TryActiveGoldRamp(palette, slot, (ushort)basis, out ushort result), "Linear gold-ramp selected");
                    AssertEqual((ushort)linear, result, "Gold linear ramp saturates and preserves blue over RGB5 domain");
                }
            }
        }
        foreach (int invalid in new[] { -1, 4, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusFullBodyCycleColorFormat.StoredShineColor(0, invalid), "Stored-shine shade bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusFullBodyCycleColorFormat.ActiveShineTint(0, invalid), "Active-shine shade bounds");
        }
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
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        for (int suit = 0; suit < 3; suit++)
        for (int color = 0; color < 16; color++)
        {
            int word = 5000 + suit * 16 + color;
            document.SpeedBooster[suit][1][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
        {
            int suit = (pointer - 0x9b20) / 0x200;
            ushort expected = pointer == 0x9b40 + suit * 0x200 ? (ushort)(5000 + suit * 16 + color) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
            AssertEqual(expected, edited.Resolve(pointer, color), "Dim-only edits preserve supplied brighter shades");
        }
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        document.SpeedBooster[0][3][9] = new PaletteRgb5 { Red = 1, Green = 2, Blue = 3 };
        document.SpeedBooster[1][3][10] = new PaletteRgb5 { Red = 4, Green = 5, Blue = 6 };
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
        {
            ushort expected = (pointer, color) switch
            {
                (0x9b80, 9) => 1 | 2 << 5 | 3 << 10,
                (0x9d80, 10) => 4 | 5 << 5 | 6 << 10,
                _ => ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color)),
            };
            AssertEqual(expected, edited.Resolve(pointer, color), "Shared blue source-only edits preserve supplied dependent colors");
        }
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        for (int suit = 0; suit < 3; suit++)
        for (int color = 0; color < 16; color++)
        {
            int word = 6000 + suit * 16 + color;
            document.ActiveShinespark[suit][0][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
        {
            int suit = (pointer - 0x9b20) / 0x200;
            ushort expected = pointer == 0x9c20 + suit * 0x200 ? (ushort)(6000 + suit * 16 + color) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
            AssertEqual(expected, edited.Resolve(pointer, color), "Active base-only edits preserve supplied tinted shades");
        }
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        for (int suit = 0; suit < 3; suit++)
        for (int color = 0; color < 16; color++)
        {
            int word = 7000 + suit * 16 + color;
            document.ScrewAttack[suit][0][color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
        {
            int suit = (pointer - 0x9b20) / 0x200;
            ushort expected = pointer == 0x9ca0 + suit * 0x200 ? (ushort)(7000 + suit * 16 + color) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
            AssertEqual(expected, edited.Resolve(pointer, color), "Screw base-only edits preserve supplied tinted shades");
        }
        document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
        document.ScrewAttack[1][3][2] = new PaletteRgb5 { Red = 3, Green = 4, Blue = 5 };
        edited = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorCatalog.Write(document)));
        foreach (ushort pointer in originalPointers)
        for (int color = 0; color < 16; color++)
            AssertEqual(pointer == 0x9f00 && color == 2 ? (ushort)(3 | 4 << 5 | 5 << 10) :
                ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color)), edited.Resolve(pointer, color),
                "Varia bright-only edit preserves supplied midpoint and other colors");
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
