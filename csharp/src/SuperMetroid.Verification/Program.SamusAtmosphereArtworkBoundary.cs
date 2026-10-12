using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class SamusAtmosphereArtworkBoundaryVerification
{

    /// <summary>Seeds the WRAM bytes a null bank-$90 atmospheric pointer reads through its system-bank mirror.</summary>
    [CpuBusBoundary("exact bus test: seeds low WRAM that a null bank-$90 atmospheric pointer reads through the $90:0000 mirror")]
    private static void SeedNullAtmosphericPointerWram(SuperMetroidAddressSpace bus)
    {
        bus.WriteByte(SamusMovementRomData.Banks.Movement, 0x5a);
        bus.WriteByte(SamusMovementRomData.Banks.Movement + 1, 0x34);
    }

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
            for (byte frame = 0; frame < SamusMovementRomData.Environment.DirectAtmosphericFrameCount; frame++)
            {
                ushort list = type == 1
                    ? SamusMovementRomData.Environment.TypeOneAtmosphericAttributes
                    : SamusMovementRomData.Environment.SharedAtmosphericAttributes;
                int source = SamusMovementRomData.Banks.Movement | (list + frame * sizeof(ushort));
                ushort expected = importBus.ReadCartridgeWord(source);
                var effects = new SamusAtmosphericEffectsState();
                effects.SetSlot(0, type, frame, 2, new(128, 128));
                var oam = new OamBuffer();
                oam.BeginFrame();
                effects.UpdateAndDraw(runtimeBus, oam, new(0, 0), 128,
                    directArtwork: artwork);
                AssertEqual((byte)expected, oam.LowTable[2],
                    $"atmospheric type {type} frame {frame} low attribute uses imported art");
                AssertEqual(SnesLittleEndian.HighByte(expected), oam.LowTable[3],
                    $"atmospheric type {type} frame {frame} high attribute uses imported art");
            }

            var missing = new SamusAtmosphericEffectsState();
            missing.SetSlot(0, 1, 0, 2, new(128, 128));
            AssertThrows<InvalidOperationException>(() =>
            {
                var oam = new OamBuffer();
                oam.BeginFrame();
                missing.UpdateAndDraw(runtimeBus, oam, new(0, 0), 128);
            }, "authored atmospheric art cannot fall back to ROM");

            SeedNullAtmosphericPointerWram(runtimeBus);
            var mutable = new SamusAtmosphericEffectsState();
            mutable.SetSlot(0, 2, 0, 2, new(128, 128));
            var mutableOam = new OamBuffer();
            mutableOam.BeginFrame();
            mutable.UpdateAndDraw(runtimeBus, mutableOam, new(0, 0), 128);
            AssertEqual((byte)0x5a, mutableOam.LowTable[2],
                "type-two null pointer reads the live mirrored WRAM low byte");
            AssertEqual((byte)0x34, mutableOam.LowTable[3],
                "type-two null pointer reads the live mirrored WRAM high byte");

            Console.WriteLine("Samus atmospheric artwork: all 16 authored frames use installed assets; null-pointer type two reads only live WRAM.");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [VerificationCommand("--samus-atmosphere-artwork-boundary")]
    internal static int SamusAtmosphereArtworkBoundaryCommand()
    {
        string atmosphereRom = RepositoryRomPath;
        Suite(nameof(VerifySamusAtmosphereArtworkBoundary), () => VerifySamusAtmosphereArtworkBoundary(atmosphereRom));
        return 0;
    }
}
