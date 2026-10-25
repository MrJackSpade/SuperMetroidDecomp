using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Palettes;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Checks extracted suit palettes and edited installed colors through Samus's normal palette paths.</summary>
internal static class SamusSuitColorsVerification
{
    /// <summary>Compares all three suit palettes to retail colors and checks catalog bounds and CGRAM installation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyNormalSuitCatalogBoundary()
    {
        var cartridge = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cartridge.Rom)), "Normal suit sharing oracle revision");
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(cartridge);
        SamusSuitColorCatalog colors = SamusSuitColorFile.Read(new MemoryStream(extracted));
        foreach (var (differences, source) in new[] { new VerifyNormalSuitCatalogBoundaryCase(colors.varia, 0x9b9520), new VerifyNormalSuitCatalogBoundaryCase(colors.gravity, 0x9b9800) })
        {
            AssertEqual(4, differences.Count, "Only four differing native colors per secondary suit remain stored");
            for (int color = 0; color < 16; color++)
            {
                ushort powerColor = ReadVerificationWord(cartridge, 0x9b9400 + 2 * color);
                ushort suitColor = ReadVerificationWord(cartridge, source + 2 * color);
                AssertEqual(powerColor != suitColor, differences.TryGetValue(new ColourCell(0, color), out RgbColor stored), "Every native shared/different slot is independently classified");
                if (powerColor != suitColor) AssertEqual(RgbColor.FromCgramPortWord(suitColor), stored, "Differing native word is preserved");
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
            var document = RequiredJson.Deserialize<SamusSuitColorDocument>(extracted, ArtworkJson.Options);
            int ordinal = 1000;
            foreach (var palette in new[] { document.Power, document.Varia, document.Gravity })
            for (int color = 0; color < 16; color++, ordinal++)
                if (!powerOnly || ReferenceEquals(palette, document.Power))
                    palette[color] = new PaletteRgb5 { Red = ordinal & 31, Green = ordinal >> 5 & 31, Blue = ordinal >> 10 & 31 };
            var edited = SamusSuitColorFile.Read(new MemoryStream(SamusSuitColorFile.Write(document)));
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
            SamusEquipmentFlags.VariaSuit.ToEquipmentWord(),
            ((SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit).ToEquipmentWord()),
        ];
        int[] sourceAddresses =
        [
            SamusSourceDefinitions.PowerSuitPalette.Value,
            SamusSourceDefinitions.VariaSuitPalette.Value,
            SamusSourceDefinitions.GravitySuitPalette.Value,
        ];

        for (int suit = 0; suit < equipment.Length; suit++)
        {
            var cgram = new SamusColourRig();
            SamusNormalSuitPalette.Load(cgram.Samus.Suit, SamusEquipmentWordCodec.FromEquipmentWord(equipment[suit]), colors);
            for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
                AssertEqual(
                    (ushort)(ReadNativeSuitColor(cartridge, sourceAddresses[suit], color) & 0x7fff),
                    cgram.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + color],
                    $"suit {suit} color {color} is installed without a runtime bus");
        }

        var power = new SamusColourRig();
        SamusNormalSuitPalette.LoadPower(power.Samus.Suit, colors);
        for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
            AssertEqual(colors.Resolve(0, color),
                power.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + color],
                $"Power Suit color {color} is installed without a runtime bus");

        AssertThrows<InvalidOperationException>(
            () => SamusNormalSuitPalette.Load(new SamusColourRig().Samus.Suit, 0, null),
            "missing installed normal suit colors fail explicitly");
        AssertThrows<InvalidOperationException>(
            () => SamusNormalSuitPalette.LoadPower(new SamusColourRig().Samus.Suit, null),
            "missing installed Power Suit colors fail explicitly");
        Console.WriteLine("Normal suit catalog boundary: all 48 stock colors, native pointers, Power copy, and missing-content errors pass without a runtime bus.");
    }

    /// <summary>Checks edited suit colors through live loading, restoration, hurt recovery, and Ceres entry.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifySamusSuitColorOverride(
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog original,
        ISnesAddressSpace bus,
        GameplayBasePaletteCatalog initialPalettes,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus);
        SamusSuitColorCatalog decoded = SamusSuitColorFile.Read(
            new MemoryStream(extracted, writable: false));
        int[] sourceAddresses =
        [
            SamusSourceDefinitions.PowerSuitPalette.Value,
            SamusSourceDefinitions.VariaSuitPalette.Value,
            SamusSourceDefinitions.GravitySuitPalette.Value,
        ];
        ushort[] offsets = [0, 2, 4];
        for (int suit = 0; suit < offsets.Length; suit++)
        for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
            AssertEqual(ReadNativeSuitColor(CartridgeImportSource.Require(bus), sourceAddresses[suit], color),
                decoded.Resolve(offsets[suit], color),
                $"extracted normal suit {suit} color {color} agrees with cartridge");

        SamusSuitColorDocument document = JsonSerializer.Deserialize<SamusSuitColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, SamusSuitColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Samus suit palette JSON is null.");
        Paint(document.Power, 1);
        Paint(document.Varia, 2);
        Paint(document.Gravity, 3);
        string replacement = Path.Combine(overrideDirectory, SamusSuitColorFormat.FileName);
        File.WriteAllBytes(replacement, SamusSuitColorFile.Write(document));
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Samus suit color override changes installed-content identity");

        var guard = NewSuitColorReadGuard(bus);
        var cgram = new SamusColourRig();
        var samus = new SamusState { Presentation = { SuitColors = edited.SamusSuitColors } };
        ushort[] equipment =
        [
            0,
            SamusEquipmentFlags.VariaSuit.ToEquipmentWord(),
            ((SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit).ToEquipmentWord()),
        ];
        for (int suit = 0; suit < equipment.Length; suit++)
        {
            samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment[suit]);
            samus.Presentation.LoadSuitPalette(samus, cgram.Samus);
            AssertEqual(edited.SamusSuitColors.Resolve(offsets[suit], 1),
                cgram.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1],
                $"edited suit {suit} reaches live Samus CGRAM");
            AssertTrue(cgram.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1] !=
                original.SamusSuitColors.Resolve(offsets[suit], 1),
                $"edited suit {suit} differs from stock");
        }

        samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment[1]);
        samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        AssertTrue(samus.HorizontalSpeed.ApplyPendingNormalSuitPaletteRestore(
                cgram.Samus, samus.EquippedItems, edited.SamusSuitColors),
            "momentum cancellation admits installed normal suit restoration");
        AssertEqual(edited.SamusSuitColors.Resolve(2, 1),
            cgram.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1],
            "momentum cancellation preserves edited Varia color");

        samus.HurtFlashCounter = 2;
        // Overwrite the restored color first so only a real normal-suit reload passes.
        ushort hurtRestoreSentinel = unchecked((ushort)(edited.SamusSuitColors.Resolve(2, 1).ToWord() ^ 0x7fff));
        cgram.SetColor(SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1, RgbColor.FromWord(checked(hurtRestoreSentinel)));
        SamusHurtFlashPalette.Update(
            cgram.Samus.Suit, samus, controllerInput: 0);
        AssertTrue(cgram.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1].ToWord() != hurtRestoreSentinel,
            "even hurt-flash frame restores the normal suit");
        AssertEqual(edited.SamusSuitColors.Resolve(2, 1),
            cgram.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1],
            "hurt-flash recovery preserves edited Varia color");

        samus.EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.ScrewAttack);
        AssertTrue(samus.HorizontalSpeed.UpdateSpeedBoosterPalette(cgram.Samus, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 0x1b, EquippedItems = samus.EquippedItems }, suitColors: edited.SamusSuitColors),
            "Screw Attack recovery frame restores the normal suit");
        AssertEqual(edited.SamusSuitColors.Resolve(2, 1),
            cgram.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1],
            "Screw Attack recovery preserves edited Varia color");

        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        runtime.Artwork.MapPresentation = edited;
        fixtureAssets.Bind(runtime);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        AssertEqual(edited.SamusSuitColors.Resolve(0, 1),
            runtime.Colours.Colors[SamusPaletteDefinitions.Common.SamusObjPaletteStart + 1],
            "installed production Ceres entry uses edited Power Suit color");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed normal suit paths do not reread the three cartridge palettes");

        PaletteRgb5 originalColor = document.Varia[1];
        document.Varia[1] = originalColor with { Red = 32 };
        byte[] malformed = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        AssertThrows<InvalidDataException>(() => SamusSuitColorFile.Read(
            new MemoryStream(malformed, writable: false)),
            "Samus suit colors reject out-of-range RGB5 channels");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing Samus suit override restores installed-content identity");
        Console.WriteLine("Samus suit colors: three stock palettes, three live CGRAM edits, production entry, normal restoration branches, ROM guard, invalid data and override removal pass.");

        static void Paint(PaletteRgb5[] colors, int blue)
        {
            PaletteRgb5 color = colors[1];
            colors[1] = color with { Blue = color.Blue == blue ? blue + 1 : blue };
        }
    }

    /// <summary>Native addresses of Power, Varia, and Gravity palette words guarded during runtime checks.</summary>
    private static readonly int[] SuitColorSources =
    [
        SamusSourceDefinitions.PowerSuitPalette.Value,
        SamusSourceDefinitions.VariaSuitPalette.Value,
        SamusSourceDefinitions.GravitySuitPalette.Value,
    ];

    /// <summary>Rejects runtime reads from the three cartridge suit color tables.</summary>
    private static MutableCartridgeReadGuard NewSuitColorReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => SuitColorSources.Any(sourceAddress => address >= sourceAddress &&
                address < sourceAddress + SamusSuitColorFormat.ColorsPerSuit * sizeof(ushort)),
            address => $"Production reread suit color ${address:X6}.");
        return guard;
    }

    /// <summary>Reads a native suit palette word solely for comparison with extracted colors.</summary>
    private static ushort ReadNativeSuitColor(IImportCartridgeSource cartridge, int sourceAddress, int color) =>
        RomDataReader.ReadWordFixedBank(cartridge, sourceAddress + color * sizeof(ushort));

    /// <summary>Runs the standalone normal suit catalog boundary verification command.</summary>
    [VerificationCommand("--normal-suit-catalog-boundary")]
    internal static int NormalSuitCatalogBoundaryCommand()
    {
        Suite(nameof(VerifyNormalSuitCatalogBoundary), () => VerifyNormalSuitCatalogBoundary());
        return 0;
    }
}
