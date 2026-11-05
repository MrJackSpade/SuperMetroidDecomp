using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Enemies.Elevator;

/// <summary>Checks the three elevator platform draw lists against native PLM words.</summary>
internal static class LookupElevatorPlatformStockVerification
{
    /// <summary>Checks stock runs, identity hashes, and invalid frame shapes.</summary>
    internal static void VerifyElevatorPlatformStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmElevatorPlatformVisualCatalog installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xaa97,"first-frame"),new LookupEscapeGateStockFrame(0xaaaf,"second-frame"),new LookupEscapeGateStockFrame(0xaac7,"third-frame")];
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
                byte x = ElevatorPlatformStockMappingRomByte(rom, 0x840000 | cursor++), y = ElevatorPlatformStockMappingRomByte(rom, 0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualRunEntry(frame.Id,
            native[frame.Pointer].Select(run => run.ToArray()).ToArray())).ToArray();
        var original = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].Select(run => run.ToArray()).ToArray());
        var stock = RoomPlmElevatorPlatformVisualCatalog.Stock();
        var imported = RoomPlmElevatorPlatformVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[][]> appearances) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(RoomPlmElevatorPlatformVisualCatalog), appearances.ToDictionary(pair => RoomPlmElevatorPlatformVisualFiles.VisualId(NativePlmDraws.At<ElevatorPlatformDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(original), catalog.ContentIdentity, "Elevator platform original run-framed identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetAppearance(NativePlmDraws.At<ElevatorPlatformDraw>(frame.Pointer),run,word), "Elevator platform original stock appearance");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<ElevatorPlatformDraw>(frame.Pointer),run,bad), "Elevator platform stock cell bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<ElevatorPlatformDraw>(frame.Pointer),bad,0), "Elevator platform stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<ElevatorPlatformDraw>((ushort)pointer),0,0), "Elevator platform stock unknown pointer");
        AssertThrows<InvalidDataException>(() => RoomPlmElevatorPlatformVisualFiles.Create(entries[..1]), "Elevator platform missing frame");
        AssertThrows<InvalidDataException>(() => RoomPlmElevatorPlatformVisualFiles.Create(entries.Append(entries[0])), "Elevator platform duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("FIRST-FRAME", entries[0].Runs);
        AssertThrows<InvalidDataException>(() => RoomPlmElevatorPlatformVisualFiles.Create(invalid), "Elevator platform ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[][] { new RoomBlockAppearance[7] });
        AssertThrows<InvalidDataException>(() => RoomPlmElevatorPlatformVisualFiles.Create(invalid), "Elevator platform exact run shape");
    }

    /// <summary>Reads a native elevator platform draw-list byte for the stock oracle.</summary>
    private static byte ElevatorPlatformStockMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
