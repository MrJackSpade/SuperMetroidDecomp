using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Verification.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Palettes;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

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

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        SamusVisorColorCatalog catalog = SamusVisorColorFile.Read(
            new MemoryStream(SamusVisorColorExtractor.Extract(rom)));
        for (int index = 0; index < SamusVisorColorFormat.ColorCount; index++)
        {
            int address = SamusSourceDefinitions.VisorColors.Value + index * sizeof(ushort);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, catalog.Resolve(index), $"native visor color {index}");
        }

        Suite(nameof(VerifyInstalledRoomVisorCycle), () => VerifyInstalledRoomVisorCycle(rom, catalog));
        Suite(nameof(VerifyInstalledXrayVisorCycle), () => VerifyInstalledXrayVisorCycle(rom, catalog));
        AssertTrue(!catalog.TryResolveByteOffset(1, out _), "odd visor offsets are outside the installed color domain");
        AssertTrue(!catalog.TryResolveByteOffset(12, out _), "adjacent visor bytes are outside the installed color domain");
        AssertThrows<InvalidDataException>(() => SamusVisorColorFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt visor JSON fails loudly");
        AssertThrows<InvalidDataException>(() => SamusVisorColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate visor color property fails loudly");
        Console.WriteLine("  Samus visor: all six native colors and guarded room/X-ray cycles pass.");
    }

    /// <summary>Builds an independent color catalog from the six native visor table words.</summary>
    private static SamusVisorColorCatalog ReadOriginalVisorColors(ISnesAddressSpace rom)
    {
        var colors = new PaletteRgb5[SamusVisorColorFormat.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            int address = SamusSourceDefinitions.VisorColors.Value + index * sizeof(ushort);
            ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = (word >> 5) & 31,
                Blue = (word >> 10) & 31,
            };
        }
        return SamusVisorColorFile.Read(new MemoryStream(SamusVisorColorFile.Write(
            new SamusVisorColorDocument { Version = SamusVisorColorFormat.Version, Colors = colors })));
    }

    /// <summary>Compares 32 room updates for timer, source offset, and CGRAM slot 196 without installed table reads.</summary>
    private static void VerifyInstalledRoomVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = new SamusVisorPaletteState { PresentationColors = ReadOriginalVisorColors(rom) };
        var installed = new SamusVisorPaletteState { PresentationColors = catalog };
        var nativeCgram = new SamusColourRig();
        var installedCgram = new SamusColourRig();
        var guarded = NewForbiddenVisorColorBus(rom);
        for (int frame = 0; frame < 32; frame++)
        {
            int sourceOffset = native.PaletteByteOffset;
            native.Update(nativeCgram.Samus, 0,
                LayerBlendingConfiguration.VisorBackdrop28);
            installed.Update(installedCgram.Samus, 0,
                LayerBlendingConfiguration.VisorBackdrop28);
            // `$91:D864` reloads the expired countdown to the frame delay only on the call
            // that writes a color; countdown calls leave it below that delay.
            bool expectedWrite = native.Timer == SamusPaletteDefinitions.Visor.FrameDelay;
            bool actualWrite = installed.Timer == SamusPaletteDefinitions.Visor.FrameDelay;
            if (expectedWrite)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusSourceDefinitions.VisorColors.Value + sourceOffset), installedCgram.Colors[196],
                    $"room visor frame {frame} matches native color word");
            AssertEqual(expectedWrite, actualWrite, $"room visor frame {frame} action");
            AssertEqual(new VerifySamusVisorPaletteCase(native.Timer, native.PaletteByteOffset), new VerifySamusVisorPaletteCase(installed.Timer, installed.PaletteByteOffset),
                $"room visor frame {frame} timer and index");
            AssertEqual(nativeCgram.Colors[196], installedCgram.Colors[196],
                $"room visor frame {frame} CGRAM");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed room cycle does not read visor ROM table");
    }

    /// <summary>Compares widening and full-beam X-ray color cycles, including the offset-six restart.</summary>
    private static void VerifyInstalledXrayVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = CreateXraySamus();
        native.Xray.PresentationColors = ReadOriginalVisorColors(rom);
        var installed = CreateXraySamus();
        installed.Xray.PresentationColors = catalog;
        var nativeCgram = new SamusColourRig();
        var installedCgram = new SamusColourRig();
        var guarded = NewForbiddenVisorColorBus(rom);

        // Check one X-ray palette update and its resulting source offset and CGRAM color.
        void Compare(int call)
        {
            // This fixture enters the full-beam stage immediately before call 11;
            // native $91:DCEA restarts that stage at color byte offset six.
            ushort sourceOffset = call == 11 ? (ushort)6 : checked((ushort)native.Xray.SpecialPaletteFrame);
            bool expected = native.Xray.UpdatePalette(nativeCgram.Samus, native.EquippedItems);
            bool actual = installed.Xray.UpdatePalette(installedCgram.Samus, installed.EquippedItems);
            if (expected)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusSourceDefinitions.VisorColors.Value + sourceOffset), installedCgram.Colors[196],
                    $"X-ray visor call {call} matches native color word");
            AssertEqual(expected, actual, $"X-ray visor call {call} write");
            AssertEqual(native.Xray.SpecialPaletteFrame, installed.Xray.SpecialPaletteFrame,
                $"X-ray visor call {call} next offset");
            AssertEqual(nativeCgram.Colors[196], installedCgram.Colors[196],
                $"X-ray visor call {call} CGRAM");
        }

        for (int call = 0; call < 11; call++) Compare(call); // Widening colors 0, 2, 4.
        for (int stage = 0; stage < 36; stage++)
        {
            native.Xray.StepBeam(native, ControllerButton.B);
            installed.Xray.StepBeam(installed, ControllerButton.B);
        }
        AssertEqual(XrayBeamPhase.Full, native.Xray.BeamPhase,
            "X-ray reaches full-beam palette cycle after native setup and widening");
        for (int call = 11; call < 31; call++) Compare(call); // Full-beam colors 6, 8, 10.
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed X-ray cycle does not read visor ROM table");
    }
    /// <summary>Initializes a standing Samus actor and enters the stock X-ray beam sequence.</summary>
    private static SamusState CreateXraySamus()
    {
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Xray.TryBegin(samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
            "stock X-ray setup is accepted");
        return samus;
    }

    /// <summary>Checks edited visor colors through newly bound Samus room/X-ray states and stock identity restoration.</summary>
    internal static void VerifySamusVisorColorOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline, ISnesAddressSpace rom,
        GameplayBasePaletteCatalog initialPalettes, MapPresentationInstalledRoomAssets fixtureAssets)
    {
        string path = Path.Combine(overrides, SamusVisorColorFormat.FileName);
        SamusVisorColorDocument document = JsonSerializer.Deserialize<SamusVisorColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusVisorColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Samus visor color document is null.");
        PaletteRgb5 original = document.Colors[3];
        PaletteRgb5 originalXray = document.Colors[0];
        document.Colors[0] = originalXray with { Red = (originalXray.Red + 1) % 32 };
        document.Colors[3] = original with { Red = (original.Red + 1) % 32 };
        File.WriteAllBytes(path, SamusVisorColorFile.Write(document));
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "visor color override changes installed content identity");
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(new SuperMetroid.Core.Gameplay.HostGameplayOverrides(), initialPaletteArt: initialPalettes);
        runtime.Artwork.MapPresentation = edited;
        fixtureAssets.Bind(runtime);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(controller1Input: 0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        SamusState runtimeSamus = runtime.Player.Samus ??
            throw new InvalidOperationException("Ceres initialization did not construct Samus.");
        AssertTrue(ReferenceEquals(edited.SamusVisorColors, runtimeSamus.VisorPalette.PresentationColors) &&
            ReferenceEquals(edited.SamusVisorColors, runtimeSamus.Xray.PresentationColors),
            "new Samus binds installed visor colors for both room and X-ray cycles");
        var visor = new SamusVisorPaletteState { PresentationColors = edited.SamusVisorColors };
        var cgram = new SamusColourRig();
        var guarded = NewForbiddenVisorColorBus(new TestAddressSpace());
        _ = visor.Update(cgram.Samus, 0, LayerBlendingConfiguration.VisorBackdrop28);
        AssertEqual(edited.SamusVisorColors.Resolve(3), cgram.Colors[196],
            "edited visor color reaches room-cycle CGRAM");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "edited visor color avoids native table");
        SamusState xraySamus = CreateXraySamus();
        xraySamus.Xray.PresentationColors = edited.SamusVisorColors;
        var xrayCgram = new SamusColourRig();
        var guardedXray = NewForbiddenVisorColorBus(rom);
        AssertTrue(xraySamus.Xray.UpdatePalette(xrayCgram.Samus, xraySamus.EquippedItems),
            "edited X-ray widening cycle writes CGRAM");
        AssertEqual(edited.SamusVisorColors.Resolve(0), xrayCgram.Colors[196],
            "edited visor color reaches X-ray CGRAM");
        AssertEqual(0, guardedXray.ForbiddenReadAttempts, "edited X-ray color avoids native table");
        AssertThrows<InvalidDataException>(() => SamusVisorColorFile.Write(document with
        {
            Colors = [.. document.Colors.Take(3), original with { Red = 32 }, .. document.Colors.Skip(4)],
        }), "out-of-range visor color fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = MapPresentationFiles.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing visor override restores stock content identity");
        Console.WriteLine("Samus visor override: edited room/X-ray colors reach CGRAM and stock restores.");
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
