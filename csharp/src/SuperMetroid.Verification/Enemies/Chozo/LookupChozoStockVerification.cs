using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks editable Chozo statue visuals against native draw runs and catalog constraints.</summary>
internal static class LookupChozoStockVerification
{
    /// <summary>Compares three native statue frames, decoded to appearances, then checks custom copies and completeness.</summary>
    internal static void VerifyChozoStatueStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmChozoStatueVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa2b5,"lower-norfair-cleared-hand"),new LookupEscapeGateStockFrame(0x9cc5,"wrecked-ship-clear-slope-access"),new LookupEscapeGateStockFrame(0x9d0f,"wrecked-ship-block-slope-access")];
        var native = new Dictionary<ushort, RoomBlockAppearance[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<RoomBlockAppearance[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var appearances = new RoomBlockAppearance[count];
                for (int cell = 0; cell < count; cell++, cursor += 2)
                    appearances[cell] = RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, 0x840000 | cursor)).Appearance;
                runs.Add(appearances);
                byte x = ChozoStatueStockMappingRomByte(rom, 0x840000 | cursor++), y = ChozoStatueStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomPlmChozoStatueVisualCatalog.Stock();
        var imported = new RoomPlmChozoStatueVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomPlmChozoStatueVisualCatalog), appearances.ToDictionary(pair => ChozoStatueDraws.VisualId(NativePlmDraws.At<ChozoStatueDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Chozo original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int cell = 0; cell < runs[run].Length; cell++)
                        AssertEqual(runs[run][cell], catalog.GetAppearance(NativePlmDraws.At<ChozoStatueDraw>(frame.Pointer),run,cell), "Chozo original stock appearance");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<ChozoStatueDraw>(frame.Pointer),run,bad), "Chozo stock cell bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<ChozoStatueDraw>(frame.Pointer),bad,0), "Chozo stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<ChozoStatueDraw>((ushort)pointer),0,0), "Chozo stock unknown pointer");
        entries[1].Blocks[20] = RoomBlockAppearance.Of(0x58, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        var mixed = new RoomPlmChozoStatueVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Chozo mixed custom/stock identity");
        entries[1].Blocks[20] = RoomBlockAppearance.Of(0x5a);
        entries[2].Blocks[26] = RoomBlockAppearance.Of(0x5b);
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int cell = 0; cell < native[frame.Pointer][run].Length; cell++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetAppearance(NativePlmDraws.At<ChozoStatueDraw>(frame.Pointer),run,cell), "Chozo cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmChozoStatueVisualCatalog(entries[..1]), "Chozo missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmChozoStatueVisualCatalog(entries.Append(entries[0])), "Chozo duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("LOWER-NORFAIR-CLEARED-HAND", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmChozoStatueVisualCatalog(invalid), "Chozo ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => new RoomPlmChozoStatueVisualCatalog(invalid), "Chozo exact flattened shape");
    }

    /// <summary>Reads a native draw-record byte to determine each statue run's displacement and terminator.</summary>
    private static byte ChozoStatueStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
