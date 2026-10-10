using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// Finds closed domains that circulate as primitives and closed-domain switches that ignore
/// unexpected values (#627).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrimitiveDomainAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A primitive is switched over a set of named constants: it is a closed domain.</summary>
    public const string PrimitiveSwitchId = "SME6270";

    /// <summary>A switch over a repository-owned enum lets undefined or unhandled values pass silently.</summary>
    public const string SilentEnumSwitchId = "SME6271";

    /// <summary>A primitive local extracted by mask or shift is then treated as a closed selector.</summary>
    public const string MaskedSelectorId = "SME6272";

    /// <summary>An enum is interpolated with a numeric format string, which throws at run time.</summary>
    public const string EnumNumericFormatId = "SME6273";

    private static readonly DiagnosticDescriptor PrimitiveSwitchRule = new(
        PrimitiveSwitchId,
        "Closed domain switched as a primitive",
        "'{0}' is a {1} switched over named constants of {2}; model the domain as a type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Values switched over named constants form a closed domain and must use an enum or domain type end to end.");

    private static readonly DiagnosticDescriptor SilentEnumSwitchRule = new(
        SilentEnumSwitchId,
        "Closed-domain switch ignores unexpected values",
        "Switch over {0} {1}; handle every member explicitly and fail loudly on undefined values",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A switch over a closed domain must not silently ignore unhandled or undefined values. " +
            "A [Flags] enum is exempt: its values are combinations of members, so a catch-all for the " +
            "combinations a switch does not name is part of the domain, not an ignored member.");

    private static readonly DiagnosticDescriptor MaskedSelectorRule = new(
        MaskedSelectorId,
        "Masked primitive used as a selector",
        "Local '{0}' extracts a field by {1} and is then switched over as a selector; decode it to a domain type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A discriminator extracted by mask or shift must be decoded to its domain type rather than carried as a primitive. " +
            "A switch with relational patterns is a piecewise numeric function, not a selector, and a switch expression " +
            "whose catch-all throws is itself the validating decoder at the raw boundary; neither is reported.");

    private static readonly DiagnosticDescriptor EnumNumericFormatRule = new(
        EnumNumericFormatId,
        "Enum interpolated with a numeric format string",
        "'{0}' is a {1} formatted as '{2}'; enums accept only G, D, X and F, so this throws FormatException. Format the underlying value instead",
        "Correctness",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Enum.ToString accepts only the G, D, X and F format strings; a width such as X4 compiles but throws at run time.");

    /// <summary>A primitive symbol is compared against several named constants of one catalog.</summary>
    public const string CatalogComparisonId = "SME6274";

    /// <summary>A primitive parameter is only ever cast, unvalidated, to one repository-owned enum.</summary>
    public const string CastOnlyParameterId = "SME6275";

    /// <summary>Arithmetic or stepping manufactures a value of a closed enum.</summary>
    public const string EnumArithmeticId = "SME6276";

    /// <summary>A closed-domain value is narrowed to a primitive and then used as the domain identity.</summary>
    public const string PrimitiveInterludeId = "SME6277";

    private static readonly DiagnosticDescriptor CatalogComparisonRule = new(
        CatalogComparisonId,
        "Closed domain compared as a primitive",
        "'{0}' is a {1} compared against {2} named constants of {3}; model the domain as a type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A primitive repeatedly compared against members of one constant catalog is a closed domain in disguise.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    private static readonly DiagnosticDescriptor CastOnlyParameterRule = new(
        CastOnlyParameterId,
        "Primitive parameter only cast to a domain enum",
        "Parameter '{0}' is a {1} whose every use is an unchecked cast to {2}; accept {2} or decode at the raw boundary",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An API whose primitive parameter is immediately cast to an enum should take the enum, or validate the raw value once at its boundary.");

    private static readonly DiagnosticDescriptor EnumArithmeticRule = new(
        EnumArithmeticId,
        "Arithmetic produces a closed-domain value",
        "{0} can produce an undefined {1}; use a bounded domain transition",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Integer arithmetic or stepping on a closed enum can manufacture an undefined member; a [Flags] enum is exempt.");

    private static readonly DiagnosticDescriptor PrimitiveInterludeRule = new(
        PrimitiveInterludeId,
        "Closed-domain value carried as a primitive",
        "'{0}' holds a {1} narrowed to {2} and is then {3}; keep the domain type and convert only at the boundary expression",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A typed domain value must stay typed through locals, fields and properties; a narrowed copy that is switched or compared is a primitive interlude.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [PrimitiveSwitchRule, SilentEnumSwitchRule, MaskedSelectorRule, EnumNumericFormatRule,
            CatalogComparisonRule, CastOnlyParameterRule, EnumArithmeticRule, PrimitiveInterludeRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeSwitchStatement, OperationKind.Switch);
        context.RegisterOperationAction(AnalyzeSwitchExpression, OperationKind.SwitchExpression);
        context.RegisterOperationAction(AnalyzeInterpolation, OperationKind.Interpolation);
        context.RegisterOperationAction(AnalyzeEnumConversion, OperationKind.Conversion);
        context.RegisterOperationAction(AnalyzeEnumStep, OperationKind.Increment, OperationKind.Decrement,
            OperationKind.CompoundAssignment);
        context.RegisterOperationBlockStartAction(AnalyzeCastOnlyParameters);
        context.RegisterCompilationStartAction(start => new CompilationDomainFlow().Register(start));
    }

    private static void AnalyzeEnumConversion(OperationAnalysisContext context)
    {
        var conversion = (IConversionOperation)context.Operation;
        if (conversion.IsImplicit || !IsClosedEnum(conversion.Type))
            return;
        if (Unwrap(conversion.Operand) is IBinaryOperation { OperatorKind: var kind } &&
            kind is BinaryOperatorKind.Add or BinaryOperatorKind.Subtract or BinaryOperatorKind.Multiply or
                BinaryOperatorKind.Divide or BinaryOperatorKind.Remainder or BinaryOperatorKind.LeftShift or
                BinaryOperatorKind.RightShift)
        {
            Report(context, EnumArithmeticRule, conversion.Syntax.GetLocation(),
                "Casting arithmetic", conversion.Type!.Name);
        }
    }

    private static void AnalyzeEnumStep(OperationAnalysisContext context)
    {
        ITypeSymbol? target = context.Operation switch
        {
            IIncrementOrDecrementOperation step => step.Target.Type,
            ICompoundAssignmentOperation compound => compound.Target.Type,
            _ => null,
        };
        if (IsClosedEnum(target))
            Report(context, EnumArithmeticRule, context.Operation.Syntax.GetLocation(),
                "Stepping an enum", target!.Name);
    }

    private static void AnalyzeCastOnlyParameters(OperationBlockStartAnalysisContext context)
    {
        if (context.OwningSymbol is not IMethodSymbol method || method.IsOverride ||
            method.ExplicitInterfaceImplementations.Length != 0 || IsInterfaceImplementation(method))
            return;
        var candidates = method.Parameters.Where(parameter => IsIntegral(parameter.Type) && parameter.RefKind == RefKind.None)
            .ToImmutableArray();
        if (candidates.IsEmpty)
            return;
        var uses = new Dictionary<IParameterSymbol, List<ITypeSymbol?>>(SymbolEqualityComparer.Default);
        var gate = new Lock();
        context.RegisterOperationAction(operationContext =>
        {
            var reference = (IParameterReferenceOperation)operationContext.Operation;
            if (!candidates.Contains(reference.Parameter, SymbolEqualityComparer.Default))
                return;
            // A cast fed to Enum.IsDefined is the validation itself, so the parameter is a checked boundary.
            ITypeSymbol? castTarget = reference.Parent is IConversionOperation { IsImplicit: false } cast &&
                IsClosedEnum(cast.Type) && !FeedsIsDefined(cast) ? cast.Type : null;
            lock (gate)
            {
                if (!uses.TryGetValue(reference.Parameter, out List<ITypeSymbol?>? list))
                    uses[reference.Parameter] = list = [];
                list.Add(castTarget);
            }
        }, OperationKind.ParameterReference);
        context.RegisterOperationBlockEndAction(endContext =>
        {
            foreach (KeyValuePair<IParameterSymbol, List<ITypeSymbol?>> entry in uses)
            {
                ITypeSymbol? first = entry.Value[0];
                if (first is null || entry.Value.Any(target => !SymbolEqualityComparer.Default.Equals(target, first)))
                    continue;
                Location location = entry.Key.Locations.FirstOrDefault() ?? Location.None;
                endContext.ReportDiagnostic(Diagnostic.Create(CastOnlyParameterRule, location,
                    entry.Key.Name, entry.Key.Type.ToDisplayString(), first.Name));
            }
        });
    }

    private static bool FeedsIsDefined(IConversionOperation cast) =>
        cast.Parent is IArgumentOperation { Parent: IInvocationOperation invocation } &&
        invocation.TargetMethod.Name == "IsDefined" &&
        invocation.TargetMethod.ContainingType.SpecialType == SpecialType.System_Enum;

    private static bool IsInterfaceImplementation(IMethodSymbol method) =>
        method.ContainingType.AllInterfaces.SelectMany(type => type.GetMembers().OfType<IMethodSymbol>())
            .Any(member => SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(member), method));

    private static bool IsClosedEnum(ITypeSymbol? type) =>
        type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && IsOwned(enumType) && !IsFlags(enumType);

    /// <summary>Compilation-wide evidence for catalog comparisons and primitive interludes.</summary>
    private sealed class CompilationDomainFlow
    {
        private readonly Lock _gate = new();
        private readonly Dictionary<(ISymbol Subject, INamedTypeSymbol Catalog), (HashSet<string> Names, Location First)> _comparisons =
            new(new SubjectCatalogComparer());
        private readonly Dictionary<ISymbol, (ITypeSymbol Domain, Location Site)> _narrowed = new(SymbolEqualityComparer.Default);
        private readonly Dictionary<ISymbol, string> _domainUses = new(SymbolEqualityComparer.Default);

        internal void Register(CompilationStartAnalysisContext context)
        {
            context.RegisterOperationAction(Comparison, OperationKind.Binary);
            context.RegisterOperationAction(IsPattern, OperationKind.IsPattern);
            context.RegisterOperationAction(Narrowing, OperationKind.VariableDeclarator, OperationKind.SimpleAssignment);
            context.RegisterOperationAction(SwitchUse, OperationKind.Switch, OperationKind.SwitchExpression);
            context.RegisterCompilationEndAction(End);
        }

        private void Comparison(OperationAnalysisContext context)
        {
            var binary = (IBinaryOperation)context.Operation;
            if (binary.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
                return;
            IOperation left = Unwrap(binary.LeftOperand), right = Unwrap(binary.RightOperand);
            Record(left, right);
            Record(right, left);
            MarkDomainUse(left, "compared");
            MarkDomainUse(right, "compared");
        }

        private void IsPattern(OperationAnalysisContext context)
        {
            var pattern = (IIsPatternOperation)context.Operation;
            IOperation subject = Unwrap(pattern.Value);
            foreach (IOperation value in PatternValues(pattern.Pattern))
                Record(subject, Unwrap(value));
            MarkDomainUse(subject, "compared");
        }

        private void SwitchUse(OperationAnalysisContext context)
        {
            IOperation value = context.Operation switch
            {
                ISwitchOperation statement => statement.Value,
                ISwitchExpressionOperation expression => expression.Value,
                _ => throw new InvalidOperationException("Unexpected switch operation."),
            };
            MarkDomainUse(Unwrap(value), "switched");
        }

        private void Record(IOperation subject, IOperation constant)
        {
            if (!IsIntegral(subject.Type) || Subject(subject) is not { } symbol)
                return;
            if (constant is not IFieldReferenceOperation { Field: { IsConst: true } field } ||
                !IsOwned(field.ContainingType) || field.ContainingType.TypeKind == TypeKind.Enum)
                return;
            lock (_gate)
            {
                var key = (symbol, field.ContainingType);
                if (!_comparisons.TryGetValue(key, out var entry))
                    _comparisons[key] = entry = (new HashSet<string>(StringComparer.Ordinal), subject.Syntax.GetLocation());
                entry.Names.Add(field.Name);
            }
        }

        private void Narrowing(OperationAnalysisContext context)
        {
            (ISymbol? target, IOperation? value) = context.Operation switch
            {
                IVariableDeclaratorOperation declarator => (declarator.Symbol, declarator.Initializer?.Value),
                ISimpleAssignmentOperation assignment => (Subject(assignment.Target), assignment.Value),
                _ => (null, null),
            };
            if (target is null || value is null || !IsIntegral(TypeOf(target)))
                return;
            if (value is not IConversionOperation { IsImplicit: false } conversion ||
                !IsClosedEnum(Unwrap(conversion.Operand).Type))
                return;
            lock (_gate)
                _narrowed.TryAdd(target, (Unwrap(conversion.Operand).Type!, value.Syntax.GetLocation()));
        }

        private void MarkDomainUse(IOperation operation, string how)
        {
            if (Subject(operation) is { } symbol && IsIntegral(operation.Type))
                lock (_gate)
                    _domainUses.TryAdd(symbol, how);
        }

        private void End(CompilationAnalysisContext context)
        {
            lock (_gate)
            {
                foreach (var entry in _comparisons)
                {
                    if (entry.Value.Names.Count < 2)
                        continue;
                    context.ReportDiagnostic(Diagnostic.Create(CatalogComparisonRule, entry.Value.First,
                        entry.Key.Subject.Name, TypeOf(entry.Key.Subject)!.ToDisplayString(), entry.Value.Names.Count,
                        entry.Key.Catalog.Name));
                }
                foreach (var entry in _narrowed)
                {
                    if (!_domainUses.TryGetValue(entry.Key, out string? how))
                        continue;
                    context.ReportDiagnostic(Diagnostic.Create(PrimitiveInterludeRule, entry.Value.Site,
                        entry.Key.Name, entry.Value.Domain.Name, TypeOf(entry.Key)!.ToDisplayString(), how));
                }
            }
        }

        private static ISymbol? Subject(IOperation operation) => operation switch
        {
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            IFieldReferenceOperation { Field.IsConst: false } field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => null,
        };

        private static ITypeSymbol? TypeOf(ISymbol symbol) => symbol switch
        {
            ILocalSymbol local => local.Type,
            IParameterSymbol parameter => parameter.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null,
        };

        private sealed class SubjectCatalogComparer : IEqualityComparer<(ISymbol Subject, INamedTypeSymbol Catalog)>
        {
            public bool Equals((ISymbol Subject, INamedTypeSymbol Catalog) x, (ISymbol Subject, INamedTypeSymbol Catalog) y) =>
                SymbolEqualityComparer.Default.Equals(x.Subject, y.Subject) &&
                SymbolEqualityComparer.Default.Equals(x.Catalog, y.Catalog);

            public int GetHashCode((ISymbol Subject, INamedTypeSymbol Catalog) value) =>
                (SymbolEqualityComparer.Default.GetHashCode(value.Subject) * 397) ^
                SymbolEqualityComparer.Default.GetHashCode(value.Catalog);
        }
    }

    private static void AnalyzeInterpolation(OperationAnalysisContext context)
    {
        var operation = (IInterpolationOperation)context.Operation;
        if (operation.FormatString is not ILiteralOperation { ConstantValue: { HasValue: true, Value: string format } })
            return;
        if (format is "G" or "g" or "D" or "d" or "X" or "x" or "F" or "f")
            return;
        ITypeSymbol? type = operation.Expression.Type;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            type = nullable.TypeArguments[0];
        if (type is not INamedTypeSymbol { TypeKind: TypeKind.Enum })
            return;
        Report(context, EnumNumericFormatRule, operation.Syntax.GetLocation(),
            operation.Expression.Syntax.ToString(), type.Name, format);
    }

    private static void AnalyzeSwitchStatement(OperationAnalysisContext context)
    {
        var operation = (ISwitchOperation)context.Operation;
        IOperation value = Unwrap(operation.Value);
        IEnumerable<IOperation> labels = operation.Cases
            .SelectMany(section => section.Clauses)
            .SelectMany(LabelValues);
        AnalyzePrimitiveSwitch(context, value, labels, operation.Syntax);
        bool numeric = operation.Cases.SelectMany(section => section.Clauses)
            .OfType<IPatternCaseClauseOperation>().Any(clause => HasRelational(clause.Pattern));
        if (!numeric)
            AnalyzeMaskedSelector(context, value);

        if (value.Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && IsOwned(enumType) && !IsFlags(enumType))
        {
            ISwitchCaseOperation? defaultSection = operation.Cases.FirstOrDefault(section =>
                section.Clauses.Any(clause => clause.CaseKind == CaseKind.Default));
            if (defaultSection is null)
            {
                if (!CoversAllMembers(enumType, labels))
                    Report(context, SilentEnumSwitchRule, operation.Syntax.GetLocation(),
                        enumType.Name, "has no default and leaves members unhandled");
            }
            else if (!AlwaysThrows(defaultSection.Body))
            {
                Report(context, SilentEnumSwitchRule, DefaultLocation(defaultSection), enumType.Name,
                    "has a default that does not fail");
            }
        }
    }

    private static void AnalyzeSwitchExpression(OperationAnalysisContext context)
    {
        var operation = (ISwitchExpressionOperation)context.Operation;
        IOperation value = Unwrap(operation.Value);
        IEnumerable<IOperation> labels = operation.Arms.SelectMany(arm => PatternValues(arm.Pattern));
        AnalyzePrimitiveSwitch(context, value, labels, operation.Syntax);
        bool numeric = operation.Arms.Any(arm => HasRelational(arm.Pattern));
        bool decoder = operation.Arms.Any(arm => arm.Pattern is IDiscardPatternOperation && arm.Guard is null &&
            Unwrap(arm.Value) is IThrowOperation);
        if (!numeric && !decoder)
            AnalyzeMaskedSelector(context, value);

        if (value.Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && IsOwned(enumType) && !IsFlags(enumType))
        {
            foreach (ISwitchExpressionArmOperation arm in operation.Arms)
            {
                if (arm.Pattern is IDiscardPatternOperation && arm.Guard is null &&
                    Unwrap(arm.Value) is not IThrowOperation)
                {
                    Report(context, SilentEnumSwitchRule, arm.Syntax.GetLocation(), enumType.Name,
                        "has a discard arm that does not fail");
                }
            }
        }
    }

    private static void AnalyzePrimitiveSwitch(OperationAnalysisContext context, IOperation value,
        IEnumerable<IOperation> labels, SyntaxNode syntax)
    {
        if (!IsIntegral(value.Type))
            return;
        // Two or more labels drawn from one catalog's named constants show a closed domain.
        var catalogs = labels
            .Select(Unwrap)
            .OfType<IFieldReferenceOperation>()
            .Where(reference => reference.Field.IsConst)
            .GroupBy(reference => reference.Field.ContainingType, SymbolEqualityComparer.Default)
            .Where(group => group.Select(reference => reference.Field.Name).Distinct().Count() >= 2)
            .Select(group => group.Key!)
            .ToArray();
        if (catalogs.Length == 0)
            return;
        Report(context, PrimitiveSwitchRule, syntax.GetLocation(), Describe(value), value.Type!.ToDisplayString(),
            string.Join(", ", catalogs.Select(catalog => catalog.Name)));
    }

    private static void AnalyzeMaskedSelector(OperationAnalysisContext context, IOperation value)
    {
        if (value is not ILocalReferenceOperation { Local: var local } || !IsIntegral(local.Type))
            return;
        foreach (SyntaxReference reference in local.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: var initializer })
                continue;
            ExpressionSyntax stripped = initializer;
            while (stripped is ParenthesizedExpressionSyntax or CastExpressionSyntax or CheckedExpressionSyntax)
            {
                stripped = stripped switch
                {
                    ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
                    CastExpressionSyntax cast => cast.Expression,
                    CheckedExpressionSyntax check => check.Expression,
                    _ => stripped,
                };
            }
            string? extraction = stripped.Kind() switch
            {
                SyntaxKind.BitwiseAndExpression => "mask",
                SyntaxKind.RightShiftExpression or SyntaxKind.UnsignedRightShiftExpression => "shift",
                _ => null,
            };
            if (extraction is not null)
                Report(context, MaskedSelectorRule, reference.GetSyntax().GetLocation(), local.Name, extraction);
        }
    }

    private static IEnumerable<IOperation> LabelValues(ICaseClauseOperation clause) => clause switch
    {
        ISingleValueCaseClauseOperation single => [single.Value],
        IPatternCaseClauseOperation pattern => PatternValues(pattern.Pattern),
        _ => [],
    };

    private static IEnumerable<IOperation> PatternValues(IPatternOperation pattern) => pattern switch
    {
        IConstantPatternOperation constant => [constant.Value],
        IBinaryPatternOperation binary => PatternValues(binary.LeftPattern).Concat(PatternValues(binary.RightPattern)),
        INegatedPatternOperation negated => PatternValues(negated.Pattern),
        _ => [],
    };

    private static bool HasRelational(IPatternOperation pattern) => pattern switch
    {
        IRelationalPatternOperation => true,
        IBinaryPatternOperation binary => HasRelational(binary.LeftPattern) || HasRelational(binary.RightPattern),
        INegatedPatternOperation negated => HasRelational(negated.Pattern),
        _ => false,
    };

    private static bool CoversAllMembers(INamedTypeSymbol enumType, IEnumerable<IOperation> labels)
    {
        var handled = new HashSet<object?>(labels.Select(label => Unwrap(label).ConstantValue)
            .Where(constant => constant.HasValue)
            .Select(constant => constant.Value));
        return enumType.GetMembers().OfType<IFieldSymbol>()
            .Where(field => field.HasConstantValue)
            .All(field => handled.Contains(field.ConstantValue));
    }

    private static bool AlwaysThrows(ImmutableArray<IOperation> body)
    {
        IOperation? last = body.LastOrDefault();
        while (last is IBlockOperation block)
            last = block.Operations.LastOrDefault();
        return last switch
        {
            IThrowOperation => true,
            IExpressionStatementOperation { Operation: IThrowOperation } => true,
            _ => false,
        };
    }

    private static Location DefaultLocation(ISwitchCaseOperation section) =>
        section.Clauses.First(clause => clause.CaseKind == CaseKind.Default).Syntax.GetLocation();

    private static bool IsOwned(INamedTypeSymbol type) =>
        type.Locations.Any(location => location.IsInSource);

    private static bool IsFlags(INamedTypeSymbol type) =>
        type.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");

    private static bool IsIntegral(ITypeSymbol? type) => type?.SpecialType is
        SpecialType.System_Byte or SpecialType.System_SByte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64;

    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation { IsImplicit: true } conversion)
            operation = conversion.Operand;
        return operation;
    }

    private static string Describe(IOperation value) => value switch
    {
        ILocalReferenceOperation local => local.Local.Name,
        IParameterReferenceOperation parameter => parameter.Parameter.Name,
        IFieldReferenceOperation field => field.Field.Name,
        IPropertyReferenceOperation property => property.Property.Name,
        _ => value.Syntax.ToString(),
    };

    private static void Report(OperationAnalysisContext context, DiagnosticDescriptor rule, Location location,
        params object[] arguments) =>
        context.ReportDiagnostic(Diagnostic.Create(rule, location, arguments));
}
