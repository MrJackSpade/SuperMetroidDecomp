using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;
using SuperMetroid.EnsureAnalyzer.Contracts;

namespace SuperMetroid.EnsureVerification.CartridgeEncoding;

/// <summary>Checks cartridge-encoding diagnostics and their scope against small C# source fixtures.</summary>
internal static class CartridgeEncodingAnalyzerTests
{
    /// <summary>Stable temporary root used to give fixture sources repository-relative paths.</summary>
    private static readonly string EncodingRoot = Path.Combine(Path.GetTempPath(), "cartridge-encoding-case");

    /// <summary>#641 cartridge encoding: positive and negative cases for each diagnostic, and the analyzer's scope.</summary>
    internal static void VerifyCartridgeEncodingAnalyzer()
    {
        const string functional = "csharp/src/SuperMetroid.Core/Game/Walker.cs";
        const string catalog = "csharp/src/SuperMetroid.Core/Game/WalkerInstructionLists.cs";
        const string pointers =
            "static class WalkerInstructionLists { public const ushort Idle = 0x9389; public const ushort Turn = 0x93a1; } " +
            "static class Limits { public const ushort Ceiling = 0x9000; }";

        // Every repository assembly is in scope except the analyzer projects; other assemblies are never analyzed.
        ExpectEncoding("SuperMetroid.Verification", [new AnalyzerTestSource(functional, "class C { int M() => 0x10; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawHexId, "NumericQuantity"));
        ExpectEncoding("SuperMetroid.EnsureAnalyzer", [new AnalyzerTestSource(functional, "class C { int M() => 0x10; }")]);
        ExpectEncoding("ThirdParty.Library", [new AnalyzerTestSource(functional, "class C { int M() => 0x10; }")]);

        // SME6340: functional code holds no hexadecimal literal; each one is reported in place.
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int a) => (a & 0x0f) + 0x10; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawHexId, "ComposableBits (bitwise operand)"),
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawHexId, "NumericQuantity"));
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int a) => a & 0x0f; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawHexId, "ComposableBits (bitwise operand)"));
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int a) => a + 0x10; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawHexId, "NumericQuantity"));
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M() => 16; }")]);
        // Catalog-owned values, reviewed allowances and build output are not functional literals.
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerInstructionLists { public const ushort Idle = 0x9389; }")]);
        // A file named for addresses, pointers, ROM data or tables is not a catalog (#1411): its literals are reported.
        foreach (string suffix in new[] { "Addresses", "CodePointers", "Pointers", "RomData", "Tables" })
        {
            ExpectEncoding([new AnalyzerTestSource($"csharp/src/SuperMetroid.Core/Game/Walker{suffix}.cs", $"static class Walker{suffix} {{ public const int Idle = 0x10; }}")],
                new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawHexId, ""));
        }
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int a) => a & 0x0f; // cartridge-encoding: allow(Mask) - low nibble\n }")]);
        ExpectEncoding([new AnalyzerTestSource("csharp/src/SuperMetroid.Core/obj/Release/Generated.cs", "class C { int M() => 0x10; }")]);


        // SME6341: a primitive interpreted against address-valued code or instruction constants.
        ExpectEncoding([new AnalyzerTestSource(catalog, pointers), new AnalyzerTestSource(functional, "class C { bool M(ushort p) => p == WalkerInstructionLists.Idle; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.PointerDispatchId, "CartridgeAddress"));
        ExpectEncoding([new AnalyzerTestSource(catalog, pointers), new AnalyzerTestSource(functional, "class C { int M(ushort p) => p - WalkerInstructionLists.Turn; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.PointerDispatchId, "offset from"));
        ExpectEncoding([new AnalyzerTestSource(catalog, pointers),
                new AnalyzerTestSource(functional, "class C { int M(ushort p) { switch (p) { case WalkerInstructionLists.Idle: return 1; default: return 0; } } }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.PointerDispatchId, "switch arm"));
        ExpectEncoding([new AnalyzerTestSource(catalog, pointers), new AnalyzerTestSource(functional, "class C { bool M(ushort p) => p is WalkerInstructionLists.Turn; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.PointerDispatchId, "pattern"));
        // Ordinary limits, constant folding and typed identities are not pointer dispatch.
        ExpectEncoding([new AnalyzerTestSource(catalog, pointers), new AnalyzerTestSource(functional, "class C { bool M(ushort p) => p >= Limits.Ceiling; }")]);
        ExpectEncoding([new AnalyzerTestSource(catalog, pointers), new AnalyzerTestSource(functional, "class C { const int Next = WalkerInstructionLists.Idle + 4; }")]);
        ExpectEncoding([new AnalyzerTestSource(catalog, "enum WalkerProgram : ushort { Idle = 0x9389 }"),
            new AnalyzerTestSource(functional, "class C { bool M(WalkerProgram p) => p == WalkerProgram.Idle; }")]);

        // SME6342: integral array and collection literals holding hexadecimal words, catalogs included.
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerInstructionLists { static readonly ushort[] Words = { 0x812f, 0x0001, 0xe329, 0x80ed }; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.OpaqueWordArrayId, "StructuredInstruction"));
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerInstructionLists { static readonly byte[] Words = [0x12, 0x34, 0x56, 0x78]; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.OpaqueWordArrayId, "collection literal"));
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerInstructionLists { static readonly ushort[] Words = { 1, 2, 3, 4 }; }")]);
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerInstructionLists { static readonly ushort[] Words = { 0x812f, 0x0001, 0xe329 }; }")]);

        // SME6345 (#223): a shift by a byte, word or bank width or a byte/word/bank mask must name its meaning.
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { byte M(ushort word) => (byte)(word >> 8); }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawWidthId, "moves a value down"));
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int bank, ushort offset) => bank << 16 | offset; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawWidthId, "moves a value up"));
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int value) => value & 255; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawWidthId, "keeps a byte, word or bank lane"));
        // A nested chain is one operation.
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int value) => (value >> 8) & 255; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawWidthId, "'(value >> 8) & 255'"));
        // Shifts and masks of other widths, constant folding, catalogs, owners and reviewed allowances are not reported.
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int value) => (value >> 3) + (value & 7) + (value << 4); }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { const int Bank = 126 << 16; int M() => Bank; }")]);
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerInstructionLists { static int M(int pointer) => pointer >> 16; }")]);
        // A named catalog mask or shift states the width's meaning; byte.MaxValue only names a storage width.
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerLayout { public const int PackedXMask = 255; }"),
            new AnalyzerTestSource(functional, "class C { int M(int packed) => packed & WalkerLayout.PackedXMask; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int value) => value & byte.MaxValue; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.RawWidthId, "keeps a byte, word or bank lane"));
        ExpectEncoding([new AnalyzerTestSource(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " +
            "[SnesEncodingOwner] readonly struct Word(ushort raw) { public byte High => (byte)(raw >> 8); }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { int M(int value) => value >> 8; // cartridge-encoding: allow(ByteLane) - DSP register high byte\n }")]);

        // SME6348 (#1324): a hand-written sign-bit test on a countdown-named member or local.
        const string countdown = "class C { ushort FunctionTimer; byte DelayFrames; ";
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "bool M() => (FunctionTimer & 32768) != 0; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "tests a countdown's sign bit by hand"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "bool M() => (DelayFrames & 128) == 0; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "'DelayFrames & 128'"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "bool M() => FunctionTimer >= 32768; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "'FunctionTimer >= 32768'"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "bool M() => unchecked((short)FunctionTimer) < 0; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "tests a countdown's sign bit by hand"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "bool M() => 0 <= (sbyte)DelayFrames; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "tests a countdown's sign bit by hand"));
        // Signed comparisons of quantities, velocities, packed flags and enums are other rules' work.
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "ushort VelocityX; bool M() => (short)VelocityX < 0; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "ushort Left; bool M() => (short)(FunctionTimer - Left) < 0; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "ushort Parameter1; bool M() => (Parameter1 & 32768) != 0; }")]);
        // The head noun decides: an angle whose name merely contains "Counter" is not a countdown.
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "ushort TargetCounterClockwiseAngle; bool M() => (TargetCounterClockwiseAngle & 32768) != 0; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "ushort DelayOrCommand; bool M() => (DelayOrCommand & 32768) != 0; }")]);
        // A word compared with a byte's sign bit is a threshold, not a sign test, and a byte has no bit 15.
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "bool M() => FunctionTimer >= 128; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "bool M() => (DelayFrames & 32768) != 0; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "int ElapsedFrames; bool M() => ElapsedFrames < 128; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "int WaitTimer; bool M() => WaitTimer >= 32768; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "'WaitTimer >= 32768'"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "int WaitTimer; bool M() => (WaitTimer & 128) != 0; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "'WaitTimer & 128'"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "ushort[] SegmentTimers = new ushort[2]; bool M() => (SegmentTimers[0] & 32768) != 0; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSignTestId, "'SegmentTimers[0] & 32768'"));
        ExpectEncoding([new AnalyzerTestSource(functional, "enum TimerFlags : ushort { Sign = 32768 } " +
            "class C { TimerFlags Timer; bool M() => (Timer & TimerFlags.Sign) != 0; }")]);

        // SME6349 (#1324): a hand-written wrapping self-decrement of narrow storage.
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "void M() => FunctionTimer = unchecked((ushort)(FunctionTimer - 1)); }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "decrements narrow storage with native wraparound"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "void M() => DelayFrames = unchecked((byte)(DelayFrames - 1)); }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "decrements narrow storage with native wraparound"));
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "void M() { FunctionTimer--; --DelayFrames; DelayFrames -= 1; } }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "'FunctionTimer--'"),
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "'--DelayFrames'"),
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.CountdownSelfDecrementId, "'DelayFrames -= 1'"));
        // Locals, wide integers and a decrement into a different member are not the DEC idiom.
        ExpectEncoding([new AnalyzerTestSource(functional, countdown + "int Wide; ushort Next; " +
            "void M(ushort count) { count--; Wide--; Next = unchecked((ushort)(FunctionTimer - 1)); } }")]);
        // An encoding owner documents the idiom itself.
        ExpectEncoding([new AnalyzerTestSource(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " +
            "[SnesEncodingOwner] struct Countdown { ushort timer; bool M() { timer--; return (timer & 32768) != 0; } }")]);

        // SME6346 (#1323): a [Flags] value cast to an integral primitive leaves its type before the encoding boundary.
        const string items = "[System.Flags] enum Items : ushort { Morph = 4, Bombs = 4096 } enum Mode : ushort { Auto = 1 } ";
        ExpectEncoding([new AnalyzerTestSource(functional, items + "class C { ushort M() => (ushort)Items.Morph; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.FlagsPrimitiveCastId, "casts [Flags] Items to ushort"));
        ExpectEncoding([new AnalyzerTestSource(functional, items + "class C { int M(Items held) => unchecked((int)held); }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.FlagsPrimitiveCastId, "'(int)held'"));
        // A plain enum, a primitive-to-flags conversion, an owner and a reviewed allowance are not reported.
        ExpectEncoding([new AnalyzerTestSource(functional, items + "class C { ushort M(ushort word) => (ushort)Mode.Auto; Items N(ushort word) => (Items)word; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " + items +
            "[SnesEncodingOwner] static class ItemWords { public static ushort Native(Items items) => (ushort)items; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, items + "class C { ushort M(Items held) => (ushort)held; // cartridge-encoding: allow(Codec) - SRAM word\n }")]);

        // SME6347 (#1323): byte and ushort instance fields, properties and positional record parameters.
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { ushort health; public byte Mode { get; set; } }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'health' stores ushort instance state"),
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'Mode' stores byte instance state"));
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { ushort a, b; ushort? sentinel; }")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'a'"),
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'b'"),
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'sentinel' stores ushort? instance state"));
        ExpectEncoding([new AnalyzerTestSource(functional, "record Slot(ushort Health, int Area);")],
            new ExpectedDiagnostic(CartridgeEncodingAnalyzer.NarrowStorageMemberId, "'Health' stores ushort instance state"));
        // Constants, statics, wide members, locals, parameters, catalogs, owners and reviewed allowances are not reported.
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { const ushort Limit = 99; static ushort shared; int wide; ushort[] words = new ushort[2]; " +
            "int M(ushort argument) { ushort local = argument; return local + Limit + shared; } }")]);
        ExpectEncoding([new AnalyzerTestSource(catalog, "static class WalkerInstructionLists { } sealed class WalkerRecord { public ushort Word { get; init; } }")]);
        ExpectEncoding([new AnalyzerTestSource(functional,
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } " +
            "[SnesEncodingOwner] readonly struct Word(ushort raw) { readonly ushort value = raw; }")]);
        ExpectEncoding([new AnalyzerTestSource(functional, "class C { ushort frameCounter; // cartridge-encoding: allow(Wrap) - wraps by design\n }")]);
    }

    /// <summary>
    /// Runs <see cref="CartridgeEncodingAnalyzer"/> over repository-relative sources compiled as Core and requires exactly
    /// the expected diagnostics, in order. SME6347 is checked only by the cases that expect it: the other rules' fixtures
    /// declare narrow members to exercise those rules.
    /// </summary>
    private static void ExpectEncoding(AnalyzerTestSource[] files, params ExpectedDiagnostic[] expected) =>
        ExpectEncoding("SuperMetroid.Core", files, expected);

    /// <summary>Runs the analyzer over sources compiled as <paramref name="assemblyName"/>; see the Core overload.</summary>
    private static void ExpectEncoding(
        string assemblyName,
        AnalyzerTestSource[] files,
        params ExpectedDiagnostic[] expected)
    {
        var trees = files.Select(file => CSharpSyntaxTree.ParseText(file.Source,
            new CSharpParseOptions(LanguageVersion.Preview), Path.Combine(EncodingRoot, file.Path.Replace('/', Path.DirectorySeparatorChar))));
        var compilation = CSharpCompilation.Create(assemblyName, trees, References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        var diagnostics = compilation
            .WithAnalyzers([new CartridgeEncodingAnalyzer()])
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
}
