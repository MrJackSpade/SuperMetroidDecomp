using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Checks editable Spore Spawn ceiling artwork while retaining compiled PLM physics and timing.</summary>
internal static class SporeSpawnCeilingVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifySporeSpawnCeilingVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore ceiling stock oracle revision");
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectSporeSpawnCeilingVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectSporeSpawnCeilingVisualFilesConstants.ValidateStock(
                installation.RoomObjectSporeSpawnCeilingVisualDirectory);
            VerifySporeSpawnCeilingStockMapping(rom, installation.LoadRoomObjectSporeSpawnCeilingVisuals());

            string stockPath = Path.Combine(
                installation.RoomObjectSporeSpawnCeilingVisualDirectory,
                RoomObjectSporeSpawnCeilingVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectSporeSpawnCeilingVisualFilesConstants.ValidateStock(
                    installation.RoomObjectSporeSpawnCeilingVisualDirectory),
                "tampered Spore Spawn ceiling stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Spore Spawn ceiling visuals: four native frames and strict failures pass.");
    }

    /// <summary>Compares flattened stock appearances with the decoded cartridge draws and checks catalog identity, shape, and bounds.</summary>
    private static void VerifySporeSpawnCeilingStockMapping(SuperMetroidAddressSpace rom,
        RoomObjectSporeSpawnCeilingVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9413,"clear-ceiling"), new LookupEscapeGateStockFrame(0x9423,"crumble-frame-0"),
            new LookupEscapeGateStockFrame(0x9433,"crumble-frame-1"), new LookupEscapeGateStockFrame(0x9443,"crumble-frame-2")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            var appearances = new RoomBlockAppearance[4];
            for (int index = 0; index < appearances.Length; index++)
                appearances[index] = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom,
                    0x840000 | (frame.Pointer + index / 2 * 8 + index % 2 * 2 + 2))).Appearance;
            native.Add(frame.Pointer, appearances);
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomObjectSporeSpawnCeilingVisualCatalog.Stock();
        var imported = RoomObjectSporeSpawnCeilingVisualFiles.Create(entries.Reverse());
        // Hash flattened visual frames by native draw pointer for catalog identity comparison.
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomObjectSporeSpawnCeilingVisualCatalog), appearances.ToDictionary(pair => RoomObjectSporeSpawnCeilingVisualFiles.VisualId(NativeObjectDraws.At<SporeSpawnCeilingDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Spore ceiling original flattened hash");
            foreach (var frame in frames)
            {
                for (int index = 0; index < 4; index++)
                    AssertEqual(native[frame.Pointer][index], catalog.GetAppearance(NativeObjectDraws.At<SporeSpawnCeilingDraw>(frame.Pointer),index / 2,index % 2), "Spore ceiling native stock word");
                foreach (int bad in new[] {int.MinValue,-1,2,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<SporeSpawnCeilingDraw>(frame.Pointer),bad,0), "Spore ceiling run bounds");
                    for (int run = 0; run < 2; run++)
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<SporeSpawnCeilingDraw>(frame.Pointer),run,bad), "Spore ceiling block bounds");
                }
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<SporeSpawnCeilingDraw>((ushort)pointer),0,0), "Spore ceiling unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectSporeSpawnCeilingVisualFiles.Create(entries[..3]), "Spore ceiling missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectSporeSpawnCeilingVisualFiles.Create(entries.Append(entries[0])), "Spore ceiling duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CLEAR-CEILING", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomObjectSporeSpawnCeilingVisualFiles.Create(invalid), "Spore ceiling ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[3]);
        AssertThrows<InvalidDataException>(() => RoomObjectSporeSpawnCeilingVisualFiles.Create(invalid), "Spore ceiling exact frame shape");
    }

    /// <summary>Runs Spore Spawn ceiling visual verification.</summary>
    [VerificationCommand("--spore-spawn-ceiling-visuals")]
    internal static int SporeSpawnCeilingVisualsCommand()
    {
        Suite(nameof(VerifySporeSpawnCeilingVisuals), () => VerifySporeSpawnCeilingVisuals());
        return 0;
    }

}
