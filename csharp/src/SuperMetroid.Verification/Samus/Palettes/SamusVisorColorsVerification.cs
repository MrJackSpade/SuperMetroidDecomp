using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Verification.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Samus.Palettes;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Checks installed Samus visor RGB5 colors through room and X-ray palette cycles.</summary>
internal static class SamusVisorColorsVerification
{
    /// <summary>Compares six extracted colors with the cartridge and verifies guarded room and X-ray cycling.</summary>
    internal static void VerifySamusVisorColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Samus visor colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        InstalledSamusVisorColors catalog = SamusVisorColorFile.Read(
            new MemoryStream(SamusVisorColorExtractor.Extract(rom)));
        for (int index = 0; index < SamusVisorColorFormat.ColorCount; index++)
        {
            int address = SamusSourceDefinitions.VisorColors.Value + index * sizeof(ushort);
            ushort native = rom.ReadCartridgeWord(address);
            AssertEqual(native, catalog.Resolve(index), $"native visor color {index}");
        }

        Suite(nameof(VerifyInstalledRoomVisorCycle), () => VerifyInstalledRoomVisorCycle(rom, catalog));
        Suite(nameof(VerifyInstalledXrayVisorCycle), () => VerifyInstalledXrayVisorCycle(rom, catalog));
        AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(SamusVisorColorFormat.ColorCount), "shades past the last are outside the installed color domain");
        AssertThrows<InvalidDataException>(() => SamusVisorColorFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt visor JSON fails loudly");
        AssertThrows<InvalidDataException>(() => SamusVisorColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate visor color property fails loudly");
        Console.WriteLine("  Samus visor: all six native colors and guarded room/X-ray cycles pass.");
    }

    /// <summary>Builds an independent color catalog from the six native visor table words.</summary>
    private static InstalledSamusVisorColors ReadOriginalVisorColors(ISnesAddressSpace rom)
    {
        var colors = new PaletteRgb5[SamusVisorColorFormat.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            int address = SamusSourceDefinitions.VisorColors.Value + index * sizeof(ushort);
            ushort word = rom.ReadCartridgeWord(address);
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = (word >> 5) & 31,
                Blue = (word >> 10) & 31,
            };
        }
        return SamusVisorColorFile.Read(new MemoryStream(SamusVisorColorFile.Write(
            new SamusVisorColorDocument { Version = SamusVisorColorFile.Version, Colors = colors })));
    }

    /// <summary>Compares 32 room updates for timer, source offset, and CGRAM slot 196 without installed table reads.</summary>
    private static void VerifyInstalledRoomVisorCycle(ISnesAddressSpace rom,
        InstalledSamusVisorColors catalog)
    {
        // Both cycles name shades (#1512); the import resolves the native one from the cartridge's table and the
        // installed one from the catalog.
        var native = new SamusVisorShadeState();
        var installed = new SamusVisorShadeState();
        var nativeSources = InstalledFixtures.SamusShadeSources with { Visor = ReadOriginalVisorColors(rom) };
        var installedSources = InstalledFixtures.SamusShadeSources with { Visor = catalog };
        var nativeColours = new SamusColours();
        var installedColours = new SamusColours();
        var guarded = NewForbiddenVisorColorBus(rom);
        for (int frame = 0; frame < 32; frame++)
        {
            native.Update(nativeColours, 0,
                RoomVisorAnimation.Animated);
            installed.Update(installedColours, 0,
                RoomVisorAnimation.Animated);
            // `$91:D864` reloads the expired countdown to the frame delay only on the call
            // that writes a color; countdown calls leave it below that delay.
            bool expectedWrite = native.Cycle.UpdatesLeft == SamusShadeDefinitionsVisorConstants.FrameDelay;
            bool actualWrite = installed.Cycle.UpdatesLeft == SamusShadeDefinitionsVisorConstants.FrameDelay;
            // The room's cycle is stored after the widening glow's shades.
            if (expectedWrite)
                AssertEqual(rom.ReadCartridgeWord(SamusSourceDefinitions.VisorColors.Value +
                        (SamusShadeDefinitions.Visor.XrayWideningShades + native.Cycle.Showing.Step) * sizeof(ushort)), installedColours.ShownVisor(installedSources),
                    $"room visor frame {frame} matches native color word");
            AssertEqual(expectedWrite, actualWrite, $"room visor frame {frame} action");
            AssertEqual(VerifySamusVisorPaletteCase.Of(native), VerifySamusVisorPaletteCase.Of(installed),
                $"room visor frame {frame} timer and index");
            AssertEqual(nativeColours.VisorShade, installedColours.VisorShade,
                $"room visor frame {frame} shade");
            AssertEqual(nativeColours.ShownVisor(nativeSources), installedColours.ShownVisor(installedSources),
                $"room visor frame {frame} colour");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed room cycle does not read visor ROM table");
    }

    /// <summary>Compares widening and full-beam X-ray color cycles, including the offset-six restart.</summary>
    private static void VerifyInstalledXrayVisorCycle(ISnesAddressSpace rom,
        InstalledSamusVisorColors catalog)
    {
        var native = CreateXraySamus();
        var installed = CreateXraySamus();
        var nativeSources = InstalledFixtures.SamusShadeSources with { Visor = ReadOriginalVisorColors(rom) };
        var installedSources = InstalledFixtures.SamusShadeSources with { Visor = catalog };
        var nativeColours = new SamusColours();
        var installedColours = new SamusColours();
        var guarded = NewForbiddenVisorColorBus(rom);

        // Check one X-ray palette update and its resulting source offset and CGRAM color.
        void Compare(int call)
        {
            // This fixture enters the full-beam stage immediately before call 11;
            // native $91:DCEA restarts that stage at the full-beam cycle's first shade.
            bool expected = native.Xray.UpdateColouring(nativeColours, native.EquippedItems);
            bool actual = installed.Xray.UpdateColouring(installedColours, installed.EquippedItems);
            if (expected)
            {
                // The stored colours hold the widening glow's shades, then the steady cycle's.
                SuperMetroid.Core.Assets.PictureShade shown = native.Xray.Visor.Showing;
                int sourceShade = shown.Effect == SamusShades.VisorCycle[0].Effect
                    ? SamusShadeDefinitions.Visor.XrayWideningShades + shown.Step
                    : shown.Step;
                AssertEqual(rom.ReadCartridgeWord(SamusSourceDefinitions.VisorColors.Value + sourceShade * sizeof(ushort)), installedColours.ShownVisor(installedSources),
                    $"X-ray visor call {call} matches native color word");
            }
            AssertEqual(expected, actual, $"X-ray visor call {call} write");
            AssertEqual(native.Xray.Visor.Shown, installed.Xray.Visor.Shown,
                $"X-ray visor call {call} sequence shade");
            AssertEqual(nativeColours.VisorShade, installedColours.VisorShade,
                $"X-ray visor call {call} shade");
            AssertEqual(nativeColours.ShownVisor(nativeSources), installedColours.ShownVisor(installedSources),
                $"X-ray visor call {call} colour");
        }

        for (int call = 0; call < 11; call++) Compare(call); // Widening colors 0, 2, 4.
        for (int stage = 0; stage < 36; stage++)
        {
            native.Xray.StepBeam(native, GameplayAction.Dash);
            installed.Xray.StepBeam(installed, GameplayAction.Dash);
        }
        AssertEqual(XrayBeamPhase.Full, native.Xray.BeamPhase,
            "X-ray reaches full-beam palette cycle after native setup and widening");
        for (int call = 11; call < 31; call++) Compare(call); // Full-beam colors 6, 8, 10.
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed X-ray cycle does not read visor ROM table");
    }
    /// <summary>Initializes a standing Samus actor and enters the stock X-ray beam sequence.</summary>
    private static SamusState CreateXraySamus()
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Xray.TryBeginAndActivate(samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay, PowerBombExplosionActive = false, ProjectileCooldownTimer = 0, BombCounter = 0 }),
            "stock X-ray setup is accepted");
        return samus;
    }

    /// <summary>Checks that the visor colour codec rejects out-of-range colours.</summary>
    internal static void VerifySamusVisorColorsRejectMalformedDocuments(string stock)
    {
        SamusVisorColorDocument document = JsonSerializer.Deserialize<SamusVisorColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusVisorColorFile.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Samus visor color document is null.");
        AssertThrows<InvalidDataException>(() => SamusVisorColorFile.Read(new MemoryStream(SamusVisorColorFile.Write(document with
        {
            Colors = [.. document.Colors.Take(3), document.Colors[3] with { Red = 32 }, .. document.Colors.Skip(4)],
        }))), "out-of-range visor color fails loudly");
    }

    /// <summary>Forbids installed room or X-ray palette updates from reading the native six-color source table.</summary>
    private static CartridgeReadGuard NewForbiddenVisorColorBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address >= SamusSourceDefinitions.VisorColors.Value &&
                address < SamusSourceDefinitions.VisorColors.Value +
                    SamusVisorColorFormat.ColorCount * sizeof(ushort),
            address => $"Installed visor read native color ${address:X6}.");

    /// <summary>Runs the cartridge-backed Samus visor color verification.</summary>
    [VerificationCommand("--samus-visor-colors")]
    internal static int SamusVisorColorsCommand()
    {
        Suite(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors());
        return 0;
    }
}
