using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Identity is semantic, never an address (#1402): in <c>SuperMetroid.Core</c>, no member, parameter or record component
/// carries a cartridge pointer or bank as a value (SME6440: its name ends in <c>Pointer</c> or <c>Pointers</c>, or is
/// <c>Bank</c>), and no dictionary or lookup is keyed by one (SME6441: a <c>ToDictionary</c>-style key selector that
/// returns such a value, or an integer-keyed dictionary indexed, probed or filled with one). The pointer-to-identity
/// mapping belongs to the import pipeline, decoded once; Core holds the semantic identity. The vocabulary rule (SME6438)
/// covers the words; this rule covers the shape of an address used as an identity.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AddressIdentityAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core member, parameter or record component carries a pointer or bank.</summary>
    public const string AddressValueId = "SME6440";

    /// <summary>A Core dictionary or lookup is keyed by a pointer or bank.</summary>
    public const string AddressKeyId = "SME6441";

    /// <summary>Methods that build a keyed collection from a key selector, their first lambda argument.</summary>
    private static readonly ImmutableHashSet<string> KeyedBuilders =
    [
        "ToDictionary", "ToFrozenDictionary", "ToImmutableDictionary", "ToImmutableSortedDictionary", "ToSortedDictionary",
        "ToLookup", "GroupBy", "ToHashSet", "ToFrozenSet", "DistinctBy",
    ];

    /// <summary>Dictionary members whose first argument is the key.</summary>
    private static readonly ImmutableHashSet<string> KeyedMembers =
        ["TryGetValue", "ContainsKey", "Add", "TryAdd", "Remove", "GetValueOrDefault", "GetValueRefOrNullRef", "Contains"];

    private static readonly DiagnosticDescriptor ValueRule = new(
        AddressValueId,
        "Core carries a cartridge pointer or bank",
        "'{0}' carries a cartridge {1}; give the thing it locates a semantic identity, decoded at import",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Core identifies artwork, frames, draws and programs semantically. A pointer or bank is the " +
            "cartridge's way of finding a thing; the import pipeline maps it to the thing's identity once (#1402).");

    private static readonly DiagnosticDescriptor KeyRule = new(
        AddressKeyId,
        "Core keys a lookup by a cartridge pointer or bank",
        "This lookup is keyed by '{0}', a cartridge {1}; key it by the semantic identity decoded at import",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A dictionary or lookup keyed by a cartridge pointer or bank is an address catalog inside the " +
            "simulation. Key it by the semantic identity the import pipeline decoded (#1402).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ValueRule, KeyRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            // A record's positional parameter is also its property; each location is reported once.
            var reported = new ConcurrentDictionary<Location, byte>();
            void Check(Action<Diagnostic> report, string name, Location location)
            {
                if (AddressKind(name) is { } kind && reported.TryAdd(location, 0))
                    report(Diagnostic.Create(ValueRule, location, name, kind));
            }
            start.RegisterSymbolAction(symbolContext => AnalyzeMember(symbolContext, Check),
                SymbolKind.Field, SymbolKind.Property, SymbolKind.Method);
            start.RegisterSyntaxNodeAction(nodeContext => AnalyzeParameter(nodeContext, Check), SyntaxKind.Parameter);
            start.RegisterSyntaxNodeAction(AnalyzeKeyedBuilder, SyntaxKind.InvocationExpression);
            start.RegisterSyntaxNodeAction(AnalyzeKeyedAccess, SyntaxKind.ElementAccessExpression, SyntaxKind.InvocationExpression);
        });
    }

    /// <summary>
    /// What a name says the value is: a <c>pointer</c> when it ends in <c>Pointer</c> or <c>Pointers</c> (which covers
    /// <c>NativePointer</c>), a <c>bank</c> when it is <c>Bank</c>; otherwise null.
    /// </summary>
    internal static string? AddressKind(string name)
    {
        string bare = name.TrimStart('_');
        if (bare.EndsWith("Pointer", StringComparison.OrdinalIgnoreCase) || bare.EndsWith("Pointers", StringComparison.OrdinalIgnoreCase))
            return "pointer";
        return string.Equals(bare, "Bank", StringComparison.OrdinalIgnoreCase) ? "bank" : null;
    }

    /// <summary>Reports a declared field, property or ordinary method; overrides and implementations of external members keep their names.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context, Action<Action<Diagnostic>, string, Location> check)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol.Locations.FirstOrDefault(location => location.IsInSource) is not { } location)
            return;
        if (symbol is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
            return;
        if (symbol.IsOverride || ImplementsExternal(symbol))
            return;
        check(context.ReportDiagnostic, symbol.Name, location);
    }

    /// <summary>Reports a declared parameter, including a record's positional parameter (its component).</summary>
    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context, Action<Action<Diagnostic>, string, Location> check)
    {
        var parameter = (ParameterSyntax)context.Node;
        if (parameter.Identifier.ValueText.Length == 0 ||
            context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken) is not { ContainingSymbol: IMethodSymbol method })
        {
            return;
        }
        if (method.IsOverride || ImplementsExternal(method))
            return;
        check(context.ReportDiagnostic, parameter.Identifier.ValueText, parameter.Identifier.GetLocation());
    }

    /// <summary>Reports a keyed-collection builder whose key selector returns a pointer or bank.</summary>
    private static void AnalyzeKeyedBuilder(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: var method } ||
            !KeyedBuilders.Contains(method) ||
            invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is not LambdaExpressionSyntax { ExpressionBody: { } body })
        {
            return;
        }
        if (NamedValue(body) is { } name && AddressKind(name) is { } kind)
            context.ReportDiagnostic(Diagnostic.Create(KeyRule, body.GetLocation(), name, kind));
    }

    /// <summary>Reports an integer-keyed dictionary or lookup indexed, probed or filled with a pointer or bank.</summary>
    private static void AnalyzeKeyedAccess(SyntaxNodeAnalysisContext context)
    {
        (ExpressionSyntax? receiver, ExpressionSyntax? key) = context.Node switch
        {
            ElementAccessExpressionSyntax access when access.ArgumentList.Arguments.Count == 1 =>
                (access.Expression, access.ArgumentList.Arguments[0].Expression),
            InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: var member } memberAccess,
                ArgumentList.Arguments: { Count: > 0 } arguments,
            } when KeyedMembers.Contains(member) => (memberAccess.Expression, arguments[0].Expression),
            _ => (null, null),
        };
        if (receiver is null || key is null || NamedValue(key) is not { } name || AddressKind(name) is not { } kind)
            return;
        if (context.SemanticModel.GetTypeInfo(receiver, context.CancellationToken).Type is { } type && HasIntegerKey(type))
            context.ReportDiagnostic(Diagnostic.Create(KeyRule, key.GetLocation(), name, kind));
    }

    /// <summary>The name an expression reads: an identifier, a member access's member, or either through a cast or parentheses.</summary>
    private static string? NamedValue(ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText,
        CastExpressionSyntax cast => NamedValue(cast.Expression),
        ParenthesizedExpressionSyntax parenthesized => NamedValue(parenthesized.Expression),
        _ => null,
    };

    /// <summary>Whether a type is, or implements, a dictionary, set or lookup whose key is an integer.</summary>
    private static bool HasIntegerKey(ITypeSymbol type)
    {
        foreach (INamedTypeSymbol candidate in type.AllInterfaces.Prepend(type as INamedTypeSymbol).OfType<INamedTypeSymbol>())
        {
            string name = candidate.OriginalDefinition.ToDisplayString();
            bool keyed = name.StartsWith("System.Collections.Generic.IReadOnlyDictionary<", StringComparison.Ordinal) ||
                name.StartsWith("System.Collections.Generic.IDictionary<", StringComparison.Ordinal) ||
                name.StartsWith("System.Linq.ILookup<", StringComparison.Ordinal) ||
                name.StartsWith("System.Collections.Generic.IReadOnlySet<", StringComparison.Ordinal) ||
                name.StartsWith("System.Collections.Generic.ISet<", StringComparison.Ordinal);
            if (keyed && candidate.TypeArguments.FirstOrDefault()?.SpecialType is
                SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or
                SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Whether a member implements an interface member declared outside this compilation, whose name it must keep.</summary>
    private static bool ImplementsExternal(ISymbol symbol)
    {
        INamedTypeSymbol? type = symbol.ContainingType;
        if (type is null)
            return false;
        foreach (INamedTypeSymbol contract in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(contract.ContainingAssembly, symbol.ContainingAssembly))
                continue;
            foreach (ISymbol member in contract.GetMembers())
            {
                if (SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member), symbol))
                    return true;
            }
        }
        return false;
    }
}
