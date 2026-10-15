namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One run of a colour record: colours written in sequence, or CGRAM entries skipped.</summary>
/// <param name="Skip">True for skipped CGRAM entries, false for written colours.</param>
/// <param name="Count">How many entries.</param>
public readonly record struct PaletteFxRecordPart(bool Skip, int Count);
