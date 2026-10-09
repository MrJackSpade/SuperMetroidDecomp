using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>Compares extracted visor colors with cartridge data and checks installed room and X-ray palette cycles.</summary>
    private static void VerifySamusVisorColors()
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
            ushort native = unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
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

    /// <summary>Builds an installed-color catalog from the six packed RGB5 words in the native visor table.</summary>
    /// <param name="rom">Address space containing the cartridge's visor color table.</param>
    /// <returns>A validated catalog equivalent to the native table.</returns>
    private static SamusVisorColorCatalog ReadOriginalVisorColors(ISnesAddressSpace rom)
    {
        var colors = new PaletteRgb5[SamusVisorColorFormat.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            int address = SamusVisorColorFormat.SourceAddress + index * sizeof(ushort);
            ushort word = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
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

    /// <summary>Compares native and installed room-visor countdowns and CGRAM writes while guarding the installed path from table reads.</summary>
    /// <param name="rom">Retail address space used by the native reference cycle and color oracle.</param>
    /// <param name="catalog">Extracted visor colors used by the installed palette state.</param>
    private static void VerifyInstalledRoomVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = new SamusVisorPaletteState { PresentationColors = ReadOriginalVisorColors(rom) };
        var installed = new SamusVisorPaletteState { PresentationColors = catalog };
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = new ForbiddenVisorColorBus(rom);
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
                AssertEqual(ReadVisorFixtureColor(rom, sourceOffset), installedCgram.Colors[196],
                    $"room visor frame {frame} matches native color word");
            AssertEqual(expectedWrite, actualWrite, $"room visor frame {frame} action");
            AssertEqual(native.PackedTimerIndex, installed.PackedTimerIndex,
                $"room visor frame {frame} timer and index");
            AssertEqual(nativeCgram.Colors[196], installedCgram.Colors[196],
                $"room visor frame {frame} CGRAM");
        }
        AssertEqual(0, guarded.ForbiddenReads, "installed room cycle does not read visor ROM table");
    }

    /// <summary>Compares native and installed X-ray visor palette cycles through widening and full-beam stages.</summary>
    /// <param name="rom">Retail address space used to initialize Samus and run the native reference cycle.</param>
    /// <param name="catalog">Extracted visor colors used by the installed X-ray palette state.</param>
    private static void VerifyInstalledXrayVisorCycle(ISnesAddressSpace rom,
        SamusVisorColorCatalog catalog)
    {
        var native = CreateXraySamus(rom);
        native.Xray.PresentationColors = ReadOriginalVisorColors(rom);
        var installed = CreateXraySamus(rom);
        installed.Xray.PresentationColors = catalog;
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = new ForbiddenVisorColorBus(rom);

        void Compare(int call)
        {
            // This fixture enters the full-beam stage immediately before call 11;
            // native $91:DCEA restarts that stage at color byte offset six.
            ushort sourceOffset = call == 11 ? (ushort)6 : native.Xray.SpecialPaletteFrame;
            bool expected = native.Xray.UpdatePalette(rom, nativeCgram, native.EquippedItems);
            bool actual = installed.Xray.UpdatePalette(guarded, installedCgram, installed.EquippedItems);
            if (expected)
                AssertEqual(ReadVisorFixtureColor(rom, sourceOffset), installedCgram.Colors[196],
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
        AssertEqual(0, guarded.ForbiddenReads, "installed X-ray cycle does not read visor ROM table");
    }

    /// <summary>Reads one packed color word at a byte offset within the native visor table.</summary>
    /// <param name="rom">Address space containing the cartridge table.</param>
    /// <param name="offset">Byte offset of the little-endian color word.</param>
    /// <returns>The packed SNES RGB5 color.</returns>
    private static ushort ReadVisorFixtureColor(ISnesAddressSpace rom, int offset) =>
        (ushort)(rom.ReadByte(SamusVisorColorFormat.SourceAddress + offset) |
            rom.ReadByte(SamusVisorColorFormat.SourceAddress + offset + 1) << 8);
    /// <summary>Creates Samus in the standing X-ray pose and initializes the stock beam setup.</summary>
    /// <param name="rom">Address space used for collision, animation, and X-ray initialization data.</param>
    /// <returns>The initialized Samus state with an active X-ray beam.</returns>
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

    /// <summary>Checks that an edited visor-color asset changes installed identity and reaches both room and X-ray CGRAM cycles.</summary>
    /// <param name="stock">Directory containing the stock presentation assets.</param>
    /// <param name="overrides">Directory where the temporary visor override is written.</param>
    /// <param name="baseline">Unmodified presentation catalog used to compare content identity.</param>
    /// <param name="rom">Retail address space used to initialize runtime and native X-ray fixtures.</param>
    /// <param name="initialPalettes">Initial gameplay palettes supplied to the constructed runtime.</param>
    /// <param name="fixtureAssets">Room assets used to bind the runtime fixture.</param>
    private static void VerifySamusVisorColorOverride(string stock, string overrides,
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
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(rom,
            initialPaletteArt: initialPalettes)
        {
            MapPresentation = edited,
        };
        fixtureAssets.Bind(runtime);
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

    /// <summary>Address-space proxy that fails and counts reads of the native visor table while forwarding other access.</summary>
    /// <param name="inner">Underlying bus used for permitted reads and all writes.</param>
    private sealed class ForbiddenVisorColorBus(ISnesAddressSpace inner) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads within the protected native visor-color table range.</summary>
        public int ForbiddenReads { get; private set; }

        /// <summary>Routes cartridge imports through the native-table read guard.</summary>
        /// <param name="address">Cartridge address being read.</param>
        /// <returns>The permitted byte from the underlying bus.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Counts and rejects reads of native visor colors; forwards all other addresses.</summary>
        /// <param name="address">Address requested from the bus.</param>
        /// <returns>The byte from the underlying bus when the address is outside the protected table.</returns>
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

        /// <summary>Forwards a write to the underlying address space.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
