using System.Collections.Immutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>Requires XML documentation on every declared repository-owned C# symbol.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class XmlDocumentationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Diagnostic identifier for declarations without XML documentation.</summary>
    public const string RuleId = "SME1274";

    /// <summary>Describes explicit declarations that lack XML documentation.</summary>
    private static readonly DiagnosticDescriptor Rule = new(
        RuleId,
        "Add XML documentation",
        "Repository-owned {0} '{1}' must have XML documentation",
        "Documentation",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Document every explicit repository-owned type and member, including private members.");

    /// <summary>Gets the missing XML documentation diagnostic.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>Registers checks for explicitly declared symbols.</summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType, SymbolKind.Method,
            SymbolKind.Property, SymbolKind.Field, SymbolKind.Event);
    }

    /// <summary>Reports an explicit source symbol whose documentation comment is empty.</summary>
    /// <param name="context">The symbol analysis context.</param>
    private static void Analyze(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol.DeclaringSyntaxReferences.Length == 0 ||
            symbol.Locations.All(location => !location.IsInSource))
            return;

        // Top-level statements synthesize a Program type whose declaration is the
        // compilation unit and a reserved entry-point method. Roslyn reports both
        // symbols as explicitly declared even though neither is a source member.
        if (symbol is INamedTypeSymbol && symbol.DeclaringSyntaxReferences.Any(reference =>
                reference.GetSyntax(context.CancellationToken) is CompilationUnitSyntax))
            return;
        if (symbol is IMethodSymbol { Name: "<Main>$" })
            return;

        // C# extension blocks surface as unnamed named-type symbols. The block is
        // only a syntax container; its declared extension members remain required.
        if (symbol is INamedTypeSymbol { Name.Length: 0 } &&
            symbol.DeclaringSyntaxReferences.Any(reference =>
                reference.GetSyntax(context.CancellationToken) is ExtensionBlockDeclarationSyntax))
            return;

        // Accessor methods inherit documentation from their property/event and are compiler generated.
        if (symbol is IMethodSymbol { MethodKind: not MethodKind.Ordinary and not MethodKind.Constructor and
            not MethodKind.StaticConstructor and not MethodKind.Destructor and not MethodKind.UserDefinedOperator and
            not MethodKind.Conversion and not MethodKind.ExplicitInterfaceImplementation })
            return;

        string? documentation = symbol.GetDocumentationCommentXml();
        if (!string.IsNullOrWhiteSpace(documentation) &&
            (symbol is not IMethodSymbol method || CoversPrimaryConstructorParameters(method, documentation)))
            return;

        Location? location = symbol.Locations.FirstOrDefault(candidate => candidate.IsInSource);
        if (location is null)
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, Category(symbol), symbol.Name));
    }

    /// <summary>Checks parameter coverage for non-record class and struct primary constructors.</summary>
    /// <param name="method">Method symbol whose primary-constructor status is inspected.</param>
    /// <param name="documentation">XML documentation associated with the method symbol.</param>
    /// <returns><see langword="true"/> when no extra coverage is required or every primary-constructor parameter is documented.</returns>
    private static bool CoversPrimaryConstructorParameters(IMethodSymbol method, string documentation)
    {
        if (method.MethodKind != MethodKind.Constructor ||
            !method.DeclaringSyntaxReferences.Any(reference =>
                reference.GetSyntax() is ClassDeclarationSyntax { ParameterList: not null } or
                    StructDeclarationSyntax { ParameterList: not null }))
            return true;

        try
        {
            HashSet<string> documentedParameters = XDocument.Parse($"<documentation>{documentation}</documentation>")
                .Descendants("param")
                .Select(element => (string?)element.Attribute("name"))
                .Where(name => name is not null)
                .Select(name => name!)
                .ToHashSet(StringComparer.Ordinal);
            return method.Parameters.All(parameter => documentedParameters.Contains(parameter.Name));
        }
        catch
        {
            // Compiler XML diagnostics report malformed documentation separately. Keep SME1274
            // active as well because malformed XML cannot establish complete parameter coverage.
            return false;
        }
    }

    /// <summary>Gets the user-facing symbol category used in the diagnostic message.</summary>
    /// <param name="symbol">The declaration being diagnosed.</param>
    /// <returns>A concise category name for the declaration.</returns>
    private static string Category(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol => "type",
        IMethodSymbol { MethodKind: MethodKind.Constructor } => "constructor",
        IMethodSymbol { MethodKind: MethodKind.StaticConstructor } => "static constructor",
        IMethodSymbol { MethodKind: MethodKind.Destructor } => "destructor",
        IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator or MethodKind.Conversion } => "operator",
        IMethodSymbol => "method",
        IPropertySymbol => "property",
        IFieldSymbol => "field",
        IEventSymbol => "event",
        _ => "member"
    };
}
