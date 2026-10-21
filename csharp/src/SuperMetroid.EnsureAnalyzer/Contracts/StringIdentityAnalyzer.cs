using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1377: identities are typed, and state has an owner. In SuperMetroid.Core this reports a switch whose case labels are
/// string constants and an equality test of a string field or property against a string constant (SME6433); a string field
/// or property whose every assigned value is a <c>const string</c> of one type (SME6434); an interpolated string of only
/// integral or enum holes used as a dictionary key (SME6435); and a non-private mutable instance field of a class, which
/// other types write directly (SME6436).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StringIdentityAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A string constant selects behaviour.</summary>
    public const string StringDispatchId = "SME6433";

    /// <summary>A string member holds one of a fixed set of string constants.</summary>
    public const string StringIdentityFieldId = "SME6434";

    /// <summary>A dictionary key is composed from numbers or enum values.</summary>
    public const string ComposedKeyId = "SME6435";

    /// <summary>A class exposes a mutable field to other types.</summary>
    public const string ExposedFieldId = "SME6436";

    /// <summary>The assembly whose code is checked.</summary>
    public const string CheckedAssembly = "SuperMetroid.Core";

    /// <summary>Reports a string switch or a string equality test against a constant.</summary>
    private static readonly DiagnosticDescriptor StringDispatchRule = new(
        StringDispatchId,
        "String constant selects behaviour",
        "{0} dispatches on string constants; name the cases as an enum or typed identity",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A switch on string constants, or a string member compared with a string constant, uses text as an identity; " +
            "an enum or typed identity names the cases and lets the compiler check them (#1377).");

    /// <summary>Reports a string member whose values are all one type's string constants.</summary>
    private static readonly DiagnosticDescriptor StringIdentityFieldRule = new(
        StringIdentityFieldId,
        "String member holds a fixed identity",
        "'{0}' only ever holds string constants of '{1}'; store an enum or typed identity",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A string field or property whose every assigned value is a const string of one type is an identity spelled " +
            "as text (#1377).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <summary>Reports a dictionary key composed from integral or enum values.</summary>
    private static readonly DiagnosticDescriptor ComposedKeyRule = new(
        ComposedKeyId,
        "Dictionary key composed from values",
        "This key formats only integral or enum values into text; key the lookup by a typed identity or index an array",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An interpolated string whose every hole is an integral or enum value, used as a dictionary key, turns a " +
            "typed value into text to look it up (#1377).");

    /// <summary>Reports a non-private mutable instance field of a class.</summary>
    private static readonly DiagnosticDescriptor ExposedFieldRule = new(
        ExposedFieldId,
        "Class exposes a mutable field",
        "'{0}' of '{2}' is a mutable {1} field; make it private and give its owner the operations other types need",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A non-private, non-readonly instance field of a class lets other types write the owner's state directly; " +
            "a property with a narrow setter or a domain operation takes its place (#1377).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [StringDispatchRule, StringIdentityFieldRule, ComposedKeyRule, ExposedFieldRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CheckedAssembly)
                return;
            var assigned = new ConcurrentDictionary<ISymbol, ConcurrentBag<INamedTypeSymbol?>>(SymbolEqualityComparer.Default);
            start.RegisterOperationAction(AnalyzeSwitch, OperationKind.Switch, OperationKind.SwitchExpression);
            start.RegisterOperationAction(AnalyzeEquality, OperationKind.Binary);
            start.RegisterOperationAction(AnalyzeKey, OperationKind.InterpolatedString);
            start.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            start.RegisterOperationAction(operation => CollectAssignment(operation, assigned),
                OperationKind.SimpleAssignment, OperationKind.FieldInitializer, OperationKind.PropertyInitializer);
            start.RegisterCompilationEndAction(end => ReportIdentities(end, assigned));
        });
    }

    /// <summary>Reports a switch statement or expression with a string constant case.</summary>
    private static void AnalyzeSwitch(OperationAnalysisContext context)
    {
        bool stringCases = context.Operation switch
        {
            ISwitchOperation statement => statement.Cases.SelectMany(section => section.Clauses).Any(clause => clause switch
            {
                ISingleValueCaseClauseOperation single => IsStringConstant(single.Value),
                IPatternCaseClauseOperation pattern => HasStringConstant(pattern.Pattern),
                _ => false,
            }),
            ISwitchExpressionOperation expression => expression.Arms.Any(arm => HasStringConstant(arm.Pattern)),
            _ => false,
        };
        if (stringCases)
            context.ReportDiagnostic(Diagnostic.Create(StringDispatchRule, context.Operation.Syntax.GetLocation(), "This switch"));
    }

    /// <summary>Whether a pattern matches a string constant anywhere within it.</summary>
    private static bool HasStringConstant(IPatternOperation pattern) => pattern switch
    {
        IConstantPatternOperation constant => IsStringConstant(constant.Value),
        IBinaryPatternOperation binary => HasStringConstant(binary.LeftPattern) || HasStringConstant(binary.RightPattern),
        INegatedPatternOperation negated => HasStringConstant(negated.Pattern),
        _ => false,
    };

    /// <summary>Reports a string field or property compared for equality with a string constant.</summary>
    private static void AnalyzeEquality(OperationAnalysisContext context)
    {
        var binary = (IBinaryOperation)context.Operation;
        if (binary.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
            return;
        IOperation left = Stripped(binary.LeftOperand), right = Stripped(binary.RightOperand);
        if ((IsStringMember(left) && IsStringConstant(right)) || (IsStringMember(right) && IsStringConstant(left)))
            context.ReportDiagnostic(Diagnostic.Create(StringDispatchRule, binary.Syntax.GetLocation(), "This comparison"));
    }

    /// <summary>Whether an operation reads a string-typed field or property.</summary>
    private static bool IsStringMember(IOperation operation) =>
        operation is IFieldReferenceOperation { Field.Type.SpecialType: SpecialType.System_String, Field.IsConst: false } or
            IPropertyReferenceOperation { Property.Type.SpecialType: SpecialType.System_String };

    /// <summary>Whether an operation is a non-null string constant.</summary>
    private static bool IsStringConstant(IOperation operation) =>
        Stripped(operation).ConstantValue is { HasValue: true, Value: string };

    /// <summary>Reports an all-integral interpolated string used as a dictionary key.</summary>
    private static void AnalyzeKey(OperationAnalysisContext context)
    {
        var interpolated = (IInterpolatedStringOperation)context.Operation;
        IInterpolationOperation[] holes = [.. interpolated.Parts.OfType<IInterpolationOperation>()];
        if (holes.Length == 0 || !holes.All(hole => IsIntegralOrEnum(hole.Expression.Type)) || !IsDictionaryKey(interpolated))
            return;
        context.ReportDiagnostic(Diagnostic.Create(ComposedKeyRule, interpolated.Syntax.GetLocation()));
    }

    /// <summary>Whether a value is an integral number or an enum.</summary>
    private static bool IsIntegralOrEnum(ITypeSymbol? type) =>
        type is not null && (type.TypeKind == TypeKind.Enum || type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or
            SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or
            SpecialType.System_Int64 or SpecialType.System_UInt64);

    /// <summary>Whether an interpolated string is a dictionary indexer key or a key passed to a dictionary lookup.</summary>
    private static bool IsDictionaryKey(IInterpolatedStringOperation interpolated)
    {
        IOperation? parent = interpolated.Parent;
        while (parent is IConversionOperation)
            parent = parent.Parent;
        if (parent is not IArgumentOperation argument)
            return false;
        return argument.Parent switch
        {
            IPropertyReferenceOperation { Property.IsIndexer: true } indexer => IsDictionary(indexer.Property.ContainingType),
            IInvocationOperation invocation => invocation.TargetMethod.Name is "TryGetValue" or "ContainsKey" or "GetValueOrDefault" or
                "Remove" or "TryAdd" or "Add" && IsDictionary(invocation.TargetMethod.ContainingType ?? invocation.Instance?.Type),
            _ => false,
        };
    }

    /// <summary>Whether a type is, or implements, a generic dictionary.</summary>
    private static bool IsDictionary(ITypeSymbol? type) =>
        type is INamedTypeSymbol named && (IsDictionaryInterface(named) || named.AllInterfaces.Any(IsDictionaryInterface) ||
            named.Name is "CollectionExtensions");

    /// <summary>Whether a type is a generic dictionary interface.</summary>
    private static bool IsDictionaryInterface(INamedTypeSymbol type) =>
        type.Name is "IDictionary" or "IReadOnlyDictionary" && type.ContainingNamespace?.ToDisplayString() == "System.Collections.Generic";

    /// <summary>Reports a non-private, non-readonly instance field of a class.</summary>
    private static void AnalyzeField(SymbolAnalysisContext context)
    {
        var field = (IFieldSymbol)context.Symbol;
        if (field.IsStatic || field.IsConst || field.IsReadOnly || field.IsImplicitlyDeclared ||
            field.DeclaredAccessibility is Accessibility.Private or Accessibility.NotApplicable ||
            field.ContainingType.TypeKind != TypeKind.Class || OnlyContainerReaches(field.ContainingType))
        {
            return;
        }
        string access = field.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Private or Accessibility.NotApplicable => throw new InvalidOperationException($"'{field.Name}' is not exposed."),
            _ => throw new InvalidOperationException($"Undefined accessibility {field.DeclaredAccessibility}."),
        };
        context.ReportDiagnostic(Diagnostic.Create(ExposedFieldRule, field.Locations[0], field.Name, access, field.ContainingType.Name));
    }

    /// <summary>Whether a type is nested privately, so only the type that declares it can reach its fields.</summary>
    private static bool OnlyContainerReaches(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility == Accessibility.Private)
                return true;
        }
        return false;
    }

    /// <summary>Records the type of the string constant each string field or property is assigned, or null for any other value.</summary>
    private static void CollectAssignment(OperationAnalysisContext context, ConcurrentDictionary<ISymbol, ConcurrentBag<INamedTypeSymbol?>> assigned)
    {
        (IEnumerable<ISymbol> targets, IOperation? value) = context.Operation switch
        {
            ISimpleAssignmentOperation { Target: IFieldReferenceOperation field } assignment => new IdentityAssignment([field.Field], assignment.Value),
            ISimpleAssignmentOperation { Target: IPropertyReferenceOperation property } assignment => new IdentityAssignment([property.Property], assignment.Value),
            IFieldInitializerOperation initializer => new IdentityAssignment(initializer.InitializedFields.Cast<ISymbol>(), initializer.Value),
            IPropertyInitializerOperation initializer => new IdentityAssignment(initializer.InitializedProperties.Cast<ISymbol>(), initializer.Value),
            _ => new IdentityAssignment(Enumerable.Empty<ISymbol>(), null),
        };
        foreach (ISymbol target in targets)
        {
            if (target is not (IFieldSymbol { Type.SpecialType: SpecialType.System_String, IsConst: false } or
                IPropertySymbol { Type.SpecialType: SpecialType.System_String }) ||
                !SymbolEqualityComparer.Default.Equals(target.ContainingAssembly, context.Compilation.Assembly))
            {
                continue;
            }
            ConcurrentBag<INamedTypeSymbol?> sources = assigned.GetOrAdd(target.OriginalDefinition, _ => []);
            foreach (INamedTypeSymbol? source in ConstantOwners(value))
                sources.Add(source);
        }
    }

    /// <summary>
    /// The types whose string constants a value can be: one for a constant read, each branch's for a conditional or
    /// coalesce, and null for anything else.
    /// </summary>
    private static IEnumerable<INamedTypeSymbol?> ConstantOwners(IOperation? value) => value is null ? [null] : Stripped(value) switch
    {
        IFieldReferenceOperation { Field: { IsConst: true, Type.SpecialType: SpecialType.System_String } constant } => [constant.ContainingType],
        IConditionalOperation { WhenFalse: { } whenFalse } conditional => ConstantOwners(conditional.WhenTrue).Concat(ConstantOwners(whenFalse)),
        ICoalesceOperation coalesce => ConstantOwners(coalesce.Value).Concat(ConstantOwners(coalesce.WhenNull)),
        _ => [null],
    };

    /// <summary>Reports each string member whose every assigned value is a string constant of one type.</summary>
    private static void ReportIdentities(CompilationAnalysisContext context, ConcurrentDictionary<ISymbol, ConcurrentBag<INamedTypeSymbol?>> assigned)
    {
        foreach (KeyValuePair<ISymbol, ConcurrentBag<INamedTypeSymbol?>> entry in assigned)
        {
            ISymbol member = entry.Key;
            INamedTypeSymbol?[] all = [.. entry.Value];
            if (all.Length == 0 || all[0] is not { } owner || member.Locations.IsEmpty || !member.Locations[0].IsInSource)
                continue;
            if (all.All(source => SymbolEqualityComparer.Default.Equals(source, owner)))
                context.ReportDiagnostic(Diagnostic.Create(StringIdentityFieldRule, member.Locations[0], member.Name, owner.Name));
        }
    }

    /// <summary>The operation under any conversions.</summary>
    private static IOperation Stripped(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        return operation;
    }
}
