using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// Reports oversized parameter lists (#633): methods, constructors, delegates and local functions above
/// the repository ceiling, and signatures that pass a cluster of nullable callbacks to toggle
/// capabilities. Overrides, interface implementations, extern members, positional record shapes and
/// members declaring <c>[ParameterListContract("reason")]</c> for an immutable external contract are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterListAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A signature above the parameter ceiling.</summary>
    public const string OversizedId = "SME6330";

    /// <summary>A signature carrying a cluster of nullable callbacks.</summary>
    public const string CallbackClusterId = "SME6331";

    /// <summary>The repository's maximum parameter count.</summary>
    public const int MaximumParameters = 7;

    /// <summary>The number of nullable delegate parameters that forms a capability-toggle cluster.</summary>
    public const int CallbackClusterSize = 3;

    /// <summary>Exempts an immutable external signature.</summary>
    public const string ContractAttributeName = "ParameterListContractAttribute";

    private static readonly DiagnosticDescriptor OversizedRule = new(
        OversizedId,
        "Oversized parameter list",
        "{0} takes {1} parameters (ceiling {2}); accept a cohesive typed operation context composed of its children's capability interfaces",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Long parameter lists hide capability boundaries and allow invalid combinations. Replace them with a named " +
            "operation interface that inherits the narrow interfaces its child operations accept, implemented by a validated type.");

    private static readonly DiagnosticDescriptor CallbackClusterRule = new(
        CallbackClusterId,
        "Nullable callback cluster",
        "{0} takes {1} nullable callbacks; replace them with a focused collaborator interface or an explicit optional capability",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Nullable delegates that switch behaviour on and off are an unstructured capability set; model the service " +
            "they represent as one interface, and make optional behaviour a named strategy or null object.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [OversizedRule, CallbackClusterRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSymbolAction(AnalyzeDelegate, SymbolKind.NamedType);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (method.IsImplicitlyDeclared || method.IsOverride || method.IsExtern ||
            method.ExplicitInterfaceImplementations.Length != 0 || ImplementsInterface(method) ||
            method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or
                MethodKind.EventRemove or MethodKind.DelegateInvoke or MethodKind.LocalFunction ||
            IsRecordShape(method) || IsExempt(method) || method.Locations.FirstOrDefault() is not { IsInSource: true } location)
            return;
        Report(context.ReportDiagnostic, Describe(method), method.Parameters, location);
    }

    private static void AnalyzeDelegate(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind != TypeKind.Delegate || type.DelegateInvokeMethod is not { } invoke || IsExempt(type) ||
            type.Locations.FirstOrDefault() is not { IsInSource: true } location)
            return;
        Report(context.ReportDiagnostic, "delegate " + type.Name, invoke.Parameters, location);
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not IMethodSymbol function)
            return;
        Report(context.ReportDiagnostic, "local function " + function.Name, function.Parameters,
            ((LocalFunctionStatementSyntax)context.Node).Identifier.GetLocation());
    }

    private static void Report(Action<Diagnostic> report, string name, ImmutableArray<IParameterSymbol> parameters, Location location)
    {
        if (parameters.Length > MaximumParameters)
            report(Diagnostic.Create(OversizedRule, location, name, parameters.Length, MaximumParameters));
        int callbacks = parameters.Count(parameter =>
            parameter.Type.TypeKind == TypeKind.Delegate && parameter.NullableAnnotation == NullableAnnotation.Annotated);
        if (callbacks >= CallbackClusterSize)
            report(Diagnostic.Create(CallbackClusterRule, location, name, callbacks));
    }

    private static string Describe(IMethodSymbol method) => method.MethodKind == MethodKind.Constructor
        ? method.ContainingType.Name + " constructor"
        : method.ContainingType.Name + "." + method.Name;

    /// <summary>A positional record's primary constructor and deconstructor declare its data shape, not an operation.</summary>
    private static bool IsRecordShape(IMethodSymbol method) =>
        method.ContainingType.IsRecord &&
        (method.MethodKind == MethodKind.Constructor || method.Name == "Deconstruct") &&
        method.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is RecordDeclarationSyntax);

    private static bool ImplementsInterface(IMethodSymbol method) =>
        method.ContainingType.AllInterfaces.SelectMany(contract => contract.GetMembers().OfType<IMethodSymbol>())
            .Any(member => SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(member), method));

    private static bool IsExempt(ISymbol symbol) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == ContractAttributeName);
}
