using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1375: a concept defined twice can disagree with itself. In SuperMetroid.Core this reports an enum whose members are
/// assigned from another type's constants, and a static class of integral constants mirroring one enum's names and values
/// (SME6428); a switch over an enum whose every arm maps a member to the member declared after it, restating the
/// declaration order (SME6429); and a member signature that carries a tuple with three or more named elements, or a tuple
/// whose names and types match a type Core already declares (SME6430).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DuplicateConceptAnalyzer : DiagnosticAnalyzer
{
    /// <summary>An enum and a constants class define the same values.</summary>
    public const string MirroredConstantsId = "SME6428";

    /// <summary>A switch restates an enum's declaration order.</summary>
    public const string SuccessorSwitchId = "SME6429";

    /// <summary>A tuple stands in for a domain type.</summary>
    public const string TupleDomainTypeId = "SME6430";

    /// <summary>The assembly whose declarations are checked.</summary>
    public const string CheckedAssembly = "SuperMetroid.Core";

    /// <summary>Reports an enum and a constants class that mirror each other.</summary>
    private static readonly DiagnosticDescriptor MirroredConstantsRule = new(
        MirroredConstantsId,
        "Enum mirrored by constants",
        "'{0}' and '{1}' define the same values twice; keep the enum and convert native values at the codec",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An enum whose members are assigned from another type's constants, or a static class of integral constants " +
            "whose names and values all appear in one enum, defines one concept twice (#1375).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <summary>Reports a switch that maps each enum member to the next one.</summary>
    private static readonly DiagnosticDescriptor SuccessorSwitchRule = new(
        SuccessorSwitchId,
        "Switch restates declaration order",
        "This switch over '{0}' maps every member to the member declared after it; define the order once and step through it",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A switch whose every arm maps an enum member to the next member in declaration order is a second copy of " +
            "that order (#1375).");

    /// <summary>Reports a tuple standing in for a declared or missing domain type.</summary>
    private static readonly DiagnosticDescriptor TupleDomainTypeRule = new(
        TupleDomainTypeId,
        "Tuple stands in for a domain type",
        "'{0}' carries the tuple {1}; {2}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A tuple in a member signature with three or more named elements, or with the element names and types of a " +
            "type Core declares, is a domain value without its type (#1375).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [MirroredConstantsRule, SuccessorSwitchRule, TupleDomainTypeRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CheckedAssembly)
                return;
            var enums = new ConcurrentBag<INamedTypeSymbol>();
            var constantClasses = new ConcurrentBag<INamedTypeSymbol>();
            var mirrors = new ConcurrentDictionary<INamedTypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var coreTypes = new Lazy<INamedTypeSymbol[]>(() => Collect(start.Compilation));
            start.RegisterSyntaxNodeAction(node =>
            {
                var declaration = (EnumDeclarationSyntax)node.Node;
                if (node.SemanticModel.GetDeclaredSymbol(declaration) is { } type && AssignedFrom(declaration, node.SemanticModel, type) is { } source)
                    mirrors.TryAdd(type, source);
            }, SyntaxKind.EnumDeclaration);
            start.RegisterSymbolAction(symbol =>
            {
                var type = (INamedTypeSymbol)symbol.Symbol;
                if (type.TypeKind == TypeKind.Enum)
                    enums.Add(type);
                else if (type.IsStatic && IntegralConstants(type).Length >= 2)
                    constantClasses.Add(type);
            }, SymbolKind.NamedType);
            start.RegisterSymbolAction(symbol => AnalyzeMember(symbol, coreTypes.Value), SymbolKind.Field, SymbolKind.Property, SymbolKind.Method);
            start.RegisterOperationAction(AnalyzeSwitchExpression, OperationKind.SwitchExpression);
            start.RegisterOperationAction(AnalyzeSwitchStatement, OperationKind.Switch);
            start.RegisterCompilationEndAction(end => ReportMirrors(end, mirrors, [.. enums], [.. constantClasses]));
        });
    }

    /// <summary>Reports enums assigned from other types' constants and constant classes that mirror an enum.</summary>
    private static void ReportMirrors(CompilationAnalysisContext context, ConcurrentDictionary<INamedTypeSymbol, INamedTypeSymbol> mirrors,
        INamedTypeSymbol[] enums, INamedTypeSymbol[] constantClasses)
    {
        // A pair is reported once: at the enum when it reads the constants, otherwise at the constants class.
        var sources = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (KeyValuePair<INamedTypeSymbol, INamedTypeSymbol> mirror in mirrors)
        {
            sources.Add(mirror.Value);
            context.ReportDiagnostic(Diagnostic.Create(MirroredConstantsRule, mirror.Key.Locations[0], mirror.Key.Name, mirror.Value.Name));
        }
        foreach (INamedTypeSymbol type in constantClasses.Where(type => !sources.Contains(type)))
        {
            IFieldSymbol[] constants = IntegralConstants(type);
            INamedTypeSymbol? mirror = enums.FirstOrDefault(candidate => constants.All(constant => candidate.GetMembers(constant.Name)
                .OfType<IFieldSymbol>().Any(member => member.HasConstantValue &&
                    Convert.ToInt64(member.ConstantValue) == Convert.ToInt64(constant.ConstantValue))));
            if (mirror is not null)
                context.ReportDiagnostic(Diagnostic.Create(MirroredConstantsRule, type.Locations[0], mirror.Name, type.Name));
        }
    }

    /// <summary>
    /// The type an enum mirrors one-for-one: every member, and at least two, is assigned exactly one constant of that same
    /// other type. An enum that offsets or combines a constant derives from it rather than restating it.
    /// </summary>
    private static INamedTypeSymbol? AssignedFrom(EnumDeclarationSyntax declaration, SemanticModel model, INamedTypeSymbol type)
    {
        INamedTypeSymbol? source = null;
        int members = 0;
        foreach (EnumMemberDeclarationSyntax member in declaration.Members)
        {
            ExpressionSyntax? value = member.EqualsValue?.Value;
            while (value is ParenthesizedExpressionSyntax or CastExpressionSyntax)
                value = value is ParenthesizedExpressionSyntax parenthesized ? parenthesized.Expression : ((CastExpressionSyntax)value).Expression;
            if (value is null || model.GetSymbolInfo(value).Symbol is not IFieldSymbol { IsConst: true } constant ||
                SymbolEqualityComparer.Default.Equals(constant.ContainingType, type) ||
                (source is not null && !SymbolEqualityComparer.Default.Equals(source, constant.ContainingType)))
            {
                return null;
            }
            source = constant.ContainingType;
            members++;
        }
        return members >= 2 ? source : null;
    }

    /// <summary>A type's declared integral constants.</summary>
    private static IFieldSymbol[] IntegralConstants(INamedTypeSymbol type) =>
        [.. type.GetMembers().OfType<IFieldSymbol>().Where(field => field.IsConst && !field.IsImplicitlyDeclared &&
            field.Type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or
                SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or
                SpecialType.System_UInt64)];

    /// <summary>Reports a switch expression whose every arm maps a member to its successor.</summary>
    private static void AnalyzeSwitchExpression(OperationAnalysisContext context)
    {
        var switchExpression = (ISwitchExpressionOperation)context.Operation;
        var pairs = new List<(IFieldSymbol From, IOperation To)>();
        foreach (ISwitchExpressionArmOperation arm in switchExpression.Arms)
        {
            if (arm.Pattern is IDiscardPatternOperation || arm.Value is IThrowOperation || Stripped(arm.Value) is IThrowOperation)
                continue;
            if (arm.Guard is not null || Member(arm.Pattern) is not { } from)
                return;
            pairs.Add((from, arm.Value));
        }
        Report(context, switchExpression.Syntax, pairs);
    }

    /// <summary>Reports a switch statement whose every section returns the successor of its one case.</summary>
    private static void AnalyzeSwitchStatement(OperationAnalysisContext context)
    {
        var switchStatement = (ISwitchOperation)context.Operation;
        var pairs = new List<(IFieldSymbol From, IOperation To)>();
        foreach (ISwitchCaseOperation section in switchStatement.Cases)
        {
            if (section.Body.Length == 1 && section.Body[0] is IThrowOperation or IExpressionStatementOperation { Operation: IThrowOperation })
                continue;
            if (section.Clauses.Length != 1 || section.Clauses[0] is not ISingleValueCaseClauseOperation clause ||
                Stripped(clause.Value) is not IFieldReferenceOperation { Field: { } from } ||
                section.Body.Length != 1 || section.Body[0] is not IReturnOperation { ReturnedValue: { } returned })
            {
                return;
            }
            pairs.Add((from, returned));
        }
        Report(context, switchStatement.Syntax, pairs);
    }

    /// <summary>Reports when two or more arms all map an enum member to the next declared member.</summary>
    private static void Report(OperationAnalysisContext context, SyntaxNode syntax, List<(IFieldSymbol From, IOperation To)> pairs)
    {
        if (pairs.Count < 2)
            return;
        INamedTypeSymbol? type = pairs[0].From.ContainingType;
        if (type is not { TypeKind: TypeKind.Enum })
            return;
        IFieldSymbol[] order = [.. type.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue)];
        foreach ((IFieldSymbol from, IOperation to) in pairs)
        {
            int index = Array.FindIndex(order, field => SymbolEqualityComparer.Default.Equals(field, from));
            if (index < 0 || index + 1 >= order.Length ||
                Stripped(to) is not IFieldReferenceOperation { Field: { } next } ||
                !SymbolEqualityComparer.Default.Equals(next, order[index + 1]))
            {
                return;
            }
        }
        context.ReportDiagnostic(Diagnostic.Create(SuccessorSwitchRule, syntax.GetLocation(), type.Name));
    }

    /// <summary>The enum member a constant pattern matches, or null.</summary>
    private static IFieldSymbol? Member(IPatternOperation pattern) =>
        pattern is IConstantPatternOperation { Value: { } value } && Stripped(value) is IFieldReferenceOperation { Field: { } field } ? field : null;

    /// <summary>The operation under any conversions.</summary>
    private static IOperation Stripped(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        return operation;
    }

    /// <summary>Reports a member signature that carries a tuple domain value.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context, INamedTypeSymbol[] coreTypes)
    {
        ISymbol member = context.Symbol;
        if (member.IsImplicitlyDeclared || member.IsOverride || !member.Locations.Any(location => location.IsInSource) ||
            member.ContainingType is { IsTupleType: true })
        {
            return;
        }
        IEnumerable<ITypeSymbol> types = member switch
        {
            IFieldSymbol field => [field.Type],
            IPropertySymbol { ExplicitInterfaceImplementations.Length: 0 } property => [property.Type],
            IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.Constructor, ExplicitInterfaceImplementations.Length: 0 } method =>
                [method.ReturnType, .. method.Parameters.Select(parameter => parameter.Type)],
            _ => [],
        };
        if (Implements(member))
            return;
        var reported = new HashSet<string>(StringComparer.Ordinal);
        foreach (INamedTypeSymbol tuple in types.SelectMany(Tuples))
        {
            string[] names = [.. tuple.TupleElements.Where(element => !element.IsImplicitlyDeclared && element.Name != element.CorrespondingTupleField?.Name)
                .Select(element => element.Name)];
            if (names.Length != tuple.TupleElements.Length)
                continue;
            string shape = "(" + string.Join(", ", tuple.TupleElements.Select(element => $"{element.Type.ToDisplayString()} {element.Name}")) + ")";
            if (!reported.Add(shape))
                continue;
            if (Matching(tuple, coreTypes) is { } declared)
            {
                context.ReportDiagnostic(Diagnostic.Create(TupleDomainTypeRule, member.Locations[0], member.Name, shape,
                    $"use the declared type '{declared.Name}'"));
            }
            else if (names.Length >= 3)
            {
                context.ReportDiagnostic(Diagnostic.Create(TupleDomainTypeRule, member.Locations[0], member.Name, shape,
                    "declare a named type for it"));
            }
        }
    }

    /// <summary>Every tuple type within a type: itself, its array elements and its generic arguments.</summary>
    private static IEnumerable<INamedTypeSymbol> Tuples(ITypeSymbol type)
    {
        IEnumerable<ITypeSymbol> inner = type switch
        {
            IArrayTypeSymbol array => [array.ElementType],
            INamedTypeSymbol { IsTupleType: true } tuple => tuple.TupleElements.Select(element => element.Type),
            INamedTypeSymbol named => named.TypeArguments,
            _ => [],
        };
        if (type is INamedTypeSymbol { IsTupleType: true } self)
            yield return self;
        foreach (INamedTypeSymbol nested in inner.SelectMany(Tuples))
            yield return nested;
    }

    /// <summary>A Core type whose public instance properties are exactly the tuple's names with the same types, or null.</summary>
    private static INamedTypeSymbol? Matching(INamedTypeSymbol tuple, INamedTypeSymbol[] coreTypes)
    {
        var elements = tuple.TupleElements.ToDictionary(element => element.Name, element => element.Type, StringComparer.OrdinalIgnoreCase);
        return coreTypes.FirstOrDefault(candidate =>
        {
            IPropertySymbol[] properties = [.. candidate.GetMembers().OfType<IPropertySymbol>()
                .Where(property => !property.IsStatic && !property.IsImplicitlyDeclared && property.DeclaredAccessibility != Accessibility.Private &&
                    property.Name != "EqualityContract")];
            return properties.Length == elements.Count && properties.All(property =>
                elements.TryGetValue(property.Name, out ITypeSymbol? type) &&
                SymbolEqualityComparer.Default.Equals(type, property.Type));
        });
    }

    /// <summary>Collects the source types of a compilation.</summary>
    private static INamedTypeSymbol[] Collect(Compilation compilation)
    {
        var found = new List<INamedTypeSymbol>();
        var pending = new Stack<INamespaceOrTypeSymbol>();
        pending.Push(compilation.Assembly.GlobalNamespace);
        while (pending.Count > 0)
        {
            INamespaceOrTypeSymbol next = pending.Pop();
            foreach (INamespaceOrTypeSymbol member in next.GetMembers().OfType<INamespaceOrTypeSymbol>())
            {
                pending.Push(member);
                if (member is INamedTypeSymbol { TypeKind: TypeKind.Struct or TypeKind.Class, IsStatic: false } type &&
                    type.Locations.Any(location => location.IsInSource) && !type.IsGenericType)
                {
                    found.Add(type);
                }
            }
        }
        return [.. found];
    }

    /// <summary>Whether a member implicitly implements an interface member, whose shape belongs to the interface.</summary>
    private static bool Implements(ISymbol member) =>
        member.ContainingType is { } type && type.AllInterfaces.SelectMany(face => face.GetMembers())
            .Any(required => SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(required), member));
}
