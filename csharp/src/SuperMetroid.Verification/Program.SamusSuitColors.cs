using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyNormalSuitCatalogBoundary()
    {
        var cartridge = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cartridge.Rom)), "Normal suit sharing oracle revision");
        byte[] extracted = SuperMetroid.AssetExtraction.SamusSuitColorExtractor.Extract(cartridge);
        SamusSuitColorCatalog colors = SamusSuitColorCatalog.Load(new MemoryStream(extracted));
        foreach (var (field, source) in new[] { ("varia", 0x9b9520), ("gravity", 0x9b9800) })
        {
            var differences = (Dictionary<int, ushort>)typeof(SamusSuitColorCatalog)
                .GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(colors)!;
            AssertEqual(4, differences.Count, "Only four differing native colors per secondary suit remain stored");
            for (int color = 0; color < 16; color++)
            {
                ushort powerColor = ReadVerificationWord(cartridge, 0x9b9400 + 2 * color);
                ushort suitColor = ReadVerificationWord(cartridge, source + 2 * color);
                AssertEqual(powerColor != suitColor, differences.TryGetValue(color, out ushort stored), "Every native shared/different slot is independently classified");
                if (powerColor != suitColor) AssertEqual(suitColor, stored, "Differing native word is preserved");
            }
        }
        for (int offset = 0; offset <= ushort.MaxValue; offset++)
            if (offset is not (0 or 2 or 4))
                AssertThrows<ArgumentOutOfRangeException>(() => colors.Resolve((ushort)offset, 0), "Complete invalid suit-offset domain");
        foreach (ushort offset in new ushort[] { 0, 2, 4 })
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => colors.Resolve(offset, invalid), "Normal color index bounds");
        foreach (bool powerOnly in new[] { false, true })
        {
            var document = JsonSerializer.Deserialize<SamusSuitColorDocument>(extracted, MapPresentationFormat.JsonOptions)!;
            int ordinal = 1000;
            foreach (var palette in new[] { document.Power, document.Varia, document.Gravity })
            for (int color = 0; color < 16; color++, ordinal++)
                if (!powerOnly || ReferenceEquals(palette, document.Power))
                    palette[color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
            var edited = SamusSuitColorCatalog.Load(new MemoryStream(SamusSuitColorCatalog.Write(document)));
            ordinal = 1000;
            for (int suit = 0; suit < 3; suit++)
            for (int color = 0; color < 16; color++, ordinal++)
            {
                int source = suit == 0 ? 0x9b9400 : suit == 1 ? 0x9b9520 : 0x9b9800;
                ushort expected = !powerOnly || suit == 0 ? (ushort)ordinal : ReadVerificationWord(cartridge, source + 2 * color);
                AssertEqual(expected, edited.Resolve((ushort)(2 * suit), color), "Every supplied suit color survives independent Power/suit edits");
            }
        }
        ushort[] equipment =
        [
            0,
            (ushort)SamusEquipmentFlags.VariaSuit,
            (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        int[] sourceAddresses =
        [
            SamusRenderingRomData.Body.PowerSuitPalette,
            SamusRenderingRomData.Body.VariaSuitPalette,
            SamusRenderingRomData.Body.GravitySuitPalette,
        ];

        for (int suit = 0; suit < equipment.Length; suit++)
        {
            var cgram = new SnesCgram();
            ushort pointer = SamusNormalSuitPalette.Load(cgram, equipment[suit], colors);
            AssertEqual(SamusPaletteRomData.Common.NormalSuitPalettePointer((ushort)(suit * 2)),
                pointer, $"suit {suit} retains its native diagnostic pointer");
            for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
                AssertEqual(
                    (ushort)(RomDataReader.ReadWordFixedBank(cartridge,
                        sourceAddresses[suit] + color * sizeof(ushort)) & 0x7fff),
                    cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                    $"suit {suit} color {color} is installed without a runtime bus");
        }

        var power = new SnesCgram();
        SamusNormalSuitPalette.LoadPower(power, colors);
        for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
            AssertEqual(colors.Resolve(0, color),
                power.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color],
                $"Power Suit color {color} is installed without a runtime bus");

        AssertThrows<InvalidOperationException>(
            () => SamusNormalSuitPalette.Load(new SnesCgram(), 0, null),
            "missing installed normal suit colors fail explicitly");
        AssertThrows<InvalidOperationException>(
            () => SamusNormalSuitPalette.LoadPower(new SnesCgram(), null),
            "missing installed Power Suit colors fail explicitly");
        Console.WriteLine("Normal suit catalog boundary: all 48 stock colors, native pointers, Power copy, and missing-content errors pass without a runtime bus.");
    }

    private static void VerifySamusSuitColorOverride(
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog original,
        ISnesAddressSpace bus,
        GameplayBasePaletteCatalog initialPalettes,
        MapPresentationInstalledRoomAssets fixtureAssets)
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
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), sourceAddresses[suit] + color * 2),
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
                guard, cgram, SamusMovementType.SpinJumping, animationFrame: 0x1b,
                samus.EquippedItems, suitColors: edited.SamusSuitColors),
            "Screw Attack recovery frame restores the normal suit");
        AssertEqual(edited.SamusSuitColors.Resolve(2, 1),
            cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + 1],
            "Screw Attack recovery preserves edited Varia color");

        var runtime = new SuperMetroidRuntime(guard,
            initialPaletteArt: initialPalettes) { MapPresentation = edited };
        fixtureAssets.Bind(runtime);
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

    private sealed class SuitColorReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
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
            RejectSuitColorRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectSuitColorRead(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Suit-color verification source requires WRAM.")).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Suit-color verification source requires SRAM.")).ReadSaveRamByte(address);

        private void RejectSuitColorRead(int address)
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
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
