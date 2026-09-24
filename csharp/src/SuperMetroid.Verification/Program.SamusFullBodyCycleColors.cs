using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifySamusFullBodyCycleColorOverride(
        string stockDirectory, string overrideDirectory,
        AreaMapPresentationCatalog original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.SamusFullBodyCycleColorExtractor.Extract(rom);
        var native = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(extracted, writable: false));
        SamusFullBodyCycleFamily[] families = Enum.GetValues<SamusFullBodyCycleFamily>();
        foreach (SamusFullBodyCycleFamily family in families)
        for (int suit = 0; suit < SamusFullBodyCycleColorFormat.SuitCount; suit++)
        for (int shade = 0; shade < SamusFullBodyCycleColorFormat.ShadesPerSuit; shade++)
        {
            ushort pointer = SamusFullBodyCycleColorFormat.Pointer(family, suit, shade);
            for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                        SamusPaletteRomData.Banks.Palette | (pointer + color * 2)),
                    native.Resolve(pointer, color),
                    $"{family} suit {suit}, shade {shade}, color {color} agrees with cartridge");
        }

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
            AssertTrue(SamusSpecialSuperPalette.Update(guard, attachmentColors, attached),
                "Metroid attachment reaches installed Speed Booster color source");
            AssertColors(attachmentColors, SamusFullBodyCycleFamily.SpeedBooster, suit, 3);
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed full-body cycles do not reread any of the 768 source color words");

        var runtime = new SuperMetroidRuntime(guard) { MapPresentation = edited };
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

    private sealed class FullBodyColorReadGuard : ISnesAddressSpace
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

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Production reread full-body color ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
