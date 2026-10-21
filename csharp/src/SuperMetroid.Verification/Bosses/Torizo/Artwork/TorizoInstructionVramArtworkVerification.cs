using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Torizo.Artwork;

/// <summary>Checks Torizo instruction-triggered VRAM transfers against cartridge bytes and installed artwork edits.</summary>
internal static class TorizoInstructionVramArtworkVerification
{
    /// <summary>Matches all Torizo transfer descriptors and bytes to cartridge data, then checks live PNG edits and rejection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo instruction VRAM artwork")]
    internal static void VerifyTorizoInstructionVramArtwork(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        AssertEqual(52, TorizoInstructionVramTransferDefinitions.All.Length,
            "all bank-AA Torizo $814B transfer descriptors are catalogued");
        AssertTrue(stock.Bosses.TorizoInstructionVram is not null,
            "installed Torizo instruction tile pages exist");
        var installedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses, AreaArtwork = stock.Areas };
        RoomEnemySlot installedSlot = installedEnemies.SlotTable.Slots[0];
        installedSlot.Bind(EnemyDefinitionId.GoldenTorizo, installedSlot.Definition);
        installedSlot.Bind(installedSlot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
        {
            Bank = TorizoInstructionVramTransferDefinitions.Bank,
        });

        ushort previousInstruction = 0;
        var authoredOpcodes = new HashSet<ushort>();
        for (int transferIndex = 0; transferIndex < TorizoInstructionVramTransferDefinitions.All.Length; transferIndex++)
        {
            TorizoInstructionVramTransferDefinition transfer = TorizoInstructionVramTransferDefinitions.All[transferIndex];
            ushort instruction = TorizoInstructionVramTransferDefinitions.Sites[transferIndex].Instruction;
            AssertTrue(instruction > previousInstruction,
                "Torizo transfer opcodes are distinct and sorted");
            previousInstruction = instruction;
            authoredOpcodes.Add(instruction);
            AssertEqual(CommonEnemyInstructionConstants.CopyToVram,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaa0000 | instruction)),
                $"Torizo transfer $AA:{instruction:X4} opcode");
            int descriptor = 0xaa0000 | unchecked((ushort)(instruction + 2));
            AssertEqual(transfer.ByteCount, CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor),
                $"Torizo transfer $AA:{instruction:X4} byte count");
            int nativeSource = (CartridgeImportSource.Require(rom).ReadCartridgeLong(descriptor + 2));
            AssertEqual(transfer.SourceAddress, nativeSource,
                $"Torizo transfer $AA:{instruction:X4} source");
            AssertEqual(transfer.DestinationWord, CartridgeImportSource.Require(rom).ReadCartridgeWord((descriptor + 5)),
                $"Torizo transfer $AA:{instruction:X4} VRAM destination");
            AssertTrue((stock.Bosses.TorizoInstructionVram ?? throw new InvalidOperationException("Verification requires stock.TorizoInstructionVram.")).TryResolve(
                    transfer.SourceAddress, transfer.ByteCount,
                    out ReadOnlyMemory<byte> installed),
                $"Torizo transfer $AA:{instruction:X4} installed tile source");
            for (int offset = 0; offset < transfer.ByteCount; offset++)
                AssertEqual(rom.ReadCartridgeByte(transfer.SourceAddress + offset),
                    installed.Span[offset],
                    $"Torizo transfer $AA:{instruction:X4} byte {offset:X4}");
            UploadTorizoTiles(installedEnemies, installedSlot, transferIndex);
            ReadOnlySpan<byte> shown = new ObjCharacterTablePage().Page(installedEnemies.Room.Session.ObjCharacters,
                new EnemyTileTransfers(stock)).Characters.Slice((transfer.DestinationWord - GameplayVramLayout.ObjectCharacters.Offset) * 2);
            for (int offset = 0; offset < transfer.ByteCount; offset++)
                AssertEqual(installed.Span[offset], shown[offset],
                    $"production Torizo upload $AA:{instruction:X4} byte {offset:X4}");
        }

        // Detect a missed valid $814B descriptor in the native instruction
        // region, not merely incorrect values in the hand-catalogued records.
        for (int address = 0xb000; address < 0xd369; address++)
        {
            int opcode = 0xaa0000 | address;
            if (CartridgeImportSource.Require(rom).ReadCartridgeWord(opcode) != CommonEnemyInstructionConstants.CopyToVram)
                continue;
            int descriptor = opcode + 2;
            ushort byteCount = CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor);
            int source = (CartridgeImportSource.Require(rom).ReadCartridgeLong(descriptor + 2));
            ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord((descriptor + 5));
            if (byteCount is 0 or > 0x2000 ||
                (SnesAddressMath.BankNumber(source)) < 0x80 || destination > 0x7fff)
                continue;
            AssertTrue(authoredOpcodes.Contains(unchecked((ushort)address)),
                $"native Torizo $814B descriptor $AA:{address:X4} is catalogued");
        }

        TileSheetDefinition golden =
            TorizoInstructionVramArtworkDefinitions.GoldenAwakening;
        string overrideDirectory = Path.Combine(stockDirectory,
            "torizo-instruction-tile-overrides");
        Directory.CreateDirectory(overrideDirectory);
        using (var input = new MemoryStream(File.ReadAllBytes(
                   Path.Combine(stockDirectory, golden.FileName)), writable: false))
        {
            int tiles = golden.ByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tiles, RoomCharacterAtlasFormat.TileColumns);
            int rows = (tiles + columns - 1) / columns;
            IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overrideDirectory,
                golden.FileName));
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                image.Pixels, image.Palette);
        }
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((stock.Bosses.TorizoInstructionVram ?? throw new InvalidOperationException("Verification requires stock.TorizoInstructionVram.")).TryResolve(
                golden.SourceAddress, golden.ByteCount,
                out ReadOnlyMemory<byte> stockBytes),
            "stock Golden Torizo upload remains addressable");
        AssertTrue((edited.Bosses.TorizoInstructionVram ?? throw new InvalidOperationException("Verification requires edited.TorizoInstructionVram.")).TryResolve(
                golden.SourceAddress, golden.ByteCount,
                out ReadOnlyMemory<byte> editedBytes),
            "edited Golden Torizo upload remains addressable");
        AssertEqual((byte)(stockBytes.Span[0] ^ 0x80), editedBytes.Span[0],
            "Golden Torizo PNG edit changes the first native tile-plane byte");
        AssertTrue(stockBytes.Span[1..].SequenceEqual(editedBytes.Span[1..]),
            "Golden Torizo PNG edit leaves every other tile byte intact");
        var editedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        RoomEnemySlot editedSlot = editedEnemies.SlotTable.Slots[0];
        editedSlot.Bind(EnemyDefinitionId.GoldenTorizo, editedSlot.Definition);
        editedSlot.Bind(editedSlot.EnemyDefinitionPointer, installedSlot.Definition);
        // Golden Torizo's initial program uploads its awakening sheet first.
        int goldenUpload = TorizoAnimations.Programs.Steps(TorizoBodyProgram.GoldenInitial0)
            .OfType<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.CueStep>().Select(step => step.Cue).OfType<TorizoCue.CopyTiles>().First().Transfer;
        AssertEqual(golden.SourceAddress, TorizoInstructionVramTransferDefinitions.All[goldenUpload].SourceAddress,
            "Golden Torizo's initial upload copies its awakening sheet");
        UploadTorizoTiles(editedEnemies, editedSlot, goldenUpload);
        ObjCharacterUpload goldenTiles = editedEnemies.Room.Session.ObjCharacters.Uploads.Single();
        AssertEqual<ObjCharacterSource>(new TorizoTileTransferCharacters(goldenUpload), goldenTiles.Source,
            "Golden Torizo's upload binds its installed transfer by entry");
        AssertEqual(editedBytes.Span[0], new ObjCharacterTablePage().Page(editedEnemies.Room.Session.ObjCharacters,
                new EnemyTileTransfers(edited)).Characters[(0x6d00 - GameplayVramLayout.ObjectCharacters.Offset) * 2],
            "edited Golden Torizo PNG changes the production OBJ page upload");
        AssertTrue((EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Bosses.TorizoInstructionVram ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory) .TorizoInstructionVram.")).TryResolve(golden.SourceAddress,
                    golden.ByteCount, out ReadOnlyMemory<byte> reloaded) &&
                   reloaded.Span.SequenceEqual(editedBytes.Span),
            "Golden Torizo tile override survives catalog reload");
        File.WriteAllBytes(Path.Combine(overrideDirectory, golden.FileName), [1, 2, 3]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "invalid Golden Torizo PNG fails loudly");

        Console.WriteLine(
            "Torizo instruction VRAM: 52 native descriptors, eight installed tile PNGs, " +
            "byte parity, live edit, reload, and invalid-asset rejection pass.");
    }

    /// <summary>Copies tile transfer <paramref name="transfer"/> through Torizo's production tile-copy cue.</summary>
    private static void UploadTorizoTiles(RoomEnemySystem enemies, RoomEnemySlot slot, int transfer) =>
        AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(slot, new TorizoEnemyState(slot, isGolden: true),
                new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounterLow = 0 },
                new TorizoCue.CopyTiles(Transfer: transfer),
                AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.GoldenInitial0)) is null,
            $"Torizo tile transfer {transfer} runs on");

}
