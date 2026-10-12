using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer;
using static SuperMetroid.EnsureVerification.AnalyzerTests;

namespace SuperMetroid.EnsureVerification;

internal static class CartridgeEncodingAnalyzerTests
{
    private static readonly string EncodingRoot = Path.Combine(Path.GetTempPath(), "cartridge-encoding-case");

    /// <summary>#641 cartridge-encoding ratchet: positive and negative cases for each diagnostic.</summary>
    internal static void VerifyCartridgeEncodingAnalyzer()
    {
        const string functional = "csharp/src/SuperMetroid.Core/Game/Walker.cs";
        const string catalog = "csharp/src/SuperMetroid.Core/Game/WalkerInstructionLists.cs";
        const string pointers =
            "static class WalkerInstructionLists { public const ushort Idle = 0x9389; public const ushort Turn = 0x93a1; } " +
            "static class Limits { public const ushort Ceiling = 0x9000; }";

        // Without the baseline additional file the analyzer is inactive: tools and tests may hold raw encoding.
        ExpectEncoding(null, [(functional, "class C { int M() => 0x10; }")]);

        // SME6340: a file absent from the baseline holds no functional hexadecimal literal; each is reported in place.
        ExpectEncoding("", [(functional, "class C { int M(int a) => a & 0x0f; }")],
            (CartridgeEncodingAnalyzer.RawHexId, "ComposableBits (bitwise operand)"));
        ExpectEncoding("", [(functional, "class C { int M(int a) => a + 0x10; }")],
            (CartridgeEncodingAnalyzer.RawHexId, "NumericQuantity"));
        ExpectEncoding("", [(functional, "class C { int M() => 16; }")]);
        // Catalog-owned values, reviewed allowances and build output are not functional literals.
        ExpectEncoding("", [(catalog, "static class WalkerInstructionLists { public const ushort Idle = 0x9389; }")]);
        ExpectEncoding("", [(functional, "class C { int M(int a) => a & 0x0f; // cartridge-encoding: allow(Mask) - low nibble\n }")]);
        ExpectEncoding("", [("csharp/src/SuperMetroid.Core/obj/Release/Generated.cs", "class C { int M() => 0x10; }")]);

        // The ratchet: a reviewed file may not grow, and a gain must be recorded by lowering its count.
        ExpectEncoding($"{functional}\tSME6340\t1\n", [(functional, "class C { int M(int a) => (a & 0x0f) + 0x10; }")],
            (CartridgeEncodingAnalyzer.RawHexId, "holds 2 occurrences but its reviewed baseline allows 1"));
        ExpectEncoding($"{functional}\tSME6340\t2\n", [(functional, "class C { int M(int a) => (a & 0x0f) + 0x10; }")]);
        ExpectEncoding($"{functional}\tSME6340\t3\n", [(functional, "class C { int M(int a) => (a & 0x0f) + 0x10; }")],
            (CartridgeEncodingAnalyzer.StaleCountId, "now holds 2 SME6340 occurrence(s) but the baseline allows 3"));
        ExpectEncoding($"{functional}\tSME6340\t1\ncsharp/src/SuperMetroid.Core/Game/Removed.cs\tSME6341\t4\n",
            [(functional, "class C { int M() => 0x10; }")],
            (CartridgeEncodingAnalyzer.StaleFileId, "lists SME6341 for csharp/src/SuperMetroid.Core/Game/Removed.cs"));

        // SME6341: a primitive interpreted against address-valued code or instruction constants.
        ExpectEncoding("", [(catalog, pointers), (functional, "class C { bool M(ushort p) => p == WalkerInstructionLists.Idle; }")],
            (CartridgeEncodingAnalyzer.PointerDispatchId, "CartridgeAddress"));
        ExpectEncoding("", [(catalog, pointers), (functional, "class C { int M(ushort p) => p - WalkerInstructionLists.Turn; }")],
            (CartridgeEncodingAnalyzer.PointerDispatchId, "offset from"));
        ExpectEncoding("", [(catalog, pointers),
                (functional, "class C { int M(ushort p) { switch (p) { case WalkerInstructionLists.Idle: return 1; default: return 0; } } }")],
            (CartridgeEncodingAnalyzer.PointerDispatchId, "switch arm"));
        ExpectEncoding("", [(catalog, pointers), (functional, "class C { bool M(ushort p) => p is WalkerInstructionLists.Turn; }")],
            (CartridgeEncodingAnalyzer.PointerDispatchId, "pattern"));
        // Ordinary limits, constant folding and typed identities are not pointer dispatch.
        ExpectEncoding("", [(catalog, pointers), (functional, "class C { bool M(ushort p) => p >= Limits.Ceiling; }")]);
        ExpectEncoding("", [(catalog, pointers), (functional, "class C { const int Next = WalkerInstructionLists.Idle + 4; }")]);
        ExpectEncoding("", [(catalog, "enum WalkerProgram : ushort { Idle = 0x9389 }"),
            (functional, "class C { bool M(WalkerProgram p) => p == WalkerProgram.Idle; }")]);

        // SME6342: integral array and collection literals holding hexadecimal words, catalogs included.
        ExpectEncoding("", [(catalog, "static class WalkerInstructionLists { static readonly ushort[] Words = { 0x812f, 0x0001, 0xe329, 0x80ed }; }")],
            (CartridgeEncodingAnalyzer.OpaqueWordArrayId, "StructuredInstruction"));
        ExpectEncoding("", [(catalog, "static class WalkerInstructionLists { static readonly byte[] Words = [0x12, 0x34, 0x56, 0x78]; }")],
            (CartridgeEncodingAnalyzer.OpaqueWordArrayId, "collection literal"));
        ExpectEncoding("", [(catalog, "static class WalkerInstructionLists { static readonly ushort[] Words = { 1, 2, 3, 4 }; }")]);
        ExpectEncoding("", [(catalog, "static class WalkerInstructionLists { static readonly ushort[] Words = { 0x812f, 0x0001, 0xe329 }; }")]);

        // SME6345 (#223): a shift by a byte, word or bank width or a byte/word/bank mask must name its meaning.
        ExpectEncoding("", [(functional, "class C { byte M(ushort word) => (byte)(word >> 8); }")],
            (CartridgeEncodingAnalyzer.RawWidthId, "moves a value down"));
        ExpectEncoding("", [(functional, "class C { int M(int bank, ushort offset) => bank << 16 | offset; }")],
            (CartridgeEncodingAnalyzer.RawWidthId, "moves a value up"));
        ExpectEncoding("", [(functional, "class C { int M(int value) => value & 255; }")],
            (CartridgeEncodingAnalyzer.RawWidthId, "keeps a byte, word or bank lane"));
        // A nested chain is one operation.
        ExpectEncoding("", [(functional, "class C { int M(int value) => (value >> 8) & 255; }")],
            (CartridgeEncodingAnalyzer.RawWidthId, "'(value >> 8) & 255'"));
        // Shifts and masks of other widths, constant folding, catalogs, owners and reviewed allowances are not reported.
        ExpectEncoding("", [(functional, "class C { int M(int value) => (value >> 3) + (value & 7) + (value << 4); }")]);
        ExpectEncoding("", [(functional, "class C { const int Bank = 126 << 16; int M() => Bank; }")]);
        ExpectEncoding("", [(catalog, "static class WalkerInstructionLists { static int M(int pointer) => pointer >> 16; }")]);
        // A named catalog mask or shift states the width's meaning; byte.MaxValue only names a storage width.
        ExpectEncoding("", [(catalog, "static class WalkerLayout { public const int PackedXMask = 255; }"),
            (functional, "class C { int M(int packed) => packed & WalkerLayout.PackedXMask; }")]);
        ExpectEncoding("", [(functional, "class C { int M(int value) => value & byte.MaxValue; }")],
            (CartridgeEncodingAnalyzer.RawWidthId, "keeps a byte, word or bank lane"));
        ExpectEncoding("", [(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " +
            "[SnesEncodingOwner] readonly struct Word(ushort raw) { public byte High => (byte)(raw >> 8); }")]);
        ExpectEncoding("", [(functional, "class C { int M(int value) => value >> 8; // cartridge-encoding: allow(ByteLane) - DSP register high byte\n }")]);
        ExpectEncoding($"{functional}\tSME6345\t1\n", [(functional, "class C { int M(int value) => value >> 8; }")]);
        ExpectEncoding($"{functional}\tSME6345\t2\n", [(functional, "class C { int M(int value) => value >> 8; }")],
            (CartridgeEncodingAnalyzer.StaleCountId, "now holds 1 SME6345 occurrence(s) but the baseline allows 2"));
    }

    /// <summary>
    /// Runs <see cref="CartridgeEncodingAnalyzer"/> over repository-relative sources with an optional baseline
    /// (null omits the additional file) and requires exactly the expected diagnostics, in order.
    /// </summary>
    private static void ExpectEncoding(
        string? baseline,
        (string Path, string Source)[] files,
        params (string Id, string Message)[] expected)
    {
        var trees = files.Select(file => CSharpSyntaxTree.ParseText(file.Source,
            new CSharpParseOptions(LanguageVersion.Preview), Path.Combine(EncodingRoot, file.Path.Replace('/', Path.DirectorySeparatorChar))));
        var compilation = CSharpCompilation.Create("SuperMetroid.Core", trees, References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        ImmutableArray<AdditionalText> additional = baseline is null ? []
            : [new MemoryAdditionalText(Path.Combine(EncodingRoot, "csharp", "cartridge-encoding-baseline.txt"), baseline)];
        var diagnostics = compilation
            .WithAnalyzers([new CartridgeEncodingAnalyzer()], new AnalyzerOptions(additional))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
            .OrderBy(diagnostic => diagnostic.Location.SourceTree?.FilePath, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.Location.SourceSpan.Start)
            .ToArray();
        string Describe() => $"[{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {string.Join(" | ", files.Select(f => f.Source))}";
        if (diagnostics.Length != expected.Length)
            throw new InvalidOperationException($"Expected {expected.Length} cartridge-encoding diagnostic(s), got {Describe()}");
        for (int index = 0; index < expected.Length; index++)
        {
            if (diagnostics[index].Id != expected[index].Id ||
                !diagnostics[index].GetMessage().Contains(expected[index].Message, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Expected {expected[index].Id} containing '{expected[index].Message}', got {Describe()}");
            }
        }
    }

    internal sealed class MemoryAdditionalText(string path, string content) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(content);
    }
}
