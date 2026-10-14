using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Ending.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Checks ending palette identity, native fade colors, installed artwork, and visible overrides.</summary>
internal static class EndingPaletteArtworkVerification
{
    /// <summary>Checks the logo's reverse-copy palette fade against native pointers and actual CGRAM writes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending logo palette fade")]
    internal static void VerifyEndingLogoPaletteFade(CartridgeImportAddressSpace rom)
    {
        var original = new byte[512 * 2];
        var colors = new PaletteRgb5[512];
        // The original pointer list and reverse-copy operand order are the oracle.
        // Neither the production source selector nor the proposed arithmetic supplies it.
        for (int step = 0; step < 16; step++)
        for (int palette = 0; palette < 2; palette++)
        for (int color = 0; color < 16; color++)
        {
            int last = rom.ReadCartridgeWord((0x8be5e7 + (step * 2 + palette) * 2));
            ushort word = rom.ReadCartridgeWord((0x8c0000 | (last - (15 - color) * 2)));
            int index = step * 32 + palette * 16 + color;
            original[index * 2] = (byte)word;
            original[index * 2 + 1] = SnesLittleEndian.HighByte(word);
            colors[index] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }
        var background = new ushort[16];
        for (int color = 0; color < 16; color++) background[color] = rom.ReadCartridgeWord((0x8cf1e9 + color * 2));
        EndingLogoBackgroundPalette gradient = EndingLogoBackgroundPalette.TryCreate(ToColors(background))
            ?? throw new InvalidOperationException("Original logo background must use a computed grey gradient.");
        for (int color = 0; color < 16; color++)
            AssertEqual(background[color], gradient.Color(color), "all original logo endpoint shades computed exactly");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => gradient.Color(invalid), "logo background gradient bounds");
        // Swapping two complete source columns preserves each temporal fade but changes
        // the endpoint's spatial gradient. Recognition must keep these supplied columns.
        byte[] reordered = (byte[])original.Clone();
        (background[1], background[2]) = (background[2], background[1]);
        AssertTrue(EndingLogoBackgroundPalette.TryCreate(ToColors(background)) is null, "independent shade order is not replaced");
        for (int step = 0; step < 16; step++)
        for (int lane = 0; lane < 2; lane++)
        {
            int first = (step * 32 + 1) * 2 + lane, second = (step * 32 + 2) * 2 + lane;
            (reordered[first], reordered[second]) = (reordered[second], reordered[first]);
        }
        EndingLogoPaletteFade reorderedFade = EndingLogoPaletteFade.TryCreate(reordered)
            ?? throw new InvalidOperationException("Reordered endpoint shades still have the original temporal fade.");
        for (int index = 0; index < 512; index++)
            AssertEqual((ushort)(SnesLittleEndian.Word(reordered[index * 2], reordered[index * 2 + 1])), reorderedFade.Color(index),
                "temporal fade preserves edited endpoint order");
        EndingLogoPaletteFade fade = EndingLogoPaletteFade.TryCreate(original)
            ?? throw new InvalidOperationException("Original logo fade must use computed interpolation.");
        EndingPalette Load(PaletteRgb5[] supplied)
        {
            using var json = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
                new EndingPaletteDocument { Version = 1, Colors = supplied }, MapPresentationFormat.JsonOptions));
            return EndingPalette.Load(json, EndingPaletteId.LogoCrossfade);
        }
        EndingPalette loaded = Load(colors);
        int remaindersSeen = 0;
        for (int index = 0; index < 512; index++)
        {
            ushort expected = (ushort)(SnesLittleEndian.Word(original[index * 2], original[index * 2 + 1]));
            AssertEqual(expected, fade.Color(index), "all original logo fade colors computed exactly");
            AssertEqual(expected, loaded.Color(index), "computed fade reaches palette accessor");
            int palette = index / 16 % 2, step = index / 32;
            PaletteRgb5 endpoint = colors[(palette == 0 ? 15 * 32 : 16) + index % 16];
            foreach (int channel in new[] { endpoint.Red, endpoint.Green, endpoint.Blue })
                remaindersSeen |= 1 << (channel * (palette == 0 ? step : 15 - step) % 15);
        }
        AssertEqual(0x7fff, remaindersSeen, "original data exercises every quantization remainder");
        AssertTrue(loaded.Transfer.Span.SequenceEqual(original), "computed fade preserves complete exported bytes");
        var cgram = new SnesCgram();
        for (int step = 0; step < 16; step++)
        {
            loaded.LoadTo(cgram, step * 32, 32, 64);
            for (int color = 0; color < 32; color++)
            {
                int offset = (step * 32 + color) * 2;
                AssertEqual((ushort)(SnesLittleEndian.Word(original[offset], original[offset + 1])), cgram.Colors[64 + color],
                    "computed fade partial CGRAM transfer");
            }
        }
        // Independent edits to a dark frame, an intermediate frame and an endpoint
        // must survive even though they no longer describe a linear fade.
        foreach (int edited in new[] { 1, 7 * 32 + 19, 15 * 32 + 1 })
            colors[edited] = colors[edited] with { Red = (colors[edited].Red + 11) & 31 };
        EndingPalette custom = Load(colors);
        for (int index = 0; index < 512; index++)
        {
            PaletteRgb5 color = colors[index];
            AssertEqual((ushort)(color.Red | color.Green << 5 | color.Blue << 10), custom.Color(index),
                "independent edited fade samples survive unchanged");
        }
        AssertTrue(EndingLogoPaletteFade.TryCreate(custom.Transfer.Span) is null, "nonlinear edits remain caller-owned content");
        foreach (int invalid in new[] { int.MinValue, -1, 512, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => fade.Color(invalid), "computed fade index domain");
            AssertThrows<ArgumentOutOfRangeException>(() => loaded.Color(invalid), "loaded fade index domain");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => loaded.LoadTo(cgram, 511, 2, 0), "computed fade source transfer bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => loaded.LoadTo(cgram, 0, 32, 240), "computed fade destination transfer bounds");
        loaded.LoadTo(cgram, 512, 0, 256);
        AssertEqual(512, loaded.ColorCount, "computed fade retains published resource size");
    }

    /// <summary>Derives six ending palette source ranges from native load operands and validates catalog metadata.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending palette metadata")]
    internal static void VerifyEndingPaletteMetadata(CartridgeImportAddressSpace rom)
    {
        // Original indexed LDA operands select the six contiguous resources. The gunship
        // uses absolute,X in bank8B; the others use long,X. No production source constants.
        (EndingPaletteId Role, int Load, bool Long)[] loads =
        [
            (EndingPaletteId.Escape, 0x8bd4a5, true),
            (EndingPaletteId.PostCredits, 0x8bf70c, true),
            (EndingPaletteId.Credits, 0x8bde8d, true),
            (EndingPaletteId.Explosion, 0x8bd97e, true),
            (EndingPaletteId.FinalGunship, 0x8bde37, false),
            (EndingPaletteId.LogoInitial, 0x8be57d, true),
        ];
        foreach (var load in loads)
        {
            AssertEqual(load.Long ? 0xbf : 0xbd, (int)rom.ReadCartridgeByte(load.Load), "native palette indexed load opcode");
            int address = (int)new SnesAddress(load.Long ? rom.ReadCartridgeByte(load.Load + 3) : (byte)SnesAddressMath.BankNumber(load.Load), rom.ReadCartridgeWord(load.Load + 1));
            AssertEqual(address, EndingPaletteDefinitions.SourceAddress(load.Role), "native palette source role");
        }
        // Allocation boundaries in pinned bank8C, independent of partial runtime transfers.
        foreach (var allocation in new[]
        {
            (EndingPaletteId.PostCredits, 0x8ce7e9, 0x8ce9e9),
            (EndingPaletteId.Credits, 0x8ce9e9, 0x8cebe9),
            (EndingPaletteId.Explosion, 0x8cebe9, 0x8cede9),
            (EndingPaletteId.Escape, 0x8cede9, 0x8cefe9),
        })
            AssertEqual((allocation.Item3 - allocation.Item2) / 2,
                EndingPaletteDefinitions.ColorCount(allocation.Item1), "complete palette allocation size");
        foreach (var single in new[] { (EndingPaletteId.FinalGunship, 0x8bde34), (EndingPaletteId.LogoInitial, 0x8be57a) })
        {
            AssertEqual(0xa2, (int)rom.ReadCartridgeByte(single.Item2), "native reverse palette LDX opcode");
            int lastOffset = rom.ReadCartridgeWord(single.Item2 + 1);
            AssertEqual(lastOffset / 2 + 1, EndingPaletteDefinitions.ColorCount(single.Item1), "single palette reverse-copy size");
        }
        AssertEqual((0x8be627 - 0x8be5e7) / 2 * 16,
            EndingPaletteDefinitions.ColorCount(EndingPaletteId.LogoCrossfade), "all native crossfade pointer payloads");
        (EndingPaletteId Role, string Name)[] published =
        [
            (EndingPaletteId.Escape, "ending-escape-palette.json"),
            (EndingPaletteId.PostCredits, "ending-post-credits-palette.json"),
            (EndingPaletteId.Credits, "ending-credits-palette.json"),
            (EndingPaletteId.Explosion, "ending-explosion-palette.json"),
            (EndingPaletteId.FinalGunship, "ending-final-gunship-palette.json"),
            (EndingPaletteId.LogoInitial, "ending-logo-initial-palette.json"),
            (EndingPaletteId.LogoCrossfade, "ending-logo-crossfade-palette.json"),
        ];
        foreach (var file in published)
            AssertEqual(file.Name, EndingPaletteDefinitions.FileName(file.Role), "published palette filename compatibility");
        AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteDefinitions.SourceAddress(EndingPaletteId.LogoCrossfade),
            "crossfade has no contiguous source");
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteDefinitions.SourceAddress((EndingPaletteId)invalid), "invalid palette source role");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteDefinitions.ColorCount((EndingPaletteId)invalid), "invalid palette size role");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteDefinitions.FileName((EndingPaletteId)invalid), "invalid palette file role");
        }
    }

    /// <summary>Checks all seven constructor-position palette roles with distinct caller-owned colors.</summary>
    internal static void VerifyEndingPaletteRoleSelection()
    {
        // Distinct caller-owned values exercise the original constructor-position contract.
        var supplied = new EndingPalette[7];
        for (int i = 0; i < supplied.Length; i++)
        {
            using var json = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(new EndingPaletteDocument
            {
                Version = 1,
                Colors = Enumerable.Range(0, 256).Select(_ => new PaletteRgb5 { Red = i, Green = 0, Blue = 0 }).ToArray(),
            }, MapPresentationFormat.JsonOptions));
            supplied[i] = EndingPalette.Load(json, EndingPaletteId.Escape);
        }
        var catalog = new EndingPaletteCatalog(supplied[0], supplied[1], supplied[2], supplied[3], supplied[4], supplied[5], supplied[6]);
        EndingPaletteId[] originalOrder = [EndingPaletteId.Escape, EndingPaletteId.PostCredits, EndingPaletteId.Credits,
            EndingPaletteId.Explosion, EndingPaletteId.FinalGunship, EndingPaletteId.LogoInitial, EndingPaletteId.LogoCrossfade];
        for (int i = 0; i < originalOrder.Length; i++)
            AssertTrue(ReferenceEquals(supplied[i], catalog[originalOrder[i]]), "ending palette role preserves supplied reference");
        string originalIdentity = SelectedPresentationHash.Create(nameof(EndingPaletteCatalog), content =>
        {
            content.Append("palette-count", supplied.Length);
            foreach (EndingPalette palette in supplied) content.Append("palette", palette.Transfer.Span);
        });
        AssertEqual(originalIdentity, catalog.ContentIdentity, "ending palette identity preserves original role order");
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = catalog[(EndingPaletteId)invalid], "ending palette invalid role compatibility");
    }

    /// <summary>Checks installed ending colors and visible scene overrides with cartridge reads guarded.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyEndingPaletteArtwork(GameInstallation installation)
    {
        Suite(nameof(VerifyEndingPaletteRoleSelection), () => VerifyEndingPaletteRoleSelection());
        EndingPaletteCatalog stock = installation.LoadEndingPalettes();
        AreaMapPresentationCatalog maps = installation.LoadMaps();
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
        {
            byte[] native = EndingPaletteArtworkFiles.ReadNativePalette(nativeBus, id,
                EndingPaletteDefinitions.ColorCount(id));
            AssertTrue(stock[id].Transfer.Span.SequenceEqual(native),
                $"installed ending {id} palette preserves every cartridge color byte");
        }

        bool rejectAllCartridgeReads = false;
        var guardedBus = NewEndingPaletteSourceReadGuard(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"),
            maps.RoomPaletteFx, () => rejectAllCartridgeReads);
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var nativeState = CreateRetailEndingFixture(nativeBus, nativeAudio, 2, 59);
        var installedState = CreateRetailEndingFixture(guardedBus, installedAudio, 2, 59);
        installedState.BindPaletteArtwork(stock);
        installedState.BindPaletteFxColors(maps.RoomPaletteFx);
        installedState.BindEndingText(maps.EndingText);
        installedState.BindEndingFont(maps.EndingFont);
        installedState.BindFlightArtwork(installation.LoadIntroCinematicArt().CeresFlight);
        installedState.BindMode7Artwork(installation.LoadEndingMode7Art());
        installedState.BindObjectArtwork(installation.LoadEndingObjectArt());
        CreditsPresentation credits = maps.StaffCredits;
        nativeState.BindStaffCredits(credits);
        installedState.BindStaffCredits(credits);
        // This matches the playable host's installed ending bindings. The fallback
        // bus remains available in narrower native-reference tests below, but the
        // production-shaped scene must complete without *any* cartridge read.
        rejectAllCartridgeReads = true;
        AssertThrows<InvalidOperationException>(
            () => guardedBus.ReadByte(EndingCreditsRomData.Assets.FlyawayCharacters),
            "ending ROM guard rejects an otherwise valid non-palette cartridge source");
        var reached = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 60_000 &&
            nativeState.Phase != EndingCreditsPhase.SeeYouNextMission; frame++)
        {
            AssertEqual(nativeState.Phase, installedState.Phase,
                $"installed ending palettes preserve phase at frame {frame}");
            bool first = reached.Add(nativeState.Phase);
            if (first || frame % 173 == 0)
            {
                LayeredRenderSnapshot expected = nativeState.Presentation.CaptureRenderSnapshot();
                LayeredRenderSnapshot actual = installedState.Presentation.CaptureRenderSnapshot();
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
        rejectAllCartridgeReads = false;
        foreach (EndingCreditsPhase phase in new[]
        {
            EndingCreditsPhase.WaitForEscapeMusic,
            EndingCreditsPhase.FadeInEscapeSceneB,
            EndingCreditsPhase.FadeInZebesExplosion,
            EndingCreditsPhase.ZebesExplosionTileUpload,
            EndingCreditsPhase.PlanetEscapeFast,
            EndingCreditsPhase.Credits,
            EndingCreditsPhase.PostCreditsBlank,
            EndingCreditsPhase.OperationSuccessfulText,
            EndingCreditsPhase.PostCreditsWhiteFlash,
        })
            AssertTrue(reached.Contains(phase),
                $"ending palette parity exercises {phase}");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed ending never rereads static, logo or palette-FX ROM colors");
        Suite(nameof(VerifyEndingLogoPaletteArtwork), () => VerifyEndingLogoPaletteArtwork(stock, nativeBus, guardedBus));

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
                var editedState = CreateRetailEndingFixture(guardedBus, editedAudio, 2, 59);
                editedState.BindPaletteArtwork(edited);
                editedState.BindPaletteFxColors(maps.RoomPaletteFx);
                var stockAudio = new CartridgeAudioState();
                var stockState = CreateRetailEndingFixture(nativeBus, stockAudio, 2, 59);
                // The ending's setup dispatch loads the escape palette only after its own NMI
                // waits; step through them so both images show the installed palette.
                do
                {
                    editedState.Step();
                    stockState.Step();
                }
                while (editedState.ResumesAfterNmiWait);
                AssertTrue(!editedState.Presentation.CaptureRenderSnapshot().Memory.Cgram.SequenceEqual(
                        stockState.Presentation.CaptureRenderSnapshot().Memory.Cgram) &&
                    editedState.Presentation.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(
                        stockState.Presentation.CaptureRenderSnapshot().Memory.Vram),
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
                AssertTrue(!new CgramPalettes(stockCgram).ImageEquals(new CgramPalettes(editedCgram)) &&
                    stockLogo.Draw(installation.LoadEndingObjectArt().LogoSprites).LowTable.SequenceEqual(editedLogo.Draw(installation.LoadEndingObjectArt().LogoSprites).LowTable),
                    $"edited {id} color changes logo CGRAM without changing actors");
            }
            File.Delete(overridePath);
        }
        Suite(nameof(VerifyVisibleCreditsPaletteOverride), () => VerifyVisibleCreditsPaletteOverride(installation, guardedBus, nativeBus));
        Suite(nameof(VerifyVisibleLogoPaletteOverride), () => VerifyVisibleLogoPaletteOverride(installation, guardedBus, nativeBus));
        Suite(nameof(VerifyVisibleEndingPaletteFxOverride), () => VerifyVisibleEndingPaletteFxOverride(installation, guardedBus, nativeBus));

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
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "stock and edited ending scenes never reread installed palette colors");
        Console.WriteLine("Ending palettes: seven native images and installed palette-FX colors, full ending CGRAM parity, ROM-free installed scene/audio-step guard, live visible overrides and strict failures pass.");
    }

    /// <summary>Edits explosion palette-FX colors and checks that the ending scene visibly consumes them.</summary>
    private static void VerifyVisibleEndingPaletteFxOverride(GameInstallation installation,
        ISnesAddressSpace guardedBus, ISnesAddressSpace nativeBus)
    {
        byte[] stockFile = File.ReadAllBytes(Path.Combine(installation.MapDirectory,
            RoomPaletteFxPresentationFormat.FileName));
        RoomPaletteFxPresentationDocument document =
            JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(stockFile,
                MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock ending palette-FX document is empty.");
        foreach (PaletteRgb5[] frame in document.ZebesExplosionLava)
            for (int color = 0; color < frame.Length; color++)
                frame[color] = new PaletteRgb5 { Red = 31, Green = 0, Blue = 0 };
        byte[] editedFile = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        RoomPaletteFxPresentation editedColors = RoomPaletteFxPresentation.Load(
            new MemoryStream(editedFile, writable: false));

        var editedAudio = new CartridgeAudioState();
        var stockAudio = new CartridgeAudioState();
        var editedScene = CreateRetailEndingFixture(guardedBus, editedAudio, 2, 59);
        var stockScene = CreateRetailEndingFixture(nativeBus, stockAudio, 2, 59);
        editedScene.BindPaletteArtwork(installation.LoadEndingPalettes());
        AreaMapPresentationCatalog maps = installation.LoadMaps();
        editedScene.BindPaletteFxColors(maps.RoomPaletteFx);
        CreditsPresentation credits = maps.StaffCredits;
        editedScene.BindStaffCredits(credits);
        stockScene.BindStaffCredits(credits);
        bool visibleDifference = false;
        bool reboundDuringScene = false;
        for (int frame = 0; frame < 60_000 &&
            stockScene.Phase != EndingCreditsPhase.Credits; frame++)
        {
            if (frame == 20)
            {
                Bgr555[] before = editedScene.Presentation.CaptureRenderSnapshot().Memory.Cgram.ToArray();
                editedScene.BindPaletteFxColors(editedColors);
                AssertTrue(before.SequenceEqual(
                        editedScene.Presentation.CaptureRenderSnapshot().Memory.Cgram),
                    "rebinding ending palette-FX preserves already-drawn CGRAM state");
                reboundDuringScene = true;
            }
            AssertEqual(stockScene.Phase, editedScene.Phase,
                $"edited ending palette-FX preserves cinematic phase at frame {frame}");
            editedScene.Step();
            stockScene.Step();
            editedAudio.AdvanceFrame(guardedBus, default);
            stockAudio.AdvanceFrame(nativeBus, default);
            if (frame % 3 != 0) continue;
            LayeredRenderSnapshot changed = editedScene.Presentation.CaptureRenderSnapshot();
            LayeredRenderSnapshot original = stockScene.Presentation.CaptureRenderSnapshot();
            if (changed.Memory.Cgram.SequenceEqual(original.Memory.Cgram)) continue;
            AssertTrue(changed.Memory.Vram.SequenceEqual(original.Memory.Vram),
                "ending palette-FX edit does not change graphics or transfer timing");
            if (!SoftwareLayeredSnapshotRenderer.Render(changed).SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(original)))
            {
                visibleDifference = true;
                break;
            }
        }
        AssertTrue(reboundDuringScene && visibleDifference,
            "edited Zebes lava palette-FX changes visible ending pixels without changing phase or VRAM");
    }

    /// <summary>Compares installed logo fade CGRAM with native playback while prohibiting source color reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending logo palette artwork")]
    private static void VerifyEndingLogoPaletteArtwork(EndingPaletteCatalog stock,
        ISnesAddressSpace nativeBus, MutableCartridgeReadGuard guardedBus)
    {
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var nativeLogo = new EndingLogo(nativeBus, nativeCgram, () => { }, stock);
        var installedLogo = new EndingLogo(guardedBus, installedCgram, () => { }, stock);
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
            "installed initial logo palette matches cartridge CGRAM");
        for (int frame = 0; frame < 300 && !nativeLogo.Completed; frame++)
        {
            nativeLogo.Step(nativeCgram, pointer => CartridgeImportSource.Require(nativeBus).ReadCartridgeWordFixedBank(0x8b0000 | pointer));
            if (nativeLogo.PaletteStep > 0)
            {
                byte[] nativeColors = EndingPaletteArtworkFiles.ReadNativePalette(nativeBus,
                    EndingPaletteId.LogoCrossfade, EndingPaletteDefinitions.ColorCount(EndingPaletteId.LogoCrossfade));
                for (int palette = 0; palette < 2; palette++)
                    for (int color = 0; color < 16; color++)
                    {
                        int offset = (((nativeLogo.PaletteStep - 1) * 2 + palette) * 16 + color) * 2;
                        nativeCgram.SetColor((palette == 0 ? 16 : 240) + color,
                            Bgr555.FromWord((ushort)(SnesLittleEndian.Word(nativeColors[offset], nativeColors[offset + 1]))));
                    }
            }
            installedLogo.Step(installedCgram, EndingLogoInstructionDefinitions.ReadWord);
            AssertEqual(nativeLogo.PaletteStep, installedLogo.PaletteStep,
                $"installed logo crossfade step {frame}");
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"installed logo palette matches every native frame {frame}");
        }
        AssertTrue(nativeLogo.Completed && installedLogo.Completed,
            "both logo palettes complete the same sixteen-step fade");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed logo never reads either native palette color source");
    }

    /// <summary>Edits credits palette colors and checks rendered ending pixels change without VRAM edits.</summary>
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
        var editedState = CreateRetailEndingFixture(guardedBus, editedAudio, 2, 59);
        var stockState = CreateRetailEndingFixture(nativeBus, stockAudio, 2, 59);
        editedState.BindPaletteArtwork(edited);
        AreaMapPresentationCatalog maps = installation.LoadMaps();
        editedState.BindPaletteFxColors(maps.RoomPaletteFx);
        CreditsPresentation credits = maps.StaffCredits;
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
            LayeredRenderSnapshot changed = editedState.Presentation.CaptureRenderSnapshot();
            LayeredRenderSnapshot original = stockState.Presentation.CaptureRenderSnapshot();
            AssertTrue(changed.Memory.Vram.SequenceEqual(original.Memory.Vram),
                "credits palette edit does not alter graphics VRAM");
            if (!SoftwareLayeredSnapshotRenderer.Render(changed).SequenceEqual(
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

    /// <summary>Edits logo crossfade colors and checks visible output while retaining tile data.</summary>
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
        var editedState = CreateRetailEndingFixture(guardedBus, editedAudio, 2, 59);
        var stockState = CreateRetailEndingFixture(nativeBus, stockAudio, 2, 59);
        editedState.BindPaletteArtwork(edited);
        AreaMapPresentationCatalog maps = installation.LoadMaps();
        editedState.BindPaletteFxColors(maps.RoomPaletteFx);
        CreditsPresentation credits = maps.StaffCredits;
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
            LayeredRenderSnapshot changed = editedState.Presentation.CaptureRenderSnapshot();
            LayeredRenderSnapshot original = stockState.Presentation.CaptureRenderSnapshot();
            AssertTrue(changed.Memory.Vram.SequenceEqual(original.Memory.Vram),
                "logo fade color edit does not alter graphics VRAM");
            if (!SoftwareLayeredSnapshotRenderer.Render(changed).SequenceEqual(
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

    /// <summary>Cartridge color intervals prohibited during ROM-free installed ending playback.</summary>
    private static readonly (EndingPaletteId Id, int Start, int End)[] EndingPaletteSourceRanges =
        Enum.GetValues<EndingPaletteId>()
            .Where(id => id != EndingPaletteId.LogoCrossfade)
            .Select(id => (id, EndingPaletteDefinitions.SourceAddress(id),
                EndingPaletteDefinitions.SourceAddress(id) +
                EndingPaletteDefinitions.ColorCount(id) * sizeof(ushort)))
            .Append((EndingPaletteId.LogoCrossfade, 0x8cefe9, 0x8cf3e9))
            .ToArray();

    /// <summary>
    /// Forbids every installed ending palette source. While <paramref name="rejectAllCartridgeReads"/>
    /// holds, any other cartridge read is also rejected; that rejection is not a forbidden-source
    /// attempt, so it throws from an observation rule and leaves the counter unchanged.
    /// </summary>
    private static MutableCartridgeReadGuard NewEndingPaletteSourceReadGuard(ISnesAddressSpace source,
        RoomPaletteFxPresentation effectColors, Func<bool> rejectAllCartridgeReads)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x8d8000 and < 0x8e0000 &&
                (effectColors.TryReadColor((ushort)address, out _) ||
                 effectColors.TryReadColor(unchecked((ushort)(address - 1)), out _)),
            address => $"Ending reread installed palette-FX color byte ${address:X6}.");
        foreach ((EndingPaletteId id, int start, int end) in EndingPaletteSourceRanges)
            guard.Forbid(address => address >= start && address < end,
                address => $"Ending reread {id} palette source ${address:X6}.");
        guard.Observe(address => rejectAllCartridgeReads() && SnesAddressMath.BankNumber(address) is not (0x7e or 0x7f) &&
                (address & 0x8000) != 0,
            address => throw new InvalidOperationException(
                $"Installed ending reread cartridge byte ${address:X6}."));
        return guard;
    }

    /// <summary>Runs the seven-role palette identity check without a cartridge source.</summary>
    [VerificationCommand("--lookup-ending-palette-roles")]
    internal static int LookupEndingPaletteRolesCommand()
    {
        Suite(nameof(VerifyEndingPaletteRoleSelection), () => VerifyEndingPaletteRoleSelection());
        Console.WriteLine("Ending palette roles: all seven supplied references, original identity order and invalid-role behavior pass.");
        return 0;
    }
}
