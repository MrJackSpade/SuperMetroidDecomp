using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks Crocomire bridge and wall PLM visuals against native draw words.</summary>
internal static class LookupCrocomireStockVerification
{
    /// <summary>Compares five frame shapes, stock and imported identity, edit ownership, and invalid entries.</summary>
    internal static void VerifyCrocomireStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmCrocomireVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9b5b,"clear-bridge"), new LookupEscapeGateStockFrame(0x9b73,"crumble-bridge-block"),
            new LookupEscapeGateStockFrame(0x9b79,"clear-bridge-block"), new LookupEscapeGateStockFrame(0x9b7f,"clear-invisible-wall"), new LookupEscapeGateStockFrame(0x9bbb,"create-invisible-wall")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        var widths = new Dictionary<ushort, int>();
        foreach (var frame in frames)
        {
            var appearances = new List<RoomBlockAppearance>();
            int cursor = frame.Pointer;
            widths.Add(frame.Pointer, ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff);
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                for (int index = 0; index < count; index++, cursor += 2)
                    appearances.Add(RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, 0x840000 | cursor)).Appearance);
                byte x = CrocomireStockMappingRomByte(rom, 0x840000 | cursor++), y = CrocomireStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, appearances.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmCrocomireVisualCatalog.Stock();
        var imported = new RoomPlmCrocomireVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomPlmCrocomireVisualCatalog), appearances.ToDictionary(pair => CrocomireArenaDraws.VisualId(NativePlmDraws.At<CrocomireArenaDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Crocomire original flattened hash");
            foreach (var frame in frames)
            {
                for (int index = 0; index < native[frame.Pointer].Length; index++)
                    AssertEqual(native[frame.Pointer][index], catalog.GetAppearance(NativePlmDraws.At<CrocomireArenaDraw>(frame.Pointer),index / widths[frame.Pointer],index % widths[frame.Pointer]), "Crocomire native stock word");
                foreach (int bad in new[] {int.MinValue,-1,native[frame.Pointer].Length / widths[frame.Pointer],int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<CrocomireArenaDraw>(frame.Pointer),bad,0), "Crocomire run bounds");
                foreach (int bad in new[] {int.MinValue,-1,widths[frame.Pointer],int.MaxValue})
                for (int run = 0; run < native[frame.Pointer].Length / widths[frame.Pointer]; run++)
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<CrocomireArenaDraw>(frame.Pointer),run,bad), "Crocomire block bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<CrocomireArenaDraw>((ushort)pointer),0,0), "Crocomire unknown pointer");
        entries[4].Blocks[23] = RoomBlockAppearance.Of(0x58, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        entries[2].Blocks[0] = RoomBlockAppearance.Of(0x59);
        var mixed = new RoomPlmCrocomireVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Crocomire mixed custom stock hash");
        entries[4].Blocks[23] = RoomBlockAppearance.Of(0x5a);
        entries[1].Blocks[0] = RoomBlockAppearance.Of(0x5b);
        foreach (var frame in frames)
        for (int index = 0; index < native[frame.Pointer].Length; index++)
            AssertEqual(expected[frame.Pointer][index], mixed.GetAppearance(NativePlmDraws.At<CrocomireArenaDraw>(frame.Pointer),index / widths[frame.Pointer],index % widths[frame.Pointer]), "Crocomire clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(entries[..4]), "Crocomire missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(entries.Append(entries[0])), "Crocomire duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CLEAR-BRIDGE", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(invalid), "Crocomire ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(invalid), "Crocomire exact frame shape");
    }

    /// <summary>Reads native draw-stream bytes for the independent stock catalog oracle.</summary>
    private static byte CrocomireStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

}
