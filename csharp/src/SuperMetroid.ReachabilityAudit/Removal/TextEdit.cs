using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>A source span and the text that replaces it.</summary>
/// <param name="Span">The span.</param>
/// <param name="Text">The text.</param>
internal readonly record struct TextEdit(TextSpan Span, string Text);
