using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifySamusVisorColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Samus visor colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        SamusVisorColorCatalog catalog = SamusVisorColorCatalog.Load(
            new MemoryStream(SamusVisorColorExtractor.Extract(rom)));
        for (int index = 0; index < SamusVisorColorFormat.ColorCount; index++)
        {
            int address = SamusVisorColorFormat.SourceAddress + index * sizeof(ushort);
            ushort native = unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
            AssertEqual(native, catalog.Resolve(index), $"native visor color {index}");
        }

        VerifyInstalledRoomVisorCycle(rom, catalog);
        VerifyInstalledXrayVisorCycle(rom, catalog);
        AssertTrue(!catalog.TryResolveByteOffset(1, out _), "odd visor offsets remain cartridge reads");
        AssertTrue(!catalog.TryResolveByteOffset(12, out _), "adjacent visor bytes remain cartridge reads");
        AssertThrows<InvalidDataException>(() => SamusVisorColorCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt visor JSON fails loudly");
        AssertThrows<InvalidDataException>(() => SamusVisorColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate visor color property fails loudly");
        Console.WriteLine("  Samus visor: all six native colors and guarded room/X-ray cycles pass.");
    }

    private static void VerifyInstalledRoomVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = new SamusVisorPaletteState();
        var installed = new SamusVisorPaletteState { PresentationColors = catalog };
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = new ForbiddenVisorColorBus(rom);
        for (int frame = 0; frame < 32; frame++)
        {
            SamusVisorPaletteStepResult expected = native.Update(rom, nativeCgram, 0,
                LayerBlendingConfiguration.VisorBackdrop28);
            SamusVisorPaletteStepResult actual = installed.Update(guarded, installedCgram, 0,
                LayerBlendingConfiguration.VisorBackdrop28);
            AssertEqual(expected.Action, actual.Action, $"room visor frame {frame} action");
            AssertEqual(native.PackedTimerIndex, installed.PackedTimerIndex,
                $"room visor frame {frame} timer and index");
            AssertEqual(nativeCgram.Colors[196], installedCgram.Colors[196],
                $"room visor frame {frame} CGRAM");
        }
        AssertEqual(0, guarded.ForbiddenReads, "installed room cycle does not read visor ROM table");
    }

    private static void VerifyInstalledXrayVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = CreateXraySamus(rom);
        var installed = CreateXraySamus(rom);
        installed.Xray.PresentationColors = catalog;
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = new ForbiddenVisorColorBus(rom);

        void Compare(int call)
        {
            bool expected = native.Xray.UpdatePalette(rom, nativeCgram, native.EquippedItems);
            bool actual = installed.Xray.UpdatePalette(guarded, installedCgram, installed.EquippedItems);
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
        AssertEqual(0, guarded.ForbiddenReads, "installed X-ray cycle does not read visor ROM table");
    }

    private static SamusState CreateXraySamus(ISnesAddressSpace rom)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 100,
            YPosition = 200,
        };
        samus.RefreshCollisionRadii(rom);
        samus.InitializeAnimation(rom);
        AssertTrue(samus.Xray.TryBegin(rom, samus, SamusMovementType.Standing),
            "stock X-ray setup is accepted");
        return samus;
    }

    private static void VerifySamusVisorColorOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline, ISnesAddressSpace rom)
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
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(rom)
        {
            MapPresentation = edited,
        };
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        SamusState runtimeSamus = runtime.Samus ??
            throw new InvalidOperationException("Ceres initialization did not construct Samus.");
        AssertTrue(ReferenceEquals(edited.SamusVisorColors, runtimeSamus.VisorPalette.PresentationColors) &&
            ReferenceEquals(edited.SamusVisorColors, runtimeSamus.Xray.PresentationColors),
            "new Samus binds installed visor colors for both room and X-ray cycles");
        var visor = new SamusVisorPaletteState { PresentationColors = edited.SamusVisorColors };
        var cgram = new SnesCgram();
        var guarded = new ForbiddenVisorColorBus(new TestAddressSpace());
        _ = visor.Update(guarded, cgram, 0, LayerBlendingConfiguration.VisorBackdrop28);
        AssertEqual(edited.SamusVisorColors.Resolve(3), cgram.Colors[196],
            "edited visor color reaches room-cycle CGRAM");
        AssertEqual(0, guarded.ForbiddenReads, "edited visor color avoids native table");
        SamusState xraySamus = CreateXraySamus(rom);
        xraySamus.Xray.PresentationColors = edited.SamusVisorColors;
        var xrayCgram = new SnesCgram();
        var guardedXray = new ForbiddenVisorColorBus(rom);
        AssertTrue(xraySamus.Xray.UpdatePalette(guardedXray, xrayCgram, xraySamus.EquippedItems),
            "edited X-ray widening cycle writes CGRAM");
        AssertEqual(edited.SamusVisorColors.Resolve(0), xrayCgram.Colors[196],
            "edited visor color reaches X-ray CGRAM");
        AssertEqual(0, guardedXray.ForbiddenReads, "edited X-ray color avoids native table");
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

    private sealed class ForbiddenVisorColorBus(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public int ForbiddenReads { get; private set; }
        public byte ReadByte(int address)
        {
            if (address >= SamusVisorColorFormat.SourceAddress &&
                address < SamusVisorColorFormat.SourceAddress +
                    SamusVisorColorFormat.ColorCount * sizeof(ushort))
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed visor read native color ${address:X6}.");
            }
            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
