using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6412, which keeps rooms, room states and doors identified by their definitions (#1333).</summary>
internal static class RoomPointerAnalyzerTests
{
    /// <summary>Number-typed room, state and door pointers are reported; definitions and other pointers are not.</summary>
    internal static void VerifyRoomPointerAnalyzer()
    {
        Expect("class C { ushort roomPointer; }", "'roomPointer' carries a room, room-state or door pointer");
        Expect("class C { ushort? DoorListPointer { get; } }", "'DoorListPointer'");
        Expect("class C { int RoomStatePointer => 0; }", "'RoomStatePointer'");
        Expect("class C { void Load(ushort doorPointer) { } }", "'doorPointer'");
        Expect("class C { ushort DestinationRoomPointer() => 0; }", "'DestinationRoomPointer'");
        ExpectNone("sealed class RoomHeaderDefinition { } class C { RoomHeaderDefinition? Room { get; } void Load(RoomHeaderDefinition room) { } }");
        ExpectNone("enum RoomSetup : ushort { } class C { RoomSetup RoomSetupPointer { get; } }");
        ExpectNone("class C { ushort InstructionPointer; ushort SpritemapPointer { get; } }");
    }

    /// <summary>Compiles one file and checks that it reports no diagnostic.</summary>
    private static void ExpectNone(string source) => Check(source, null);

    /// <summary>Compiles one file and checks that it reports exactly one SME6412 containing <paramref name="message"/>.</summary>
    private static void Expect(string source, string message) => Check(source, message);

    /// <summary>Compiles one file and checks its analyzer diagnostics against <paramref name="expected"/>.</summary>
    private static void Check(string source, string? expected)
    {
        var compilation = CSharpCompilation.Create("RoomPointerFixture", [CSharpSyntaxTree.ParseText(source)], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid room-pointer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic[] diagnostics = compilation.WithAnalyzers([new RoomPointerAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
        bool matches = expected is { } message
            ? diagnostics.Length == 1 && diagnostics[0].Id == RoomPointerAnalyzer.RoomPointerId &&
              diagnostics[0].GetMessage().Contains(message, StringComparison.Ordinal)
            : diagnostics.Length == 0;
        if (!matches)
        {
            throw new InvalidOperationException($"Expected {(expected is { } m ? $"SME6412 containing '{m}'" : "no diagnostic")} for {source}; got " +
                string.Join("; ", diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
        }
    }
}
