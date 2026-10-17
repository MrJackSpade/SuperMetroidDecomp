namespace SuperMetroid.ArchitectureAudit;

/// <summary>One <c>[ArchitectureReview]</c> declared on a type: the finding kind it justifies and the site's reason.</summary>
internal sealed record ArchitectureReview(string Finding, string Reason);
