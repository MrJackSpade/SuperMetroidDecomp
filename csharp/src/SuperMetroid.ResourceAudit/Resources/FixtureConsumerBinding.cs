using Microsoft.CodeAnalysis;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// The binding errors of a contract check's fixture consumer. A consumer call that does not bind is never audited, so a
/// consumer that no longer binds would otherwise pass or fail on counts with no visible cause. The reviewed sources are
/// compiled against Core's public surface and may carry errors of their own; only the consumer's calls must bind.
/// </summary>
internal static class FixtureConsumerBinding
{
    /// <summary>Fails with every error in <paramref name="consumer"/> when any of its calls does not bind.</summary>
    /// <exception cref="InvalidOperationException">The fixture consumer does not bind.</exception>
    internal static void Require(Compilation compilation, SyntaxTree consumer, string check)
    {
        string errors = string.Join(Environment.NewLine, compilation.GetSemanticModel(consumer).GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => diagnostic.ToString()));
        if (errors.Length > 0)
            throw new InvalidOperationException($"{check}: the fixture consumer must bind every call:{Environment.NewLine}{errors}");
    }
}
