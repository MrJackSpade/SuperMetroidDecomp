namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The shape and height of the file copy arrow.</summary>
/// <param name="Shape">The arrow's shape.</param>
/// <param name="Y">The arrow's screen Y.</param>
public readonly record struct FileCopyArrowPlacement(FileCopyArrowShape Shape, int Y);
