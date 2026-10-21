using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1373: a bool parameter that selects between behaviours reads as <c>Foo(x, true, false)</c> at its call sites, lets
/// a meaningless combination of flags be written, and leaves no room for a third case. In SuperMetroid.Core a bool
/// parameter that only ever receives <c>true</c>/<c>false</c> literals is reported (SME6423), as is a method,
/// constructor or positional record taking two or more bools (SME6424) and a delegate-typed field, property or parameter
/// whose delegate takes a bool input (SME6425). A bool that is a genuine independent fact says so with
/// <c>[IndependentFlag(reason)]</c> at its parameter, member or record.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BoolFlagAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A bool parameter receives only literals.</summary>
    public const string LiteralOnlyId = "SME6423";

    /// <summary>A member takes two or more bools.</summary>
    public const string BoolGroupId = "SME6424";

    /// <summary>A delegate signature takes a bool input.</summary>
    public const string DelegateBoolId = "SME6425";

    /// <summary>The attribute that marks a bool as a genuine independent fact.</summary>
    public const string IndependentFlagAttributeName = "IndependentFlagAttribute";

    /// <summary>The assembly whose members are checked.</summary>
    public const string CheckedAssembly = "SuperMetroid.Core";

    /// <summary>Reports a bool parameter every caller passes a literal.</summary>
    private static readonly DiagnosticDescriptor LiteralOnlyRule = new(
        LiteralOnlyId,
        "Bool parameter selects behaviour",
        "Bool parameter '{0}' of '{1}' only ever receives the literals {2}; split the method in two, or take an enum of the cases",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A bool parameter of a SuperMetroid.Core method, constructor or delegate to which every argument in the " +
            "compilation is a true/false literal selects between behaviours at compile time (#1373).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <summary>Reports a member taking several bools.</summary>
    private static readonly DiagnosticDescriptor BoolGroupRule = new(
        BoolGroupId,
        "Bool group encodes a choice",
        "'{0}' takes {1} bool parameters ({2}); name the combinations that occur as an enum, or split the member",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A SuperMetroid.Core method, constructor or positional record with two or more bool parameters lets " +
            "meaningless combinations be written (#1373).");

    /// <summary>Reports a delegate signature with a bool input.</summary>
    private static readonly DiagnosticDescriptor DelegateBoolRule = new(
        DelegateBoolId,
        "Bool in a delegate signature",
        "'{0}' is a delegate taking a bool input; put the fact on the event's context as a named property, or split the delegate",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A SuperMetroid.Core field, property or parameter whose delegate type takes a bool input passes an " +
            "unnamed flag to every subscriber (#1373).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [LiteralOnlyRule, BoolGroupRule, DelegateBoolRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CheckedAssembly)
                return;
            var arguments = new ConcurrentBag<BoolArgument>();
            var escaped = new ConcurrentDictionary<IMethodSymbol, bool>(SymbolEqualityComparer.Default);
            start.RegisterOperationAction(operation => RecordArguments(operation, arguments),
                OperationKind.Invocation, OperationKind.ObjectCreation);
            start.RegisterOperationAction(reference =>
                escaped.TryAdd(((IMethodReferenceOperation)reference.Operation).Method.OriginalDefinition, true), OperationKind.MethodReference);
            start.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
            start.RegisterSymbolAction(AnalyzeMember, SymbolKind.Field, SymbolKind.Property);
            start.RegisterCompilationEndAction(end =>
            {
                foreach (IGrouping<ISymbol, BoolArgument> calls in
                         arguments.GroupBy<BoolArgument, ISymbol>(entry => entry.Parameter, SymbolEqualityComparer.Default))
                {
                    var parameter = (IParameterSymbol)calls.Key;
                    if (calls.Any(entry => entry.Literal is null) ||
                        parameter.ContainingSymbol is not IMethodSymbol method ||
                        escaped.ContainsKey(method) || !Checked(method) || Justified(parameter) || JustifiedMember(method))
                    {
                        continue;
                    }
                    string literals = string.Join(" and ", calls.Select(entry => entry.Literal == true ? "true" : "false")
                        .Distinct().OrderBy(text => text, StringComparer.Ordinal));
                    end.ReportDiagnostic(Diagnostic.Create(LiteralOnlyRule, parameter.Locations[0], parameter.Name,
                        Describe(method), literals));
                }
            });
        });
    }

    /// <summary>Records, for each bool argument of a call or creation, its literal value or null for data.</summary>
    private static void RecordArguments(OperationAnalysisContext context, ConcurrentBag<BoolArgument> arguments)
    {
        ImmutableArray<IArgumentOperation> passed = context.Operation switch
        {
            IInvocationOperation invocation => invocation.Arguments,
            IObjectCreationOperation creation => creation.Arguments,
            _ => [],
        };
        foreach (IArgumentOperation argument in passed)
        {
            if (argument.Parameter is not { Type.SpecialType: SpecialType.System_Boolean } parameter ||
                !parameter.ContainingSymbol.Locations.Any(location => location.IsInSource))
            {
                continue;
            }
            arguments.Add(new BoolArgument(parameter.OriginalDefinition, Literal(argument)));
        }
    }

    /// <summary>The literal an argument passes, counting an omitted optional argument as its literal default; null for data.</summary>
    private static bool? Literal(IArgumentOperation argument)
    {
        IOperation value = argument.Value;
        while (value is IConversionOperation conversion)
            value = conversion.Operand;
        return value is ILiteralOperation or IDefaultValueOperation && value.ConstantValue is { HasValue: true, Value: bool literal }
            ? literal
            : null;
    }

    /// <summary>Reports a bool group and every parameter whose delegate takes a bool input.</summary>
    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (!Checked(method) || method.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor))
            return;
        // A ref or out bool is a result the method writes, not a choice the caller makes.
        IParameterSymbol[] bools = [.. method.Parameters.Where(parameter =>
            parameter.Type.SpecialType == SpecialType.System_Boolean && parameter.RefKind is RefKind.None or RefKind.In)];
        bool record = IsPrimaryRecordConstructor(method);
        if (bools.Length >= 2 && !JustifiedMember(method))
        {
            Location location = record ? method.ContainingType.Locations[0] : method.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(BoolGroupRule, location, Describe(method), bools.Length,
                string.Join(", ", bools.Select(parameter => parameter.Name))));
        }
        foreach (IParameterSymbol parameter in method.Parameters)
        {
            if (TakesBool(parameter.Type) && !Justified(parameter) && !JustifiedMember(method))
                context.ReportDiagnostic(Diagnostic.Create(DelegateBoolRule, parameter.Locations[0], parameter.Name));
        }
    }

    /// <summary>Reports a field or property whose delegate takes a bool input.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol member = context.Symbol;
        ITypeSymbol? type = member switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null,
        };
        // A positional record's property is reported at its parameter.
        if (type is null || member.IsImplicitlyDeclared || member.IsOverride || !member.Locations.Any(location => location.IsInSource) ||
            member.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is ParameterSyntax) ||
            member is IPropertySymbol { ExplicitInterfaceImplementations.Length: > 0 } || Implements(member))
        {
            return;
        }
        if (TakesBool(type) && !Justified(member))
            context.ReportDiagnostic(Diagnostic.Create(DelegateBoolRule, member.Locations[0], member.Name));
    }

    /// <summary>Whether a type is a delegate with a bool parameter.</summary>
    private static bool TakesBool(ITypeSymbol type) =>
        type is INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke } &&
        invoke.Parameters.Any(parameter => parameter.Type.SpecialType == SpecialType.System_Boolean);

    /// <summary>
    /// Whether a method is a source declaration of its own: not compiler generated, not an override and not an interface
    /// implementation, whose bools belong to the declaration it fulfils.
    /// </summary>
    private static bool Checked(IMethodSymbol method) =>
        !method.IsImplicitlyDeclared && !method.IsOverride && method.ExplicitInterfaceImplementations.Length == 0 &&
        method.Locations.Any(location => location.IsInSource) && !Implements(method);

    /// <summary>Whether a member implicitly implements an interface member.</summary>
    private static bool Implements(ISymbol member) =>
        member.ContainingType is { } type && type.AllInterfaces.SelectMany(face => face.GetMembers())
            .Any(required => SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(required), member));

    /// <summary>Whether a method, or the positional record whose primary constructor it is, carries the justification.</summary>
    private static bool JustifiedMember(IMethodSymbol method) =>
        Justified(method) || (IsPrimaryRecordConstructor(method) && Justified(method.ContainingType));

    /// <summary>Whether a constructor is a positional record's primary constructor.</summary>
    private static bool IsPrimaryRecordConstructor(IMethodSymbol method) =>
        method.MethodKind == MethodKind.Constructor && method.ContainingType.IsRecord &&
        method.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is RecordDeclarationSyntax);

    /// <summary>Whether a symbol carries <c>[IndependentFlag(reason)]</c>.</summary>
    private static bool Justified(ISymbol symbol) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == IndependentFlagAttributeName);

    /// <summary>The member's name as a reader finds it: the type name for a constructor.</summary>
    private static string Describe(IMethodSymbol method) =>
        method.MethodKind == MethodKind.Constructor ? method.ContainingType.Name : $"{method.ContainingType.Name}.{method.Name}";
}
