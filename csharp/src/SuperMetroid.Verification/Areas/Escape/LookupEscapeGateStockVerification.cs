using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks the three escape-gate block drawings and catalog identity against native PLM words.</summary>
internal static class LookupEscapeGateStockVerification
{
    /// <summary>Compares open, half-closed, and closed rows with stock and installed art, then checks bounds, edited-copy isolation, and invalid imports.</summary>
    internal static void VerifyEscapeGateStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmEscapeGateVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9473,"open"),new LookupEscapeGateStockFrame(0x947f,"half-closed"),new LookupEscapeGateStockFrame(0x948b,"closed")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            var appearances = new RoomBlockAppearance[4];
            for (int row = 0; row < appearances.Length; row++)
                appearances[row] = RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, 0x840000 | (frame.Pointer + 2 + row * 2))).Appearance;
            native.Add(frame.Pointer, appearances);
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmEscapeGateVisualCatalog.Stock();
        var imported = new RoomPlmEscapeGateVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> appearances) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomPlmEscapeGateVisualCatalog), appearances.ToDictionary(pair => MotherBrainEscapeGateDraws.VisualId(NativePlmDraws.At<MotherBrainEscapeGateDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Escape gate original stock identity");
            foreach (var frame in frames)
            {
                for (int row = 0; row < 4; row++)
                    AssertEqual(native[frame.Pointer][row], catalog.GetAppearance(NativePlmDraws.At<MotherBrainEscapeGateDraw>(frame.Pointer),row), "Escape gate native stock visual");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<MotherBrainEscapeGateDraw>(frame.Pointer),bad), "Escape gate stock row bounds");
            }
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!native.ContainsKey((ushort)raw))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<MotherBrainEscapeGateDraw>((ushort)raw),0), "Escape gate unknown stock pointer");
        entries[2].Blocks[1] = RoomBlockAppearance.Of(0x58, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        entries[1].Blocks[3] = RoomBlockAppearance.Of(0x59);
        var mixed = new RoomPlmEscapeGateVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer,
            frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Escape gate custom/stock hash");
        entries[2].Blocks[1] = RoomBlockAppearance.Of(0x5a);
        entries[0].Blocks[0] = RoomBlockAppearance.Of(0x5b);
        foreach (var frame in frames)
        for (int row = 0; row < 4; row++)
            AssertEqual(expected[frame.Pointer][row], mixed.GetAppearance(NativePlmDraws.At<MotherBrainEscapeGateDraw>(frame.Pointer),row), "Escape gate custom clone and stock isolation");
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(entries[..2]), "Escape gate missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(entries.Append(entries[0])), "Escape gate duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("OPEN", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(invalid), "Escape gate ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(invalid), "Escape gate exact shape");
    }
}
