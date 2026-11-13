using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.Verification.Samus.Palettes;

/// <summary>Checks installed full-body palette cycles and shade components against the retail color tables.</summary>
internal static class SamusFullBodyCycleColorsVerification
{
    /// <summary>Compares extracted Speed Booster, Screw Attack, and shinespark colours with their native words and the shade each row reaches.</summary>
    internal static void VerifyFullBodyPaletteColorData(ISnesAddressSpace rom, byte[] extracted)
    {
        var native = SamusFullBodyCycleColorFile.Read(new MemoryStream(extracted));
        SamusFullBodyCycleColours nativeColours = native.ToColours();
        // Samus names cycle shades (#1512); the import resolves them from the decoded cycles.
        var nativeSources = InstalledFixtures.SamusShadeSources with { FullBodyCycles = nativeColours };
        var originalPointers = new SortedSet<ushort>();
        foreach (var (header, phases) in new[] { new SamusFullBodyCycleColorsVerifyFullBodyPaletteColorDataCase(0x91daa9, 4), new SamusFullBodyCycleColorsVerifyFullBodyPaletteColorDataCase(0x91da4a, 6), new SamusFullBodyCycleColorsVerifyFullBodyPaletteColorDataCase(0x91db10, 6), new SamusFullBodyCycleColorsVerifyFullBodyPaletteColorDataCase(0x91db75, 4) })
        for (int suit = 0; suit < 3; suit++)
        {
            ushort list = ReadVerificationWord(rom, header + 2 * suit);
            for (int phase = 0; phase < phases; phase++)
                originalPointers.Add(ReadVerificationWord(rom, 0x910000 | (list + 2 * phase)));
        }
        AssertEqual(48, originalPointers.Count, "Original dispatcher lists select48 distinct color rows");
        int ordinal = 0;
        foreach (ushort pointer in originalPointers)
        {
            AssertEqual(ordinal++, SamusFullBodyCycleColorFormat.PaletteIndex(pointer), "Original palette allocation order equals calculated index");
            var cgram = new SamusColours();
            SamusPaletteNative.ShowFullBodyCycle(cgram, pointer);
            for (int color = 0; color < 16; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
                AssertEqual(expected, native.Resolve(pointer, color), "Every original full-body palette word");
                AssertEqual((ushort)(expected & 0x7fff), cgram.ShownSuit(nativeSources)[color], "Every full-body palette row reaches Samus's shade");
            }
            foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => native.Resolve(pointer, invalid), "Full-body color bounds");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!originalPointers.Contains((ushort)pointer))
                AssertThrows<ArgumentOutOfRangeException>(() => native.Resolve((ushort)pointer, 0), "Complete original palette identity domain rejects gaps and outside addresses");
    }
    /// <summary>Drives the four installed cycle families' production owners for each suit and checks CGRAM without source rereads.</summary>
    internal static void VerifySamusFullBodyCycleColors(
        string stockDirectory,
        AreaMapPresentation original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(rom);
        Suite(nameof(VerifyFullBodyPaletteColorData), () => VerifyFullBodyPaletteColorData(rom, extracted));

        SamusFullBodyCycleColorDocument document = JsonSerializer.Deserialize<SamusFullBodyCycleColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, SamusFullBodyCycleColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock full-body cycle color JSON is null.");
        var installedSources = InstalledFixtures.SamusShadeSources with { FullBodyCycles = InstalledFixtures.ImportOf(original).SamusColours.FullBodyCycles };
        var guard = NewFullBodyColorReadGuard(rom);
        SamusEquipmentFlags[] equipment = [0, SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit];
        foreach (SamusEquipmentFlags items in equipment)
        {
            int suit = items.HasAny(SamusEquipmentFlags.GravitySuit) ? 2 :
                items.HasAny(SamusEquipmentFlags.VariaSuit) ? 1 : 0;
            for (int phase = 0; phase < 4; phase++)
            {
                var speed = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir)
                {
                    SpeedBoostCounter = new SamusSpeedBoostCounter(4, 0),
                    SpecialPaletteTimer = 1,
                    CyclePaletteStep = phase,
                };
                var cgram = new SamusColours();
                AssertTrue(speed.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.Running, AnimationFrame = 0, EquippedItems = items }),
                    "Speed Booster reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.SpeedBooster, suit, phase);
            }

            for (int phase = 0; phase < 6; phase++)
            {
                var screw = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { CyclePaletteStep = phase };
                var cgram = new SamusColours();
                AssertTrue(screw.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 1, EquippedItems = (items | SamusEquipmentFlags.ScrewAttack) }),
                    "Screw Attack reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.ScrewAttack, suit,
                    Math.Min(phase, 6 - phase));
            }

            var stored = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { EquippedItems = items };
            AssertTrue(stored.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0)), "seed stored shine");
            for (int phase = 0; phase < 6; phase++)
            {
                var cgram = new SamusColours();
                AssertTrue(stored.Shinespark.UpdatePalette(cgram, items),
                    "stored shine reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.StoredShine, suit,
                    Math.Min(phase, 6 - phase));
            }

            var active = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { EquippedItems = items };
            AssertTrue(active.Shinespark.TryStoreFromSpeedBooster(new SamusSpeedBoostCounter(4, 0)), "seed active shinespark");
            active.Shinespark.BeginWindup(active);
            for (int phase = 0; phase < 4; phase++)
            {
                var cgram = new SamusColours();
                AssertTrue(active.Shinespark.UpdatePalette(cgram, items),
                    "active shinespark reaches installed color source");
                AssertColors(cgram, SamusFullBodyCycleFamily.ActiveShinespark, suit, phase);
            }

            var attached = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                EquippedItems = items,
                MetroidAttachmentPalette = SamusMetroidAttachmentPalette.SpeedBoostNext,
            };
            var attachmentColors = new SamusColours();
            AssertTrue(SamusSpecialSuperPalette.Update(attachmentColors, attached),
                "Metroid attachment reaches installed Speed Booster color source");
            AssertColors(attachmentColors, SamusFullBodyCycleFamily.SpeedBooster, suit, 3);
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed full-body cycles do not reread any of the 768 source color words");

        // Samus binds no colour table (#1512): the installed cycles reach her only through the import's shades, which the
        // checks above resolve.

        PaletteRgb5 originalColor = document.SpeedBooster[0][0][1];
        document.SpeedBooster[0][0][1] = originalColor with { Blue = 32 };
        byte[] malformed = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        AssertThrows<InvalidDataException>(() => SamusFullBodyCycleColorFile.Read(
            new MemoryStream(malformed, writable: false)),
            "full-body cycle colors reject out-of-range RGB5 values");

        Console.WriteLine("Samus full-body cycles: 768 native colors, four live families/three suits, attachment flash, ROM guard and strict values pass.");

        void AssertColors(SamusColours cgram, SamusFullBodyCycleFamily family, int suit, int shade)
        {
            ushort pointer = SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
            for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
                AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.FullBodyCycles.Resolve(pointer, color),
                    cgram.ShownSuit(installedSources)[color],
                    $"{family} suit {suit}, shade {shade}, color {color} reaches live CGRAM");
        }
    }

    /// <summary>Forbids production access to every installed full-body palette row across families, suits, and shades.</summary>
    private static MutableCartridgeReadGuard NewFullBodyColorReadGuard(ISnesAddressSpace source)
    {
        var forbidden = new HashSet<int>();
        foreach (SamusFullBodyCycleFamily family in Enum.GetValues<SamusFullBodyCycleFamily>())
        for (int suit = 0; suit < SamusFullBodyCycleColorFormat.SuitCount; suit++)
        for (int shade = 0; shade < SamusFullBodyCycleColorFormat.ShadesPerSuit; shade++)
        {
            int address = SamusSourceDefinitions.PaletteBank.Value |
                SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
            for (int offset = 0;
                 offset < SamusFullBodyCycleColorFormat.ColorsPerPalette * sizeof(ushort);
                 offset++)
                forbidden.Add(address + offset);
        }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains, address => $"Production reread full-body color ${address:X6}.");
        return guard;
    }
}
