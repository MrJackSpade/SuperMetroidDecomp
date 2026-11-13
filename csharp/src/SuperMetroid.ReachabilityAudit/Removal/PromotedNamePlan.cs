using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>The names promoted underscore-prefixed members take, and why each whose rename is refused cannot take one.</summary>
/// <param name="Names">Each renamed member's new name, by its repository key.</param>
/// <param name="Refusals">Why each refused member cannot be renamed, which keeps the moved code reaching it in place.</param>
internal sealed record PromotedNamePlan(IReadOnlyDictionary<string, string> Names, IReadOnlyDictionary<ISymbol, string> Refusals);
