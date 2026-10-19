using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Keeps room state inside the room scope (#1329). A type marked <c>[RoomLifetime]</c> holds one room's state: each
/// room load builds it, inside the room scope's construction or a type marked <c>[RoomScopeBoundary]</c> (or as part
/// of another room-lifetime type), and only room-lifetime types or a boundary keep it in a field or auto-property, so
/// no session owner caches a room across loads. Room state is never reset field by field: a method named as a room
/// reset is reported wherever it is declared.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RoomLifetimeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A room-lifetime type constructed outside the room scope's construction.</summary>
    public const string ConstructionId = "SME6409";

    /// <summary>A session type's field or auto-property of a room-lifetime type.</summary>
    public const string StoredRoomStateId = "SME6410";

    /// <summary>A method named as a room reset.</summary>
    public const string RoomResetId = "SME6411";

    /// <summary>The marker for room-lifetime types, matched by its short name.</summary>
    private const string RoomLifetimeAttribute = "RoomLifetimeAttribute";

    /// <summary>The marker for types that build or hold room scopes, matched by its short name.</summary>
    private const string BoundaryAttribute = "RoomScopeBoundaryAttribute";

    /// <summary>The method names of the field-by-field room resets the room scope replaced.</summary>
    private static readonly Regex RoomResetName = new(@"^(ResetForRoomLoad|Reset\w*RoomState|ResetForRoom\w*)$", RegexOptions.CultureInvariant);

    private static readonly DiagnosticDescriptor ConstructionRule = new(
        ConstructionId,
        "Room-lifetime type constructed outside the room scope",
        "'{0}' is room-lifetime state; construct it in the room scope's construction, a [RoomScopeBoundary] type or another [RoomLifetime] type, not in '{1}'",
        "Architecture",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Each room load builds its room-lifetime state as part of the room scope (#1329).");

    private static readonly DiagnosticDescriptor StoredRoomStateRule = new(
        StoredRoomStateId,
        "Session type stores room-lifetime state",
        "'{0}' stores room-lifetime '{1}' in a type that outlives the room; receive it as a parameter, or mark the type [RoomLifetime] when each room builds it",
        "Architecture",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only room-lifetime types and room-scope boundaries keep room state, so no owner caches a room across loads (#1329).");

    private static readonly DiagnosticDescriptor RoomResetRule = new(
        RoomResetId,
        "Room reset method",
        "'{0}' is named as a room reset; each room builds its state fresh with the room scope instead of resetting it",
        "Architecture",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Room state is built per room by the room scope; field-by-field room resets are not declared (#1329).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ConstructionRule, StoredRoomStateRule, RoomResetRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeCreation, OperationKind.ObjectCreation);
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    /// <summary>Reports a room-lifetime construction outside the room scope's construction.</summary>
    private static void AnalyzeCreation(OperationAnalysisContext context)
    {
        if (((IObjectCreationOperation)context.Operation).Type is not INamedTypeSymbol created || !IsRoomLifetime(created))
            return;
        if (InRoomContext(context.ContainingSymbol))
            return;
        INamedTypeSymbol? owner = context.ContainingSymbol.ContainingType ?? context.ContainingSymbol as INamedTypeSymbol;
        context.ReportDiagnostic(Diagnostic.Create(ConstructionRule, context.Operation.Syntax.GetLocation(),
            created.Name, owner?.Name ?? context.ContainingSymbol.Name));
    }

    /// <summary>Reports each field or auto-property of a session type whose type holds room-lifetime state.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (InRoomContext(type))
            return;
        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsConst || RoomLifetimeType(field.Type) is not { } room)
                continue;
            ISymbol reported = field.IsImplicitlyDeclared && field.AssociatedSymbol is { } property ? property : field;
            if (reported.IsImplicitlyDeclared || reported.Locations.Length == 0 || !reported.Locations[0].IsInSource)
                continue;
            context.ReportDiagnostic(Diagnostic.Create(StoredRoomStateRule, reported.Locations[0], reported.Name, room.Name));
        }
    }

    /// <summary>Reports a method named as a room reset.</summary>
    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (method.MethodKind is MethodKind.Ordinary && !method.IsImplicitlyDeclared && RoomResetName.IsMatch(method.Name))
            context.ReportDiagnostic(Diagnostic.Create(RoomResetRule, method.Locations[0], method.Name));
    }

    /// <summary>Whether <paramref name="symbol"/> lies in a room-lifetime type or a room-scope boundary.</summary>
    private static bool InRoomContext(ISymbol symbol)
    {
        for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
        {
            if (current is INamedTypeSymbol type && (IsRoomLifetime(type) || HasAttribute(type, BoundaryAttribute)))
                return true;
        }
        return false;
    }

    /// <summary>The room-lifetime type <paramref name="type"/> stores directly, as an array element or as a type argument.</summary>
    private static INamedTypeSymbol? RoomLifetimeType(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                return RoomLifetimeType(array.ElementType);
            case INamedTypeSymbol named:
                if (IsRoomLifetime(named))
                    return named;
                foreach (ITypeSymbol argument in named.TypeArguments)
                {
                    if (RoomLifetimeType(argument) is { } inner)
                        return inner;
                }
                return null;
            default:
                return null;
        }
    }

    /// <summary>Whether <paramref name="type"/> is marked room-lifetime.</summary>
    private static bool IsRoomLifetime(INamedTypeSymbol type) => HasAttribute(type.OriginalDefinition, RoomLifetimeAttribute);

    /// <summary>Matches an analyzer marker by its short attribute type name.</summary>
    private static bool HasAttribute(ISymbol symbol, string name) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == name);
}
