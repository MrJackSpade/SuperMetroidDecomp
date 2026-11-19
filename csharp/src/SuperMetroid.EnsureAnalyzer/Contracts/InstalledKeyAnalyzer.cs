using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// An installed key is written out (SME6460, #2511, #2520): in <c>SuperMetroid.Core</c> and
/// <c>SuperMetroid.AssetExtraction</c>, no enum member's name is written into text that becomes an installed key, a
/// parameter, property, field or method result marked <c>[InstalledKey]</c> (<c>PictureShade.Effect</c>,
/// <c>ArtworkFrame.Name</c>, <c>SpriteSheet.Frame</c>'s name) or overriding a marked member, nor into a JSON document a
/// <c>Utf8JsonWriter</c> writes. An interpolation hole, a <c>ToString()</c>, a
/// concatenation, a <c>string.Concat</c>, <c>Format</c> or <c>Join</c> argument, an <c>Enum.GetName</c> or a
/// <c>nameof</c> of an enum member, or of a value typed <c>System.Enum</c>, spells the key by the identity's source name, so
/// renaming the member silently changes a
/// name on disk and every installation stops matching. The text is followed through interpolation, concatenation, string
/// transforms such as <c>ToLowerInvariant()</c>, conditional branches, switch-expression arms, a LINQ selector's result
/// and the collections built from it, and one local variable. A numeric format (<c>{kind:D}</c>) writes the value, not
/// the name, and is SME6450's read.
/// <para>
/// A name helper is followed too (#2544): text that becomes the result of an unmarked method, local function or
/// property of the same assembly is reported when any call of that helper hands its result to an installed key or a
/// collection's keys, directly, through a local, or through another helper's result. <c>string.Create</c>'s interpolated
/// text inside such a helper (<c>RoomBlockPictureNames.OfCollectible</c>'s) is therefore an installed key's.
/// </para>
/// <para>
/// #2548 widens both to every source name: a <c>nameof</c> of any member, a property, field, method or event, or of a type,
/// is the member's or the type's source name, and a name handed to a parameter of the same assembly is followed into
/// whatever that parameter's references reach, through the parameters it is handed on to; a record's positional
/// parameter reaches whatever its property's reads reach. A definition found again by its own name (a <c>Find(nameof(X))</c>
/// into a dictionary keyed by the definitions' names) is reported where the name is spelled.
/// </para>
/// <para>
/// Its sibling SME6462 reports an identity keyed or read by its members' names: an enum member's name made a
/// dictionary's or set's key (an <c>Add</c>, <c>TryGetValue</c>, <c>ContainsKey</c> or indexer key, or a
/// <c>ToDictionary</c> key selector), <c>Enum.Parse</c>, <c>Enum.TryParse</c> and <c>Enum.GetNames</c>, and
/// <c>JsonStringEnumConverter</c> in any form. Each spells documents by the identity's source names; the identity's
/// written-out key table (<c>InstalledKeys</c>, its JSON converter) is the one codec that reads and writes them.
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InstalledKeyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>An enum member's name is written into an installed key.</summary>
    public const string EnumNameInKeyId = "SME6460";

    /// <summary>An identity is keyed or read by its members' names.</summary>
    public const string EnumNameKeyedId = "SME6462";

    /// <summary>The attribute that marks installed-key text.</summary>
    public const string MarkerName = "SuperMetroid.Core.InstalledKeyAttribute";

    /// <summary>The import pipeline, which writes the installed keys the game reads.</summary>
    public const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The converter that spells every member of an enum by its source name.</summary>
    private const string NameConverter = "JsonStringEnumConverter";

    private static readonly DiagnosticDescriptor Rule = new(
        EnumNameInKeyId,
        "An enum member's name is written into an installed key",
        "'{0}' writes the name of {1} into the installed key '{2}'; write the key out, as a literal or in a " +
            "table of the identity's keys, so renaming the member never changes a name on disk",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An installed key is the name the import writes into the installation and the game looks content up by. " +
            "Spelling it from an enum member's source name ties the installation to an identifier: renaming the member " +
            "changes the key without a format change, and installed content stops matching. A nameof of any member or type, " +
            "and a name handed through a parameter, spell a key so too (#2548). Each key is written out, as " +
            "RidleyShades.BabyKeys, TorizoLook's key table and the identities' InstalledKeys tables do (#2409, #2472, #2511, #2520).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    private static readonly DiagnosticDescriptor KeyedRule = new(
        EnumNameKeyedId,
        "An identity is keyed or read by its members' names",
        "'{0}' {1}: it spells a '{2}' by its members' names; key and read the identity through its written-out key table " +
            "(InstalledKeys and its JSON converter), so renaming a member never changes a document",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A dictionary keyed by an enum's member names, Enum.Parse, Enum.TryParse, Enum.GetNames and " +
            "JsonStringEnumConverter read and write an identity by its source names, so renaming a member silently changes " +
            "the documents the import installs and the saves the game writes, or stops reading them. The identity's " +
            "InstalledKeys table is its one codec at the import and export boundary (#2520).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, KeyedRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is not (CoreLayerAnalyzer.CoreAssembly or ExtractionAssembly))
                return;
            INamedTypeSymbol? marker = start.Compilation.GetTypeByMetadataName(MarkerName);
            var helpers = new NameHelperFlows();
            IAssemblySymbol assembly = start.Compilation.Assembly;
            start.RegisterOperationBlockStartAction(block => AnalyzeBlock(block, marker, helpers, assembly));
            start.RegisterOperationAction(AnalyzeNameRead, OperationKind.Invocation, OperationKind.ObjectCreation, OperationKind.TypeOf);
            start.RegisterCompilationEndAction(end => helpers.Report(write => Report(end.ReportDiagnostic, write.Write, write.Key)));
        });
    }

    /// <summary>
    /// Reports each enum name a block writes into an installed key or a collection's keys, directly or through a local the
    /// block then hands to one, and records for <paramref name="helpers"/> each name the block makes its member's result and
    /// each call of a name helper of <paramref name="assembly"/> whose result reaches a key or the member's result.
    /// </summary>
    private static void AnalyzeBlock(OperationBlockStartAnalysisContext block, INamedTypeSymbol? marker, NameHelperFlows helpers,
        IAssemblySymbol assembly)
    {
        var written = new ConcurrentDictionary<ILocalSymbol, ConcurrentBag<EnumNameWrite>>(SymbolEqualityComparer.Default);
        var called = new ConcurrentDictionary<ILocalSymbol, ConcurrentBag<ISymbol>>(SymbolEqualityComparer.Default);
        var keyed = new ConcurrentDictionary<ILocalSymbol, KeyDestination>(SymbolEqualityComparer.Default);
        var returned = new ConcurrentDictionary<ILocalSymbol, ISymbol>(SymbolEqualityComparer.Default);
        var handed = new ConcurrentDictionary<ILocalSymbol, IParameterSymbol>(SymbolEqualityComparer.Default);
        block.RegisterOperationAction(operation =>
        {
            if (NameWrite(operation.Operation) is not { } write)
                return;
            KeyDestination? key = Destination(write.Text, operation.ContainingSymbol, marker, assembly, out ILocalSymbol? local, out ISymbol? result,
                out IParameterSymbol? parameter);
            if (key is { } destination)
                Report(operation.ReportDiagnostic, write, destination);
            else if (local is not null)
                written.GetOrAdd(local, _ => []).Add(write);
            else if (result is not null)
                helpers.Returns(result, write);
            else if (parameter is not null)
                helpers.PassesTo(parameter, write);
        }, OperationKind.Interpolation, OperationKind.InterpolatedStringAppendFormatted, OperationKind.Invocation,
            OperationKind.Binary, OperationKind.NameOf);
        block.RegisterOperationAction(operation =>
        {
            if (NameHelper(operation.Operation, assembly) is not { } helper)
                return;
            KeyDestination? key = Destination(operation.Operation, operation.ContainingSymbol, marker, assembly, out ILocalSymbol? local, out ISymbol? result,
                out IParameterSymbol? parameter);
            if (key is { } destination)
                helpers.Feeds(helper, destination);
            else if (local is not null)
                called.GetOrAdd(local, _ => []).Add(helper);
            else if (result is not null)
                helpers.ReturnsResultOf(result, helper);
            else if (parameter is not null)
                helpers.HandsOn(helper, parameter);
        }, OperationKind.Invocation, OperationKind.PropertyReference);
        block.RegisterOperationAction(operation =>
        {
            var reference = (ILocalReferenceOperation)operation.Operation;
            if (Destination(reference, operation.ContainingSymbol, marker, assembly, out _, out ISymbol? result, out IParameterSymbol? parameter) is { } key)
                keyed.TryAdd(reference.Local, key);
            else if (result is not null)
                returned.TryAdd(reference.Local, result);
            else if (parameter is not null)
                handed.TryAdd(reference.Local, parameter);
        }, OperationKind.LocalReference);
        block.RegisterOperationAction(operation =>
        {
            // A parameter's value reaches what its references reach: a key, or a parameter it is handed on to. A helper returning
            // its parameter, such as Ensure.NotNull, is not followed: its result reaches a key at some calls and not others.
            var reference = (IParameterReferenceOperation)operation.Operation;
            if (!SymbolEqualityComparer.Default.Equals(reference.Parameter.ContainingAssembly, assembly))
                return;
            KeyDestination? key = Destination(reference, operation.ContainingSymbol, marker, assembly, out _, out _,
                out IParameterSymbol? parameter);
            if (key is { } destination)
                helpers.Feeds(reference.Parameter.OriginalDefinition, destination);
            else if (parameter is not null)
                helpers.HandsOn(reference.Parameter, parameter);
        }, OperationKind.ParameterReference);
        block.RegisterOperationBlockEndAction(end =>
        {
            foreach (KeyValuePair<ILocalSymbol, IParameterSymbol> entry in handed)
            {
                if (written.TryGetValue(entry.Key, out ConcurrentBag<EnumNameWrite>? writes))
                {
                    foreach (EnumNameWrite write in writes)
                        helpers.PassesTo(entry.Value, write);
                }
                if (called.TryGetValue(entry.Key, out ConcurrentBag<ISymbol>? feeding))
                {
                    foreach (ISymbol helper in feeding)
                        helpers.HandsOn(helper, entry.Value);
                }
            }
            foreach (KeyValuePair<ILocalSymbol, KeyDestination> entry in keyed)
            {
                if (written.TryGetValue(entry.Key, out ConcurrentBag<EnumNameWrite>? writes))
                {
                    foreach (EnumNameWrite write in writes)
                        Report(end.ReportDiagnostic, write, entry.Value);
                }
                if (called.TryGetValue(entry.Key, out ConcurrentBag<ISymbol>? feeding))
                {
                    foreach (ISymbol helper in feeding)
                        helpers.Feeds(helper, entry.Value);
                }
            }
            foreach (KeyValuePair<ILocalSymbol, ISymbol> entry in returned)
            {
                if (written.TryGetValue(entry.Key, out ConcurrentBag<EnumNameWrite>? writes))
                {
                    foreach (EnumNameWrite write in writes)
                        helpers.Returns(entry.Value, write);
                }
                if (called.TryGetValue(entry.Key, out ConcurrentBag<ISymbol>? inner))
                {
                    foreach (ISymbol helper in inner)
                        helpers.ReturnsResultOf(entry.Value, helper);
                }
            }
        });
    }

    /// <summary>
    /// The name helper an operation calls or reads: a method, local function or property of <paramref name="assembly"/>
    /// whose result is text; null for any other operation.
    /// </summary>
    private static ISymbol? NameHelper(IOperation operation, IAssemblySymbol assembly)
    {
        ISymbol? helper = operation switch
        {
            IInvocationOperation { TargetMethod: { ReturnType.SpecialType: SpecialType.System_String } method } => method.OriginalDefinition,
            IPropertyReferenceOperation { Property: { Type.SpecialType: SpecialType.System_String, IsIndexer: false } property } => property.OriginalDefinition,
            _ => null,
        };
        return helper is not null && SymbolEqualityComparer.Default.Equals(helper.ContainingAssembly, assembly) ? helper : null;
    }

    /// <summary>Reports one enum name written into the installed key or collection keys <paramref name="key"/>.</summary>
    private static void Report(Action<Diagnostic> report, EnumNameWrite write, KeyDestination key) =>
        report(key.IsCollection
            ? Diagnostic.Create(KeyedRule, write.Text.Syntax.GetLocation(), write.Text.Syntax.ToString(), "keys the collection " + key.Name, write.Enum.Name)
            : Diagnostic.Create(Rule, write.Text.Syntax.GetLocation(), write.Text.Syntax.ToString(), write.Named, key.Name));

    /// <summary>
    /// Reports <c>Enum.Parse</c>, <c>Enum.TryParse</c>, <c>Enum.GetNames</c> and each construction or <c>typeof</c> of a
    /// <c>JsonStringEnumConverter</c>, an attribute's <c>typeof</c> among them.
    /// </summary>
    private static void AnalyzeNameRead(OperationAnalysisContext context)
    {
        IOperation operation = context.Operation;
        (string Verb, string Enum)? read = operation switch
        {
            IInvocationOperation invocation when invocation.TargetMethod.ContainingType.SpecialType == SpecialType.System_Enum &&
                invocation.TargetMethod.Name is "Parse" or "TryParse" or "GetNames" =>
                (invocation.TargetMethod.Name == "GetNames" ? "lists an identity's names" : "reads text", ParsedName(invocation)),
            IObjectCreationOperation creation when IsNameConverter(creation.Type) => ("converts JSON", ConvertedName(creation.Type)),
            ITypeOfOperation typeOf when IsNameConverter(typeOf.TypeOperand) => ("converts JSON", ConvertedName(typeOf.TypeOperand)),
            _ => null,
        };
        if (read is { } found)
        {
            context.ReportDiagnostic(Diagnostic.Create(KeyedRule, operation.Syntax.GetLocation(), operation.Syntax.ToString(),
                found.Verb, found.Enum));
        }
    }

    /// <summary>The enum an <c>Enum</c> name call reads or lists, or "enum" when the call does not say.</summary>
    private static string ParsedName(IInvocationOperation invocation) =>
        (invocation.TargetMethod.TypeArguments.FirstOrDefault() ??
            invocation.Arguments.Select(argument => EnumOf(Unconverted(argument.Value).Type)).FirstOrDefault(type => type is not null))?.Name
        ?? "enum";

    /// <summary>Whether a type is <c>JsonStringEnumConverter</c>, its generic form or a type derived from either.</summary>
    private static bool IsNameConverter(ITypeSymbol? type)
    {
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (current.Name == NameConverter && current.ContainingNamespace?.ToDisplayString() == "System.Text.Json.Serialization")
                return true;
        }
        return false;
    }

    /// <summary>The enum a name converter converts, or "enum" for the converter of every enum.</summary>
    private static string ConvertedName(ITypeSymbol? converter) =>
        converter is INamedTypeSymbol { TypeArguments.Length: > 0 } generic ? generic.TypeArguments[0].Name : "enum";

    /// <summary>The source name an operation writes into text, or null when it writes none.</summary>
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
        INameOfOperation name => NameOfWrite(name),
        _ => null,
    };

    /// <summary>
    /// The source name a <c>nameof</c> writes (#2548): an enum member's, any other member's (a property, field, method or
    /// event of a type), or a type's own; null for a local, a parameter, a namespace or a type parameter, whose name is no
    /// identity's.
    /// </summary>
    private static EnumNameWrite? NameOfWrite(INameOfOperation name)
    {
        if (name.Argument is IFieldReferenceOperation { Field.ContainingType: { TypeKind: TypeKind.Enum } enumType })
            return new EnumNameWrite(name, enumType);
        if (name.SemanticModel is not { } model)
            return null;
        SymbolInfo info = model.GetSymbolInfo(name.Argument.Syntax);
        ISymbol? named = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
        return named switch
        {
            INamedTypeSymbol type => new EnumNameWrite(name, type, NamesType: true),
            IFieldSymbol or IPropertySymbol or IMethodSymbol or IEventSymbol when named.ContainingType is { } owner => new EnumNameWrite(name, owner),
            _ => null,
        };
    }

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
    /// The installed key or collection keys <paramref name="text"/> becomes part of, or null when it reaches none; a text
    /// first held in a local is given as <paramref name="local"/>, a text that becomes the result of an unmarked member, a
    /// name helper whose callers may hand it on to a key, as <paramref name="result"/>, and a text handed to an unmarked
    /// parameter of <paramref name="assembly"/>, whose references may hand it on to a key (#2548), as
    /// <paramref name="handed"/>.
    /// </summary>
    private static KeyDestination? Destination(IOperation text, ISymbol owner, INamedTypeSymbol? marker, IAssemblySymbol assembly,
        out ILocalSymbol? local, out ISymbol? result, out IParameterSymbol? handed)
    {
        local = null;
        result = null;
        handed = null;
        IOperation current = text;
        IOperation? parent = current.Parent;
        while (parent is not null)
        {
            switch (parent)
            {
                case IConversionOperation or IParenthesizedOperation or IInterpolationOperation or IInterpolatedStringOperation or
                    IInterpolatedStringAppendOperation or IInterpolatedStringHandlerCreationOperation or ICoalesceOperation or
                    ISpreadOperation or ICollectionExpressionOperation or IArrayInitializerOperation or IArrayCreationOperation:
                case IBinaryOperation { OperatorKind: BinaryOperatorKind.Add, Type.SpecialType: SpecialType.System_String }:
                case IConditionalOperation conditional when !ReferenceEquals(conditional.Condition, current):
                case ISwitchExpressionArmOperation arm when ReferenceEquals(arm.Value, current):
                case ISwitchExpressionOperation switched when !ReferenceEquals(switched.Value, current):
                case IArgumentOperation { Parent: IInvocationOperation { TargetMethod: var building } } when IsStringBuilding(building):
                case IInvocationOperation invocation when IsStringBuilding(invocation.TargetMethod) || IsStringTransform(invocation, current) ||
                    (ReferenceEquals(invocation.Instance, current) && IsGathering(invocation.TargetMethod)):
                    break;
                case IArgumentOperation { Parent: IInvocationOperation gathering } argument when
                    argument.Parameter is { Name: "source" or "first" or "second" or "span" } && IsGathering(gathering.TargetMethod):
                    current = gathering;
                    parent = gathering.Parent;
                    continue;
                case IArgumentOperation argument when IsCollectionKey(argument):
                    return ReceiverKey(argument.Parent switch
                    {
                        IInvocationOperation call => call.Instance,
                        IPropertyReferenceOperation indexer => indexer.Instance,
                        _ => null,
                    }, marker);
                case IArgumentOperation { Parameter: { } parameter } argument:
                    if (Writes(argument) && argument.Parent is IInvocationOperation writer)
                        return new KeyDestination(writer.TargetMethod.Name, false);
                    if (IsKey(parameter, marker))
                        return new KeyDestination(parameter.Name, false);
                    if (SymbolEqualityComparer.Default.Equals(parameter.ContainingAssembly, assembly))
                        handed = parameter.OriginalDefinition;
                    return null;
                case IReturnOperation:
                    if (SelectorOf(parent) is { } selector)
                    {
                        if (selector.MakesKeys)
                            return new KeyDestination("of '" + selector.Invocation.Syntax + "'", true);
                        current = selector.Invocation;
                        parent = selector.Invocation.Parent;
                        continue;
                    }
                    if (ReturningMember(parent, owner) is not { } member)
                        return null;
                    if (IsKey(member, marker))
                        return new KeyDestination(member.Name, false);
                    result = member.OriginalDefinition;
                    return null;
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
            current = parent;
            parent = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// The call a return of a selector lambda hands its result to, and whether the call makes the results keys (a key
    /// selector) rather than elements (a selector): LINQ's, the immutable and frozen collections' and the repository's own
    /// span queries alike, each of which names its selectors so.
    /// </summary>
    private static (IInvocationOperation Invocation, bool MakesKeys)? SelectorOf(IOperation returned)
    {
        IOperation? ancestor = returned.Parent;
        while (ancestor is IBlockOperation)
            ancestor = ancestor.Parent;
        if (ancestor is not IAnonymousFunctionOperation lambda || lambda.Parent is not IDelegateCreationOperation creation ||
            creation.Parent is not IArgumentOperation { Parameter: { } parameter, Parent: IInvocationOperation call })
            return null;
        return parameter.Name switch
        {
            "keySelector" => (call, true),
            "selector" => (call, false),
            _ => null,
        };
    }

    /// <summary>
    /// Whether a call gathers the elements of the collection it is handed into another collection or keeps them, such as
    /// <c>ToArray</c>, <c>ToHashSet</c>, <c>Prepend</c> or <c>Concat</c>.
    /// </summary>
    private static bool IsGathering(IMethodSymbol method) =>
        IsCollectionApi(method) && method.Name is "ToArray" or "ToList" or "ToHashSet" or "ToFrozenSet" or "ToImmutableArray" or "ToImmutableHashSet" or
            "Prepend" or "Append" or "Concat" or "Distinct" or "Order" or "OrderBy" or "Where" or "WhereToArray" or "ConcatToArray";

    /// <summary>Whether a method is a LINQ, immutable or frozen collection extension, or one of the repository's span queries.</summary>
    private static bool IsCollectionApi(IMethodSymbol method)
    {
        string space = method.ContainingNamespace?.ToDisplayString() ?? "";
        return space is "System.Linq" or "System.Collections.Immutable" or "System.Collections.Frozen" ||
            method.ContainingType.Name is "ReadOnlySpanQueries" or "ImportSpanQueries";
    }

    /// <summary>
    /// Whether an argument is a key or element of the collection the call or indexer reads or writes: a <c>key</c> or
    /// <c>item</c> of an instance member of a collection.
    /// </summary>
    private static bool IsCollectionKey(IArgumentOperation argument)
    {
        if (argument.Parameter is not { Name: "key" or "item" } parameter)
            return false;
        ISymbol? member = argument.Parent switch
        {
            IInvocationOperation call => call.TargetMethod,
            IPropertyReferenceOperation indexer => indexer.Property,
            _ => null,
        };
        return member is { IsStatic: false } && member.ContainingType is { } type && IsCollection(type) && parameter.Type.SpecialType == SpecialType.System_String;
    }

    /// <summary>Whether a type is a collection: it, or an interface it implements, is an enumerable of something.</summary>
    private static bool IsCollection(INamedTypeSymbol type) =>
        type.AllInterfaces.Prepend(type).Any(contract => contract.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);

    /// <summary>The keys a collection key reaches: the marked member the collection is, else the collection's own keys.</summary>
    private static KeyDestination ReceiverKey(IOperation? receiver, INamedTypeSymbol? marker)
    {
        IOperation? collection = receiver is null ? null : Unconverted(receiver);
        ISymbol? member = collection switch
        {
            IFieldReferenceOperation field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            ILocalReferenceOperation reference => reference.Local,
            IParameterReferenceOperation reference => reference.Parameter,
            _ => null,
        };
        if (member is not null && IsKey(member, marker))
            return new KeyDestination(member.Name, false);
        return new KeyDestination("of '" + (collection?.Syntax.ToString() ?? "the collection") + "'", true);
    }

    /// <summary>
    /// Whether an argument is text a JSON document writer writes into the document it writes. A binary writer is not one:
    /// the import spells contents with it to tell pictures apart within one run, and installs none of it.
    /// </summary>
    private static bool Writes(IArgumentOperation argument) =>
        argument.Parent is IInvocationOperation { TargetMethod: { } method } &&
        method.ContainingNamespace?.ToDisplayString() == "System.Text.Json" && method.ContainingType.Name == "Utf8JsonWriter" &&
        argument.Parameter?.Type.SpecialType == SpecialType.System_String;

    /// <summary>The installed key an assignment writes, or null; a local it writes is given as <paramref name="local"/>.</summary>
    private static KeyDestination? Assigned(IOperation target, INamedTypeSymbol? marker, out ILocalSymbol? local)
    {
        local = (target as ILocalReferenceOperation)?.Local;
        ISymbol? member = target switch
        {
            IPropertyReferenceOperation { Arguments.Length: 0 } property => property.Property,
            IFieldReferenceOperation field => field.Field,
            _ => null,
        };
        return member is not null && IsKey(member, marker) ? new KeyDestination(member.Name, false) : null;
    }

    /// <summary>The installed key a field or property initializer writes, or null.</summary>
    private static KeyDestination? InitializedKey(ISymbolInitializerOperation initializer, INamedTypeSymbol? marker)
    {
        IEnumerable<ISymbol> members = initializer switch
        {
            IFieldInitializerOperation field => field.InitializedFields,
            IPropertyInitializerOperation property => property.InitializedProperties,
            _ => [],
        };
        return members.FirstOrDefault(member => IsKey(member, marker)) is { } key ? new KeyDestination(key.Name, false) : null;
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

    /// <summary>Whether a symbol is marked as installed-key text, or overrides a member that is.</summary>
    private static bool IsKey(ISymbol symbol, INamedTypeSymbol? marker)
    {
        if (marker is null)
            return false;
        for (ISymbol? current = symbol; current is not null; current = Overridden(current))
        {
            if (current.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker)))
                return true;
        }
        return false;
    }

    /// <summary>The member a member overrides, or null.</summary>
    private static ISymbol? Overridden(ISymbol symbol) => symbol switch
    {
        IPropertySymbol property => property.OverriddenProperty,
        IMethodSymbol method => method.OverriddenMethod,
        _ => null,
    };

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

    /// <summary>
    /// The enum a type is, or the enum a nullable type wraps, or <c>System.Enum</c> itself, whose value is some enum's
    /// member and whose text is that member's name; null for any other type.
    /// </summary>
    private static ITypeSymbol? EnumOf(ITypeSymbol? type) =>
        type is null ? null :
        type.TypeKind == TypeKind.Enum || type.SpecialType == SpecialType.System_Enum ? type :
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable &&
            nullable.TypeArguments[0] is { TypeKind: TypeKind.Enum } wrapped ? wrapped :
        null;
}
