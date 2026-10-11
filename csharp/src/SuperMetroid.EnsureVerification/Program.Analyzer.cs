using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.Core;
using SuperMetroid.EnsureAnalyzer;

namespace SuperMetroid.EnsureVerification;

internal static partial class Program
{
    private static readonly MetadataReference[] References = BuildReferences();

    private static void VerifyAnalyzer()
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

    private static void VerifySnippet(string source, string? expectedId = null, string? expectedMessage = null,
        DiagnosticAnalyzer? analyzer = null)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
        var compilation = CSharpCompilation.Create(
            "EnsureAnalyzerCase",
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
            diagnostics[0].Severity != DiagnosticSeverity.Warning ||
            (expectedMessage is not null && !diagnostics[0].GetMessage().Contains(expectedMessage, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Expected one {expectedId} warning containing '{expectedMessage}', got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in {source}");
        }
    }

    /// <summary>#627 primitive-domain rules: positive and negative cases for each diagnostic.</summary>
    private static void VerifyPrimitiveDomainAnalyzer()
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
    }

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
