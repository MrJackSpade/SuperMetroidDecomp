using SuperMetroid.Core.Animation;
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
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Bosses.Torizo.Artwork;

/// <summary>Checks Torizo instruction-triggered VRAM transfers against cartridge bytes and the installed artwork.</summary>
internal static class TorizoSwapArtworkVerification
{
    /// <summary>Matches all Torizo transfer descriptors and bytes to cartridge data, then checks malformed artwork rejection.</summary>
    internal static void VerifyTorizoSwapArtwork(
        ISnesAddressSpace rom, ImportEnemyArtwork stock)
    {
        AssertEqual(52, TorizoArtworkSwapDefinitions.All.Length,
            "all bank-AA Torizo $814B transfer descriptors are catalogued");
        AssertTrue(stock.Bosses.TorizoSwaps is not null,
            "installed Torizo instruction tile pages exist");
        var installedEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = stock.Bosses };
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
            EncounterLook look = LookOf(new TorizoArtworkSwap(transferIndex));
            EncounterArtworkUpload upload = UploadOf(look, stock);
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
            AssertTrue(installedEnemies.Room.Session.Looks.Shows(look),
                $"production Torizo swap $AA:{instruction:X4} shows its look {look}");
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
        AssertTrue((stock.Bosses.TorizoSwaps ?? throw new InvalidOperationException("Verification requires stock.TorizoSwaps.")).TryResolve(
                golden.SourceAddress, golden.ByteCount,
                out ReadOnlyMemory<byte> stockBytes),
            "stock Golden Torizo upload remains addressable");
        var goldenEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = stock.Bosses };
        RoomEnemySlot goldenSlot = goldenEnemies.SlotTable.Slots[0];
        goldenSlot.Bind(EnemyDefinitionId.GoldenTorizo, goldenSlot.Definition);
        goldenSlot.Bind(goldenSlot.DefinitionId, installedSlot.Definition);
        // Golden Torizo's initial program shows its awakening sheet first.
        TorizoArtworkSwap goldenSwap = TorizoAnimations.Segments.Beats(TorizoBodySegment.GoldenInitial0)
            .OfType<AnimationBeat<EnemyFrameId, TorizoCue>.Event>().Select(beat => beat.Raised).OfType<TorizoCue.ShowArtwork>().First().Swap;
        int goldenUpload = goldenSwap.Ordinal;
        AssertEqual(golden.SourceAddress, TorizoArtworkSwapDefinitions.All[goldenUpload].SourceAddress,
            "Golden Torizo's initial swap shows its awakening sheet");
        ShowTorizoArtwork(goldenEnemies, goldenSlot, goldenSwap);
        EncounterLook goldenLook = LookOf(goldenSwap);
        AssertTrue(goldenEnemies.Room.Session.Looks.Shows(goldenLook), "Golden Torizo's swap shows its awakening look");
        AssertEqual(1, EncounterLookUploads.IndexOf(ComposedEncounter.GoldenTorizo, goldenLook.Name),
            "Golden Torizo's awakening is the first look after its room's load");
        EncounterArtworkUpload awakening = UploadOf(goldenLook, stock);
        AssertEqual((0x6d00 - GameplayVramLayoutConstants.ObjectCharacters.Offset) * 2, awakening.FirstByte,
            "Golden Torizo's awakening sheet lands where the cartridge copies it");
        AssertTrue(awakening.Characters.Span.SequenceEqual(stockBytes.Span),
            "Golden Torizo's awakening sheet pictures the installed tiles");

        Console.WriteLine(
            "Torizo instruction VRAM: 52 native descriptors, eight installed tile PNGs, " +
            "byte parity and the awakening sheet's upload pass.");
    }

    /// <summary>The look <paramref name="swap"/> gives the Torizo whose programs make it: Golden Torizo's swaps, else Bomb Torizo's (#1538).</summary>
    private static EncounterLook LookOf(TorizoArtworkSwap swap) =>
        swap.Look(TorizoArtworkSwapDefinitions.GoldenSequence.Contains(swap) ? EncounterArtwork.GoldenTorizo : EncounterArtwork.BombTorizo);

    /// <summary>The one copy Torizo look <paramref name="look"/> makes, resolved against <paramref name="stock"/>.</summary>
    private static EncounterArtworkUpload UploadOf(EncounterLook look, ImportEnemyArtwork stock)
    {
        ComposedEncounter torizo = look.Encounter == EncounterArtwork.GoldenTorizo ? ComposedEncounter.GoldenTorizo : ComposedEncounter.BombTorizo;
        return EncounterLookUploads.Uploads(torizo, EncounterLookUploads.IndexOf(torizo, look.Name), new EnemyTileTransfers(stock)).Single();
    }

    /// <summary>Shows artwork swap <paramref name="swap"/> through the production cue of the Torizo whose programs make it.</summary>
    private static void ShowTorizoArtwork(RoomEnemySystem enemies, RoomEnemySlot slot, TorizoArtworkSwap swap) =>
        AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(slot,
                new TorizoEnemyState(slot, isGolden: LookOf(swap).Encounter == EncounterArtwork.GoldenTorizo),
                new TorizoCueInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounter = 0 },
                new TorizoCue.ShowArtwork(swap),
                TorizoAnimations.Start(TorizoBodySegment.GoldenInitial0)) is null,
            $"Torizo artwork swap {swap.Ordinal} runs on");
}
