using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks imported authored atmosphere frames render without a cartridge, missing art fails explicitly, and type-two null pointers use live WRAM.</summary>
    /// <param name="sourceRom">Retail ROM path used to extract the authored atmosphere artwork and reference attributes.</param>
    private static void VerifySamusAtmosphereArtworkBoundary(string sourceRom)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "samus-atmosphere-boundary-" + Guid.NewGuid().ToString("N")));
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
                ushort expected = (ushort)(importBus.ReadCartridgeByte(source) |
                    importBus.ReadCartridgeByte(source + 1) << 8);
                var effects = new SamusAtmosphericEffectsState();
                effects.SetSlot(0, type, frame, 2, 128, 128);
                var oam = new OamBuffer();
                oam.BeginFrame();
                effects.UpdateAndDraw(runtimeBus, oam, 0, 0, 128,
                    directArtwork: artwork);
                AssertEqual((byte)expected, oam.LowTable[2],
                    $"atmospheric type {type} frame {frame} low attribute uses imported art");
                AssertEqual((byte)(expected >> 8), oam.LowTable[3],
                    $"atmospheric type {type} frame {frame} high attribute uses imported art");
            }

            var missing = new SamusAtmosphericEffectsState();
            missing.SetSlot(0, 1, 0, 2, 128, 128);
            AssertThrows<InvalidOperationException>(() =>
            {
                var oam = new OamBuffer();
                oam.BeginFrame();
                missing.UpdateAndDraw(runtimeBus, oam, 0, 0, 128);
            }, "authored atmospheric art cannot fall back to ROM");

            runtimeBus.WriteByte(SamusMovementRomData.Banks.Movement, 0x5a);
            runtimeBus.WriteByte(SamusMovementRomData.Banks.Movement + 1, 0x34);
            var mutable = new SamusAtmosphericEffectsState();
            mutable.SetSlot(0, 2, 0, 2, 128, 128);
            var mutableOam = new OamBuffer();
            mutableOam.BeginFrame();
            mutable.UpdateAndDraw(runtimeBus, mutableOam, 0, 0, 128);
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
}
