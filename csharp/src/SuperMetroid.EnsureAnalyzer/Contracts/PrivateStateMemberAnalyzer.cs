using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A check's private-state access names a member its owner has (SME6477, #2570): a call of test-support's
/// <c>SuperMetroid.Core.PrivateState</c> that names a field, property or method by a constant string, on an owner the
/// call states (its <c>TOwner</c> type argument, or the <c>typeof</c> it passes), names a member of that kind the owner
/// declares, or a non-private instance member one of its base types declares, as the reflection behind the call finds
/// it. A rename that leaves the string behind fails here, at build time, rather than when the check runs.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrivateStateMemberAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A private-state call names a member its owner does not have.</summary>
    public const string PrivateStateMemberId = "SME6477";

    private static readonly DiagnosticDescriptor Rule = new(
        PrivateStateMemberId,
        "Private-state access names a missing member",
        "'{0}' names the {1} '{2}', which {3} does not have; name a member it declares, or reach the state another way",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "PrivateState reaches a member by its name through reflection, so the compiler does not see the name: " +
            "a member renamed or removed leaves the string behind, and the check fails only when it runs. Every call " +
            "whose owner and name are known names a member that owner has (#2570).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Analyze, OperationKind.Invocation);
    }

    /// <summary>
    /// Reports a private-state call whose owner lacks the member it names. A build compiles a check against the product
    /// assemblies' metadata, which holds none of their private members, so the rule judges only an owner whose private
    /// members the compilation sees: one declared in its own source, or any owner when it imports every member. The
    /// architecture audit, which binds checks against the product's source, judges every call (#2570).
    /// </summary>
    private static void Analyze(OperationAnalysisContext context)
    {
        if (PrivateStateMembers.MissingMember((IInvocationOperation)context.Operation) is not { } missing ||
            !missing.Owner.Locations.Any(location => location.IsInSource) &&
            context.Compilation.Options.MetadataImportOptions != MetadataImportOptions.All)
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, missing.Site.GetLocation(), missing.Call, missing.Kind, missing.Name,
            missing.Owner.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
    }
}
