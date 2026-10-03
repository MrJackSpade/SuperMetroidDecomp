using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySamusChargeColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.SamusChargeColorExtractor.Extract(rom);
        var native = SamusChargeColorCatalog.Load(new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        forbidden.UnionWith(VerifySamusChargeColorPhases(rom));
        for (int frame = 0; frame < SamusChargeColorFormat.HyperFrameCount; frame++)
        {
            ushort offset = (ushort)(SamusChargePalettePointerDefinitions.LastHyperTableByteOffset -
                frame * sizeof(ushort));
            ushort pointer = BlockWord(SamusProjectileRomData.Palettes.HyperBeamShotPointers + offset);
            AssertTrue(SamusChargePalettePointerDefinitions.TryHyperShot(offset, out ushort compiledPointer),
                "all native Hyper-shot entries are compiled");
            AssertEqual(pointer, compiledPointer, $"Hyper-shot frame {frame} pointer");
            for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            {
                int address = SamusProjectileRomData.Banks.PaletteAndTrailData |
                    (pointer + color * sizeof(ushort));
                AssertEqual(BlockWord(address), native.ResolveHyper(frame, color),
                    $"Hyper-shot frame {frame} color {color}");
            }
        }

        SamusChargeColorDocument document = JsonSerializer.Deserialize<SamusChargeColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, SamusChargeColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock charge colors are null.");
        Paint(document.ChargedBeam[0][0]);
        Paint(document.PseudoScrew[0][0]);
        Paint(document.HyperShot[0]);
        string replacement = Path.Combine(overrideDirectory, SamusChargeColorFormat.FileName);
        File.WriteAllBytes(replacement, SamusChargeColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "charge colors change installed content identity");
        var guarded = new ChargeColorReadGuard(rom, forbidden);
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            EquippedBeams = (ushort)SamusBeamFlags.Charge,
            ChargeColors = edited.SamusChargeColors,
        };
        var projectiles = new SamusProjectileSystem();
        typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.FlareCounter))!
            .SetValue(projectiles, (ushort)60);
        var cgram = new SnesCgram();
        ushort[] suits = [0, 1, 0x20];
        for (int family = 0; family < 2; family++)
        for (int suit = 0; suit < suits.Length; suit++)
        {
            samus.EquippedItems = suits[suit];
            samus.HorizontalSpeed.ContactDamageIndex = family == 0 ? (ushort)0 : (ushort)4;
            for (int phase = 0; phase < SamusChargeColorFormat.PhasesPerSuit; phase++)
            {
                var step = projectiles.UpdateBeamChargePalette(guarded, cgram, samus);
                AssertEqual(family == 0 ? SamusBeamChargePaletteAction.ChargeCycle :
                    SamusBeamChargePaletteAction.PseudoScrewCycle, step.Action,
                    $"installed family {family} suit {suit} phase {phase}");
                AssertEqual(phase, step.ChargePaletteIndex,
                    $"installed charge phase {family}/{suit}/{phase}");
                for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
                    AssertEqual(edited.SamusChargeColors.ResolveCharge(family != 0, suit, phase, color),
                        cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                        $"edited charge CGRAM {family}/{suit}/{phase}/{color}");
            }
        }
        AssertEqual(0, projectiles.SamusChargePaletteIndex, "six charge phases wrap");
        samus.HyperBeam = 0x8000;
        typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.ChargedShotGlowTimer))!
            .SetValue(projectiles, (ushort)0x8014);
        for (int call = 0; call < 20; call++)
        {
            ushort[] before = cgram.Colors.ToArray();
            var step = projectiles.UpdateBeamChargePalette(guarded, cgram, samus);
            if ((call & 1) != 0)
            {
                AssertEqual(SamusBeamChargePaletteAction.HyperHold, step.Action,
                    $"Hyper-shot call {call} holds");
                AssertSequenceEqual(before, cgram.Colors.ToArray(),
                    $"Hyper-shot call {call} does not repaint");
                continue;
            }
            int frame = call / 2;
            AssertEqual(SamusBeamChargePaletteAction.HyperPalette, step.Action,
                $"Hyper-shot call {call} paints");
            AssertEqual(frame, step.HyperPaletteIndex, $"Hyper-shot playback frame {frame}");
            for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
                AssertEqual(edited.SamusChargeColors.ResolveHyper(frame, color),
                    cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                    $"edited Hyper-shot CGRAM {frame}/{color}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed charge and Hyper-shot avoid extracted source reads");
        PaletteRgb5 previous = document.HyperShot[0][1];
        document.HyperShot[0][1] = previous with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => SamusChargeColorCatalog.Write(document),
            "charge colors reject invalid RGB5 channels");

        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "charge-color-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog repaired = AreaMapPresentationCatalog.Load(replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, repaired.ContentIdentity,
            "stock re-extraction preserves charge color override");
        AssertTrue(overrideBeforeRepair.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)),
            "stock re-extraction does not rewrite charge colors");
        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing charge override restores stock identity");
        Console.WriteLine("Samus charge colors: 736 ROM words, 36 charge phases, 20 Hyper calls, edited CGRAM, source-read guard and stock repair pass.");

        ushort BlockWord(int address)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            return RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
        }
        static void Paint(PaletteRgb5[] colors)
        {
            PaletteRgb5 existing = colors[1];
            colors[1] = existing with { Blue = existing.Blue == 31 ? 30 : existing.Blue + 1 };
        }
    }

    private static HashSet<int> VerifySamusChargeColorPhases(ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.SamusChargeColorExtractor.Extract(rom);
        var catalog = SamusChargeColorCatalog.Load(new MemoryStream(extracted));
        var forbidden = new HashSet<int>();
        var expected = new ushort[2, 3, 6, 16];
        var cgram = new SnesCgram();
        for (int family = 0; family < 2; family++)
        {
            object input = typeof(SamusChargeColorCatalog).GetField(family == 0 ? "chargedBeam" : "pseudoScrew",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(catalog)!;
            var stored = (Dictionary<int, ushort>)input.GetType().GetField("colors",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(input)!;
            AssertEqual(family == 0 ? 48 : 96, stored.Count, "Charge catalog stores each native phase input once");
            for (int suit = 0; suit < 3; suit++)
            {
                int table = family == 0 ? SamusProjectileRomData.Palettes.BeamChargePointers : SamusProjectileRomData.Palettes.PseudoScrewPointers;
                ushort list = Word(table + suit * 2);
                for (int phase = 0; phase < 6; phase++)
                {
                    ushort pointer = Word(0x910000 | (list + phase * 2));
                    int firstPhase = 0;
                    while (Word(0x910000 | (list + firstPhase * 2)) != pointer) firstPhase++;
                    AssertEqual(firstPhase, SamusChargeColorFormat.CanonicalPhase(family != 0, phase), "Canonical phase matches first native pointer identity");
                    bool found = family == 0
                        ? SamusChargePalettePointerDefinitions.TryChargedBeam((ushort)(suit * 2), (ushort)(phase * 2), out ushort compiled)
                        : SamusChargePalettePointerDefinitions.TryPseudoScrew((ushort)(suit * 2), (ushort)(phase * 2), out compiled);
                    AssertTrue(found, "Native charge pointer supported");
                    AssertEqual(pointer, compiled, "Native charge pointer selection");
                    for (int index = 0; index < 256; index++) cgram.SetColor(index, 0x1234);
                    catalog.ApplyCharge(cgram, family != 0, suit, phase);
                    for (int color = 0; color < 16; color++)
                    {
                        ushort native = Word(0x9b0000 | (pointer + color * 2));
                        expected[family, suit, phase, color] = native;
                        AssertEqual(native, catalog.ResolveCharge(family != 0, suit, phase, color), "Native charge color");
                        AssertEqual(family == 0 ? phase == 0 : phase == firstPhase, stored.ContainsKey((suit * 6 + phase) * 16 + color), "Only canonical phase owns stock input");
                    }
                    for (int index = 0; index < 256; index++)
                        AssertEqual(index is >= 192 and < 208 ? expected[family, suit, phase, index - 192] : (ushort)0x1234,
                            cgram.Colors[index], "Charge copy changes exactly its sixteen native colors");
                }
            }
        }
        var deathSuited = new ushort[3][][];
        var deathSuitless = new ushort[10][];
        for (int suit = 0; suit < 3; suit++)
        {
            deathSuited[suit] = new ushort[10][];
            for (int palette = 0; palette < 10; palette++)
            {
                ushort pointer = Word(SamusPaletteRomData.Death.SuitPointers + (suit * 10 + palette) * 2);
                deathSuited[suit][palette] = Enumerable.Range(0, 16).Select(color => Word(0x9b0000 | (pointer + color * 2))).ToArray();
            }
        }
        for (int palette = 0; palette < 10; palette++)
        {
            ushort pointer = Word(SamusPaletteRomData.Death.SuitlessPointers + palette * 2);
            deathSuitless[palette] = Enumerable.Range(0, 16).Select(color => Word(0x9b0000 | (pointer + color * 2))).ToArray();
        }
        var whiteout = Enumerable.Range(0, SamusPaletteRomData.Death.WhiteoutShadeCount)
            .Select(index => Word(SamusPaletteRomData.Death.WhiteoutShades + index * 2)).ToArray();
        var selectors = Enumerable.Range(0, SamusDeathExplosionTimingDefinitions.RecordCount)
            .Select(index => (ushort)rom.ReadByte(SamusPaletteRomData.Death.ExplosionTimingAndPaletteIndices + 2 * index + 1)).ToArray();
        var death = new SamusDeathPaletteArtworkCatalog(deathSuited, deathSuitless, whiteout, selectors);
        foreach (string fieldName in new[] { "suitedFadeInputs", "suitlessFadeInputs" })
        {
            var inputs = (Dictionary<int, LoadingPaletteInputView.Channels>)typeof(SamusDeathPaletteArtworkCatalog)
                .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(death)!;
            AssertEqual(0, inputs.Count, "Original death shades need no correction inputs");
        }
        foreach (var (fieldName, count) in new[] { ("suited", 49), ("suitless", 17), ("explosionPaletteIndices", 0) })
        {
            var inputs = (Dictionary<int, ushort>)typeof(SamusDeathPaletteArtworkCatalog)
                .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(death)!;
            AssertEqual(count, inputs.Count, "Only independent death rows remain stored");
        }
        object whiteoutInput = typeof(SamusDeathPaletteArtworkCatalog).GetField("whiteout",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(death)!;
        var whiteoutOverrides = (Dictionary<int, LoadingPaletteInputView.Channels>)whiteoutInput.GetType().GetField("inputs",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(whiteoutInput)!;
        AssertEqual(0, whiteoutOverrides.Count, "Native whiteout has no interpolation or neutral-channel corrections");
        for (int index = 0; index < whiteout.Length; index++)
            AssertEqual(whiteout[index], death.WhiteoutColor(index), "Every original whiteout RGB word");
        for (int first = 0; first < 32; first++)
        for (int last = 0; last < 32; last++)
        foreach (int steps in new[] { 6, 13 })
        for (int step = 0; step <= steps; step++)
        {
            // Exact decimal quotient avoids a replacement integer-division oracle.
            int expectedIntensity = (int)decimal.Floor(((decimal)first * (steps - step) + (decimal)last * step) / steps);
            AssertEqual(expectedIntensity, SamusDeathPaletteArtworkCatalog.InterpolateWhiteoutIntensity(first, last, step, steps),
                "All endpoint intensities and whiteout interpolation steps");
        }
        foreach (int invalid in new[] { -1, 22, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => death.WhiteoutColor(invalid), "Invalid whiteout selector");
        foreach (int invalid in new[] { -1, 32, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusDeathPaletteArtworkCatalog.InterpolateWhiteoutIntensity(invalid, 0, 0, 6), "Invalid first intensity");
            AssertThrows<ArgumentOutOfRangeException>(() => SamusDeathPaletteArtworkCatalog.InterpolateWhiteoutIntensity(0, invalid, 0, 6), "Invalid last intensity");
        }
        foreach (int steps in new[] { 6, 13 })
        foreach (int invalid in new[] { -1, steps + 1, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusDeathPaletteArtworkCatalog.InterpolateWhiteoutIntensity(0, 31, invalid, steps), "Invalid interpolation position");
        foreach (int invalid in new[] { -1, 0, 7, 14, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusDeathPaletteArtworkCatalog.InterpolateWhiteoutIntensity(0, 31, 0, invalid), "Unsupported whiteout span");
        for (int scope = 0; scope < 4; scope++)
        {
            ushort[][][] suited = deathSuited.Select(rows => rows.Select(row => (ushort[])row.Clone()).ToArray()).ToArray();
            ushort[][] suitless = deathSuitless.Select(row => (ushort[])row.Clone()).ToArray();
            for (int family = 0; family < 4; family++)
            for (int palette = 0; palette < 10; palette++)
            for (int color = 0; color < 16; color++)
            {
                ushort[][] rows = family < 3 ? suited[family] : suitless;
                if (scope == 0 || scope == 1 && palette != 0 || scope == 2 && palette == 0) continue;
                rows[palette][color] = (ushort)((rows[palette][color] + 37 * (1 + family + palette + color)) & 0x7fff);
            }
            if (scope == 3)
            {
                // Only shared color roots change: all other supplied words stay original.
                suited = deathSuited.Select(rows => rows.Select(row => (ushort[])row.Clone()).ToArray()).ToArray();
                suitless = deathSuitless.Select(row => (ushort[])row.Clone()).ToArray();
                suited[0][1][0] ^= 0x421;
                suitless[9][0] ^= 0x421;
            }
            ushort[] editedSelectors = (ushort[])selectors.Clone();
            if (scope != 0)
                for (int frame = 0; frame < editedSelectors.Length; frame++)
                    editedSelectors[frame] = (ushort)((editedSelectors[frame] + scope) % 10);
            ushort[] editedWhiteout = (ushort[])whiteout.Clone();
            for (int index = 0; index < editedWhiteout.Length; index++)
            {
                bool anchor = index is 0 or 6 or 7 or 8 or 21;
                if (scope == 0 || scope == 2 && !anchor || scope == 3 && anchor) continue;
                editedWhiteout[index] = (ushort)((editedWhiteout[index] + 137 * (index + 1)) & 0x7fff);
            }
            var editedDeath = new SamusDeathPaletteArtworkCatalog(suited, suitless, editedWhiteout, editedSelectors);
            for (int index = 0; index < editedWhiteout.Length; index++)
                AssertEqual(editedWhiteout[index], editedDeath.WhiteoutColor(index), "Independent whiteout and endpoint-only edits");
            for (int frame = 0; frame < editedSelectors.Length; frame++)
                AssertEqual(editedSelectors[frame], editedDeath.ExplosionPaletteIndex(frame), "Original and independently edited death selectors");
            for (int family = 0; family < 4; family++)
            for (int palette = 0; palette < 10; palette++)
            for (int color = 0; color < 16; color++)
                AssertEqual((family < 3 ? suited[family] : suitless)[palette][color],
                    family < 3 ? editedDeath.SuitedColor(family, palette, color) : editedDeath.SuitlessColor(palette, color),
                    "Original death palette view and independent edits");
            string originalIdentity = SelectedPresentationHash.Create(nameof(SamusDeathPaletteArtworkCatalog), content =>
            {
                foreach (ushort[][] rows in suited)
                foreach (ushort[] row in rows) content.AppendWords("suited row", row);
                foreach (ushort[] row in suitless) content.AppendWords("suitless row", row);
                content.AppendWords("whiteout", editedWhiteout);
                content.AppendWords("explosion palette indices", editedSelectors);
            });
            AssertEqual(originalIdentity, editedDeath.ContentIdentity, "Death color identity preserves original row serialization");
            ushort before = editedDeath.SuitedColor(0, 0, 0);
            suited[0][0][0] ^= 1;
            AssertEqual(before, editedDeath.SuitedColor(0, 0, 0), "Death catalog copies inputs instead of retaining caller arrays");
        }
        for (int rgb = 0; rgb <= 0x7fff; rgb++)
        for (int shade = 0; shade < 8; shade++)
        {
            ushort actual = SamusPaletteFade.EighthTowardWhite((ushort)rgb, shade);
            for (int channel = 0; channel < 3; channel++)
            {
                int basis = rgb >> (channel * 5) & 31;
                int interpolated = (int)decimal.Floor(basis + (31 - basis) * (shade / 8m));
                AssertEqual(interpolated, actual >> (channel * 5) & 31, "Complete RGB5 eighth whitening arithmetic");
            }
        }
        foreach (int invalid in new[] { -1, 8, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusPaletteFade.EighthTowardWhite(0, invalid), "Invalid fade shade");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusPaletteFade.EighthTowardWhite(invalid, 0), "Invalid fade RGB5");
        foreach (int invalid in new[] { -1, 9, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => death.ExplosionPaletteIndex(invalid), "Invalid explosion frame");
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => death.SuitedColor(0, invalid, 0), "Invalid suited death palette");
            AssertThrows<IndexOutOfRangeException>(() => death.SuitlessColor(invalid, 0), "Invalid suitless death palette");
        }
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => death.SuitedColor(invalid, 0, 0), "Invalid death suit");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => death.SuitedColor(0, 0, invalid), "Invalid suited color");
            AssertThrows<IndexOutOfRangeException>(() => death.SuitlessColor(0, invalid), "Invalid suitless color");
        }
        // Distinct edits, source-only edits and repeated-phase-only edits must remain independent.
        for (int editScope = 0; editScope < 3; editScope++)
        {
            var document = JsonSerializer.Deserialize<SamusChargeColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
            for (int family = 0; family < 2; family++)
            for (int suit = 0; suit < 3; suit++)
            for (int phase = 0; phase < 6; phase++)
            for (int color = 0; color < 16; color++)
            {
                bool isSource = family == 0 ? phase < 4 : phase is 0 or 3;
                if (editScope == 1 && !isSource || editScope == 2 && isSource) continue;
                var rows = family == 0 ? document.ChargedBeam : document.PseudoScrew;
                ushort value = expected[family, suit, phase, color];
                int delta = 1 + (suit * 6 + phase) % 31;
                rows[suit][phase][color] = new PaletteRgb5 { Red = ((value & 31) + delta) % 32,
                    Green = ((value >> 5 & 31) + delta) % 32, Blue = ((value >> 10 & 31) + delta) % 32 };
            }
            var edited = SamusChargeColorCatalog.Load(new MemoryStream(SamusChargeColorCatalog.Write(document)));
            for (int family = 0; family < 2; family++)
            for (int suit = 0; suit < 3; suit++)
            for (int phase = 0; phase < 6; phase++)
            for (int color = 0; color < 16; color++)
            {
                PaletteRgb5 rgb = (family == 0 ? document.ChargedBeam : document.PseudoScrew)[suit][phase][color];
                AssertEqual((ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10), edited.ResolveCharge(family != 0, suit, phase, color), "Independent charge phase edit");
            }
        }
        foreach (bool pseudo in new[] { false, true })
        foreach (int invalid in new[] { -1, 6, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusChargeColorFormat.CanonicalPhase(pseudo, invalid), "Invalid phase rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.ResolveCharge(pseudo, 0, invalid, 0), "Invalid charge phase rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.ApplyCharge(cgram, pseudo, 0, invalid), "Invalid apply phase rejected");
        }
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.ResolveCharge(false, invalid, 0, 0), "Invalid suit rejected");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.ResolveCharge(false, 0, 0, invalid), "Invalid color rejected");
        var guarded = new ChargeColorReadGuard(rom, forbidden);
        var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose,
            EquippedBeams = (ushort)SamusBeamFlags.Charge, ChargeColors = catalog };
        var projectiles = new SamusProjectileSystem();
        typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.FlareCounter))!.SetValue(projectiles, (ushort)60);
        ushort[] suits = [0, 1, 0x20];
        for (int family = 0; family < 2; family++)
        for (int suit = 0; suit < 3; suit++)
        {
            samus.EquippedItems = suits[suit];
            samus.HorizontalSpeed.ContactDamageIndex = family == 0 ? (ushort)0 : (ushort)4;
            for (int phase = 0; phase < 6; phase++)
            {
                var step = projectiles.UpdateBeamChargePalette(guarded, cgram, samus);
                AssertEqual(phase, step.ChargePaletteIndex, "Native charge phase order");
                AssertEqual(family == 0 ? SamusBeamChargePaletteAction.ChargeCycle : SamusBeamChargePaletteAction.PseudoScrewCycle,
                    step.Action, "Native charge family action");
                for (int color = 0; color < 16; color++)
                    AssertEqual(expected[family, suit, phase, color], cgram.Colors[192 + color], "Guarded runtime charge color");
            }
        }
        AssertEqual(0, projectiles.SamusChargePaletteIndex, "Charge phase wraps after six calls");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "Runtime charge uses no palette ROM reads");
        Console.WriteLine("Charge/death palettes: 1216 original colors, shared fade arithmetic, edits, identity, bounds and36 guarded charge calls pass.");
        return forbidden;

        ushort Word(int address)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            return ReadVerificationWord(rom, address);
        }
    }
    private sealed class ChargeColorReadGuard(ISnesAddressSpace inner, HashSet<int> forbidden)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Charge palette reread ${address:X6}.");
            }
            return inner.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
