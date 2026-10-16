using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>Requires source documentation on every declared C# type and member, regardless of accessibility or static state.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MemberDocumentationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic reported for a declaration without an XML documentation comment.</summary>
    public const string Id = "SME6370";

    /// <summary>Build-breaking documentation requirement shared by every supported declaration kind.</summary>
    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "Document every member",
        "{0} needs an XML documentation comment",
        "Documentation",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All source declarations, including private and static members, require their own XML documentation comment.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.ExtensionBlockDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EnumMemberDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration);
    }

    /// <summary>Reports a declaration when its own leading trivia has no XML documentation.</summary>
    private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (MemberDeclarationSyntax)context.Node;
        if (GeneratedSource.IsGenerated(declaration.SyntaxTree, context.CancellationToken))
            return;
        if (declaration.GetLeadingTrivia().Any(trivia => trivia.GetStructure() is DocumentationCommentTriviaSyntax))
            return;

        string name = declaration switch
        {
            ExtensionBlockDeclarationSyntax => "ExtensionBlockDeclaration",
            BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
            DelegateDeclarationSyntax member => member.Identifier.ValueText,
            EnumMemberDeclarationSyntax member => member.Identifier.ValueText,
            MethodDeclarationSyntax member => member.Identifier.ValueText,
            ConstructorDeclarationSyntax member => member.Identifier.ValueText,
            DestructorDeclarationSyntax member => member.Identifier.ValueText,
            PropertyDeclarationSyntax member => member.Identifier.ValueText,
            EventDeclarationSyntax member => member.Identifier.ValueText,
            OperatorDeclarationSyntax member => member.OperatorToken.ValueText,
            ConversionOperatorDeclarationSyntax member => member.ImplicitOrExplicitKeyword.ValueText,
            _ => declaration.Kind().ToString(),
        };
        context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.GetFirstToken().GetLocation(), name));
    }
}
