namespace SuperMetroid.ResourceAudit;

/// <summary>A resource of a domain.</summary>
/// <param name="Domain">The domain.</param>
/// <param name="Resource">The resource.</param>
internal readonly record struct DomainResource(string Domain, string Resource);
