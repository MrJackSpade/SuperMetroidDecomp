using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;

/// <summary>Checks imported Samus atmospheric art and the special live-WRAM null-pointer path.</summary>
internal static class SamusAtmosphereArtworkBoundaryVerification
{

    /// <summary>Seeds the WRAM bytes a null bank-$90 atmospheric pointer reads through its system-bank mirror.</summary>
    private static void SeedNullAtmosphericPointerWram(SuperMetroidAddressSpace bus)
    {
        bus.WriteByte(SamusSourceDefinitions.MovementBank.Value, 0x5a);
        bus.WriteByte(SamusSourceDefinitions.MovementBank.Value + 1, 0x34);
    }

    /// <summary>Compares authored atmospheric frame attributes with cartridge words and checks missing-art rejection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifySamusAtmosphereArtworkBoundary(string sourceRom)
    {
        using var rootScratch = new TestScratchDirectory("samus-atmosphere-boundary");
        string root = rootScratch.Root;
        try
        {
            var importBus = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            SamusAtmosphericArtworkFiles.Extract(importBus, root, SupportedCartridge.Sha256);
            SamusAtmosphericArtworkCatalog artwork = SamusAtmosphericArtworkFiles.Load(root, null);
            var runtimeBus = SuperMetroidAddressSpace.CreateWithoutCartridge();

            foreach (byte type in new byte[] { 1, 4, 6, 7 })
            for (byte frame = 0; frame < SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount; frame++)
            {
                ushort list = type == 1
                    ? SamusSourceDefinitions.TypeOneAtmosphericAttributes
                    : SamusSourceDefinitions.SharedAtmosphericAttributes;
                int source = SamusSourceDefinitions.MovementBank.Value | (list + frame * sizeof(ushort));
                ushort expected = importBus.ReadCartridgeWord(source);
                var effects = new SamusAtmosphericEffectsState();
                effects.SetSlot(0, (SamusAtmosphericEffectType)type, frame, 2, new(128, 128));
                var oamFrame = new SpriteFrameBuffer();
                oamFrame.BeginFrame();
                OamBuffer oam;
                effects.UpdateAndDraw(oamFrame, new(0, 0), 128,
                    SamusBodySheetFixtures.ForAtmosphere(atmosphere: artwork));
                oam = oamFrame.FinalizeAsOam();
                AssertEqual((byte)expected, oam.LowTable[2],
                    $"atmospheric type {type} frame {frame} low attribute uses imported art");
                AssertEqual(SnesLittleEndian.HighByte(expected), oam.LowTable[3],
                    $"atmospheric type {type} frame {frame} high attribute uses imported art");
            }

            var missing = new SamusAtmosphericEffectsState();
            missing.SetSlot(0, SamusAtmosphericEffectType.FootstepSplash, 0, 2, new(128, 128));
            AssertThrows<InvalidOperationException>(() =>
            {
                var oamFrame = new SpriteFrameBuffer();
                oamFrame.BeginFrame();
                OamBuffer oam;
                missing.UpdateAndDraw(oamFrame, new(0, 0), 128);
                oam = oamFrame.FinalizeAsOam();
            }, "authored atmospheric art cannot fall back to ROM");

            SeedNullAtmosphericPointerWram(runtimeBus);
            var mutable = new SamusAtmosphericEffectsState();
            mutable.SetSlot(0, SamusAtmosphericEffectType.AlternateFootstepSplash, 0, 2, new(128, 128));
            OamBuffer mutableOam = SpriteFrameBuffer.DrawnOam(frame =>
                mutable.UpdateAndDraw(frame, new(0, 0), 128));
            OamBuffer emptyOam = SpriteFrameBuffer.DrawnOam(frame =>
                new SamusAtmosphericEffectsState().UpdateAndDraw(frame, new(0, 0), 128));
            AssertTrue(mutableOam.LowTable.ToArray().SequenceEqual(emptyOam.LowTable.ToArray()),
                "type two's null list pointer draws nothing, whatever WRAM holds (#1318)");

            Console.WriteLine("Samus atmospheric artwork: all 16 authored frames use installed assets; null-pointer type two draws nothing.");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
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
