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
        TorizoLookUploadDefinitionsConstants.TransferSite[] sites = TorizoLookUploadDefinitions.Sites.ToArray();
        AssertEqual(TorizoLookUploadDefinitions.BombSequence.Length + TorizoLookUploadDefinitions.GoldenSequence.Length, sites.Length,
            "all bank-AA Torizo $814B transfer descriptors are catalogued");
        AssertTrue(sites.Where(site => site.Torizo == EncounterArtwork.BombTorizo).Select(site => site.Look).Order().SequenceEqual(TorizoLookUploadDefinitions.BombSequence.Order())
            && sites.Where(site => site.Torizo == EncounterArtwork.GoldenTorizo).Select(site => site.Look).Order().SequenceEqual(TorizoLookUploadDefinitions.GoldenSequence.Order()),
            "each Torizo's sequence holds the looks its instruction lists copy");
        AssertTrue(stock.Bosses.TorizoSwaps is not null,
            "installed Torizo instruction tile pages exist");
        var installedEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = stock.Bosses };
        RoomEnemySlot installedSlot = installedEnemies.SlotTable.Slots[0];
        installedSlot.Bind(EnemyDefinitionId.GoldenTorizo, installedSlot.Definition);
        installedSlot.Bind(installedSlot.DefinitionId, default(RoomEnemyDefinition));

        ushort previousInstruction = 0;
        var authoredOpcodes = new HashSet<ushort>();
        foreach (TorizoLookUploadDefinitionsConstants.TransferSite site in sites)
        {
            TorizoLookUpload transfer = TorizoLookUploadDefinitions.Uploads[site.Look];
            ushort instruction = site.Instruction;
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
            EncounterLook look = site.Look.Of(site.Torizo);
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
            ShowTorizoArtwork(installedEnemies, installedSlot, site.Torizo, site.Look);
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
        TorizoArtworkLook goldenAwakening = TorizoAnimations.Segments.Beats(TorizoAnimation.GoldenTorizoDormant)
            .OfType<AnimationBeat<EnemyFrameId, TorizoCue>.Event>().Select(beat => beat.Raised).OfType<TorizoCue.ShowArtwork>().First().Look;
        AssertEqual(golden.SourceAddress, TorizoLookUploadDefinitions.Uploads[goldenAwakening].SourceAddress,
            "Golden Torizo's initial look shows its awakening sheet");
        ShowTorizoArtwork(goldenEnemies, goldenSlot, EncounterArtwork.GoldenTorizo, goldenAwakening);
        EncounterLook goldenLook = goldenAwakening.Of(EncounterArtwork.GoldenTorizo);
        AssertTrue(goldenEnemies.Room.Session.Looks.Shows(goldenLook), "Golden Torizo's first look is its awakening");
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

    /// <summary>The one copy Torizo look <paramref name="look"/> makes, resolved against <paramref name="stock"/>.</summary>
    private static EncounterArtworkUpload UploadOf(EncounterLook look, ImportEnemyArtwork stock)
    {
        ComposedEncounter torizo = look.Encounter == EncounterArtwork.GoldenTorizo ? ComposedEncounter.GoldenTorizo : ComposedEncounter.BombTorizo;
        return EncounterLookUploads.Uploads(torizo, EncounterLookUploads.IndexOf(torizo, look.Name), new EnemyTileTransfers(stock)).Single();
    }

    /// <summary>Shows artwork look <paramref name="look"/> through the production cue of <paramref name="torizo"/>, whose programs make it.</summary>
    private static void ShowTorizoArtwork(RoomEnemySystem enemies, RoomEnemySlot slot, EncounterArtwork torizo, TorizoArtworkLook look) =>
        AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(slot,
                new TorizoEnemyState(slot, isGolden: torizo == EncounterArtwork.GoldenTorizo),
                new TorizoCueInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounter = 0 },
                new TorizoCue.ShowArtwork(look),
                TorizoAnimations.Start(TorizoAnimation.GoldenTorizoDormant)) is null,
            $"{torizo} artwork look {look} runs on");
}
