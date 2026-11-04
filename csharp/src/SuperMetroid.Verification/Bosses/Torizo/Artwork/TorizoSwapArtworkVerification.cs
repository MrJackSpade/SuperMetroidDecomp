using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.Verification.Bosses.Torizo.Artwork;

/// <summary>Checks Torizo instruction-triggered VRAM transfers against cartridge bytes and installed artwork edits.</summary>
internal static class TorizoSwapArtworkVerification
{
    /// <summary>Matches all Torizo transfer descriptors and bytes to cartridge data, then checks live PNG edits and rejection.</summary>
    internal static void VerifyTorizoSwapArtwork(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        AssertEqual(52, TorizoArtworkSwapDefinitions.All.Length,
            "all bank-AA Torizo $814B transfer descriptors are catalogued");
        AssertTrue(stock.Bosses.TorizoSwaps is not null,
            "installed Torizo instruction tile pages exist");
        var installedEnemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        RoomEnemySlot installedSlot = installedEnemies.SlotTable.Slots[0];
        installedSlot.Bind(EnemyDefinitionId.GoldenTorizo, installedSlot.Definition);
        installedSlot.Bind(installedSlot.DefinitionId, default(RoomEnemyDefinition));

        ushort previousInstruction = 0;
        var authoredOpcodes = new HashSet<ushort>();
        for (int transferIndex = 0; transferIndex < TorizoArtworkSwapDefinitions.All.Length; transferIndex++)
        {
            TorizoArtworkSwapDefinition transfer = TorizoArtworkSwapDefinitions.All[transferIndex];
            ushort instruction = TorizoArtworkSwapDefinitions.Sites[transferIndex].Instruction;
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
            (EncounterArtworkSequence sequence, int step) = StepOf(new TorizoArtworkSwap(transferIndex));
            EncounterArtworkUpload upload = EncounterArtworkUploads.Step(sequence, step, new EnemyTileTransfers(stock)).Single();
            AssertEqual(ObjCharacterNativeWords.Of(upload.FirstByte / 32), CartridgeImportSource.Require(rom).ReadCartridgeWord((descriptor + 5)),
                $"Torizo transfer $AA:{instruction:X4} VRAM destination");
            AssertTrue((stock.Bosses.TorizoSwaps ?? throw new InvalidOperationException("Verification requires stock.TorizoSwaps.")).TryResolve(
                    transfer.SourceAddress, transfer.ByteCount,
                    out ReadOnlyMemory<byte> installed),
                $"Torizo transfer $AA:{instruction:X4} installed tile source");
            for (int offset = 0; offset < transfer.ByteCount; offset++)
                AssertEqual(rom.ReadCartridgeByte(transfer.SourceAddress + offset),
                    installed.Span[offset],
                    $"Torizo transfer $AA:{instruction:X4} byte {offset:X4}");
            ShowTorizoArtwork(installedEnemies, installedSlot, new TorizoArtworkSwap(transferIndex));
            AssertEqual<int?>(step, installedEnemies.Room.Session.ArtworkProgress.StepOf(sequence),
                $"production Torizo swap $AA:{instruction:X4} reaches its step of {sequence}");
            for (int offset = 0; offset < transfer.ByteCount; offset++)
                AssertEqual(installed.Span[offset], upload.Characters.Span[offset],
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

        TorizoTileSheet golden =
            TorizoSwapArtworkDefinitions.GoldenAwakening;
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
                image.Pixels, image.Colours);
        }
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((stock.Bosses.TorizoSwaps ?? throw new InvalidOperationException("Verification requires stock.TorizoSwaps.")).TryResolve(
                golden.SourceAddress, golden.ByteCount,
                out ReadOnlyMemory<byte> stockBytes),
            "stock Golden Torizo upload remains addressable");
        AssertTrue((edited.Bosses.TorizoSwaps ?? throw new InvalidOperationException("Verification requires edited.TorizoSwaps.")).TryResolve(
                golden.SourceAddress, golden.ByteCount,
                out ReadOnlyMemory<byte> editedBytes),
            "edited Golden Torizo upload remains addressable");
        AssertEqual((byte)(stockBytes.Span[0] ^ 0x80), editedBytes.Span[0],
            "Golden Torizo PNG edit changes the first native tile-plane byte");
        AssertTrue(stockBytes.Span[1..].SequenceEqual(editedBytes.Span[1..]),
            "Golden Torizo PNG edit leaves every other tile byte intact");
        var editedEnemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses };
        RoomEnemySlot editedSlot = editedEnemies.SlotTable.Slots[0];
        editedSlot.Bind(EnemyDefinitionId.GoldenTorizo, editedSlot.Definition);
        editedSlot.Bind(editedSlot.DefinitionId, installedSlot.Definition);
        // Golden Torizo's initial program shows its awakening sheet first.
        TorizoArtworkSwap goldenSwap = TorizoAnimations.Programs.Steps(TorizoBodyProgram.GoldenInitial0)
            .OfType<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.CueStep>().Select(step => step.Cue).OfType<TorizoCue.ShowArtwork>().First().Swap;
        int goldenUpload = goldenSwap.Ordinal;
        AssertEqual(golden.SourceAddress, TorizoArtworkSwapDefinitions.All[goldenUpload].SourceAddress,
            "Golden Torizo's initial swap shows its awakening sheet");
        ShowTorizoArtwork(editedEnemies, editedSlot, goldenSwap);
        (EncounterArtworkSequence goldenSequence, int goldenStep) = StepOf(goldenSwap);
        AssertEqual<int?>(goldenStep, editedEnemies.Room.Session.ArtworkProgress.StepOf(goldenSequence),
            "Golden Torizo's swap reaches its sequence's first step");
        EncounterArtworkUpload awakening = EncounterArtworkUploads.Step(goldenSequence, goldenStep, new EnemyTileTransfers(edited)).Single();
        AssertEqual((0x6d00 - GameplayVramLayoutConstants.ObjectCharacters.Offset) * 2, awakening.FirstByte,
            "Golden Torizo's awakening sheet lands where the cartridge copies it");
        AssertEqual(editedBytes.Span[0], awakening.Characters.Span[0],
            "edited Golden Torizo PNG changes the pictured awakening sheet");
        AssertTrue((EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Bosses.TorizoSwaps ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory) .TorizoSwaps.")).TryResolve(golden.SourceAddress,
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

    /// <summary>The artwork sequence and step <paramref name="swap"/> takes: Golden Torizo's swaps, else Bomb Torizo's (#1513).</summary>
    private static EncounterArtworkStep StepOf(TorizoArtworkSwap swap)
    {
        int golden = TorizoArtworkSwapDefinitions.GoldenSequence.ToList().IndexOf(swap);
        return golden >= 0 ? new EncounterArtworkStep(EncounterArtworkSequence.GoldenTorizo, golden)
            : new EncounterArtworkStep(EncounterArtworkSequence.BombTorizo, TorizoArtworkSwapDefinitions.BombSequence.ToList().IndexOf(swap));
    }

    /// <summary>Shows artwork swap <paramref name="swap"/> through the production cue of the Torizo whose programs make it.</summary>
    private static void ShowTorizoArtwork(RoomEnemySystem enemies, RoomEnemySlot slot, TorizoArtworkSwap swap) =>
        AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(slot,
                new TorizoEnemyState(slot, isGolden: StepOf(swap).Sequence == EncounterArtworkSequence.GoldenTorizo),
                new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounterLow = 0 },
                new TorizoCue.ShowArtwork(swap),
                AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.GoldenInitial0)) is null,
            $"Torizo artwork swap {swap.Ordinal} runs on");

}
