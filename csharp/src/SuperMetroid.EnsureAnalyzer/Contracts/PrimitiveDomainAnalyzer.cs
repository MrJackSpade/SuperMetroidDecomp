using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

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

    /// <summary>Reports primitive switches whose labels use multiple constants from one named catalog.</summary>
    private static readonly DiagnosticDescriptor PrimitiveSwitchRule = new(
        PrimitiveSwitchId,
        "Closed domain switched as a primitive",
        "'{0}' is a {1} switched over named constants of {2}; model the domain as a type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Values switched over named constants form a closed domain and must use an enum or domain type end to end.");

    /// <summary>Reports owned, non-flags enum switches that can silently accept an unhandled value.</summary>
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

    /// <summary>Reports a mask- or shift-extracted local switched as a closed selector.</summary>
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

    /// <summary>Reports enum interpolation formats unsupported by Enum.ToString.</summary>
    private static readonly DiagnosticDescriptor EnumNumericFormatRule = new(
        EnumNumericFormatId,
        "Enum interpolated with a numeric format string",
        "'{0}' is a {1} formatted as '{2}'; enums accept only G, D, X and F, so this throws FormatException. Format the underlying value instead.",
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

    /// <summary>Storage written and compared only as a small set of literals is a closed domain.</summary>
    public const string LiteralDomainId = "SME6278";

    /// <summary>A masked or shifted local is compared against several values as a hand-decoded selector.</summary>
    public const string MaskedComparisonId = "SME6279";

    /// <summary>Controller buttons are converted to or from a raw word outside a controller-word boundary (#1306).</summary>
    public const string ControllerWordConversionId = "SME6284";

    /// <summary>Marks the serialization and external-word declarations that may convert controller buttons.</summary>
    public const string ControllerWordBoundaryAttributeName = "ButtonWordEncodingAttribute";

    /// <summary>The Core type that carries controller state; its raw words exist only where they are stored or read.</summary>
    private const string ControllerButtonsType = "SuperMetroid.Core.Input.ControllerButton";

    /// <summary>Reports controller buttons narrowed to or widened from an integer outside a declared boundary.</summary>
    private static readonly DiagnosticDescriptor ControllerWordConversionRule = new(
        ControllerWordConversionId,
        "Controller buttons converted outside a boundary",
        "'{0}' converts controller buttons {1} a raw word outside a [ButtonWordEncoding]; keep ControllerButton and convert only where words are stored or read",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Controller state flows through Core as ControllerButton; only the declarations that store or read controller words " +
            "(journals, SRAM, enemy variables, codecs) convert it, and each says why with [ButtonWordEncoding].");

    /// <summary>Reports a mask- or shift-extracted local compared with several nonzero values.</summary>
    private static readonly DiagnosticDescriptor MaskedComparisonRule = new(
        MaskedComparisonId,
        "Masked primitive compared as a selector",
        "Local '{0}' extracts a field by {1} and is then compared against several values as a selector; decode it to a domain type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A masked or shifted local compared for equality against two or more distinct nonzero constants is a " +
            "selector decoded by hand. Zero tests and bit tests of positions or phases are arithmetic and are not reported.");

    /// <summary>Reports storage whose complete writes and identity tests form a small literal state set.</summary>
    private static readonly DiagnosticDescriptor LiteralDomainRule = new(
        LiteralDomainId,
        "Closed domain stored as literals",
        "'{0}' is a {1} written and compared only as the values {2}; model the closed domain as a type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A field or property whose every write is one of a few constants, and which is only switched or " +
            "compared against those constants, is a closed state set; stepped or ordered quantities are excluded.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <summary>Reports primitive storage compared against multiple constants from one repository catalog.</summary>
    private static readonly DiagnosticDescriptor CatalogComparisonRule = new(
        CatalogComparisonId,
        "Closed domain compared as a primitive",
        "'{0}' is a {1} compared against {2} named constants of {3}; model the domain as a type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A primitive repeatedly compared against members of one constant catalog is a closed domain in disguise.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <summary>Reports primitive parameters used solely as unchecked casts to one closed enum.</summary>
    private static readonly DiagnosticDescriptor CastOnlyParameterRule = new(
        CastOnlyParameterId,
        "Primitive parameter only cast to a domain enum",
        "Parameter '{0}' is a {1} whose every use is an unchecked cast to {2}; accept {2} or decode at the raw boundary",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An API whose primitive parameter is immediately cast to an enum should take the enum, or validate the raw value once at its boundary.");

    /// <summary>Reports arithmetic or stepping that can create an undefined closed-enum value.</summary>
    private static readonly DiagnosticDescriptor EnumArithmeticRule = new(
        EnumArithmeticId,
        "Arithmetic produces a closed-domain value",
        "{0} can produce an undefined {1}; use a bounded domain transition",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Integer arithmetic or stepping on a closed enum can manufacture an undefined member; a [Flags] enum is exempt.");

    /// <summary>Reports a closed-enum value narrowed to primitive storage before identity use.</summary>
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
            CatalogComparisonRule, CastOnlyParameterRule, EnumArithmeticRule, PrimitiveInterludeRule, LiteralDomainRule, MaskedComparisonRule,
            ControllerWordConversionRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeSwitchStatement, OperationKind.Switch);
        context.RegisterOperationAction(AnalyzeSwitchExpression, OperationKind.SwitchExpression);
        context.RegisterOperationAction(AnalyzeInterpolation, OperationKind.Interpolation);
        context.RegisterOperationAction(AnalyzeEnumConversion, OperationKind.Conversion);
        context.RegisterOperationAction(AnalyzeControllerWordConversion, OperationKind.Conversion);
        context.RegisterOperationAction(AnalyzeEnumStep, OperationKind.Increment, OperationKind.Decrement,
            OperationKind.CompoundAssignment);
        context.RegisterOperationBlockStartAction(AnalyzeCastOnlyParameters);
        context.RegisterOperationBlockStartAction(AnalyzeMaskedComparisons);
        context.RegisterCompilationStartAction(start => new CompilationDomainFlow().Register(start));
    }

    /// <summary>Flags an explicit closed-enum cast when its operand is arithmetic rather than a validated domain transition.</summary>
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
                "Casting arithmetic", conversion.Type.Name);
        }
    }

    /// <summary>
    /// Flags an explicit ControllerButton/integer conversion outside a [ButtonWordEncoding] declaration in the assembly that
    /// declares ControllerButton (Core); hosts, tools and tests reference it and convert at their own boundaries.
    /// </summary>
    private static void AnalyzeControllerWordConversion(OperationAnalysisContext context)
    {
        var conversion = (IConversionOperation)context.Operation;
        if (conversion.IsImplicit)
            return;
        IAssemblySymbol analyzed = context.Compilation.Assembly;
        bool toButtons = IsControllerButtons(conversion.Type, analyzed) && IsIntegral(conversion.Operand.Type);
        bool fromButtons = IsControllerButtons(conversion.Operand.Type, analyzed) && IsIntegral(conversion.Type);
        if (!toButtons && !fromButtons || InControllerWordBoundary(context.ContainingSymbol))
            return;
        Report(context, ControllerWordConversionRule, conversion.Syntax.GetLocation(),
            conversion.Syntax.ToString(), toButtons ? "from" : "to");
    }

    /// <summary>Whether a type is the controller button set declared by the compilation being analyzed.</summary>
    private static bool IsControllerButtons(ITypeSymbol? type, IAssemblySymbol analyzed) =>
        type is INamedTypeSymbol named && named.ToDisplayString() == ControllerButtonsType &&
        SymbolEqualityComparer.Default.Equals(named.ContainingAssembly, analyzed);

    /// <summary>Whether a declaration, its property or a containing type carries [ButtonWordEncoding].</summary>
    private static bool InControllerWordBoundary(ISymbol? symbol)
    {
        for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
        {
            if (HasControllerWordBoundary(current) ||
                current is IMethodSymbol { AssociatedSymbol: { } property } && HasControllerWordBoundary(property))
                return true;
        }
        return false;
    }

    /// <summary>Matches the boundary marker by its short attribute type name.</summary>
    private static bool HasControllerWordBoundary(ISymbol symbol) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == ControllerWordBoundaryAttributeName);

    /// <summary>Flags increment, decrement, or compound assignment on a closed enum.</summary>
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
                "Stepping an enum", target.Name);
    }

    /// <summary>Tracks every parameter reference within a method and reports primitive parameters used only as unvalidated enum casts.</summary>
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
        var gate = new object();
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

    /// <summary>
    /// A masked or shifted local compared or bit-tested against two or more distinct constants
    /// is a selector decoded by hand: report it like a switched selector.
    /// </summary>
    private static void AnalyzeMaskedComparisons(OperationBlockStartAnalysisContext context)
    {
        var tests = new Dictionary<ILocalSymbol, HashSet<object?>>(SymbolEqualityComparer.Default);
        var gate = new object();
        context.RegisterOperationAction(operationContext =>
        {
            var binary = (IBinaryOperation)operationContext.Operation;
            if (binary.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
                return;
            foreach ((IOperation subject, IOperation other) in new[] { new OperandPair(binary.LeftOperand, binary.RightOperand), new OperandPair(binary.RightOperand, binary.LeftOperand) })
            {
                // Only whole-value equality selects a member; bit tests of a masked position or
                // phase (parity, sub-tile bits) are arithmetic on a number.
                IOperation tested = Unwrap(subject);
                object? key = Unwrap(other).ConstantValue is { HasValue: true } constant ? constant.Value : null;
                // A plain zero test asks whether the field is empty, not which member it holds.
                if (tested is not ILocalReferenceOperation { Local: var local } || key is null || IsZero(key) ||
                    !IsIntegral(local.Type) || MaskExtraction(local) is null)
                    continue;
                lock (gate)
                {
                    if (!tests.TryGetValue(local, out HashSet<object?>? keys))
                        tests[local] = keys = [];
                    keys.Add(key);
                }
            }
        }, OperationKind.Binary);
        context.RegisterOperationBlockEndAction(endContext =>
        {
            foreach (KeyValuePair<ILocalSymbol, HashSet<object?>> entry in tests)
            {
                if (entry.Value.Count < 2)
                    continue;
                foreach (SyntaxReference reference in entry.Key.DeclaringSyntaxReferences)
                    endContext.ReportDiagnostic(Diagnostic.Create(MaskedComparisonRule, reference.GetSyntax().GetLocation(),
                        entry.Key.Name, MaskExtraction(entry.Key)));
            }
        });
    }

    /// <summary>Recognizes numeric zero tests, which indicate emptiness rather than a selector member.</summary>
    private static bool IsZero(object key) => NumericValue(key) == 0;

    /// <summary>The value of an integral or floating constant; null for strings, chars, booleans and other kinds.</summary>
    private static decimal? NumericValue(object value) => value switch
    {
        sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal =>
            Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture),
        _ => null,
    };

    /// <summary>Finds a local initializer's outer mask or right shift after removing parentheses and casts.</summary>
    private static string? MaskExtraction(ILocalSymbol local)
    {
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
            string? extraction = stripped.IsKind(SyntaxKind.BitwiseAndExpression) ? "mask"
                : stripped.IsKind(SyntaxKind.RightShiftExpression) || stripped.IsKind(SyntaxKind.UnsignedRightShiftExpression) ? "shift"
                : null;
            if (extraction is not null)
                return extraction;
        }
        return null;
    }

    /// <summary>Recognizes a cast passed to Enum.IsDefined as validation at a raw input boundary.</summary>
    private static bool FeedsIsDefined(IConversionOperation cast) =>
        cast.Parent is IArgumentOperation { Parent: IInvocationOperation invocation } &&
        invocation.TargetMethod.Name == "IsDefined" &&
        invocation.TargetMethod.ContainingType.SpecialType == SpecialType.System_Enum;

    /// <summary>Excludes implicit interface implementations whose parameter types are fixed by the contract.</summary>
    private static bool IsInterfaceImplementation(IMethodSymbol method) =>
        method.ContainingType.AllInterfaces.SelectMany(type => type.GetMembers().OfType<IMethodSymbol>())
            .Any(member => SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(member), method));

    /// <summary>Restricts enum diagnostics to repository-owned enums without composable flags semantics.</summary>
    private static bool IsClosedEnum([NotNullWhen(true)] ITypeSymbol? type) =>
        type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && IsOwned(enumType) && !IsFlags(enumType);

    /// <summary>Compilation-wide evidence for catalog comparisons and primitive interludes.</summary>
    private sealed class CompilationDomainFlow
    {
        /// <summary>Serializes evidence collected by concurrent operation callbacks.</summary>
        private readonly object _gate = new();
        /// <summary>Groups named-constant comparisons by storage symbol and declaring catalog.</summary>
        private readonly Dictionary<CatalogComparisonKey, CatalogComparison> _comparisons =
            new(new SubjectCatalogComparer());
        /// <summary>Records every storage write, including its narrowed enum, site, stepping, and constant value.</summary>
        private readonly Dictionary<ISymbol, List<DomainWrite>> _writes = new(SymbolEqualityComparer.Default);
        /// <summary>Collects literal equality values used to recognize small closed storage domains.</summary>
        private readonly Dictionary<ISymbol, HashSet<object>> _comparedConstants = new(SymbolEqualityComparer.Default);
        /// <summary>Tracks storage used as a switch selector.</summary>
        private readonly HashSet<ISymbol> _switched = new(SymbolEqualityComparer.Default);
        /// <summary>Records whether narrowed primitive storage is later compared or switched.</summary>
        private readonly Dictionary<ISymbol, string> _domainUses = new(SymbolEqualityComparer.Default);
        /// <summary>Excludes ordered quantities from identity-domain inference.</summary>
        private readonly HashSet<ISymbol> _ordered = new(SymbolEqualityComparer.Default);

        /// <summary>Installs evidence collectors and the compilation-end diagnostic pass.</summary>
        internal void Register(CompilationStartAnalysisContext context)
        {
            context.RegisterOperationAction(Comparison, OperationKind.Binary);
            context.RegisterOperationAction(IsPattern, OperationKind.IsPattern);
            context.RegisterOperationAction(Narrowing, OperationKind.VariableDeclarator, OperationKind.SimpleAssignment,
                OperationKind.CompoundAssignment, OperationKind.Increment, OperationKind.Decrement);
            context.RegisterOperationAction(SwitchUse, OperationKind.Switch, OperationKind.SwitchExpression);
            context.RegisterCompilationEndAction(End);
        }

        /// <summary>Collects equality against named or literal constants and excludes ordered quantities.</summary>
        private void Comparison(OperationAnalysisContext context)
        {
            var binary = (IBinaryOperation)context.Operation;
            IOperation left = Unwrap(binary.LeftOperand), right = Unwrap(binary.RightOperand);
            if (binary.OperatorKind is BinaryOperatorKind.LessThan or BinaryOperatorKind.LessThanOrEqual or
                BinaryOperatorKind.GreaterThan or BinaryOperatorKind.GreaterThanOrEqual)
            {
                lock (_gate)
                {
                    if (Subject(left) is { } l) _ordered.Add(l);
                    if (Subject(right) is { } r) _ordered.Add(r);
                }
                return;
            }
            if (binary.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
                return;
            Record(left, right);
            Record(right, left);
            RecordConstant(left, right);
            RecordConstant(right, left);
            MarkDomainUse(left, "compared");
            MarkDomainUse(right, "compared");
        }

        /// <summary>Collects constant patterns and marks their input as an identity comparison.</summary>
        private void IsPattern(OperationAnalysisContext context)
        {
            var pattern = (IIsPatternOperation)context.Operation;
            IOperation subject = Unwrap(pattern.Value);
            foreach (IOperation value in PatternValues(pattern.Pattern))
                Record(subject, Unwrap(value));
            MarkDomainUse(subject, "compared");
        }

        /// <summary>Marks switch inputs as domain uses and literal-domain candidates.</summary>
        private void SwitchUse(OperationAnalysisContext context)
        {
            IOperation value = context.Operation switch
            {
                ISwitchOperation statement => statement.Value,
                ISwitchExpressionOperation expression => expression.Value,
                _ => throw new InvalidOperationException("Unexpected switch operation."),
            };
            MarkDomainUse(Unwrap(value), "switched");
            if (Subject(Unwrap(value)) is { } switched)
                lock (_gate)
                    _switched.Add(switched);
        }

        /// <summary>Records a source-owned primitive compared with a named constant in an owned catalog.</summary>
        private void Record(IOperation subject, IOperation constant)
        {
            // Only repository-declared storage can be retyped; a library member such as Length
            // pools unrelated arrays and is never one domain.
            if (!IsIntegral(subject.Type) || Subject(subject) is not { } symbol ||
                !RepositoryOwnership.Owns(symbol))
                return;
            if (constant is not IFieldReferenceOperation { Field: { IsConst: true } field } ||
                !IsOwned(field.ContainingType) || field.ContainingType.TypeKind == TypeKind.Enum)
                return;
            lock (_gate)
            {
                var key = new CatalogComparisonKey(symbol, field.ContainingType);
                if (!_comparisons.TryGetValue(key, out var entry))
                    _comparisons[key] = entry = new CatalogComparison(new HashSet<string>(StringComparer.Ordinal), subject.Syntax.GetLocation());
                entry.Names.Add(field.Name);
            }
        }

        /// <summary>Collects numeric constant comparisons for literal-domain inference.</summary>
        private void RecordConstant(IOperation subject, IOperation other)
        {
            if (!IsIntegral(subject.Type) || Subject(subject) is not { } symbol ||
                other.ConstantValue is not { HasValue: true, Value: { } value } || NumericValue(value) is not { } number)
                return;
            lock (_gate)
            {
                if (!_comparedConstants.TryGetValue(symbol, out HashSet<object>? set))
                    _comparedConstants[symbol] = set = [];
                set.Add(number);
            }
        }

        /// <summary>Records writes and recognizes explicit narrowing from a closed enum to integral storage.</summary>
        private void Narrowing(OperationAnalysisContext context)
        {
            (ISymbol? target, IOperation? value) = context.Operation switch
            {
                IVariableDeclaratorOperation declarator => new NarrowingWrite(declarator.Symbol, declarator.Initializer?.Value),
                ISimpleAssignmentOperation assignment => new NarrowingWrite(Subject(assignment.Target), assignment.Value),
                ICompoundAssignmentOperation compound => new NarrowingWrite(Subject(compound.Target), null),
                IIncrementOrDecrementOperation step => new NarrowingWrite(Subject(step.Target), null),
                _ => new NarrowingWrite(null, null),
            };
            if (target is null || !IsIntegral(TypeOf(target)))
                return;
            if (context.Operation is IVariableDeclaratorOperation && value is null)
                return;
            ITypeSymbol? domain = value is IConversionOperation { IsImplicit: false } conversion &&
                IsClosedEnum(Unwrap(conversion.Operand).Type) ? Unwrap(conversion.Operand).Type : null;
            Location site = (value ?? context.Operation).Syntax.GetLocation();
            lock (_gate)
            {
                if (!_writes.TryGetValue(target, out var writes))
                    _writes[target] = writes = [];
                object? constantValue = value is not null && Unwrap(value).ConstantValue is { HasValue: true, Value: { } written }
                    ? NumericValue(written) : null;
                writes.Add(new DomainWrite(domain, site, context.Operation is ICompoundAssignmentOperation or IIncrementOrDecrementOperation,
                    constantValue));
            }
        }

        /// <summary>Reports nonlocal storage written only with a small literal set and used for identity selection.</summary>
        private void ReportLiteralDomain(CompilationAnalysisContext context, ISymbol symbol,
            List<DomainWrite> writes)
        {
            if (symbol is ILocalSymbol || !RepositoryOwnership.Owns(symbol) ||
                _ordered.Contains(symbol) || writes.Any(write => write.Stepped || write.Constant is null))
                return;
            var written = new HashSet<object>(writes.Select(write => write.Constant).OfType<object>());
            if (written.Count < 2)
                return;
            bool compared = _comparedConstants.TryGetValue(symbol, out HashSet<object>? constants) &&
                constants.Count >= 2 && constants.IsSubsetOf(written);
            if (!compared && !_switched.Contains(symbol))
                return;
            Location site = writes.Select(write => write.Site)
                .OrderBy(location => location.SourceTree?.FilePath, StringComparer.Ordinal)
                .ThenBy(location => location.SourceSpan.Start).First();
            context.ReportDiagnostic(Diagnostic.Create(LiteralDomainRule, site, symbol.Name,
                StoredType(symbol).ToDisplayString(), string.Join(", ", written.OrderBy(value => value))));
        }

        /// <summary>Excludes counters and accumulators modified by stepping from catalog-domain inference.</summary>
        private bool IsStepped(ISymbol symbol) =>
            _writes.TryGetValue(symbol, out var writes) && writes.Any(write => write.Domain is null && write.Stepped);

        /// <summary>Remembers the first comparison or switch use of integral storage.</summary>
        private void MarkDomainUse(IOperation operation, string how)
        {
            if (Subject(operation) is { } symbol && IsIntegral(operation.Type))
                lock (_gate)
                {
                    // The first use wins (netstandard2.0 has no Dictionary.TryAdd).
                    if (!_domainUses.ContainsKey(symbol))
                        _domainUses.Add(symbol, how);
                }
        }

        /// <summary>Reconciles compilation-wide evidence and reports catalog comparisons, literal domains, and primitive interludes.</summary>
        private void End(CompilationAnalysisContext context)
        {
            lock (_gate)
            {
                foreach (var entry in _comparisons)
                {
                    if (entry.Value.Names.Count < 2 || _ordered.Contains(entry.Key.Subject) || IsStepped(entry.Key.Subject))
                        continue;
                    context.ReportDiagnostic(Diagnostic.Create(CatalogComparisonRule, entry.Value.First,
                        entry.Key.Subject.Name, StoredType(entry.Key.Subject).ToDisplayString(), entry.Value.Names.Count,
                        entry.Key.Catalog.Name));
                }
                foreach (var entry in _writes)
                {
                    ReportLiteralDomain(context, entry.Key, entry.Value);
                    if (!_domainUses.TryGetValue(entry.Key, out string? how))
                        continue;
                    var narrowed = entry.Value
                        .SelectMany(write => write.Domain is { } domain
                            ? new[] { new DomainSite(Domain:domain, write.Site) }
                            : Array.Empty<DomainSite>())
                        .OrderBy(write => write.Site.SourceTree?.FilePath, StringComparer.Ordinal)
                        .ThenBy(write => write.Site.SourceSpan.Start).ToArray();
                    if (narrowed.Length == 0)
                        continue;
                    // A field or property is the domain's storage only when every write narrows that
                    // one domain; multipurpose storage (native slot words) is a raw boundary.
                    if (entry.Key is not ILocalSymbol && (narrowed.Length != entry.Value.Count ||
                        narrowed.Any(write => !SymbolEqualityComparer.Default.Equals(write.Domain, narrowed[0].Domain))))
                        continue;
                    context.ReportDiagnostic(Diagnostic.Create(PrimitiveInterludeRule, narrowed[0].Site,
                        entry.Key.Name, narrowed[0].Domain.Name, StoredType(entry.Key).ToDisplayString(), how));
                }
            }
        }

        /// <summary>Extracts the referenced local, parameter, mutable field, or property from an operation.</summary>
        private static ISymbol? Subject(IOperation operation) => operation switch
        {
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            IFieldReferenceOperation { Field.IsConst: false } field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => null,
        };

        /// <summary>The stored type of a tracked symbol; only locals, parameters, fields and properties are tracked.</summary>
        private static ITypeSymbol StoredType(ISymbol symbol) => TypeOf(symbol) ??
            throw new InvalidOperationException($"Tracked {symbol.Kind} {symbol.Name} has no stored type.");

        /// <summary>Reads the stored type of supported symbol kinds.</summary>
        private static ITypeSymbol? TypeOf(ISymbol symbol) => symbol switch
        {
            ILocalSymbol local => local.Type,
            IParameterSymbol parameter => parameter.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null,
        };

        /// <summary>Uses Roslyn symbol identity for both parts of a subject/catalog evidence key.</summary>
        private sealed class SubjectCatalogComparer : IEqualityComparer<CatalogComparisonKey>
        {
            /// <summary>Matches storage and catalog by semantic symbol identity.</summary>
            public bool Equals(CatalogComparisonKey x, CatalogComparisonKey y) =>
                SymbolEqualityComparer.Default.Equals(x.Subject, y.Subject) &&
                SymbolEqualityComparer.Default.Equals(x.Catalog, y.Catalog);

            /// <summary>Combines Roslyn symbol hashes for the storage/catalog pair.</summary>
            public int GetHashCode(CatalogComparisonKey value) =>
                (SymbolEqualityComparer.Default.GetHashCode(value.Subject) * 397) ^
                SymbolEqualityComparer.Default.GetHashCode(value.Catalog);
        }
    }

    /// <summary>Rejects enum interpolation formats beyond the four specifiers accepted by Enum.ToString.</summary>
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

    /// <summary>Checks primitive and masked selectors, then verifies that owned enum statements handle missing values loudly.</summary>
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

    /// <summary>Checks expression selectors while recognizing throwing discard arms as raw-boundary decoders.</summary>
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

    /// <summary>Reports integral switches with two or more named case values from one constant catalog.</summary>
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
            .Select(group => group.Key ?? throw new InvalidOperationException("A constant field always has a containing type."))
            .ToArray();
        if (catalogs.Length == 0)
            return;
        Report(context, PrimitiveSwitchRule, syntax.GetLocation(), Describe(value), value.Type.ToDisplayString(),
            string.Join(", ", catalogs.Select(catalog => catalog.Name)));
    }

    /// <summary>Reports a switched integral local initialized from a mask or right shift.</summary>
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
            string? extraction = stripped.IsKind(SyntaxKind.BitwiseAndExpression) ? "mask"
                : stripped.IsKind(SyntaxKind.RightShiftExpression) || stripped.IsKind(SyntaxKind.UnsignedRightShiftExpression) ? "shift"
                : null;
            if (extraction is not null)
                Report(context, MaskedSelectorRule, reference.GetSyntax().GetLocation(), local.Name, extraction);
        }
    }

    /// <summary>Extracts constant values from ordinary and pattern-based switch clauses.</summary>
    private static IEnumerable<IOperation> LabelValues(ICaseClauseOperation clause) => clause switch
    {
        ISingleValueCaseClauseOperation single => [single.Value],
        IPatternCaseClauseOperation pattern => PatternValues(pattern.Pattern),
        _ => [],
    };

    /// <summary>Walks constants through combined or negated switch patterns.</summary>
    private static IEnumerable<IOperation> PatternValues(IPatternOperation pattern) => pattern switch
    {
        IConstantPatternOperation constant => [constant.Value],
        IBinaryPatternOperation binary => PatternValues(binary.LeftPattern).Concat(PatternValues(binary.RightPattern)),
        INegatedPatternOperation negated => PatternValues(negated.Pattern),
        _ => [],
    };

    /// <summary>Recognizes numeric range patterns so they are not mistaken for closed selectors.</summary>
    private static bool HasRelational(IPatternOperation pattern) => pattern switch
    {
        IRelationalPatternOperation => true,
        IBinaryPatternOperation binary => HasRelational(binary.LeftPattern) || HasRelational(binary.RightPattern),
        INegatedPatternOperation negated => HasRelational(negated.Pattern),
        _ => false,
    };

    /// <summary>Checks whether switch labels cover every declared constant value of an enum.</summary>
    private static bool CoversAllMembers(INamedTypeSymbol enumType, IEnumerable<IOperation> labels)
    {
        var handled = new HashSet<object?>(labels.Select(label => Unwrap(label).ConstantValue)
            .Where(constant => constant.HasValue)
            .Select(constant => constant.Value));
        return enumType.GetMembers().OfType<IFieldSymbol>()
            .Where(field => field.HasConstantValue)
            .All(field => handled.Contains(field.ConstantValue));
    }

    /// <summary>Recognizes a statement default that ends by throwing, including a nested block.</summary>
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

    /// <summary>Points an enum fallthrough diagnostic at its default clause.</summary>
    private static Location DefaultLocation(ISwitchCaseOperation section) =>
        section.Clauses.First(clause => clause.CaseKind == CaseKind.Default).Syntax.GetLocation();

    /// <summary>Limits closed-domain diagnostics to types this repository declares, in any of its projects.</summary>
    private static bool IsOwned(INamedTypeSymbol type) => RepositoryOwnership.Owns(type);

    /// <summary>Excludes composable flags enums from exhaustive closed-enum handling.</summary>
    private static bool IsFlags(INamedTypeSymbol type) =>
        type.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");

    /// <summary>Accepts only the eight signed and unsigned integral primitive types as raw domain carriers.</summary>
    private static bool IsIntegral([NotNullWhen(true)] ITypeSymbol? type) => type?.SpecialType is
        SpecialType.System_Byte or SpecialType.System_SByte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64;

    /// <summary>Removes implicit conversions while retaining explicit casts that mark raw boundaries.</summary>
    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation { IsImplicit: true } conversion)
            operation = conversion.Operand;
        return operation;
    }

    /// <summary>Names a switch subject from its referenced symbol, falling back to source syntax.</summary>
    private static string Describe(IOperation value) => value switch
    {
        ILocalReferenceOperation local => local.Local.Name,
        IParameterReferenceOperation parameter => parameter.Parameter.Name,
        IFieldReferenceOperation field => field.Field.Name,
        IPropertyReferenceOperation property => property.Property.Name,
        _ => value.Syntax.ToString(),
    };

    /// <summary>Creates an operation diagnostic at the selected source location with descriptor arguments.</summary>
    private static void Report(OperationAnalysisContext context, DiagnosticDescriptor rule, Location location,
        params object[] arguments) =>
        context.ReportDiagnostic(Diagnostic.Create(rule, location, arguments));
}
