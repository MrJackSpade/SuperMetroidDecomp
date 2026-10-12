using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Rewrites a raw CPU-bus read (SME6290) to the domain-view member that declares it returns exactly
/// that cell's raw value (#629). The fix is offered only when the mapping is unambiguous: the read's
/// constant address (or named cell), width and region identify one catalog cell, exactly one
/// <c>[CpuBusViewOf]</c> property exposes that cell with the read's own type, and the bus expression
/// converts to the view's <c>ISnesMemoryBus</c> constructor parameter.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CpuBusAccessCodeFixProvider)), Shared]
public sealed class CpuBusAccessCodeFixProvider : CodeFixProvider
{
    /// <summary>Defines view of attribute name.</summary>
    private const string ViewOfAttributeName = "CpuBusViewOfAttribute";
    /// <summary>Defines cell type name.</summary>
    private const string CellTypeName = "CpuBusCell";
    /// <summary>Defines memory bus type name.</summary>
    private const string MemoryBusTypeName = "ISnesMemoryBus";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [CpuBusAccessAnalyzer.RawAccessId];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SemanticModel? model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null || model is null)
            return;
        SyntaxNode node = root.FindNode(context.Span, getInnermostNodeForTie: true);
        if (ReadOf(node, model, context.CancellationToken) is not { } read)
            return;
        Solution solution = context.Document.Project.Solution;
        IFieldSymbol? cell = read.Cell ?? await CellAtAsync(solution, read, context.CancellationToken).ConfigureAwait(false);
        if (cell is null)
            return;
        if (await ViewMemberAsync(solution, cell, read.ResultType, context.CancellationToken).ConfigureAwait(false) is not { } member)
            return;
        INamedTypeSymbol view = member.ContainingType;
        if (!AcceptsBus(model, view, read.Bus))
            return;
        context.RegisterCodeFix(
            CodeAction.Create(
                $"Read {view.Name}.{member.Name}",
                token => Task.FromResult(Replace(context.Document, root, read, view, member)),
                equivalenceKey: $"{view.ToDisplayString()}.{member.Name}"),
            context.Diagnostics);
    }

    /// <summary>One raw read: the expression to replace, its bus, and either a named cell or a constant address.</summary>
    private sealed record RawRead(ExpressionSyntax Expression, ExpressionSyntax Bus, ITypeSymbol ResultType,
        IFieldSymbol? Cell, int Address, int Width, string Region);

    /// <summary>Reads of.</summary>
    private static RawRead? ReadOf(SyntaxNode node, SemanticModel model, CancellationToken token)
    {
        for (SyntaxNode? current = node; current is not null; current = current.Parent)
        {
            if (current is InvocationExpressionSyntax invocation)
                return Classify(invocation, model, token);
            if (current is StatementSyntax or MemberDeclarationSyntax)
                return null;
        }
        return null;
    }

    /// <summary>Recognizes the raw reads whose destination can be mapped: a constant-address WRAM word or
    /// byte read, a constant-address SRAM byte read, or a primitive read over a named catalog cell.</summary>
    private static RawRead? Classify(InvocationExpressionSyntax invocation, SemanticModel model, CancellationToken token)
    {
        if (model.GetSymbolInfo(invocation, token).Symbol is not IMethodSymbol method)
            return null;
        SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
        ExpressionSyntax? receiver = (invocation.Expression as MemberAccessExpressionSyntax)?.Expression;
        switch (method.ContainingType.Name, method.Name, arguments.Count)
        {
            case (_, "ReadWorkRamWordFixedBank", 1) when receiver is not null:
                return Constant(receiver, arguments[0].Expression, 2, "WorkRam");
            case (_, "ReadWorkRamByte", 1) when receiver is not null:
                return Constant(receiver, arguments[0].Expression, 1, "WorkRam");
            case (_, "ReadSaveRamByte", 1) when receiver is not null:
                return Constant(receiver, arguments[0].Expression, 1, "SaveRam");
            case ("WramWord" or "WramByte", "Read", 0) when receiver is BaseObjectCreationExpressionSyntax
            {
                ArgumentList.Arguments: [{ Expression: { } bus }, { Expression: { } cellExpression }],
            }:
                return model.GetSymbolInfo(cellExpression, token).Symbol is IFieldSymbol { Type.Name: CellTypeName } cell
                    ? new RawRead(invocation, bus, method.ReturnType, cell, 0, 0, "")
                    : null;
            default:
                return null;
        }

        RawRead? Constant(ExpressionSyntax bus, ExpressionSyntax address, int width, string region) =>
            model.GetConstantValue(address, token) is { HasValue: true, Value: int value }
                ? new RawRead(invocation, bus, method.ReturnType, null, value, width, region)
                : null;
    }

    /// <summary>Replace.</summary>
    private static Document Replace(Document document, SyntaxNode root, RawRead read, INamedTypeSymbol view, IPropertySymbol member)
    {
        ExpressionSyntax creation = SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.ParseTypeName(view.ToDisplayString()).WithAdditionalAnnotations(Simplifier.Annotation))
            .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Argument(read.Bus.WithoutTrivia()))));
        ExpressionSyntax replacement = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                creation, SyntaxFactory.IdentifierName(member.Name))
            .WithTriviaFrom(read.Expression)
            .WithAdditionalAnnotations(Simplifier.Annotation);
        return document.WithSyntaxRoot(root.ReplaceNode(read.Expression, replacement));
    }

    /// <summary>Accepts bus.</summary>
    private static bool AcceptsBus(SemanticModel model, INamedTypeSymbol view, ExpressionSyntax bus) =>
        view.InstanceConstructors.Any(constructor =>
            constructor.Parameters.Length == 1 &&
            constructor.Parameters[0].Type.Name == MemoryBusTypeName &&
            model.ClassifyConversion(bus, constructor.Parameters[0].Type) is { IsImplicit: true });

    /// <summary>Cell at async.</summary>
    private static async Task<IFieldSymbol?> CellAtAsync(Solution solution, RawRead read, CancellationToken token)
    {
        IFieldSymbol? match = null;
        foreach (IFieldSymbol field in await CellFieldsAsync(solution, token).ConfigureAwait(false))
        {
            if (await CellShapeAsync(solution, field, token).ConfigureAwait(false) is not { } shape ||
                shape.Address != read.Address || shape.Width != read.Width || shape.Region != read.Region)
                continue;
            if (match is not null)
                return null;
            match = field;
        }
        return match;
    }

    /// <summary>View member async.</summary>
    private static async Task<IPropertySymbol?> ViewMemberAsync(Solution solution, IFieldSymbol cell, ITypeSymbol resultType,
        CancellationToken token)
    {
        IPropertySymbol? match = null;
        foreach (INamedTypeSymbol type in await SourceTypesAsync(solution, token).ConfigureAwait(false))
        {
            foreach (IPropertySymbol property in type.GetMembers().OfType<IPropertySymbol>())
            {
                if (!Declares(property, cell) || property.GetMethod is null ||
                    property.Type.SpecialType != resultType.SpecialType)
                    continue;
                if (match is not null)
                    return null;
                match = property;
            }
        }
        return match;
    }

    /// <summary>Declares.</summary>
    private static bool Declares(IPropertySymbol property, IFieldSymbol cell) =>
        property.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.Name == ViewOfAttributeName &&
            attribute.ConstructorArguments is [{ Value: INamedTypeSymbol catalog }, { Value: string name }] &&
            name == cell.Name &&
            catalog.ToDisplayString() == cell.ContainingType.ToDisplayString());

    /// <summary>Cell fields async.</summary>
    private static async Task<IReadOnlyList<IFieldSymbol>> CellFieldsAsync(Solution solution, CancellationToken token) =>
        (await SourceTypesAsync(solution, token).ConfigureAwait(false))
            .SelectMany(type => type.GetMembers().OfType<IFieldSymbol>())
            .Where(field => field.IsStatic && field.Type.Name == CellTypeName)
            .ToArray();

    /// <summary>Source types async.</summary>
    private static async Task<IReadOnlyList<INamedTypeSymbol>> SourceTypesAsync(Solution solution, CancellationToken token)
    {
        var types = new List<INamedTypeSymbol>();
        foreach (Project project in solution.Projects)
        {
            if (await project.GetCompilationAsync(token).ConfigureAwait(false) is not { } compilation)
                continue;
            Collect(compilation.Assembly.GlobalNamespace, types);
        }
        return types;
    }

    /// <summary>Collect.</summary>
    private static void Collect(INamespaceOrTypeSymbol container, List<INamedTypeSymbol> types)
    {
        foreach (INamespaceOrTypeSymbol member in container.GetMembers().OfType<INamespaceOrTypeSymbol>())
        {
            if (member is INamedTypeSymbol type)
                types.Add(type);
            Collect(member, types);
        }
    }

    /// <summary>The address, width and region a catalog cell field's factory initializer declares.</summary>
    private static async Task<(int Address, int Width, string Region)?> CellShapeAsync(Solution solution, IFieldSymbol field,
        CancellationToken token)
    {
        if (field.DeclaringSyntaxReferences is not [{ } reference] ||
            await reference.GetSyntaxAsync(token).ConfigureAwait(false) is not VariableDeclaratorSyntax
            {
                Initializer.Value: InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: { } factory },
                    ArgumentList.Arguments: { Count: >= 2 } arguments,
                },
            } ||
            solution.GetDocument(reference.SyntaxTree) is not { } document ||
            await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not { } model ||
            model.GetConstantValue(arguments[1].Expression, token) is not { HasValue: true, Value: int address })
            return null;
        (int width, string region)? shape = factory switch
        {
            "WramWord" => (2, "WorkRam"),
            "WramByte" => (1, "WorkRam"),
            "SramBytes" when arguments.Count >= 3 &&
                model.GetConstantValue(arguments[2].Expression, token) is { HasValue: true, Value: 1 } => (1, "SaveRam"),
            _ => null,
        };
        return shape is { } known ? (address, known.width, known.region) : null;
    }
}
