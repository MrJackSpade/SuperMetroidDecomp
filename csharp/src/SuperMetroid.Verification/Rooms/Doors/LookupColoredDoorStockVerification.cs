using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks the 48 colored-door draw frames, and their validation.</summary>
internal static class LookupColoredDoorStockVerification
{
    /// <summary>Compares each four-row colored-door frame with ROM and validates lookup, hashing, and malformed edits.</summary>
    internal static void VerifyColoredDoorStockMapping(SuperMetroidAddressSpace rom,
        InstalledRoomObjectColoredDoorVisuals installed)
    {
        LookupColoredCapFamily[] families = [
            new LookupColoredCapFamily(0xa767,"yellow-left"),new LookupColoredCapFamily(0xa797,"yellow-right"),new LookupColoredCapFamily(0xa7c7,"yellow-up"),new LookupColoredCapFamily(0xa7f7,"yellow-down"),
            new LookupColoredCapFamily(0xa827,"green-left"),new LookupColoredCapFamily(0xa857,"green-right"),new LookupColoredCapFamily(0xa887,"green-up"),new LookupColoredCapFamily(0xa8b7,"green-down"),
            new LookupColoredCapFamily(0xa8e7,"red-left"),new LookupColoredCapFamily(0xa917,"red-right"),new LookupColoredCapFamily(0xa947,"red-up"),new LookupColoredCapFamily(0xa977,"red-down")];
        var frames = families.SelectMany(family => Enumerable.Range(0,4).Select(frame =>
            new LookupEscapeGateStockFrame(Pointer:(ushort)(family.First + 12 * frame), Id:$"{family.Name}-frame-{frame}"))).ToArray();
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            var words = new RoomBlockAppearance[4];
            for (int row = 0; row < words.Length; row++)
                words[row] = RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterObjectWord(rom, 0x840000 | (frame.Pointer + 2 + row * 2)));
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = InstalledRoomObjectColoredDoorVisuals.Stock();
        var imported = RoomObjectColoredDoorVisualFiles.Create(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> words) => SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(
            nameof(InstalledRoomObjectColoredDoorVisuals), words.ToDictionary(pair => RoomObjectColoredDoorVisualFiles.VisualId(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Colored door original stock identity");
            foreach (var frame in frames)
            {
                for (int row = 0; row < 4; row++)
                    AssertEqual(native[frame.Pointer][row], catalog.GetAppearance(NativeObjectDraws.At<ColoredDoorCaps.Draw>(frame.Pointer),row), "Colored door native stock visual");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<ColoredDoorCaps.Draw>(frame.Pointer),bad), "Colored door stock row bounds");
            }
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!native.ContainsKey((ushort)raw))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<ColoredDoorCaps.Draw>((ushort)raw),0), "Colored door unknown stock pointer");
        AssertThrows<InvalidDataException>(() => RoomObjectColoredDoorVisualFiles.Create(entries[..47]), "Colored door missing frame");
        AssertThrows<InvalidDataException>(() => RoomObjectColoredDoorVisualFiles.Create(entries.Append(entries[0])), "Colored door duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("YELLOW-LEFT-FRAME-0", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => RoomObjectColoredDoorVisualFiles.Create(invalid), "Colored door ordinal identity");
        invalid[0] = new(entries[0].Id, new RoomBlockAppearance[3]);
        AssertThrows<InvalidDataException>(() => RoomObjectColoredDoorVisualFiles.Create(invalid), "Colored door exact shape");
    }
}
