using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>The edits that make a member private and keep checks reaching it through PrivateState, or why they cannot.</summary>
/// <param name="Edits">The declaration's edits and every check site's.</param>
/// <param name="Refusal">Why a site cannot be rewritten, or null when every one can.</param>
internal sealed record SiteRewrite(List<(SyntaxTree Tree, TextChange Change)> Edits, string? Refusal);
