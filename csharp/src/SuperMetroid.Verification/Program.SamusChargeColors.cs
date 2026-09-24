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
        for (int family = 0; family < 2; family++)
        for (int suit = 0; suit < SamusChargeColorFormat.SuitCount; suit++)
        {
            int table = family == 0
                ? SamusProjectileRomData.Palettes.BeamChargePointers
                : SamusProjectileRomData.Palettes.PseudoScrewPointers;
            int top = table + suit * sizeof(ushort);
            ushort list = BlockWord(top);
            for (int phase = 0; phase < SamusChargeColorFormat.PhasesPerSuit; phase++)
            {
                ushort offset = (ushort)(phase * sizeof(ushort));
                ushort pointer = BlockWord(SamusProjectileRomData.Banks.Pose | (list + offset));
                bool compiled = family == 0
                    ? SamusChargePalettePointerDefinitions.TryChargedBeam((ushort)(suit * 2), offset,
                        out ushort compiledPointer)
                    : SamusChargePalettePointerDefinitions.TryPseudoScrew((ushort)(suit * 2), offset,
                        out compiledPointer);
                AssertTrue(compiled, "all native charge list entries are compiled");
                AssertEqual(pointer, compiledPointer, $"family {family} suit {suit} phase {phase} pointer");
                for (int color = 0; color < SamusChargeColorFormat.ColorsPerPalette; color++)
                {
                    int address = SamusProjectileRomData.Banks.PaletteAndTrailData |
                        (pointer + color * sizeof(ushort));
                    AssertEqual(BlockWord(address), native.ResolveCharge(family != 0, suit, phase, color),
                        $"family {family} suit {suit} phase {phase} color {color}");
                }
            }
        }
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
            return RomDataReader.ReadWordFixedBank(rom, address);
        }
        static void Paint(PaletteRgb5[] colors)
        {
            PaletteRgb5 existing = colors[1];
            colors[1] = existing with { Blue = existing.Blue == 31 ? 30 : existing.Blue + 1 };
        }
    }

    private sealed class ChargeColorReadGuard(ISnesAddressSpace inner, HashSet<int> forbidden)
        : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }
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
