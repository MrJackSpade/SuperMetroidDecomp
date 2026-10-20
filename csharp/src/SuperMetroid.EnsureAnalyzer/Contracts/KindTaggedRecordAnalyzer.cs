using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1376: a closed set of variants modelled as one type that carries every variant's fields, plus a <c>Kind</c> enum
/// saying which of them mean anything, lets a reader read a field that is meaningless for the current kind. In
/// SuperMetroid.Core this reports a construction of a type with an enum <c>Kind</c> member that passes two or more
/// placeholder arguments beside the kind (SME6431): a <c>default</c>, a literal <c>0</c>, <c>false</c> or <c>null</c>, or an
/// optional parameter left to its default. Such a type is kind-tagged, and a switch on its kind whose arms read different
/// subsets of its other members is reported too (SME6432). Each variant becomes a sealed
/// record holding only its own fields, and consumers switch on the type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class KindTaggedRecordAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A kind-tagged type is built with placeholder fields.</summary>
    public const string PlaceholderConstructionId = "SME6431";

    /// <summary>A switch on a kind reads different fields per arm.</summary>
    public const string KindSwitchId = "SME6432";

    /// <summary>The assembly whose code is checked.</summary>
    public const string CheckedAssembly = "SuperMetroid.Core";

    /// <summary>Reports a construction that fills the fields other kinds own with placeholders.</summary>
    private static readonly DiagnosticDescriptor PlaceholderConstructionRule = new(
        PlaceholderConstructionId,
        "Kind-tagged type built with placeholders",
        "'{0}' is built with {1} placeholder arguments beside its '{2}'; make each kind a sealed record holding only its own fields",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A type with an enum Kind member whose constructions pass default, 0, false or null for the fields the kind " +
            "does not use carries every variant's fields at once; a closed hierarchy holds each variant's fields alone (#1376).");

    /// <summary>Reports a switch on a kind whose arms read different members of the switched type.</summary>
    private static readonly DiagnosticDescriptor KindSwitchRule = new(
        KindSwitchId,
        "Switch on a kind reads per-kind fields",
        "This switch on '{0}.{1}' reads different members of '{0}' in different arms; switch on the variant type instead",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A switch on the Kind member of a kind-tagged type whose arms read different subsets of the type's other " +
            "members is a type switch over variants that share one record; each variant's fields belong to its own sealed record (#1376).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [PlaceholderConstructionRule, KindSwitchRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CheckedAssembly)
                return;
            var tagged = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);
            var switches = new ConcurrentBag<(INamedTypeSymbol Type, Diagnostic Diagnostic)>();
            start.RegisterOperationAction(operation => AnalyzeCreation(operation, tagged), OperationKind.ObjectCreation);
            start.RegisterOperationAction(operation => AnalyzeSwitchExpression(operation, switches), OperationKind.SwitchExpression);
            start.RegisterOperationAction(operation => AnalyzeSwitchStatement(operation, switches), OperationKind.Switch);
            start.RegisterCompilationEndAction(end =>
            {
                // A switch is reported only over a type some construction shows to be kind-tagged.
                foreach (var (type, diagnostic) in switches)
                    if (tagged.ContainsKey(type))
                        end.ReportDiagnostic(diagnostic);
            });
        });
    }

    /// <summary>Reports a creation that passes the kind and two or more placeholders.</summary>
    private static void AnalyzeCreation(OperationAnalysisContext context, ConcurrentDictionary<INamedTypeSymbol, byte> tagged)
    {
        var creation = (IObjectCreationOperation)context.Operation;
        if (creation.Type is not INamedTypeSymbol type || KindMember(type) is not { } kind)
            return;
        ITypeSymbol kindType = MemberType(kind);
        bool passesKind = false;
        int placeholders = 0;
        foreach (IArgumentOperation argument in creation.Arguments)
        {
            if (argument.ArgumentKind == ArgumentKind.DefaultValue)
            {
                placeholders++;
                continue;
            }
            if (argument.ArgumentKind != ArgumentKind.Explicit)
                continue;
            IOperation value = Stripped(argument.Value);
            if (SymbolEqualityComparer.Default.Equals(value.Type, kindType))
                passesKind = true;
            else if (IsPlaceholder(value))
                placeholders++;
        }
        if (!passesKind || placeholders < 2)
            return;
        tagged.TryAdd(type.OriginalDefinition, 0);
        context.ReportDiagnostic(Diagnostic.Create(PlaceholderConstructionRule, creation.Syntax.GetLocation(), type.Name, placeholders, kind.Name));
    }

    /// <summary>Reports a switch expression on a kind whose arms read different members.</summary>
    private static void AnalyzeSwitchExpression(OperationAnalysisContext context, ConcurrentBag<(INamedTypeSymbol, Diagnostic)> switches)
    {
        var switchExpression = (ISwitchExpressionOperation)context.Operation;
        if (SwitchedKind(switchExpression.Value) is not { } kind)
            return;
        Report(switches, switchExpression.Syntax, kind,
            switchExpression.Arms.Select(arm => (IEnumerable<IOperation>)[.. arm.Guard is null ? [] : new[] { arm.Guard }, arm.Value]));
    }

    /// <summary>Reports a switch statement on a kind whose sections read different members.</summary>
    private static void AnalyzeSwitchStatement(OperationAnalysisContext context, ConcurrentBag<(INamedTypeSymbol, Diagnostic)> switches)
    {
        var switchStatement = (ISwitchOperation)context.Operation;
        if (SwitchedKind(switchStatement.Value) is not { } kind)
            return;
        Report(switches, switchStatement.Syntax, kind, switchStatement.Cases.Select(section => (IEnumerable<IOperation>)section.Body));
    }

    /// <summary>Reports when the arms that read members of the kind's type do not all read the same ones.</summary>
    private static void Report(ConcurrentBag<(INamedTypeSymbol, Diagnostic)> switches, SyntaxNode syntax, ISymbol kind, IEnumerable<IEnumerable<IOperation>> arms)
    {
        INamedTypeSymbol type = kind.ContainingType;
        var subsets = new HashSet<string>(StringComparer.Ordinal);
        foreach (IEnumerable<IOperation> arm in arms)
        {
            var read = new SortedSet<string>(StringComparer.Ordinal);
            foreach (IOperation operation in arm.SelectMany(root => root.DescendantsAndSelf()))
            {
                if (operation is IMemberReferenceOperation { Member: IPropertySymbol or IFieldSymbol } reference &&
                    !reference.Member.IsStatic &&
                    SymbolEqualityComparer.Default.Equals(reference.Member.ContainingType, type) &&
                    !SymbolEqualityComparer.Default.Equals(reference.Member, kind))
                {
                    read.Add(reference.Member.Name);
                }
            }
            if (read.Count > 0)
                subsets.Add(string.Join(",", read));
        }
        if (subsets.Count > 1)
            switches.Add((type.OriginalDefinition, Diagnostic.Create(KindSwitchRule, syntax.GetLocation(), type.Name, kind.Name)));
    }

    /// <summary>The kind member a switch's governing value reads, when it is one.</summary>
    private static ISymbol? SwitchedKind(IOperation value) =>
        Stripped(value) is IMemberReferenceOperation { Member: IPropertySymbol or IFieldSymbol } reference &&
        !reference.Member.IsStatic && KindMember(reference.Member.ContainingType) is { } kind &&
        SymbolEqualityComparer.Default.Equals(kind, reference.Member)
            ? kind
            : null;

    /// <summary>The type's instance member named <c>Kind</c>, or ending in <c>Kind</c>, whose type is an enum.</summary>
    private static ISymbol? KindMember(INamedTypeSymbol type) =>
        type.GetMembers().FirstOrDefault(member => !member.IsStatic && !member.IsImplicitlyDeclared &&
            member is IPropertySymbol or IFieldSymbol && member.Name.EndsWith("Kind", StringComparison.Ordinal) &&
            MemberType(member).TypeKind == TypeKind.Enum);

    /// <summary>The type of a property or field.</summary>
    private static ITypeSymbol MemberType(ISymbol member) => member switch
    {
        IPropertySymbol property => property.Type,
        IFieldSymbol field => field.Type,
        _ => throw new ArgumentException($"'{member.Name}' is neither a property nor a field.", nameof(member)),
    };

    /// <summary>
    /// Whether an argument is a placeholder: <c>default</c>, or a literal zero, false or null. A named constant, such as an
    /// enum member whose value happens to be zero, is a chosen value.
    /// </summary>
    private static bool IsPlaceholder(IOperation value) =>
        value is IDefaultValueOperation ||
        (value is ILiteralOperation && value.ConstantValue is { HasValue: true, Value: var constant } &&
            constant switch
            {
                null => true,
                bool flag => !flag,
                string => false,
                char character => character == '\0',
                IConvertible number => Convert.ToDecimal(number, System.Globalization.CultureInfo.InvariantCulture) == 0,
                _ => false,
            });

    /// <summary>The operation under any conversions.</summary>
    private static IOperation Stripped(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        return operation;
    }
}
