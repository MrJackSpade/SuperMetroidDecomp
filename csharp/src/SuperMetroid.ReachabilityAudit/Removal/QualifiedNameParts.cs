using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>The qualifier and the simple name of a member access or qualified name.</summary>
/// <param name="Left">The left.</param>
/// <param name="Name">The name.</param>
internal readonly record struct QualifiedNameParts(SyntaxNode Left, SimpleNameSyntax Name);
