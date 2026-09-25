using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingPaletteArtwork(GameInstallation installation)
    {
        EndingPaletteCatalog stock = installation.LoadEndingPalettes();
        var nativeBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
        {
            byte[] native = EndingPaletteArtworkFiles.ReadNativePalette(nativeBus, id,
                EndingPaletteDefinitions.ColorCount(id));
            AssertTrue(stock[id].Transfer.Span.SequenceEqual(native),
                $"installed ending {id} palette preserves every cartridge color byte");
        }

        var guardedBus = new EndingPaletteSourceReadGuard(
            SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"));
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var nativeState = new EndingCreditsState(nativeBus, nativeAudio, 2, 59);
        var installedState = new EndingCreditsState(guardedBus, installedAudio, 2, 59);
        installedState.BindPaletteArtwork(stock);
        CreditsPresentation credits = installation.LoadMaps().StaffCredits;
        nativeState.BindStaffCredits(credits);
        installedState.BindStaffCredits(credits);
        var reached = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 60_000 &&
            nativeState.Phase != EndingCreditsPhase.SeeYouNextMission; frame++)
        {
            AssertEqual(nativeState.Phase, installedState.Phase,
                $"installed ending palettes preserve phase at frame {frame}");
            bool first = reached.Add(nativeState.Phase);
            if (first || frame % 173 == 0)
            {
                LayeredRenderSnapshot expected = nativeState.CaptureRenderSnapshot();
                LayeredRenderSnapshot actual = installedState.CaptureRenderSnapshot();
                AssertTrue(actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                        actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                        actual.Brightness == expected.Brightness,
                    $"installed ending palettes preserve native PPU state at frame {frame}");
            }
            nativeState.Step();
            installedState.Step();
            nativeAudio.AdvanceFrame(nativeBus, default);
            installedAudio.AdvanceFrame(guardedBus, default);
        }
        AssertEqual(EndingCreditsPhase.SeeYouNextMission, nativeState.Phase,
            "native ending reaches final hold in palette parity fixture");
        AssertEqual(nativeState.Phase, installedState.Phase,
            "installed ending reaches the same final hold");
        foreach (EndingCreditsPhase phase in new[]
        {
            EndingCreditsPhase.WaitForEscapeMusic,
            EndingCreditsPhase.FadeInZebesExplosion,
            EndingCreditsPhase.ZebesExplosionTileUpload,
            EndingCreditsPhase.Credits,
            EndingCreditsPhase.PostCreditsBlank,
            EndingCreditsPhase.OperationSuccessfulText,
            EndingCreditsPhase.PostCreditsWhiteFlash,
        })
            AssertTrue(reached.Contains(phase),
                $"ending palette parity exercises {phase}");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed ending never rereads static or logo palette ROM colors");
        VerifyEndingLogoPaletteArtwork(stock, nativeBus, guardedBus);

        Directory.CreateDirectory(installation.EndingPaletteOverrideDirectory);
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
        {
            string name = EndingPaletteDefinitions.FileName(id);
            string stockPath = Path.Combine(installation.EndingPaletteDirectory, name);
            string overridePath = Path.Combine(installation.EndingPaletteOverrideDirectory, name);
            EndingPaletteDocument document = JsonSerializer.Deserialize<EndingPaletteDocument>(
                File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException($"Ending {id} palette JSON is empty.");
            int changedColor = id switch
            {
                EndingPaletteId.Explosion => 128,
                EndingPaletteId.PostCredits => 4,
                _ => 0,
            };
            document.Colors[changedColor] = document.Colors[changedColor] with
            {
                Red = (document.Colors[changedColor].Red + 17) & 31,
            };
            using (var output = File.Create(overridePath))
                EndingPalette.Write(output, id, document);
            EndingPaletteCatalog edited = installation.LoadEndingPalettes();
            ReadOnlySpan<byte> original = stock[id].Transfer.Span;
            ReadOnlySpan<byte> changed = edited[id].Transfer.Span;
            int byteOffset = changedColor * sizeof(ushort);
            AssertTrue(!original.SequenceEqual(changed) &&
                    original[..byteOffset].SequenceEqual(changed[..byteOffset]) &&
                    original[(byteOffset + sizeof(ushort))..].SequenceEqual(
                        changed[(byteOffset + sizeof(ushort))..]),
                $"ending {id} override changes only its selected RGB5 color");
            if (id == EndingPaletteId.Escape)
            {
                var editedAudio = new CartridgeAudioState();
                var editedState = new EndingCreditsState(guardedBus, editedAudio, 2, 59);
                editedState.BindPaletteArtwork(edited);
                var stockAudio = new CartridgeAudioState();
                var stockState = new EndingCreditsState(nativeBus, stockAudio, 2, 59);
                editedState.Step();
                stockState.Step();
                AssertTrue(!editedState.CaptureRenderSnapshot().Memory.Cgram.SequenceEqual(
                        stockState.CaptureRenderSnapshot().Memory.Cgram) &&
                    editedState.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(
                        stockState.CaptureRenderSnapshot().Memory.Vram),
                    "edited escape palette reaches the active CGRAM image");
            }
            if (id is EndingPaletteId.LogoInitial or EndingPaletteId.LogoCrossfade)
            {
                var stockCgram = new SnesCgram();
                var editedCgram = new SnesCgram();
                var stockLogo = new EndingLogo(guardedBus, stockCgram, () => { }, stock);
                var editedLogo = new EndingLogo(guardedBus, editedCgram, () => { }, edited);
                if (id == EndingPaletteId.LogoCrossfade)
                {
                    for (int frame = 0; frame < 300 && stockLogo.PaletteStep == 0; frame++)
                    {
                        stockLogo.Step(stockCgram, EndingLogoInstructionDefinitions.ReadWord);
                        editedLogo.Step(editedCgram, EndingLogoInstructionDefinitions.ReadWord);
                    }
                    AssertEqual(1, stockLogo.PaletteStep,
                        "logo override fixture reaches its first palette transfer");
                }
                AssertTrue(!stockCgram.Colors.SequenceEqual(editedCgram.Colors) &&
                    stockLogo.Draw().LowTable.SequenceEqual(editedLogo.Draw().LowTable),
                    $"edited {id} color changes logo CGRAM without changing actors");
            }
            File.Delete(overridePath);
        }
        VerifyVisibleCreditsPaletteOverride(installation, guardedBus, nativeBus);
        VerifyVisibleLogoPaletteOverride(installation, guardedBus, nativeBus);

        string invalidPath = Path.Combine(installation.EndingPaletteOverrideDirectory,
            EndingPaletteDefinitions.FileName(EndingPaletteId.Escape));
        File.WriteAllBytes(invalidPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingPalettes(),
            "malformed ending palette override fails loudly");
        File.Delete(invalidPath);
        EndingPaletteDocument persistent = JsonSerializer.Deserialize<EndingPaletteDocument>(
            File.ReadAllBytes(Path.Combine(installation.EndingPaletteDirectory,
                EndingPaletteDefinitions.FileName(EndingPaletteId.Escape))),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock ending escape palette is empty.");
        persistent.Colors[0] = persistent.Colors[0] with
        {
            Green = (persistent.Colors[0].Green + 1) & 31,
        };
        using (var output = File.Create(invalidPath))
            EndingPalette.Write(output, EndingPaletteId.Escape, persistent);
        byte[] selected = installation.LoadEndingPalettes()[EndingPaletteId.Escape].Transfer.ToArray();
        string stockCreditsPath = Path.Combine(installation.EndingPaletteDirectory,
            EndingPaletteDefinitions.FileName(EndingPaletteId.Credits));
        File.WriteAllBytes(stockCreditsPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingPalettes(),
            "valid override cannot conceal a corrupt stock ending palette");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ending palettes vanished during stock repair.");
        AssertTrue(repaired.LoadEndingPalettes()[EndingPaletteId.Escape].Transfer.Span
                .SequenceEqual(selected) &&
            repaired.LoadEndingPalettes()[EndingPaletteId.Credits].Transfer.Span
                .SequenceEqual(stock[EndingPaletteId.Credits].Transfer.Span),
            "stock palette repair preserves the external override and restores native credits colors");
        File.Delete(invalidPath);
        Console.WriteLine("Ending palettes: seven native images, complete logo fade, full ending CGRAM parity, isolated overrides and strict failures pass.");
    }

    private static void VerifyEndingLogoPaletteArtwork(EndingPaletteCatalog stock,
        ISnesAddressSpace nativeBus, EndingPaletteSourceReadGuard guardedBus)
    {
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var nativeLogo = new EndingLogo(nativeBus, nativeCgram, () => { });
        var installedLogo = new EndingLogo(guardedBus, installedCgram, () => { }, stock);
        AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
            "installed initial logo palette matches cartridge CGRAM");
        for (int frame = 0; frame < 300 && !nativeLogo.Completed; frame++)
        {
            nativeLogo.Step(nativeCgram);
            installedLogo.Step(installedCgram, EndingLogoInstructionDefinitions.ReadWord);
            AssertEqual(nativeLogo.PaletteStep, installedLogo.PaletteStep,
                $"installed logo crossfade step {frame}");
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"installed logo palette matches every native frame {frame}");
        }
        AssertTrue(nativeLogo.Completed && installedLogo.Completed,
            "both logo palettes complete the same sixteen-step fade");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed logo never reads either native palette color source");
    }

    private static void VerifyVisibleCreditsPaletteOverride(GameInstallation installation,
        ISnesAddressSpace guardedBus, ISnesAddressSpace nativeBus)
    {
        string name = EndingPaletteDefinitions.FileName(EndingPaletteId.Credits);
        string overridePath = Path.Combine(installation.EndingPaletteOverrideDirectory, name);
        EndingPaletteDocument document = JsonSerializer.Deserialize<EndingPaletteDocument>(
            File.ReadAllBytes(Path.Combine(installation.EndingPaletteDirectory, name)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock ending credits palette is empty.");
        for (int index = 0; index < document.Colors.Length; index++)
            document.Colors[index] = new PaletteRgb5 { Red = 31, Green = 0, Blue = 0 };
        using (var output = File.Create(overridePath))
            EndingPalette.Write(output, EndingPaletteId.Credits, document);
        EndingPaletteCatalog edited = installation.LoadEndingPalettes();
        var editedAudio = new CartridgeAudioState();
        var stockAudio = new CartridgeAudioState();
        var editedState = new EndingCreditsState(guardedBus, editedAudio, 2, 59);
        var stockState = new EndingCreditsState(nativeBus, stockAudio, 2, 59);
        editedState.BindPaletteArtwork(edited);
        CreditsPresentation credits = installation.LoadMaps().StaffCredits;
        editedState.BindStaffCredits(credits);
        stockState.BindStaffCredits(credits);
        bool visibleDifference = false;
        for (int frame = 0; frame < 60_000 &&
            stockState.Phase != EndingCreditsPhase.PostCreditsBlank; frame++)
        {
            editedState.Step();
            stockState.Step();
            editedAudio.AdvanceFrame(guardedBus, default);
            stockAudio.AdvanceFrame(nativeBus, default);
            if (stockState.Phase != EndingCreditsPhase.Credits || stockState.Brightness == 0 ||
                frame % 19 != 0) continue;
            LayeredRenderSnapshot changed = editedState.CaptureRenderSnapshot();
            LayeredRenderSnapshot original = stockState.CaptureRenderSnapshot();
            AssertTrue(changed.Memory.Vram.SequenceEqual(original.Memory.Vram),
                "credits palette edit does not alter graphics VRAM");
            if (!SoftwareLayeredSnapshotRenderer.Render(changed).AsSpan().SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(original)))
            {
                visibleDifference = true;
                break;
            }
        }
        AssertTrue(visibleDifference,
            "edited credits palette changes visible ending pixels without changing VRAM");
        File.Delete(overridePath);
    }

    private static void VerifyVisibleLogoPaletteOverride(GameInstallation installation,
        ISnesAddressSpace guardedBus, ISnesAddressSpace nativeBus)
    {
        string name = EndingPaletteDefinitions.FileName(EndingPaletteId.LogoCrossfade);
        string overridePath = Path.Combine(installation.EndingPaletteOverrideDirectory, name);
        EndingPaletteDocument document = JsonSerializer.Deserialize<EndingPaletteDocument>(
            File.ReadAllBytes(Path.Combine(installation.EndingPaletteDirectory, name)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock ending logo fade palette is empty.");
        for (int index = 0; index < document.Colors.Length; index++)
            document.Colors[index] = new PaletteRgb5 { Red = 31, Green = 0, Blue = 0 };
        using (var output = File.Create(overridePath))
            EndingPalette.Write(output, EndingPaletteId.LogoCrossfade, document);
        EndingPaletteCatalog edited = installation.LoadEndingPalettes();
        var editedAudio = new CartridgeAudioState();
        var stockAudio = new CartridgeAudioState();
        var editedState = new EndingCreditsState(guardedBus, editedAudio, 2, 59);
        var stockState = new EndingCreditsState(nativeBus, stockAudio, 2, 59);
        editedState.BindPaletteArtwork(edited);
        CreditsPresentation credits = installation.LoadMaps().StaffCredits;
        editedState.BindStaffCredits(credits);
        stockState.BindStaffCredits(credits);
        bool visibleDifference = false;
        for (int frame = 0; frame < 60_000 &&
            stockState.Phase != EndingCreditsPhase.ItemPercentage; frame++)
        {
            editedState.Step();
            stockState.Step();
            editedAudio.AdvanceFrame(guardedBus, default);
            stockAudio.AdvanceFrame(nativeBus, default);
            if (stockState.Phase != EndingCreditsPhase.PostCreditsLogo || frame % 3 != 0)
                continue;
            LayeredRenderSnapshot changed = editedState.CaptureRenderSnapshot();
            LayeredRenderSnapshot original = stockState.CaptureRenderSnapshot();
            AssertTrue(changed.Memory.Vram.SequenceEqual(original.Memory.Vram),
                "logo fade color edit does not alter graphics VRAM");
            if (!SoftwareLayeredSnapshotRenderer.Render(changed).AsSpan().SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(original)))
            {
                visibleDifference = true;
                break;
            }
        }
        AssertTrue(visibleDifference,
            "edited logo crossfade changes visible ending pixels without changing VRAM");
        File.Delete(overridePath);
    }

    private sealed class EndingPaletteSourceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        private static readonly (EndingPaletteId Id, int Start, int End)[] Ranges =
            Enum.GetValues<EndingPaletteId>()
                .Where(id => id != EndingPaletteId.LogoCrossfade)
                .Select(id => (id, EndingPaletteDefinitions.SourceAddress(id),
                    EndingPaletteDefinitions.SourceAddress(id) +
                    EndingPaletteDefinitions.ColorCount(id) * sizeof(ushort)))
                .Append((EndingPaletteId.LogoCrossfade, 0x8cefe9, 0x8cf3e9))
                .ToArray();

        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach ((EndingPaletteId id, int start, int end) in Ranges)
            {
                if (address < start || address >= end) continue;
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending reread {id} palette source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
