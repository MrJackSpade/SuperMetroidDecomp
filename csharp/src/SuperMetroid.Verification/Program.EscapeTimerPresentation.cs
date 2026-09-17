using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEscapeTimerPresentationAssets(ISnesAddressSpace bus, string stock,
        string overrides, AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, EscapeTimerPresentationDefinitions.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.EscapeTimerPresentationExtractor.Extract(bus);
        AssertTrue(extracted.AsSpan().SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed timer JSON is the deterministic cartridge extraction");

        var presentation = EscapeTimerPresentation.Load(new MemoryStream(extracted));
        var timer = new EscapeTimer();
        timer.Clear();
        timer.SetTime(0x01, 0x23, 0x45);
        var native = new OamBuffer();
        var installed = new OamBuffer();
        native.BeginFrame();
        installed.BeginFrame();
        EscapeTimerRenderer.Draw(timer, native, bus);
        EscapeTimerRenderer.Draw(timer, installed, new ForbiddenMapBus(), presentation);
        AssertEqual(native.NextByteOffset, installed.NextByteOffset,
            "installed timer emits the native number of OAM parts");
        AssertTrue(native.LowTable.SequenceEqual(installed.LowTable),
            "installed timer label/digits exactly match native low OAM");
        AssertTrue(native.HighTable.SequenceEqual(installed.HighTable),
            "installed timer label/digits exactly match native high OAM");

        var document = JsonSerializer.Deserialize<EscapeTimerPresentationDocument>(extracted,
            MapPresentationFormat.JsonOptions)!;
        document.Anchors["Minutes"] = document.Anchors["Minutes"] with
        {
            X = document.Anchors["Minutes"].X + 1,
        };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, EscapeTimerPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            EscapeTimerPresentation.Write(output, document);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "timer override changes catalog identity");

        var moved = new OamBuffer();
        moved.BeginFrame();
        EscapeTimerRenderer.Draw(timer, moved, new ForbiddenMapBus(), edited.EscapeTimer);
        // Label occupies the first five OAM entries. The next two parts are the minute tens.
        AssertTrue(native.LowTable[..20].SequenceEqual(moved.LowTable[..20]),
            "editing minute anchor leaves TIME label untouched");
        AssertEqual(unchecked((byte)(native.LowTable[20] + 1)), moved.LowTable[20],
            "edited minute anchor reaches the first minute OAM part");
        AssertEqual(unchecked((byte)(native.LowTable[24] + 1)), moved.LowTable[24],
            "edited minute anchor reaches the second minute OAM part");

        File.WriteAllText(replacement, "{ broken timer JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt timer override fails loudly");
        using (var output = File.Create(replacement))
            EscapeTimerPresentation.Write(output, document);

        var missingFrame = document with
        {
            Frames = document.Frames.Where(pair => pair.Key != "Digit.9")
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
        };
        AssertThrows<InvalidDataException>(() => EscapeTimerPresentation.Write(
            Stream.Null, missingFrame), "timer resource rejects missing named digits");
        AssertThrows<InvalidDataException>(() => EscapeTimerPresentation.Write(
            Stream.Null, document with { Palette = 8 }), "timer resource rejects invalid palette");
        AssertThrows<InvalidDataException>(() => EscapeTimerPresentation.Write(
            Stream.Null, document with { DigitSpacing = 256 }), "timer resource rejects invalid spacing");

        Console.WriteLine("Escape timer presentation: exact stock OAM, ROM-free drawing, edited anchors and strict failures pass.");
    }
}
