using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;

/// <summary>Checks imported Samus atmospheric art.</summary>
internal static class SamusAtmosphereArtworkBoundaryVerification
{
    /// <summary>Particles holding one particle of <paramref name="kind"/> on <paramref name="step"/>, shown for two updates.</summary>
    private static SamusParticles OneParticle(SamusParticleKind kind, int step)
    {
        var particles = new SamusParticles();
        particles.Add(new SamusParticle(kind, step, SamusParticleStart.Shown(2), new(128, 128), Offset16<RoomPixels>.Zero));
        return particles;
    }

    /// <summary>Compares authored atmospheric frame attributes with cartridge words and checks missing-art rejection.</summary>
    internal static void VerifySamusAtmosphereArtworkBoundary(string sourceRom)
    {
        var importBus = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        SamusAtmosphericSourceArtwork artwork = InstalledFixtures.SamusBody.Atmosphere;

        foreach (byte type in new byte[] { 1, 4, 6, 7 })
        for (byte frame = 0; frame < SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount; frame++)
        {
            ushort list = type == 1
                ? SamusSourceDefinitions.TypeOneAtmosphericAttributes
                : SamusSourceDefinitions.SharedAtmosphericAttributes;
            int source = SamusSourceDefinitions.MovementBank.Value | (list + frame * sizeof(ushort));
            ushort expected = importBus.ReadCartridgeWord(source);
            SamusParticles effects = OneParticle(SamusAtmosphericEffectWordCodec.KindNumbered(type)
                ?? throw new InvalidDataException($"Effect number {type} names no particle."), frame);
            var oamFrame = new SpriteFrameBuffer();
            oamFrame.BeginFrame();
            OamBuffer oam;
            effects.UpdateAndDraw(oamFrame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(atmosphere: artwork));
            oam = oamFrame.FinalizeAsOam();
            AssertEqual((byte)expected, oam.LowTable[2],
                $"atmospheric type {type} frame {frame} low attribute uses imported art");
            AssertEqual(LittleEndianBytes.HighByte(expected), oam.LowTable[3],
                $"atmospheric type {type} frame {frame} high attribute uses imported art");
        }

        SamusParticles missing = OneParticle(SamusParticleKind.SurfaceSplash, 0);
        AssertThrows<InvalidOperationException>(() =>
        {
            var oamFrame = new SpriteFrameBuffer();
            oamFrame.BeginFrame();
            OamBuffer oam;
            missing.UpdateAndDraw(oamFrame, new(0, 0), 128);
            oam = oamFrame.FinalizeAsOam();
        }, "authored atmospheric art cannot fall back to ROM");

        Console.WriteLine("Samus atmospheric artwork: all 16 authored frames use installed assets.");
    }

    /// <summary>Runs the atmospheric artwork boundary check using the repository cartridge.</summary>
    [VerificationCommand("--samus-atmosphere-artwork-boundary")]
    internal static int SamusAtmosphereArtworkBoundaryCommand()
    {
        string atmosphereRom = RepositoryRomPath;
        Suite(nameof(VerifySamusAtmosphereArtworkBoundary), () => VerifySamusAtmosphereArtworkBoundary(atmosphereRom));
        return 0;
    }
}
