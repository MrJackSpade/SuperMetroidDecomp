using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.Generators;

/// <summary>
/// #2602: the named members of definition catalogs, written at compile time so code that spells a catalog's members (a
/// generator rendering a catalog's source) names them through code instead of scanning the catalog's properties. A static
/// partial method carrying <c>[NamedMembers(typeof(A), typeof(B), ...)]</c> and returning an array of
/// <c>(T Value, string Owner, string Name)</c> is implemented as every public static property and field of each named
/// type whose type converts to <c>T</c> by identity or reference: the member's value, its type's name and its own name, in
/// the order the types are named and each type declares its members.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class NamedMembersGenerator : IIncrementalGenerator
{
    private const string AttributeName = "NamedMembersAttribute";

    private static readonly DiagnosticDescriptor Shape = new("SME6495", "Named members method has the wrong shape",
        "{0} must be a static partial method returning an array of (T Value, string Owner, string Name)", "Build",
        DiagnosticSeverity.Error, isEnabledByDefault: true);

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<MethodDeclarationSyntax> methods = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is MethodDeclarationSyntax method && method.Modifiers.Any(SyntaxKind.PartialKeyword) &&
                method.Body is null && method.ExpressionBody is null &&
                method.AttributeLists.SelectMany(list => list.Attributes).Any(attribute => attribute.Name.ToString() is "NamedMembers" or AttributeName),
            static (syntax, _) => (MethodDeclarationSyntax)syntax.Node);
        context.RegisterSourceOutput(methods.Collect().Combine(context.CompilationProvider), static (output, input) =>
        {
            (var declarations, Compilation compilation) = input;
            foreach (MethodDeclarationSyntax declaration in declarations)
            {
                if (compilation.GetSemanticModel(declaration.SyntaxTree).GetDeclaredSymbol(declaration, output.CancellationToken) is not { } method ||
                    !method.IsPartialDefinition || method.PartialImplementationPart is not null)
                    continue;
                AttributeData? attribute = method.GetAttributes().FirstOrDefault(data => data.AttributeClass?.Name == AttributeName);
                if (attribute is null)
                    continue;
                if (!method.IsStatic || method.Parameters.Length != 0 || ValueType(method.ReturnType) is not { } value)
                {
                    output.ReportDiagnostic(Diagnostic.Create(Shape, declaration.GetLocation(), method.Name));
                    continue;
                }
                INamedTypeSymbol[] owners = attribute.ConstructorArguments
                    .SelectMany(argument => argument.Kind == TypedConstantKind.Array ? argument.Values.AsEnumerable() : new[] { argument })
                    .Select(argument => argument.Value).OfType<INamedTypeSymbol>().ToArray();
                output.AddSource($"{method.ContainingType.ToDisplayString()}.{method.Name}.g.cs", Emit(method, compilation, value, owners));
            }
        });
    }

    /// <summary>The <c>T</c> of a <c>(T Value, string Owner, string Name)[]</c>, or none for any other type.</summary>
    private static ITypeSymbol? ValueType(ITypeSymbol returned) =>
        returned is IArrayTypeSymbol { Rank: 1, ElementType: INamedTypeSymbol { IsTupleType: true, TupleElements: { Length: 3 } elements } } &&
        elements[1].Type.SpecialType == SpecialType.System_String && elements[2].Type.SpecialType == SpecialType.System_String
            ? elements[0].Type
            : null;

    private static string Emit(IMethodSymbol method, Compilation compilation, ITypeSymbol value, INamedTypeSymbol[] owners)
    {
        var entries = new StringBuilder();
        foreach (INamedTypeSymbol owner in owners)
        {
            string qualified = owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            foreach (ISymbol member in owner.GetMembers())
            {
                if (!member.IsStatic || member.DeclaredAccessibility != Accessibility.Public || member.IsImplicitlyDeclared)
                    continue;
                ITypeSymbol? type = member switch
                {
                    IPropertySymbol { IsIndexer: false, GetMethod: not null } property => property.Type,
                    IFieldSymbol field => field.Type,
                    _ => null,
                };
                if (type is null || compilation.ClassifyConversion(type, value) is not { Exists: true } conversion ||
                    !(conversion.IsIdentity || (conversion.IsImplicit && conversion.IsReference)))
                    continue;
                entries.Append($"        ({qualified}.{member.Name}, {Literal(owner.Name)}, {Literal(member.Name)}),\n");
            }
        }
        INamedTypeSymbol type0 = method.ContainingType;
        var opening = new StringBuilder();
        var closing = new StringBuilder();
        var chain = new Stack<INamedTypeSymbol>();
        for (INamedTypeSymbol? current = type0; current is not null; current = current.ContainingType)
            chain.Push(current);
        foreach (INamedTypeSymbol current in chain)
        {
            opening.Append($"{SyntaxFacts.GetText(current.DeclaredAccessibility)} {(current.IsStatic ? "static " : "")}partial class {current.Name}\n{{\n");
            closing.Append("}\n");
        }
        string returned = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
        return $$"""
            // <auto-generated/>
            #nullable enable
            namespace {{type0.ContainingNamespace.ToDisplayString()}};

            {{opening}}    {{SyntaxFacts.GetText(method.DeclaredAccessibility)}} static partial {{returned}} {{method.Name}}() =>
                [
            {{entries}}    ];
            {{closing}}
            """;
    }

    private static string Literal(string value) => SymbolDisplay.FormatLiteral(value, quote: true);
}
