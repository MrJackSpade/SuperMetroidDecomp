using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Preconversion sparse visual identities, independent of the production selector.
    private static (string Name, ushort Id)[] ReserveFrameOracle() =>
        [("Full", 0x1b), ("EndCap", 0x1f), ("Empty", 0x20), ("Fill1", 0x21), ("Fill2", 0x22),
         ("Fill3", 0x23), ("Fill4", 0x24), ("Fill5", 0x25), ("Fill6", 0x26), ("Fill7", 0x27)];

    private static void VerifyPauseReserveFrameCases(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseReserveTankExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseReserveTankDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = PauseReserveTankPresentation.Load(new MemoryStream(bytes));
        VerifyPauseReserveNativeFrames(rom, stock);
        foreach (var changed in ReserveFrameOracle())
        {
            var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames); frames[changed.Name] = [];
            using var stream = new MemoryStream(); PauseReserveTankPresentation.Write(stream, document with { Frames = frames }); stream.Position = 0;
            var edited = PauseReserveTankPresentation.Load(stream);
            foreach (var frame in ReserveFrameOracle())
            {
                var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
                if (frame.Id != changed.Id)
                {
                    int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.Id * 2);
                    DrawImportedSpritemap(rom, expected, pointer, 24, 95, 0x600);
                }
                edited.Draw(actual, frame.Id, 0);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                    "each authored reserve frame independently selected");
            }
            frames.Remove(changed.Name);
            AssertThrows<InvalidDataException>(() => PauseReserveTankPresentation.Write(new MemoryStream(), document with { Frames = frames }),
                "each reserve role remains required");
        }
        foreach (ushort invalid in new ushort[] { 0, 0x1a, 0x1c, 0x1d, 0x1e, 0x28, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.Draw(new OamBuffer(), invalid, 0), "unsupported reserve identity");
        AssertThrows<IndexOutOfRangeException>(() => stock.Draw(new OamBuffer(), 0, -1), "anchor error precedes unknown visual");
    }

    private static void VerifyPauseReserveNativeFrames(ISnesAddressSpace rom, PauseReserveTankPresentation presentation)
    {
        foreach (var (site, identity) in new[] { (0x82b305, 0x1b), (0x82b37d, 0x20), (0x82b396, 0x1f) })
        {
            AssertEqual((byte)0xa9, rom.ReadByte(site), "native reserve role LDA");
            AssertEqual(identity, (int)ReadVerificationWord(rom, site + 1), "native reserve role identity");
        }
        for (int index = 0; index < 16; index++)
            AssertEqual(0x20 + index % 8, (int)ReadVerificationWord(rom, 0x82b3d9 + index * 2), "native repeated partial-fill identities");
        foreach (var frame in ReserveFrameOracle())
        for (int index = 0; index < 6; index++)
        foreach (int occupied in new[] { 0, 127, 128 })
        {
            var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
            for (int i = 0; i < occupied; i++) { expected.AddRawSmallSprite(12, 34, 56); actual.AddRawSmallSprite(12, 34, 56); }
            ushort x = ReadVerificationWord(rom, 0x82c1d6 + index * 2);
            ushort y = (ushort)(ReadVerificationWord(rom, 0x82c1e2) - 1);
            int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + frame.Id * 2);
            DrawImportedSpritemap(rom, expected, pointer, x, y, 0x600);
            presentation.Draw(actual, frame.Id, index);
            expected.FinalizeFrame(); actual.FinalizeFrame();
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                "native reserve frame selection, OAM attributes, ordering and capacity");
        }
    }
}
