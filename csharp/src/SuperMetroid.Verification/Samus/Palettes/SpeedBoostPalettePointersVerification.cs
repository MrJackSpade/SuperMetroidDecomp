using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Checks suit-dependent Speed Booster palette selection and its installed CGRAM colors.</summary>
internal static class SpeedBoostPalettePointersVerification
{
    /// <summary>Matches four equipment selections to native pointers and the colours the installed Speed Booster shade shows.</summary>
    internal static void VerifySpeedBoostPalettePointers()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusFullBodyCycleColours cycleColors = SamusFullBodyCycleColorFile.Read(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorExtractor.Extract(rom))).ToColours();
        SamusEquipmentFlags[] equipment =
        [
            0,
            SamusEquipmentFlags.VariaSuit,
            SamusEquipmentFlags.GravitySuit,
            (SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit),
        ];
        ushort[] expectedPointers = [0x9b80, 0x9d80, 0x9f80, 0x9f80];
        for (int selection = 0; selection < equipment.Length; selection++)
        {
            ushort offset = SamusPaletteNative.SuitTableByteOffset(equipment[selection].ShownSuit());
            int source = SamusPaletteDefinitions.FullBodyCycles.SpeedBoostPointers + offset;
            ushort nativePointer = rom.ReadCartridgeWord(source);
            AssertEqual(expectedPointers[selection], nativePointer,
                $"Speed Booster pointer selection {selection} matches retail ROM");
            AssertEqual(nativePointer,
                SamusPaletteDefinitions.FullBodyCycles.SpeedBoostPalettePointer(offset),
                $"compiled Speed Booster pointer selection {selection} matches retail ROM");

            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                EquippedItems = equipment[selection],
                MetroidAttachmentPalette = SamusMetroidAttachmentPalette.SpeedBoostNext,
            };
            var installedColours = new SamusColours();
            AssertTrue(SamusSpecialSuperPalette.Update(installedColours, samus),
                "installed special palette loads Speed Booster shade without a CPU bus");
            for (int color = 0; color < SamusFullBodyCycleColorFormat.ColorsPerPalette; color++)
                AssertEqual((ushort)(ReadNativeSpeedBoosterShadeColor(rom, nativePointer, color) & 0x7fff),
                    installedColours.ShownSuit(InstalledFixtures.SamusShadeSources with { FullBodyCycles = cycleColors })[color],
                    $"Speed Booster CGRAM selection {selection}, color {color} matches ROM");
            AssertEqual(SamusMetroidAttachmentPalette.NormalSuitNext, samus.MetroidAttachmentPalette,
                "the Speed Booster palette alternates to the normal suit");
        }

        // Without an attacker the alternation leaves the palette alone.
        var unattached = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        AssertTrue(!SamusSpecialSuperPalette.Update(new SamusColours(), unattached),
            "no attachment, no alternation");
        AssertEqual(SamusMetroidAttachmentPalette.Inactive, unattached.MetroidAttachmentPalette,
            "an inactive alternation stays inactive");
        Console.WriteLine("Speed Booster palettes: four equipment cases name the Speed Booster shade, whose installed colours match ROM with no CPU bus.");
    }

    /// <summary>Reads one native full-body shade as the verification oracle for installed Speed Booster colors.</summary>
    private static ushort ReadNativeSpeedBoosterShadeColor(IImportCartridgeSource rom, int nativePointer, int color) =>
        RomDataReader.ReadWordFixedBank(rom, SamusSourceDefinitions.PaletteBank.Value | (nativePointer + color * sizeof(ushort)));

    /// <summary>Runs the Speed Booster palette pointer and CGRAM checks.</summary>
    [VerificationCommand("--speed-boost-palette-pointers")]
    internal static int SpeedBoostPalettePointersCommand()
    {
        Suite(nameof(VerifySpeedBoostPalettePointers), () => VerifySpeedBoostPalettePointers());
        return 0;
    }
}
