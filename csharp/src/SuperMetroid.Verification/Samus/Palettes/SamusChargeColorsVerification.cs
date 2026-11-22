using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>Checks extracted Samus charge palettes, native phase selection, and guarded runtime color updates.</summary>
internal static class SamusChargeColorsVerification
{
    /// <summary>Checks the installed charge and Hyper colors' CGRAM output without rereading native palette words.</summary>
    internal static void VerifySamusChargeColors(string stockDirectory,
        InstalledScreenContent original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusChargeColorExtractor.Extract(rom);
        var native = SamusChargeColorFile.Read(new MemoryStream(extracted, writable: false)).ToColours();
        var forbidden = new HashSet<int>();
        forbidden.UnionWith(VerifySamusChargeColorPhases(rom));
        for (int frame = 0; frame < SamusChargeColorFormat.HyperFrameCount; frame++)
        {
            ushort offset = (ushort)((SamusChargeColorFormat.HyperFrameCount - frame) * sizeof(ushort));
            ushort pointer = BlockWord(SamusSourceDefinitions.HyperBeamShotPointers.Value + offset);
            AssertTrue(SamusPaletteNative.TryHyperShot(offset, out ushort compiledPointer),
                "all native Hyper-shot entries are compiled");
            AssertEqual(pointer, compiledPointer, $"Hyper-shot frame {frame} pointer");
            for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
            {
                int address = SamusSourceDefinitions.PaletteBank.Value |
                    (pointer + color * sizeof(ushort));
                AssertEqual(BlockWord(address), native.ResolveHyperStep(SuperMetroid.Core.Samus.Colours.SamusShadeDefinitions.FullBodyCycles.HyperBeamShadeCount - 1 - (frame), color),
                    $"Hyper-shot frame {frame} color {color}");
            }
        }

        SamusChargeColorDocument document = JsonSerializer.Deserialize<SamusChargeColorDocument>(File.ReadAllBytes(Path.Combine(stockDirectory, SamusChargeColorFormat.FileName)), global::SuperMetroid.Verification.VerificationJson.Info<global::SuperMetroid.AssetExtraction.Samus.SamusChargeColorDocument>(ArtworkJson.Options))
            ?? throw new InvalidDataException("Stock charge colors are null.");
        var guarded = NewChargeColorReadGuard(rom, forbidden);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            EquippedBeams = SamusBeamFlags.Charge,
        }.InPose(SamusPoseId.FacingRightNormalPose);
        // Charge and Hyper-shot steps name shades (#1512); the import resolves them from the installed charge colours.
        var installedSources = InstalledFixtures.SamusShadeSources with { Charge = InstalledFixtures.ImportOf(original).SamusColours.Charge };
        var projectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        projectiles.Charge.SetFlareCount(60);
        var samusColours = new SamusColours();
        ushort[] suits = [0, 1, 0x20];
        for (int family = 0; family < 2; family++)
        for (int suit = 0; suit < suits.Length; suit++)
        {
            samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(suits[suit]);
            samus.HorizontalSpeed.BodyAttack = family == 0 ? SamusBodyAttack.None : SamusBodyAttack.PseudoScrew;
            for (int phase = 0; phase < SamusChargeColorFormat.PhasesPerSuit; phase++)
            {
                // The live charge-palette step is the phase this call applies.
                int appliedPhase = projectiles.Charge.ChargeGlowStep;
                AssertTrue(!projectiles.UpdateBeamChargeGlow(samusColours, samus, global::SuperMetroid.Core.Fx.RoomVisorAnimation.Steady),
                    $"installed family {family} suit {suit} phase {phase} restores no suit");
                AssertEqual(phase, appliedPhase,
                    $"installed charge phase {family}/{suit}/{phase}");
                for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
                    AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.Charge.ResolveCharge(family != 0, suit, phase, color),
                        samusColours.ShownSuit(installedSources)[color],
                        $"installed charge CGRAM {family}/{suit}/{phase}/{color}");
            }
        }
        AssertEqual(0, projectiles.Charge.ChargeGlowStep, "six charge phases wrap");
        samus.EnableHyperBeam();
        projectiles.Charge.ChargedShotGlow = ChargedShotGlow.StartHyperBeamGlow();
        for (int call = 0; call < 20; call++)
        {
            PictureShade before = samusColours.Suit;
            int glowStepsBefore = SuperMetroid.TestSupport.VerificationImporters.SamusSnapshotWords.EncodeChargedShotGlow(projectiles.Charge.ChargedShotGlow) - (1 << 15);
            AssertTrue(!projectiles.UpdateBeamChargeGlow(samusColours, samus, global::SuperMetroid.Core.Fx.RoomVisorAnimation.Steady),
                $"Hyper-shot call {call} restores no suit");
            if ((call & 1) != 0)
            {
                AssertEqual(before, samusColours.Suit,
                    $"Hyper-shot call {call} does not repaint");
                continue;
            }
            int frame = call / 2;
            // Even glow steps 20..2 select Hyper palettes 0..9.
            AssertEqual(20 - frame * 2, glowStepsBefore, $"Hyper-shot playback frame {frame}");
            for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
                AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.Charge.ResolveHyperStep(SuperMetroid.Core.Samus.Colours.SamusShadeDefinitions.FullBodyCycles.HyperBeamShadeCount - 1 - (frame), color),
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
            for (int suit = 0; suit < 3; suit++)
            {
                int table = family == 0 ? SamusSourceDefinitions.BeamChargePointers.Value : SamusSourceDefinitions.PseudoScrewPointers.Value;
                ushort list = Word(table + suit * 2);
                for (int phase = 0; phase < 6; phase++)
                {
                    ushort pointer = Word(0x910000 | (list + phase * 2));
                    bool found = family == 0
                        ? SamusPaletteNative.TryChargedBeam((ushort)(suit * 2), (ushort)(phase * 2), out ushort compiled)
                        : SamusPaletteNative.TryPseudoScrew((ushort)(suit * 2), (ushort)(phase * 2), out compiled);
                    AssertTrue(found, "Native charge pointer supported");
                    AssertEqual(pointer, compiled, "Native charge pointer selection");
                    samusColours.ShowSuit(SamusShades.Charge(new SamusChargeShade(
                        family != 0 ? SamusChargeKind.PseudoScrew : SamusChargeKind.ChargedBeam, SamusSuitListLayoutConstants.AtPlace(suit), phase)));
                    RgbColor[] shownSuit = samusColours.ShownSuit(catalogSources);
                    for (int color = 0; color < 16; color++)
                    {
                        ushort native = Word(0x9b0000 | (pointer + color * 2));
                        expected[family, suit, phase, color] = native;
                        AssertEqual(native, colours.ResolveCharge(family != 0, suit, phase, color), "Native charge color");
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
        var whiteout = Enumerable.Range(0, SamusShadeDefinitions.Death.WhiteoutShadeCount)
            .Select(index => Word(SamusSourceDefinitions.DeathWhiteoutShades.Value + index * 2)).ToArray();
        var selectors = Enumerable.Range(0, SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordCount)
            .Select(index => (ushort)rom.ReadCartridgeByte(SamusSourceDefinitions.DeathExplosionTimingAndPaletteIndices.Value + 2 * index + 1)).ToArray();
        var death = new InstalledSamusDeathPalettes(SamusDeathPaletteFixtureKeys.Suits(ToColors(deathSuited)), ToColors(deathSuitless), ToColors(whiteout),
            SamusDeathPaletteFixtureKeys.ExplosionSteps(selectors));
        for (int index = 0; index < whiteout.Length; index++)
        {
            AssertEqual(whiteout[index], death.WhiteoutColor(index), "Every original whiteout RGB word");
        }
        foreach (int invalid in new[] { -1, 22, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => death.WhiteoutColor(invalid), "Invalid whiteout selector");
        }
        // The stock death palette view returns every supplied colour, whiteout shade and selector, serializes its rows
        // as they were supplied, and copies its inputs.
        {
            ushort[][][] suited = deathSuited.Select(rows => rows.Select(row => (ushort[])row.Clone()).ToArray()).ToArray();
            var stockDeath = new InstalledSamusDeathPalettes(SamusDeathPaletteFixtureKeys.Suits(ToColors(suited)), ToColors(deathSuitless), ToColors(whiteout),
                SamusDeathPaletteFixtureKeys.ExplosionSteps(selectors));
            for (int index = 0; index < whiteout.Length; index++)
                AssertEqual(whiteout[index], stockDeath.WhiteoutColor(index), "Original whiteout shades");
            for (int frame = 0; frame < selectors.Length; frame++)
                AssertEqual(selectors[frame], stockDeath.ExplosionPaletteIndex(frame), "Original death selectors");
            for (int family = 0; family < 4; family++)
            for (int palette = 0; palette < 10; palette++)
            for (int color = 0; color < 16; color++)
                AssertEqual((family < 3 ? suited[family] : deathSuitless)[palette][color],
                    family < 3 ? stockDeath.SuitedColor(SuperMetroid.AssetExtraction.Samus.SamusSuitListLayout.Order[family], palette, color) : stockDeath.SuitlessColor(palette, color),
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
            RgbColor before = stockDeath.SuitedColor(SuperMetroid.Core.Samus.Colours.SamusSuit.Power, 0, 0);
            suited[0][0][0] ^= 1;
            AssertEqual(before, stockDeath.SuitedColor(SuperMetroid.Core.Samus.Colours.SamusSuit.Power, 0, 0), "Death catalog copies inputs instead of retaining caller arrays");
        }
        foreach (int invalid in new[] { -1, 9, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => death.ExplosionPaletteIndex(invalid), "Invalid explosion frame");
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitedColor(SuperMetroid.Core.Samus.Colours.SamusSuit.Power, invalid, 0), "Invalid suited death palette");
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitlessColor(invalid, 0), "Invalid suitless death palette");
        }
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitedColor(SuperMetroid.Core.Samus.Colours.SamusSuit.Power, 0, invalid), "Invalid suited color");
            AssertThrows<ArgumentOutOfRangeException>(() => death.SuitlessColor(0, invalid), "Invalid suitless color");
        }
        foreach (bool pseudo in new[] { false, true })
        foreach (int invalid in new[] { -1, 6, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => colours.ResolveCharge(pseudo, 0, invalid, 0), "Invalid charge phase rejected");
        }
        foreach (int invalid in new[] { -1, 3, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => colours.ResolveCharge(false, invalid, 0, 0), "Invalid suit rejected");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => colours.ResolveCharge(false, 0, 0, invalid), "Invalid color rejected");
        var guarded = NewChargeColorReadGuard(rom, forbidden);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { 
            EquippedBeams = SamusBeamFlags.Charge }.InPose(SamusPoseId.FacingRightNormalPose);
        var projectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        projectiles.Charge.SetFlareCount(60);
        ushort[] suits = [0, 1, 0x20];
        for (int family = 0; family < 2; family++)
        for (int suit = 0; suit < 3; suit++)
        {
            samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(suits[suit]);
            samus.HorizontalSpeed.BodyAttack = family == 0 ? SamusBodyAttack.None : SamusBodyAttack.PseudoScrew;
            for (int phase = 0; phase < 6; phase++)
            {
                int appliedPhase = projectiles.Charge.ChargeGlowStep;
                AssertTrue(!projectiles.UpdateBeamChargeGlow(samusColours, samus, global::SuperMetroid.Core.Fx.RoomVisorAnimation.Steady), "Native charge cycle restores no suit");
                AssertEqual(phase, appliedPhase, "Native charge phase order");
                for (int color = 0; color < 16; color++)
                    AssertEqual(expected[family, suit, phase, color], samusColours.ShownSuit(catalogSources)[color], "Guarded runtime charge color");
            }
        }
        AssertEqual(0, projectiles.Charge.ChargeGlowStep, "Charge phase wraps after six calls");
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
