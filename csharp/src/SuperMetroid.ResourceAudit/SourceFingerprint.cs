using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// The one fingerprint every reviewed-source proof pins: SHA-256 over the C# tokens, joined by
/// spaces, with preprocessor directives and disabled text kept as tokens of their own. Comments,
/// documentation and whitespace are not code, so editing them cannot revoke a behavioral proof;
/// any token change still does.
/// </summary>
internal static class SourceFingerprint
{
    internal static string Of(string text) => Of(CSharpSyntaxTree.ParseText(text).GetRoot());

    internal static string Of(SyntaxNode node)
    {
        var parts = new List<string>();
        foreach (SyntaxToken token in node.DescendantTokens())
        {
            AddCompiledTrivia(token.LeadingTrivia, parts);
            parts.Add(token.Text);
            AddCompiledTrivia(token.TrailingTrivia, parts);
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(" ", parts))));
    }

    private static void AddCompiledTrivia(SyntaxTriviaList trivia, List<string> parts)
    {
        foreach (SyntaxTrivia item in trivia)
            if (item.IsDirective || item.IsKind(SyntaxKind.DisabledTextTrivia))
                parts.Add(item.ToString().Replace("\r\n", "\n", StringComparison.Ordinal).Trim());
    }
}
