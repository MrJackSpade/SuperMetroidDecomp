using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1371: a helper pasted into every species or family that needs it, changed only by a type name or an error string,
/// has to be fixed once per copy, and the copies drift. In the Core assembly a method body of at least
/// <see cref="MinimumStatements"/> statements is reported when, after its references to its own declaring type's
/// members, every type argument and every string literal's contents are normalised, it is the same token sequence as
/// a method in a different type; each clone group is reported once and names every member. Two positional records with
/// the same two or more parameter names and types and no members of their own are one record; a one-field record is a
/// typed identity whose distinct type is its purpose. A copy that must stay separate says
/// so with <c>[IntendedDuplicate(reason)]</c> on the member.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StructuralCloneAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A method body is pasted into several types.</summary>
    public const string StructuralCloneId = "SME6419";

    /// <summary>Two records are the same record under different names.</summary>
    public const string CloneRecordId = "SME6420";

    /// <summary>The attribute that justifies one deliberate copy at its site.</summary>
    public const string IntendedDuplicateAttributeName = "IntendedDuplicateAttribute";

    /// <summary>The assembly whose methods and records are compared.</summary>
    public const string ComparedAssembly = "SuperMetroid.Core";

    /// <summary>
    /// The fewest positional parameters a compared record declares. A one-field record is a typed identity whose purpose
    /// is its distinct type, as each species' pose type is, not a repeated data shape.
    /// </summary>
    public const int MinimumRecordParameters = 2;

    /// <summary>The fewest statements, nested ones included and blocks excluded, a compared body holds.</summary>
    public const int MinimumStatements = 5;

    /// <summary>Reports a structural clone group.</summary>
    private static readonly DiagnosticDescriptor StructuralCloneRule = new(
        StructuralCloneId,
        "Method body pasted into several types",
        "{0} have the same body; write it once as a shared implementation and pass what differs as data",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "In SuperMetroid.Core, a method body of at least five statements (nested statements counted, blocks not) is " +
            "compared after normalising references to its declaring type's own members, every type argument and every string " +
            "literal's contents; equal bodies in different types are one shared implementation (#1371).");

    /// <summary>Reports a clone record group.</summary>
    private static readonly DiagnosticDescriptor CloneRecordRule = new(
        CloneRecordId,
        "Record duplicated under another name",
        "{0} declare the same positional parameters and nothing else; use one record type",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Positional records in SuperMetroid.Core with two or more parameters of the same names, types and attributes " +
            "and no members of their own are one record type; a one-field record is a typed identity and is not compared (#1371).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [StructuralCloneRule, CloneRecordRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != ComparedAssembly)
                return;
            var methods = new ConcurrentBag<(string Key, IMethodSymbol Method)>();
            var records = new ConcurrentBag<(string Key, INamedTypeSymbol Record)>();
            start.RegisterSyntaxNodeAction(node => CollectMethod(node, methods), SyntaxKind.MethodDeclaration);
            start.RegisterSyntaxNodeAction(node => CollectRecord(node, records), SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);
            start.RegisterCompilationEndAction(end =>
            {
                foreach (IGrouping<string, (string Key, IMethodSymbol Method)> group in methods.GroupBy(entry => entry.Key))
                {
                    var members = group.Select(entry => entry.Method).OrderBy(Order, StringComparer.Ordinal).ToList();
                    if (members.Select(member => member.ContainingType).Distinct(SymbolEqualityComparer.Default).Count() < 2)
                        continue;
                    Report(end, StructuralCloneRule, members.Cast<ISymbol>().ToList(), member => $"{member.ContainingType.Name}.{member.Name}");
                }
                foreach (IGrouping<string, (string Key, INamedTypeSymbol Record)> group in records.GroupBy(entry => entry.Key))
                {
                    var members = group.Select(entry => entry.Record).OrderBy(Order, StringComparer.Ordinal).ToList();
                    if (members.Count >= 2)
                        Report(end, CloneRecordRule, members.Cast<ISymbol>().ToList(), member => member.Name);
                }
            });
        });
    }

    /// <summary>Records a method's normalised body when it is large enough to compare and not justified.</summary>
    private static void CollectMethod(SyntaxNodeAnalysisContext context, ConcurrentBag<(string Key, IMethodSymbol Method)> methods)
    {
        var declaration = (MethodDeclarationSyntax)context.Node;
        SyntaxNode? body = (SyntaxNode?)declaration.Body ?? declaration.ExpressionBody?.Expression;
        if (body is null || body.DescendantNodesAndSelf().OfType<StatementSyntax>().Count(statement => statement is not BlockSyntax) < MinimumStatements)
            return;
        if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not { } method || IsJustified(method))
            return;
        methods.Add((Normalised(body, method.ContainingType, context.SemanticModel, context.CancellationToken), method));
    }

    /// <summary>Records a member-free positional record by its parameter names and types.</summary>
    private static void CollectRecord(SyntaxNodeAnalysisContext context, ConcurrentBag<(string Key, INamedTypeSymbol Record)> records)
    {
        var declaration = (RecordDeclarationSyntax)context.Node;
        if (declaration.ParameterList is not { Parameters.Count: >= MinimumRecordParameters } parameters || declaration.Members.Count != 0 ||
            declaration.BaseList is not null || declaration.TypeParameterList is not null)
            return;
        if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not { } record || IsJustified(record))
            return;
        var key = new StringBuilder(declaration.IsKind(SyntaxKind.RecordStructDeclaration) ? "struct(" : "class(");
        foreach (ParameterSyntax parameter in parameters.Parameters)
        {
            ITypeSymbol? type = parameter.Type is null ? null : context.SemanticModel.GetTypeInfo(parameter.Type, context.CancellationToken).Type;
            key.Append(parameter.AttributeLists.ToString()).Append(type?.ToDisplayString() ?? "?").Append(' ')
                .Append(parameter.Identifier.ValueText).Append(',');
        }
        records.Add((key.ToString(), record));
    }

    /// <summary>
    /// The body's tokens with references to the declaring type's own members (and to the type itself), every type
    /// argument and every string literal's contents replaced by placeholders.
    /// </summary>
    private static string Normalised(SyntaxNode body, INamedTypeSymbol declaringType, SemanticModel model, CancellationToken cancellationToken)
    {
        var text = new StringBuilder();
        foreach (SyntaxToken token in body.DescendantTokens())
        {
            if (token.IsKind(SyntaxKind.StringLiteralToken) || token.IsKind(SyntaxKind.InterpolatedStringTextToken))
            {
                text.Append("\u0001s ");
                continue;
            }
            if (token.IsKind(SyntaxKind.IdentifierToken) && token.Parent is { } parent)
            {
                if (parent.Parent is TypeArgumentListSyntax || IsTypeArgument(parent))
                {
                    text.Append("\u0001t ");
                    continue;
                }
                ISymbol? symbol = model.GetSymbolInfo(parent, cancellationToken).Symbol;
                if (symbol is not null && IsOwn(symbol, declaringType))
                {
                    text.Append("\u0001m ");
                    continue;
                }
            }
            text.Append(token.ValueText).Append(' ');
        }
        return text.ToString();
    }

    /// <summary>Whether a name sits inside a generic name's type-argument list.</summary>
    private static bool IsTypeArgument(SyntaxNode node) => node.Ancestors().TakeWhile(ancestor => ancestor is TypeSyntax)
        .Any(ancestor => ancestor.Parent is TypeArgumentListSyntax);

    /// <summary>Whether <paramref name="symbol"/> is the declaring type, one of its members, or nested in it.</summary>
    private static bool IsOwn(ISymbol symbol, INamedTypeSymbol declaringType)
    {
        for (ISymbol? current = symbol; current is not null; current = current.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, declaringType.OriginalDefinition))
                return true;
        }
        return false;
    }

    /// <summary>Reports one group at its first member, naming every member and pointing at the rest.</summary>
    private static void Report(CompilationAnalysisContext context, DiagnosticDescriptor rule, IReadOnlyList<ISymbol> members, Func<ISymbol, string> name) =>
        context.ReportDiagnostic(Diagnostic.Create(rule, members[0].Locations[0], members.Skip(1).Select(member => member.Locations[0]),
            string.Join(", ", members.Select(name))));

    /// <summary>A stable ordering key: the member's source path and position.</summary>
    private static string Order(ISymbol symbol) =>
        $"{symbol.Locations[0].SourceTree?.FilePath}:{symbol.Locations[0].SourceSpan.Start:D8}";

    /// <summary>Whether <paramref name="symbol"/> carries an intended-duplicate justification.</summary>
    private static bool IsJustified(ISymbol symbol) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == IntendedDuplicateAttributeName);
}
