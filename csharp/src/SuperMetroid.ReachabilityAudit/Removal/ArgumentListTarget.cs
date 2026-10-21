using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>An argument list and the constructor or method that receives it.</summary>
/// <param name="ArgumentList">The argument list.</param>
/// <param name="Symbol">The symbol.</param>
internal readonly record struct ArgumentListTarget(ArgumentListSyntax? ArgumentList, ISymbol? Symbol);
