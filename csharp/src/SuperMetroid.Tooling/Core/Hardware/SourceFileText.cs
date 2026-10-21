namespace SuperMetroid.Core.Hardware;

/// <summary>A source file's path and text.</summary>
/// <param name="Path">The path.</param>
/// <param name="Text">The text.</param>
internal readonly record struct SourceFileText(string Path, string Text);
