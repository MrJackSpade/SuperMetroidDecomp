using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// Keeps the evaluation of a removed member's initializer (#1659, #1661): <c>_ = value;</c> at the start of every
/// constructor of its type that runs field initializers (each one not chained to <c>this(...)</c>; the static
/// constructor for a static member), the kept initializers in the order the compiler runs them. An expression-bodied
/// constructor gets a block body to hold them. A type that declares no such constructor gets one (#2599), with the
/// accessibility of the one the compiler gave it (<c>protected</c> for an abstract class, <c>public</c> otherwise); a
/// static one makes the type's initialization precise, which the build's rules may report. A primary constructor has no
/// body to hold a statement, so the statement goes at the start of the type's body, which cannot hold it, and the build
/// fails there.
/// </summary>
internal static class InitializerEvaluation
{
    /// <summary>Inserts the evaluations of <paramref name="kept"/>.</summary>
    internal static async Task KeepAsync(SolutionEdits edits, List<KeptInitializer> kept, CancellationToken cancellationToken)
    {
        foreach (IGrouping<INamedTypeSymbol, KeptInitializer> byType in kept.GroupBy(
            initializer => initializer.Member.ContainingType ?? throw new InvalidOperationException($"{initializer.Member} has no containing type."),
            (IEqualityComparer<INamedTypeSymbol>)SymbolEqualityComparer.Default))
        {
            INamedTypeSymbol type = byType.Key;
            foreach (var group in byType.GroupBy(initializer => initializer.Member.IsStatic))
            {
                var ordered = group.OrderBy(initializer => Order(initializer)).ThenBy(initializer => initializer.Value.SpanStart).ToList();
                var statements = ordered.Select(initializer => $"_ = {Render(initializer)};").ToList();
                TypeDeclarationSyntax holder = ordered[0].Value.FirstAncestorOrSelf<TypeDeclarationSyntax>()
                    ?? throw new InvalidOperationException($"The initializer of {ordered[0].Member} is in no type declaration.");
                var constructors = (group.Key ? type.StaticConstructors : type.InstanceConstructors).Where(c => !c.IsImplicitlyDeclared).ToList();
                if (constructors.Count == 0)
                {
                    AddConstructor(await edits.ForAsync(holder.SyntaxTree, cancellationToken).ConfigureAwait(false), holder, type, group.Key, statements);
                    continue;
                }
                foreach (IMethodSymbol constructor in constructors)
                {
                    foreach (SyntaxReference reference in constructor.DeclaringSyntaxReferences)
                    {
                        SyntaxNode declared = await reference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
                        SourceEdits own = await edits.ForAsync(declared.SyntaxTree, cancellationToken).ConfigureAwait(false);
                        if (declared is not ConstructorDeclarationSyntax declaration)
                        {
                            // A primary constructor: the type's body cannot hold the statement, so the build fails here.
                            InsertInto(own, declared is TypeDeclarationSyntax primary ? primary : holder, statements);
                            continue;
                        }
                        if (declaration.Initializer is { } chained && chained.IsKind(SyntaxKind.ThisConstructorInitializer))
                            continue;
                        if (declaration.Body is { } body)
                            Insert(own, body, statements);
                        else if (declaration.ExpressionBody is { } arrow)
                            GiveBody(own, declaration, arrow, statements);
                        else
                            throw new InvalidOperationException($"{type.Name} has a constructor without a body.");
                    }
                }
            }
        }
    }

    /// <summary>The compiler's order of a member's syntax tree among its compilation's trees.</summary>
    private static int Order(KeptInitializer initializer) =>
        initializer.Model.Compilation.SyntaxTrees.ToList().IndexOf(initializer.Value.SyntaxTree);

    /// <summary>The initializer as a stand-alone expression: a target-typed value gets the member's type written out,
    /// and a user-defined conversion to it is made explicit, so the same work runs.</summary>
    internal static string Render(KeptInitializer initializer)
    {
        ExpressionSyntax value = initializer.Value;
        string type = initializer.DeclaredType.ToString();
        if (value is ImplicitObjectCreationExpressionSyntax creation)
            return $"new {type}{creation.ArgumentList}" + (creation.Initializer is null ? "" : " " + creation.Initializer);
        if (value is CollectionExpressionSyntax)
            return $"({type}){value}";
        if (initializer.Model.GetTypeInfo(value).Type is null || initializer.Model.GetConversion(value).IsUserDefined)
            return $"({type})({value})";
        return value.ToString();
    }

    /// <summary>Inserts statements at the start of a constructor body.</summary>
    private static void Insert(SourceEdits edits, BlockSyntax body, List<string> statements)
    {
        SourceText text = edits.Text;
        SyntaxToken open = body.OpenBraceToken;
        TextLine braceLine = text.Lines.GetLineFromPosition(open.SpanStart);
        SyntaxToken next = open.GetNextToken();
        if (next.SpanStart < braceLine.EndIncludingLineBreak)
        {
            edits.Insert(open.Span.End, string.Concat(statements.Select(statement => " " + statement)));
            return;
        }
        string newline = Newline(text, braceLine);
        string indent = body.Statements.Count > 0
            ? Indentation(text.Lines.GetLineFromPosition(body.Statements[0].SpanStart).ToString())
            : Indentation(braceLine.ToString()) + "    ";
        edits.Insert(braceLine.EndIncludingLineBreak, string.Concat(statements.Select(statement => indent + statement + newline)));
    }

    /// <summary>Gives an expression-bodied constructor a block body: the statements, then its expression as a statement.</summary>
    private static void GiveBody(SourceEdits edits, ConstructorDeclarationSyntax constructor, ArrowExpressionClauseSyntax arrow, List<string> statements)
    {
        SourceText text = edits.Text;
        TextLine line = text.Lines.GetLineFromPosition(constructor.SpanStart);
        string newline = Newline(text, line);
        string indent = Indentation(line.ToString());
        string last = arrow.Expression is ThrowExpressionSyntax thrown ? $"throw {thrown.Expression};" : $"{arrow.Expression};";
        string body = newline + indent + "{" + newline +
            string.Concat(statements.Append(last).Select(statement => indent + "    " + statement + newline)) + indent + "}";
        edits.Replace(TextSpan.FromBounds(arrow.ArrowToken.GetPreviousToken().Span.End, constructor.SemicolonToken.Span.End), body);
    }

    /// <summary>Adds a constructor holding the statements at the start of the type's body.</summary>
    private static void AddConstructor(SourceEdits edits, TypeDeclarationSyntax holder, INamedTypeSymbol type, bool isStatic, List<string> statements)
    {
        SourceText text = edits.Text;
        TextLine braceLine = text.Lines.GetLineFromPosition(holder.OpenBraceToken.SpanStart);
        string newline = Newline(text, braceLine);
        string indent = holder.Members.Count > 0
            ? Indentation(text.Lines.GetLineFromPosition(holder.Members[0].SpanStart).ToString())
            : Indentation(braceLine.ToString()) + "    ";
        string modifiers = isStatic ? "static" : type.IsAbstract ? "protected" : "public";
        string constructor = indent + modifiers + " " + holder.Identifier.ValueText + "()" + newline + indent + "{" + newline +
            string.Concat(statements.Select(statement => indent + "    " + statement + newline)) + indent + "}" + newline;
        edits.Insert(braceLine.EndIncludingLineBreak, constructor + (holder.Members.Count > 0 ? newline : ""));
    }

    /// <summary>Inserts the statements at the start of a type's body, which cannot hold them.</summary>
    private static void InsertInto(SourceEdits edits, TypeDeclarationSyntax holder, List<string> statements)
    {
        SourceText text = edits.Text;
        TextLine braceLine = text.Lines.GetLineFromPosition(holder.OpenBraceToken.SpanStart);
        string newline = Newline(text, braceLine);
        string indent = Indentation(braceLine.ToString()) + "    ";
        edits.Insert(braceLine.EndIncludingLineBreak, string.Concat(statements.Select(statement => indent + statement + newline)));
    }

    /// <summary>A line's line break, or the file's first when it has none.</summary>
    private static string Newline(SourceText text, TextLine line)
    {
        string own = text.ToString(TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
        return own.Length > 0 ? own : text.Lines.Count > 1 ? text.ToString(TextSpan.FromBounds(text.Lines[0].End, text.Lines[0].EndIncludingLineBreak)) : "\n";
    }

    /// <summary>The white space a line starts with.</summary>
    private static string Indentation(string line) => line.Substring(0, line.Length - line.TrimStart().Length);
}
