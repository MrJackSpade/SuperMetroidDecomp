using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Each thing has one identity named for what it is (SME6488, #2574). In <c>SuperMetroid.Core</c> and
/// <c>SuperMetroid.AssetExtraction</c> the rule reports an enum mirrored member for member by another: a switch expression
/// or a dictionary initializer that maps every member of one source enum to the member of the same name of another enum
/// with exactly the same members (<c>DoorSide.Left =&gt; CapSide.Left, DoorSide.Right =&gt; CapSide.Right</c> where both
/// are left and right), each member to its own. A narrower enum embedded into a wider one
/// (<c>HorizontalDirection.Left =&gt; SamusCollisionDirection.Left</c>, the two horizontal directions among the four
/// collision directions) or a wider one collapsed onto a narrower one converts between two domains and is not reported.
/// It also reports a member of a generated catalog (<c>&lt;auto-generated/&gt;</c>) named by joining
/// the names of two or more members of one other type, each of two words or more
/// (<c>EastAqueductQuicksandWestAqueductQuicksand</c>, joined from two rooms): a generator names a thing several users
/// share for what it is, not by listing them. A hand-written name built of two others (a charged Ice Spazer's trail, a
/// scene's asteroid) is composed, not a list of users.
/// </summary>
/// <remarks>
/// A mirrored enum is a second identity for the same things, and its map must be kept in step with the first. The fix is
/// one identity: the users name the shared enum, or the definition carries the member of the other it stands for. A map
/// onto members of other names (a pose to the picture it shows, a piece to its colours) is a definition carrying its
/// property, and two enums of the same member names that nothing maps onto each other are separate identities. Generated
/// code is read too: a generated catalog's names are its generator's, which names a shared entry for what it is.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IdentityDuplicationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A second identity for one thing, or a name joined from the names of its users.</summary>
    public const string IdentityDuplicationId = "SME6488";

    /// <summary>The import pipeline's assembly, which the rule reads beside Core.</summary>
    private const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The fewest members of an enum a mirror covers.</summary>
    private const int MirroredMembers = 2;

    /// <summary>The fewest words of each member name a joined name is made of.</summary>
    private const int JoinedPartWords = 2;

    private static readonly DiagnosticDescriptor MirrorRule = new(
        IdentityDuplicationId,
        "A thing has two identities",
        "{0}; name the one identity and let the users name it, or let the definition carry the member it stands for",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each thing has one identity named for what it is (#2574): an enum that mirrors another member for " +
            "member, the same members mapped each to its own, is a second identity for the same things, kept in step by a " +
            "map that restates them. A narrower enum embedded into a wider one is a conversion, not a mirror.");

    private static readonly DiagnosticDescriptor DeclarationRule = new(
        IdentityDuplicationId,
        "A name joined from its users",
        "{0}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each thing has one identity named for what it is (#2574): a thing several users share is named for " +
            "what it is, not by joining their names.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MirrorRule, DeclarationRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is not (CoreLayerAnalyzer.CoreAssembly or ExtractionAssembly))
                return;
            var types = new ConcurrentBag<INamedTypeSymbol>();
            start.RegisterSymbolAction(symbol => types.Add((INamedTypeSymbol)symbol.Symbol), SymbolKind.NamedType);
            start.RegisterOperationAction(AnalyzeSwitch, OperationKind.SwitchExpression);
            start.RegisterOperationAction(AnalyzeInitializer, OperationKind.ObjectOrCollectionInitializer);
            start.RegisterCompilationEndAction(end =>
            {
                ReportJoinedNames(end, [.. types.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)]);
            });
        });
    }

    /// <summary>Reports a switch expression whose arms map one enum's members onto another's.</summary>
    private static void AnalyzeSwitch(OperationAnalysisContext context)
    {
        var expression = (ISwitchExpressionOperation)context.Operation;
        var entries = new List<(IFieldSymbol[] Keys, IFieldSymbol Value)>();
        foreach (ISwitchExpressionArmOperation arm in expression.Arms)
        {
            if (arm.Pattern is IDiscardPatternOperation)
                continue;
            var keys = new List<IFieldSymbol>();
            if (arm.Guard is not null || !CollectKeys(arm.Pattern, keys) || EnumMember(arm.Value) is not { } value)
                return;
            entries.Add(([.. keys], value));
        }
        ReportMirror(context, expression.Syntax, entries, "The switch");
    }

    /// <summary>Reports a dictionary initializer whose entries map one enum's members onto another's.</summary>
    private static void AnalyzeInitializer(OperationAnalysisContext context)
    {
        var initializer = (IObjectOrCollectionInitializerOperation)context.Operation;
        var entries = new List<(IFieldSymbol[] Keys, IFieldSymbol Value)>();
        foreach (IOperation entry in initializer.Initializers)
        {
            IFieldSymbol? key;
            IFieldSymbol? value;
            switch (entry)
            {
                case ISimpleAssignmentOperation { Target: IPropertyReferenceOperation { Arguments.Length: 1 } indexer } assignment:
                    key = EnumMember(indexer.Arguments[0].Value);
                    value = EnumMember(assignment.Value);
                    break;
                case IInvocationOperation { Arguments.Length: 2 } add:
                    key = EnumMember(add.Arguments[0].Value);
                    value = EnumMember(add.Arguments[1].Value);
                    break;
                default:
                    return;
            }
            if (key is null || value is null)
                return;
            entries.Add(([key], value));
        }
        ReportMirror(context, initializer.Syntax, entries, "The initializer");
    }

    /// <summary>
    /// Reports <paramref name="syntax"/> when <paramref name="entries"/> map every member of one source enum to the member of
    /// the same name of another that has exactly the same members, each member to its own. A narrower enum's members
    /// embedded into a wider one (a horizontal direction into the four collision directions) is a conversion between two
    /// domains, not a mirror, and so is a wider enum's members collapsed onto a narrower one's.
    /// </summary>
    private static void ReportMirror(OperationAnalysisContext context, SyntaxNode syntax, List<(IFieldSymbol[] Keys, IFieldSymbol Value)> entries, string what)
    {
        if (entries.Count < MirroredMembers)
            return;
        INamedTypeSymbol keyType = entries[0].Keys[0].ContainingType;
        INamedTypeSymbol valueType = entries[0].Value.ContainingType;
        if (SymbolEqualityComparer.Default.Equals(keyType, valueType) || !IsIdentityEnum(keyType) || !IsIdentityEnum(valueType) ||
            entries.Any(entry => !SymbolEqualityComparer.Default.Equals(entry.Value.ContainingType, valueType) ||
                entry.Keys.Any(key => !SymbolEqualityComparer.Default.Equals(key.ContainingType, keyType))))
            return;
        var keyMembers = new HashSet<string>(Members(keyType), StringComparer.Ordinal);
        if (!keyMembers.SetEquals(Members(valueType)))
            return;
        string[] keyNames = [.. entries.SelectMany(entry => entry.Keys).Select(key => key.Name)];
        bool eachToItsOwn = entries.All(entry => entry.Keys.Length == 1 && string.Equals(entry.Keys[0].Name, entry.Value.Name, StringComparison.Ordinal));
        bool everyKeyOnce = keyNames.Distinct(StringComparer.Ordinal).Count() == keyNames.Length && keyMembers.SetEquals(keyNames);
        if (!eachToItsOwn || !everyKeyOnce)
            return;
        context.ReportDiagnostic(Diagnostic.Create(MirrorRule, syntax.GetLocation(),
            $"{what} maps every member of {keyType.Name} to the member of {valueType.Name} of the same name: one of them mirrors the other member for member"));
    }

    /// <summary>Reports each member named by joining the names of two or more members of one type.</summary>
    private static void ReportJoinedNames(CompilationAnalysisContext context, INamedTypeSymbol[] types)
    {
        var owners = new Dictionary<string, HashSet<INamedTypeSymbol>>(StringComparer.Ordinal);
        var names = new Dictionary<INamedTypeSymbol, HashSet<string>>(SymbolEqualityComparer.Default);
        foreach (INamedTypeSymbol type in types)
        {
            var own = new HashSet<string>(StringComparer.Ordinal);
            foreach (ISymbol member in NamedMembers(type))
            {
                own.Add(member.Name);
                if (!owners.TryGetValue(member.Name, out HashSet<INamedTypeSymbol>? holding))
                    owners[member.Name] = holding = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                holding.Add(type);
            }
            names[type] = own;
        }
        foreach (INamedTypeSymbol type in types)
        {
            foreach (ISymbol member in NamedMembers(type).Where(IsGenerated))
            {
                if (JoinedFrom(member.Name, type, owners, names) is not { } joined)
                    continue;
                foreach (Location location in member.Locations.Where(location => location.IsInSource))
                    context.ReportDiagnostic(Diagnostic.Create(DeclarationRule, location,
                        $"'{member.Name}' joins the names of {joined.Owner.Name}'s members {string.Join(" and ", joined.Parts)}: a thing they share is named for what it is, not by listing its users"));
            }
        }
    }

    /// <summary>
    /// The type other than <paramref name="declaring"/> whose members' names, each of two words or more, join into
    /// <paramref name="name"/>, and those names; a type's own combined member (a reach spanning two of its others) is its own.
    /// </summary>
    private static (INamedTypeSymbol Owner, IReadOnlyList<string> Parts)? JoinedFrom(string name, INamedTypeSymbol declaring,
        Dictionary<string, HashSet<INamedTypeSymbol>> owners, Dictionary<INamedTypeSymbol, HashSet<string>> names)
    {
        var candidates = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        for (int length = 1; length < name.Length; length++)
        {
            if (owners.TryGetValue(name.Substring(0, length), out HashSet<INamedTypeSymbol>? holding))
                candidates.UnionWith(holding);
        }
        candidates.Remove(declaring);
        foreach (INamedTypeSymbol owner in candidates)
        {
            if (Split(name, names[owner]) is { } parts)
                return (owner, parts);
        }
        return null;
    }

    /// <summary>
    /// <paramref name="name"/> split into two or more of <paramref name="parts"/>, each of two words or more and none the
    /// whole name, or null.
    /// </summary>
    private static IReadOnlyList<string>? Split(string name, HashSet<string> parts)
    {
        var reached = new List<string>?[name.Length + 1];
        reached[0] = [];
        for (int start = 0; start < name.Length; start++)
        {
            if (reached[start] is not { } before || !char.IsUpper(name[start]))
                continue;
            for (int end = start + 1; end <= name.Length; end++)
            {
                if (reached[end] is not null || (end < name.Length && !char.IsUpper(name[end])))
                    continue;
                string part = name.Substring(start, end - start);
                if (part.Length == name.Length || Words(part) < JoinedPartWords || !parts.Contains(part))
                    continue;
                reached[end] = [.. before, part];
            }
        }
        return reached[name.Length];
    }

    /// <summary>Whether <paramref name="member"/> is declared in a generated file, one opening with an <c>&lt;auto-generated/&gt;</c> comment.</summary>
    private static bool IsGenerated(ISymbol member) => member.Locations.Any(location => location.SourceTree is { } tree &&
        tree.GetText().Lines.Count > 0 && tree.GetText().Lines[0].ToString().Contains("<auto-generated"));

    /// <summary>The words of a PascalCase <paramref name="name"/>: its capital letters.</summary>
    private static int Words(string name) => name.Count(char.IsUpper);

    /// <summary>The static fields and properties, and the enum members, <paramref name="type"/> declares in source.</summary>
    private static IEnumerable<ISymbol> NamedMembers(INamedTypeSymbol type) =>
        type.GetMembers().Where(member => !member.IsImplicitlyDeclared && member.Locations.Any(location => location.IsInSource) &&
            (member is IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true, IsIndexer: false } ||
                member is IFieldSymbol && type.TypeKind == TypeKind.Enum));

    /// <summary>Whether <paramref name="type"/> is a source enum that is not <c>[Flags]</c>.</summary>
    private static bool IsIdentityEnum(INamedTypeSymbol type) =>
        type.TypeKind == TypeKind.Enum && type.Locations.Any(location => location.IsInSource) &&
        !type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "FlagsAttribute");

    /// <summary>The names of <paramref name="type"/>'s members.</summary>
    private static string[] Members(INamedTypeSymbol type) =>
        [.. type.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue).Select(field => field.Name)];

    /// <summary>Collects the enum members a constant pattern names, through its <c>or</c> combinators; false for any other pattern.</summary>
    private static bool CollectKeys(IPatternOperation pattern, List<IFieldSymbol> keys)
    {
        switch (pattern)
        {
            case IBinaryPatternOperation { OperatorKind: BinaryOperatorKind.Or } either:
                return CollectKeys(either.LeftPattern, keys) && CollectKeys(either.RightPattern, keys);
            case IConstantPatternOperation constant when EnumMember(constant.Value) is { } member:
                keys.Add(member);
                return true;
            default:
                return false;
        }
    }

    /// <summary>The enum member <paramref name="operation"/> names directly, or null.</summary>
    private static IFieldSymbol? EnumMember(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        return operation is IFieldReferenceOperation { Field: { HasConstantValue: true, ContainingType.TypeKind: TypeKind.Enum } field } ? field : null;
    }
}
