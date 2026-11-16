using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1325 enemy-slot word rule, SME6408.</summary>
internal static class EnemySlotWordAnalyzerTests
{
    /// <summary>Positive and negative cases for SME6408.</summary>
    internal static void VerifyEnemySlotWordAnalyzer()
    {
        Expect("public int Parameter1 { get; set; }", "'Parameter1' is a multipurpose word");
        Expect("public int VariableC { get; set; }", "'VariableC' is a multipurpose word");
        Expect("internal ushort Parameter2;", "'Parameter2' is a multipurpose word");
        Expect("public int Health { get; set; }");
        Expect("public int ParameterCount { get; set; }");
        // A family's own state may name its fields freely; only the common slot is guarded.
        Expect("public int Health { get; set; }", other: "public int VariableA { get; set; }");
        // Other assemblies are not the simulation's slot.
        Expect("public int Parameter1 { get; set; }", assembly: "SuperMetroid.Verification");
    }

    /// <summary>Compiles a slot with <paramref name="slotMember"/> and a family state with <paramref name="other"/>, and checks the result.</summary>
    private static void Expect(string slotMember, string? message = null, string other = "", string assembly = CoreLayerAnalyzer.CoreAssembly)
    {
        string source = $$"""
            namespace SuperMetroid.Core.Enemies
            {
                public sealed class RoomEnemySlot { {{slotMember}} }
                public sealed class FamilyEnemyState { {{other}} }
            }
            """;
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/RoomEnemySlot.cs");
        var compilation = CSharpCompilation.Create(assembly, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid enemy-slot fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic[] diagnostics = compilation.WithAnalyzers([new EnemySlotWordAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
        string context = $"{assembly}: slot '{slotMember}', family '{other}'";
        if (message is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected {diagnostics[0].Id}: {diagnostics[0].GetMessage()} for {context}");
            return;
        }
        if (diagnostics.Length != 1 || diagnostics[0].Id != EnemySlotWordAnalyzer.Id ||
            !diagnostics[0].GetMessage().Contains(message, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Expected one {EnemySlotWordAnalyzer.Id} containing '{message}' for {context}; got " +
                string.Join("; ", diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
        }
    }
}
