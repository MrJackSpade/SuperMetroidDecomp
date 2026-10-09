using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Verifies the elevatube's physical draw, sixteen-frame hold, sound instruction, and deletion without source reads.</summary>
    private static void VerifyMaridiaElevatubePlm()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevatube oracle revision");
        Suite(nameof(VerifyElevatubePhysicalDraw), () => VerifyElevatubePhysicalDraw(rom));
        Suite(nameof(VerifyElevatubeProgramControls), () => VerifyElevatubeProgramControls(rom));
        Suite(nameof(VerifyElevatubeProgramDraw), () => VerifyElevatubeProgramDraw(rom));
        Suite(nameof(VerifyElevatubeProgramSound), () => VerifyElevatubeProgramSound(rom));

        RoomLevelData level = CreateRoom(4, 4, new ushort[16], new byte[16],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnMaridiaElevatube(level),
            "native door setup allocates the elevatube PLM");
        var guarded = new MaridiaElevatubeSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int blockIndex = level.GetBlockIndex(
            MaridiaElevatubePlmRomData.BlockX,
            MaridiaElevatubePlmRomData.BlockY);
        int heardAtFrame = -1;
        for (int frame = 0; frame < 24 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(guarded, level, streamer, 0, 0, 0);
            if (frame == 0)
                AssertEqual((ushort)0x8180,
                    level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                    "first elevatube handler pass draws its physical block");
            if (plms.SoundRequests.Any(request =>
                    request.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        MaridiaElevatubePlmDefinitions.SoundId)))
                heardAtFrame = frame;
        }
        AssertEqual(16, heardAtFrame,
            "elevatube sound follows the native sixteen-frame hold");
        AssertEqual(0, plms.ActiveCount,
            "elevatube PLM deletes after the sound instruction");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "elevatube program and draw execute without their source ROM bytes");
        Console.WriteLine(
            "Maridia elevatube PLM: native delay, draw, sound, and delete pass with source reads forbidden.");
    }

    /// <summary>Rejects runtime reads from the compiled elevatube instruction and draw records.</summary>
    /// <param name="source">Address space that serves unrelated reads and receives writes.</param>
    private sealed class MaridiaElevatubeSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Number of attempted reads from the migrated elevatube source ranges.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the same source-range guard.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Blocks bank $84 reads within the instruction and draw records, forwarding other addresses.</summary>
        /// <param name="address">Full cartridge address requested by the caller.</param>
        /// <returns>The source byte when the address is outside the compiled elevatube ranges.</returns>
        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= MaridiaElevatubePlmDefinitions.InstructionList &&
                  pointer < MaridiaElevatubePlmDefinitions.InstructionList + 9) ||
                 (pointer >= MaridiaElevatubePlmDefinitions.DrawPointer &&
                  pointer < MaridiaElevatubePlmDefinitions.DrawPointer + 6)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Elevatube PLM reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Full cartridge address to write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
