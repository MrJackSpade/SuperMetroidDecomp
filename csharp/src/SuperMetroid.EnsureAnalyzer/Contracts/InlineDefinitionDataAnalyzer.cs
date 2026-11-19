using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Definition data lives in domain catalogs (SME6472, #2575): in <c>SuperMetroid.Core</c>, behaviour code writes no
/// definition data as literals. Outside a catalog file, a call to a Core member is reported when it builds a Core value
/// from a nonzero numeric literal (a constructor, or a static factory that returns its own type:
/// <c>new Offset16&lt;RoomPixels&gt;(1, 0)</c>, <c>SignedSixteenSixteen.FromWholePixels(1)</c>,
/// <c>UpdateCountdown.Lasting(1)</c>) or passes a nonzero numeric literal by name (<c>count: 5</c>,
/// <c>ticks: 1</c>): the literal is a duration, speed, distance, count or shape the game defines, and it belongs to a
/// domain-named catalog member whose summary gives its meaning. Behaviour code is every member of an instance type,
/// its static fields included, and every member of a static type that decides something. A static type's member that
/// holds no decision or procedure (what SME6451 would accept in a catalog) is a table of data and is not reported. A type
/// building or naming its own values (a quantity's own <c>FromWholePixels</c>, a record's own named instances) is
/// that type's representation and is not reported either; zero, the absence of a quantity, is never reported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InlineDefinitionDataAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core behaviour code writes definition data as a literal.</summary>
    public const string InlineDefinitionDataId = "SME6472";

    private static readonly DiagnosticDescriptor Rule = new(
        InlineDefinitionDataId,
        "Core behaviour code writes definition data inline",
        "'{0}' {1}; definition data lives in a domain-named catalog member whose summary gives its meaning, and behaviour code reads it by name",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Definition data (durations, speeds, shapes, damage, frame lists, names) lives in domain-named catalog " +
            "types with the meaning on each member's XML summary, and call sites read as named domain operations " +
            "(AGENTS.md). A value built from a literal, or a literal passed by name, in behaviour code is that data written " +
            "where it is used: its meaning is in prose or nowhere, and every other place that needs it repeats the number. " +
            "Move it to the catalog of the domain that defines it and read the member (#2575).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            var tableMembers = new ConcurrentDictionary<SyntaxNode, bool>();
            start.RegisterSyntaxNodeAction(node => Analyze(node, tableMembers),
                SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression, SyntaxKind.InvocationExpression);
        });
    }

    /// <summary>Reports a Core value built from a literal, or a literal passed by name, in behaviour code.</summary>
    private static void Analyze(SyntaxNodeAnalysisContext context, ConcurrentDictionary<SyntaxNode, bool> tableMembers)
    {
        SyntaxTree tree = context.Node.SyntaxTree;
        if (CatalogDecisionAnalyzer.IsCatalog(tree) || CartridgeEncodingScanner.IsBuildGenerated(tree.FilePath))
            return;
        ArgumentListSyntax? arguments = context.Node switch
        {
            BaseObjectCreationExpressionSyntax creation => creation.ArgumentList,
            InvocationExpressionSyntax invocation => invocation.ArgumentList,
            _ => null,
        };
        if (arguments is null || arguments.Arguments.Count == 0 ||
            context.SemanticModel.GetSymbolInfo(context.Node, context.CancellationToken).Symbol is not IMethodSymbol method ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingAssembly, context.Compilation.Assembly) ||
            context.Node.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } typeDeclaration ||
            context.SemanticModel.GetDeclaredSymbol(typeDeclaration, context.CancellationToken) is not { } enclosing)
        {
            return;
        }
        bool construction = IsConstruction(method);
        if (construction && IsOwnRepresentation(method.ContainingType, enclosing))
            return;
        if (IsTableMember(context, enclosing, tableMembers))
            return;
        string? what = construction
            ? Built(method, arguments, context.SemanticModel, context.CancellationToken)
            : null;
        what ??= Named(arguments, context.SemanticModel, context.CancellationToken);
        if (what is not null)
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), enclosing.Name, what));
    }

    /// <summary>Whether <paramref name="method"/> builds a value: a constructor, or a static factory returning its own type.</summary>
    private static bool IsConstruction(IMethodSymbol method) =>
        method.MethodKind == MethodKind.Constructor ||
        (method.IsStatic && SymbolEqualityComparer.Default.Equals(method.ReturnType.OriginalDefinition, method.ContainingType.OriginalDefinition));

    /// <summary>Whether <paramref name="built"/> is <paramref name="enclosing"/> or a type that contains it.</summary>
    private static bool IsOwnRepresentation(INamedTypeSymbol built, INamedTypeSymbol enclosing)
    {
        for (INamedTypeSymbol? type = enclosing; type is not null; type = type.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(built.OriginalDefinition, type.OriginalDefinition))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Whether the call sits in a table: a member of a static type that holds no decision or procedure. The answer is
    /// kept per member, since a table builds many values.
    /// </summary>
    private static bool IsTableMember(SyntaxNodeAnalysisContext context, INamedTypeSymbol enclosing, ConcurrentDictionary<SyntaxNode, bool> tableMembers)
    {
        if (!enclosing.IsStatic)
            return false;
        MemberDeclarationSyntax? member = context.Node.Ancestors().OfType<MemberDeclarationSyntax>()
            .FirstOrDefault(declaration => declaration is not BaseTypeDeclarationSyntax);
        if (member is null)
            return false;
        SemanticModel model = context.SemanticModel;
        CancellationToken cancellationToken = context.CancellationToken;
        return tableMembers.GetOrAdd(member, declaration => !CatalogDecisions.Decides(declaration, model, cancellationToken));
    }

    /// <summary>How a construction of a value from nonzero literals is named, or none when no argument is one.</summary>
    private static string? Built(IMethodSymbol method, ArgumentListSyntax arguments, SemanticModel model, CancellationToken cancellationToken)
    {
        string[] literals = [.. arguments.Arguments.Where(argument => IsNonZeroLiteral(argument.Expression, model, cancellationToken))
            .Select(argument => argument.Expression.ToString())];
        return literals.Length == 0
            ? null
            : $"builds a '{method.ContainingType.Name}' from the literal{(literals.Length == 1 ? "" : "s")} {string.Join(", ", literals)}";
    }

    /// <summary>How the nonzero literals passed by name are named, or none when no named argument is one.</summary>
    private static string? Named(ArgumentListSyntax arguments, SemanticModel model, CancellationToken cancellationToken)
    {
        string[] named = [.. arguments.Arguments
            .Where(argument => argument.NameColon is not null && IsNonZeroLiteral(argument.Expression, model, cancellationToken))
            .Select(argument => $"{argument.Expression} as '{argument.NameColon!.Name.Identifier.Text}'")];
        return named.Length == 0 ? null : $"passes the literal {string.Join(", ", named)}";
    }

    /// <summary>Whether <paramref name="expression"/> is a numeric literal, signed or not, whose value is not zero.</summary>
    private static bool IsNonZeroLiteral(ExpressionSyntax expression, SemanticModel model, CancellationToken cancellationToken)
    {
        ExpressionSyntax operand = expression is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression or (int)SyntaxKind.UnaryPlusExpression } signed
            ? signed.Operand
            : expression;
        if (!operand.IsKind(SyntaxKind.NumericLiteralExpression))
            return false;
        Optional<object?> value = model.GetConstantValue(expression, cancellationToken);
        return value.HasValue && value.Value is not null && Convert.ToDouble(value.Value, System.Globalization.CultureInfo.InvariantCulture) != 0;
    }
}
