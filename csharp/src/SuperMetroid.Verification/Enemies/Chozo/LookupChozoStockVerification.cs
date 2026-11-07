using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks editable Chozo statue visuals against native draw runs and catalog constraints.</summary>
internal static class LookupChozoStockVerification
{
    /// <summary>Compares three native statue frames, decoded to appearances, then checks completeness.</summary>
    internal static void VerifyChozoStatueStockMapping(SuperMetroidAddressSpace rom,
        RoomObjectChozoStatueVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa2b5,"lower-norfair-cleared-hand"),new LookupEscapeGateStockFrame(0x9cc5,"wrecked-ship-clear-slope-access"),new LookupEscapeGateStockFrame(0x9d0f,"wrecked-ship-block-slope-access")];
        var native = new Dictionary<ushort, RoomBlockAppearance[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<RoomBlockAppearance[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterObjectWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var appearances = new RoomBlockAppearance[count];
                for (int cell = 0; cell < count; cell++, cursor += 2)
                    appearances[cell] = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | cursor)).Appearance;
                runs.Add(appearances);
                byte x = ChozoStatueStockMappingRomByte(rom, 0x840000 | cursor++), y = ChozoStatueStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomObjectChozoStatueVisualCatalog.Stock();
        var imported = RoomObjectChozoStatueVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomObjectChozoStatueVisualCatalog), appearances.ToDictionary(pair => RoomObjectChozoStatueVisualFiles.VisualId(NativeObjectDraws.At<ChozoStatueDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Chozo original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int cell = 0; cell < runs[run].Length; cell++)
                        AssertEqual(runs[run][cell], catalog.GetAppearance(NativeObjectDraws.At<ChozoStatueDraw>(frame.Pointer),run,cell), "Chozo original stock appearance");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<ChozoStatueDraw>(frame.Pointer),run,bad), "Chozo stock cell bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<ChozoStatueDraw>(frame.Pointer),bad,0), "Chozo stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<ChozoStatueDraw>((ushort)pointer),0,0), "Chozo stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectChozoStatueVisualFiles.Create(entries[..1]), "Chozo missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectChozoStatueVisualFiles.Create(entries.Append(entries[0])), "Chozo duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("LOWER-NORFAIR-CLEARED-HAND", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomObjectChozoStatueVisualFiles.Create(invalid), "Chozo ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => RoomObjectChozoStatueVisualFiles.Create(invalid), "Chozo exact flattened shape");
    }

    /// <summary>Reads a native draw-record byte to determine each statue run's displacement and terminator.</summary>
    private static byte ChozoStatueStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
