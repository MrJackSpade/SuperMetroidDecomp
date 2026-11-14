using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
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
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Samus.Palettes;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Checks extracted suit palettes and installed colors through Samus's normal palette paths.</summary>
internal static class SamusSuitColorsVerification
{
    /// <summary>Compares all three suit palettes to retail colors and checks catalog bounds and CGRAM installation.</summary>
    internal static void VerifyNormalSuitCatalogBoundary()
    {
        var cartridge = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(cartridge.Rom)), "Normal suit sharing oracle revision");
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(cartridge);
        InstalledSamusSuitColors colors = SamusSuitColorFile.Read(new MemoryStream(extracted));
        foreach (SamusSuit suit in Enum.GetValues<SamusSuit>())
        {
            int source = suit switch
            {
                SamusSuit.Power => 0x9b9400,
                SamusSuit.Varia => 0x9b9520,
                SamusSuit.Gravity => 0x9b9800,
                _ => throw new InvalidOperationException($"Unknown suit {suit}."),
            };
            for (int color = 0; color < 16; color++)
                AssertEqual(RgbColor.FromWordIgnoringTopBit(ReadVerificationWord(cartridge, source + 2 * color)), colors.Resolve(suit, color),
                    "Every native suit word is kept as installed");
        }
        // A suit outside the three the cartridge shows is rejected, as the 0/2/4 table offsets were.
        AssertThrows<ArgumentOutOfRangeException>(() => colors.Resolve((SamusSuit)Enum.GetValues<SamusSuit>().Length, 0), "Undefined suit");
        foreach (SamusSuit shownSuit in Enum.GetValues<SamusSuit>())
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => colors.Resolve(shownSuit, invalid), "Normal color index bounds");
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

        // Samus names the suit's shade (#1512); the import resolves it from the catalog.
        var sources = InstalledFixtures.SamusShadeSources with { Suits = colors };
        for (int suit = 0; suit < equipment.Length; suit++)
        {
            var samusColours = new SamusColours();
            SamusNormalSuitPalette.Load(samusColours, SamusEquipmentWordCodec.FromEquipmentWord(equipment[suit]));
            for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
                AssertEqual(
                    (ushort)(ReadNativeSuitColor(cartridge, sourceAddresses[suit], color) & 0x7fff),
                    samusColours.ShownSuit(sources)[color],
                    $"suit {suit} color {color} is installed without a runtime bus");
        }

        var powerColours = new SamusColours();
        SamusNormalSuitPalette.LoadPower(powerColours);
        for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
            AssertEqual(colors.Resolve(SamusSuit.Power, color),
                powerColours.ShownSuit(sources)[color],
                $"Power Suit color {color} is installed without a runtime bus");

        Console.WriteLine("Normal suit catalog boundary: all 48 stock colors, native pointers and the Power shade pass without a runtime bus.");
    }

    /// <summary>Checks installed suit colors through live loading, restoration, hurt recovery, and Ceres entry.</summary>
    internal static void VerifySamusSuitColors(
        string stockDirectory,
        AreaMapPresentation original,
        ISnesAddressSpace bus,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus);
        InstalledSamusSuitColors decoded = SamusSuitColorFile.Read(
            new MemoryStream(extracted, writable: false));
        int[] sourceAddresses =
        [
            SamusSourceDefinitions.PowerSuitPalette.Value,
            SamusSourceDefinitions.VariaSuitPalette.Value,
            SamusSourceDefinitions.GravitySuitPalette.Value,
        ];
        SamusSuit[] offsets = [SamusSuit.Power, SamusSuit.Varia, SamusSuit.Gravity];
        for (int suit = 0; suit < offsets.Length; suit++)
        for (int color = 0; color < SamusSuitColorFormat.ColorsPerSuit; color++)
            AssertEqual(ReadNativeSuitColor(CartridgeImportSource.Require(bus), sourceAddresses[suit], color),
                decoded.Resolve(offsets[suit], color),
                $"extracted normal suit {suit} color {color} agrees with cartridge");

        SamusSuitColorDocument document = JsonSerializer.Deserialize<SamusSuitColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, SamusSuitColorFile.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Samus suit palette JSON is null.");
        var guard = NewSuitColorReadGuard(bus);
        var samusColours = new SamusColours();
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        // The installed catalog reaches Samus through the shades the import makes from it (#1512).
        var installedSources = InstalledFixtures.SamusShadeSources with { Suits = InstalledFixtures.ImportOf(original).SamusColours.Suits };
        ushort[] equipment =
        [
            0,
            SamusEquipmentFlags.VariaSuit.ToEquipmentWord(),
            ((SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit).ToEquipmentWord()),
        ];
        for (int suit = 0; suit < equipment.Length; suit++)
        {
            samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment[suit]);
            SamusNormalSuitPalette.Load(samusColours, samus.EquippedItems);
            AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.Suits.Resolve(offsets[suit], 1),
                samusColours.ShownSuit(installedSources)[1],
                $"installed suit {suit} reaches live Samus CGRAM");
        }

        samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment[1]);
        samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();
        AssertTrue(samus.HorizontalSpeed.ApplyPendingNormalSuitPaletteRestore(
                samusColours, samus.EquippedItems),
            "momentum cancellation admits installed normal suit restoration");
        AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.Suits.Resolve(SamusSuit.Varia, 1),
            samusColours.ShownSuit(installedSources)[1],
            "momentum cancellation restores the installed Varia color");

        samus.HurtFlash.HoldAt(2);
        // Show another shade first so only a real normal-suit reload passes.
        samusColours.ShowSuit(SamusShades.Hurt(SamusHurtColorVariant.Hurt));
        SamusHurtFlashPalette.Update(
            samusColours, samus, controllerInput: 0);
        AssertEqual(SamusShades.Suit(SamusSuit.Varia), samusColours.Suit,
            "even hurt-flash frame restores the normal suit's shade");
        AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.Suits.Resolve(SamusSuit.Varia, 1),
            samusColours.ShownSuit(installedSources)[1],
            "hurt-flash recovery restores the installed Varia color");

        samus.EquippedItems = (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.ScrewAttack);
        AssertTrue(samus.HorizontalSpeed.UpdateSpeedBoosterPalette(samusColours, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 0x1b, EquippedItems = samus.EquippedItems }),
            "Screw Attack recovery frame restores the normal suit");
        AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.Suits.Resolve(SamusSuit.Varia, 1),
            samusColours.ShownSuit(installedSources)[1],
            "Screw Attack recovery restores the installed Varia color");

        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork);
        runtime.Artwork.MapPresentation = original;
        fixtureAssets.Bind(runtime);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.Suits.Resolve(SamusSuit.Power, 1),
            runtime.Player.Colours.ShownSuit(installedSources)[1],
            "installed production Ceres entry uses the installed Power Suit color");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed normal suit paths do not reread the three cartridge palettes");

        PaletteRgb5 originalColor = document.Varia[1];
        document.Varia[1] = originalColor with { Red = 32 };
        byte[] malformed = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        AssertThrows<InvalidDataException>(() => SamusSuitColorFile.Read(
            new MemoryStream(malformed, writable: false)),
            "Samus suit colors reject out-of-range RGB5 channels");

        Console.WriteLine("Samus suit colors: three stock palettes reach live CGRAM, production entry, normal restoration branches, ROM guard and invalid data pass.");
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
