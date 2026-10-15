using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports oversized parameter lists (#633): methods, constructors, delegates and local functions above
/// the repository ceiling, signatures that pass a cluster of nullable callbacks to toggle capabilities,
/// and parameters typed with a broad context when a narrower capability interface that the context
/// implements covers everything the operation reads. Overrides, interface implementations, extern
/// members, positional record shapes and members declaring <c>[ParameterListContract("reason")]</c>
/// for an immutable external contract are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterListAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A signature above the parameter ceiling.</summary>
    public const string OversizedId = "SME6330";

    /// <summary>A signature carrying a cluster of nullable callbacks.</summary>
    public const string CallbackClusterId = "SME6331";

    /// <summary>A parameter typed with a broad context whose narrower capability interface suffices.</summary>
    public const string BroadContextId = "SME6332";

    /// <summary>The repository's maximum parameter count.</summary>
    public const int MaximumParameters = 7;

    /// <summary>The number of nullable delegate parameters that forms a capability-toggle cluster.</summary>
    public const int CallbackClusterSize = 3;

    /// <summary>Exempts an immutable external signature.</summary>
    public const string ContractAttributeName = "ParameterListContractAttribute";

    /// <summary>Reports an operation whose argument count exceeds the repository ceiling.</summary>
    private static readonly DiagnosticDescriptor OversizedRule = new(
        OversizedId,
        "Oversized parameter list",
        "{0} takes {1} parameters (ceiling {2}); accept a cohesive typed operation context composed of its children's capability interfaces",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Long parameter lists hide capability boundaries and allow invalid combinations. Replace them with a named " +
            "operation interface that inherits the narrow interfaces its child operations accept, implemented by a validated type.");

    /// <summary>Reports a signature that uses nullable delegates as a cluster of optional capabilities.</summary>
    private static readonly DiagnosticDescriptor CallbackClusterRule = new(
        CallbackClusterId,
        "Nullable callback cluster",
        "{0} takes {1} nullable callbacks; replace them with a focused collaborator interface or an explicit optional capability",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Nullable delegates that switch behaviour on and off are an unstructured capability set; model the service " +
            "they represent as one interface, and make optional behaviour a named strategy or null object.");

    /// <summary>Reports a parameter whose used members fit an interface already implemented by its concrete context.</summary>
    private static readonly DiagnosticDescriptor BroadContextRule = new(
        BroadContextId,
        "Broad context parameter",
        "Parameter '{0}' of {1} only reads members of the narrower capability interface {2} that {3} implements; accept {2}",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A child operation must accept only its own capability contract. When the context passed to it is a " +
            "superset that implements a narrower interface covering every member the operation uses, accept that interface.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [OversizedRule, CallbackClusterRule, BroadContextRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSymbolAction(AnalyzeDelegate, SymbolKind.NamedType);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
        context.RegisterSyntaxNodeAction(AnalyzeBroadContexts, SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration, SyntaxKind.LocalFunctionStatement);
    }

    /// <summary>Finds source-defined context parameters whose body reads can be covered by a narrower source interface.</summary>
    private static void AnalyzeBroadContexts(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not IMethodSymbol method ||
            method.IsOverride || method.IsExtern || method.IsAbstract || method.ExplicitInterfaceImplementations.Length != 0 ||
            ImplementsInterface(method) || IsExempt(method))
            return;
        SyntaxNode? body = context.Node switch
        {
            BaseMethodDeclarationSyntax declaration => (SyntaxNode?)declaration.Body ?? declaration.ExpressionBody,
            LocalFunctionStatementSyntax local => (SyntaxNode?)local.Body ?? local.ExpressionBody,
            _ => null,
        };
        if (body is null)
            return;
        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (parameter.RefKind != RefKind.None || parameter.Type is not INamedTypeSymbol type ||
                !type.Locations.Any(location => location.IsInSource))
                continue;
            INamedTypeSymbol[] narrower = type.AllInterfaces
                .Where(contract => contract.Locations.Any(location => location.IsInSource) && contract.GetMembers().Length != 0)
                .ToArray();
            if (narrower.Length == 0 || UsedMembers(context.SemanticModel, body, parameter) is not { Count: > 0 } used)
                continue;
            INamedTypeSymbol? covering = narrower
                .Where(contract => used.All(member => Covers(type, contract, member)))
                .OrderBy(contract => contract.GetMembers().Length + contract.AllInterfaces.Sum(parent => parent.GetMembers().Length))
                .FirstOrDefault();
            if (covering is not null)
                context.ReportDiagnostic(Diagnostic.Create(BroadContextRule, parameter.Locations[0],
                    parameter.Name, Describe(method), covering.Name, type.Name));
        }
    }

    /// <summary>
    /// The members the body reads through <paramref name="parameter"/>, or null when the body also uses
    /// the parameter as a whole value (passing, returning, storing, comparing or pattern-matching it).
    /// </summary>
    private static HashSet<ISymbol>? UsedMembers(SemanticModel model, SyntaxNode body, IParameterSymbol parameter)
    {
        var used = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (IdentifierNameSyntax name in body.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (name.Identifier.Text != parameter.Name ||
                !SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(name).Symbol, parameter))
                continue;
            ISymbol? member = name.Parent switch
            {
                MemberAccessExpressionSyntax access when access.Expression == name => model.GetSymbolInfo(access).Symbol,
                ConditionalAccessExpressionSyntax conditional when conditional.Expression == name &&
                    conditional.WhenNotNull is MemberBindingExpressionSyntax binding => model.GetSymbolInfo(binding).Symbol,
                ConditionalAccessExpressionSyntax conditional when conditional.Expression == name &&
                    conditional.WhenNotNull is InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax binding } =>
                    model.GetSymbolInfo(binding).Symbol,
                _ => null,
            };
            if (member is not (IPropertySymbol or IMethodSymbol or IEventSymbol))
                return null;
            used.Add(member is IMethodSymbol { ReducedFrom: { } extension } ? extension : member);
        }
        return used;
    }

    /// <summary>Whether <paramref name="member"/>, read through <paramref name="type"/>, is a member of <paramref name="contract"/>.</summary>
    private static bool Covers(INamedTypeSymbol type, INamedTypeSymbol contract, ISymbol member)
    {
        if (member is IMethodSymbol { IsExtensionMethod: true })
            return false;
        IEnumerable<ISymbol> contractMembers = contract.GetMembers().Concat(contract.AllInterfaces.SelectMany(parent => parent.GetMembers()));
        return contractMembers.Any(candidate =>
            SymbolEqualityComparer.Default.Equals(candidate, member) ||
            SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(candidate), member));
    }

    /// <summary>Checks ordinary method and constructor signatures while excluding framework contracts and record shapes.</summary>
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

    /// <summary>Applies the argument and callback limits to declared delegate invocation signatures.</summary>
    private static void AnalyzeDelegate(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind != TypeKind.Delegate || type.DelegateInvokeMethod is not { } invoke || IsExempt(type) ||
            type.Locations.FirstOrDefault() is not { IsInSource: true } location)
            return;
        Report(context.ReportDiagnostic, "delegate " + type.Name, invoke.Parameters, location);
    }

    /// <summary>Checks local function arguments from their syntax because local functions are skipped by the method symbol action.</summary>
    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not IMethodSymbol function)
            return;
        Report(context.ReportDiagnostic, "local function " + function.Name, function.Parameters,
            ((LocalFunctionStatementSyntax)context.Node).Identifier.GetLocation());
    }

    /// <summary>Emits diagnostics for argument overflow and three-or-more nullable delegate parameters.</summary>
    private static void Report(Action<Diagnostic> report, string name, ImmutableArray<IParameterSymbol> parameters, Location location)
    {
        if (parameters.Length > MaximumParameters)
            report(Diagnostic.Create(OversizedRule, location, name, parameters.Length, MaximumParameters));
        int callbacks = parameters.Count(parameter =>
            parameter.Type.TypeKind == TypeKind.Delegate && parameter.NullableAnnotation == NullableAnnotation.Annotated);
        if (callbacks >= CallbackClusterSize)
            report(Diagnostic.Create(CallbackClusterRule, location, name, callbacks));
    }

    /// <summary>Formats a constructor or named operation for the diagnostic message.</summary>
    private static string Describe(IMethodSymbol method) => method.MethodKind == MethodKind.Constructor
        ? method.ContainingType.Name + " constructor"
        : method.ContainingType.Name + "." + method.Name;

    /// <summary>A positional record's primary constructor and deconstructor declare its data shape, not an operation.</summary>
    private static bool IsRecordShape(IMethodSymbol method) =>
        method.ContainingType.IsRecord &&
        (method.MethodKind == MethodKind.Constructor || method.Name == "Deconstruct") &&
        method.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is RecordDeclarationSyntax);

    /// <summary>Recognizes implicit implementations whose signatures are dictated by an interface contract.</summary>
    private static bool ImplementsInterface(IMethodSymbol method) =>
        method.ContainingType.AllInterfaces.SelectMany(contract => contract.GetMembers().OfType<IMethodSymbol>())
            .Any(member => SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(member), method));

    /// <summary>Recognizes the explicit exemption for immutable external signatures.</summary>
    private static bool IsExempt(ISymbol symbol) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == ContractAttributeName);
}
