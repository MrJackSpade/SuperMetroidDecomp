using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifySamusSuitColorOverride(
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog original,
        ISnesAddressSpace bus)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.SamusSuitColorExtractor.Extract(bus);
        SamusSuitColorCatalog decoded = SamusSuitColorCatalog.Load(
            new MemoryStream(extracted, writable: false));
        int[] sourceAddresses =
        [
            SamusRenderingRomData.Body.PowerSuitPalette,
            SamusRenderingRomData.Body.VariaSuitPalette,
            SamusRenderingRomData.Body.GravitySuitPalette,
        ];
        ushort[] offsets = [0, 2, 4];
        for (int suit = 0; suit < offsets.Length; suit++)
        for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, sourceAddresses[suit] + color * 2),
                decoded.Resolve(offsets[suit], color),
                $"extracted normal suit {suit} color {color} agrees with cartridge");

        SamusSuitColorDocument document = JsonSerializer.Deserialize<SamusSuitColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, SamusSuitColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Samus suit palette JSON is null.");
        Paint(document.Power, 1);
        Paint(document.Varia, 2);
        Paint(document.Gravity, 3);
        string replacement = Path.Combine(overrideDirectory, SamusSuitColorFormat.FileName);
        File.WriteAllBytes(replacement, SamusSuitColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Samus suit color override changes installed-content identity");

        var guard = new SuitColorReadGuard(bus);
        var cgram = new SnesCgram();
        var samus = new SamusState { SuitColors = edited.SamusSuitColors };
        ushort[] equipment =
        [
            0,
            (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        for (int suit = 0; suit < equipment.Length; suit++)
        {
            samus.EquippedItems = equipment[suit];
            samus.LoadSuitPalette(guard, cgram);
            AssertEqual(edited.SamusSuitColors.Resolve(offsets[suit], 1),
                cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + 1],
                $"edited suit {suit} reaches live Samus CGRAM");
            AssertTrue(cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + 1] !=
                original.SamusSuitColors.Resolve(offsets[suit], 1),
                $"edited suit {suit} differs from stock");
        }

        samus.EquippedItems = equipment[1];
        samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        AssertTrue(samus.HorizontalSpeed.ApplyPendingNormalSuitPaletteRestore(
                guard, cgram, samus.EquippedItems, edited.SamusSuitColors),
            "momentum cancellation admits installed normal suit restoration");
        AssertEqual(edited.SamusSuitColors.Resolve(2, 1),
            cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + 1],
            "momentum cancellation preserves edited Varia color");

        samus.HurtFlashCounter = 2;
        SamusHurtFlashPaletteStepResult hurtRestore = SamusHurtFlashPalette.Update(
            guard, cgram, samus, controllerInput: 0);
        AssertEqual(SamusHurtFlashPaletteAction.NormalSuitRestore, hurtRestore.Action,
            "even hurt-flash frame restores the normal suit");
        AssertEqual(edited.SamusSuitColors.Resolve(2, 1),
            cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + 1],
            "hurt-flash recovery preserves edited Varia color");

        samus.EquippedItems = (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.ScrewAttack);
        AssertTrue(samus.HorizontalSpeed.UpdateSpeedBoosterPalette(
                guard, cgram, SamusMovementType.SpinJumping, animationFrame: 1,
                samus.EquippedItems, suitColors: edited.SamusSuitColors),
            "early Screw Attack frame restores the normal suit");
        AssertEqual(edited.SamusSuitColors.Resolve(2, 1),
            cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + 1],
            "Screw Attack recovery preserves edited Varia color");

        var runtime = new SuperMetroidRuntime(guard) { MapPresentation = edited };
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        AssertEqual(edited.SamusSuitColors.Resolve(0, 1),
            runtime.Cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + 1],
            "installed production Ceres entry uses edited Power Suit color");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed normal suit paths do not reread the three cartridge palettes");

        PaletteRgb5 originalColor = document.Varia[1];
        document.Varia[1] = originalColor with { Red = 32 };
        byte[] malformed = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        AssertThrows<InvalidDataException>(() => SamusSuitColorCatalog.Load(
            new MemoryStream(malformed, writable: false)),
            "Samus suit colors reject out-of-range RGB5 channels");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing Samus suit override restores installed-content identity");
        Console.WriteLine("Samus suit colors: three stock palettes, three live CGRAM edits, production entry, normal restoration branches, ROM guard, invalid data and override removal pass.");

        static void Paint(PaletteRgb5[] colors, int blue)
        {
            PaletteRgb5 color = colors[1];
            colors[1] = color with { Blue = color.Blue == blue ? blue + 1 : blue };
        }
    }

    private sealed class SuitColorReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        private static readonly int[] Sources =
        [
            SamusRenderingRomData.Body.PowerSuitPalette,
            SamusRenderingRomData.Body.VariaSuitPalette,
            SamusRenderingRomData.Body.GravitySuitPalette,
        ];
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (int sourceAddress in Sources)
            {
                if (address >= sourceAddress &&
                    address < sourceAddress + SamusSuitColorFormat.ColorsPerSuit * sizeof(ushort))
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException($"Production reread suit color ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
