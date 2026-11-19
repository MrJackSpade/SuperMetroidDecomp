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

    /// <summary>The calls that name a field, by the parameter that takes its name.</summary>
    private static readonly ImmutableHashSet<string> FieldCalls = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Field", "SetField", "ValueField", "SetValueField", "StaticField", "SetStaticField");

    /// <summary>The calls that name a property.</summary>
    private static readonly ImmutableHashSet<string> PropertyCalls = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Get", "Set", "ValueGet", "ValueSet", "With", "StaticGet", "StaticSet", "GetterDelegate");

    /// <summary>The calls that name a method.</summary>
    private static readonly ImmutableHashSet<string> MethodCalls = ImmutableHashSet.Create(StringComparer.Ordinal,
        "Call", "Invoke", "InvokeValue", "InvokeStatic", "InvokeGeneric", "InvokeStaticGeneric", "MethodDelegate");

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

    /// <summary>Reports a private-state call whose owner lacks the member it names.</summary>
    private static void Analyze(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (method.ContainingType is not { Name: "PrivateState", ContainingType: null } state ||
            state.ContainingNamespace?.ToDisplayString() != "SuperMetroid.Core")
            return;
        string kind = FieldCalls.Contains(method.Name) ? "field"
            : PropertyCalls.Contains(method.Name) ? "property"
            : MethodCalls.Contains(method.Name) ? "method"
            : "";
        if (kind.Length == 0 || Owner(invocation) is not { TypeKind: not TypeKind.Error } owner ||
            invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name is "field" or "property" or "method") is not { } named ||
            named.Value.ConstantValue is not { HasValue: true, Value: string name } ||
            Has(owner, kind, name))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, named.Value.Syntax.GetLocation(), invocation.Syntax.ToString(), kind, name,
            owner.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
    }

    /// <summary>The owner a call states: the <c>typeof</c> its <c>owner</c> argument passes, or its first type argument.</summary>
    private static INamedTypeSymbol? Owner(IInvocationOperation invocation)
    {
        IArgumentOperation? owner = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "owner");
        if (owner?.Parameter?.Type is { Name: "Type", ContainingNamespace.Name: "System" })
            return owner.Value is ITypeOfOperation { TypeOperand: INamedTypeSymbol stated } ? stated : null;
        return invocation.TargetMethod.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
    }

    /// <summary>
    /// Whether the owner has a member of the kind and name, as reflection over its declared and inherited members finds
    /// it: any member the owner declares, or a non-private instance member of a base type.
    /// </summary>
    private static bool Has(INamedTypeSymbol owner, string kind, string name)
    {
        for (INamedTypeSymbol? type = owner; type is not null; type = type.BaseType)
        {
            bool own = SymbolEqualityComparer.Default.Equals(type, owner);
            foreach (ISymbol member in type.GetMembers(name))
            {
                if (!own && (member.IsStatic || member.DeclaredAccessibility == Accessibility.Private))
                    continue;
                bool matches = kind switch
                {
                    "field" => member is IFieldSymbol { IsImplicitlyDeclared: false },
                    "property" => member is IPropertySymbol { IsIndexer: false },
                    _ => member is IMethodSymbol { MethodKind: MethodKind.Ordinary },
                };
                if (matches)
                    return true;
            }
        }
        return false;
    }
}
