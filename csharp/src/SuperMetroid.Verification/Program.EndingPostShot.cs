using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Verifies the post-shot cinematic's frame timing, palette fades, and exact VRAM upload sequence.</summary>
    /// <param name="bus">The retail cartridge bus supplying source artwork and native fixture data.</param>
    private static void VerifyEndingPostShot(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyEndingPostShotTransferFields), () => VerifyEndingPostShotTransferFields(bus));
        var guardedBus = new PostShotUploadTableReadGuard(bus);
        var cgram = new SnesCgram();
        SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(cgram, bus, 0x8ce7e9);
        ushort[] initial = cgram.Colors.ToArray();
        var vram = new SnesVram();
        var expected = Enumerable.Repeat((byte)0xa5, SnesVram.ByteCount).ToArray();
        vram.LoadBytes(0, expected);
        byte[] font = RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(bus), 0x97e7de, 0x8000);
        using var fontPng = new MemoryStream(
            SuperMetroid.AssetExtraction.EndingFontAtlasExtractor.Extract(bus), writable: false);
        EndingFontAtlas fontAtlas = EndingFontAtlas.Load(fontPng);
        var shot = new EndingPostShot(guardedBus, cgram, fontAtlas, RepositoryInstallation.Installation.LoadEndingObjectArt());
        byte[] tiles = RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(bus), 0x99e089, 0x8000);
        byte[] map = RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(bus), 0x99ecc4, 0x8000);
        for (int frame = 1; frame <= 216; frame++)
        {
            shot.Step(vram, cgram);
            AssertEqual(Math.Max(24, 2304 - Math.Min(frame, 36) * 64), shot.Scale, "native post-shot scale trajectory");
            AssertEqual(unchecked((byte)(-8 * Math.Min(frame, 36))), shot.Angle, "native post-shot rotation trajectory");
            AssertEqual(frame >= 36, shot.RotationFinished, "rotation handoff occurs on call 36");
            AssertEqual(frame == 216, shot.ReadyForWhiteFlash, "white-flash handoff follows a full 180-frame hold");
            AssertEqual(Math.Clamp(frame - 47, 0, 6), shot.Uploads, "logo uploads wait for Samus fade, one transfer per frame");
            foreach (int start in new[] { 192, 240 })
            {
                int remaining = 32 - Math.Clamp(frame - (start == 240 ? 16 : 0), 0, 32);
                for (int i = start; i < start + 16; i++)
                {
                    ushort color = initial[i];
                    ushort faded = (ushort)(((color & 31) * remaining / 32)
                        | ((color >> 5 & 31) * remaining / 32) << 5
                        | ((color >> 10 & 31) * remaining / 32) << 10);
                    AssertEqual(faded, cgram.Colors[i], "post-shot palette component and delayed Samus fade");
                }
            }
            if (frame == 48) font.AsSpan(0x1000, 0x400).CopyTo(expected.AsSpan(0x9000));
            if (frame >= 49 && frame <= 52)
            {
                int offset = (frame - 49) * 0x800;
                tiles.AsSpan(offset, 0x800).CopyTo(expected.AsSpan(0xc000 + offset));
            }
            if (frame == 53) map.AsSpan(0, 0x800).CopyTo(expected.AsSpan(0xa800));
            AssertTrue(expected.AsSpan().SequenceEqual(vram.Bytes), "each native logo transfer changes exactly its intended VRAM bytes");
        }
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "post-shot uploads never reread the fixed cartridge transfer table");
        Console.WriteLine("  Post-shot: 36 rotation frames, delayed palette fades, six exact uploads, 180-frame hold.");
    }

    /// <summary>Rejects rereads of the fixed post-shot upload table while delegating all other cartridge access.</summary>
    /// <param name="source">The underlying cartridge bus used for addresses outside the upload table.</param>
    private sealed class PostShotUploadTableReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from the fixed post-shot upload transfer table.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes a cartridge-import read through the same upload-table guard as ordinary bus reads.</summary>
        /// <param name="address">The bus address to read.</param>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the fixed upload table and forwards all other reads to the cartridge bus.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The underlying cartridge byte for an address outside the guarded table.</returns>
        public byte ReadByte(int address)
        {
            if (address >= EndingPostShotUploadDefinitions.TableAddress &&
                address < EndingPostShotUploadDefinitions.TableAddress +
                    EndingPostShotUploadDefinitions.Count * EndingPostShotUploadDefinitions.RecordBytes)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Post-shot upload reread fixed cartridge table at ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards a write to the underlying cartridge bus.</summary>
        /// <param name="address">The bus address to write.</param>
        /// <param name="value">The byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
