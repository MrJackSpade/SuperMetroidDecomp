using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.Core;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Hardware;
using SuperMetroid.EnsureAnalyzer.Rooms;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.EnsureVerification.Harness;

/// <summary>Compiles small C# fixtures to check Ensure usage and repository source-boundary analyzer diagnostics.</summary>
internal static class AnalyzerTests
{
    /// <summary>Runtime and core references supplied to isolated analyzer fixture compilations.</summary>
    internal static readonly MetadataReference[] References = BuildReferences();

    /// <summary>Checks supported guard rewrites and preserves nonmatching or domain-specific validation.</summary>
    internal static void VerifyAnalyzer()
    {
        VerifySnippet("using System; class C { public void M(object value) { ArgumentNullException.ThrowIfNull(value); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.NotNull");
        VerifySnippet("using System; class C { public void M(int value) { ArgumentOutOfRangeException.ThrowIfNegative(value); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.AtLeastZero");
        VerifySnippet("using System; class C { public void M(object? value) { if (value is null) throw new ArgumentNullException(nameof(value)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.NotNull");
        VerifySnippet("using System; class C { public void M(int value) { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.AtLeastZero");
        VerifySnippet("using System; class C { public void M(int value) { if (value < 0 || value > 64) throw new ArgumentOutOfRangeException(nameof(value)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.BetweenInclusive");
        VerifySnippet("using System; class C { public void M(int[] items) { if (items.Length != 4) throw new ArgumentOutOfRangeException(nameof(items)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.LengthEqual");
        VerifySnippet("using System; class C { public void M(int[] items) { if (items.Length != 4) throw new ArgumentException(\"Expected four items.\", nameof(items)); } }",
            EnsureUsageAnalyzer.GuardId, "Ensure.LengthEqual");
        VerifySnippet("using System; class C { public object M(object? value) => value ?? throw new ArgumentNullException(nameof(value)); }",
            EnsureUsageAnalyzer.GuardId, "Ensure.NotNull");

        // Guards without a shared Ensure operation are left as written.
        VerifySnippet("using System; class C { public void M(int value) { if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value)); } }");
        VerifySnippet("using System; class C { public void M(int value) { if (value is not (32 or 64)) throw new ArgumentOutOfRangeException(nameof(value)); } }");
        VerifySnippet("using System; enum Mode { A, B } class C { public void M(Mode mode) { if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode)); } }");
        VerifySnippet("using System; using SuperMetroid.Core; class C { public void M(object value) { Ensure.NotNull(value); } }");
        VerifySnippet("using System; using System.IO; class C { public void M(int value) { if (value < 0) throw new InvalidDataException(\"corrupt state\"); } }");
        VerifySnippet("using System; class C { private void PortNative(int spawnArgument) { if (spawnArgument < 0) throw new ArgumentOutOfRangeException(nameof(spawnArgument)); } }");
        VerifySnippet("using System; class C { public void M(int value) { if (value > 3) throw new ArgumentOutOfRangeException(nameof(value), \"Cartridge phase is invalid.\"); } }");
        VerifySnippet("using System; class C { public void M(int width, int height) { if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width)); } }");
    }

    /// <summary>Requires a fixture to compile cleanly, then compares its analyzer output with one expected diagnostic or none.</summary>
    internal static void VerifySnippet(string source, string? expectedId = null, string? expectedMessage = null,
        DiagnosticAnalyzer? analyzer = null, string assemblyName = "EnsureAnalyzerCase")
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [tree],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");

        var analyzers = ImmutableArray.Create(analyzer ?? new EnsureUsageAnalyzer());
        var diagnostics = compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (expectedId is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected analyzer diagnostic: {diagnostics[0]} in {source}");
            return;
        }
        if (diagnostics.Length != 1 || diagnostics[0].Id != expectedId ||
            diagnostics[0].Severity != diagnostics[0].Descriptor.DefaultSeverity ||
            (expectedMessage is not null && !diagnostics[0].GetMessage().Contains(expectedMessage, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Expected one {expectedId} diagnostic containing '{expectedMessage}', got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
        }
    }

    /// <summary>#628 spatial data-clump rules: positive and negative cases for each diagnostic.</summary>
    internal static void VerifyCpuBusAccessAnalyzer()
    {
        var analyzer = new CpuBusAccessAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string api =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CpuBusAccessAttribute : System.Attribute { } " +
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CpuBusBoundaryAttribute(string reason) : System.Attribute { } " +
            "[CpuBusAccess] interface IBus { void WriteByte(int address, byte value); byte ReadWorkRamByte(int address); } " +
            "[CpuBusAccess] static class WorkRam { public static ushort ReadWord(IBus bus, int address) => 0; } " +
            "[CpuBusAccess] readonly struct Word { public Word(IBus bus, int address) { } public ushort Read() => 0; } " +
            "[CpuBusBoundary(\"implementation\")] sealed class Bus : IBus { public void WriteByte(int a, byte v) { } public byte ReadWorkRamByte(int a) => 0; " +
            "[CpuBusAccess] public System.Span<byte> SaveRam => default; } ";

        // SME6290: raw reads and writes, helpers, buffers, primitives and method groups outside a boundary.
        Expect(api + "class Game { void M(IBus bus) => bus.WriteByte(0x7e0a1c, 1); }",
            CpuBusAccessAnalyzer.RawAccessId, "'IBus.WriteByte' is raw CPU-bus access");
        Expect(api + "class Game { ushort M(IBus bus) => WorkRam.ReadWord(bus, 0x7e0998); }",
            CpuBusAccessAnalyzer.RawAccessId, "'WorkRam.ReadWord'");
        Expect(api + "class Game { byte M(Bus bus) => bus.ReadWorkRamByte(0x7e0998); }",
            CpuBusAccessAnalyzer.RawAccessId, "'Bus.ReadWorkRamByte'");
        Expect(api + "class Game { int M(Bus bus) => bus.SaveRam.Length; }",
            CpuBusAccessAnalyzer.RawAccessId, "'Bus.SaveRam'");
        Expect(api + "class Game { object M(IBus bus) => new Word(bus, 0x7e0998); }",
            CpuBusAccessAnalyzer.RawAccessId, "'Word.ctor'");
        Expect(api + "class Game { System.Func<int, byte> M(IBus bus) => bus.ReadWorkRamByte; }",
            CpuBusAccessAnalyzer.RawAccessId, "'IBus.ReadWorkRamByte'");
        // A typed domain view, a boundary member or property, and code that only passes the bus along pass.
        Expect(api + "[CpuBusBoundary(\"domain view\")] sealed class SamusMemory(IBus bus) { public ushort Health { get => WorkRam.ReadWord(bus, 0x7e09c2); } } " +
            "class Game { ushort M(SamusMemory samus) => samus.Health; }");
        Expect(api + "class Codec { [CpuBusBoundary(\"codec\")] static byte M(IBus bus) => bus.ReadWorkRamByte(0); " +
            "ushort P(IBus bus) { return Q; } [CpuBusBoundary(\"view\")] ushort Q => new Word(null!, 0).Read(); }");
        Expect(api + "class Game { SamusView M(IBus bus) => new SamusView(bus); } class SamusView(IBus bus) { }");

        // SME6291: raw contracts implemented outside a boundary.
        Expect(api + "sealed class Fake : IBus { public void WriteByte(int a, byte v) { } public byte ReadWorkRamByte(int a) => 0; }",
            CpuBusAccessAnalyzer.RawImplementationId, "Fake implements raw CPU-bus contract 'IBus'");
        Expect(api + "[CpuBusBoundary(\"exact bus test double\")] sealed class Fake : IBus { public void WriteByte(int a, byte v) { } public byte ReadWorkRamByte(int a) => 0; }");
    }

    /// <summary>Checks raw PPU access diagnostics while accepting typed layout views and declared boundary members.</summary>
    internal static void VerifyPpuLayoutAccessAnalyzer()
    {
        var analyzer = new PpuLayoutAccessAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string api =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class PpuLayoutAccessAttribute : System.Attribute { } " +
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class PpuLayoutBoundaryAttribute(string reason) : System.Attribute { } " +
            "[PpuLayoutAccess] sealed class Vram { public ushort ReadWord(int word) => 0; public void LoadBytes(int offset, byte[] bytes) { } } " +
            "[PpuLayoutAccess] static class Registers { public const ushort MainScreen = 0x69; } " +
            "[PpuLayoutAccess] interface IOamSink { void Write(int offset, byte value); } " +
            "[PpuLayoutBoundary(\"backend\")] sealed class Oam { [PpuLayoutAccess] public System.ReadOnlySpan<byte> LowTable => default; public void AddSprite() { } } ";

        // SME6300: raw VRAM/register/OAM access, including register-number constants, outside a boundary.
        Expect(api + "class Renderer { ushort M(Vram vram) => vram.ReadWord(0x5000); }",
            PpuLayoutAccessAnalyzer.RawAccessId, "'Vram.ReadWord' is raw PPU layout access");
        Expect(api + "class Renderer { ushort M() => Registers.MainScreen; }",
            PpuLayoutAccessAnalyzer.RawAccessId, "'Registers.MainScreen'");
        Expect(api + "class Renderer { int M(Oam oam) => oam.LowTable.Length; }",
            PpuLayoutAccessAnalyzer.RawAccessId, "'Oam.LowTable'");
        // Typed members of a backend, a rendering view, and passing the backend along pass.
        Expect(api + "class Renderer { void M(Oam oam) => oam.AddSprite(); }");
        Expect(api + "[PpuLayoutBoundary(\"rendering view\")] sealed class Bg1Tilemap(Vram vram) { public ushort At(int cell) => vram.ReadWord(0x5000 + cell); } " +
            "class Renderer { ushort M(Bg1Tilemap map) => map.At(3); }");
        Expect(api + "class Renderer { Bg1Tilemap M(Vram vram) => new(vram); } class Bg1Tilemap(Vram vram) { }");

        // SME6301: raw PPU contracts implemented outside a boundary.
        Expect(api + "sealed class Sink : IOamSink { public void Write(int offset, byte value) { } }",
            PpuLayoutAccessAnalyzer.RawImplementationId, "Sink implements raw PPU contract 'IOamSink'");
        Expect(api + "[PpuLayoutBoundary(\"exact hardware test\")] sealed class Sink : IOamSink { public void Write(int offset, byte value) { } }");
    }

    /// <summary>Checks that frontend classes cannot hold PPU memory while structs, locals and non-frontend owners pass (#1358).</summary>
    internal static void VerifyFrontendPpuMemoryAnalyzer()
    {
        var analyzer = new FrontendPpuMemoryAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string api =
            "namespace SuperMetroid.Core.Hardware { sealed class SnesVram { } sealed class SnesCgram { } sealed class OamBuffer { } } ";

        // SME6302: a frontend class field, property or primary-constructor parameter holding PPU memory.
        Expect(api + "namespace SuperMetroid.Core.Frontend.Title { using SuperMetroid.Core.Hardware; sealed class Screen { private readonly SnesVram vram = new(); } }",
            FrontendPpuMemoryAnalyzer.OwnedMemoryId, "Screen holds SnesVram in 'vram'");
        Expect(api + "namespace SuperMetroid.Core.Frontend { using SuperMetroid.Core.Hardware; sealed class Screen { public SnesCgram Cgram { get; } = new(); } }",
            FrontendPpuMemoryAnalyzer.OwnedMemoryId, "Screen holds SnesCgram in 'Cgram'");
        Expect(api + "namespace SuperMetroid.Core.Frontend.Pause { using SuperMetroid.Core.Hardware; sealed class Screen(OamBuffer oam) { public int M() => oam.GetHashCode(); } }",
            FrontendPpuMemoryAnalyzer.OwnedMemoryId, "Screen holds OamBuffer in 'oam'");
        // A transient rendering view, a codec's local scratch memory and a non-frontend owner pass.
        Expect(api + "namespace SuperMetroid.Core.Frontend.Title { using SuperMetroid.Core.Hardware; readonly struct TitleVram(SnesVram vram) { public SnesVram Memory => vram; } }");
        Expect(api + "namespace SuperMetroid.Core.Frontend.Title { using SuperMetroid.Core.Hardware; static class Codec { public static int Compose() { var vram = new SnesVram(); return vram.GetHashCode(); } } }");
        Expect(api + "namespace SuperMetroid.Core.Gameplay { using SuperMetroid.Core.Hardware; sealed class Runtime { private readonly SnesVram vram = new(); } }");
    }

    /// <summary>Checks that only XML documentation and in-body comments remain outside executable bodies (#1378).</summary>
    internal static void VerifyLooseCommentAnalyzer()
    {
        var analyzer = new LooseCommentAnalyzer();
        void Expect(string source, string? id = null) => VerifySnippet(source, id, null, analyzer);

        // SME6413: a type-level comment, a comment trailing a field, a comment in a static array initialiser.
        Expect("class C { // the counter\n int x; }", LooseCommentAnalyzer.Id);
        Expect("class C { int x; // the counter\n }", LooseCommentAnalyzer.Id);
        Expect("class C { static readonly int[] T = [ 1, // first\n 2 ]; }", LooseCommentAnalyzer.Id);
        // A comment in a method, an accessor and a lambda, XML documentation and the generated marker pass.
        Expect("class C { void M() { // why\n } }");
        Expect("class C { int P { get { /* why */ return 1; } } }");
        Expect("class C { System.Func<int> F = () => { // why\n return 1; }; }");
        Expect("class C { /// <summary>The counter.</summary>\n int x; }");
        Expect("// <auto-generated/>\nclass C { }");
        Expect("class C { const int Mask = 0x7FFF; // cartridge-encoding: allow(Mask) - low fifteen bits\n }");
        Expect("class C { const int Mask = 0x7FFF; // cartridge-encoding: allow(Mask)\n }", LooseCommentAnalyzer.Id);
        // #region and #endregion are each reported.
        var regions = CSharpCompilation.Create("RegionCase", [CSharpSyntaxTree.ParseText("class C {\n#region Fields\n int x;\n#endregion\n}")],
                References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (regions.Length != 2 || regions.Any(diagnostic => diagnostic.Id != LooseCommentAnalyzer.Id))
            throw new InvalidOperationException($"#region and #endregion must each be reported once: {string.Join("; ", regions)}");
    }

    /// <summary>Checks numbered member families, index-to-member switches and hand-written list types (#1369).</summary>
    internal static void VerifyNumberedSequenceAnalyzer()
    {
        var analyzer = new NumberedSequenceAnalyzer();
        void Expect(string source, string? id = null, string? message = null) => VerifySnippet(source, id, message, analyzer);
        const string attribute = "[System.AttributeUsage(System.AttributeTargets.All)] sealed class NumberedIdentitiesAttribute(string reason) : System.Attribute { }";

        // SME6415: a same-typed family whose ordinals run gap-free from 0, or from 1.
        Expect("record Neck { public int Segment0Distance { get; init; } public int Segment1Distance { get; init; } public int Segment2Distance { get; init; } }",
            NumberedSequenceAnalyzer.FamilyId, "'Segment0Distance'..'Segment2Distance'");
        Expect("class Shutter { int level1; int level2; int level3; int level4; }", NumberedSequenceAnalyzer.FamilyId, "4 elements");
        // A gapped family, a mixed-type family, two members, and a justified family pass.
        Expect("class C { int Sound01; int Sound03; int Sound07; }");
        Expect("class C { int Part0; long Part1; int Part2; }");
        Expect("class C { int Joint0; int Joint1; }");
        Expect(attribute + " class Poses { [NumberedIdentities(\"each pose is its own animation\")] int Pose0; int Pose1; int Pose2; }");

        // SME6416: consecutive integer arms that each pick one member of a family.
        Expect("class Shutter { int Level0Y; int Level1Y; int Y(int level) => level switch { 0 => Level0Y, 1 => Level1Y, _ => throw new System.Exception() }; }",
            NumberedSequenceAnalyzer.IndexSwitchId, "indices 0..1");
        Expect("class C { int A0; int A1; void Set(int i, int v) { switch (i) { case 0: A0 = v; break; case 1: A1 = v; break; } } }",
            NumberedSequenceAnalyzer.IndexSwitchId);
        // Arms that pick unrelated members or skip an index pass.
        Expect("class C { int Surface; int Dark; int Y(int i) => i switch { 0 => Surface, 1 => Dark, _ => 0 }; }");
        Expect("class C { int A0; int A2; int Y(int i) => i switch { 0 => A0, 2 => A2, _ => 0 }; }");

        // SME6417: an indexer that switches over constant indices, or ignores its index, beside a hand-written enumerator.
        Expect("using System.Collections; using System.Collections.Generic; class Pair : IReadOnlyList<int> { public int Count => 2; " +
            "public int this[int index] => index switch { 0 => 1, 1 => 2, _ => throw new System.ArgumentOutOfRangeException() }; " +
            "public IEnumerator<int> GetEnumerator() { yield return this[0]; yield return this[1]; } IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); }",
            NumberedSequenceAnalyzer.HandWrittenListId, "switches over constant indices");
        Expect("using System.Collections; using System.Collections.Generic; class Same : IEnumerable<int> { public int this[int index] { get { if (index < 0) throw new System.ArgumentOutOfRangeException(nameof(index)); return 7; } } " +
            "public IEnumerator<int> GetEnumerator() { yield return this[0]; } IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); }",
            NumberedSequenceAnalyzer.HandWrittenListId, "same value for every index");
        // An indexer that computes its value from the index, past a bounds check, passes.
        Expect("using System.Collections; using System.Collections.Generic; class Computed : IEnumerable<int> { public int this[int index] { get { if (index < 0) throw new System.ArgumentOutOfRangeException(); int doubled = index * 2; return doubled; } } " +
            "public IEnumerator<int> GetEnumerator() { yield return this[0]; } IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); }");
        // A returned choice between two values by the index is a two-element list; a comparison inside a calculation is not.
        Expect("using System.Collections; using System.Collections.Generic; class Two : IEnumerable<int> { int a; int b; public int this[int index] => index == 0 ? a : b; " +
            "public IEnumerator<int> GetEnumerator() { yield return this[0]; } IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); }",
            NumberedSequenceAnalyzer.HandWrittenListId, "switches over constant indices");
        Expect("using System.Collections; using System.Collections.Generic; class Calc : IEnumerable<int> { public int this[int index] { get { int x = index == 0 ? 8 : 0; return x + index; } } " +
            "public IEnumerator<int> GetEnumerator() { yield return this[0]; } IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); }");
        // An indexer over real storage passes.
        Expect("using System.Collections; using System.Collections.Generic; class Wrap : IEnumerable<int> { int[] items = [1, 2]; public int this[int index] => items[index]; " +
            "public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)items).GetEnumerator(); IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); }");
    }

    /// <summary>Checks that a mirrored method pair is reported, a genuinely different pair is not, and a justified pair is not (#1370).</summary>
    internal static void VerifyMirroredMethodPairAnalyzer()
    {
        var analyzer = new MirroredMethodPairAnalyzer();
        void Expect(string source, string? id = null, string? message = null) => VerifySnippet(source, id, message, analyzer);
        const string attribute = "[System.AttributeUsage(System.AttributeTargets.Method)] sealed class MirroredAsymmetryAttribute(string reason) : System.Attribute { }";

        // SME6418: mirrored identifiers, a sign-flipped literal and mirrored operators.
        Expect("class Shutter { int x; int leftEnd; int rightEnd; " +
            "void MoveLeft() { x -= 2; if (x < leftEnd) x = leftEnd; } " +
            "void MoveRight() { x += 2; if (x > rightEnd) x = rightEnd; } }",
            MirroredMethodPairAnalyzer.MirroredPairId, "'MoveLeft' and 'MoveRight'");
        Expect("class Draygon { int v; void AddVelocity(int a) { v = v + a * -1; } void SubtractVelocity(int a) { v = v - a * 1; } }",
            MirroredMethodPairAnalyzer.MirroredPairId);
        Expect("enum Pose { SpinRight, SpinLeft, FallRight, FallLeft } static class Poses { " +
            "static bool IsSpinRight(Pose p) => p is Pose.SpinRight or Pose.FallRight; static bool IsSpinLeft(Pose p) => p is Pose.SpinLeft or Pose.FallLeft; }",
            MirroredMethodPairAnalyzer.MirroredPairId);
        Expect("class Rotor { int a; void TurnClockwise() { a++; } void TurnCounterclockwise() { a--; } }", MirroredMethodPairAnalyzer.MirroredPairId);
        // A pair whose bodies differ beyond a mirror, a forwarding wrapper pair, a lone method and a justified pair pass.
        Expect("class C { int x; void StepLeft() { x -= 2; } void StepRight() { x += 2; x *= 3; } }");
        Expect("class C { int x; void Step(int d) { x += d; } void StepLeft() => Step(-1); void StepRight() => Step(1); }");
        Expect("class C { int x; void Update() { x++; } void Upload() { x--; } }");
        Expect("class C { int x; int y; void StepLeft() { x -= 2; } void StepRight() { y += 2; } }");
        // Both sides naming the same direction unflipped is a shared order, not a mirror.
        Expect("class C { int left; int right; int MatchLeft(int h) { if (h == 1) return left; return right; } int MatchRight(int h) { if (h == 1) return left; return right; } }");
        Expect(attribute + " class C { int x; [MirroredAsymmetry(\"the left wall has no ledge\")] void StepLeft() { x -= 2; } void StepRight() { x += 2; } }");
    }

    /// <summary>Checks that a clone group and a clone record are reported, and a near-miss and a justified copy are not (#1371).</summary>
    internal static void VerifyStructuralCloneAnalyzer()
    {
        var analyzer = new StructuralCloneAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer, assemblyName: StructuralCloneAnalyzer.ComparedAssembly);
        const string attribute = "[System.AttributeUsage(System.AttributeTargets.All)] sealed class IntendedDuplicateAttribute(string reason) : System.Attribute { }";
        const string body = "{ int total = 0; total += a; total += b; if (total > 9) total = 9; if (total < 0) total = 0; return total; }";

        // SME6419: the same body in two types, differing only in their own member names and string contents.
        Expect("class Alpha { int a; int b; int Sum() " + body + " } class Beta { int a; int b; int Combine() " + body + " }",
            StructuralCloneAnalyzer.StructuralCloneId, "Alpha.Sum, Beta.Combine");
        Expect("class Gamma { int g; void Check(int v) { if (v < 0) throw new System.Exception(\"gamma low\"); if (v > 9) throw new System.Exception(\"gamma high\"); g = v; g++; g--; } } " +
            "class Delta { int d; void Check(int v) { if (v < 0) throw new System.Exception(\"delta low\"); if (v > 9) throw new System.Exception(\"delta high\"); d = v; d++; d--; } }",
            StructuralCloneAnalyzer.StructuralCloneId);
        // A near-miss with a real behavioural difference, a short body, a copy within one type, and a justified copy pass.
        Expect("class Alpha { int a; int b; int Sum() " + body + " } class Beta { int a; int b; int Combine() { int total = 0; total += a; total -= b; if (total > 9) total = 9; if (total < 0) total = 0; return total; } }");
        Expect("class Alpha { int a; int Get() { return a; } } class Beta { int a; int Get() { return a; } }");
        Expect("class Alpha { int a; int b; int Sum() " + body + " int Again() " + body + " }");
        Expect(attribute + " class Alpha { int a; int b; int Sum() " + body + " } class Beta { int a; int b; [IntendedDuplicate(\"the copy runs on another thread\")] int Combine() " + body + " }");

        // SME6420: two member-free positional records with the same parameters.
        Expect("record KraidEntry(string Id, ushort[] Blocks); record CrocomireEntry(string Id, ushort[] Blocks);",
            StructuralCloneAnalyzer.CloneRecordId, "KraidEntry, CrocomireEntry");
        // Different parameter names, one-field typed identities, a record with members of its own, and a justified record pass.
        Expect("record struct DachoraFrame(int Pose); record struct AtomicFrame(int Pose);");
        // Parameter attributes (a serializer converter, for instance) make the records differ.
        Expect("[System.AttributeUsage(System.AttributeTargets.All)] sealed class ConverterAttribute : System.Attribute { } " +
            "record PartA(int X, int Y); record PartB([property: Converter] int X, int Y);");
        Expect("record KraidEntry(string Id, ushort[] Blocks); record CrocomireEntry(string Id, ushort[] Runs);");
        Expect("record KraidEntry(string Id, ushort[] Blocks); record CrocomireEntry(string Id, ushort[] Blocks) { public int Count => Blocks.Length; }");
        Expect(attribute + " record KraidEntry(string Id, ushort[] Blocks); [IntendedDuplicate(\"serialized under its own schema\")] record CrocomireEntry(string Id, ushort[] Blocks);");
        // Other assemblies are not compared.
        VerifySnippet("record KraidEntry(string Id, ushort[] Blocks); record CrocomireEntry(string Id, ushort[] Blocks);", null, null, analyzer, assemblyName: "Other");
    }

    /// <summary>Checks that every pragma warning disable is reported and the sanctioned per-site form is not (#1396).</summary>
    internal static void VerifyPragmaWarningDisableAnalyzer()
    {
        var analyzer = new PragmaWarningDisableAnalyzer();
        void Expect(string source, string? id = null) => VerifySnippet(source, id, null, analyzer);

        // SME6414: a disable with a reason, without one, and naming several diagnostics.
        Expect("class C { void M() {\n#pragma warning disable CS0168 // the reason\n int x;\n#pragma warning restore CS0168\n } }", PragmaWarningDisableAnalyzer.Id);
        Expect("class C { void M() {\n#pragma warning disable CS0168\n int x; } }", PragmaWarningDisableAnalyzer.Id);
        Expect("#pragma warning disable CS0618, CS0672\nclass C { }", PragmaWarningDisableAnalyzer.Id);
        // A restore on its own and a justified SuppressMessage pass.
        Expect("class C { void M() {\n#pragma warning restore CS0168\n } }");
        Expect("class C { [System.Diagnostics.CodeAnalysis.SuppressMessage(\"Usage\", \"CA1801\", Justification = \"The platform calls it by name.\")] void M(int unused) { } }");
    }

    /// <summary>Checks that only the PLM slot table claims a PLM slot (#1361).</summary>
    internal static void VerifyPlmSlotAllocationAnalyzer()
    {
        var analyzer = new PlmSlotAllocationAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string api =
            "namespace SuperMetroid.Core.Rooms.Plms { sealed class PlmSlot { public bool Active { get; private set; } internal void Claim() => Active = true; } " +
            "sealed class PlmSlotTable { private readonly PlmSlot[] slots = new PlmSlot[40]; internal PlmSlot? SpawnHardcoded() { foreach (PlmSlot slot in slots) if (!slot.Active) { slot.Claim(); return slot; } return null; } } } ";

        // SME6303: an open-coded free-slot search claims its slot directly.
        Expect(api + "namespace SuperMetroid.Core.Rooms.Doors { using SuperMetroid.Core.Rooms.Plms; sealed class DoorPlms { void M(PlmSlot[] slots) { foreach (PlmSlot slot in slots) if (!slot.Active) { slot.Claim(); return; } } } }",
            PlmSlotAllocationAnalyzer.OpenCodedClaimId, "DoorPlms claims a PLM slot directly");
        // The slot table's spawn operations, and callers that use them, pass.
        Expect(api + "namespace SuperMetroid.Core.Rooms.Doors { using SuperMetroid.Core.Rooms.Plms; sealed class DoorPlms { PlmSlot? M(PlmSlotTable table) => table.SpawnHardcoded(); } }");
    }

    /// <summary>Checks DSP register-write and numeric-register diagnostics against typed control exceptions.</summary>
    internal static void VerifyDspRegisterAccessAnalyzer()
    {
        var analyzer = new DspRegisterAccessAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string api =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class DspRegisterAccessAttribute : System.Attribute { } " +
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class DspRegisterBoundaryAttribute(string reason) : System.Attribute { } " +
            "[DspRegisterAccess] enum GlobalRegister : byte { KeyOn = 0x4c } " +
            "[DspRegisterBoundary(\"implementation\")] sealed class Dsp { [DspRegisterAccess] public void WriteRegister(byte address, byte value) { } public void Cycle() { } } ";

        // SME6310: raw register writes and register numbers outside a boundary.
        Expect(api + "class Driver { void M(Dsp dsp) => dsp.WriteRegister(0x4c, 1); }",
            DspRegisterAccessAnalyzer.RawAccessId, "'Dsp.WriteRegister' is raw S-DSP register access");
        Expect(api + "class Driver { byte M() => (byte)GlobalRegister.KeyOn; }",
            DspRegisterAccessAnalyzer.RawAccessId, "'GlobalRegister.KeyOn'");
        // Typed controls, ordinary DSP members and passing the DSP along pass.
        Expect(api + "[DspRegisterBoundary(\"typed controls\")] readonly struct Controls(Dsp dsp) { public void KeyOn(byte voices) => dsp.WriteRegister((byte)GlobalRegister.KeyOn, voices); } " +
            "class Driver { void M(Controls dsp) { dsp.KeyOn(1); } }");
        Expect(api + "class Driver { void M(Dsp dsp) => dsp.Cycle(); }");
    }

    /// <summary>Checks direct APU RAM indexing diagnostics while permitting typed memory views.</summary>
    internal static void VerifySpcMemoryAccessAnalyzer()
    {
        var analyzer = new SpcMemoryAccessAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string api =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SpcMemoryAccessAttribute : System.Attribute { } " +
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SpcMemoryBoundaryAttribute(string reason) : System.Attribute { } " +
            "sealed class Player { [SpcMemoryAccess] private readonly byte[] ram = new byte[0x10000]; " +
            "[SpcMemoryBoundary(\"driver-memory view\")] private byte Next(ref ushort cursor) => ram[cursor++]; ";

        // SME6320: raw APU RAM indexing outside a boundary.
        Expect(api + "byte M(ushort pointer) => ram[pointer]; }",
            SpcMemoryAccessAnalyzer.RawAccessId, "'Player.ram' is raw SPC700 memory access");
        // A typed view and code that uses it pass.
        Expect(api + "byte M(ref ushort pointer) => Next(ref pointer); }");
    }

    /// <summary>Checks paired coordinate, parameter and parallel-array diagnostics without flagging named spatial values.</summary>
    internal static void VerifySpatialClumpAnalyzer()
    {
        var analyzer = new SpatialClumpAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string attribute = "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SpatialComponentsAttribute(string reason) : System.Attribute { }";

        // SME6280: paired X/Y, width/height or horizontal/vertical members of one type.
        Expect("class Camera { public ushort CameraX { get; set; } public ushort CameraY { get; set; } }",
            SpatialClumpAnalyzer.MemberClumpId, "'CameraX' and 'CameraY'");
        Expect("class Kinematics { public ushort ExtraXDisplacement; public ushort ExtraYDisplacement; }",
            SpatialClumpAnalyzer.MemberClumpId, "X/Y components");
        Expect("class Box { public int Width; public int Height; }",
            SpatialClumpAnalyzer.MemberClumpId, "width/height");
        // A spatial value type or a declared boundary adapter, unrelated names, or different types pass.
        Expect(attribute + " [SpatialComponents(\"value\")] struct Point { public ushort X; public ushort Y; }");
        Expect("class C { public int MaxValue; public int MaxCount; public ushort X; public int Y; }");
        Expect("class C { public int Index; public int Xenon; public int Yield; }");

        // SME6281: adjacent paired parameters.
        Expect("class C { void Move(ushort cameraX, ushort cameraY) { } }",
            SpatialClumpAnalyzer.ParameterClumpId, "'cameraX' and 'cameraY'");
        Expect("class C { void Draw(int x, int y, int palette) { } }",
            SpatialClumpAnalyzer.ParameterClumpId, "'x' and 'y'");
        Expect(attribute + " class C { [SpatialComponents(\"hardware\")] void Write(ushort x, ushort y) { } }");
        Expect("class C { void M(int x, int count, int y) { } }");

        // SME6282: parallel X/Y collections indexed by the same element.
        Expect("class Segments { public ushort[] SegmentXOffsets = []; public ushort[] SegmentYOffsets = []; }",
            SpatialClumpAnalyzer.CollectionClumpId, "'SegmentXOffsets' and 'SegmentYOffsets'");
        Expect("using System.Collections.Generic; class Path { public List<short> X = new(); public List<short> Y = new(); }",
            SpatialClumpAnalyzer.CollectionClumpId, "X/Y components");
        // A collection of spatial values, or an encapsulated data-oriented layout, passes.
        Expect(attribute + " [SpatialComponents(\"measured layout\")] class Soa { public float[] X = []; public float[] Y = []; }");

        // SME6283: a spatial value rebuilt from per-axis sums of two spatial values.
        const string point = attribute + " [SpatialComponents(\"value\")] readonly struct P { public P(int x, int y) { X = x; Y = y; } public int X { get; } public int Y { get; } public static P operator +(P a, P b) => new(a.X + b.X, a.Y + b.Y); }";
        Expect(point + " class C { P M(P a, P b) => new P(a.X + b.X, a.Y + b.Y); }",
            SpatialClumpAnalyzer.ComponentArithmeticId, "use the whole-value operator");
        // Mixed receivers or mixed operators are component-level work, not a duplicated operator.
        Expect(point + " class C { P M(P a, P b) => new P(a.X + b.X, a.Y - b.Y); }");
        Expect(point + " class C { P M(P a, P b, P c) => new P(a.X + b.X, a.Y + c.Y); }");
        // Building one type from another (a named space conversion) is not a duplicated operator.
        Expect("[SpatialComponents(\"value\")] readonly struct Q { public Q(int x, int y) { X = x; Y = y; } public int X { get; } public int Y { get; } } " +
            point + " class C { P M(Q room, Q camera) => new P(room.X - camera.X, room.Y - camera.Y); }");
    }

    /// <summary>#627 primitive-domain rules: positive and negative cases for each diagnostic.</summary>
    internal static void VerifyPrimitiveDomainAnalyzer()
    {
        var analyzer = new PrimitiveDomainAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string catalog = "static class Stage { public const byte Read = 2; public const byte Build = 4; }";

        // SME6270: a primitive switched over two named constants of one catalog.
        Expect(catalog + " class C { void M(byte stage) { switch (stage) { case Stage.Read: break; case Stage.Build: break; } } }",
            PrimitiveDomainAnalyzer.PrimitiveSwitchId, "named constants of Stage");
        Expect(catalog + " class C { int M(byte stage) => stage switch { Stage.Read => 1, Stage.Build => 2, _ => throw new System.Exception() }; }",
            PrimitiveDomainAnalyzer.PrimitiveSwitchId);
        // Open numeric quantities switched over literals, or a single constant, are not flagged.
        Expect("class C { int M(int count) => count switch { 0 => 1, 1 => 2, _ => 3 }; }");
        Expect(catalog + " class C { void M(byte stage) { switch (stage) { case Stage.Build: break; } } }");

        // SME6271: a repository-owned enum switch that ignores unexpected values.
        const string domain = "enum Mode : byte { A, B, C }";
        Expect(domain + " class C { void M(Mode m) { switch (m) { case Mode.A: break; default: break; } } }",
            PrimitiveDomainAnalyzer.SilentEnumSwitchId, "default that does not fail");
        Expect(domain + " class C { void M(Mode m) { switch (m) { case Mode.A: break; case Mode.B: break; } } }",
            PrimitiveDomainAnalyzer.SilentEnumSwitchId, "leaves members unhandled");
        Expect(domain + " class C { int M(Mode m) => m switch { Mode.A => 1, _ => 0 }; }",
            PrimitiveDomainAnalyzer.SilentEnumSwitchId, "discard arm that does not fail");
        // Exhaustive handling, a throwing default, and a non-owned enum are accepted.
        Expect(domain + " class C { void M(Mode m) { switch (m) { case Mode.A: case Mode.B: case Mode.C: break; } } }");
        Expect(domain + " class C { void M(Mode m) { switch (m) { case Mode.A: break; default: throw new System.ArgumentOutOfRangeException(nameof(m)); } } }");
        Expect(domain + " class C { int M(Mode m) => m switch { Mode.A => 1, Mode.B => 2, Mode.C => 3, _ => throw new System.ArgumentOutOfRangeException(nameof(m)) }; }");
        Expect("class C { int M(System.DayOfWeek d) => d switch { System.DayOfWeek.Monday => 1, _ => 0 }; }");
        // A [Flags] enum's catch-all covers the combinations a switch does not name.
        const string flags = "[System.Flags] enum Bits : byte { None = 0, X = 1, Y = 2 }";
        Expect(flags + " class C { int M(Bits b) => b switch { Bits.X => 1, _ => 0 }; }");
        Expect(flags + " class C { void M(Bits b) { switch (b) { case Bits.X: break; default: break; } } }");

        // SME6272: a masked or shifted local switched as a selector.
        Expect("class C { int M(byte header) { int direction = header & 3; return direction switch { 0 => 1, 1 => 2, _ => 3 }; } }",
            PrimitiveDomainAnalyzer.MaskedSelectorId, "by mask");
        Expect("class C { int M(ushort word) { int high = word >> 8; return high switch { 0 => 1, _ => 2 }; } }",
            PrimitiveDomainAnalyzer.MaskedSelectorId, "by shift");
        // A masked value used as arithmetic, not as a selector, is accepted.
        Expect("class C { int M(byte header) { int low = header & 3; return low + 1; } }");
        // A piecewise numeric function over a shifted index is not a selector.
        Expect("class C { int M(int index) { int frame = index >> 1; return frame switch { < 32 => 1, 70 => 2, _ => 3 }; } }");
        // SME6273: an enum with a width or other numeric format throws at run time.
        Expect(domain + " class C { string M(Mode m) => $\"{m:X4}\"; }",
            PrimitiveDomainAnalyzer.EnumNumericFormatId, "formatted as 'X4'");
        Expect(domain + " class C { string M(Mode? m) => $\"{m:D2}\"; }",
            PrimitiveDomainAnalyzer.EnumNumericFormatId, "formatted as 'D2'");
        Expect(domain + " class C { string M(Mode m) => $\"{m:X} {m:G} {(byte)m:X4}\"; }");
        // A switch expression that throws for every unnamed value is the boundary decoder itself.
        Expect("class C { char M(ushort word) { int tile = word & 0x3ff; return tile switch { 1 => 'a', 2 => 'b', _ => throw new System.IO.InvalidDataException() }; } }");
        // A statement switch that dispatches on a masked selector is still reported, even with a throwing default.
        Expect("class C { void M(byte header) { int direction = header & 3; switch (direction) { case 0: break; default: throw new System.Exception(); } } }",
            PrimitiveDomainAnalyzer.MaskedSelectorId, "by mask");

        // SME6279: a masked local compared against two distinct nonzero values.
        Expect("class C { bool M(byte header) { int kind = header & 7; return kind == 1 || kind == 4; } }",
            PrimitiveDomainAnalyzer.MaskedComparisonId, "by mask");
        Expect("class C { int M(ushort timer) { int direction = (timer & 6) >> 1; return direction == 1 ? 2 : direction != 3 ? 1 : 0; } }",
            PrimitiveDomainAnalyzer.MaskedComparisonId, "by shift");
        // A zero test, or parity and bound tests of a masked position, are arithmetic.
        Expect("class C { bool M(byte header) { int low = header & 3; return low == 0; } }");
        Expect("class C { int M(ushort phase) { int offset = phase & 0x1ff; return (offset & 1) == 0 ? 1 : offset == 0x1ff ? 2 : 3; } }");

        // SME6274: a primitive compared against several named constants of one catalog.
        Expect(catalog + " class C { bool M(byte stage) => stage == Stage.Read || stage == Stage.Build; }",
            PrimitiveDomainAnalyzer.CatalogComparisonId, "2 named constants of Stage");
        Expect(catalog + " class C { byte s; bool A() => s == Stage.Read; bool B() => s is Stage.Build; }",
            PrimitiveDomainAnalyzer.CatalogComparisonId, "named constants of Stage");
        // An ordered quantity, a stepped counter and a library member are not domains.
        Expect(catalog + " class C { bool M(byte distance) => distance == Stage.Read || distance == Stage.Build || distance <= 1; }");
        Expect(catalog + " class C { int M() { int n = 0; for (byte i = 0; i < 8; i++) { if (i == Stage.Read || i == Stage.Build) n++; } return n; } }");
        Expect(catalog + " class C { bool M(byte[] a, byte[] b) => a.Length == Stage.Read && b.Length == Stage.Build; }");
        // One named bound, or an open quantity compared with literals, is not a domain.
        Expect(catalog + " class C { bool M(byte stage) => stage == Stage.Read; }");
        Expect("class C { bool M(int count) => count == 0 || count == 1; }");

        // SME6275: a primitive parameter whose every use is an unchecked enum cast.
        Expect(domain + " class C { int M(byte raw) => (Mode)raw switch { Mode.A => 1, Mode.B => 2, Mode.C => 3, _ => throw new System.Exception() }; }",
            PrimitiveDomainAnalyzer.CastOnlyParameterId, "accept Mode");
        // A parameter that is also used numerically, or decoded through a validating helper, is a boundary.
        Expect(domain + " class C { int M(byte raw) => raw > 2 ? 0 : (int)(Mode)raw; }");
        Expect(domain + " static class D { public static Mode Decode(byte raw) => System.Enum.IsDefined((Mode)raw) ? (Mode)raw : throw new System.Exception(); }");

        // SME6276: arithmetic or stepping manufactures a closed-enum value.
        Expect(domain + " class C { Mode M(Mode m) => (Mode)((byte)m + 1); }",
            PrimitiveDomainAnalyzer.EnumArithmeticId, "undefined Mode");
        Expect(domain + " class C { void M(Mode m) { m++; } }",
            PrimitiveDomainAnalyzer.EnumArithmeticId, "Stepping an enum");
        // A [Flags] combination and a plain enum-to-number conversion are accepted.
        Expect(flags + " class C { Bits M(Bits b) => (Bits)((byte)b + 1); }");
        Expect(domain + " class C { int M(Mode m) => (int)m * 4; }");

        // SME6277: a domain value narrowed into a primitive and then used as the identity.
        Expect(domain + " class C { bool M(Mode m) { int code = (int)m; return code == 1; } }",
            PrimitiveDomainAnalyzer.PrimitiveInterludeId, "narrowed to int and is then compared");
        Expect(domain + " class C { int field; void Set(Mode m) => field = (int)m; int Get() => field switch { 0 => 1, _ => 2 }; }",
            PrimitiveDomainAnalyzer.PrimitiveInterludeId, "is then switched");
        // SME6278: storage written and compared only as a small literal set is a closed domain.
        Expect("class C { ushort direction; void Rise() => direction = 1; void Fall() => direction = 2; void Stop() => direction = 0; bool Up() => direction == 1; bool Down() => direction == 2; }",
            PrimitiveDomainAnalyzer.LiteralDomainId, "only as the values 0, 1, 2");
        // A stepped counter, an ordered quantity, or a single written value is not a literal domain.
        Expect("class C { int count; void Reset() => count = 0; void Add() => count++; bool Done() => count == 3 || count == 0; }");
        Expect("class C { int speed; void A() => speed = 2; void B() => speed = 5; bool Fast() => speed > 3 || speed == 2 || speed == 5; }");
        Expect("class C { int mode; void A() => mode = 4; bool M() => mode == 4 || mode == 0; }");
        // Multipurpose storage written from several sources (a native slot word) is a raw boundary.
        Expect(domain + " class C { ushort slot; void A(Mode m) => slot = (ushort)m; void B(ushort timer) => slot = timer; bool T() => slot == 3; }");
        // A conversion consumed immediately as an index or arithmetic is a boundary expression.
        Expect(domain + " class C { int[] table = new int[3]; int M(Mode m) { int index = (int)m; return table[index] + index; } }");

        // SME6284: the assembly declaring SnesButton (Core) converts it to or from raw words only inside a declared boundary.
        const string buttons = "namespace SuperMetroid.Core.Input { [System.Flags] public enum SnesButton : ushort { None = 0, A = 0x0080 } " +
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class ControllerWordBoundaryAttribute(string reason) : System.Attribute " +
            "{ public string Reason { get; } = reason; } } ";
        void ExpectCore(string source, string? id = null, string? message = null) =>
            Expect(buttons + source, id, message);
        ExpectCore("class C { ushort M(SuperMetroid.Core.Input.SnesButton b) => (ushort)b; }",
            PrimitiveDomainAnalyzer.ControllerWordConversionId, "converts controller buttons to a raw word");
        ExpectCore("class C { SuperMetroid.Core.Input.SnesButton M(ushort w) => (SuperMetroid.Core.Input.SnesButton)w; }",
            PrimitiveDomainAnalyzer.ControllerWordConversionId, "converts controller buttons from a raw word");
        ExpectCore("class C { [SuperMetroid.Core.Input.ControllerWordBoundary(\"codec\")] ushort M(SuperMetroid.Core.Input.SnesButton b) => (ushort)b; }");
        ExpectCore("[SuperMetroid.Core.Input.ControllerWordBoundary(\"codec\")] class C { ushort M(SuperMetroid.Core.Input.SnesButton b) => (ushort)b; }");
        ExpectCore("class C { SuperMetroid.Core.Input.SnesButton M() => 0; }");
        // Hosts, tools and tests use Core's SnesButton from metadata and convert at their own boundaries.
        Expect("class C { ushort M(SuperMetroid.Core.Input.SnesButton b) => (ushort)b; }");
    }

    /// <summary>Loads trusted platform assemblies plus Core so each isolated snippet has the same type boundary.</summary>
    private static MetadataReference[] BuildReferences()
    {
        string assemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("Trusted platform assemblies are unavailable.");
        return assemblies.Split(Path.PathSeparator)
            .Append(typeof(Ensure).Assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}
