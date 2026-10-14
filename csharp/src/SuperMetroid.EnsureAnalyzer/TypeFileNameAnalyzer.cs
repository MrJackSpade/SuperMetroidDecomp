using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// #626: a source file is named after the one top-level type it declares, <c>Type.cs</c>, so a type is found from its
/// name. A deliberate partial part adds its purpose, <c>Type.Purpose.cs</c>. Files that declare no type (entry-point
/// statements, assembly attributes) and the named third-party generators' output are not checked.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeFileNameAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A file whose name does not identify its declared type.</summary>
    public const string Id = "SME6401";

    /// <summary>Reports the declaration whose file is misnamed.</summary>
    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "File named after its type",
        "'{0}' is declared in '{1}'; name the file '{0}.cs'{2}",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A source file is named after the one top-level type it declares (#626).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeTree);
    }

    /// <summary>Whether <paramref name="stem"/> (the file name without <c>.cs</c>) names <paramref name="declaration"/>.</summary>
    public static bool NamesType(string stem, MemberDeclarationSyntax declaration)
    {
        string name = OneTypePerFileAnalyzer.NameOf(declaration);
        if (stem == name)
            return true;
        return declaration.Modifiers.Any(SyntaxKind.PartialKeyword) && stem.StartsWith(name + ".", System.StringComparison.Ordinal) &&
            stem.Length > name.Length + 1;
    }

    /// <summary>Reports a single declared type whose file name does not identify it.</summary>
    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        string path = context.Tree.FilePath;
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase) ||
            OneTypePerFileAnalyzer.IsThirdPartyGenerated(path) || context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
            return;
        ImmutableArray<MemberDeclarationSyntax> types = OneTypePerFileAnalyzer.TopLevelTypes(root);
        if (types.Length != 1 || root.Members.OfType<GlobalStatementSyntax>().Any())
            return;
        string fileName = Path.GetFileName(path);
        string stem = fileName.Substring(0, fileName.Length - ".cs".Length);
        MemberDeclarationSyntax declaration = types[0];
        if (NamesType(stem, declaration))
            return;
        string name = OneTypePerFileAnalyzer.NameOf(declaration);
        string partialHint = declaration.Modifiers.Any(SyntaxKind.PartialKeyword) ? $", or '{name}.<Purpose>.cs' for this partial part" : "";
        context.ReportDiagnostic(Diagnostic.Create(Rule, IdentifierLocation(declaration), name, fileName, partialHint));
    }

    /// <summary>The declaration's identifier, where the diagnostic is shown.</summary>
    private static Location IdentifierLocation(MemberDeclarationSyntax declaration) => declaration switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier.GetLocation(),
        DelegateDeclarationSyntax @delegate => @delegate.Identifier.GetLocation(),
        _ => declaration.GetLocation(),
    };
}
