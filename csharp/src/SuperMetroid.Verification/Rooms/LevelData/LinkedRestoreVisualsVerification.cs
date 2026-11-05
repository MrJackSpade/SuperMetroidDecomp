using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>Checks bomb and crumble block restore artwork against native draw lists and production PLM behavior.</summary>
internal static class LinkedRestoreVisualsVerification
{
    /// <summary>The bomb-block or contact-crumble restoration draw the cartridge stores at <paramref name="pointer"/>.</summary>
    private static PlmDraw LinkedAt(int pointer) => NativePlmDraws.IsDrawAt<BombBlockRestoreDraw>(pointer)
        ? NativePlmDraws.At<BombBlockRestoreDraw>(pointer) : NativePlmDraws.At<ContactCrumbleRestoreDraw>(pointer);

    /// <summary>Compares six flattened stock layouts with bank-$84 draw words and checks catalog bounds and edit isolation.</summary>
    private static void VerifyLinkedRestoreStockMapping(SuperMetroidAddressSpace rom)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa4c7,"bomb-horizontal"), new LookupEscapeGateStockFrame(0xa4cf,"bomb-vertical"),
            new LookupEscapeGateStockFrame(0xa4d7,"bomb-square"), new LookupEscapeGateStockFrame(0xa4a1,"crumble-horizontal"), new LookupEscapeGateStockFrame(0xa4a9,"crumble-vertical"), new LookupEscapeGateStockFrame(0xa4b1,"crumble-square")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            var words = new List<RoomBlockAppearance>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadRomWord(rom, 0x840000 | cursor) & 0x7fff;
                for (int block = 0; block < count; block++)
                    words.Add(RoomLevelWordCodec.DecodeAppearance(ReadRomWord(rom, 0x840000 | (cursor + 2 + block * 2))));
                cursor += 2 + count * 2;
                ushort offset = ReadRomWord(rom, 0x840000 | cursor);
                cursor += 2;
                if (offset == 0) break;
            }
            native.Add(frame.Pointer, words.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = InstalledRoomPlmLinkedRestoreVisuals.Stock();
        var imported = RoomPlmLinkedRestoreVisualFiles.Create(entries);
        string expectedHash = SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(nameof(InstalledRoomPlmLinkedRestoreVisuals), native.ToDictionary(pair => RoomPlmLinkedRestoreVisualFiles.VisualId(LinkedAt(pair.Key)), pair => pair.Value));
        AssertEqual(expectedHash, stock.ContentIdentity, "linked restore calculated stock preserves original flattened hash");
        AssertEqual(expectedHash, imported.ContentIdentity, "linked restore imported stock hash");
        foreach (var frame in frames)
        {
            for (int word = 0; word < native[frame.Pointer].Length; word++)
            {
                AssertEqual(native[frame.Pointer][word], stock.GetAppearance(LinkedAt(frame.Pointer), word / 2, word % 2), "linked restore native stock word");
                AssertEqual(native[frame.Pointer][word], imported.GetAppearance(LinkedAt(frame.Pointer), word / 2, word % 2), "linked restore imported native word");
            }
            foreach (int bad in new[] {int.MinValue,-1,native[frame.Pointer].Length / 2,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => stock.GetAppearance(LinkedAt(frame.Pointer),bad,0), "linked restore stock run bounds");
            foreach (int bad in new[] {int.MinValue,-1,2,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => stock.GetAppearance(LinkedAt(frame.Pointer),0,bad), "linked restore stock block bounds");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(LinkedAt(pointer),0,0), "linked restore rejects unknown frame");
        AssertThrows<InvalidDataException>(() => RoomPlmLinkedRestoreVisualFiles.Create(entries[..5]), "linked restore rejects incomplete stock");
        AssertThrows<InvalidDataException>(() => RoomPlmLinkedRestoreVisualFiles.Create(entries.Append(entries[0])), "linked restore rejects duplicate stock");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyLinkedRestoreVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyLinkedRestoreStockMapping), () => VerifyLinkedRestoreStockMapping(rom));
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmLinkedRestoreVisualDirectory);
        GameInstallation installation = copy.Installation;
        // The installer's extraction checked all six compiled row shapes, full physical
        // words, and signed offsets against the pinned retail cartridge.
        RoomPlmLinkedRestoreVisualFilesConstants.ValidateStock(
            installation.RoomPlmLinkedRestoreVisualDirectory);
        AssertEqual(RoomBlockAppearance.Of(0x058),
            installation.LoadRoomPlmLinkedRestoreVisuals().GetAppearance(
                PlmDraw.Of(BombBlockRestoreDraw.Horizontal), 0, 0),
            "stock bomb restore selects cartridge block 058");
        AssertEqual(RoomBlockAppearance.Of(0x0bc),
            installation.LoadRoomPlmLinkedRestoreVisuals().GetAppearance(
                PlmDraw.Of(ContactCrumbleRestoreDraw.Horizontal), 0, 0),
            "stock crumble restore selects cartridge block 0BC");
        AssertThrows<InvalidDataException>(
            () => RoomPlmLinkedRestoreVisualFiles.Create(
                [new RoomPlmVisualEntry("bomb-horizontal",
                    new RoomBlockAppearance[2])]),
            "restore catalog rejects missing linked layouts");

        string stockPath = Path.Combine(
            installation.RoomPlmLinkedRestoreVisualDirectory,
            RoomPlmLinkedRestoreVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => RoomPlmLinkedRestoreVisualFilesConstants.ValidateStock(
                installation.RoomPlmLinkedRestoreVisualDirectory),
            "tampered restore stock fails loudly where it is read");
    }

}
