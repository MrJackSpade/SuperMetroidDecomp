using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2569 magnitude-and-sign rule, SME6471.</summary>
internal static class MagnitudeAndSignAnalyzerTests
{
    /// <summary>The advice a chosen reversal ends with.</summary>
    private const string ReversalAdvice = "chooses a value or its reversal by a condition";

    /// <summary>The advice a chosen sum or difference ends with.</summary>
    private const string SumAdvice = "chooses between adding and subtracting the same change by a condition";

    /// <summary>The advice a sign enum ends with.</summary>
    private const string SignAdvice = "names a sign";

    /// <summary>A speed with its own negation, the attributes that exempt a type and a direction, as Core declares them.</summary>
    private const string Declarations = """
        namespace SuperMetroid.Core
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public sealed class EncodingOwnerAttribute(string format) : System.Attribute
            {
                public string Format { get; } = format;
            }
        }
        namespace SuperMetroid.Core.Compatibility
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute
            {
                public string Behaviour { get; } = behaviour;
            }
        }
        namespace SuperMetroid.Core.Spatial
        {
            public enum HorizontalDirection { Left, Right }
            [EncodingOwner("signed 8.8 fixed-point number")]
            public readonly record struct Speed(int Raw)
            {
                public bool IsNegative => Raw < 0;
                public Speed Negated() => new(-Raw);
                public Speed Magnitude() => IsNegative ? -this : this;
                public static Speed operator -(Speed speed) => new(-speed.Raw);
                public static Speed operator +(Speed left, Speed right) => new(left.Raw + right.Raw);
                public static Speed operator -(Speed left, Speed right) => new(left.Raw - right.Raw);
            }
        }
        """;

    /// <summary>One positive per way a magnitude and a sign are told, and the uses the rule leaves alone.</summary>
    internal static void VerifyMagnitudeAndSignAnalyzer()
    {
        // A value or its reversal chosen by a direction test, a bool or a sign test, either way round and in parentheses.
        Expect(Body("return direction == HorizontalDirection.Right ? speed : -speed;"),
            "'direction == HorizontalDirection.Right ? speed : -speed' " + ReversalAdvice);
        Expect(Body("return left ? -speed : speed;"), ReversalAdvice);
        Expect(Body("return speed.IsNegative ? (-change) : (change);"), ReversalAdvice);
        Expect(Body("return left ? speed.Negated() : speed;"), ReversalAdvice);
        Expect(Body("int pixels = left ? -4 : 4;\n        return speed;"), ReversalAdvice);
        // A sum or a difference of the same operands chosen by a condition.
        Expect(Body("return speed.IsNegative ? speed - change : speed + change;"), "'speed.IsNegative ? speed - change : speed + change' " + SumAdvice);
        // An enum of signs.
        Expect(Body("return speed;") + "\nnamespace SuperMetroid.Core.Enemies { internal enum SpeedSign { Positive, Negative } }", SignAdvice);

        // Different values, a direction chosen by a condition, a reversal without a condition, the encoding owner's own
        // arithmetic and a compatibility service's reproduction pass.
        Expect(Body("return left ? speed : change;"), count: 0);
        Expect(Body("HorizontalDirection side = left ? HorizontalDirection.Left : HorizontalDirection.Right;\n        return -speed;"), count: 0);
        Expect(Body("return left ? speed - change : speed + speed;"), count: 0);
        Expect(Body("return speed;") + """

            namespace SuperMetroid.Core.Compatibility
            {
                using SuperMetroid.Core.Spatial;
                [CompatibilityService("a reproduced unsigned product")]
                internal static class Reproduction
                {
                    internal static Speed Of(bool left, Speed speed) => left ? -speed : speed;
                }
            }
            """, count: 0);
        Expect(Body("return speed;") + "\nnamespace SuperMetroid.Core.Enemies { internal enum Side { Positive, Zero } }", count: 0);
        // Outside Core the rule does not apply.
        Expect(Body("return left ? -speed : speed;"), count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>A Core class whose method <c>Make</c> runs <paramref name="statements"/> over a speed, a change and a side.</summary>
    private static string Body(string statements) => Declarations + $$"""

        namespace SuperMetroid.Core.Enemies
        {
            using SuperMetroid.Core.Spatial;
            internal static class Fixture
            {
                internal static Speed Make(Speed speed, Speed change, bool left, HorizontalDirection direction)
                {
                    {{statements}}
                }
            }
        }
        """;

    /// <summary>Compiles one source and compares the SME6471 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new MagnitudeAndSignAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != MagnitudeAndSignAnalyzer.MagnitudeAndSignId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {MagnitudeAndSignAnalyzer.MagnitudeAndSignId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
