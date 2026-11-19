using SuperMetroid.EnsureAnalyzer.Contracts;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6463: a JSON document of the import holds no hardware data (#2564).</summary>
internal static class InstalledDocumentHardwareAnalyzerTests
{
    /// <summary>Stand-ins for the import's channel colour, its document reader and a cell with a palette row and priority.</summary>
    private const string Helpers = """
        using System.Text.Json;
        using System.Text.Json.Serialization;
        public sealed record PaletteRgb5 { public int Red { get; init; } public int Green { get; init; } public int Blue { get; init; } }
        public sealed record Cell { public int Tile { get; init; } public int Palette { get; init; } public bool Priority { get; init; } public bool FlipX { get; init; } }
        static class JsonAssetDocument { public static T Read<T>(System.IO.Stream json, JsonSerializerOptions options) => JsonSerializer.Deserialize<T>(json, options)!; }

        """;

    /// <summary>Each hardware representation a document holds, each way a document is found, and what the rule leaves alone.</summary>
    internal static void VerifyInstalledDocumentHardwareAnalyzer()
    {
        var analyzer = new InstalledDocumentHardwareAnalyzer();
        string extraction = InstalledDocumentHardwareAnalyzer.ExtractionAssembly;
        string id = InstalledDocumentHardwareAnalyzer.InstalledHardwareDataId;
        void Expect(string source, string? message = null, string? assembly = null) =>
            VerifySnippet(Helpers + source, message is null ? null : id, message, analyzer, assemblyName: assembly ?? extraction);

        // A colour held as five-bit channel levels, written by the serializer, at its path.
        Expect("""
            public sealed record Scene { public required PaletteRgb5 Backdrop { get; init; } }
            static class W { static byte[] M(Scene s) => JsonSerializer.SerializeToUtf8Bytes(s); }
            """, "Document 'Scene' holds a colour as red, green and blue channel levels at 'Backdrop'");
        // A type of its own holding the levels, as a gradient stop does, held in a nested list.
        Expect("""
            public sealed record Stop { public int Line { get; init; } public int Red { get; init; } public int Green { get; init; } public int Blue { get; init; } }
            public sealed record Variant { public required Stop[] Stops { get; init; } }
            public sealed record Gradient { public required System.Collections.Generic.List<Variant> Variants { get; init; } }
            static class R { static Gradient M(string s) => JsonSerializer.Deserialize<Gradient>(s)!; }
            """, "at 'Variants.Stops'");
        // Colours keyed by name in a dictionary, read through the import's reader.
        Expect("""
            public sealed record Areas { public required System.Collections.Generic.Dictionary<string, PaletteRgb5[]> Backdrops { get; init; } }
            static class R { static Areas M(System.IO.Stream s) => JsonAssetDocument.Read<Areas>(s, new()); }
            """, "'Backdrops'");
        // A palette row number, and a ushort word.
        Expect("""
            public sealed record Hud { public required int SelectedPalette { get; init; } }
            static class W { static string M(Hud h) => JsonSerializer.Serialize(h); }
            """, "holds a palette row number at 'SelectedPalette'");
        Expect("""
            public sealed record Shade { public required ushort[] Colours { get; init; } }
            static class W { static string M(Shade h) => JsonSerializer.Serialize(h); }
            """, "holds a native sixteen-bit word at 'Colours'");
        Expect("""
            public sealed record Shade { public required int[] Inks { get; init; } }
            static class W { static string M(Shade h) => JsonSerializer.Serialize(h); }
            """, "holds a palette index at 'Inks'");
        // A cell's palette row and its priority are each reported, through an array of cells.
        VerifySnippetIds(Helpers + """
            public sealed record Layout { public required Cell[] Cells { get; init; } }
            static class W { static string M(Layout l) => JsonSerializer.Serialize(l); }
            """, analyzer, extraction, id, id);
        // A generic helper of the import whose type argument reaches the serializer finds the document at its call.
        Expect("""
            public sealed record Eyes { public required PaletteRgb5[] Colours { get; init; } }
            static class R
            {
                static T ReadJson<T>(string s) where T : class => JsonSerializer.Deserialize<T>(s)!;
                static Eyes M(string s) => ReadJson<Eyes>(s);
            }
            """, "Document 'Eyes'");
        // A property the serializer writes only when it is not its default is still written.
        Expect("""
            public sealed record Part { [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public int Palette { get; init; } }
            static class W { static string M(Part p) => JsonSerializer.Serialize(p); }
            """, "'Palette'");

        // Ordinary colours, a sound's priority, a palette named by text, flips and ignored members are no hardware data.
        Expect("""
            public sealed record Scene { public required string Backdrop { get; init; } public required int Priority { get; init; } public required string Palette { get; init; } }
            static class W { static string M(Scene s) => JsonSerializer.Serialize(s); }
            """);
        Expect("""
            public sealed record Frame { public bool FlipX { get; init; } [JsonIgnore] public int Palette { get; init; } [JsonIgnore(Condition = JsonIgnoreCondition.Always)] public ushort Word { get; init; } }
            static class W { static string M(Frame f) => JsonSerializer.Serialize(f); }
            """);
        // A type no serializer reads or writes is no document.
        Expect("public sealed record Scratch { public required PaletteRgb5 Colour { get; init; } }");
        // A document only the import reads is justified where it is declared.
        Expect("""
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "SME6463", Justification = "Import data: only the import reads it, from its import-data directory.")]
            public sealed record Colours { public required PaletteRgb5[] Body { get; init; } }
            static class W { static string M(Colours c) => JsonSerializer.Serialize(c); }
            """);
        // Other assemblies are not checked.
        Expect("""
            public sealed record Scene { public required PaletteRgb5 Backdrop { get; init; } }
            static class W { static string M(Scene s) => JsonSerializer.Serialize(s); }
            """, assembly: "SuperMetroid.Verification");
        Console.WriteLine("Installed documents: a serialized document of the import holding hardware data is SME6463 unless it is justified.");
    }
}
