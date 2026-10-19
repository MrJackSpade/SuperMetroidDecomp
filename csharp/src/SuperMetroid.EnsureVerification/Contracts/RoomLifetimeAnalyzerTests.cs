using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6409-SME6411, which keep room state inside the room scope (#1329).</summary>
internal static class RoomLifetimeAnalyzerTests
{
    /// <summary>The fixture markers and a room-lifetime type, as Core declares them.</summary>
    private const string Markers = """
        [System.AttributeUsage(System.AttributeTargets.Class)] sealed class RoomLifetimeAttribute : System.Attribute { }
        [System.AttributeUsage(System.AttributeTargets.Class)] sealed class RoomScopeBoundaryAttribute(string reason) : System.Attribute { public string Reason { get; } = reason; }
        [RoomLifetime] sealed class Pool { }
        """;

    /// <summary>Construction, storage and reset names are reported only outside the room scope.</summary>
    internal static void VerifyRoomLifetimeAnalyzer()
    {
        Expect("class Session { object Make() => new Pool(); }", RoomLifetimeAnalyzer.ConstructionId, "'Pool' is room-lifetime state");
        ExpectNone("[RoomScopeBoundary(\"builds rooms\")] class Loader { object Make() => new Pool(); }");
        ExpectNone("[RoomLifetime] class Room { Pool Part { get; } = new Pool(); }");
        Expect("class Session { Pool? cached; }", RoomLifetimeAnalyzer.StoredRoomStateId, "'cached' stores room-lifetime 'Pool'");
        Expect("class Session { Pool? Cached { get; set; } }", RoomLifetimeAnalyzer.StoredRoomStateId, "'Cached' stores room-lifetime 'Pool'");
        Expect("class Session { System.Collections.Generic.List<Pool> pools = []; }", RoomLifetimeAnalyzer.StoredRoomStateId, "'pools' stores room-lifetime 'Pool'");
        Expect("class Session { Pool[] pools = []; }", RoomLifetimeAnalyzer.StoredRoomStateId, "'pools' stores room-lifetime 'Pool'");
        ExpectNone("class Session { void Use(Pool pool) { } Pool Read(Pool pool) => pool; }");
        ExpectNone("[RoomScopeBoundary(\"holds the loaded room\")] class Holder { Pool? Current { get; set; } }");
        Expect("class Session { void ResetForRoomLoad() { } }", RoomLifetimeAnalyzer.RoomResetId, "'ResetForRoomLoad' is named as a room reset");
        Expect("class Session { void ResetCrocomireRoomState() { } }", RoomLifetimeAnalyzer.RoomResetId, "'ResetCrocomireRoomState'");
        Expect("class Session { void ResetForRoomTransition() { } }", RoomLifetimeAnalyzer.RoomResetId, "'ResetForRoomTransition'");
        ExpectNone("class Session { void Reset() { } void ResetStatus() { } }");
    }

    /// <summary>Compiles one file with the markers and checks that it reports no diagnostic.</summary>
    private static void ExpectNone(string source) => Check(source, null);

    /// <summary>Compiles one file with the markers and checks that it reports exactly one diagnostic with <paramref name="id"/> containing <paramref name="message"/>.</summary>
    private static void Expect(string source, string id, string message) => Check(source, (id, message));

    /// <summary>Compiles one file with the markers and checks its analyzer diagnostics against <paramref name="expected"/>.</summary>
    private static void Check(string source, (string Id, string Message)? expected)
    {
        var compilation = CSharpCompilation.Create("RoomLifetimeFixture",
            [CSharpSyntaxTree.ParseText(Markers), CSharpSyntaxTree.ParseText(source)], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid room-lifetime fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic[] diagnostics = compilation.WithAnalyzers([new RoomLifetimeAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
        bool matches = expected is { } diagnostic
            ? diagnostics.Length == 1 && diagnostics[0].Id == diagnostic.Id &&
              diagnostics[0].GetMessage().Contains(diagnostic.Message, StringComparison.Ordinal)
            : diagnostics.Length == 0;
        if (!matches)
        {
            throw new InvalidOperationException($"Expected {(expected is { } d ? $"{d.Id} containing '{d.Message}'" : "no diagnostic")} for {source}; got " +
                string.Join("; ", diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
        }
    }
}
