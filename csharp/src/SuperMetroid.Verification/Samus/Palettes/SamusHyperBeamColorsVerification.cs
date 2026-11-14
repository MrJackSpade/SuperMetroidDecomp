using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Samus.Palettes;

using SuperMetroid.Verification.Harness;

/// <summary>Retail and installed-artwork checks for Samus's ten-frame Hyper Beam rainbow palette.</summary>
internal static class SamusHyperBeamColorsVerification
{
    /// <summary>Checks the installed colours keep every native row and reach the shade CGRAM, and guarded rainbow playback against the supported cartridge.</summary>
    internal static void VerifySamusHyperBeamColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Hyper Beam colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hyper Beam color oracle revision");
        InstalledSamusHyperBeamColors catalog = SamusHyperBeamColorFile.Read(
            new MemoryStream(SamusHyperBeamColorExtractor.Extract(rom)));
        for (int frame = 0; frame < SamusHyperBeamColorFormat.FrameCount; frame++)
        {
            int pointerAddress = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPointers + frame * 2;
            ushort pointer = rom.ReadCartridgeWord(pointerAddress);
            int paletteAddress = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(frame);
            AssertEqual(paletteAddress, SamusSourceDefinitions.PaletteBank.Value | pointer,
                $"Hyper Beam frame {frame} compiled pointer");
            for (int index = 0; index < SamusHyperBeamColorFormat.ColorsPerFrame; index++)
            {
                int wordAddress = paletteAddress + index * 2;
                ushort native = rom.ReadCartridgeWord(wordAddress);
                AssertEqual(native, catalog.Resolve(frame, index),
                    $"Hyper Beam frame {frame} color {index}");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() =>
            SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(10),
            "compiled Hyper Beam pointer table remains bounded");
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(invalid, 0), "Hyper Beam frame bounds");
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(0, invalid), "Hyper Beam color bounds");
        byte[] chargeJson = SamusChargeColorExtractor.Extract(rom);
        var shot = SamusChargeColorFile.Read(new MemoryStream(chargeJson)).ToColours();
        var shotColours = new SamusColours();
        // The Hyper shot names shades (#1512); the import resolves them from these charge colours.
        var shotSources = InstalledFixtures.SamusShadeSources with { Charge = shot };
        for (int frame = 0; frame < 10; frame++)
        {
            ushort pointer = ReadVerificationWord(rom, 0x91d83d - 2 * frame);
            AssertEqual(ReadVerificationWord(rom, 0x91d99e + 2 * (9 - frame)), pointer,
                "Both native pointer views address the same rows in reverse playback order");
            shotColours.ShowSuit(SamusShades.HyperShot(SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount - 1 - frame));
            for (int color = 0; color < 16; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color));
                AssertEqual(expected, shot.ResolveHyper(frame, color), "Every original Hyper-shot color");
                AssertEqual(expected, shotColours.ShownSuit(shotSources)[color], "Every original Hyper-shot shade color");
            }
        }
        foreach (int invalid in new[] { -1, 10, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => shot.ResolveHyper(invalid, 0), "Hyper-shot frame bounds");
        }
        foreach (int invalid in new[] { -1, 16, int.MinValue, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => shot.ResolveHyper(0, invalid), "Hyper-shot color bounds");
        var shotSamus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { HyperBeamEnabled = true };
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        projectiles.Charge.ChargedShotGlow = ChargedShotGlow.HyperBeamGlow;
        var shotGuard = NewForbiddenHyperBeamColorBus(rom);
        for (int call = 0; call < 20; call++)
        {
            var step = projectiles.UpdateBeamChargePalette(shotColours, shotSamus);
            AssertEqual((call & 1) == 0 ? SamusBeamChargePaletteAction.HyperPalette : SamusBeamChargePaletteAction.HyperHold,
                step.Action, "Original Hyper-shot alternating paint/hold cadence");
            ushort pointer = ReadVerificationWord(rom, 0x91d83d - 2 * (call / 2));
            for (int color = 0; color < 16; color++)
                AssertEqual(ReadVerificationWord(rom, 0x9b0000 | (pointer + 2 * color)), shotColours.ShownSuit(shotSources)[color],
                    "Guarded Hyper-shot cycle preserves original shade colors and held rows");
        }
        AssertEqual(0, shotGuard.ForbiddenReadAttempts, "Hyper-shot installed colors avoid original palette and pointer reads");
        var installedSamus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        installedSamus.Drained.EnableRainbow(installedSamus);
        var installedSources = InstalledFixtures.SamusShadeSources with { HyperBeamSuit = catalog.ToColours() };
        var installedColours = new SamusColours();
        var guarded = NewForbiddenHyperBeamColorBus(rom);
        for (int call = 0; call < 22; call++)
        {
            AssertTrue(installedSamus.Drained.UpdatePalette(installedColours, 0),
                $"Hyper Beam call {call} owns palette");
            // Original91D96F copies the selected row before DEC; EnableRainbow's
            // timer/reload are1, so every call advances and wraps at native limit10.
            AssertEqual((ushort)((call + 1) % 10), installedSamus.Drained.HyperBeamGlowStep,
                $"Hyper Beam call {call} next frame");
            AssertEqual(1, installedSamus.Drained.CommonPaletteTimer,
                $"Hyper Beam call {call} native timer");
            ushort originalPointer = ReadVerificationWord(rom, 0x91d99e + 2 * (call % 10));
            for (int color = 0; color < 16; color++)
                AssertEqual((ushort)(ReadVerificationWord(rom, 0x9b0000 | (originalPointer + 2 * color)) & 0x7fff),
                    installedColours.ShownSuit(installedSources)[color], $"Hyper Beam call {call} original shade color {color}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed Hyper Beam cycle does not read pointer or palette ROM tables");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Hyper Beam JSON property fails loudly");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt Hyper Beam JSON fails loudly");
        Console.WriteLine("  Hyper Beam: both native palette views and guarded runtime cycles pass.");
    }

    /// <summary>Checks that the Hyper Beam colour codec rejects out-of-range colours.</summary>
    internal static void VerifySamusHyperBeamColorsRejectMalformedDocuments(string stock)
    {
        SamusHyperBeamColorDocument document = JsonSerializer.Deserialize<SamusHyperBeamColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusHyperBeamColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Hyper Beam color document is null.");
        AssertThrows<InvalidDataException>(() => SamusHyperBeamColorFile.Read(new MemoryStream(SamusHyperBeamColorFile.Write(document with
        {
            Frames = [.. document.Frames.Take(4),
                [.. document.Frames[4].Take(7), document.Frames[4][7] with { Red = 32 },
                 .. document.Frames[4].Skip(8)], .. document.Frames.Skip(5)],
        }))), "out-of-range Hyper Beam RGB5 fails loudly");
    }

    /// <summary>Blocks native Hyper Beam pointer and palette tables while installed colors update Samus.</summary>
    private static CartridgeReadGuard NewForbiddenHyperBeamColorBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int pointerStart = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPointers;
                int paletteStart = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(9);
                int paletteEnd = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteSource(0) +
                    SamusHyperBeamColorFormat.ColorsPerFrame * sizeof(ushort);
                return (address >= 0x91d829 && address < 0x91d83f) ||
                    (address >= pointerStart && address < pointerStart +
                        SamusHyperBeamColorFormat.FrameCount * sizeof(ushort)) ||
                    (address >= paletteStart && address < paletteEnd);
            },
            address => $"Installed Hyper Beam read native artwork ${address:X6}.");

    /// <summary>Runs Samus Hyper Beam color extraction and runtime palette verification.</summary>
    [VerificationCommand("--samus-hyper-beam-colors")]
    internal static int SamusHyperBeamColorsCommand()
    {
        Suite(nameof(VerifySamusHyperBeamColors), () => VerifySamusHyperBeamColors());
        return 0;
    }
}
