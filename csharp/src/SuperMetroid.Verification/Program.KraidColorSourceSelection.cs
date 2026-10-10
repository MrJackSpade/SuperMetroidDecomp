using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Verifies each named Kraid palette source resolves its own editable colors, enforces source-specific bounds, and preserves catalog identity.</summary>
    private static void VerifyKraidColorSourceSelection()
    {
        var original = new Dictionary<KraidPaletteSource, PaletteRgb5[]>();
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
            original.Add(source, Enumerable.Range(0, source is KraidPaletteSource.Health or KraidPaletteSource.Secondary ? 144 : 16)
                .Select(index => new PaletteRgb5 { Red = (int)source, Green = index & 31, Blue = index >> 5 }).ToArray());
        var document = new KraidColorDocument
        {
            Version = 1,
            RoomBackdrop = original[KraidPaletteSource.RoomBackdrop],
            InitialTarget = original[KraidPaletteSource.InitialTarget],
            Health = original[KraidPaletteSource.Health],
            Secondary = original[KraidPaletteSource.Secondary],
            DeathArm = original[KraidPaletteSource.DeathArm],
        };
        using var json = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document));
        var catalog = KraidColorCatalog.Load(json);
        ushort Encode(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        foreach (var (source, row) in original)
        {
            for (int index = 0; index < row.Length; index++)
                AssertEqual(Encode(row[index]), catalog.Resolve(source, index), "Named source resolves its own editable color");
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(source, -1), "Negative color index");
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(source, row.Length), "Selected source bounds");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve((KraidPaletteSource)invalid, 0), "Unknown palette source");
        string expectedIdentity = SelectedPresentationHash.Create("enemy-kraid-colors-v1", content =>
        {
            // Preserve the original dictionary's enum-key ordering contract.
            foreach (var (source, row) in original.OrderBy(pair => pair.Key))
            {
                content.Append("source", (int)source);
                content.AppendWords("colors", row.Select(Encode).ToArray());
            }
        });
        AssertEqual(expectedIdentity, catalog.ContentIdentity, "Source selection preserves content identity");
        Console.WriteLine("Kraid palette source selection: all five distinct fixtures, every color, bounds and identity pass.");
    }
}
