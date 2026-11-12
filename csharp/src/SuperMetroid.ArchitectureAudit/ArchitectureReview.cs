using Microsoft.CodeAnalysis;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>One <c>[ArchitectureReview]</c> declared on a type: the finding kind it justifies and the site's reason.</summary>
/// <param name="Finding">The finding kind the review justifies.</param>
/// <param name="Reason">Why the finding does not apply to this type.</param>
/// <param name="Site">The attribute's source, which <see cref="StaleReviewPruner"/> deletes once the finding is gone.</param>
internal sealed record ArchitectureReview(string Finding, string Reason, SyntaxReference Site);
