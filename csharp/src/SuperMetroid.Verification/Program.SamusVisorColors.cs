using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static AssertionsVerification;
using static MapPresentationVerification;
using static SuiteTimingVerification;

internal static class SamusVisorColorsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus visor colors")]
    internal static void VerifySamusVisorColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Samus visor colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        SamusVisorColorCatalog catalog = SamusVisorColorCatalog.Load(
            new MemoryStream(SamusVisorColorExtractor.Extract(rom)));
        for (int index = 0; index < SamusVisorColorFormat.ColorCount; index++)
        {
            int address = SamusVisorColorFormat.SourceAddress + index * sizeof(ushort);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, catalog.Resolve(index), $"native visor color {index}");
        }

        Suite(nameof(VerifyInstalledRoomVisorCycle), () => VerifyInstalledRoomVisorCycle(rom, catalog));
        Suite(nameof(VerifyInstalledXrayVisorCycle), () => VerifyInstalledXrayVisorCycle(rom, catalog));
        AssertTrue(!catalog.TryResolveByteOffset(1, out _), "odd visor offsets are outside the installed color domain");
        AssertTrue(!catalog.TryResolveByteOffset(12, out _), "adjacent visor bytes are outside the installed color domain");
        AssertThrows<InvalidDataException>(() => SamusVisorColorCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt visor JSON fails loudly");
        AssertThrows<InvalidDataException>(() => SamusVisorColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate visor color property fails loudly");
        Console.WriteLine("  Samus visor: all six native colors and guarded room/X-ray cycles pass.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for original visor colors")]
    private static SamusVisorColorCatalog ReadOriginalVisorColors(ISnesAddressSpace rom)
    {
        var colors = new PaletteRgb5[SamusVisorColorFormat.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            int address = SamusVisorColorFormat.SourceAddress + index * sizeof(ushort);
            ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = (word >> 5) & 31,
                Blue = (word >> 10) & 31,
            };
        }
        return SamusVisorColorCatalog.Load(new MemoryStream(SamusVisorColorCatalog.Write(
            new SamusVisorColorDocument { Version = SamusVisorColorFormat.Version, Colors = colors })));
    }

    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void VerifyInstalledRoomVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = new SamusVisorPaletteState { PresentationColors = ReadOriginalVisorColors(rom) };
        var installed = new SamusVisorPaletteState { PresentationColors = catalog };
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = NewForbiddenVisorColorBus(rom);
        for (int frame = 0; frame < 32; frame++)
        {
            byte sourceOffset = native.PaletteByteOffset;
            native.Update(rom, nativeCgram, 0,
                LayerBlendingConfiguration.VisorBackdrop28);
            installed.Update(guarded, installedCgram, 0,
                LayerBlendingConfiguration.VisorBackdrop28);
            // `$91:D864` reloads the expired countdown to the frame delay only on the call
            // that writes a color; countdown calls leave it below that delay.
            bool expectedWrite = native.Timer == SamusPaletteRomData.Visor.FrameDelay;
            bool actualWrite = installed.Timer == SamusPaletteRomData.Visor.FrameDelay;
            if (expectedWrite)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusVisorColorFormat.SourceAddress + sourceOffset), installedCgram.Colors[196],
                    $"room visor frame {frame} matches native color word");
            AssertEqual(expectedWrite, actualWrite, $"room visor frame {frame} action");
            AssertEqual(native.PackedTimerIndex, installed.PackedTimerIndex,
                $"room visor frame {frame} timer and index");
            AssertEqual(nativeCgram.Colors[196], installedCgram.Colors[196],
                $"room visor frame {frame} CGRAM");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed room cycle does not read visor ROM table");
    }

    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void VerifyInstalledXrayVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = CreateXraySamus(rom);
        native.Xray.PresentationColors = ReadOriginalVisorColors(rom);
        var installed = CreateXraySamus(rom);
        installed.Xray.PresentationColors = catalog;
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = NewForbiddenVisorColorBus(rom);

        void Compare(int call)
        {
            // This fixture enters the full-beam stage immediately before call 11;
            // native $91:DCEA restarts that stage at color byte offset six.
            ushort sourceOffset = call == 11 ? (ushort)6 : native.Xray.SpecialPaletteFrame;
            bool expected = native.Xray.UpdatePalette(rom, nativeCgram, native.EquippedItems);
            bool actual = installed.Xray.UpdatePalette(guarded, installedCgram, installed.EquippedItems);
            if (expected)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(SamusVisorColorFormat.SourceAddress + sourceOffset), installedCgram.Colors[196],
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
            native.Xray.StepBeam(rom, native, (ushort)SnesButton.B);
            installed.Xray.StepBeam(rom, installed, (ushort)SnesButton.B);
        }
        AssertEqual(XrayBeamPhase.Full, native.Xray.BeamPhase,
            "X-ray reaches full-beam palette cycle after native setup and widening");
        for (int call = 11; call < 31; call++) Compare(call); // Full-beam colors 6, 8, 10.
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed X-ray cycle does not read visor ROM table");
    }
    private static SamusState CreateXraySamus(ISnesAddressSpace rom)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 200)),
        };
        samus.PoseMetadata.RefreshCollisionRadii(rom);
        samus.Presentation.InitializeAnimation(rom);
        AssertTrue(samus.Xray.TryBegin(rom, samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = (ushort)SuperMetroidGameState.MainGameplay }),
            "stock X-ray setup is accepted");
        return samus;
    }

    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifySamusVisorColorOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline, ISnesAddressSpace rom,
        GameplayBasePaletteCatalog initialPalettes, MapPresentationInstalledRoomAssets fixtureAssets)
    {
        string path = Path.Combine(overrides, SamusVisorColorFormat.FileName);
        SamusVisorColorDocument document = JsonSerializer.Deserialize<SamusVisorColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusVisorColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Samus visor color document is null.");
        PaletteRgb5 original = document.Colors[3];
        PaletteRgb5 originalXray = document.Colors[0];
        document.Colors[0] = originalXray with { Red = (originalXray.Red + 1) % 32 };
        document.Colors[3] = original with { Red = (original.Red + 1) % 32 };
        File.WriteAllBytes(path, SamusVisorColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "visor color override changes installed content identity");
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(rom, new SuperMetroid.Core.Runtime.HostGameplayOverrides(), initialPaletteArt: initialPalettes)
        {
            Artwork = { MapPresentation = edited },
        };
        fixtureAssets.Bind(runtime);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.NmiHandler.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        SamusState runtimeSamus = runtime.Player.Samus ??
            throw new InvalidOperationException("Ceres initialization did not construct Samus.");
        AssertTrue(ReferenceEquals(edited.SamusVisorColors, runtimeSamus.VisorPalette.PresentationColors) &&
            ReferenceEquals(edited.SamusVisorColors, runtimeSamus.Xray.PresentationColors),
            "new Samus binds installed visor colors for both room and X-ray cycles");
        var visor = new SamusVisorPaletteState { PresentationColors = edited.SamusVisorColors };
        var cgram = new SnesCgram();
        var guarded = NewForbiddenVisorColorBus(new TestAddressSpace());
        _ = visor.Update(guarded, cgram, 0, LayerBlendingConfiguration.VisorBackdrop28);
        AssertEqual(edited.SamusVisorColors.Resolve(3), cgram.Colors[196],
            "edited visor color reaches room-cycle CGRAM");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "edited visor color avoids native table");
        SamusState xraySamus = CreateXraySamus(rom);
        xraySamus.Xray.PresentationColors = edited.SamusVisorColors;
        var xrayCgram = new SnesCgram();
        var guardedXray = NewForbiddenVisorColorBus(rom);
        AssertTrue(xraySamus.Xray.UpdatePalette(guardedXray, xrayCgram, xraySamus.EquippedItems),
            "edited X-ray widening cycle writes CGRAM");
        AssertEqual(edited.SamusVisorColors.Resolve(0), xrayCgram.Colors[196],
            "edited visor color reaches X-ray CGRAM");
        AssertEqual(0, guardedXray.ForbiddenReadAttempts, "edited X-ray color avoids native table");
        AssertThrows<InvalidDataException>(() => SamusVisorColorCatalog.Write(document with
        {
            Colors = [.. document.Colors.Take(3), original with { Red = 32 }, .. document.Colors.Skip(4)],
        }), "out-of-range visor color fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing visor override restores stock content identity");
        Console.WriteLine("Samus visor override: edited room/X-ray colors reach CGRAM and stock restores.");
    }

    private static CartridgeReadGuard NewForbiddenVisorColorBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= SamusVisorColorFormat.SourceAddress and
                < (SamusVisorColorFormat.SourceAddress +
                    SamusVisorColorFormat.ColorCount * sizeof(ushort)),
            address => $"Installed visor read native color ${address:X6}.");

    [VerificationCommand("--samus-visor-colors")]
    internal static int SamusVisorColorsCommand()
    {
        Suite(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors());
        return 0;
    }
}
