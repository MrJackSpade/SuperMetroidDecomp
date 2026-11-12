using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Merges a type's hand-written partial declarations into one (#621), the asynchronous cleanup's fix for SME6360
/// (#1668). The merged declaration is the part in the file named after the type (for a nested type, or when no file is,
/// the first part). Its members are every part's, concatenated in the compiler's own syntax-tree order across the
/// parts, so field initializers run in the order they ran before, which is the only behaviour a merge could otherwise
/// change. It takes the union of the parts' modifiers, attributes and base types, the documentation of a part when it
/// has none, and in its file the union of the parts' files' using directives (the cleanup's IDE0005 pass removes the
/// unneeded ones). <c>partial</c> stays only while a generated part or a partial member needs it. A generated part is
/// not touched. The other parts go, and a file left without a declaration is deleted, with its project list entries
/// (the cleanup's runner removes those). Parts with preprocessor directives, a primary constructor on another part,
/// differing type parameters or constraints, or text spanning lines that a different indentation would re-indent fail
/// the fix.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PartialMergeCodeFixProvider)), Shared]
public sealed class PartialMergeCodeFixProvider : DocumentFixProvider
{
    /// <summary>The order the merged declaration's modifiers are written in.</summary>
    private static readonly SyntaxKind[] ModifierOrder =
    [
        SyntaxKind.PublicKeyword, SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword, SyntaxKind.FileKeyword,
        SyntaxKind.StaticKeyword, SyntaxKind.ExternKeyword, SyntaxKind.NewKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.AbstractKeyword,
        SyntaxKind.SealedKeyword, SyntaxKind.OverrideKeyword, SyntaxKind.ReadOnlyKeyword, SyntaxKind.UnsafeKeyword, SyntaxKind.RequiredKeyword,
        SyntaxKind.VolatileKeyword, SyntaxKind.AsyncKeyword, SyntaxKind.RefKeyword, SyntaxKind.PartialKeyword,
    ];

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [PartialSplitAnalyzer.SplitId];

    /// <inheritdoc />
    internal override string Title => "Merge the hand-written parts";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no syntax tree.");
        SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no semantic model.");
        var edits = new SolutionEdits(document.Project.Solution);
        var merged = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var removed = new List<TypeDeclarationSyntax>();
        var targets = new HashSet<SyntaxTree>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            SyntaxToken identifier = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (identifier.Parent is not TypeDeclarationSyntax declaration || declaration.Identifier != identifier ||
                model.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol type)
                throw new CleanupFailure(diagnostic, "the finding is not at a type declaration");
            if (merged.Add(type))
                await MergeAsync(edits, type, model.Compilation, diagnostic, removed, targets, cancellationToken).ConfigureAwait(false);
        }
        foreach (var file in removed.GroupBy(part => part.SyntaxTree))
        {
            if (!targets.Contains(file.Key) && IsEmptyWithout((CompilationUnitSyntax)await file.Key.GetRootAsync(cancellationToken).ConfigureAwait(false), [.. file]))
                edits.Delete(file.Key);
        }
        return edits.Apply();
    }

    /// <summary>Merges the hand-written parts of <paramref name="type"/> into one, adding the removed parts to
    /// <paramref name="removed"/> and the merged declaration's file to <paramref name="targets"/>.</summary>
    private static async Task MergeAsync(SolutionEdits edits, INamedTypeSymbol type, Compilation compilation, Diagnostic site,
        List<TypeDeclarationSyntax> removed, HashSet<SyntaxTree> targets, CancellationToken cancellationToken)
    {
        PartialDeclarations declarations = PartialDeclarations.Of(type, cancellationToken)
            ?? throw new CleanupFailure(site, $"{type.Name} is not partial");
        var trees = compilation.SyntaxTrees.ToList();
        var parts = declarations.HandWritten.OrderBy(part => trees.IndexOf(part.SyntaxTree)).ThenBy(part => part.SpanStart).ToList();
        if (parts.Count < 2)
            throw new CleanupFailure(site, $"{type.Name} has only one hand-written declaration");
        TypeDeclarationSyntax target = type.ContainingType is null
            ? parts.FirstOrDefault(part => Path.GetFileNameWithoutExtension(part.SyntaxTree.FilePath) == type.Name) ?? parts[0]
            : parts[0];
        Refuse(parts, target, site);
        targets.Add(target.SyntaxTree);
        bool keepPartial = declarations.All.Count > parts.Count ||
            parts.SelectMany(part => part.Members).Any(member => member.Modifiers.Any(SyntaxKind.PartialKeyword));
        SourceEdits own = await edits.ForAsync(target.SyntaxTree, cancellationToken).ConfigureAwait(false);
        SourceText text = own.Text;
        string newline = Newline(text);
        string typeIndent = Indentation(text.Lines.GetLineFromPosition(target.Keyword.SpanStart).ToString());
        bool oneLine = Line(text, target.OpenBraceToken.SpanStart) == Line(text, target.CloseBraceToken.SpanStart);
        string memberIndent = target.Members.Count > 0 && !oneLine
            ? Indentation(text.Lines.GetLineFromPosition(target.Members[0].SpanStart).ToString())
            : typeIndent + "    ";

        int index = parts.IndexOf(target);
        var before = new List<string>();
        foreach (TypeDeclarationSyntax part in parts.Take(index))
            before.AddRange(await MemberTextsAsync(edits, part, memberIndent, newline, site, cancellationToken).ConfigureAwait(false));
        var after = new List<string>();
        foreach (TypeDeclarationSyntax part in parts.Skip(index + 1))
            after.AddRange(await MemberTextsAsync(edits, part, memberIndent, newline, site, cancellationToken).ConfigureAwait(false));

        MergeHeader(own, parts, target, compilation, keepPartial, typeIndent, newline);
        await MergeUsingsAsync(own, parts, target, newline, cancellationToken).ConfigureAwait(false);
        if (oneLine)
        {
            var all = before.Concat(target.Members.Select(member => memberIndent + member)).Concat(after).ToList();
            if (all.Count > 0)
                own.Replace(TextSpan.FromBounds(target.OpenBraceToken.SpanStart, target.CloseBraceToken.Span.End),
                    "{" + newline + string.Join(newline + newline, all) + newline + typeIndent + "}");
        }
        else
        {
            if (before.Count > 0)
            {
                if (target.OpenBraceToken.GetNextToken().SpanStart < text.Lines.GetLineFromPosition(target.OpenBraceToken.SpanStart).EndIncludingLineBreak)
                    throw new CleanupFailure(site, $"{type.Name}'s opening brace does not end its line");
                own.Insert(text.Lines.GetLineFromPosition(target.OpenBraceToken.SpanStart).EndIncludingLineBreak,
                    string.Join(newline + newline, before) + newline + (target.Members.Count > 0 ? newline : ""));
            }
            if (after.Count > 0)
            {
                TextLine closeLine = text.Lines.GetLineFromPosition(target.CloseBraceToken.SpanStart);
                if (target.CloseBraceToken.GetPreviousToken().Span.End > closeLine.Start)
                    throw new CleanupFailure(site, $"{type.Name}'s closing brace does not start its line");
                bool preceded = target.Members.Count > 0 || before.Count > 0;
                own.Insert(closeLine.Start, string.Concat(after.Select((member, position) => (position > 0 || preceded ? newline : "") + member + newline)));
            }
        }
        foreach (TypeDeclarationSyntax part in parts.Where(part => part != target))
        {
            (await edits.ForAsync(part.SyntaxTree, cancellationToken).ConfigureAwait(false)).Remove(part);
            removed.Add(part);
        }
    }

    /// <summary>Fails for parts whose merge would need a choice or could change their text.</summary>
    private static void Refuse(List<TypeDeclarationSyntax> parts, TypeDeclarationSyntax target, Diagnostic site)
    {
        string Shape(TypeParameterListSyntax? list) => list?.ToString().Replace(" ", "") ?? "";
        foreach (TypeDeclarationSyntax part in parts)
        {
            if (part.DescendantTrivia(descendIntoTrivia: true).Any(trivia => trivia.IsDirective))
                throw new CleanupFailure(part.Identifier.GetLocation(), site.Id, "a part holds preprocessor directives");
            if (Shape(part.TypeParameterList) != Shape(target.TypeParameterList))
                throw new CleanupFailure(part.Identifier.GetLocation(), site.Id, "the parts' type parameters differ");
            if (part == target)
                continue;
            if (part.ParameterList is not null)
                throw new CleanupFailure(part.Identifier.GetLocation(), site.Id, "a part other than the merged one declares the primary constructor");
            if (part.ConstraintClauses.Count > 0 && part.ConstraintClauses.ToString() != target.ConstraintClauses.ToString())
                throw new CleanupFailure(part.Identifier.GetLocation(), site.Id, "the parts' constraints differ");
        }
    }

    /// <summary>The members of a part as text, each with the comments and documentation directly above it, indented as
    /// the merged declaration's members are.</summary>
    private static async Task<List<string>> MemberTextsAsync(SolutionEdits edits, TypeDeclarationSyntax part, string memberIndent, string newline,
        Diagnostic site, CancellationToken cancellationToken)
    {
        SourceEdits partEdits = await edits.ForAsync(part.SyntaxTree, cancellationToken).ConfigureAwait(false);
        SourceText text = partEdits.Text;
        string partIndent = part.Members.Count > 0
            ? Indentation(text.Lines.GetLineFromPosition(part.Members[0].SpanStart).ToString())
            : memberIndent;
        var texts = new List<string>();
        foreach (MemberDeclarationSyntax member in part.Members)
        {
            TextLine first = text.Lines.GetLineFromPosition(partEdits.AttachedStart(member));
            TextLine last = text.Lines.GetLineFromPosition(member.Span.End);
            if (member.GetFirstToken().GetPreviousToken().Span.End > first.Start || member.GetLastToken().GetNextToken().SpanStart < last.End)
            {
                if (member.ToString().Contains("\n"))
                    throw new CleanupFailure(member.GetLocation(), site.Id, "a member spanning lines shares a line with its part's braces");
                texts.Add(memberIndent + member);
                continue;
            }
            if (partIndent != memberIndent && member.DescendantTokens().Any(token => token.ToString().Contains("\n")))
                throw new CleanupFailure(member.GetLocation(), site.Id, "re-indenting the member would change text spanning lines");
            var lines = text.ToString(TextSpan.FromBounds(first.Start, last.End)).Replace("\r\n", "\n").Split('\n')
                .Select(line => line.StartsWith(partIndent, StringComparison.Ordinal) ? memberIndent + line.Substring(partIndent.Length) : line);
            texts.Add(string.Join(newline, lines));
        }
        return texts;
    }

    /// <summary>Gives the merged declaration the union of the parts' modifiers, attributes and base types, and another
    /// part's documentation when it has none.</summary>
    private static void MergeHeader(SourceEdits own, List<TypeDeclarationSyntax> parts, TypeDeclarationSyntax target, Compilation compilation,
        bool keepPartial, string typeIndent, string newline)
    {
        SourceText text = own.Text;
        var others = parts.Where(part => part != target).ToList();
        int headerLine = text.Lines.GetLineFromPosition(target.Modifiers.Count > 0 ? target.Modifiers[0].SpanStart : target.Keyword.SpanStart).Start;
        if (!HasDocumentation(target) && others.FirstOrDefault(HasDocumentation) is { } documented)
        {
            SourceText otherText = documented.SyntaxTree.GetText();
            int start = otherText.Lines.GetLineFromPosition(documented.GetLeadingTrivia().First(IsDocumentation).SpanStart).Start;
            int end = otherText.Lines.GetLineFromPosition(documented.SpanStart).Start;
            string documentation = otherText.ToString(TextSpan.FromBounds(start, end));
            string otherIndent = Indentation(otherText.Lines.GetLineFromPosition(start).ToString());
            own.Insert(text.Lines.GetLineFromPosition(target.SpanStart).Start, Reindent(documentation, otherIndent, typeIndent, newline));
        }
        var attributes = others.SelectMany(part => part.AttributeLists).Select(list => list.ToString()).ToList();
        if (attributes.Count > 0)
            own.Insert(headerLine, string.Concat(attributes.Select(list => typeIndent + list + newline)));

        var kinds = new HashSet<SyntaxKind>(parts.SelectMany(part => part.Modifiers).Select(modifier => modifier.Kind()));
        if (!keepPartial)
            kinds.Remove(SyntaxKind.PartialKeyword);
        var existing = new HashSet<SyntaxKind>(target.Modifiers.Select(modifier => modifier.Kind()));
        if (!kinds.SetEquals(existing))
        {
            string modifiers = string.Join(" ", ModifierOrder.Where(kinds.Contains).Select(SyntaxFacts.GetText));
            own.Replace(modifiers.Length > 0
                ? TextSpan.FromBounds(target.Modifiers[0].SpanStart, target.Modifiers[target.Modifiers.Count - 1].Span.End)
                : TextSpan.FromBounds(target.Modifiers[0].SpanStart, target.Keyword.SpanStart), modifiers);
        }

        var present = new HashSet<string>((target.BaseList?.Types ?? default).Select(baseType => baseType.ToString()));
        var classes = new List<string>();
        var interfaces = new List<string>();
        foreach (TypeDeclarationSyntax part in others)
        {
            if (part.BaseList is null)
                continue;
            SemanticModel model = compilation.GetSemanticModel(part.SyntaxTree);
            foreach (BaseTypeSyntax baseType in part.BaseList.Types)
            {
                if (!present.Add(baseType.ToString()))
                    continue;
                (model.GetTypeInfo(baseType.Type).Type?.TypeKind == TypeKind.Class ? classes : interfaces).Add(baseType.ToString());
            }
        }
        if (classes.Count + interfaces.Count == 0)
            return;
        if (target.BaseList is null)
        {
            SyntaxNode? after = (SyntaxNode?)target.ParameterList ?? target.TypeParameterList;
            own.Insert(after?.Span.End ?? target.Identifier.Span.End, " : " + string.Join(", ", classes.Concat(interfaces)));
            return;
        }
        if (classes.Count > 0)
            own.Insert(target.BaseList.Types[0].SpanStart, string.Join(", ", classes) + ", ");
        if (interfaces.Count > 0)
            own.Insert(target.BaseList.Types[target.BaseList.Types.Count - 1].Span.End, ", " + string.Join(", ", interfaces));
    }

    /// <summary>Adds to the merged declaration's file the using directives of the other parts' files it lacks.</summary>
    private static async Task MergeUsingsAsync(SourceEdits own, List<TypeDeclarationSyntax> parts, TypeDeclarationSyntax target, string newline,
        CancellationToken cancellationToken)
    {
        var targetRoot = (CompilationUnitSyntax)await target.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        var present = new HashSet<string>(targetRoot.Usings.Select(directive => directive.ToString()));
        var missing = new List<string>();
        foreach (TypeDeclarationSyntax part in parts.Where(part => part.SyntaxTree != target.SyntaxTree))
        {
            var root = (CompilationUnitSyntax)await part.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var usings = root.Usings.Concat(part.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().SelectMany(ns => ns.Usings))
                .Where(directive => directive.GlobalKeyword.IsKind(SyntaxKind.None));
            foreach (UsingDirectiveSyntax directive in usings)
            {
                if (present.Add(directive.ToString()))
                    missing.Add(directive.ToString());
            }
        }
        if (missing.Count == 0)
            return;
        if (targetRoot.Usings.Count > 0)
        {
            int position = own.Text.Lines.GetLineFromPosition(targetRoot.Usings[targetRoot.Usings.Count - 1].Span.End).EndIncludingLineBreak;
            own.Insert(position, string.Concat(missing.Select(directive => directive + newline)));
        }
        else
        {
            own.Insert(0, string.Concat(missing.Select(directive => directive + newline)) + newline);
        }
    }

    /// <summary>Whether a file holds nothing once <paramref name="parts"/> are gone: no other type, statement,
    /// attribute or global using.</summary>
    private static bool IsEmptyWithout(CompilationUnitSyntax root, List<TypeDeclarationSyntax> parts) =>
        root.AttributeLists.Count == 0 && root.Externs.Count == 0 &&
        !root.Usings.Any(directive => !directive.GlobalKeyword.IsKind(SyntaxKind.None)) &&
        !root.Members.OfType<GlobalStatementSyntax>().Any() &&
        OneTypePerFileAnalyzer.TopLevelTypes(root).All(type => type is TypeDeclarationSyntax declaration && parts.Contains(declaration));

    /// <summary>Whether a declaration carries a documentation comment.</summary>
    private static bool HasDocumentation(TypeDeclarationSyntax declaration) => declaration.GetLeadingTrivia().Any(IsDocumentation);

    /// <summary>Whether trivia is a documentation comment.</summary>
    private static bool IsDocumentation(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);

    /// <summary>Lines re-indented from one indentation to another.</summary>
    private static string Reindent(string lines, string from, string to, string newline) =>
        string.Join(newline, lines.Replace("\r\n", "\n").Split('\n')
            .Select(line => line.StartsWith(from, StringComparison.Ordinal) && line.Length > 0 ? to + line.Substring(from.Length) : line));

    /// <summary>The line number of a position.</summary>
    private static int Line(SourceText text, int position) => text.Lines.GetLineFromPosition(position).LineNumber;

    /// <summary>The text's line break.</summary>
    private static string Newline(SourceText text) =>
        text.Lines.Count > 1 ? text.ToString(TextSpan.FromBounds(text.Lines[0].End, text.Lines[0].EndIncludingLineBreak)) : "\n";

    /// <summary>The white space a line starts with.</summary>
    private static string Indentation(string line) => line.Substring(0, line.Length - line.TrimStart().Length);
}
