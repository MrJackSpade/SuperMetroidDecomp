using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>The initializer of a removed field or property whose evaluation could have an effect, so it is kept.</summary>
/// <param name="Member">The removed member.</param>
/// <param name="Value">The initializer's value.</param>
/// <param name="DeclaredType">The member's declared type, which a target-typed value needs once the member is gone.</param>
/// <param name="Model">The semantic model of the member's document.</param>
internal sealed record KeptInitializer(ISymbol Member, ExpressionSyntax Value, TypeSyntax DeclaredType, SemanticModel Model);
