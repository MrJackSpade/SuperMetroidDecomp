using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// An installed key is written out (SME6460, #2511): in <c>SuperMetroid.Core</c> and <c>SuperMetroid.AssetExtraction</c>,
/// no enum member's name is written into text that becomes an installed key, a parameter, property, field or method result
/// marked <c>[InstalledKey]</c> (<c>PictureShade.Effect</c>, <c>ArtworkFrame.Name</c>). An interpolation hole, a
/// <c>ToString()</c>, a concatenation, a <c>string.Concat</c>, <c>Format</c> or <c>Join</c> argument, an
/// <c>Enum.GetName</c> or a <c>nameof</c> of an enum member spells the key by the identity's source name, so renaming the
/// member silently changes a name on disk and every installation stops matching. The text is followed through
/// interpolation, concatenation, string transforms such as <c>ToLowerInvariant()</c>, conditional branches, switch-expression
/// arms and one local variable. A numeric format (<c>{kind:D}</c>) writes the value, not the name, and is SME6450's read.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InstalledKeyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>An enum member's name is written into an installed key.</summary>
    public const string EnumNameInKeyId = "SME6460";

    /// <summary>The attribute that marks installed-key text.</summary>
    public const string MarkerName = "SuperMetroid.Core.InstalledKeyAttribute";

    /// <summary>The import pipeline, which writes the installed keys the game reads.</summary>
    public const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    private static readonly DiagnosticDescriptor Rule = new(
        EnumNameInKeyId,
        "An enum member's name is written into an installed key",
        "'{0}' writes the name of a '{1}' member into the installed key '{2}'; write the key out, as a literal or in a " +
            "table of the identity's keys, so renaming the member never changes a name on disk",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An installed key is the name the import writes into the installation and the game looks content up by. " +
            "Spelling it from an enum member's source name ties the installation to an identifier: renaming the member " +
            "changes the key without a format change, and installed content stops matching. Each key is written out, as " +
            "RidleyShades.BabyKeys and TorizoLook's key table do (#2409, #2472, #2511).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is not (CoreLayerAnalyzer.CoreAssembly or ExtractionAssembly) ||
                start.Compilation.GetTypeByMetadataName(MarkerName) is not { } marker)
            {
                return;
            }
            start.RegisterOperationBlockStartAction(block => AnalyzeBlock(block, marker));
        });
    }

    /// <summary>
    /// Reports each enum name a block writes into an installed key, directly or through a local the block then hands to
    /// one.
    /// </summary>
    private static void AnalyzeBlock(OperationBlockStartAnalysisContext block, INamedTypeSymbol marker)
    {
        var written = new ConcurrentDictionary<ILocalSymbol, ConcurrentBag<EnumNameWrite>>(SymbolEqualityComparer.Default);
        var keyed = new ConcurrentDictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        block.RegisterOperationAction(operation =>
        {
            if (NameWrite(operation.Operation) is not { } write)
                return;
            string? key = Destination(write.Text, operation.ContainingSymbol, marker, out ILocalSymbol? local);
            if (key is not null)
                Report(operation.ReportDiagnostic, write, key);
            else if (local is not null)
                written.GetOrAdd(local, _ => []).Add(write);
        }, OperationKind.Interpolation, OperationKind.InterpolatedStringAppendFormatted, OperationKind.Invocation,
            OperationKind.Binary, OperationKind.NameOf);
        block.RegisterOperationAction(operation =>
        {
            var reference = (ILocalReferenceOperation)operation.Operation;
            if (Destination(reference, operation.ContainingSymbol, marker, out _) is { } key)
                keyed.TryAdd(reference.Local, key);
        }, OperationKind.LocalReference);
        block.RegisterOperationBlockEndAction(end =>
        {
            foreach (KeyValuePair<ILocalSymbol, string> entry in keyed)
            {
                if (!written.TryGetValue(entry.Key, out ConcurrentBag<EnumNameWrite>? writes))
                    continue;
                foreach (EnumNameWrite write in writes)
                    Report(end.ReportDiagnostic, write, entry.Value);
            }
        });
    }

    /// <summary>Reports one enum name written into the installed key <paramref name="key"/>.</summary>
    private static void Report(Action<Diagnostic> report, EnumNameWrite write, string key) =>
        report(Diagnostic.Create(Rule, write.Text.Syntax.GetLocation(), write.Text.Syntax.ToString(), write.Enum.Name, key));

    /// <summary>The enum name an operation writes into text, or null when it writes none.</summary>
    private static EnumNameWrite? NameWrite(IOperation operation) => operation switch
    {
        IInterpolationOperation hole => NamedEnum(hole.Expression, hole.FormatString) is { } type ? new EnumNameWrite(hole, type) : null,
        IInterpolatedStringAppendOperation { AppendCall: IInvocationOperation append } formatted =>
            append.Arguments.FirstOrDefault() is { } value && NamedEnum(value.Value, FormatArgument(append)) is { } type
                ? new EnumNameWrite(formatted, type) : null,
        IInvocationOperation invocation => InvocationWrite(invocation),
        IBinaryOperation { OperatorKind: BinaryOperatorKind.Add, Type.SpecialType: SpecialType.System_String } concatenation =>
            (EnumOf(Unconverted(concatenation.LeftOperand).Type) ?? EnumOf(Unconverted(concatenation.RightOperand).Type)) is { } type
                ? new EnumNameWrite(concatenation, type) : null,
        INameOfOperation { Argument: IFieldReferenceOperation { Field.ContainingType: { TypeKind: TypeKind.Enum } type } } name =>
            new EnumNameWrite(name, type),
        _ => null,
    };

    /// <summary>
    /// The enum name a call writes: an enum's <c>ToString</c> without a numeric format, <c>Enum.GetName</c>, or an enum
    /// argument of <c>string.Concat</c>, <c>Format</c> or <c>Join</c>.
    /// </summary>
    private static EnumNameWrite? InvocationWrite(IInvocationOperation invocation)
    {
        IMethodSymbol method = invocation.TargetMethod;
        if (method.Name == "ToString" && invocation.Instance is { } instance && EnumOf(Unconverted(instance).Type) is { } shown)
        {
            IOperation? format = invocation.Arguments.Select(argument => argument.Value)
                .FirstOrDefault(value => value.Type?.SpecialType == SpecialType.System_String);
            return IsNumericFormat(format) ? null : new EnumNameWrite(invocation, shown);
        }
        if (method.Name == "GetName" && method.ContainingType.SpecialType == SpecialType.System_Enum)
        {
            ITypeSymbol? named = method.TypeArguments.FirstOrDefault() ??
                invocation.Arguments.Select(argument => EnumOf(Unconverted(argument.Value).Type)).FirstOrDefault(type => type is not null);
            return named is null ? null : new EnumNameWrite(invocation, named);
        }
        if (!IsStringJoining(method))
            return null;
        ITypeSymbol? joined = invocation.Arguments.SelectMany(argument => Joined(argument.Value))
            .Select(value => EnumOf(Unconverted(value).Type)).FirstOrDefault(type => type is not null);
        return joined is null ? null : new EnumNameWrite(invocation, joined);
    }

    /// <summary>The values an argument of a joining call hands it: a params array's or collection's elements, else itself.</summary>
    private static IEnumerable<IOperation> Joined(IOperation value) => Unconverted(value) switch
    {
        IArrayCreationOperation { Initializer: { } items } => items.ElementValues,
        ICollectionExpressionOperation collection => collection.Elements,
        _ => [value],
    };

    /// <summary>The enum a formatted value writes the name of, or null for a non-enum value or a numeric format.</summary>
    private static ITypeSymbol? NamedEnum(IOperation value, IOperation? format) =>
        IsNumericFormat(format) ? null : EnumOf(Unconverted(value).Type);

    /// <summary>The format argument of an interpolated string handler's <c>AppendFormatted</c> call, or null.</summary>
    private static IOperation? FormatArgument(IInvocationOperation append) =>
        append.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "format" && argument.ArgumentKind != ArgumentKind.DefaultValue)?.Value;

    /// <summary>Whether a format is a constant that writes an enum's number (D or X) rather than its name.</summary>
    private static bool IsNumericFormat(IOperation? format) =>
        format?.ConstantValue is { HasValue: true, Value: string text } && text.Length > 0 && text[0] is 'D' or 'd' or 'X' or 'x';

    /// <summary>
    /// The installed key <paramref name="text"/> becomes part of, or null when it reaches none; a text first held in a
    /// local is given as <paramref name="local"/>.
    /// </summary>
    private static string? Destination(IOperation text, ISymbol owner, INamedTypeSymbol marker, out ILocalSymbol? local)
    {
        local = null;
        IOperation current = text;
        for (IOperation? parent = current.Parent; parent is not null; current = parent, parent = parent.Parent)
        {
            switch (parent)
            {
                case IConversionOperation or IParenthesizedOperation or IInterpolationOperation or IInterpolatedStringOperation or
                    IInterpolatedStringAppendOperation or IInterpolatedStringHandlerCreationOperation or ICoalesceOperation:
                case IBinaryOperation { OperatorKind: BinaryOperatorKind.Add, Type.SpecialType: SpecialType.System_String }:
                case IConditionalOperation conditional when !ReferenceEquals(conditional.Condition, current):
                case ISwitchExpressionArmOperation arm when ReferenceEquals(arm.Value, current):
                case ISwitchExpressionOperation switched when !ReferenceEquals(switched.Value, current):
                case IArgumentOperation { Parent: IInvocationOperation { TargetMethod: var building } } when IsStringBuilding(building):
                case IInvocationOperation invocation when IsStringBuilding(invocation.TargetMethod) || IsStringTransform(invocation, current):
                    continue;
                case IArgumentOperation { Parameter: { } parameter }:
                    return IsKey(parameter, marker) ? parameter.Name : null;
                case IReturnOperation:
                    return ReturningMember(parent, owner) is { } member && IsKey(member, marker) ? member.Name : null;
                case ISimpleAssignmentOperation assignment when ReferenceEquals(assignment.Value, current):
                    return Assigned(assignment.Target, marker, out local);
                case IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator }:
                    local = declarator.Symbol;
                    return null;
                case ISymbolInitializerOperation initializer:
                    return InitializedKey(initializer, marker);
                default:
                    return null;
            }
        }
        return null;
    }

    /// <summary>The installed key an assignment writes, or null; a local it writes is given as <paramref name="local"/>.</summary>
    private static string? Assigned(IOperation target, INamedTypeSymbol marker, out ILocalSymbol? local)
    {
        local = (target as ILocalReferenceOperation)?.Local;
        ISymbol? member = target switch
        {
            IPropertyReferenceOperation property => property.Property,
            IFieldReferenceOperation field => field.Field,
            _ => null,
        };
        return member is not null && IsKey(member, marker) ? member.Name : null;
    }

    /// <summary>The installed key a field or property initializer writes, or null.</summary>
    private static string? InitializedKey(ISymbolInitializerOperation initializer, INamedTypeSymbol marker)
    {
        IEnumerable<ISymbol> members = initializer switch
        {
            IFieldInitializerOperation field => field.InitializedFields,
            IPropertyInitializerOperation property => property.InitializedProperties,
            _ => [],
        };
        return members.FirstOrDefault(member => IsKey(member, marker))?.Name;
    }

    /// <summary>
    /// The member whose result a return gives: the enclosing local function, the property a getter belongs to, or the
    /// method; null inside a lambda, whose result is not a member's.
    /// </summary>
    private static ISymbol? ReturningMember(IOperation returned, ISymbol owner)
    {
        for (IOperation? ancestor = returned.Parent; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ancestor is IAnonymousFunctionOperation)
                return null;
            if (ancestor is ILocalFunctionOperation function)
                return function.Symbol;
        }
        return owner is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } ? property : owner;
    }

    /// <summary>Whether a symbol is marked as installed-key text.</summary>
    private static bool IsKey(ISymbol symbol, INamedTypeSymbol marker) =>
        symbol.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker));

    /// <summary>Whether a method joins the values it is handed into a string: <c>string.Concat</c>, <c>Format</c> or <c>Join</c>.</summary>
    private static bool IsStringJoining(IMethodSymbol method) =>
        method.ContainingType.SpecialType == SpecialType.System_String && method.Name is "Concat" or "Format" or "Join";

    /// <summary>
    /// Whether a method's string holds the text it is handed: a joining call, <c>string.Create</c>, or an interpolated
    /// string handler's appends.
    /// </summary>
    private static bool IsStringBuilding(IMethodSymbol method) =>
        IsStringJoining(method) || (method.ContainingType.SpecialType == SpecialType.System_String && method.Name == "Create") ||
        method.ContainingType.Name == "DefaultInterpolatedStringHandler";

    /// <summary>Whether a call transforms the string it is called on into another string that still holds its text.</summary>
    private static bool IsStringTransform(IInvocationOperation invocation, IOperation current) =>
        ReferenceEquals(invocation.Instance, current) && invocation.TargetMethod.ContainingType.SpecialType == SpecialType.System_String &&
        invocation.TargetMethod.ReturnType.SpecialType == SpecialType.System_String;

    /// <summary>The operand beneath any implicit conversions, such as the boxing of an interpolated value.</summary>
    private static IOperation Unconverted(IOperation operand)
    {
        while (operand is IConversionOperation { IsImplicit: true } conversion)
            operand = conversion.Operand;
        return operand;
    }

    /// <summary>The enum a type is, or the enum a nullable type wraps, or null.</summary>
    private static ITypeSymbol? EnumOf(ITypeSymbol? type) =>
        type is null ? null :
        type.TypeKind == TypeKind.Enum ? type :
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable &&
            nullable.TypeArguments[0] is { TypeKind: TypeKind.Enum } wrapped ? wrapped :
        null;
}
