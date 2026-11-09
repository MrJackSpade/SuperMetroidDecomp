using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>Checks extracted Samus charge palettes, native phase selection, and guarded runtime color updates.</summary>
internal static class SamusChargeColorsVerification
{
    /// <summary>Checks the installed charge and Hyper colors' CGRAM output without rereading native palette words.</summary>
    internal static void VerifySamusChargeColors(string stockDirectory,
        AreaMapPresentation original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusChargeColorExtractor.Extract(rom);
        var native = SamusChargeColorFile.Read(new MemoryStream(extracted, writable: false)).ToColours();
        var forbidden = new HashSet<int>();
        forbidden.UnionWith(VerifySamusChargeColorPhases(rom));
        for (int frame = 0; frame < SamusChargeColorFormat.HyperFrameCount; frame++)
        {
            ushort offset = (ushort)((SamusChargePaletteDefinitions.HyperShotSteps - frame) * sizeof(ushort));
            ushort pointer = BlockWord(SamusSourceDefinitions.HyperBeamShotPointers.Value + offset);
            AssertTrue(SamusChargePaletteDefinitions.TryHyperShot(offset, out ushort compiledPointer),
                "all native Hyper-shot entries are compiled");
            AssertEqual(pointer, compiledPointer, $"Hyper-shot frame {frame} pointer");
            for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            {
                int address = SamusSourceDefinitions.PaletteBank.Value |
                    (pointer + color * sizeof(ushort));
                AssertEqual(BlockWord(address), native.ResolveHyper(frame, color),
                    $"Hyper-shot frame {frame} color {color}");
            }
        }

        SamusChargeColorDocument document = JsonSerializer.Deserialize<SamusChargeColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, SamusChargeColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock charge colors are null.");
        var guarded = NewChargeColorReadGuard(rom, forbidden);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            EquippedBeams = SamusBeamFlags.Charge,
        };
        // Charge and Hyper-shot steps name shades (#1512); the import resolves them from the installed charge colours.
        var installedSources = InstalledFixtures.SamusShadeSources with { Charge = ImportAreaMapPresentation.SamusColoursOf(original).Charge };
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
        projectiles.Charge.FlareCounter = 60;
        var samusColours = new SamusColours();
        ushort[] suits = [0, 1, 0x20];
        for (int family = 0; family < 2; family++)
        for (int suit = 0; suit < suits.Length; suit++)
        {
            samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(suits[suit]);
            samus.HorizontalSpeed.ContactDamageIndex = family == 0 ? SamusContactDamageIndex.None : SamusContactDamageIndex.PseudoScrew;
            for (int phase = 0; phase < SamusChargeColorFormat.PhasesPerSuit; phase++)
            {
                // The live charge-palette step is the phase this call applies.
                int appliedPhase = projectiles.Charge.ChargePaletteStep;
                var step = projectiles.UpdateBeamChargePalette(samusColours, samus);
                AssertEqual(family == 0 ? SamusBeamChargePaletteAction.ChargeCycle :
                    SamusBeamChargePaletteAction.PseudoScrewCycle, step.Action,
                    $"installed family {family} suit {suit} phase {phase}");
                AssertEqual(phase, appliedPhase,
                    $"installed charge phase {family}/{suit}/{phase}");
                for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
                    AssertEqual(ImportAreaMapPresentation.SamusColoursOf(original).Charge.ResolveCharge(family != 0, suit, phase, color),
                        samusColours.ShownSuit(installedSources)[color],
                        $"installed charge CGRAM {family}/{suit}/{phase}/{color}");
            }
        }
        AssertEqual(0, projectiles.Charge.ChargePaletteStep, "six charge phases wrap");
        samus.HyperBeamEnabled = true;
        projectiles.Charge.ChargedShotGlowTimer = WrappingCountdown.Start(0x8014);
        for (int call = 0; call < 20; call++)
        {
            PictureShade before = samusColours.Suit;
            ushort glowTimerBefore = projectiles.Charge.ChargedShotGlowTimer.RawValue;
            var step = projectiles.UpdateBeamChargePalette(samusColours, samus);
            if ((call & 1) != 0)
            {
                AssertEqual(SamusBeamChargePaletteAction.HyperHold, step.Action,
                    $"Hyper-shot call {call} holds");
                AssertEqual(before, samusColours.Suit,
                    $"Hyper-shot call {call} does not repaint");
                continue;
            }
            int frame = call / 2;
            AssertEqual(SamusBeamChargePaletteAction.HyperPalette, step.Action,
                $"Hyper-shot call {call} paints");
            // Even low-five-bit timer offsets $14..$02 select Hyper palettes 0..9.
            AssertEqual(0x14 - frame * 2, glowTimerBefore & 0x001e, $"Hyper-shot playback frame {frame}");
            for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
                AssertEqual(ImportAreaMapPresentation.SamusColoursOf(original).Charge.ResolveHyper(frame, color),
                    samusColours.ShownSuit(installedSources)[color],
                    $"installed Hyper-shot colour {frame}/{color}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed charge and Hyper-shot avoid extracted source reads");
        PaletteRgb5 previous = document.HyperShot[0][1];
        document.HyperShot[0][1] = previous with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => SamusChargeColorFile.Read(new MemoryStream(SamusChargeColorFile.Write(document))),
            "charge colors reject invalid RGB5 channels");

        Console.WriteLine("Samus charge colors: 736 ROM words, 36 charge phases, 20 Hyper calls reach CGRAM and source-read guard pass.");

        ushort BlockWord(int address)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            return RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
        }
    }

    /// <summary>Checks all suit and charge phases against ROM, including canonical shared pointers and the runtime six-step palette cadence.</summary>
    internal static HashSet<int> VerifySamusChargeColorPhases(ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusChargeColorExtractor.Extract(rom);
        var catalog = SamusChargeColorFile.Read(new MemoryStream(extracted));
        SamusChargeColours colours = catalog.ToColours();
        var catalogSources = InstalledFixtures.SamusShadeSources with { Charge = colours };
        var forbidden = new HashSet<int>();
        var expected = new ushort[2, 3, 6, 16];
        var samusColours = new SamusColours();
        for (int family = 0; family < 2; family++)
        {
            var input = family == 0 ? catalog.chargedBeam : catalog.pseudoScrew;
            var stored = input.colors;
            AssertEqual(family == 0 ? 25 : 24, stored.Count, "Charge catalog stores each native phase input once");
            var tintInputs = input.fadeInputs;
            AssertEqual(family == 0 ? 0 : 2, tintInputs.Count, "Only Power/Varia pseudo-Screw ink2 need tint input overrides");
            if (family != 0)
            {
                foreach (var entry in tintInputs)
                {
                    AssertTrue(entry.Key is 2 or 98, "Pseudo-Screw tint input belongs to Power/Varia ink2");
                    foreach (var field in MetadataShape.Fields(typeof(LoadingPaletteInputView.Channels), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                        AssertEqual(field.Name == "blue", field.GetValue(entry.Value) is not null, "Only differing blue channel remains stored");
                }
            }
            for (int suit = 0; suit < 3; suit++)
            {
                int table = family == 0 ? SamusSourceDefinitions.BeamChargePointers.Value : SamusSourceDefinitions.PseudoScrewPointers.Value;
                ushort list = Word(table + suit * 2);
                for (int phase = 0; phase < 6; phase++)
                {
                    ushort pointer = Word(0x910000 | (list + phase * 2));
                    int firstPhase = 0;
                    while (Word(0x910000 | (list + firstPhase * 2)) != pointer) firstPhase++;
                    AssertEqual(firstPhase, SamusChargeColorFormat.CanonicalPhase(family != 0, phase), "Canonical phase matches first native pointer identity");
                    bool found = family == 0
                        ? SamusChargePaletteDefinitions.TryChargedBeam((ushort)(suit * 2), (ushort)(phase * 2), out ushort compiled)
                        : SamusChargePaletteDefinitions.TryPseudoScrew((ushort)(suit * 2), (ushort)(phase * 2), out compiled);
                    AssertTrue(found, "Native charge pointer supported");
                    AssertEqual(pointer, compiled, "Native charge pointer selection");
                    samusColours.ShowSuit(SamusShades.Charge(new SamusChargePalette(
                        family != 0 ? SamusChargeKind.PseudoScrew : SamusChargeKind.ChargedBeam, SamusSuits.AtListPlace(suit), phase)));
                    RgbColor[] shownSuit = samusColours.ShownSuit(catalogSources);
                    for (int color = 0; color < 16; color++)
                    {
                        ushort native = Word(0x9b0000 | (pointer + color * 2));
                        expected[family, suit, phase, color] = native;
                        AssertEqual(native, colours.ResolveCharge(family != 0, suit, phase, color), "Native charge color");
                        AssertEqual((family == 0 ? phase == 0 : phase == 3) && (suit == 0 || native != expected[family, 0, phase, color]), stored.ContainsKey(new TableColourPlace((suit * 6 + phase), color)), "Only canonical phase owns stock input");
                    }
                    for (int color = 0; color < 16; color++)
                        AssertEqual(expected[family, suit, phase, color], shownSuit[color], "Charge shade shows its sixteen native colors");
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
                ushort pointer = Word(SamusSourceDefinitions.SuitPointers.Value + (suit * 10 + palette) * 2);
                deathSuited[suit][palette] = Enumerable.Range(0, 16).Select(color => Word(0x9b0000 | (pointer + color * 2))).ToArray();
            }
        }
        for (int palette = 0; palette < 10; palette++)
        {
            ushort pointer = Word(SamusSourceDefinitions.SuitlessPointers.Value + palette * 2);
            deathSuitless[palette] = Enumerable.Range(0, 16).Select(color => Word(0x9b0000 | (pointer + color * 2))).ToArray();
        }
        var whiteout = Enumerable.Range(0, SamusPaletteDefinitions.Death.WhiteoutShadeCount)
            .Select(index => Word(SamusSourceDefinitions.DeathWhiteoutShades.Value + index * 2)).ToArray();
        var selectors = Enumerable.Range(0, SamusDeathExplosionTimingDefinitions.RecordCount)
            .Select(index => (ushort)rom.ReadCartridgeByte(SamusSourceDefinitions.DeathExplosionTimingAndPaletteIndices.Value + 2 * index + 1)).ToArray();
        var death = new InstalledSamusDeathPalettes(ToColors(deathSuited), ToColors(deathSuitless), ToColors(whiteout), selectors);
        foreach (var inputs in new[] { death.suitedFadeInputs, death.suitlessFadeInputs })
            AssertEqual(0, inputs.Count, "Original death shades need no correction inputs");
        foreach (var (stored, count) in new[] { new VerifySamusChargeColorPhasesCase(death.suited.Count, 25), new VerifySamusChargeColorPhasesCase(death.suitless.Count, 3), new VerifySamusChargeColorPhasesCase(death.explosionPaletteIndices.Count, 0) })
            AssertEqual(count, stored, "Only independent death rows remain stored");
        var deathTintInputs = death.tintShadeInputs;
        AssertEqual(3, deathTintInputs.Count, "Two interior intensities and dark-endpoint red/blue remain supplied");
        foreach (var entry in deathTintInputs)
        {
            AssertTrue(entry.Key is 7 or 9 or 10, "Only the tint endpoint and two interior inks have inputs");
            foreach (var field in MetadataShape.Fields(typeof(LoadingPaletteInputView.Channels), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                AssertEqual(field.Name == "blue" || entry.Key == 10 && field.Name == "red",
                    field.GetValue(entry.Value) is not null, "Only independent native tint channels remain stored");
        }
        for (int brightGreen = 0; brightGreen < 32; brightGreen++)
        for (int brightBlue = 0; brightBlue < 32; brightBlue++)
        for (int darkBlue = 0; darkBlue < 32; darkBlue++)
        {
            ushort bright = (ushort)(brightGreen << 5 | brightBlue << 10);
            ushort dark = (ushort)(31 - darkBlue | darkBlue << 10);
            bool valid = InstalledSamusDeathPalettes.TrySuitlessTintEndpoint(RgbColor.FromWord(checked(bright)), RgbColor.FromWord(checked(dark)), out RgbColor result);
            int expectedGreen = brightGreen - (brightBlue - darkBlue);
            AssertEqual(expectedGreen is >= 0 and <= 31, valid, "Complete endpoint balance bounds");
            if (valid)
            {
                AssertEqual(dark & 0x7c1f, result.ToWord() & 0x7c1f, "Endpoint keeps independent red/blue");
                AssertEqual(brightGreen - brightBlue, (result.ToWord() >> 5 & 31) - (result.ToWord() >> 10), "Endpoint preserves green-blue balance");
            }
            else AssertEqual(0, result, "Unrepresentable endpoint is not clamped or wrapped");
        }
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.TrySuitlessTintEndpoint(RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0), out _), "Invalid bright tint endpoint");
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.TrySuitlessTintEndpoint(new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid)), out _), "Invalid dark tint endpoint");
        }
        var deathWarmInputs = death.warmShadeInputs;
        AssertEqual(1, deathWarmInputs.Count, "Only warm dark-endpoint components remain stored");
        foreach (var field in MetadataShape.Fields(typeof(LoadingPaletteInputView.Channels), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            AssertEqual(field.Name != "blue", field.GetValue(deathWarmInputs[4]) is not null, "Warm endpoint stores only independent red/green");
        for (int blue = 0; blue < 32; blue++)
        for (int redGreen = 0; redGreen < 1024; redGreen++)
        {
            RgbColor actual = InstalledSamusDeathPalettes.SuitlessWarmEndpoint(RgbColor.FromWord(checked((ushort)(blue << 10))), RgbColor.FromWord(checked((ushort)(redGreen | (31 - blue) << 10))));
            AssertEqual(blue, actual.ToWord() >> 10, "Warm endpoint takes bright blue");
            AssertEqual(redGreen, actual.ToWord() & 1023, "Warm endpoint preserves dark red/green");
        }
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessWarmEndpoint(RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0)), "Invalid bright warm endpoint");
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessWarmEndpoint(new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid))), "Invalid dark warm endpoint");
        }
        for (int first = 0; first < 32; first++)
        for (int last = 0; last < 32; last++)
        for (int channel = 0; channel < 3; channel++)
            AssertEqual((ushort)((int)decimal.Floor((first + last) / 2m) << (channel * 5)),
                InstalledSamusDeathPalettes.SuitlessTintMidpoint(RgbColor.FromWord(checked((ushort)(first << (channel * 5)))), RgbColor.FromWord(checked((ushort)(last << (channel * 5))))),
                "Suitless tint midpoint floors each independent RGB5 channel");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessTintMidpoint(RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0)), "Invalid midpoint first endpoint");
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessTintMidpoint(new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid))), "Invalid midpoint last endpoint");
        }
        foreach (int ink in new[] { 7, 8, 9 })
        {
            AssertTrue(!InstalledSamusDeathPalettes.TrySuitlessTintShade(RgbColor.FromWord(0x001f), RgbColor.FromWord(0x001f), 31, ink, out _), "Tint red overflow remains explicit input");
            AssertTrue(!InstalledSamusDeathPalettes.TrySuitlessTintShade(RgbColor.FromWord(0x7c00), RgbColor.FromWord(0x7c00), 0, ink, out _), "Tint red/green underflow remains explicit input");
        }
        foreach (int invalid in new[] { -1, 6, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.TrySuitlessTintShade(new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), 0, invalid, out _), "Invalid tint ink");
        foreach (int invalid in new[] { -1, 32, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.TrySuitlessTintShade(new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), invalid, 7, out _), "Invalid tint blue");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.TrySuitlessTintShade(RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0), 0, 7, out _), "Invalid tint first endpoint");
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.TrySuitlessTintShade(new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid)), 0, 7, out _), "Invalid tint last endpoint");
        }
        var neutralInputs = death.neutralInputs;
        AssertEqual(2, neutralInputs.Count, "Only gray peak and final neutral intensities remain");
        foreach (var entry in neutralInputs)
        {
            AssertTrue(entry.Key is 11 or 144, "Only native gray roots store shared intensity inputs");
            foreach (var field in MetadataShape.Fields(typeof(LoadingPaletteInputView.Channels), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                AssertEqual(field.Name == "red", field.GetValue(entry.Value) is not null, "Neutral stock inputs store one intensity only");
        }
        for (int peak = 0; peak < 32; peak++)
        for (int color = 11; color < 16; color++)
            AssertEqual((int)decimal.Ceiling(peak * ((16 - color) / 5m)),
                InstalledSamusDeathPalettes.SuitlessGrayIntensity(peak, color), "Complete suitless gray intensity domain");
        foreach (int invalid in new[] { -1, 32, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessGrayIntensity(invalid, 11), "Invalid gray peak");
        foreach (int invalid in new[] { -1, 0, 10, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessGrayIntensity(19, invalid), "Invalid gray ink");
        for (int first = 0; first < 32; first++)
        for (int last = 0; last < 32; last++)
        for (int color = 2; color <= 3; color++)
        {
            // Complemented/swapped channel pairs exercise independent RGB packing.
            ushort a = (ushort)(first | (31 - first) << 5 | last << 10);
            ushort b = (ushort)(last | (31 - last) << 5 | first << 10);
            RgbColor actual = InstalledSamusDeathPalettes.SuitlessWarmShade(RgbColor.FromWord(checked(a)), RgbColor.FromWord(checked(b)), color);
            for (int channel = 0; channel < 3; channel++)
            {
                int start = a >> (5 * channel) & 31, end = b >> (5 * channel) & 31;
                int expectedChannel = (int)decimal.Floor(((decimal)start * (4 - color) + (decimal)end * (color - 1)) / 3);
                AssertEqual(expectedChannel, actual.ToWord() >> (5 * channel) & 31, "All suitless warm-ramp channel endpoints");
            }
        }
        foreach (int invalid in new[] { -1, 0, 1, 4, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessWarmShade(new RgbColor(0, 0, 0), new RgbColor(0, 0, 0), invalid), "Invalid warm shade ink");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessWarmShade(RgbColor.FromWord(checked(invalid)), new RgbColor(0, 0, 0), 2), "Invalid warm start");
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.SuitlessWarmShade(new RgbColor(0, 0, 0), RgbColor.FromWord(checked(invalid)), 2), "Invalid warm end");
        }
        var whiteoutInput = death.whiteout;
        var whiteoutOverrides = whiteoutInput.inputs;
        AssertEqual(0, whiteoutOverrides.Count, "Native whiteout has no interpolation or neutral-channel corrections");
        for (int index = 0; index < whiteout.Length; index++)
        {
            AssertEqual(whiteout[index], death.WhiteoutColor(index), "Every original whiteout RGB word");
            AssertEqual(whiteout[index], InstalledSamusDeathPalettes.DefaultWhiteoutColor(index), "Calculated two-rate whiteout matches original ROM");
        }
        foreach (int invalid in new[] { -1, 22, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => death.WhiteoutColor(invalid), "Invalid whiteout selector");
            AssertThrows<ArgumentOutOfRangeException>(() => InstalledSamusDeathPalettes.DefaultWhiteoutColor(invalid), "Invalid calculated whiteout selector");
        }
        // The stock death palette view returns every supplied colour, whiteout shade and selector, serializes its rows
        // as they were supplied, and copies its inputs.
        {
            ushort[][][] suited = deathSuited.Select(rows => rows.Select(row => (ushort[])row.Clone()).ToArray()).ToArray();
            var stockDeath = new InstalledSamusDeathPalettes(ToColors(suited), ToColors(deathSuitless), ToColors(whiteout), selectors);
            for (int index = 0; index < whiteout.Length; index++)
                AssertEqual(whiteout[index], stockDeath.WhiteoutColor(index), "Original whiteout shades");
            for (int frame = 0; frame < selectors.Length; frame++)
                AssertEqual(selectors[frame], stockDeath.ExplosionPaletteIndex(frame), "Original death selectors");
            for (int family = 0; family < 4; family++)
            for (int palette = 0; palette < 10; palette++)
            for (int color = 0; color < 16; color++)
                AssertEqual((family < 3 ? suited[family] : deathSuitless)[palette][color],
                    family < 3 ? stockDeath.SuitedColor(family, palette, color) : stockDeath.SuitlessColor(palette, color),
                    "Original death palette view");
            string originalIdentity = SelectedPresentationHash.Create(nameof(InstalledSamusDeathPalettes), content =>
            {
                foreach (ushort[][] rows in suited)
                foreach (ushort[] row in rows) content.AppendWords("suited row", row);
                foreach (ushort[] row in deathSuitless) content.AppendWords("suitless row", row);
                content.AppendWords("whiteout", whiteout);
                content.AppendWords("explosion palette indices", selectors);
            });
            AssertEqual(originalIdentity, stockDeath.ContentIdentity, "Death color identity preserves original row serialization");
            RgbColor before = stockDeath.SuitedColor(0, 0, 0);
            suited[0][0][0] ^= 1;
            AssertEqual(before, stockDeath.SuitedColor(0, 0, 0), "Death catalog copies inputs instead of retaining caller arrays");
        }
        for (int rgb = 0; rgb <= 0x7fff; rgb++)
        for (int shade = 0; shade < 8; shade++)
        {
            RgbColor actual = SamusPaletteFade.EighthTowardWhite(RgbColor.FromWord(checked((ushort)rgb)), shade);
            for (int channel = 0; channel < 3; channel++)
            {
                int basis = rgb >> (channel * 5) & 31;
                int interpolated = (int)decimal.Floor(basis + (31 - basis) * (shade / 8m));
                AssertEqual(interpolated, actual.ToWord() >> (channel * 5) & 31, "Complete RGB5 eighth whitening arithmetic");
            }
        }
        foreach (int invalid in new[] { -1, 8, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusPaletteFade.EighthTowardWhite(new RgbColor(0, 0, 0), invalid), "Invalid fade shade");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusPaletteFade.EighthTowardWhite(RgbColor.FromWord(checked(invalid)), 0), "Invalid fade RGB5");
        foreach (int invalid in new[] { -1, 9, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => death.ExplosionPaletteIndex(invalid), "Invalid explosion frame");
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitedColor(0, invalid, 0), "Invalid suited death palette");
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitlessColor(invalid, 0), "Invalid suitless death palette");
        }
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitedColor(invalid, 0, 0), "Invalid death suit");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitedColor(0, 0, invalid), "Invalid suited color");
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitlessColor(0, invalid), "Invalid suitless color");
        }
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusChargeColorFormat.PseudoScrewBrightColor(invalid, 1, new RgbColor(0, 0, 0)), "Invalid pseudo tint suit");
        foreach (int invalid in new[] { -1, 0, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusChargeColorFormat.PseudoScrewBrightColor(0, invalid, new RgbColor(0, 0, 0)), "Invalid pseudo tint ink");
        foreach (ushort invalid in new ushort[] { 0x8000, 0xffff })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusChargeColorFormat.PseudoScrewBrightColor(0, 1, RgbColor.FromWord(checked(invalid))), "Invalid pseudo tint RGB5");
        foreach (bool pseudo in new[] { false, true })
        foreach (int invalid in new[] { -1, 6, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SamusChargeColorFormat.CanonicalPhase(pseudo, invalid), "Invalid phase rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => colours.ResolveCharge(pseudo, 0, invalid, 0), "Invalid charge phase rejected");
        }
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => colours.ResolveCharge(false, invalid, 0, 0), "Invalid suit rejected");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => colours.ResolveCharge(false, 0, 0, invalid), "Invalid color rejected");
        var guarded = NewChargeColorReadGuard(rom, forbidden);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = SamusPoseId.FacingRightNormalPose,
            EquippedBeams = SamusBeamFlags.Charge };
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
        projectiles.Charge.FlareCounter = 60;
        ushort[] suits = [0, 1, 0x20];
        for (int family = 0; family < 2; family++)
        for (int suit = 0; suit < 3; suit++)
        {
            samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(suits[suit]);
            samus.HorizontalSpeed.ContactDamageIndex = family == 0 ? SamusContactDamageIndex.None : SamusContactDamageIndex.PseudoScrew;
            for (int phase = 0; phase < 6; phase++)
            {
                int appliedPhase = projectiles.Charge.ChargePaletteStep;
                var step = projectiles.UpdateBeamChargePalette(samusColours, samus);
                AssertEqual(phase, appliedPhase, "Native charge phase order");
                AssertEqual(family == 0 ? SamusBeamChargePaletteAction.ChargeCycle : SamusBeamChargePaletteAction.PseudoScrewCycle,
                    step.Action, "Native charge family action");
                for (int color = 0; color < 16; color++)
                    AssertEqual(expected[family, suit, phase, color], samusColours.ShownSuit(catalogSources)[color], "Guarded runtime charge color");
            }
        }
        AssertEqual(0, projectiles.Charge.ChargePaletteStep, "Charge phase wraps after six calls");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "Runtime charge uses no palette ROM reads");
        Console.WriteLine("Charge/death palettes: 1216 original colors, shared fade arithmetic, identity, bounds and 36 guarded charge calls pass.");
        return forbidden;

        ushort Word(int address)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            return ReadVerificationWord(rom, address);
        }
    }
    /// <summary>Rejects runtime reads from the native palette bytes already checked against extracted charge colors.</summary>
    private static CartridgeReadGuard NewChargeColorReadGuard(ISnesAddressSpace inner, HashSet<int> forbidden) =>
        new CartridgeReadGuard(inner).Forbid(forbidden.Contains,
            address => $"Charge palette reread ${address:X6}.");
}
