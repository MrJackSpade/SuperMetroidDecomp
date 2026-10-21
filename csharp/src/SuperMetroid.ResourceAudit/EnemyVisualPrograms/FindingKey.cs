namespace SuperMetroid.ResourceAudit.EnemyVisualPrograms;

/// <summary>The owner, resource and message that identify a finding.</summary>
/// <param name="Owner">The owner.</param>
/// <param name="Resource">The resource.</param>
/// <param name="Message">The message.</param>
internal readonly record struct FindingKey(string Owner, string Resource, string Message);
