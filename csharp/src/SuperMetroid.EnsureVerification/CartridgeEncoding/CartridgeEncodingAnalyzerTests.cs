using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.EnsureAnalyzer.Contracts;

namespace SuperMetroid.EnsureVerification.CartridgeEncoding;

/// <summary>Checks cartridge-encoding diagnostics and baseline ratchets against small C# source fixtures.</summary>
internal static class CartridgeEncodingAnalyzerTests
{
    /// <summary>Stable temporary root used to give fixture sources repository-relative paths.</summary>
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

        // SME6348 (#1324): a hand-written sign-bit test on a countdown-named member or local.
        const string countdown = "class C { ushort FunctionTimer; byte DelayFrames; ";
        ExpectEncoding("", [(functional, countdown + "bool M() => (FunctionTimer & 32768) != 0; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "tests a countdown's sign bit by hand"));
        ExpectEncoding("", [(functional, countdown + "bool M() => (DelayFrames & 128) == 0; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "'DelayFrames & 128'"));
        ExpectEncoding("", [(functional, countdown + "bool M() => FunctionTimer >= 32768; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "'FunctionTimer >= 32768'"));
        ExpectEncoding("", [(functional, countdown + "bool M() => unchecked((short)FunctionTimer) < 0; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "tests a countdown's sign bit by hand"));
        ExpectEncoding("", [(functional, countdown + "bool M() => 0 <= (sbyte)DelayFrames; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "tests a countdown's sign bit by hand"));
        // Signed comparisons of quantities, velocities, packed flags and enums are other rules' work.
        ExpectEncoding("", [(functional, countdown + "ushort VelocityX; bool M() => (short)VelocityX < 0; }")]);
        ExpectEncoding("", [(functional, countdown + "ushort Left; bool M() => (short)(FunctionTimer - Left) < 0; }")]);
        ExpectEncoding("", [(functional, countdown + "ushort Parameter1; bool M() => (Parameter1 & 32768) != 0; }")]);
        // The head noun decides: an angle whose name merely contains "Counter" is not a countdown.
        ExpectEncoding("", [(functional, countdown + "ushort TargetCounterClockwiseAngle; bool M() => (TargetCounterClockwiseAngle & 32768) != 0; }")]);
        ExpectEncoding("", [(functional, countdown + "ushort DelayOrCommand; bool M() => (DelayOrCommand & 32768) != 0; }")]);
        // A word compared with a byte's sign bit is a threshold, not a sign test, and a byte has no bit 15.
        ExpectEncoding("", [(functional, countdown + "bool M() => FunctionTimer >= 128; }")]);
        ExpectEncoding("", [(functional, countdown + "bool M() => (DelayFrames & 32768) != 0; }")]);
        ExpectEncoding("", [(functional, countdown + "int ElapsedFrames; bool M() => ElapsedFrames < 128; }")]);
        ExpectEncoding("", [(functional, countdown + "int WaitTimer; bool M() => WaitTimer >= 32768; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "'WaitTimer >= 32768'"));
        ExpectEncoding("", [(functional, countdown + "int WaitTimer; bool M() => (WaitTimer & 128) != 0; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "'WaitTimer & 128'"));
        ExpectEncoding("", [(functional, countdown + "ushort[] SegmentTimers = new ushort[2]; bool M() => (SegmentTimers[0] & 32768) != 0; }")],
            (CartridgeEncodingAnalyzer.CountdownSignTestId, "'SegmentTimers[0] & 32768'"));
        ExpectEncoding("", [(functional, "enum TimerFlags : ushort { Sign = 32768 } " +
            "class C { TimerFlags Timer; bool M() => (Timer & TimerFlags.Sign) != 0; }")]);
        ExpectEncoding($"{functional}\tSME6348\t1\n", [(functional, countdown + "bool M() => (FunctionTimer & 32768) != 0; }")]);

        // SME6349 (#1324): a hand-written wrapping self-decrement of narrow storage.
        ExpectEncoding("", [(functional, countdown + "void M() => FunctionTimer = unchecked((ushort)(FunctionTimer - 1)); }")],
            (CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "decrements narrow storage with native wraparound"));
        ExpectEncoding("", [(functional, countdown + "void M() => DelayFrames = unchecked((byte)(DelayFrames - 1)); }")],
            (CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "decrements narrow storage with native wraparound"));
        ExpectEncoding("", [(functional, countdown + "void M() { FunctionTimer--; --DelayFrames; DelayFrames -= 1; } }")],
            (CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "'FunctionTimer--'"),
            (CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "'--DelayFrames'"),
            (CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "'DelayFrames -= 1'"));
        // Locals, wide integers and a decrement into a different member are not the DEC idiom.
        ExpectEncoding("", [(functional, countdown + "int Wide; ushort Next; " +
            "void M(ushort count) { count--; Wide--; Next = unchecked((ushort)(FunctionTimer - 1)); } }")]);
        // An encoding owner documents the idiom itself.
        ExpectEncoding("", [(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " +
            "[SnesEncodingOwner] struct Countdown { ushort timer; bool M() { timer--; return (timer & 32768) != 0; } }")]);
        ExpectEncoding($"{functional}\tSME6349\t2\n", [(functional, countdown + "void M() => FunctionTimer--; }")],
            (CartridgeEncodingAnalyzer.StaleCountId, "now holds 1 SME6349 occurrence(s) but the baseline allows 2"));

        // SME6346 (#1323): a [Flags] value cast to an integral primitive leaves its type before the encoding boundary.
        const string items = "[System.Flags] enum Items : ushort { Morph = 4, Bombs = 4096 } enum Mode : ushort { Auto = 1 } ";
        ExpectEncoding("", [(functional, items + "class C { ushort M() => (ushort)Items.Morph; }")],
            (CartridgeEncodingAnalyzer.FlagsPrimitiveCastId, "casts [Flags] Items to ushort"));
        ExpectEncoding("", [(functional, items + "class C { int M(Items held) => unchecked((int)held); }")],
            (CartridgeEncodingAnalyzer.FlagsPrimitiveCastId, "'(int)held'"));
        // A plain enum, a primitive-to-flags conversion, an owner and a reviewed allowance are not reported.
        ExpectEncoding("", [(functional, items + "class C { ushort M(ushort word) => (ushort)Mode.Auto; Items N(ushort word) => (Items)word; }")]);
        ExpectEncoding("", [(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " + items +
            "[SnesEncodingOwner] static class ItemWords { public static ushort Native(Items items) => (ushort)items; }")]);
        ExpectEncoding("", [(functional, items + "class C { ushort M(Items held) => (ushort)held; // cartridge-encoding: allow(Codec) - SRAM word\n }")]);
        ExpectEncoding($"{functional}\tSME6346\t1\n", [(functional, items + "class C { ushort M() => (ushort)Items.Bombs; }")]);

        // SME6347 (#1323): byte and ushort instance fields, properties and positional record parameters.
        ExpectEncoding("", [(functional, "class C { ushort health; public byte Mode { get; set; } }")],
            (CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'health' stores ushort instance state"),
            (CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'Mode' stores byte instance state"));
        ExpectEncoding("", [(functional, "class C { ushort a, b; ushort? sentinel; }")],
            (CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'a'"),
            (CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'b'"),
            (CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'sentinel' stores ushort? instance state"));
        ExpectEncoding("", [(functional, "record Slot(ushort Health, int Area);")],
            (CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'Health' stores ushort instance state"));
        // Constants, statics, wide members, locals, parameters, catalogs, owners and reviewed allowances are not reported.
        ExpectEncoding("", [(functional, "class C { const ushort Limit = 99; static ushort shared; int wide; ushort[] words = new ushort[2]; " +
            "int M(ushort argument) { ushort local = argument; return local + Limit + shared; } }")]);
        ExpectEncoding("", [(catalog, "static class WalkerInstructionLists { } sealed class WalkerRecord { public ushort Word { get; init; } }")]);
        ExpectEncoding("", [(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " +
            "[SnesEncodingOwner] readonly struct Word(ushort raw) { readonly ushort value = raw; }")]);
        ExpectEncoding("", [(functional, "class C { ushort frameCounter; // cartridge-encoding: allow(Wrap) - wraps by design\n }")]);
        ExpectEncoding($"{functional}\tSME6347\t2\n", [(functional, "class C { ushort a; byte b; }")]);
        ExpectEncoding($"{functional}\tSME6347\t3\n", [(functional, "class C { ushort a; byte b; }")],
            (CartridgeEncodingAnalyzer.StaleCountId, "now holds 2 SME6347 occurrence(s) but the baseline allows 3"));
    }

    /// <summary>
    /// Runs <see cref="CartridgeEncodingAnalyzer"/> over repository-relative sources with an optional baseline
    /// (null omits the additional file) and requires exactly the expected diagnostics, in order. SME6347 is checked only
    /// by the cases that expect it: the other rules' fixtures declare narrow members to exercise those rules.
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
        if (!expected.Any(item => item.Id == CartridgeEncodingAnalyzer.NarrowStorageMemberId))
            diagnostics = diagnostics.Where(diagnostic => diagnostic.Id != CartridgeEncodingAnalyzer.NarrowStorageMemberId).ToArray();
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

    /// <summary>Supplies an in-memory encoding baseline to a Roslyn analyzer fixture.</summary>
    internal sealed class MemoryAdditionalText(string path, string content) : AdditionalText
    {
        /// <summary>Gets the synthetic additional-file path used for baseline discovery.</summary>
        public override string Path { get; } = path;

        /// <summary>Returns the fixture baseline content without filesystem access.</summary>
        public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(content);
    }
}
