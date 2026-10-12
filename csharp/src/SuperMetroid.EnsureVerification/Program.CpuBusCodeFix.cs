using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer;
using SuperMetroid.EnsureAnalyzer.CodeFixes;
using static SuperMetroid.EnsureVerification.AnalyzerTests;

namespace SuperMetroid.EnsureVerification;

internal static class CpuBusCodeFixTests
{
    /// <summary>The SME6290 code fix rewrites only unambiguous raw reads, to the declared raw-equivalent view member (#629).</summary>
    internal static void VerifyCpuBusAccessCodeFix()
    {
        const string api =
            "using System; " +
            "[AttributeUsage(AttributeTargets.All)] sealed class CpuBusAccessAttribute : Attribute { } " +
            "[AttributeUsage(AttributeTargets.All)] sealed class CpuBusBoundaryAttribute(string reason) : Attribute { } " +
            "[AttributeUsage(AttributeTargets.Property)] sealed class CpuBusViewOfAttribute(Type catalog, string cell) : Attribute { } " +
            "[CpuBusAccess] interface ISnesMemoryBus { byte ReadWorkRamByte(int address); byte ReadSaveRamByte(int address); } " +
            "[CpuBusAccess] static class SnesWordReads { public static ushort ReadWorkRamWordFixedBank(this ISnesMemoryBus bus, int address) => 0; } " +
            "[CpuBusAccess] sealed record CpuBusCell(int Address) { " +
            "public static CpuBusCell WramWord(string name, int address, string encoding = \"\") => new(address); " +
            "public static CpuBusCell WramByte(string name, int address, string encoding = \"\") => new(address); } " +
            "[CpuBusAccess] readonly struct WramWord(ISnesMemoryBus bus, CpuBusCell cell) { public ushort Read() => 0; } " +
            "[CpuBusBoundary(\"catalog\")] static class SamusWorkRam { " +
            "public static readonly CpuBusCell Health = CpuBusCell.WramWord(nameof(Health), 0x7e09c2); " +
            "public static readonly CpuBusCell Pose = CpuBusCell.WramWord(nameof(Pose), 0x7e0a1c); " +
            "public static readonly CpuBusCell Flags = CpuBusCell.WramByte(nameof(Flags), 0x7e0a1e); } " +
            "[CpuBusBoundary(\"domain view\")] readonly struct SamusMemory(ISnesMemoryBus bus) { " +
            "[CpuBusViewOf(typeof(SamusWorkRam), nameof(SamusWorkRam.Health))] public ushort Health => new WramWord(bus, SamusWorkRam.Health).Read(); " +
            "public ushort Pose => new WramWord(bus, SamusWorkRam.Pose).Read(); } ";

        // A constant-address word read and a primitive read over a named cell map to the declared member.
        ExpectFix(api + "class Game { ushort M(ISnesMemoryBus bus) => bus.ReadWorkRamWordFixedBank(0x7e09c2); }",
            "class Game { ushort M(ISnesMemoryBus bus) => new SamusMemory(bus).Health; }");
        ExpectFix(api + "class Game { ushort M(ISnesMemoryBus bus) => new WramWord(bus, SamusWorkRam.Health).Read(); }",
            "class Game { ushort M(ISnesMemoryBus bus) => new SamusMemory(bus).Health; }");
        // No declared raw-equivalent member, a width mismatch, a non-constant address, or an unknown address: no fix.
        ExpectFix(api + "class Game { ushort M(ISnesMemoryBus bus) => bus.ReadWorkRamWordFixedBank(0x7e0a1c); }", null);
        ExpectFix(api + "class Game { byte M(ISnesMemoryBus bus) => bus.ReadWorkRamByte(0x7e09c2); }", null);
        ExpectFix(api + "class Game { ushort M(ISnesMemoryBus bus, int address) => bus.ReadWorkRamWordFixedBank(address); }", null);
        ExpectFix(api + "class Game { ushort M(ISnesMemoryBus bus) => bus.ReadWorkRamWordFixedBank(0x7e0000); }", null);
    }

    /// <summary>Applies the single offered fix for the first SME6290 in <paramref name="source"/> and requires
    /// the rewritten <c>Game</c> class to equal <paramref name="expectedGame"/>, or requires no fix when null.</summary>
    private static void ExpectFix(string source, string? expectedGame)
    {
        using var workspace = new AdhocWorkspace();
        Project project = workspace.AddProject("CpuBusFixCase", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Preview))
            .WithMetadataReferences(References);
        Document document = project.AddDocument("Case.cs", SourceText.From(source));
        Compilation compilation = document.Project.GetCompilationAsync().GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Code-fix fixture produced no compilation.");
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid code-fix fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic diagnostic = compilation.WithAnalyzers([new CpuBusAccessAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult()
            .Where(d => d.Id == CpuBusAccessAnalyzer.RawAccessId && d.Location.SourceSpan.Start > source.IndexOf("class Game", StringComparison.Ordinal))
            .OrderBy(d => d.Location.SourceSpan.Start)
            .FirstOrDefault() ?? throw new InvalidOperationException($"Code-fix fixture has no SME6290 in Game: {source}");

        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);
        new CpuBusAccessCodeFixProvider().RegisterCodeFixesAsync(context).GetAwaiter().GetResult();
        if (expectedGame is null)
        {
            if (actions.Count != 0)
                throw new InvalidOperationException($"Unexpected SME6290 fix '{actions[0].Title}' for: {source}");
            return;
        }
        if (actions.Count != 1)
            throw new InvalidOperationException($"Expected one SME6290 fix, got {actions.Count} for: {source}");
        ImmutableArray<CodeActionOperation> operations = actions[0].GetOperationsAsync(CancellationToken.None).GetAwaiter().GetResult();
        Solution changed = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        string text = changed.GetDocument(document.Id)!.GetTextAsync().GetAwaiter().GetResult().ToString();
        string game = text[text.IndexOf("class Game", StringComparison.Ordinal)..];
        if (game != expectedGame)
            throw new InvalidOperationException($"SME6290 fix produced '{game}', expected '{expectedGame}'.");
    }
}
