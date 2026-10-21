namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>Text inserted at a position in a source file.</summary>
/// <param name="Position">The position.</param>
/// <param name="Text">The text.</param>
internal readonly record struct TextInsertion(int Position, string Text);
