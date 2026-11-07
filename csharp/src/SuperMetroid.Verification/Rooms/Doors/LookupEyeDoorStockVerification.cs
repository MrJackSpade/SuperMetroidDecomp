using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks installed eye-door visual frames, mirrored clear tiles, and import validation.</summary>
internal static class LookupEyeDoorStockVerification
{
    /// <summary>Compares 23 bank $84 frame identities and every stock tile word with the native map.</summary>
    internal static void VerifyEyeDoorStockMapping(SuperMetroidAddressSpace rom, InstalledRoomObjectEyeDoorVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [
            new LookupEscapeGateStockFrame(0x9c03,"left-eye-frame-0"),new LookupEscapeGateStockFrame(0x9c0b,"left-eye-frame-1"),new LookupEscapeGateStockFrame(0x9c13,"left-eye-frame-2"),
            new LookupEscapeGateStockFrame(0x9c1b,"left-eye-frame-3"),new LookupEscapeGateStockFrame(0x9c23,"left-eye-frame-4"),
            new LookupEscapeGateStockFrame(0x9c2b,"left-middle-frame-0"),new LookupEscapeGateStockFrame(0x9c31,"left-middle-frame-1"),new LookupEscapeGateStockFrame(0x9c37,"left-middle-frame-2"),
            new LookupEscapeGateStockFrame(0x9c3d,"left-bottom-frame-0"),new LookupEscapeGateStockFrame(0x9c43,"left-bottom-frame-1"),new LookupEscapeGateStockFrame(0x9c49,"left-bottom-frame-2"),
            new LookupEscapeGateStockFrame(0x9c4f,"left-eye-clear"),
            new LookupEscapeGateStockFrame(0x9c5b,"right-eye-frame-0"),new LookupEscapeGateStockFrame(0x9c63,"right-eye-frame-1"),new LookupEscapeGateStockFrame(0x9c6b,"right-eye-frame-2"),
            new LookupEscapeGateStockFrame(0x9c73,"right-eye-frame-3"),new LookupEscapeGateStockFrame(0x9c7b,"right-eye-frame-4"),
            new LookupEscapeGateStockFrame(0x9c83,"right-middle-frame-0"),new LookupEscapeGateStockFrame(0x9c89,"right-middle-frame-1"),new LookupEscapeGateStockFrame(0x9c8f,"right-middle-frame-2"),
            new LookupEscapeGateStockFrame(0x9c95,"right-bottom-frame-0"),new LookupEscapeGateStockFrame(0x9c9b,"right-bottom-frame-1"),new LookupEscapeGateStockFrame(0x9ca1,"right-bottom-frame-2"),
        ];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        foreach (var frame in frames)
        {
            int count = ReadSamusEaterObjectWord(rom,0x840000 | frame.Pointer) & 0x7fff;
            native.Add(frame.Pointer,Enumerable.Range(0,count).Select(cell =>
                RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterObjectWord(rom,0x840000 | (frame.Pointer + 2 + cell * 2)))).ToArray());
        }
        string Hash(Dictionary<ushort,RoomBlockAppearance[]> words) =>
            SuperMetroid.AssetExtraction.SelectedPresentationHash.FromAppearanceFrames(nameof(InstalledRoomObjectEyeDoorVisuals),words.ToDictionary(pair => RoomObjectEyeDoorVisualFiles.VisualId(NativeObjectDraws.At<EyeDoorDraws.Draw>(pair.Key)), pair => pair.Value));
        void Compare(InstalledRoomObjectEyeDoorVisuals catalog, Dictionary<ushort,RoomBlockAppearance[]> words)
        {
            AssertEqual(Hash(words),catalog.ContentIdentity,"Eye door original stock identity framing");
            foreach (var frame in frames)
                for (int cell = 0; cell < words[frame.Pointer].Length; cell++)
                    AssertEqual(words[frame.Pointer][cell],catalog.GetAppearance(NativeObjectDraws.At<EyeDoorDraws.Draw>(frame.Pointer),cell),"Eye door stock word");
            for (int cell = 0; cell < 4; cell++)
                AssertEqual(words[0x9c4f][cell].WithMirroring(words[0x9c4f][cell].Mirroring ^ BlockMirroring.Horizontal),catalog.GetAppearance(NativeObjectDraws.At<EyeDoorDraws.Draw>(0x9bf7),cell),"Eye door shared clear mirror");
        }
        var entries = frames.Select(frame => new RoomObjectVisualEntry(frame.Id,native[frame.Pointer].ToArray())).ToArray();
        var stock = InstalledRoomObjectEyeDoorVisuals.Stock();
        foreach (var catalog in new[] {stock,installed,RoomObjectEyeDoorVisualFiles.Create(entries.Reverse())})
        {
            Compare(catalog,native);
            foreach (var frame in frames)
                foreach (int bad in new[] {int.MinValue,-1,native[frame.Pointer].Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<EyeDoorDraws.Draw>(frame.Pointer),bad),"Eye door stock word bounds");
            foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativeObjectDraws.At<EyeDoorDraws.Draw>(0x9bf7),bad),"Eye door mirrored clear bounds");
            for (int cell = 0; cell < 4; cell++)
                AssertEqual(RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterObjectWord(rom,0x849bf9 + cell * 2)),
                    catalog.GetAppearance(NativeObjectDraws.At<EyeDoorDraws.Draw>(0x9bf7),cell),"Eye door original mirrored native word");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (raw != 0x9bf7 && !native.ContainsKey((ushort)raw))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativeObjectDraws.At<EyeDoorDraws.Draw>((ushort)raw),0),"Eye door stock full pointer rejection");
        AssertThrows<InvalidDataException>(() => RoomObjectEyeDoorVisualFiles.Create(entries[..1]),"Eye door missing import");
        AssertThrows<InvalidDataException>(() => RoomObjectEyeDoorVisualFiles.Create(entries.Append(entries[0])),"Eye door duplicate import");
        foreach (RoomObjectVisualEntry bad in new RoomObjectVisualEntry[]
            {new("LEFT-EYE-FRAME-0",[default,default]),new("unknown",[default,default]),
             new(entries[0].Id,[default])})
        {
            var invalid = entries.ToArray();
            invalid[0] = bad;
            AssertThrows<InvalidDataException>(() => RoomObjectEyeDoorVisualFiles.Create(invalid),"Eye door invalid import");
        }
    }
}
