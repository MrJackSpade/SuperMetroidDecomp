namespace SuperMetroid.ResourceAudit.EnemyVisualPrograms;

/// <summary>A method and the owner it routes to.</summary>
/// <param name="Method">The method.</param>
/// <param name="Owner">The owner.</param>
internal readonly record struct RoutedMethod(string Method, string Owner);
