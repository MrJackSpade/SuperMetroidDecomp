using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// #626: a source file declares at most one top-level named type (class, record, struct, interface, enum or
/// delegate, in any namespace form, partial or not), so a type is found by its file name and the surrounding domain
/// directory, not by searching umbrella files. Every declaration after the first is reported, naming the file's first
/// declaration. A file of top-level statements holds no explicit type at all. Generated sources are analyzed except
/// the named third-party generators' output, which this repository does not maintain.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OneTypePerFileAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A further top-level type in a file that already declares one, or any type beside top-level statements.</summary>
    public const string Id = "SME6400";

    /// <summary>
    /// Path prefixes of the framework source generators whose output combines types by design; the repository owns no
    /// generator, and a repository generator would be analyzed like any other source.
    /// </summary>
    internal static readonly string[] ThirdPartyGeneratorPrefixes =
    [
        "Microsoft.Interop.", "System.Text.RegularExpressions.Generator", "System.Text.Json.SourceGeneration",
        "Microsoft.Extensions.", "Microsoft.Gen.", "System.Runtime.InteropServices.Generator", "__Microsoft.Android.",
    ];

    /// <summary>Reports each extra declaration at its identifier.</summary>
    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "One top-level type per file",
        "'{0}' is another top-level type in a file that declares '{1}' (line {2}); move '{0}' to its own '{0}.cs' in its domain directory",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each source file declares at most one top-level type, named by the file (#626).");

    /// <summary>Reports an explicit type declared beside top-level statements.</summary>
    private static readonly DiagnosticDescriptor StatementsRule = new(
        Id,
        "One top-level type per file",
        "'{0}' is declared beside top-level statements; an entry-point statement file declares no type, so move '{0}' to its own '{0}.cs'",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each source file declares at most one top-level type, named by the file (#626).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, StatementsRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeTree);
    }

    /// <summary>The file's top-level type declarations in source order, through any namespace nesting.</summary>
    public static ImmutableArray<MemberDeclarationSyntax> TopLevelTypes(CompilationUnitSyntax root)
    {
        var builder = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();
        Collect(root.Members, builder);
        return builder.ToImmutable();
    }

    /// <summary>The declared name of a top-level type declaration.</summary>
    public static string NameOf(MemberDeclarationSyntax declaration) => declaration switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
        DelegateDeclarationSyntax @delegate => @delegate.Identifier.ValueText,
        _ => throw new System.ArgumentException($"Not a type declaration: {declaration.Kind()}", nameof(declaration)),
    };

    /// <summary>Whether the tree is output of a framework generator this repository does not maintain.</summary>
    internal static bool IsThirdPartyGenerated(string path)
    {
        string normalized = path.Replace('\\', '/');
        return ThirdPartyGeneratorPrefixes.Any(prefix =>
            normalized.StartsWith(prefix, System.StringComparison.Ordinal) ||
            normalized.Contains("/" + prefix));
    }

    /// <summary>Reports every declaration after the first, or every declaration beside top-level statements.</summary>
    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        if (IsThirdPartyGenerated(context.Tree.FilePath) || context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
            return;
        ImmutableArray<MemberDeclarationSyntax> types = TopLevelTypes(root);
        if (root.Members.OfType<GlobalStatementSyntax>().Any())
        {
            foreach (MemberDeclarationSyntax type in types)
                context.ReportDiagnostic(Diagnostic.Create(StatementsRule, IdentifierLocation(type), NameOf(type)));
            return;
        }
        if (types.Length < 2)
            return;
        MemberDeclarationSyntax first = types[0];
        int firstLine = first.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        foreach (MemberDeclarationSyntax extra in types.Skip(1))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule, IdentifierLocation(extra), [first.GetLocation()], NameOf(extra), NameOf(first), firstLine));
        }
    }

    /// <summary>Gathers type declarations, descending into block- and file-scoped namespaces.</summary>
    private static void Collect(SyntaxList<MemberDeclarationSyntax> members, ImmutableArray<MemberDeclarationSyntax>.Builder builder)
    {
        foreach (MemberDeclarationSyntax member in members)
        {
            if (member is BaseNamespaceDeclarationSyntax ns)
                Collect(ns.Members, builder);
            else if (member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
                builder.Add(member);
        }
    }

    /// <summary>The declaration's identifier, where the diagnostic is shown.</summary>
    private static Location IdentifierLocation(MemberDeclarationSyntax declaration) => declaration switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier.GetLocation(),
        DelegateDeclarationSyntax @delegate => @delegate.Identifier.GetLocation(),
        _ => declaration.GetLocation(),
    };
}
