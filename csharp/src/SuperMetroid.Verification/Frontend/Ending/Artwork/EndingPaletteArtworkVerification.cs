using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Fx.Artwork;
namespace SuperMetroid.Verification.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Checks the ending colours the import decodes (#1544): their cartridge sources, the logo's decoded fade, the installed
/// scene backdrops, and an installed ending that reads no colour from the cartridge.
/// </summary>
internal static class EndingPaletteArtworkVerification
{
    /// <summary>$95:A82F: the flyaway's compressed character lane, a valid non-palette cartridge source the guard must reject (#1416: moved from the deleted test-support constants).</summary>
    private const int FlyawayCharactersSource = 0x95a82f;

    /// <summary>Checks the import's decoded logo fade against native pointers and actual CGRAM writes.</summary>
    internal static void VerifyEndingLogoPaletteFade(CartridgeImportAddressSpace rom)
    {
        var original = new byte[512 * 2];
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
            original[index * 2 + 1] = LittleEndianBytes.HighByte(word);
        }
        EndingPalette loaded = EndingPaletteImport.Read(rom)[EndingPaletteId.LogoCrossfade];
        for (int index = 0; index < 512; index++)
        {
            ushort expected = LittleEndianBytes.Word(original[index * 2], original[index * 2 + 1]);
            AssertEqual(expected, loaded.CrossfadeColorAt(index), "every native logo fade colour reaches the decoded crossfade");
        }
        // The logo reads each step's colours by layer; the native resource holds each step's background colours, then its
        // sprite colours.
        for (int step = 0; step < 16; step++)
        for (int color = 0; color < 32; color++)
        {
            int offset = (step * 32 + color) * 2;
            AssertEqual(LittleEndianBytes.Word(original[offset], original[offset + 1]),
                loaded.LogoCrossfadeColour(step, color < 16 ? EndingLogoLayer.Background : EndingLogoLayer.Sprites, color % 16),
                "decoded fade step colour by layer");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 512, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => loaded.CrossfadeColorAt(invalid), "decoded fade index domain");
        AssertThrows<ArgumentOutOfRangeException>(() => loaded.LogoCrossfadeColour(16, EndingLogoLayer.Background, 0), "decoded fade step bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => loaded.LogoCrossfadeColour(0, EndingLogoLayer.Sprites, 16), "decoded fade colour bounds");
        AssertEqual(512, loaded.Colours.Length, "decoded fade holds every crossfade colour");
    }

    /// <summary>Derives six ending palette source ranges from native load operands and validates the import's colour counts.</summary>
    internal static void VerifyEndingPaletteMetadata(CartridgeImportAddressSpace rom)
    {
        // Original indexed LDA operands select the six contiguous resources. The gunship
        // uses absolute,X in bank8B; the others use long,X. No production source constants.
        EndingPaletteArtworkLoad[] loads =
        [
            new EndingPaletteArtworkLoad(EndingPaletteId.Escape, 0x8bd4a5, true),
            new EndingPaletteArtworkLoad(EndingPaletteId.PostCredits, 0x8bf70c, true),
            new EndingPaletteArtworkLoad(EndingPaletteId.Credits, 0x8bde8d, true),
            new EndingPaletteArtworkLoad(EndingPaletteId.Explosion, 0x8bd97e, true),
            new EndingPaletteArtworkLoad(EndingPaletteId.FinalGunship, 0x8bde37, false),
            new EndingPaletteArtworkLoad(EndingPaletteId.LogoInitial, 0x8be57d, true),
        ];
        foreach (var load in loads)
        {
            AssertEqual(load.Long ? 0xbf : 0xbd, (int)rom.ReadCartridgeByte(load.Load), "native palette indexed load opcode");
            int address = (int)new SnesAddress(load.Long ? rom.ReadCartridgeByte(load.Load + 3) : (byte)SnesAddressMath.BankNumber(load.Load), rom.ReadCartridgeWord(load.Load + 1));
            AssertEqual(address, EndingPaletteSources.Source(load.Role), "native palette source role");
        }
        // Allocation boundaries in pinned bank8C, independent of partial runtime transfers.
        foreach (var allocation in new[]
        {
            new EndingPaletteArtworkVerificationValue(EndingPaletteId.PostCredits, 0x8ce7e9, 0x8ce9e9),
            new EndingPaletteArtworkVerificationValue(EndingPaletteId.Credits, 0x8ce9e9, 0x8cebe9),
            new EndingPaletteArtworkVerificationValue(EndingPaletteId.Explosion, 0x8cebe9, 0x8cede9),
            new EndingPaletteArtworkVerificationValue(EndingPaletteId.Escape, 0x8cede9, 0x8cefe9),
        })
            AssertEqual((allocation.End - allocation.Start) / 2,
                EndingPaletteImport.ColorCount(allocation.Id), "complete palette allocation size");
        foreach (var single in new[] { new VerifyEndingPaletteMetadataCase(EndingPaletteId.FinalGunship, 0x8bde34), new VerifyEndingPaletteMetadataCase(EndingPaletteId.LogoInitial, 0x8be57a) })
        {
            AssertEqual(0xa2, (int)rom.ReadCartridgeByte(single.Second), "native reverse palette LDX opcode");
            int lastOffset = rom.ReadCartridgeWord(single.Second + 1);
            AssertEqual(lastOffset / 2 + 1, EndingPaletteImport.ColorCount(single.First), "single palette reverse-copy size");
        }
        AssertEqual((0x8be627 - 0x8be5e7) / 2 * 16,
            EndingPaletteImport.ColorCount(EndingPaletteId.LogoCrossfade), "all native crossfade pointer payloads");
        AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteSources.Source(EndingPaletteId.LogoCrossfade),
            "crossfade has no contiguous source");
        foreach (int invalid in new[] { int.MinValue, -1, 7, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteSources.Source((EndingPaletteId)invalid), "invalid palette source role");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteImport.ColorCount((EndingPaletteId)invalid), "invalid palette size role");
        }
    }

    /// <summary>
    /// Checks the installed scene backdrops against the cartridge's scene colours, and an installed ending that reads no
    /// colour from the cartridge (#1544).
    /// </summary>
    internal static void VerifyEndingPaletteArtwork(GameInstallation installation)
    {
        EndingBackdrops backdrops = installation.LoadEndingBackdrops();
        AreaMapPresentation maps = installation.LoadMaps();
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        foreach (EndingBackdropScene scene in new[]
        {
            new EndingBackdropScene(EndingPaletteId.Escape, backdrops.Escape),
            new EndingBackdropScene(EndingPaletteId.Explosion, backdrops.Explosion),
            new EndingBackdropScene(EndingPaletteId.Credits, backdrops.Credits),
            new EndingBackdropScene(EndingPaletteId.PostCredits, backdrops.PostCredits),
        })
        {
            byte[] native = EndingPaletteImport.ReadNativePalette(nativeBus, scene.Screen, EndingPaletteImport.ColorCount(scene.Screen));
            AssertEqual(LittleEndianBytes.Word(native[0], native[1]), scene.Backdrop.ToWord(),
                $"installed ending {scene.Screen} backdrop is the cartridge scene's first colour");
        }
        // The space behind the exploding planet shows each frame's first palette-effect colour, which the import reads (#1637).
        AssertTrue(EndingSpaceBackdropColours.InFrameOrder(backdrops.Space).SequenceEqual(
                EndingSpaceBackdropColours.InFrameOrder(EndingSpaceBackdropColours.From(InstalledFixtures.ImportMaps.RoomPaletteFx))),
            "each installed backdrop of the space behind the exploding planet is its frame's first colour");
        VerifyEndingBackdropsRejectMalformedSpace(installation.EndingBackdropDirectory);

        bool rejectAllCartridgeReads = false;
        var guardedBus = NewEndingPaletteSourceReadGuard(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"),
            InstalledFixtures.ImportMaps.RoomPaletteFx, () => rejectAllCartridgeReads);
        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var nativeState = CreateRetailEndingFixture(nativeAudio, 2, 59);
        var installedState = CreateRetailEndingFixture(installedAudio, 2, 59);
        installedState.InstalledArtwork.BindBackdrops(backdrops);
        installedState.InstalledArtwork.BindEndingText(maps.EndingText);
        installedState.InstalledArtwork.BindFixtureObjectArtwork(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingObjects);
        // This matches the playable host's installed ending bindings: the production-shaped scene must complete without
        // *any* cartridge read.
        rejectAllCartridgeReads = true;
        AssertThrows<InvalidOperationException>(
            () => guardedBus.ReadByte(FlyawayCharactersSource),
            "ending ROM guard rejects an otherwise valid non-palette cartridge source");
        var reached = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 60_000 &&
            nativeState.Stage.Phase != EndingCreditsPhase.SeeYouNextMission; frame++)
        {
            AssertEqual(nativeState.Stage.Phase, installedState.Stage.Phase,
                $"installed ending backdrops preserve phase at frame {frame}");
            bool first = reached.Add(nativeState.Stage.Phase);
            if (first || frame % 173 == 0)
            {
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(nativeState);
                RgbColor[] expectedColours = EndingShownColours.Of(nativeState);
                byte[] expectedVram = nativeState.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installedState);
                RgbColor[] actualColours = EndingShownColours.Of(installedState);
                byte[] actualVram = installedState.ShownVram();
                AssertTrue(actualColours.SequenceEqual(expectedColours) &&
                        actualVram.SequenceEqual(expectedVram) &&
                        actual.Brightness == expected.Brightness,
                    $"installed ending backdrops preserve the shown ending at frame {frame}");
            }
            nativeState.Step();
            installedState.Step();
            nativeAudio.AdvanceFrame(default);
            installedAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.SeeYouNextMission, nativeState.Stage.Phase,
            "native ending reaches final hold in palette parity fixture");
        AssertEqual(nativeState.Stage.Phase, installedState.Stage.Phase,
            "installed ending reaches the same final hold");
        rejectAllCartridgeReads = false;
        foreach (EndingCreditsPhase phase in new[]
        {
            EndingCreditsPhase.WaitForEscapeMusic,
            EndingCreditsPhase.FadeInEscapeSceneB,
            EndingCreditsPhase.FadeInZebesExplosion,
            EndingCreditsPhase.ZebesExplosionHold,
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
        Suite(nameof(VerifyEndingLogoPaletteArtwork), () => VerifyEndingLogoPaletteArtwork(EndingPaletteImport.Read(nativeBus), nativeBus, guardedBus));

        Console.WriteLine("Ending colours: installed scene backdrops, the decoded logo colours and a ROM-free installed scene/audio-step guard pass.");
    }

    /// <summary>Compares the logo's pre-coloured shades with the cartridge's colours while prohibiting source color reads.</summary>
    private static void VerifyEndingLogoPaletteArtwork(EndingPaletteSet decoded,
        ISnesAddressSpace nativeBus, MutableCartridgeReadGuard guardedBus)
    {
        // The import pre-colours the logo's first sprite colours and each crossfade step from the decoded colours
        // (#1512, #1544): each shade holds the cartridge's colours, the step's background colours before its sprite colours.
        IReadOnlyList<SheetShade> shades = EndingScreenShades.Screen(decoded, InstalledFixtures.ImportMaps.RoomPaletteFx);
        byte[] nativeInitial = EndingPaletteImport.ReadNativePalette(nativeBus,
            EndingPaletteId.LogoInitial, EndingPaletteImport.ColorCount(EndingPaletteId.LogoInitial));
        SheetShade initial = shades.Single(shade => shade.Shade == EndingShades.Loaded(EndingColourLoad.LogoInitial));
        AssertTrue(initial.Inks.SequenceEqual(Enumerable.Range(240, 16)), "the logo's first colours cover its sprite colours");
        for (int color = 0; color < 16; color++)
            AssertEqual(LittleEndianBytes.Word(nativeInitial[color * 2], nativeInitial[color * 2 + 1]), initial.Colours[color].ToWord(),
                "installed initial logo colours match the cartridge");
        byte[] nativeColors = EndingPaletteImport.ReadNativePalette(nativeBus,
            EndingPaletteId.LogoCrossfade, EndingPaletteImport.ColorCount(EndingPaletteId.LogoCrossfade));
        for (int step = 0; step < EndingShades.LogoCrossfadeSteps; step++)
        {
            SheetShade shown = shades.Single(shade => shade.Shade == EndingShades.LogoCrossfade(step));
            for (int palette = 0; palette < 2; palette++)
                for (int color = 0; color < 16; color++)
                {
                    int offset = ((step * 2 + palette) * 16 + color) * 2;
                    AssertEqual(LittleEndianBytes.Word(nativeColors[offset], nativeColors[offset + 1]), shown.Colours[palette * 16 + color].ToWord(),
                        $"installed logo crossfade step {step} matches the cartridge");
                }
        }
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed logo never reads either native palette color source");
    }

    /// <summary>Cartridge color intervals prohibited during ROM-free installed ending playback.</summary>
    private static readonly EndingPaletteArtworkVerificationValue[] EndingPaletteSourceRanges =
        Enum.GetValues<EndingPaletteId>()
            .Where(id => id != EndingPaletteId.LogoCrossfade)
            .Select(id => new EndingPaletteArtworkVerificationValue(id, EndingPaletteSources.Source(id),
                EndingPaletteSources.Source(id) +
                EndingPaletteImport.ColorCount(id) * sizeof(ushort)))
            .Append(new EndingPaletteArtworkVerificationValue(EndingPaletteId.LogoCrossfade, 0x8cefe9, 0x8cf3e9))
            .ToArray();

    /// <summary>
    /// Forbids every ending colour source. While <paramref name="rejectAllCartridgeReads"/>
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

    /// <summary>
    /// Checks that an ending backdrop document whose space behind the exploding planet lacks a frame's backdrop, or holds a
    /// channel outside the fade scale, fails to load (#1637).
    /// </summary>
    /// <param name="installed">The installed ending backdrop directory, whose document each case edits a copy of.</param>
    private static void VerifyEndingBackdropsRejectMalformedSpace(string installed)
    {
        var document = (System.Text.Json.Nodes.JsonObject)(System.Text.Json.Nodes.JsonNode.Parse(
            File.ReadAllText(Path.Combine(installed, EndingBackdropFiles.FileName)))
            ?? throw new InvalidDataException("The installed ending backdrops are null."));
        string directory = Path.Combine(Path.GetTempPath(), "ending-backdrops-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            Reject(space => space.RemoveAt(space.Count - 1), "an ending backdrop document without a frame's space backdrop");
            Reject(space => space[0]!["blue"] = FadeLevelColour.FullLevel + 1, "an ending space backdrop channel outside the fade scale");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }

        void Reject(Action<System.Text.Json.Nodes.JsonArray> edit, string description)
        {
            var copy = (System.Text.Json.Nodes.JsonObject)document.DeepClone();
            edit((System.Text.Json.Nodes.JsonArray)(copy["spaceBehindExplodingPlanet"]
                ?? throw new InvalidDataException("The installed ending backdrops hold no space backdrops.")));
            File.WriteAllText(Path.Combine(directory, EndingBackdropFiles.FileName), copy.ToJsonString());
            AssertThrows<InvalidDataException>(() => EndingBackdropFiles.Load(directory), "reject " + description);
        }
    }
}
