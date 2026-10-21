namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>An extension member's receiver and its relocated text.</summary>
/// <param name="Receiver">The receiver.</param>
/// <param name="Text">The text.</param>
internal readonly record struct RelocatedExtension(string Receiver, string Text);
